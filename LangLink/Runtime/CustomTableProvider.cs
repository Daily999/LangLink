using System;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.Localization.Tables;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Studio.Daily.LangLink
{
    [Serializable]
    public class CustomTableProvider : ITableProvider
    {
        public AsyncOperationHandle<TTable> ProvideTableAsync<TTable>(string tableCollectionName, Locale locale) where TTable : LocalizationTable
        {
            Debug.Log($"Requested {locale.LocaleName} {typeof(TTable).Name} with the name <{tableCollectionName}>.");

            if (LangLink.LoadedCustomLang == null || LangLink.LoadedCustomLang.Count == 0)
            {
                Debug.LogWarning("Custom language dictionary is not loaded.");
                return default;
            }

            var defaultLocale = LocalizationSettings.AvailableLocales.Locales[0];
            if (LangLink.LoadedCustomLang.TryGetValue(locale.LocaleName, out var customLangList))
            {
                foreach (var customLang in customLangList)
                {
                    if (customLang.TableName != tableCollectionName || typeof(TTable) != typeof(StringTable))
                    {
                        continue;
                    }

                    var t = LocalizationSettings.StringDatabase.GetTable(tableCollectionName, defaultLocale);
                    var table = ScriptableObject.CreateInstance<StringTable>();
                    table.SharedData = t.SharedData;
                    table.LocaleIdentifier = locale.Identifier;
                    foreach (var kvp in customLang.Content)
                    {
                        table.AddEntry(kvp.Key, kvp.Value);
                    }

                    return Addressables.ResourceManager.CreateCompletedOperation(table as TTable, null);
                }
            }
            return default;
        }
    }
}