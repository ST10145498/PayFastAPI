using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace PayFastAPI.Models
{
    public class PaymentNotification
    {
        [FromForm(Name = "payment_status")]
        public string PaymentStatus { get; set; } = "";

        [FromForm(Name = "pf_payment_id")]
        public string PfPaymentId { get; set; } = "";

        [FromForm(Name = "m_payment_id")]
        public string MPaymentId { get; set; } = "";

        [FromForm(Name = "item_name")]
        public string ItemName { get; set; } = "";

        [FromForm(Name = "item_description")]
        public string ItemDescription { get; set; } = "";

        [FromForm(Name = "amount_gross")]
        public decimal AmountGross { get; set; }

        [FromForm(Name = "amount_fee")]
        public decimal AmountFee { get; set; }

        [FromForm(Name = "amount_net")]
        public decimal AmountNet { get; set; }

        [FromForm(Name = "email_address")]
        public string EmailAddress { get; set; } = "";

        [FromForm(Name = "merchant_id")]
        public string MerchantId { get; set; } = "";

        [FromForm(Name = "signature")]
        public string Signature { get; set; } = "";
    }
}