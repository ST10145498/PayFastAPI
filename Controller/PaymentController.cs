



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
//         private readonly ILogger<PaymentController> _logger;

//         public PaymentController(
//             PayFastService payFastService,
//             TransactionService transactionService,
//             ILogger<PaymentController> logger)
//         {
//             _payFastService = payFastService;
//             _transactionService = transactionService;
//             _logger = logger;
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

//                 string returnUrl, cancelUrl;
//                 if (request.ClientType == "mobile")
//                 {
//                     returnUrl = $"https://{Request.Host}/api/Payment/mobile-return?orderId={transaction.OrderId}&status=success";
//                     cancelUrl = $"https://{Request.Host}/api/Payment/mobile-return?orderId={transaction.OrderId}&status=cancel";
//                 }
//                 else
//                 {
//                     returnUrl = $"{request.BaseUrl}/payment/success?orderId={transaction.OrderId}";
//                     cancelUrl = $"{request.BaseUrl}/payment/cancel?orderId={transaction.OrderId}";
//                 }

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
//                 _logger.LogError(ex, "Error initiating payment");
//                 return BadRequest(new PaymentResponse
//                 {
//                     Success = false,
//                     Message = $"Error initiating payment: {ex.Message}"
//                 });
//             }
//         }

//         [HttpGet("mobile-return")]
//         public IActionResult MobileReturn(string orderId, string status)
//         {
//             try
//             {
//                 var deepLink = status == "success" 
//                     ? $"myapp://payment/success?orderId={orderId}" 
//                     : $"myapp://payment/cancel?orderId={orderId}";

//                 var html = $@"
// <!DOCTYPE html>
// <html>
// <head>
//     <meta charset='utf-8'>
//     <title>Redirecting to App...</title>
//     <meta http-equiv='refresh' content='0;url={deepLink}'>
//     <style>
//         body {{ font-family: Arial, sans-serif; text-align: center; padding: 50px; }}
//         .message {{ color: #333; margin-bottom: 20px; }}
//         .link {{ color: #007bff; }}
//     </style>
// </head>
// <body>
//     <div class='message'>
//         <h2>Payment {(status == "success" ? "Successful" : "Cancelled")}</h2>
//         <p>Redirecting back to the app...</p>
//         <p>If you're not redirected automatically, <a href='{deepLink}' class='link'>click here</a></p>
//     </div>
//     <script>
//         setTimeout(function() {{ window.location.href = '{deepLink}'; }}, 1000);
//     </script>
// </body>
// </html>";

//                 return Content(html, "text/html");
//             }
//             catch (Exception ex)
//             {
//                 _logger.LogError(ex, "Error processing mobile return");
//                 return BadRequest($"Error processing mobile return: {ex.Message}");
//             }
//         }

//         [HttpPost("notify")]
//         public IActionResult PaymentNotify([FromForm] PaymentNotification notification, [FromQuery] string orderId)
//         {
//             try
//             {
//                 // Log the incoming notification for debugging
//                 _logger.LogInformation("Received PayFast ITN for orderId: {OrderId}, Status: {Status}, PfPaymentId: {PfPaymentId}", 
//                     orderId, notification.PaymentStatus, notification.PfPaymentId);

//                 // Validate required fields
//                 if (string.IsNullOrEmpty(notification.PaymentStatus))
//                 {
//                     _logger.LogWarning("PaymentStatus is missing in notification");
//                     return BadRequest("PaymentStatus is required");
//                 }

//                 if (string.IsNullOrEmpty(orderId))
//                 {
//                     _logger.LogWarning("OrderId is missing in notification");
//                     return BadRequest("OrderId is required");
//                 }

//                 // Process the notification
//                 if (notification.PaymentStatus.Equals("COMPLETE", StringComparison.OrdinalIgnoreCase))
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
                        
//                         _logger.LogInformation("Transaction updated successfully for orderId: {OrderId}", orderId);
//                     }
//                     else
//                     {
//                         _logger.LogWarning("Transaction not found for orderId: {OrderId}", orderId);
//                         return BadRequest("Transaction not found");
//                     }
//                 }

//                 return Ok("ITN processed successfully");
//             }
//             catch (Exception ex)
//             {
//                 _logger.LogError(ex, "Error processing PayFast notification for orderId: {OrderId}", orderId);
//                 return BadRequest($"Error processing notification: {ex.Message}");
//             }
//         }

//         [HttpGet("return")]
//         public IActionResult Return()
//         {
//             return Ok(new { Message = "Payment completed successfully." });
//         }

