using BotApp.Luis.Router.Domain;
using BotApp.Luis.Router.Domain.Enums;
using BotApp.Luis.Router.Domain.Exceptions;
using BotApp.Luis.Router.Domain.Requests;
using BotApp.Luis.Router.Domain.Responses;
using BotApp.Luis.Router.Domain.Settings;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Adapters;
using Microsoft.Bot.Builder.AI.Luis;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace BotApp.Luis.Router.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LuisDiscoveryController : ControllerBase
    {
        private readonly IBotTelemetryClient telemetry = null;
        private readonly ILogger logger = null;

        public LuisDiscoveryController(IBotTelemetryClient telemetry, ILogger<LuisDiscoveryController> logger)
        {
            this.telemetry = telemetry;
            this.logger = logger;
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Post([FromBody]LuisDiscoveryRequest model)
        {
            // non-forced-to-disposal
            LuisDiscoveryResponse result = new LuisDiscoveryResponse
            {
                IsSucceded = true,
                ResultId = (int)LuisDiscoveryResponseEnum.Success
            };

            // forced-to-disposal

            try
            {
                if (string.IsNullOrEmpty(model.Text))
                    throw new BusinessException((int)LuisDiscoveryResponseEnum.FailedEmptyText);

                // building service list
                Settings.LuisServices = new Dictionary<string, LuisRecognizer>();
                foreach (LuisAppRegistration app in Settings.LuisAppRegistrations)
                {
                    var luis = new LuisApplication(app.LuisAppId, app.LuisAuthoringKey, app.LuisEndpoint);

                    LuisPredictionOptions luisPredictionOptions = null;
                    LuisRecognizer recognizer = null;

                    bool needsPredictionOptions = false;
                    if ((!string.IsNullOrEmpty(model.BingSpellCheckSubscriptionKey)) || (model.EnableLuisTelemetry))
                    {
                        needsPredictionOptions = true;
                    }

                    if (needsPredictionOptions)
                    {
                        luisPredictionOptions = new LuisPredictionOptions();

                        if (model.EnableLuisTelemetry)
                        {
                            luisPredictionOptions.TelemetryClient = telemetry;
                            luisPredictionOptions.Log = true;
                            luisPredictionOptions.LogPersonalInformation = true;
                        }

                        if (!string.IsNullOrEmpty(model.BingSpellCheckSubscriptionKey))
                        {
                            luisPredictionOptions.BingSpellCheckSubscriptionKey = model.BingSpellCheckSubscriptionKey;
                            luisPredictionOptions.SpellCheck = true;
                            luisPredictionOptions.IncludeAllIntents = true;
                        }

                        recognizer = new LuisRecognizer(luis, luisPredictionOptions);
                    }
                    else
                    {
                        recognizer = new LuisRecognizer(luis);
                    }

                    Settings.LuisServices.Add(app.LuisName, recognizer);
                }

                foreach (LuisAppRegistration app in Settings.LuisAppRegistrations)
                {
                    var storage = new MemoryStorage();
                    var conversationState = new ConversationState(storage);

                    var adapter = new TestAdapter().Use(new AutoSaveStateMiddleware(conversationState));

                    IMessageActivity msg = Activity.CreateMessageActivity();
                    msg.Id = Guid.NewGuid().ToString();
                    msg.From = new ChannelAccount("sip: account@botapp-luis-router.com", "bot");
                    msg.Recipient = new ChannelAccount("sip: account@botapp-luis-router.com", "agent");
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
                    if (topIntent != null && topIntent.HasValue && topIntent.Value.score >= .90 && topIntent.Value.intent != "None")
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
                    result.ResultId = (int)LuisDiscoveryResponseEnum.Failed;

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

            string message = EnumDescription.GetEnumDescription((LuisDiscoveryResponseEnum)result.ResultId);
            this.logger.LogInformation($">> Message information: {message}");

            return (result.IsSucceded) ? (ActionResult)new OkObjectResult(result) : (ActionResult)new BadRequestObjectResult(new { message = message });
        }
    }
}