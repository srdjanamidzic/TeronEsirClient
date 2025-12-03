using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using TeronEsirClient.Enums;
using TeronEsirClient.Enums.Utils;
using TeronEsirClient.Json;
using TeronEsirClient.Models;
using TeronEsirClient.Models.Fiscalization.Read;
using TeronEsirClient.Models.Fiscalization.Write;

namespace TeronEsirClient.Client
{
    public partial class TeronEsirApiClient
    {
        public async Task<TeronEsirResult<TeronEsirStatus>> GetStatusAsync()
        {
            return await GetStatusAsync(GetParametersFromHttpClient());
        }

        public async Task<TeronEsirResult<TeronEsirStatus>> GetStatusAsync(TeronEsirApiParameters apiParameters)
        {
            var requestMessage = CreateGetRequestMessage(apiParameters, "status");
            var response = await _httpClient.SendAsync(requestMessage);

            return await TeronEsirResult<TeronEsirStatus>.FromHttpResponseAsync(response);
        }

        public async Task<TeronEsirResult<LpfrStatus>> UnlockSecureElementAsync(string pin)
        {
            return await UnlockSecureElementAsync(GetParametersFromHttpClient(), pin);
        }

        public async Task<TeronEsirResult<LpfrStatus>> UnlockSecureElementAsync(TeronEsirApiParameters apiParameters, string pin)
        {
            var requestMessage = CreatePostRequestMessage(apiParameters, "pin", pin, "text/plain");
            var response = await _httpClient.SendAsync(requestMessage);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (!string.IsNullOrEmpty(responseContent) && responseContent.Length == 6)
            {
                responseContent = responseContent.Substring(1, 4);
            }
            else
            {
                return TeronEsirResult<LpfrStatus>.Fail(response.ReasonPhrase, responseContent);
            }

            if (!response.IsSuccessStatusCode)
            {
                return TeronEsirResult<LpfrStatus>.Fail(response.ReasonPhrase, responseContent, EnumConverter.ParseStringValue<LpfrStatus>(responseContent));
            }

            return TeronEsirResult<LpfrStatus>.Success(EnumConverter.ParseStringValue<LpfrStatus>(responseContent), responseContent);
        }

        public async Task<TeronEsirResult<TeronEsirInvoiceResponse>> IssueInvoiceAsync(TeronEsirInvoice invoice, string requestId = null)
        {
            return await IssueInvoiceAsync(GetParametersFromHttpClient(), invoice, requestId);
        }

        public async Task<TeronEsirResult<TeronEsirInvoiceResponse>> IssueInvoiceAsync(TeronEsirApiParameters apiParameters, TeronEsirInvoice invoice, string requestId = null)
        {
            var requestMessage = CreatePostRequestMessage(apiParameters, "invoices", invoice);
            if (!string.IsNullOrWhiteSpace(requestId))
            {
                ThrowIfInvalidRequestId(requestId);
                requestMessage.Headers.Add("RequestId", requestId);
            }

            var response = await _httpClient.SendAsync(requestMessage);

            return await CreateIssueInvoiceResultFromHttpResponseAsync(response);
        }

        public async Task<TeronEsirResult<TeronEsirInvoiceResponse>> IssueFinalInvoiceAsync(TeronEsirFinalInvoice finalInvoice, string requestId = null)
        {
            return await IssueFinalInvoiceAsync(GetParametersFromHttpClient(), finalInvoice, requestId);
        }

        public async Task<TeronEsirResult<TeronEsirInvoiceResponse>> IssueFinalInvoiceAsync(TeronEsirApiParameters apiParameters, TeronEsirFinalInvoice finalInvoice, string requestId = null)
        {
            var requestMessage = CreatePostRequestMessage(apiParameters, "invoices/final", finalInvoice);
            if (!string.IsNullOrWhiteSpace(requestId))
            {
                ThrowIfInvalidRequestId(requestId);
                requestMessage.Headers.Add("RequestId", requestId);
            }

            var response = await _httpClient.SendAsync(requestMessage);

            return await CreateIssueInvoiceResultFromHttpResponseAsync(response);
        }

        public async Task<TeronEsirResult<TeronEsirInvoiceContents>> GetInvoiceContentsAsync(string invoiceNumber, ReceiptLayout receiptLayout, ReceiptImageFormat receiptImageFormat, bool includeHeaderAndFooter)
        {
            return await GetInvoiceContentsAsync(GetParametersFromHttpClient(), invoiceNumber, receiptLayout, receiptImageFormat, includeHeaderAndFooter);
        }

        public async Task<TeronEsirResult<TeronEsirInvoiceContents>> GetInvoiceContentsAsync(TeronEsirApiParameters apiParameters, string invoiceNumber, ReceiptLayout receiptLayout, ReceiptImageFormat receiptImageFormat, bool includeHeaderAndFooter)
        {
            NameValueCollection queryParams = new NameValueCollection()
            {
                { "receiptLayout", EnumConverter.GetStringValue(receiptLayout) },
                { "receiptImageFormat", EnumConverter.GetStringValue(receiptImageFormat) },
                { "includeHeaderAndFooter", includeHeaderAndFooter.ToString() },
            };

            var requestMessage = CreateGetRequestMessage(apiParameters, $"invoices/{invoiceNumber}", queryParams);
            var response = await _httpClient.SendAsync(requestMessage);

            return await TeronEsirResult<TeronEsirInvoiceContents>.FromHttpResponseAsync(response);
        }