//         [HttpGet("cancel")]
//         public IActionResult Cancel()
//         {
//             return Ok(new { Message = "Payment was cancelled by the user." });
//         }

//         [HttpGet("status/{orderId}")]
//         public IActionResult GetPaymentStatus(string orderId)
//         {
//             try
//             {
//                 var transactions = _transactionService.GetTransactions();
//                 var transaction = transactions.FirstOrDefault(t => t.OrderId == orderId);

//                 if (transaction == null)
//                 {
//                     return NotFound(new { message = "Transaction not found" });
//                 }

//                 return Ok(new
//                 {
//                     orderId = transaction.OrderId,
//                     status = transaction.PaymentStatus,
//                     amount = transaction.Amount,
//                     date = transaction.PaymentDate,
//                     payFastPaymentId = transaction.PayFastPaymentId,
//                     amountPaid = transaction.AmountPaid
//                 });
//             }
//             catch (Exception ex)
//             {
//                 _logger.LogError(ex, "Error getting payment status for orderId: {OrderId}", orderId);
//                 return BadRequest($"Error getting payment status: {ex.Message}");
//             }
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

                // KEY FIX: Use the correct notify URL format like lecturer's code
                var notifyUrl = $"https://{Request.Host}/api/payment/notify";

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
        public async Task<IActionResult> Notify()
        {
            try
            {
                _logger.LogInformation("PayFast ITN received");

                // Read the form data exactly like the lecturer's working code
                var form = await Request.ReadFormAsync();
                
                var notification = new PaymentNotification
                {
                    PaymentStatus = form["payment_status"],
                    PfPaymentId = form["pf_payment_id"],
                    MPaymentId = form["m_payment_id"],
                    ItemName = form["item_name"],
                    ItemDescription = form["item_description"],
                    AmountGross = decimal.TryParse(form["amount_gross"], out var gross) ? gross : 0,
                    AmountFee = decimal.TryParse(form["amount_fee"], out var fee) ? fee : 0,
                    AmountNet = decimal.TryParse(form["amount_net"], out var net) ? net : 0,
                    CustomStr1 = form["custom_str1"],
                    CustomStr2 = form["custom_str2"],
                    CustomStr3 = form["custom_str3"],
                    CustomStr4 = form["custom_str4"],
                    CustomStr5 = form["custom_str5"],
                    EmailAddress = form["email_address"],
                    MerchantId = form["merchant_id"],
                    Signature = form["signature"]
                };

                _logger.LogInformation("ITN Data - Status: {Status}, PfPaymentId: {PfPaymentId}, ItemName: {ItemName}", 
                    notification.PaymentStatus, notification.PfPaymentId, notification.ItemName);

                if (notification.PaymentStatus == "COMPLETE")
                {
                    var transactions = _transactionService.GetTransactions();
                    
                    // KEY FIX: Use ItemName to find transaction like lecturer's code
                    // ItemName contains the description we sent, which should help us identify the transaction
                    var transaction = transactions.FirstOrDefault(t => 
                        t.OrderDescription.Contains(notification.ItemName) || 
                        t.Email == notification.EmailAddress ||
                        Math.Abs(t.Amount - notification.AmountGross) < 0.01m);
                    
                    if (transaction != null)
                    {
                        transaction.PayFastPaymentId = notification.PfPaymentId;
                        transaction.PaymentStatus = notification.PaymentStatus;
                        transaction.AmountPaid = notification.AmountGross;
                        transaction.PaymentDate = DateTime.UtcNow;

                        _transactionService.UpdateTransaction(transaction);
                        
                        _logger.LogInformation("Transaction updated successfully for PfPaymentId: {PfPaymentId}", 
                            notification.PfPaymentId);
                    }
                    else
                    {
                        _logger.LogWarning("Transaction not found for ItemName: {ItemName}, Email: {Email}, Amount: {Amount}", 
                            notification.ItemName, notification.EmailAddress, notification.AmountGross);
                    }
                }

                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing PayFast notification");
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

        // Add lecturer's health check endpoint
        [HttpGet("health")]
        public IActionResult HealthCheck()
        {
            return Ok(new { 
                message = "App is running!", 
                timestamp = DateTime.UtcNow,
                host = Request.Host.ToString()
            });
        }

        // Add lecturer's endpoints for debugging
        [HttpGet("all-transactions")]
        public IActionResult GetAllTransactions()
        {
            var transactions = _transactionService.GetTransactions();
            return Ok(transactions);
        }

        [HttpGet("clear-transactions")]
        public IActionResult ClearTransactions()
        {
            _transactionService.ClearTransactions();
            return Ok(new { message = "All transactions cleared" });
        }
    }
}