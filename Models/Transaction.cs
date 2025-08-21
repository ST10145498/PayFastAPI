// FILE: Models/Transaction.cs
namespace PayFastAPI.Models
{
    public class Transaction
    {
        public string OrderId { get; set; } = Guid.NewGuid().ToString();
        public string OrderDescription { get; set; } = "";
        public string Email { get; set; } = "";
        public string CustomerName { get; set; } = "";
        public decimal Amount { get; set; }
        public string PaymentStatus { get; set; } = "PENDING";
        public DateTime PaymentDate { get; set; } = DateTime.UtcNow;
        public string PayFastPaymentId { get; set; } = "";
        public decimal AmountPaid { get; set; } = 0m;
    }
}
