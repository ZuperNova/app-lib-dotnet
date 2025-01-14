using System.Text;
using Altinn.App.Core.Configuration;
using Altinn.App.Core.Constants;
using Altinn.App.Core.Helpers;
using Altinn.App.Core.Internal.Pdf;
using Altinn.App.Core.Models;
using Microsoft.Extensions.Options;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Altinn.App.Core.Infrastructure.Clients.Pdf
{
    /// <summary>
    /// A client for handling actions on pdf in Altinn Platform.
    /// </summary>
    public class PDFClient : IPDF
    {
        private readonly HttpClient _client;
        private readonly JsonSerializer _camelCaseSerializer;

        /// <summary>
        /// Creates a new instance of the <see cref="PDFClient"/> class
        /// </summary>
        /// <param name="platformSettings">The platform settingssettings</param>
        /// <param name="httpClient">The http client</param>
        public PDFClient(
            IOptions<PlatformSettings> platformSettings,
            HttpClient httpClient)
        {
            _camelCaseSerializer = JsonSerializer.Create(
                new JsonSerializerSettings
                {
                    ContractResolver = new CamelCaseExceptDictionaryResolver()
                });

            httpClient.BaseAddress = new Uri(platformSettings.Value.ApiPdfEndpoint);
            httpClient.DefaultRequestHeaders.Add(General.SubscriptionKeyHeaderName, platformSettings.Value.SubscriptionKey);
            _client = httpClient;
        }

        /// <inheritdoc/>
        public async Task<Stream> GeneratePDF(PDFContext pdfContext)
        {
            using HttpContent data = new StringContent(JObject.FromObject(pdfContext, _camelCaseSerializer).ToString(), Encoding.UTF8, "application/json");
            HttpResponseMessage response = await _client.PostAsync("generate", data);
            response.EnsureSuccessStatusCode();
            Stream pdfContent = await response.Content.ReadAsStreamAsync();
            return pdfContent;
        }
    }
}
