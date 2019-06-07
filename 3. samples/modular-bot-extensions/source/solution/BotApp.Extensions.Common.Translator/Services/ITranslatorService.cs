using BotApp.Extensions.Common.Translator.Domain;
using System.Threading.Tasks;

namespace BotApp.Extensions.Common.Translator.Services
{
    public interface ITranslatorService
    {
        TranslatorConfig GetConfiguration();

        Task<string> GetDesiredLanguageAsync(string content);

        Task<string> TranslateSentenceAsync(string content, string originLanguage, string targetLanguage);
    }
}