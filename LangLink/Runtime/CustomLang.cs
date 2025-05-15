using System.Collections.Generic;
using UnityEngine.Localization;
namespace Studio.Daily.LangLink
{
    public class CustomLang
    {
        public Locale Locale { get; private set; }
        public string LocaleCode { get; set; }
        public string TableName { get; private set; }
        

        public Dictionary<string, string> Content { get; private set; }

        public CustomLang(Locale locale, string tableName, Dictionary<string, string> content)
        {
            Locale = locale;
            TableName = tableName;
            Content = content;
        }
    }
}