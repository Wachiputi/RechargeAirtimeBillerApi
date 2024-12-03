using Core.Entities;
using Core.Interfaces;

namespace Application.Services
{
    public class RechargeManager : IRechargeService
    {
        private readonly Infrastructure.Services.RechargeService _rechargeService;

        public RechargeManager(Infrastructure.Services.RechargeService rechargeService)
        {
            _rechargeService = rechargeService;
        }

        public async Task<RechargeResponse> PerformRechargeAsync(RechargeRequest request)
        {
            // Validate the request parameters
            if (request.Amount <= 0)
            {
                return new RechargeResponse
                {
                   // Status = "Failed",
                    //Message = "Invalid recharge amount"
                };
            }

            // Call PerformRechargeAsync from the RechargeService and return the result
            return await _rechargeService.PerformRechargeAsync(request);
        }
    }
}
