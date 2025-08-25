

// using Microsoft.AspNetCore.Mvc;
// using PayFastAPI.Services;
// using PayFastAPI.Models;

// namespace PayFastAPI.Controllers
// {
//     [ApiController]
//     [Route("api/[controller]")]
//     public class PaymentController : ControllerBase
//     {
//         private readonly PayFastService _payFastService;
//         private readonly TransactionService _transactionService;

//         public PaymentController(
//             PayFastService payFastService,
//             TransactionService transactionService)
//         {
//             _payFastService = payFastService;
//             _transactionService = transactionService;
//         }

//         [HttpPost("initiate")]
//         public IActionResult InitiatePayment([FromBody] PaymentRequest request)
//         {
//             try
//             {
//                 var transaction = new Transaction
//                 {
//                     OrderId = Guid.NewGuid().ToString(),
//                     Amount = request.Amount,
//                     OrderDescription = request.Description ?? $"Payment by {request.Name}",
//                     Email = request.Email,
//                     PaymentStatus = "PENDING",
//                     PaymentDate = DateTime.UtcNow
//                 };

//                 _transactionService.AddTransaction(transaction);

//                 // FIXED: Handle mobile vs web URLs differently
//                 string returnUrl, cancelUrl;
                
//                 if (request.ClientType == "mobile")
//                 {
//                     // Use your Render API as intermediary for mobile deep links
//                     returnUrl = $"https://{Request.Host}/api/Payment/mobile-return?orderId={transaction.OrderId}&status=success";
//                     cancelUrl = $"https://{Request.Host}/api/Payment/mobile-return?orderId={transaction.OrderId}&status=cancel";
//                 }
//                 else
//                 {
//                     // For web clients, use the provided baseUrl
//                     returnUrl = $"{request.BaseUrl}/payment/success?orderId={transaction.OrderId}";
//                     cancelUrl = $"{request.BaseUrl}/payment/cancel?orderId={transaction.OrderId}";
//                 }

//                 // Your API's notify endpoint
//                 var notifyUrl = $"https://{Request.Host}/api/payment/notify?orderId={transaction.OrderId}";

//                 var paymentData = _payFastService.GeneratePaymentDataForAPI(
//                     request.Amount,
//                     request.ItemName ?? $"Payment by {request.Name}",
//                     request.Description ?? $"Payment from {request.Name}",
//                     request.Email,
//                     returnUrl,
//                     cancelUrl,
//                     notifyUrl
//                 );

//                 return Ok(new PaymentResponse
//                 {
//                     Success = true,
//                     OrderId = transaction.OrderId,
//                     PaymentUrl = paymentData.PaymentUrl,
//                     PaymentData = paymentData.FormData,
//                     Message = "Payment initiated successfully"
//                 });
//             }
//             catch (Exception ex)
//             {
//                 return BadRequest(new PaymentResponse
//                 {
//                     Success = false,
//                     Message = $"Error initiating payment: {ex.Message}"
//                 });
//             }
//         }

//         // NEW: Handle mobile app returns and redirect to deep links
//         [HttpGet("mobile-return")]
//         public IActionResult MobileReturn(string orderId, string status)
//         {
//             try
//             {
//                 // Create the appropriate deep link
//                 var deepLink = status == "success" 
//                     ? $"myapp://payment/success?orderId={orderId}"
//                     : $"myapp://payment/cancel?orderId={orderId}";

//                 // Return HTML that immediately redirects to the deep link
//                 var html = $@"
//                     <!DOCTYPE html>
//                     <html>
//                     <head>
//                         <meta charset='utf-8'>
//                         <title>Redirecting to App...</title>
//                         <meta http-equiv='refresh' content='0;url={deepLink}'>
//                         <style>
//                             body {{ font-family: Arial, sans-serif; text-align: center; padding: 50px; }}
//                             .message {{ color: #333; margin-bottom: 20px; }}
//                             .link {{ color: #007bff; }}
//                         </style>
//                     </head>
//                     <body>
//                         <div class='message'>
//                             <h2>Payment {(status == "success" ? "Successful" : "Cancelled")}</h2>
//                             <p>Redirecting back to the app...</p>
//                             <p>If you're not redirected automatically, <a href='{deepLink}' class='link'>click here</a></p>
//                         </div>
//                         <script>
//                             setTimeout(function() {{
//                                 window.location.href = '{deepLink}';
//                             }}, 1000);
//                         </script>
//                     </body>
//                     </html>";

