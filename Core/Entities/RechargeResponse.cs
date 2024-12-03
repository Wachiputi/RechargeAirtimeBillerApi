namespace Core.Entities
{
    public class RechargeResponse
    {
        public string TransactionId { get; set; } = string.Empty;
        public string ExternalTxnId { get; set; } = string.Empty;
        public string CommandStatus { get; set; } = string.Empty;
        public string ResponseMsg { get; set; } = string.Empty;
    }
}
