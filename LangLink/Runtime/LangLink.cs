using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
#if !LANGLINK_SUPPORT_UNITASK
using System.Threading.Tasks;
#endif

namespace Studio.Daily.LangLink
{
    public static partial class LangLink
    {
        public static string TargetFileFormat { get; set; } = "*.csv";
        public static string DefaultLoadPath { get; set; } = $"{Application.streamingAssetsPath}/LangLink";
        public static IFileNameParser FileNameParser { get; set; } = new DefaultFileNameParser();
        public static ITableTxtToDictionary TableParser { get; set; } = new CsvToDictionary();

        public static Dictionary<string, List<CustomLang>> LoadedCustomLang { get; private set; }

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

            Application.quitting -= UnAssignTableProvider;
            Application.quitting += UnAssignTableProvider;
            AssignTableProvider();
        }

        public static void AssignTableProvider()
        {
            var provider = new CustomTableProvider();
            var settings = LocalizationSettings.Instance;
            settings.GetStringDatabase().TableProvider = provider;
        }
        public static void UnAssignTableProvider()
        {
            var settings = LocalizationSettings.Instance;
            settings.GetStringDatabase().TableProvider = null;
        }
        
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
                return new CultureInfo(customLang.LocaleCode);
            }

            var cultureInfo = currentLocale.Identifier.CultureInfo;
            return cultureInfo;
        }

#if !LANGLINK_SUPPORT_UNITASK
        public static async void SetupLangLinkAsync()
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
            Application.quitting -= UnAssignTableProvider;
            Application.quitting += UnAssignTableProvider;
            AssignTableProvider();
        }
        public static async Task<Dictionary<string, string>> LoadCustomLocalizationAsync()
            => await LoadCustomLocalizationAsync(DefaultLoadPath);
        public static async Task<Dictionary<string, string>> LoadCustomLocalizationAsync(string loadPath)
        {
            if (!Directory.Exists(loadPath))
            {
                Debug.LogWarning($"<LangLink> Directory does not exist: {loadPath}");
                return null;
            }

            var files = Directory.GetFiles(loadPath, TargetFileFormat, SearchOption.AllDirectories);
            var readTasks = new List<Task<(string FileName, string Content)>>();
            foreach (var filePath in files)
            {
                if (!File.Exists(filePath))
                    continue;

                readTasks.Add(Task.Run(async () =>
                {
                    var fileName = Path.GetFileNameWithoutExtension(filePath);
                    var content = await File.ReadAllTextAsync(filePath);
                    return (fileName, content);
                }));
            }

            var results = await Task.WhenAll(readTasks);

            var output = new Dictionary<string, string>();
            foreach (var result in results)
            {
                output[result.FileName] = result.Content;
            }

            return output;
        }
#endif

        public static Dictionary<string, string> LoadCustomLocalization()
            => LoadCustomLocalization(DefaultLoadPath);
        public static Dictionary<string, string> LoadCustomLocalization(string loadPath)
        {
            if (!Directory.Exists(loadPath))
            {
                Debug.LogWarning($"<LangLink> Directory does not exist: {loadPath}");
                return null;
            }

            var files = Directory.GetFiles(loadPath, TargetFileFormat, SearchOption.AllDirectories);
            var output = new Dictionary<string, string>();
            foreach (var filePath in files)
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

    }


}