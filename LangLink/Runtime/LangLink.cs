using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.Localization.Tables;
using UnityEngine.ResourceManagement.AsyncOperations;
using Object = UnityEngine.Object;
#if !LANGLINK_SUPPORT_UNITASK
using System.Threading.Tasks;
#endif

namespace Studio.Daily.LangLink
{
    public static partial class LangLink
    {
        public static event Action LangLinkChangeEvent;
        public static string TargetFileFormat { get; set; } = "*.csv";
        public static string DefaultLoadPath { get; set; } = $"{Application.streamingAssetsPath}/LangLink";
        public static IFileNameParser FileNameParser { get; set; } = new DefaultFileNameParser();
        public static ITableTxtToDictionary TableParser { get; set; } = new CsvToDictionary();

        public static Dictionary<string, SharedTableData> SharedTableCache { get; private set; } = new Dictionary<string, SharedTableData>();
        public static Dictionary<string, List<CustomLang>> LoadedCustomLang { get; private set; }

        public static CultureInfo GetCurrentCultureInfo()
        {
            var currentLocale = LocalizationSettings.SelectedLocale;
            if (currentLocale == null)
            {
                Debug.LogWarning("<LangLink> Current locale is null.");
                return CultureInfo.InvariantCulture;
            }
            if (LoadedCustomLang.TryGetValue(currentLocale.LocaleName, out var customLangList))
            {
                var customLang = customLangList[0];
                Debug.Log(customLang.LocaleCode);
                return new CultureInfo(customLang.LocaleCode);
            }

            var cultureInfo = currentLocale.Identifier.CultureInfo;
            return cultureInfo;
        }

        public static void ReloadLangLink()
        {
            ReleaseLangLink();
            SetupLangLink();
        }

        public static void ReleaseLangLink()
        {
            LocalizationSettings.SelectedLocale = LocalizationSettings.ProjectLocale;
            foreach (var lang in LoadedCustomLang)
            {
                LocalizationSettings.AvailableLocales.RemoveLocale(lang.Value[0].Locale);
            }
            LoadedCustomLang.Clear();
            SharedTableCache.Clear();
            UnAssignTableProvider();
            Application.quitting -= UnAssignTableProvider;
        }

        public static void SetupLangLink()
        {
            LoadedCustomLang = new Dictionary<string, List<CustomLang>>();
            var files = LoadCustomLocalization();
            if (files == null) return;
            foreach (var file in files)
            {
                var customLang = CreateCustomLocalization(file.Key, file.Value);
                if (customLang == null)
                {
                    continue;
                }
                var newLocal = customLang.Locale;
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
                SetSharedTableCache();
                Application.quitting -= UnAssignTableProvider;
                Application.quitting += UnAssignTableProvider;
                AssignTableProvider();
            }
        }

        public static void AssignTableProvider()
        {
            var provider = new CustomTableProvider();
            var settings = LocalizationSettings.Instance;
            settings.GetStringDatabase().TableProvider = provider;
            settings.GetAssetDatabase().TableProvider = provider;
        }
        public static void UnAssignTableProvider()
        {
            var settings = LocalizationSettings.Instance;
            settings.GetStringDatabase().TableProvider = null;
            settings.GetAssetDatabase().TableProvider = null;
        }



        public static Dictionary<string, string> LoadCustomLocalization() => LoadCustomLocalization(DefaultLoadPath);
        public static Dictionary<string, string> LoadCustomLocalization(string loadPath)
        {
            if (!Directory.Exists(loadPath))
            {
                Debug.LogWarning($"<LangLink> Directory does not exist: {loadPath}");
                return null;
            }

            var filePaths = Directory.GetFiles(loadPath, TargetFileFormat, SearchOption.AllDirectories);
            var output = new Dictionary<string, string>();
            foreach (var filePath in filePaths)
            {
                if (!File.Exists(filePath))
                {
                    continue;
                }
                var fileName = Path.GetFileNameWithoutExtension(filePath);
                var fileContent = File.ReadAllText(filePath);
                output.Add(fileName, fileContent);
            }
            return output;
        }
        public static CustomLang CreateCustomLocalization(string fileName, string tableTxt)
        {
            var name = FileNameParser.ParseFileName(fileName);
            if (string.IsNullOrEmpty(name.localeName) || string.IsNullOrEmpty(name.tableName))
            {
                Debug.LogError("<LangLink> FileName is parse error. Please check the file name.");
                return null;
            }

            var customLangContent = TableParser.ParseTableTxt(tableTxt);

            Locale newLocal;
            if (customLangContent.TryGetValue("Key", out var key))
            {
                newLocal = TryGetCultureCode(key, out var cultureCode) ? Locale.CreateLocale(cultureCode) : Locale.CreateLocale(name.localeName);
            }
            else
            {
                newLocal = TryGetCultureCode(name.localeName, out var cultureCode) ? Locale.CreateLocale(cultureCode) : Locale.CreateLocale(name.localeName);
            }

            newLocal.LocaleName = name.localeName;

            var newCustomLang = new CustomLang(
                newLocal,
                name.tableName,
                customLangContent
            );

            return newCustomLang;
        }

