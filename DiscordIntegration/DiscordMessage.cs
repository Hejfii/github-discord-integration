using Newtonsoft.Json;
using Polly;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace DiscordIntegration
{
    [JsonObject(MemberSerialization.OptIn)]
    public class DiscordMessage
    {
        /// <summary>
        /// Url of the webhook
        /// </summary>
        public string Url { get; private set; }
        
        /// <summary>
        /// Overrides the current username of the webhook
        /// </summary>
        [JsonProperty("username")]
        public string Username { get; private set; }
        
        /// <summary>
        /// Overrides the default avatar of the webhook
        /// </summary>
        [JsonProperty("avatar_url")]
        public string AvatarUrl { get; private set; }
        
        /// <summary>
        /// Message content up to 2000 characters
        /// </summary>
        [JsonProperty("content")]
        public string Content { get; private set; }
        
        /// <summary>
        /// If true, the message will be pronounced in chat like tts message
        /// </summary>
        [JsonProperty("tts")]
        public bool Tts { get; private set; }
        
        /// <summary>
        /// An array of embed objects. Max 10
        /// </summary>
        [JsonProperty("embeds")]
        public DiscordMessageEmbed[] Embeds { get; private set; }

        [JsonConstructor]
        private DiscordMessage()
        {
            
        }

        public DiscordMessage(
            string url,
            string content,
            string username = null,
            string avatarUrl = null,
            bool tts = false,
            DiscordMessageEmbed[] embeds = null)
        {
            this.Url = url;
            this.Content = content;
            this.Username = username?.Trim();
            this.AvatarUrl = avatarUrl?.Trim();
            this.Tts = tts;
            this.Embeds = embeds?.Where(x => x != null)?.ToArray();

            Validate();
        }

        internal void Validate()
        {
            if (!string.IsNullOrEmpty(this.Content) && this.Content?.Length > 2000)
            {
                this.Content =
                    $"Message content cannot be longer than 2000 characters, current length is {this.Content.Length}.";
            }
        }

        /// <summary>
        /// Send a message to Discord using a webhook
        /// </summary>
        /// <param name="message">Message to be sent</param>
        public async Task Send(DiscordMessage message = null)
        {
            try
            {
                if (message == null && this != null)
                    message = this;
                        
                if (message == null)
                    throw new ArgumentNullException(nameof(message), "The message cannot be null.");

                using (var content = new StringContent(JsonConvert.SerializeObject(message), Encoding.UTF8, "application/json"))
                using (var client = new HttpClient {Timeout = new TimeSpan(0, 0, 30)})
                {
                    var response = await Policy
                        .HandleResult<HttpResponseMessage>(x => !x.IsSuccessStatusCode)
                        .RetryAsync(3, onRetry: (httpResponse, _) =>
                        {
                            if ((int) httpResponse.Result.StatusCode == 429) // too many requests
                            {
                                var jsonBody =
                                    JsonConvert.DeserializeObject<DiscordTooManyRequestsResponse>(httpResponse.Result
                                        .Content.ReadAsStringAsync().Result);

                                if (jsonBody != null)
                                    System.Threading.Thread.Sleep(jsonBody.RetryAfter + 1);
                            }
                        })
                        .ExecuteAsync(async () => await client.PostAsync(this.Url, content));

                    if (!response.IsSuccessStatusCode && response.StatusCode != HttpStatusCode.NoContent)
                    {
                        var responseContent = await response.Content.ReadAsStringAsync();

                        throw new DiscordWebhookClientException(
                            $"An error occured in sending the message: {responseContent} - HTTP status code {(int) response.StatusCode} - {response.StatusCode}",
                            responseContent, response.StatusCode);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error has occured {ex}");
                throw;
            }
        }
    }

    public class DiscordTooManyRequestsResponse
    {
        [JsonProperty("message")]
        public string Message { get; set; }
        
        [JsonProperty("retry_after")]
        public int RetryAfter { get; set; }
        
        [JsonProperty("global")]
        public bool Global { get; set; }
    }
}