//                 return Content(html, "text/html");
//             }
//             catch (Exception ex)
//             {
//                 return BadRequest($"Error processing mobile return: {ex.Message}");
//             }
//         }

//         [HttpPost("notify")]
//         public IActionResult PaymentNotify([FromForm] PaymentNotification notification, [FromQuery] string orderId)
//         {
//             try
//             {
//                 if (notification.PaymentStatus == "COMPLETE")
//                 {
//                     var transactions = _transactionService.GetTransactions();
//                     var transaction = transactions.FirstOrDefault(t => t.OrderId == orderId);

//                     if (transaction != null)
//                     {
//                         transaction.PaymentStatus = notification.PaymentStatus;
//                         transaction.PaymentDate = DateTime.UtcNow;
//                         transaction.PayFastPaymentId = notification.PfPaymentId;
//                         transaction.AmountPaid = notification.AmountGross;
//                         _transactionService.UpdateTransaction(transaction);
//                     }
//                 }
//                 return Ok();
//             }
//             catch (Exception ex)
//             {
//                 return BadRequest($"Error processing notification: {ex.Message}");
//             }
//         }

//         // Payment successful (for web clients)
//         [HttpGet("return")]
//         public IActionResult Return()
//         {
//             return Ok(new
//             {
//                 Message = "Payment completed successfully."
//             });
//         }

//         // Payment cancelled (for web clients)
//         [HttpGet("cancel")]
//         public IActionResult Cancel()
//         {
//             return Ok(new
//             {
//                 Message = "Payment was cancelled by the user."
//             });
//         }

//         [HttpGet("status/{orderId}")]
//         public IActionResult GetPaymentStatus(string orderId)
//         {
//             var transactions = _transactionService.GetTransactions();
//             var transaction = transactions.FirstOrDefault(t => t.OrderId == orderId);

//             if (transaction == null)
//             {
//                 return NotFound(new { message = "Transaction not found" });
//             }

//             return Ok(new
//             {
//                 orderId = transaction.OrderId,
//                 status = transaction.PaymentStatus,
//                 amount = transaction.Amount,
//                 date = transaction.PaymentDate
//             });
//         }
//     }
// }

using Microsoft.AspNetCore.Mvc;
using PayFastAPI.Services;
using PayFastAPI.Models;

