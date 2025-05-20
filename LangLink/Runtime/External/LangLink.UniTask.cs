#if LANGLINK_SUPPORT_UNITASK
using System.Collections.Generic;
using System.IO;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Studio.Daily.LangLink
{
    public static partial class LangLink
    {
        public static async UniTask SetupLangLinkAsync()
        {
            LoadedCustomLang = new Dictionary<string, List<CustomLang>>();
            var files = await LoadCustomLocalizationAsync();
            if (files == null) return;
            foreach (var file in files)
            {
                var customLang = CreateCustomLocalization(file.Key, file.Value);
                if (customLang == null)
                {
                    continue;
                }
                var newLocal = customLang.Locale;
                customLang.LocaleCode = newLocal.Identifier.Code;
                newLocal.Identifier = new LocaleIdentifier(newLocal.LocaleName);
                LocalizationSettings.AvailableLocales.AddLocale(newLocal);

                if (LoadedCustomLang.TryGetValue(newLocal.LocaleName, out var customLangList))
                {
                    customLangList.Add(customLang);
                }
                else
                {
                    LoadedCustomLang.Add(newLocal.LocaleName, new List<CustomLang> { customLang });
                }
            }
            if (LoadedCustomLang.Count > 0)
            {
                await SetSharedTableCacheAsync();
                Application.quitting -= UnAssignTableProvider;
                Application.quitting += UnAssignTableProvider;
                AssignTableProvider();
            }
        }
        public static async UniTask<Dictionary<string, string>> LoadCustomLocalizationAsync() => await LoadCustomLocalizationAsync(DefaultLoadPath);
        public static async UniTask<Dictionary<string, string>> LoadCustomLocalizationAsync(string loadPath)
        {
            if (!Directory.Exists(loadPath))
            {
                Debug.LogWarning($"<LangLink> Directory does not exist: {loadPath}");
                return null;
            }

            var files = Directory.GetFiles(loadPath, TargetFileFormat, SearchOption.AllDirectories);
            var readTasks = new List<UniTask<(string FileName, string Content)>>();
            foreach (var filePath in files)
            {
                if (!File.Exists(filePath))
                    continue;
                readTasks.Add(LoadFile(filePath));
            }

            var results = await UniTask.WhenAll(readTasks);

            var output = new Dictionary<string, string>();
            foreach (var result in results)
            {
                output[result.FileName] = result.Content;
            }

            return output;

            static async UniTask<(string FileName, string Content)> LoadFile(string filePath)
            {
                var fileName = Path.GetFileNameWithoutExtension(filePath);
                var content = await File.ReadAllTextAsync(filePath);
                return (fileName, content);
            }
        }
        public static async UniTask SetSharedTableCacheAsync()
        {
            var settings = LocalizationSettings.Instance;
            var defaultLocale = LocalizationSettings.ProjectLocale;

            var handle = settings.GetStringDatabase().GetAllTables(defaultLocale);
            await handle;

            if (handle.Status != AsyncOperationStatus.Succeeded)
            {
                Debug.LogError($"<LangLink> Failed to get all tables: {handle.Status}");
                return;
            }
            for (int i = 0; i < handle.Result.Count; i++)
            {
                var table = handle.Result[i];
                if (table.SharedData == null)
                {
                    Debug.Log($"<LangLink> Table {table.TableCollectionName} does not have shared data.");
                    continue;
                }
                var sharedTableData  = Object.Instantiate(table.SharedData);
                if (sharedTableData != null)
                {
                    SharedTableCache.TryAdd(table.TableCollectionName, sharedTableData);
                }
            }
            Debug.Log($"<LangLink> SharedTableCache count: {SharedTableCache.Count}");
        }
    }
}
#endif