        public async Task<TeronEsirResult<TeronEsirInvoiceContents>> GetLastInvoiceContentsAsync(ReceiptLayout receiptLayout, ReceiptImageFormat receiptImageFormat, bool includeHeaderAndFooter)
        {
            return await GetLastInvoiceContentsAsync(GetParametersFromHttpClient(), receiptLayout, receiptImageFormat, includeHeaderAndFooter);
        }

        public async Task<TeronEsirResult<TeronEsirInvoiceContents>> GetLastInvoiceContentsAsync(TeronEsirApiParameters apiParameters, ReceiptLayout receiptLayout, ReceiptImageFormat receiptImageFormat, bool includeHeaderAndFooter)
        {
            NameValueCollection queryParams = new NameValueCollection()
            {
                { "receiptLayout", EnumConverter.GetStringValue(receiptLayout) },
                { "receiptImageFormat", EnumConverter.GetStringValue(receiptImageFormat) },
                { "includeHeaderAndFooter", includeHeaderAndFooter.ToString() },
            };

            var requestMessage = CreateGetRequestMessage(apiParameters, "invoices/last", queryParams);
            var response = await _httpClient.SendAsync(requestMessage);

            return await TeronEsirResult<TeronEsirInvoiceContents>.FromHttpResponseAsync(response);
        }

        public async Task<TeronEsirResult<TeronEsirInvoiceResponse>> GetInvoiceResponseByRequestIdAsync(string requestId)
        {
            return await GetInvoiceResponseByRequestIdAsync(GetParametersFromHttpClient(), requestId);
        }
        public async Task<TeronEsirResult<TeronEsirInvoiceResponse>> GetInvoiceResponseByRequestIdAsync(TeronEsirApiParameters apiParameters, string requestId)
        {
            var requestMessage = CreateGetRequestMessage(apiParameters, $"invoices/request/{requestId}");
            var response = await _httpClient.SendAsync(requestMessage);

            return await TeronEsirResult<TeronEsirInvoiceResponse>.FromHttpResponseAsync(response);
        }

        public async Task<TeronEsirResult<TeronEsirInvoiceSearchResult[]>> SearchInvoicesAsync(TeronEsirInvoiceSearchParameters searchParameters)
        {
            return await SearchInvoicesAsync(GetParametersFromHttpClient(), searchParameters);
        }

        public async Task<TeronEsirResult<TeronEsirInvoiceSearchResult[]>> SearchInvoicesAsync(TeronEsirApiParameters apiParameters, TeronEsirInvoiceSearchParameters searchParameters)
        {
            var requestMessage = CreatePostRequestMessage(apiParameters, "invoices/search", searchParameters);
            var response = await _httpClient.SendAsync(requestMessage);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                return TeronEsirResult<TeronEsirInvoiceSearchResult[]>.Fail(response.ReasonPhrase, responseContent);
            }

            if (string.IsNullOrWhiteSpace(responseContent))
            {
                return TeronEsirResult<TeronEsirInvoiceSearchResult[]>.Success(new TeronEsirInvoiceSearchResult[] { }, responseContent);
            }

            var results = responseContent.Split('\n')
                .Select(a => TeronEsirInvoiceSearchResult.FromCsvString(a))
                .ToArray();

            return TeronEsirResult<TeronEsirInvoiceSearchResult[]>.Success(results, responseContent);
        }

        private static async Task<TeronEsirResult<TeronEsirInvoiceResponse>> CreateIssueInvoiceResultFromHttpResponseAsync(HttpResponseMessage httpResponseMessage)
        {
            var responseContent = await httpResponseMessage.Content.ReadAsStringAsync();

            if (httpResponseMessage.IsSuccessStatusCode)
            {
                TeronEsirInvoiceResponse value = JsonConvert.DeserializeObject<TeronEsirInvoiceResponse>(
                    responseContent,
                    GlobalJsonSerializerSettings.Settings);

                return TeronEsirResult<TeronEsirInvoiceResponse>.Success(value, responseContent);
            }

            string message = string.Empty;
            try
            {
                Dictionary<string, JToken> responseFieldDict = JsonConvert.DeserializeObject<Dictionary<string, JToken>>(responseContent);

                if (responseFieldDict.TryGetValue(nameof(TeronEsirError.Message).ToLower(), out JToken deserializedMessage))
                {
                    message = deserializedMessage.ToString();
                }

                if (responseFieldDict.TryGetValue("invoiceResponse", out JToken invoiceResponse))
                {
                    TeronEsirInvoiceResponse value = invoiceResponse.ToObject<TeronEsirInvoiceResponse>(JsonSerializer.Create(GlobalJsonSerializerSettings.Settings));
                    return TeronEsirResult<TeronEsirInvoiceResponse>.Fail(message, responseContent, value);
                }
            }
            catch (JsonSerializationException)
            {
                message = httpResponseMessage.ReasonPhrase;
            }

            return TeronEsirResult<TeronEsirInvoiceResponse>.Fail(message, responseContent);
        }

        private void ThrowIfInvalidRequestId(string requestId)
        {
            const int maxRequestIdLength = 32;

            if (requestId != null && requestId.Length > maxRequestIdLength)
            {
                throw new System.ArgumentException($"RequestId cannot be longer than {maxRequestIdLength} characters.", nameof(requestId));
            }

            if (requestId.Any(a => !char.IsLetterOrDigit(a)))
            {
                throw new System.ArgumentException("RequestId can only contain alphanumeric characters.", nameof(requestId));
            }
        }
    }
}
