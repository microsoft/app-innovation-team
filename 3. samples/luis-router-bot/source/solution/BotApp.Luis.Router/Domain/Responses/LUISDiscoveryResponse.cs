using System.Collections.Generic;

namespace BotApp.Luis.Router.Domain.Responses
{
    public class LUISDiscoveryResponse
    {
        public bool IsSucceded { get; set; }
        public int ResultId { get; set; }
        public List<LuisAppDetail> LuisAppDetails { get; set; } = new List<LuisAppDetail>();
    }
}