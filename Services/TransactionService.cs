// FILE: Services/TransactionService.cs
using Newtonsoft.Json;
using PayFastAPI.Models;

namespace PayFastAPI.Services
{
    public class TransactionService
    {
        private readonly string _filePath = Path.Combine(Directory.GetCurrentDirectory(), "transactions.json");
        private readonly object _lock = new();

        public List<Transaction> GetTransactions()
        {
            lock (_lock)
            {
                if (!File.Exists(_filePath)) return new List<Transaction>();
                var json = File.ReadAllText(_filePath);
                return JsonConvert.DeserializeObject<List<Transaction>>(json) ?? new List<Transaction>();
            }
        }

        public void SaveTransactions(List<Transaction> transactions)
        {
            lock (_lock)
            {
                var json = JsonConvert.SerializeObject(transactions, Formatting.Indented);
                File.WriteAllText(_filePath, json);
            }
        }

        public void AddTransaction(Transaction transaction)
        {
            lock (_lock)
            {
                var transactions = GetTransactions();
                transactions.Add(transaction);
                SaveTransactions(transactions);
            }
        }

        public void UpdateTransaction(Transaction updated)
        {
            lock (_lock)
            {
                var transactions = GetTransactions();
                var idx = transactions.FindIndex(t => t.OrderId == updated.OrderId);
                if (idx >= 0)
                {
                    transactions[idx] = updated;
                    SaveTransactions(transactions);
                }
            }
        }

        public void ClearTransactions()
        {
            SaveTransactions(new List<Transaction>());
        }
    }
}
