# 💬 LangLink — Unity Community Translation Solution
[![繁體中文](https://img.shields.io/badge/README-繁體中文-red?style=flat-square)](./ReadMe_zh.md)

LangLink is a localization import plugin designed for Unity that allows importing custom-formatted translation data at runtime.  
Based on Unity's Localization package, LangLink provides convenient external language data import features, making it easy for anyone to integrate translation data into their game.

## ✨ Key Features

✅ Supports dynamic loading of custom-formatted translation data (e.g., TSV, CSV, JSON… defined by the developer)  
🔁 Registers and switches language resources instantly at runtime  
📦 Fully integrates with Unity Localization package system  
🔌 Modular design for easy extension and maintenance  
🌍 Allows players or community members to contribute language data, enhancing game internationalization  
Whether you are an indie developer or a project team that needs multi-language support, LangLink provides an extensible and low-coupling language data management solution.

## 🚀 Quick Start

### Easy Setup

The fastest way to start. Keep all default settings, and LangLink will automatically load all translation data under the `streamingAssets/LangLink` folder.

~~~csharp
LangLink.SetupLangLink();
~~~

### Translation Data

- Place translation files into the loading directory (default is `StreamingAssets/LangLink`)
- Default file format is TSV; you can specify other formats via `LangLink.TargetFileFormat = "*.tsv"`
- Filename format: `<locale>_<tableName>`:
    - `locale` is the language name, which will appear in Unity’s language list
    - `tableName` corresponds to the table name in Unity Localization
    - You can customize the filename format via the `IFileNameParser` interface

### Custom Translation Data Format

LangLink supports custom translation data formats. Developers can define their own format by implementing the `ITableTxtToDictionary` interface and registering it in `LangLink`.

~~~csharp
public class MyTableTxtToDictionary : ITableTxtToDictionary
{
    public Dictionary<string, string> Convert(string filePath)
    {
        // Read the file and convert it to a dictionary
        // ...
        return new Dictionary<string, string>();
    }
}
public class Test 
{
    void Start()
    {
        // Register custom format translation parser
        LangLink.TableParser= new MyTableTxtToDictionary();
        // Setup LangLink
        LangLink.SetupLangLink();
    }
}
~~~

## 🔧 LangLink Core API

LangLink provides several core APIs for managing language data. Here are some commonly used ones:

- `LangLink.SetupLangLink()` — Start LangLink with default settings
- `LangLink.SetupLangLinkAsync()` — Start LangLink asynchronously

### Advanced Usage

If you want to control the startup process of LangLink yourself, you can use the following APIs:

- `LangLink.LoadCustomLocalization()` — Load language data
- `LangLink.LoadCustomLocalization(string path)` — Load language data from a specified path
- `LangLink.LoadCustomLocalizationAsync()` — Load language data asynchronously
- `LangLink.LoadCustomLocalizationAsync(string path)` — Load language data asynchronously from a specified path
- `LangLink.CreateCustomLocalization(string fileName, string tableTxt)` — Create custom language data
- `AssignTableProvider()` — Register a table provider to Unity Localization

## 🛠️ Installation

### Using Unity Package Manager (UPM)

~~~json
"studio.daily.langlink" : "https://github.com/Daily999/langlink.git"
~~~

✅ Dependencies  
LangLink depends on the Unity Localization package. Please make sure your project has it installed:

Unity Localization (recommended version: 1.5.0 or above)  
If not installed, you can install it via Package Manager:

Open Unity → Window > Package Manager  
Search for "Localization"  
Click Install

## UniTask Support

LangLink supports UniTask for asynchronous operations, providing more efficient async handling.  
Make sure UniTask is installed in your project, and import the UniTask namespace where needed.

LangLink internally uses define symbols to switch call methods,  
and the define symbols are controlled by assembly definition files.

~~~csharp
#if LANGLINK_SUPPORT_UNITASK
    // Use UniTask
   public static async UniTask<Dictionary<string, string>> LoadCustomLocalizationAsync()
#else
    // Standard method
    public static async Task<Dictionary<string, string>> LoadCustomLocalizationAsync()
#endif
~~~

The method signatures are the same, but the return types differ.

# Community Translation Implementation Suggestions

## Filename
You can load multiple versions of the same language. Use version tags in the filename to distinguish them, for example:  
`繁體中文 <Daily漢化組>_UI`, `繁體中文 <GoogleMachineTranslation>_UI`.  
This way, different versions will be shown as separate languages in the game.

## Preserve Community Translator Info
Since community translations come from various contributors, developers can reserve a localized key to keep contributor information visible.

## Culture Info
Unity Localization supports `CultureInfo` language culture codes. Some games use this info to display specific data (like currency or date formats).  
LangLink will try to convert keys in the table columns to `CultureInfo` language culture codes and pass them to Unity Localization.  
However, this conversion is risky because LangLink cannot guarantee all keys convert correctly.  
To ensure successful conversion, translators may add hints inside () brackets.  
