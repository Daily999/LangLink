using System.Collections.Generic;
namespace Studio.Daily.LangLink
{
    public interface ITableTxtToDictionary
    {
        Dictionary<string, string> ParseTableTxt(string tableTxt);
    }
}