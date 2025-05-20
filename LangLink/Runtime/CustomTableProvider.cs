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
            if (LangLink.LoadedCustomLang == null || LangLink.LoadedCustomLang.Count == 0)
            {
                Debug.Log("<LangLink> Custom language dictionary is not loaded.");
                return default;
            }

            if (LangLink.LoadedCustomLang.TryGetValue(locale.LocaleName, out var customLangList))
            {
                var defaultLocale = LocalizationSettings.ProjectLocale;

                // Check if the table is a StringTable or AssetTable. if is AssetTable, return the default local table.
                if (typeof(TTable) == typeof(AssetTable))
                {
                    var operationHandle = LocalizationSettings.AssetDatabase.GetTableAsync(tableCollectionName, defaultLocale);
                    var castedHandle = (object)operationHandle;
                    return (AsyncOperationHandle<TTable>)castedHandle;
                }
                
                foreach (var customLang in customLangList)
                {
                    if (customLang.TableName != tableCollectionName || typeof(TTable) != typeof(StringTable))
                    {
                        continue;
                    }

                    if (LangLink.SharedTableCache.TryGetValue(customLang.TableName, out var sharedTableData))
                    {
                        Debug.Log(sharedTableData != null);
                        Debug.Log($"<LangLink> {locale.LocaleName} custom table {tableCollectionName} found");

                        var table = ScriptableObject.CreateInstance<StringTable>();
                        table.SharedData = sharedTableData;
                        table.LocaleIdentifier = locale.Identifier;
                        foreach (var kvp in customLang.Content)
                        {
                            table.AddEntry(kvp.Key, kvp.Value);
                        }

                        return Addressables.ResourceManager.CreateCompletedOperation(table as TTable, null);
                    }
                }

                // If the table is not found, return the default locale table.
                var missingTable = LocalizationSettings.StringDatabase.GetTable(tableCollectionName, defaultLocale);
                return Addressables.ResourceManager.CreateCompletedOperation(missingTable as TTable, null);
            }
            
            
            return default;
        }
    }
}