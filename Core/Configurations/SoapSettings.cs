using System.Collections.Generic;

namespace Core.Entities
{
    public class SoapSettings
    {
        public string WSUsername { get; set; } = string.Empty;
        public string WSPassword { get; set; } = string.Empty;
        public string Pin { get; set; } = string.Empty;
        public string Channel { get; set; } = string.Empty;
        public string SubscriberMsisdn { get; set; } = string.Empty;
        public string ExternalSystem { get; set; } = string.Empty;
        public string Prefix { get; set; } = string.Empty;
        public string Additionalnfo { get; set; } = string.Empty;
        public List<Parameter> Parameters { get; set; } = new List<Parameter>();
        public string? ServiceUrl { get; set; }
    }
}