namespace PayFastAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PaymentController : ControllerBase
    {
        private readonly PayFastService _payFastService;
        private readonly TransactionService _transactionService;
        private readonly ILogger<PaymentController> _logger;

        public PaymentController(
            PayFastService payFastService,
            TransactionService transactionService,
            ILogger<PaymentController> logger)
        {
            _payFastService = payFastService;
            _transactionService = transactionService;
            _logger = logger;
        }

        [HttpPost("initiate")]
        public IActionResult InitiatePayment([FromBody] PaymentRequest request)
        {
            try
            {
                var transaction = new Transaction
                {
                    OrderId = Guid.NewGuid().ToString(),
                    Amount = request.Amount,
                    OrderDescription = request.Description ?? $"Payment by {request.Name}",
                    Email = request.Email,
                    PaymentStatus = "PENDING",
                    PaymentDate = DateTime.UtcNow
                };

                _transactionService.AddTransaction(transaction);

                string returnUrl, cancelUrl;
                if (request.ClientType == "mobile")
                {
                    returnUrl = $"https://{Request.Host}/api/Payment/mobile-return?orderId={transaction.OrderId}&status=success";
                    cancelUrl = $"https://{Request.Host}/api/Payment/mobile-return?orderId={transaction.OrderId}&status=cancel";
                }
                else
                {
                    returnUrl = $"{request.BaseUrl}/payment/success?orderId={transaction.OrderId}";
                    cancelUrl = $"{request.BaseUrl}/payment/cancel?orderId={transaction.OrderId}";
                }

                var notifyUrl = $"https://{Request.Host}/api/payment/notify?orderId={transaction.OrderId}";

                var paymentData = _payFastService.GeneratePaymentDataForAPI(
                    request.Amount,
                    request.ItemName ?? $"Payment by {request.Name}",
                    request.Description ?? $"Payment from {request.Name}",
                    request.Email,
                    returnUrl,
                    cancelUrl,
                    notifyUrl
                );

                return Ok(new PaymentResponse
                {
                    Success = true,
                    OrderId = transaction.OrderId,
                    PaymentUrl = paymentData.PaymentUrl,
                    PaymentData = paymentData.FormData,
                    Message = "Payment initiated successfully"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error initiating payment");
                return BadRequest(new PaymentResponse
                {
                    Success = false,
                    Message = $"Error initiating payment: {ex.Message}"
                });
            }
        }

        [HttpGet("mobile-return")]
        public IActionResult MobileReturn(string orderId, string status)
        {
            try
            {
                var deepLink = status == "success" 
                    ? $"myapp://payment/success?orderId={orderId}" 
                    : $"myapp://payment/cancel?orderId={orderId}";

                var html = $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <title>Redirecting to App...</title>
    <meta http-equiv='refresh' content='0;url={deepLink}'>
    <style>
        body {{ font-family: Arial, sans-serif; text-align: center; padding: 50px; }}
        .message {{ color: #333; margin-bottom: 20px; }}
        .link {{ color: #007bff; }}
    </style>
</head>
<body>
    <div class='message'>
        <h2>Payment {(status == "success" ? "Successful" : "Cancelled")}</h2>
        <p>Redirecting back to the app...</p>
        <p>If you're not redirected automatically, <a href='{deepLink}' class='link'>click here</a></p>
    </div>
    <script>
        setTimeout(function() {{ window.location.href = '{deepLink}'; }}, 1000);
    </script>
</body>
</html>";

                return Content(html, "text/html");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing mobile return");
                return BadRequest($"Error processing mobile return: {ex.Message}");
            }
        }

        [HttpPost("notify")]
        public IActionResult PaymentNotify([FromForm] PaymentNotification notification, [FromQuery] string orderId)
        {
            try
            {
                // Log the incoming notification for debugging
                _logger.LogInformation("Received PayFast ITN for orderId: {OrderId}, Status: {Status}, PfPaymentId: {PfPaymentId}", 
                    orderId, notification.PaymentStatus, notification.PfPaymentId);

                // Validate required fields
                if (string.IsNullOrEmpty(notification.PaymentStatus))
                {
                    _logger.LogWarning("PaymentStatus is missing in notification");
                    return BadRequest("PaymentStatus is required");
                }

                if (string.IsNullOrEmpty(orderId))
                {
                    _logger.LogWarning("OrderId is missing in notification");
                    return BadRequest("OrderId is required");
                }

                // Process the notification
                if (notification.PaymentStatus.Equals("COMPLETE", StringComparison.OrdinalIgnoreCase))
                {
                    var transactions = _transactionService.GetTransactions();
                    var transaction = transactions.FirstOrDefault(t => t.OrderId == orderId);
                    
                    if (transaction != null)
                    {
                        transaction.PaymentStatus = notification.PaymentStatus;
                        transaction.PaymentDate = DateTime.UtcNow;
                        transaction.PayFastPaymentId = notification.PfPaymentId;
                        transaction.AmountPaid = notification.AmountGross;
                        
                        _transactionService.UpdateTransaction(transaction);
                        
                        _logger.LogInformation("Transaction updated successfully for orderId: {OrderId}", orderId);
                    }
                    else
                    {
                        _logger.LogWarning("Transaction not found for orderId: {OrderId}", orderId);
                        return BadRequest("Transaction not found");
                    }
                }

                return Ok("ITN processed successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing PayFast notification for orderId: {OrderId}", orderId);
                return BadRequest($"Error processing notification: {ex.Message}");
            }
        }

        [HttpGet("return")]
        public IActionResult Return()
        {
            return Ok(new { Message = "Payment completed successfully." });
        }

        [HttpGet("cancel")]
        public IActionResult Cancel()
        {
            return Ok(new { Message = "Payment was cancelled by the user." });
        }

        [HttpGet("status/{orderId}")]
        public IActionResult GetPaymentStatus(string orderId)
        {
            try
            {
                var transactions = _transactionService.GetTransactions();
                var transaction = transactions.FirstOrDefault(t => t.OrderId == orderId);

                if (transaction == null)
                {
                    return NotFound(new { message = "Transaction not found" });
                }

                return Ok(new
                {
                    orderId = transaction.OrderId,
                    status = transaction.PaymentStatus,
                    amount = transaction.Amount,
                    date = transaction.PaymentDate,
                    payFastPaymentId = transaction.PayFastPaymentId,
                    amountPaid = transaction.AmountPaid
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting payment status for orderId: {OrderId}", orderId);
                return BadRequest($"Error getting payment status: {ex.Message}");
            }
        }
    }
}