using Microsoft.AspNetCore.Mvc;
using RPCore.Services;

namespace RPCore.Controllers
{
    [ApiController]
    [Route("api/bank")]
    public class BankController : ControllerBase
    {
        private readonly BankService _bankService;
        public BankController(BankService bankService)
        {
            _bankService = bankService;
        }

        [HttpPost("deposit")]
        public IActionResult Deposit([FromBody] DepositRequest req)
        {
            var result = _bankService.Deposit(req.Uuid, req.Amount);
            return Ok(new { success = result.Success, newBalance = result.NewBalance });
        }

        [HttpPost("withdraw")]
        public IActionResult Withdraw([FromBody] DepositRequest req)
        {
            var result = _bankService.Withdraw(req.Uuid, req.Amount);
            return Ok(new { success = result.Success, newBalance = result.NewBalance });
        }

        [HttpPost("transfer")]
        public IActionResult Transfer([FromBody] TransferRequest req)
        {
            var result = _bankService.Transfer(req.FromUuid, req.ToUuid, req.Amount);
            return Ok(new { success = result.Success, fromBalance = result.FromBalance, toBalance = result.ToBalance });
        }

        [HttpGet("balance")]
        public IActionResult GetBalance([FromQuery] string uuid)
        {
            var balance = _bankService.GetBalance(uuid);
            return Ok(new { balance });
        }

        public class DepositRequest
        {
            public string Uuid { get; set; }
            public int Amount { get; set; }
        }
        public class TransferRequest
        {
            public string FromUuid { get; set; }
            public string ToUuid { get; set; }
            public int Amount { get; set; }
        }
    }
}
