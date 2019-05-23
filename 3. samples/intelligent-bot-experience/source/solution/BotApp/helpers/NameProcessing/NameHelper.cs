using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BotApp
{
    public class NameHelper
    {
        public static List<string> GetFromSquareBrackets(string content)
        {
            List<string> result = new List<string>();
            int idxOpen = 0;
            for (int i = 0; i < content.Length; i++)
            {
                if (content[i].ToString() == "[")
                {
                    idxOpen = i;
                }
                else if (content[i].ToString() == "]")
                {
                    var delta = i - 1 - idxOpen;
                    result.Add(content.Substring(idxOpen + 1, delta));
                    idxOpen = 0;
                }
            }
            return result;
        }

        public static List<(string name, string lastname)> Combinations(string name)
        {
            var characters = name.Select(c => char.IsLetter(c) || char.IsWhiteSpace(c) ? c : '\0').ToArray();
            StringBuilder sb = new StringBuilder();
            sb.Append(characters);
            name = sb.ToString().Replace("\0", "");

            List<(string name, string lastname)> result = new List<(string name, string lastname)>();

            for (int i = 0; i < name.Length; i++)
            {
                if (name[i].ToString() == " ")
                {
                    string n = name.Substring(0, i);
                    var delta = name.Length - 1 - i;
                    string l = name.Substring(i + 1, delta);

                    result.Add((n, l));
                }
            }

            if (result.Count == 0)
            {
                result.Add((name, name));
            }

            return result;
        }
    }
}