using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json.Linq;

namespace DiscordIntegration.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class GitHubController : ControllerBase
    {
        private const string Sha1Prefix = "sha1=";
        public string gitAvatarUrl = "https://github.githubassets.com/images/modules/logos_page/GitHub-Mark.png";
            
        [HttpGet]
        public ActionResult Get()
        {
            return Ok("Ok");
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] Object content, [FromQuery(Name = "serverId")] string serverId, [FromQuery(Name = "webhookToken")] string webhookToken)
        {
            Request.Headers.TryGetValue("X-GitHub-Event", out StringValues eventName);
            Request.Headers.TryGetValue("X-Hub-Signature", out StringValues signature);
            Request.Headers.TryGetValue("X-GitHub-Delivery", out StringValues delivery);

            using (var reader = new StreamReader(Request.Body))
            {
                var txt = await reader.ReadToEndAsync();

                if (IsGithubPushAllowed(content.ToString(), eventName, signature))
                {

                    if (serverId == null || webhookToken == null || content == null)
                    {
                        return Conflict("Empty data");
                    }

                    var discordUrl = $"https://discord.com/api/webhooks/{serverId}/{webhookToken}";
                    var data = JObject.Parse(content.ToString());
                    var commits = data["commits"];
                    var repoName = data["repository"]["name"];
                    var branch = data["repository"]["master_branch"];
                    var embeds = new List<DiscordMessageEmbed>();
                    var privateCommit = "**This commit has ben marked as private.**";
                    bool privateC = false;

                    if (commits != null)
                    {
                        foreach (var commit in commits)
                        {
                            if (commit["message"].ToString().StartsWith("!"))
                            {
                                privateC = true;
                            }

                            string[] commitMsg = commit["message"].ToString().Split(": ");

                            var author = new DiscordMessageEmbedAuthor
                            {
                                Name = commit["committer"]["name"].ToString(),
                                Url = $"https://github.com/{commit["committer"]["username"].ToString()}",
                                IconUrl =
                                    $"https://avatars.githubusercontent.com/{commit["committer"]["username"].ToString()}"
                            };

                            embeds.Add(new DiscordMessageEmbed
                            {
                                Title = privateC ? null : commitMsg[0],
                                Color = int.Parse("3498db", System.Globalization.NumberStyles.HexNumber),
                                Author = author,
                                Url = commit["url"].ToString(),
                                Description = privateC ? privateCommit : commitMsg[1],
                                Footer = new DiscordMessageEmbedFooter(
                                    $"{repoName}:{branch} | {commit["timestamp"].ToString()}")
                            });
                        }
                    }

                    if (data["action"] != null && data["action"].ToString() == "completed" &&
                        data["check_run"]["check_suite"] != null)
                    {
                        var author = new DiscordMessageEmbedAuthor
                        {
                            Name = data["sender"]["login"].ToString(),
                            Url = data["sender"]["html_url"].ToString(),
                            IconUrl = data["sender"]["avatar_url"].ToString()
                        };

                        var end = DateTime.Parse(data["check_run"]["completed_at"].ToString(),
                            CultureInfo.CurrentCulture);
                        var start = DateTime.Parse(data["check_run"]["started_at"].ToString(),
                            CultureInfo.CurrentCulture);
                        var result = end - start;

                        embeds.Add(new DiscordMessageEmbed
                        {
                            Title = data["check_run"]["name"].ToString(),
                            Color = int.Parse(
                                data["check_run"]["conclusion"].ToString() == "success" ? "27ae5f" : "e91b1b",
                                System.Globalization.NumberStyles.HexNumber),
                            Author = author,
                            Url = data["check_run"]["url"].ToString(),
                            Description = data["check_run"]["conclusion"].ToString() == "success"
                                ? $"Passed test in {result}"
                                : $"Failed test in {result}",
                            Footer = new DiscordMessageEmbedFooter(
                                $"{repoName} | {data["check_run"]["completed_at"].ToString()}")
                        });
                    }

                    //var orderedEmbeds = embeds.ToArray().OrderBy(e => e.Timestamp).ToList();
                    var messages = new List<DiscordMessage>();
                    for (var i = 0; i < embeds.Count; i += 10)
                    {
                        messages.Add(new DiscordMessage
                        (
                            discordUrl, // WEBHOOK URL
                            embeds.Count > 1 ? $"**{embeds.Count} commits**" : "", // CONTENT
                            "GitHub", // USERNAME
                            gitAvatarUrl, // AVATAR URL
                            false, // TTS
                            embeds.ToArray()
                        ));
                    }

                    if (embeds.Count <= 0)
                    {
                        return Conflict("No data");
                    }

                    foreach (var message in messages)
                    {
                        await message.Send();
                    }

                    return Ok("Ok");
                }
            }

            return Unauthorized();
        }
        
        private bool IsGithubPushAllowed(string payload, string eventName, string signatureWithPrefix)
        {
            if (string.IsNullOrWhiteSpace(payload))
            {
                throw new ArgumentNullException(nameof(payload));
            }
            if (string.IsNullOrWhiteSpace(eventName))
            {
                throw new ArgumentNullException(nameof(eventName));
            }
            if (string.IsNullOrWhiteSpace(signatureWithPrefix))
            {
                throw new ArgumentNullException(nameof(signatureWithPrefix));
            }

            /* test if the eventName is ok if you want 
            if (!eventName.Equals("push", StringComparison.OrdinalIgnoreCase))
            {
                ...
            } */

            if (signatureWithPrefix.StartsWith(Sha1Prefix, StringComparison.OrdinalIgnoreCase))
            {
                var signature = signatureWithPrefix.Substring(Sha1Prefix.Length);
                var secret = Encoding.ASCII.GetBytes("CgRrjXUC8a2MXcp");
                var payloadBytes = Encoding.ASCII.GetBytes(payload);

                using (var hmSha1 = new HMACSHA1(secret))
                {
                    var hash = hmSha1.ComputeHash(payloadBytes);

                    var hashString = ToHexString(hash);

                    if (hashString.Equals(signature))
                    {
                        return true;
                    }
                }
            }

            return false;
        }
        
        public static string ToHexString(byte[] bytes)
        {
            var builder = new StringBuilder(bytes.Length * 2);
            foreach (byte b in bytes)
            {
                builder.AppendFormat("{0:x2}", b);
            }

            return builder.ToString();
        }
    }
}