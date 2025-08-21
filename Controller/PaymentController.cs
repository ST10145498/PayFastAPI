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
        private readonly IConfiguration _configuration;

        public PaymentController(
            PayFastService payFastService,
            TransactionService transactionService,
            IConfiguration configuration)
        {
            _payFastService = payFastService;
            _transactionService = transactionService;
            _configuration = configuration;
        }

        // API endpoint to initiate payment
        [HttpPost("initiate")]
        public IActionResult InitiatePayment([FromBody] PaymentRequest request)
        {
            try
            {
                // Create transaction record
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

                // Get URLs from configuration
                var payFastConfig = _configuration.GetSection("PayFast");
                var returnUrl = $"{request.BaseUrl}/payment/success?orderId={transaction.OrderId}";
                var cancelUrl = $"{request.BaseUrl}/payment/cancel?orderId={transaction.OrderId}";
                var notifyUrl = $"{payFastConfig["NotifyUrl"]}?orderId={transaction.OrderId}";

                // Generate payment data
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
                return BadRequest(new PaymentResponse
                {
                    Success = false,
                    Message = $"Error initiating payment: {ex.Message}"
                });
            }
        }

        // API endpoint for payment notifications from PayFast
        [HttpPost("notify")]
        public async Task<IActionResult> PaymentNotify([FromForm] PaymentNotification notification, [FromQuery] string orderId)
        {
            try
            {
                if (notification.PaymentStatus == "COMPLETE")
                {
                    var transactions = _transactionService.GetTransactions();
                    var transaction = transactions.FirstOrDefault(t => t.OrderId == orderId);
                    
                    if (transaction != null)
                    {
                        transaction.PaymentStatus = notification.PaymentStatus;
                        transaction.PaymentDate = DateTime.UtcNow;
                        _transactionService.UpdateTransaction(transaction);
                    }
                }
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest($"Error processing notification: {ex.Message}");
            }
        }

        // API endpoint to check payment status
        [HttpGet("status/{orderId}")]
        public IActionResult GetPaymentStatus(string orderId)
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
                date = transaction.PaymentDate
            });
        }
    }
}