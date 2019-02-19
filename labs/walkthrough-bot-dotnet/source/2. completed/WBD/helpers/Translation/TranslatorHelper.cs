using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace WBD
{
    public static class TranslatorHelper
    {
        private static string translatorUrl = "https://api.cognitive.microsofttranslator.com";

        public async static Task<string> GetDesiredLanguageAsync(string content)
        {
            System.Object[] body = new System.Object[] { new { Text = content } };
            var requestBody = JsonConvert.SerializeObject(body);
            StringContent queryString = new StringContent(requestBody, System.Text.Encoding.UTF8, "application/json");

            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", Settings.TranslatorTextAPIKey);
                var response = await client.PostAsync(
               $"{translatorUrl}/detect?api-version=3.0", queryString);

                string result = string.Empty;
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    JArray jsonArray = JArray.Parse(json) as JArray;
                    dynamic first = jsonArray[0] as JObject;
                    result = first.language;
                }
                return result;
            }
        }

        public async static Task<string> TranslateSentenceAsync(string content, string originLanguage, string targetLanguage)
        {
            if (originLanguage == "en" && targetLanguage == "en")
                return content;

            System.Object[] body = new System.Object[] { new { Text = content } };
            var requestBody = JsonConvert.SerializeObject(body);
            StringContent queryString = new StringContent(requestBody, System.Text.Encoding.UTF8, "application/json");

            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", Settings.TranslatorTextAPIKey);
                var response = await client.PostAsync(
                    $"{translatorUrl}/translate?api-version=3.0&from={originLanguage}&to={targetLanguage}", queryString);

                string result = string.Empty;
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var res = JsonConvert.DeserializeObject<List<Dictionary<string, List<Dictionary<string, string>>>>>(json);
                    var translations = res[0]["translations"];
                    result = translations[0]["text"];
                }
                return result;
            }
        }
    }
}