using System.Collections.Generic;

namespace BotApp
{
    public class LuisDiscoveryResponseResult
    {
        public LuisDiscoveryResponse Result { get; set; }
    }

    public class LuisDiscoveryResponse
    {
        public bool IsSucceded { get; set; }
        public int ResultId { get; set; }
        public List<LuisAppDetail> LuisAppDetails { get; set; } = new List<LuisAppDetail>();
    }
}