using Microsoft.Bot.Builder;
using System.Threading;
using System.Threading.Tasks;

namespace BotApp
{
    public class TextCustomResponse : ICustomResponse
    {
        public async Task SendActivityAsync(ITurnContext context, CancellationToken cancellationToken, string message)
        {
            await context.SendActivityAsync(message, cancellationToken: cancellationToken);
        }
    }
}