namespace PayFastAPI.Models
{
    public class PayFastOptions
    {
        public bool UseSandbox { get; set; } = true;
        public string MerchantId { get; set; } = "";
        public string MerchantKey { get; set; } = "";
        public string? Passphrase { get; set; }
        public string ReturnUrl { get; set; } = "";
        public string CancelUrl { get; set; } = "";
        public string NotifyUrl { get; set; } = "";
    }
}