        public static void SetSharedTableCache()
        {
            var settings = LocalizationSettings.Instance;
            var defaultLocale = LocalizationSettings.ProjectLocale;

            var tables = settings.GetStringDatabase().GetAllTables(defaultLocale);
            tables.WaitForCompletion();
            if (tables.Status != AsyncOperationStatus.Succeeded)
            {
                Debug.LogError($"<LangLink> Failed to get all tables: {tables.Status}");
                return;
            }
            for (int i = 0; i < tables.Result.Count; i++)
            {
                var table = tables.Result[i];

                var sharedTableData = Object.Instantiate(table.SharedData);
                if (sharedTableData != null)
                {
                    SharedTableCache.TryAdd(table.TableCollectionName, sharedTableData);
                }
            }
        }

        private static bool TryGetCultureCode(string localeName, out CultureInfo cultureCode)
        {
            cultureCode = null;
            try
            {
                cultureCode = new CultureInfo(localeName);
                return true;
            }
            catch (CultureNotFoundException) { }

            foreach (var culture in CultureInfo.GetCultures(CultureTypes.AllCultures))
            {
                if (string.Equals(culture.EnglishName, localeName, StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(culture.NativeName, localeName, StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(culture.DisplayName, localeName, StringComparison.OrdinalIgnoreCase))
                {
                    cultureCode = culture;
                    return true;
                }
            }
            return false;
        }

#if !LANGLINK_SUPPORT_UNITASK
        public static async Task SetupLangLinkAsync()
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
        public static async Task<Dictionary<string, string>> LoadCustomLocalizationAsync() => await LoadCustomLocalizationAsync(DefaultLoadPath);
        public static async Task<Dictionary<string, string>> LoadCustomLocalizationAsync(string loadPath)
        {
            if (!Directory.Exists(loadPath))
            {
                Debug.LogWarning($"<LangLink> Directory does not exist: {loadPath}");
                return null;
            }

            var files = Directory.GetFiles(loadPath, TargetFileFormat, SearchOption.AllDirectories);
            var loadTasks = new List<Task<(string FileName, string Content)>>();
            foreach (var filePath in files)
            {
                if (!File.Exists(filePath))
                    continue;

                loadTasks.Add(Task.Run(async () =>
                {
                    var fileName = Path.GetFileNameWithoutExtension(filePath);
                    var content = await File.ReadAllTextAsync(filePath);
                    return (fileName, content);
                }));
            }

            var results = await Task.WhenAll(loadTasks);

            var output = new Dictionary<string, string>();
            foreach (var result in results)
            {
                output[result.FileName] = result.Content;
            }

            return output;
        }
        public static async Task SetSharedTableCacheAsync()
        {
            var settings = LocalizationSettings.Instance;
            var defaultLocale = LocalizationSettings.ProjectLocale;

            var handle = settings.GetStringDatabase().GetAllTables(defaultLocale);
            await handle.Task;

            if (handle.Status != AsyncOperationStatus.Succeeded)
            {
                Debug.LogError($"<LangLink> Failed to get all tables: {handle.Status}");
                return;
            }
            for (int i = 0; i < handle.Result.Count; i++)
            {
                var table = handle.Result[i];

                var sharedTableData  = Object.Instantiate(table.SharedData);
                if (sharedTableData != null)
                {
                    SharedTableCache.TryAdd(table.TableCollectionName, sharedTableData);
                }

            }
        }
#endif
    }


}