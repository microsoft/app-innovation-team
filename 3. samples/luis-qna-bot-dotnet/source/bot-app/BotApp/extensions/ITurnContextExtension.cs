using Microsoft.Bot.Builder;
using System.Threading;
using System.Threading.Tasks;

namespace BotApp
{
    public static class ITurnContextExtension
    {
        public static async Task SendCustomResponseAsync(this ITurnContext context, string message, string responseType = "text", CancellationToken cancellationToken = default(CancellationToken))
        {
            ICustomResponse customResponse = null;

            switch (responseType)
            {
                case "text":
                    customResponse = new TextCustomResponse();
                    break;

                case "video":
                    customResponse = new VideoCardCustomResponse();
                    break;

                default:
                    customResponse = new TextCustomResponse();
                    message = "There was an error identifying the metadata tag";
                    break;
            }

            await customResponse.SendActivityAsync(context, cancellationToken, message);
        }
    }
}