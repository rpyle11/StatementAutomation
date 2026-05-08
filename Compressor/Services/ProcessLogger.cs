using Compressor.Models;
using Microsoft.Extensions.Options;
using StatementProcessorModels;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace Compressor.Services
{
    public class ProcessLogger(IOptions<AppSettings> settings) : IProcessLogger
    {
        public async Task<bool> WriteProcessLog(WriteJobStepParameters parameters)
        {
            var httpHandler = new HttpClientHandler();
            httpHandler.Credentials = CredentialCache.DefaultNetworkCredentials;

            using var client = new HttpClient(httpHandler);
            client.BaseAddress = new Uri(settings.Value.ApiUrl!);
            client.DefaultRequestHeaders.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            var response = await client.PostAsJsonAsync($"{client.BaseAddress}{settings.Value.StepLogUrl}", parameters);

            return response.IsSuccessStatusCode;

           
        }
    }
}
