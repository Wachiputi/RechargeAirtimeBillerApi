using Microsoft.AspNetCore.Mvc;
using Core.Entities;
using Core.Interfaces;
using System.Threading.Tasks;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RechargeController : ControllerBase
    {
        private readonly IRechargeService _rechargeService;

        public RechargeController(IRechargeService rechargeService)
        {
            _rechargeService = rechargeService;
        }

        [HttpPost]
        public async Task<IActionResult> Recharge([FromBody] RechargeRequest request)
        {
            if (string.IsNullOrEmpty(request.SenderMsisdn) || request.Amount <= 0)
            {
                return BadRequest(new { Message = "SenderMsisdn or a valid Amount is required." });
            }

            try
            {
                var response = await _rechargeService.PerformRechargeAsync(request);
                if (string.IsNullOrEmpty(response.TransactionId))
                {
                    return BadRequest(new { Message = response.ResponseMsg });
                }

                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }
    }
}
