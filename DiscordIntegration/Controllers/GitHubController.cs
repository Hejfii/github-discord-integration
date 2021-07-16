using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace DiscordIntegration.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class GitHubController : ControllerBase
    {
        [HttpGet]
        public ActionResult Get()
        {
            return Ok("Ok");
        }

        [HttpPost]
        public async Task<ActionResult> Post([FromQuery(Name = "serverId")] string serverId, [FromQuery(Name = "webhookToken")] string webhookToken)
        {
            var discordUrl = $"https://discord.com/api/webhooks/{serverId}/{webhookToken}";
            var gitAvatarUrl = "https://github.githubassets.com/images/modules/logos_page/GitHub-Mark.png";
            //var orderedEmbeds = embeds.OrderBy(e => e.Timestamp).ToList();
            var messages = new List<DiscordMessage>();
            for (var i = 0; i < 50; i += 10)
            {
                messages.Add(new DiscordMessage
                (
                    discordUrl, // WEBHOOK URL
                    8 > 1 ? "**8 commits**" : "", // CONTENT
                    "GitHub", // USERNAME
                    gitAvatarUrl, // AVATAR URL
                    false // TTS
                ));
            }

            foreach (var message in messages)
            {
                await message.Send();
            }
            return Ok(serverId + " " + webhookToken);
        }
    }
}