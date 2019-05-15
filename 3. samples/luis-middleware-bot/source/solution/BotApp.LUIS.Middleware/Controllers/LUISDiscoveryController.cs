using BotApp.LUIS.Middleware.Domain;
using BotApp.LUIS.Middleware.Domain.Enums;
using BotApp.LUIS.Middleware.Domain.Exceptions;
using BotApp.LUIS.Middleware.Domain.Requests;
using BotApp.LUIS.Middleware.Domain.Responses;
using BotApp.LUIS.Middleware.Domain.Settings;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Adapters;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace BotApp.LUIS.Middleware.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LuisDiscoveryController : ControllerBase
    {
        private readonly ILogger logger = null;

        public LuisDiscoveryController(ILogger<LuisDiscoveryController> logger)
        {
            this.logger = logger;
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Post([FromBody]LUISDiscoveryRequest model)
        {
            // non-forced-to-disposal
            LUISDiscoveryResponse result = new LUISDiscoveryResponse
            {
                IsSucceded = true,
                ResultId = (int)LUISDiscoveryResponseEnum.Success
            };

            // forced-to-disposal

            try
            {
                if (string.IsNullOrEmpty(model.Text))
                    throw new BusinessException((int)LUISDiscoveryResponseEnum.FailedEmptyText);

                foreach (LuisAppRegistration app in Settings.LuisAppRegistrations)
                {
                    var storage = new MemoryStorage();
                    var conversationState = new ConversationState(storage);

                    var adapter = new TestAdapter().Use(new AutoSaveStateMiddleware(conversationState));

                    IMessageActivity msg = Activity.CreateMessageActivity();
                    msg.Id = Guid.NewGuid().ToString();
                    msg.From = new ChannelAccount("sip: account@middleware.com", "bot");
                    msg.Recipient = new ChannelAccount("sip: account@middleware.com ", "agent");
                    msg.Text = model.Text;
                    msg.Locale = "en-us";
                    msg.ServiceUrl = "url";
                    msg.ChannelId = Guid.NewGuid().ToString();
                    msg.Conversation = new ConversationAccount();
                    msg.Type = ActivityTypes.Message;
                    msg.Timestamp = DateTime.UtcNow;

                    var context = new TurnContext(adapter, (Activity)msg);

                    var recognizerResult = await Settings.LuisServices[app.LuisName].RecognizeAsync(context, new CancellationToken());
                    var topIntent = recognizerResult?.GetTopScoringIntent();
                    if (topIntent != null && topIntent.HasValue && topIntent.Value.score >= .80 && topIntent.Value.intent != "None")
                    {
                        result.LuisAppDetails.Add(new LuisAppDetail() { Name = app.LuisName, Intent = topIntent.Value.intent, Score = topIntent.Value.score });
                    }
                }
            }
            catch (Exception ex)
            {
                result.IsSucceded = false;

                if (ex is BusinessException)
                {
                    result.ResultId = ((BusinessException)ex).ResultId;
                }
                else
                {
                    result.ResultId = (int)LUISDiscoveryResponseEnum.Failed;

                    this.logger.LogError($">> Exception: {ex.Message}, StackTrace: {ex.StackTrace}");

                    if (ex.InnerException != null)
                    {
                        this.logger.LogError($">> Inner Exception Message: {ex.InnerException.Message}, Inner Exception StackTrace: {ex.InnerException.StackTrace}");
                    }
                }
            }
            finally
            {
                // clean forced-to-disposal

                GC.Collect();
            }

            string message = EnumDescription.GetEnumDescription((LUISDiscoveryResponseEnum)result.ResultId);
            this.logger.LogInformation($">> Message information: {message}");

            return (result.IsSucceded) ? (ActionResult)new OkObjectResult(new { result = result }) : (ActionResult)new BadRequestObjectResult(new { message = message });
        }
    }
}