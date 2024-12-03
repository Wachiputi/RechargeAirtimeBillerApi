using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Microsoft.Extensions.Configuration;
using System.Net.Http;
using Core.Entities;
using Core.Interfaces;
using Microsoft.Extensions.Options;

namespace Infrastructure.Services
{
    public class RechargeService : IRechargeService
    {
        private readonly HttpClient _httpClient;
        private readonly SoapSettings _soapSettings;

        public RechargeService(HttpClient httpClient, IOptions<SoapSettings> soapSettings)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _soapSettings = soapSettings?.Value ?? throw new ArgumentNullException(nameof(soapSettings));
            ValidateSoapSettings();
        }

        private void ValidateSoapSettings()
        {
            if (string.IsNullOrEmpty(_soapSettings.WSUsername) ||
                string.IsNullOrEmpty(_soapSettings.WSPassword) ||
                string.IsNullOrEmpty(_soapSettings.ServiceUrl) ||
                string.IsNullOrEmpty(_soapSettings.Pin) ||
                string.IsNullOrEmpty(_soapSettings.Channel) ||
                string.IsNullOrEmpty(_soapSettings.ExternalSystem) ||
                string.IsNullOrEmpty(_soapSettings.Prefix) ||
                string.IsNullOrEmpty(_soapSettings.Additionalnfo) ||
                _soapSettings.Parameters == null || !_soapSettings.Parameters.Any())
            {
                throw new ArgumentException("Some required SoapSettings properties are missing or invalid.");
            }
        }

        public async Task<RechargeResponse> PerformRechargeAsync(RechargeRequest request)
        {
            try
            {
                string externalTxnId = GenerateNumericExternalTxnId();
                string requestTime = DateTime.UtcNow.ToString("o");
                string parametersXml = BuildParametersXml(_soapSettings.Parameters);

                string soapRequest = BuildSoapRequest(request, externalTxnId, requestTime, parametersXml);

                Console.WriteLine("Generated SOAP Request:");
                Console.WriteLine(soapRequest);

                var response = await SendSoapRequestAsync(soapRequest);

                var responseContent = await response.Content.ReadAsStringAsync();
                if (string.IsNullOrWhiteSpace(responseContent))
                {
                    return CreateFailedResponse("Empty or null response received.");
                }

                return ParseSoapResponse(responseContent) ?? CreateFailedResponse("Error parsing response.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during recharge operation: {ex.Message}");
                return CreateFailedResponse("An error occurred while processing the recharge request.");
            }
        }

        private string BuildSoapRequest(RechargeRequest request, string externalTxnId, string requestTime, string parametersXml)
        {
            return $@"
<soapenv:Envelope xmlns:soapenv=""http://schemas.xmlsoap.org/soap/envelope/"" xmlns:erec=""http://erecharge.sixdee.com"">
    <soapenv:Header/>
    <soapenv:Body>
        <erec:recharge>
            <RechargeRequest>
                <WSUsername>{_soapSettings.WSUsername}</WSUsername>
                <WSPassword>{_soapSettings.WSPassword}</WSPassword>
                <ExternalTxnId>{externalTxnId}</ExternalTxnId>
                <RequestTime>{requestTime}</RequestTime>
                <Channel>{_soapSettings.Channel}</Channel>
                <SenderMsisdn>{request.SenderMsisdn}</SenderMsisdn>
                <Pin>{_soapSettings.Pin}</Pin>
                <SubscriberMsisdn>{request.SenderMsisdn}</SubscriberMsisdn>
                <Amount>{request.Amount}</Amount>
                <ExternalSystem>{_soapSettings.ExternalSystem}</ExternalSystem>
                <Parameters>
                    {parametersXml}
                </Parameters>
                <Prefix>{_soapSettings.Prefix}</Prefix>
                <Additionalnfo>{_soapSettings.Additionalnfo}</Additionalnfo>
            </RechargeRequest>
        </erec:recharge>
    </soapenv:Body>
</soapenv:Envelope>";
        }

        private string BuildParametersXml(List<Parameter> parameters)
        {
            var sb = new StringBuilder();
            foreach (var parameter in parameters)
            {
                sb.AppendLine($"<Parameter><key>{parameter.Key}</key><value>{parameter.Value}</value></Parameter>");
            }
            return sb.ToString();
        }

        private async Task<HttpResponseMessage> SendSoapRequestAsync(string soapRequest)
        {
            try
            {
                var content = new StringContent(soapRequest, Encoding.UTF8, "text/xml");
                _httpClient.Timeout = TimeSpan.FromSeconds(60);

                var response = await _httpClient.PostAsync(_soapSettings.ServiceUrl, content);
                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception($"SOAP request failed with status code: {response.StatusCode}");
                }

                return response;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during HTTP request: {ex.Message}");
                throw;
            }
        }

        private RechargeResponse CreateFailedResponse(string message)
        {
            return new RechargeResponse
            {
                TransactionId = string.Empty,
                ExternalTxnId = string.Empty,
                CommandStatus = "Failed",
                ResponseMsg = message
            };
        }

        private string GenerateNumericExternalTxnId()
        {
            return new Random().Next(10000000, 99999999).ToString();
        }

        private RechargeResponse ParseSoapResponse(string responseContent)
        {
            try
            {
                var doc = XDocument.Parse(responseContent);
                var ns = "http://erecharge.sixdee.com";

                var rechargeResponse = doc.Descendants(XName.Get("RechargeResponse", ns)).FirstOrDefault();
                if (rechargeResponse == null)
                    throw new Exception("RechargeResponse node not found.");

                return new RechargeResponse
                {
                    TransactionId = rechargeResponse.Element(XName.Get("TransactionID", ns))?.Value ?? string.Empty,
                    ExternalTxnId = rechargeResponse.Element(XName.Get("ExternalTxnId", ns))?.Value ?? string.Empty,
                    CommandStatus = rechargeResponse.Element(XName.Get("CommandStatus", ns))?.Value ?? string.Empty,
                    ResponseMsg = rechargeResponse.Element(XName.Get("ResponseMsg", ns))?.Value ?? string.Empty
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error parsing SOAP response: {ex.Message}");
                return null;
            }
        }
    }
}
