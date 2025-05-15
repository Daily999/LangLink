using System;
using System.Collections.Generic;
namespace Studio.Daily.LangLink
{
    public class TsvToDictionary : ITableTxtToDictionary
    {
        public Dictionary<string, string> ParseTableTxt(string tableTxt)
        {
            var dict = new Dictionary<string, string>();
            var lines = tableTxt.Split(new[] { "\r\n", "\n", "\r" }, StringSplitOptions.RemoveEmptyEntries);

            for (int i = 1; i < lines.Length; i++)
            {
                var parts = lines[i].Split('\t');
                if (parts.Length > 1)
                {
                    string key = parts[0].Trim();
                    string value = parts[1].Trim();

                    if (!string.IsNullOrEmpty(key))
                        dict[key] = value;
                }
            }
            return dict;
        }
    }
}