using DTOs;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Utils;

namespace Api
{
    public class AgentApiClient:ApiClientBase
    {
        public AgentApiClient() : base() { }

        public async Task<AgentResponse?> SendVoiceCommandAsync(string text)
        {
            try
            {
                var requestUrl = $"{Config.AgentUrl}/agent";
                var requestDto = new AgentRequest { Text = text };

                var json = JsonSerializer.Serialize(requestDto);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
           

                var response = await Http.PostAsync(requestUrl, content);

                if (response.IsSuccessStatusCode)
                {
                    var responseJson = await response.Content.ReadAsStringAsync();
                    // Case insensitive para asegurar que mapea bien
                    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    return JsonSerializer.Deserialize<AgentResponse>(responseJson, options);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error Agent API: {ex.Message}");
            }

            return null;
        }
    }
}