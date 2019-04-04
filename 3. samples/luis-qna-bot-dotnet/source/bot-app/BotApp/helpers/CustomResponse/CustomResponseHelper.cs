using Microsoft.Bot.Builder;
using System.Threading;
using System.Threading.Tasks;

namespace BotApp
{
    public class CustomResponseHelper : BaseHelper
    {
        private ICustomResponse customResponse = null;

        public CustomResponseHelper()
        {
        }

        public async Task SendActivityAsync(ITurnContext context, CancellationToken cancellationToken, string responseType, string message)
        {
            switch (responseType)
            {
                case "text":
                    this.customResponse = new TextCustomResponse();
                    break;

                case "video":
                    this.customResponse = new VideoCardCustomResponse();
                    break;

                default:
                    this.customResponse = new TextCustomResponse();
                    message = "There was an error identifying the metadata tag";
                    break;
            }

            await this.customResponse.SendActivityAsync(context, cancellationToken, message);
        }
    }
}