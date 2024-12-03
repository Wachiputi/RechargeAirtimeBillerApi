using Core.Entities;

namespace Core.Interfaces
{
    public interface IRechargeService
    {
        Task<RechargeResponse> PerformRechargeAsync(RechargeRequest request);
    }
}
