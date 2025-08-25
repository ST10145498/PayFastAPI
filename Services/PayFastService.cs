
// using System.Globalization;
// using System.Security.Cryptography;
// using System.Text;
// using System.Text.RegularExpressions;
// using PayFastAPI.Models;

// namespace PayFastAPI.Services
// {
//     public class PayFastService
//     {
//         private readonly string _merchantId;
//         private readonly string _merchantKey;
//         private readonly string? _passphrase;
//         private readonly string _processUrl;

//         public PayFastService(IConfiguration configuration)
//         {
//             // Correct names: match Render env vars exactly
//             _merchantId  = configuration["PAYFAST_MERCHANT_ID"] ?? "";
//             _merchantKey = configuration["PAYFAST_MERCHANT_KEY"] ?? "";
//             _passphrase  = configuration["PAYFAST_PASSPHRASE"];
//             _processUrl  = configuration["PAYFAST_URL"] ?? "https://sandbox.payfast.co.za/eng/process";
//         }

//         public PaymentDataResult GeneratePaymentDataForAPI(
//             decimal amount,
//             string itemName,
//             string itemDescription,
//             string emailAddress,
//             string returnUrl,
//             string cancelUrl,
//             string notifyUrl)
//         {
//             var data = new Dictionary<string, string>
//             {
//                 { "merchant_id", _merchantId },
//                 { "merchant_key", _merchantKey },
//                 { "return_url", returnUrl },
//                 { "cancel_url", cancelUrl },
//                 { "notify_url", notifyUrl },
//                 { "email_address", emailAddress },
//                 { "amount", amount.ToString("F2", CultureInfo.InvariantCulture) },
//                 { "item_name", itemName },
//                 { "item_description", itemDescription },
//             };

//             // Generate signature
//             var signature = CreateSignature(data);
//             data.Add("signature", signature);

//             return new PaymentDataResult
//             {
//                 PaymentUrl = _processUrl,
//                 FormData = data
//             };
//         }

//         private string CreateSignature(Dictionary<string, string> data)
//         {
//             var ordered = data
//                 .Where(kv => kv.Key != "signature")
//                 .OrderBy(kv => kv.Key)
//                 .Select(kv => $"{kv.Key}={UrlEncode(kv.Value)}");

//             var queryString = string.Join("&", ordered);

//             if (!string.IsNullOrEmpty(_passphrase))
//             {
//                 queryString += $"&passphrase={UrlEncode(_passphrase)}";
//             }

//             using var md5 = MD5.Create();
//             var hash = md5.ComputeHash(Encoding.ASCII.GetBytes(queryString));
//             return string.Concat(hash.Select(b => b.ToString("x2")));
//         }

//         private static string UrlEncode(string value)
//         {
//             var convertPairs = new Dictionary<string, string>
//             {
//                 { "%", "%25" }, { " ", "+" }, { "!", "%21" }, { "#", "%23" },
//                 { "$", "%24" }, { "&", "%26" }, { "'", "%27" }, { "(", "%28" },
//                 { ")", "%29" }, { "*", "%2A" }, { "+", "%2B" }, { ",", "%2C" },
//                 { "/", "%2F" }, { ":", "%3A" }, { ";", "%3B" }, { "=", "%3D" },
//                 { "?", "%3F" }, { "@", "%40" }, { "[", "%5B" }, { "]", "%5D" }
//             };
//             var replaceRegex = new Regex(@"[% !#\$&'()*+,/:;=?@\[\]]");
//             return replaceRegex.Replace(value, m => convertPairs[m.Value]);
//         }
//     }
// }
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using PayFastAPI.Models;

namespace PayFastAPI.Services
{
    public class PayFastService
    {
        private readonly string _merchantId;
        private readonly string _merchantKey;
        private readonly string? _passphrase;
        private readonly string _processUrl;

        public PayFastService(IConfiguration configuration)
        {
            // Environment variables from Render
            _merchantId  = configuration["PAYFAST_MERCHANT_ID"] ?? "";
            _merchantKey = configuration["PAYFAST_MERCHANT_KEY"] ?? "";
            _passphrase  = configuration["PAYFAST_PASSPHRASE"];
            _processUrl  = configuration["PAYFAST_URL"] ?? "https://sandbox.payfast.co.za/eng/process";
        }

        public PaymentDataResult GeneratePaymentDataForAPI(
            decimal amount,
            string itemName,
            string itemDescription,
            string emailAddress,
            string returnUrl,
            string cancelUrl,
            string notifyUrl)
        {
            var data = new Dictionary<string, string>
            {
                { "merchant_id", _merchantId },
                { "merchant_key", _merchantKey },
                { "return_url", returnUrl },
                { "cancel_url", cancelUrl },
                { "notify_url", notifyUrl },
                { "email_address", emailAddress },
                { "amount", amount.ToString("F2", CultureInfo.InvariantCulture) },
                { "item_name", itemName },
                { "item_description", itemDescription },
            };

            // Generate signature using PayFast's exact requirements
            var signature = CreateSignature(data);
            data.Add("signature", signature);

            return new PaymentDataResult
            {
                PaymentUrl = _processUrl,
                FormData = data
            };
        }

        
        private string CreateSignature(Dictionary<string, string> data)
        {
            
            var orderedData = new List<KeyValuePair<string, string>>
            {
                new("merchant_id", data.GetValueOrDefault("merchant_id", "")),
                new("merchant_key", data.GetValueOrDefault("merchant_key", "")),
                new("return_url", data.GetValueOrDefault("return_url", "")),
                new("cancel_url", data.GetValueOrDefault("cancel_url", "")),
                new("notify_url", data.GetValueOrDefault("notify_url", "")),
                new("email_address", data.GetValueOrDefault("email_address", "")),
                new("amount", data.GetValueOrDefault("amount", "")),
                new("item_name", data.GetValueOrDefault("item_name", "")),
                new("item_description", data.GetValueOrDefault("item_description", "")),
                // Passphrase is added at the end
                new("passphrase", _passphrase ?? "")
            };

            // Build the payload string exactly as PayFast expects
            var payload = new StringBuilder();
            foreach (var item in orderedData)
            {
                if (item.Key != orderedData.Last().Key)
                {
                    payload.Append($"{item.Key}={UrlEncode(item.Value)}&");
                }
                else
                {
                    payload.Append($"{item.Key}={UrlEncode(item.Value)}");
                }
            }

            // Generate MD5 hash
            using var md5 = MD5.Create();
            var inputBytes = Encoding.ASCII.GetBytes(payload.ToString());
            var hash = md5.ComputeHash(inputBytes);

            // Convert to hex string
            var signature = new StringBuilder();
            for (int i = 0; i < hash.Length; i++)
            {
                signature.Append(hash[i].ToString("x2"));
            }
            return signature.ToString();
        }

       
        private static string UrlEncode(string value)
        {
            var convertPairs = new Dictionary<string, string>
            {
                { "%", "%25" }, { "!", "%21" }, { "#", "%23" }, { " ", "+" },
                { "$", "%24" }, { "&", "%26" }, { "'", "%27" }, { "(", "%28" },
                { ")", "%29" }, { "*", "%2A" }, { "+", "%2B" }, { ",", "%2C" },
                { "/", "%2F" }, { ":", "%3A" }, { ";", "%3B" }, { "=", "%3D" },
                { "?", "%3F" }, { "@", "%40" }, { "[", "%5B" }, { "]", "%5D" }
            };
            var replaceRegex = new Regex(@"[%!# $&'()*+,/:;=?@\[\]]");
            return replaceRegex.Replace(value, m => convertPairs[m.Value]);
        }
    }
}