using System;
using UnityEngine;
namespace Studio.Daily.LangLink
{
    public class DefaultFileNameParser : IFileNameParser
    {
        public (string localeName, string tableName) ParseFileName(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
            {
                Debug.LogWarning("<LangLink> fileName is null or empty");
                return (string.Empty, string.Empty);
            }
            var parts = fileName.Split(new[] { '_' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length < 2)
            {
                Debug.LogWarning($"<LangLink> fileName format is incorrect : {fileName} should be <locale>_<tableName>");
                return (string.Empty, string.Empty);
            }

            var localeName = parts[0].Trim();
            var tableName = parts[1].Trim();

            if (string.IsNullOrEmpty(localeName) || string.IsNullOrEmpty(tableName))
            {
                Debug.LogWarning($"<LangLink> fileName format is incorrect : {fileName} should be <locale>_<tableName>");
                return (string.Empty, string.Empty);
            }
            return (localeName, tableName);
        }
    }
}