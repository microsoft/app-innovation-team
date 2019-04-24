using Microsoft.Bot.Builder;
using System.Threading;
using System.Threading.Tasks;

namespace BotApp
{
    public interface ICustomResponse
    {
        Task SendActivityAsync(ITurnContext context, CancellationToken cancellationToken, string message);
    }
}