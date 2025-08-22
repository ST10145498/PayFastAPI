// namespace PayFastAPI.Models
// {
//     public class PaymentRequest
//     {
//         public decimal Amount { get; set; }
//         public string Name { get; set; } = "";
//         public string Email { get; set; } = "";
//         public string? Description { get; set; }
//         public string? ItemName { get; set; }
//         public string BaseUrl { get; set; } = ""; // The URL of your calling application
//     }

//     public class PaymentResponse
//     {
//         public bool Success { get; set; }
//         public string OrderId { get; set; } = "";
//         public string PaymentUrl { get; set; } = "";
//         public Dictionary<string, string> PaymentData { get; set; } = new();
//         public string Message { get; set; } = "";
//     }

//     public class PaymentDataResult
//     {
//         public string PaymentUrl { get; set; } = "";
//         public Dictionary<string, string> FormData { get; set; } = new();
//     }
// }
namespace PayFastAPI.Models
{
    public class PaymentRequest
    {
        public decimal Amount { get; set; }
        public string Name { get; set; } = "";
        public string Email { get; set; } = "";
        public string? Description { get; set; }
        public string? ItemName { get; set; }
        public string BaseUrl { get; set; } = ""; // The URL of your calling application
        public string? ClientType { get; set; } = "web"; // "web" or "mobile"
        public string? SessionId { get; set; } // Optional unique session identifier
    }

    public class PaymentResponse
    {
        public bool Success { get; set; }
        public string OrderId { get; set; } = "";
        public string PaymentUrl { get; set; } = "";
        public Dictionary<string, string> PaymentData { get; set; } = new();
        public string Message { get; set; } = "";
    }

    public class PaymentDataResult
    {
        public string PaymentUrl { get; set; } = "";
        public Dictionary<string, string> FormData { get; set; } = new();
    }
}