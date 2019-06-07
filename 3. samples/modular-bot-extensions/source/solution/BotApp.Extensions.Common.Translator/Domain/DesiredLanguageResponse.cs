using System.Collections.Generic;

namespace BotApp.Extensions.Common.Translator.Domain
{
    public class DesiredLanguageResponse
    {
        public string language { get; set; }
        public double score { get; set; }
        public bool isTranslationSupported { get; set; }
        public bool isTransliterationSupported { get; set; }
        public IEnumerable<DesiredLanguageResponse> alternatives { get; set; }
    }
}