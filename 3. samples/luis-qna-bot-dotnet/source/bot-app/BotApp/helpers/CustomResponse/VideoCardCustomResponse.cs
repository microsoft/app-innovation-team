using Microsoft.Bot.Builder;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Schema;

namespace BotApp
{
    public class VideoCardCustomResponse : ICustomResponse
    {
        public async Task SendActivityAsync(ITurnContext context, CancellationToken cancellationToken, string message)
        {
            Activity reply = context.Activity.CreateReply();

            try
            {
                VideoCardEntity videoCardEntity = JsonConvert.DeserializeObject<VideoCardEntity>(message);

                List<MediaUrl> mediaList = new List<MediaUrl>();
                foreach (string s in videoCardEntity.Media)
                    mediaList.Add(new MediaUrl(s));

                VideoCard videoCard = new VideoCard
                {
                    Title = videoCardEntity.Title,
                    Text = videoCardEntity.Text,
                    Autostart = false,
                    Media = mediaList
                };
                reply.Attachments.Add(videoCard.ToAttachment());
                await context.SendActivityAsync(reply);
            }
            catch
            {
                await context.SendActivityAsync("There was an error parsing the message", cancellationToken: cancellationToken);
            }
        }
    }
}