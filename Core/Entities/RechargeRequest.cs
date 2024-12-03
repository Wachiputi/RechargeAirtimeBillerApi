namespace Core.Entities
{
    public class RechargeRequest
    {
        public string SenderMsisdn { get; set; } = string.Empty;
        public decimal Amount { get; set; }
    }
}
