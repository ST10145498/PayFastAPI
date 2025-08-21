// FILE: Services/PayFastService.cs
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
        private readonly string _sandboxUrl;

        public PayFastService(IConfiguration configuration)
        {
            // Prefer environment variables (Render), fall back to appsettings.json
            _merchantId  = configuration["PayFast_MerchantId"] ?? configuration["PayFast:MerchantId"] ?? "";
            _merchantKey = configuration["PayFast_MerchantKey"] ?? configuration["PayFast:MerchantKey"] ?? "";
            _passphrase  = configuration["PayFast_Passphrase"] ?? configuration["PayFast:Passphrase"];
            _sandboxUrl  = configuration["PayFast_SandboxUrl"] ?? "https://sandbox.payfast.co.za/eng/process";
        }

        /// <summary>
        /// Returns the PayFast post URL and a dictionary of fields for the client to post.
        /// </summary>
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

            var signature = CreateSignature(data);
            data.Add("signature", signature);

            return new PaymentDataResult
            {
                PaymentUrl = _sandboxUrl,
                FormData = data
            };
        }

        private string CreateSignature(Dictionary<string, string> data)
        {
            // Signature requires a specific ordering AND inclusion of passphrase.
            var ordered = new List<KeyValuePair<string, string>>
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
            };

            // Append passphrase last (even if empty)
            var payload = new StringBuilder();
            foreach (var kv in ordered)
            {
                payload.Append($"{kv.Key}={UrlEncode(kv.Value)}&");
            }
            payload.Append($"passphrase={UrlEncode(_passphrase ?? string.Empty)}");

            using var md5 = MD5.Create();
            var hash = md5.ComputeHash(Encoding.ASCII.GetBytes(payload.ToString()));
            var sb = new StringBuilder();
            foreach (var b in hash) sb.Append(b.ToString("x2"));
            return sb.ToString();
        }

        // Adapted encoding to PayFast-compatible format
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
