// using Microsoft.AspNetCore.Mvc;
// using System.ComponentModel.DataAnnotations;

// namespace PayFastAPI.Models
// {
//     public class PaymentNotification
//     {
//         [FromForm(Name = "payment_status")]
//         public string PaymentStatus { get; set; } = "";

//         [FromForm(Name = "pf_payment_id")]
//         public string PfPaymentId { get; set; } = "";

//         [FromForm(Name = "m_payment_id")]
//         public string MPaymentId { get; set; } = "";

//         [FromForm(Name = "item_name")]
//         public string ItemName { get; set; } = "";

//         [FromForm(Name = "item_description")]
//         public string ItemDescription { get; set; } = "";

//         [FromForm(Name = "amount_gross")]
//         public decimal AmountGross { get; set; }

//         [FromForm(Name = "amount_fee")]
//         public decimal AmountFee { get; set; }

//         [FromForm(Name = "amount_net")]
//         public decimal AmountNet { get; set; }

//         [FromForm(Name = "email_address")]
//         public string EmailAddress { get; set; } = "";

//         [FromForm(Name = "merchant_id")]
//         public string MerchantId { get; set; } = "";

//         [FromForm(Name = "signature")]
//         public string Signature { get; set; } = "";
//     }
// }

using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace PayFastAPI.Models
{
    public class PaymentNotification
    {
        [FromForm(Name = "m_payment_id")]
        public string MPaymentId { get; set; } = "";

        [FromForm(Name = "pf_payment_id")]
        public string PfPaymentId { get; set; } = "";

        [FromForm(Name = "payment_status")]
        public string PaymentStatus { get; set; } = "";

        [FromForm(Name = "item_name")]
        public string ItemName { get; set; } = "";

        [FromForm(Name = "item_description")]
        public string ItemDescription { get; set; } = "";

        [FromForm(Name = "amount_gross")]
        public decimal AmountGross { get; set; }

        [FromForm(Name = "amount_fee")]
        public decimal AmountFee { get; set; }  // This can be negative!

        [FromForm(Name = "amount_net")]
        public decimal AmountNet { get; set; }

        [FromForm(Name = "custom_str1")]
        public string CustomStr1 { get; set; } = "";

        [FromForm(Name = "custom_str2")]
        public string CustomStr2 { get; set; } = "";

        [FromForm(Name = "custom_str3")]
        public string CustomStr3 { get; set; } = "";

        [FromForm(Name = "custom_str4")]
        public string CustomStr4 { get; set; } = "";

        [FromForm(Name = "custom_str5")]
        public string CustomStr5 { get; set; } = "";

        [FromForm(Name = "custom_int1")]
        public int CustomInt1 { get; set; }

        [FromForm(Name = "custom_int2")]
        public int CustomInt2 { get; set; }

        [FromForm(Name = "custom_int3")]
        public int CustomInt3 { get; set; }

        [FromForm(Name = "custom_int4")]
        public int CustomInt4 { get; set; }

        [FromForm(Name = "custom_int5")]
        public int CustomInt5 { get; set; }

        [FromForm(Name = "name_first")]
        public string NameFirst { get; set; } = "";

        [FromForm(Name = "name_last")]
        public string NameLast { get; set; } = "";

        [FromForm(Name = "email_address")]
        public string EmailAddress { get; set; } = "";

        [FromForm(Name = "merchant_id")]
        public string MerchantId { get; set; } = "";

        [FromForm(Name = "signature")]
        public string Signature { get; set; } = "";
    }
}