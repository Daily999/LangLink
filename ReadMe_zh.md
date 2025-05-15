# 💬 LangLink — Unity社群翻譯解決方案

LangLink 是一款為 Unity 打造的本地化語言導入插件，可以在運行時導入自訂格式的翻譯資料。  
LangLink基於 Unity 的 Localization 套件，並提供了方便的外部語言資料導入功能，讓任何人能夠輕鬆地將翻譯資料整合到遊戲中。

## ✨ 主要特點

✅ 支援動態載入自訂格式的翻譯資料（如 TSV、CSV、JSON…由開發者定義）  
🔁 執行階段即時註冊並切換語言資源  
📦 完整整合 Unity Localization 套件系統  
🔌 模組化設計，易於擴充與維護  
🌍 允許玩家或社群貢獻語系資料，提升遊戲國際化深度

## 🚀 快速開始

### 簡易啟動

最快速的啟動方式，一切保持預設值，LangLink 會自動載入 `streamingAssets/LangLink` 資料夾下的所有翻譯資料。

~~~csharp
LangLink.SetupLangLink();
~~~

### 翻譯資料

- 將翻譯檔案放入載入目錄（預設為 `StreamingAssets/LangLink`）
- 預設檔案格式為 TSV，可透過 `LangLink.TargetFileFormat = "*.tsv"` 指定其他格式
- 檔名格式為 `<locale>_<tableName>`：
    - `locale` 是語言名稱，會顯示在 Unity 的語言清單中
    - `tableName` 則對應 Unity Localization 中的表格名稱
    - 可以透過 `IFileNameParser` 介面來自訂檔名格式

### 自訂翻譯資料格式

LangLink 支援自訂翻譯資料格式，開發者可以根據需求定義資料格式。只需要實作 `ITableTxtToDictionary` 介面，並在 `LangLink`  
中註冊即可。

~~~csharp
public class MyTableTxtToDictionary : ITableTxtToDictionary
{
    public Dictionary<string, string> Convert(string filePath)
    {
        // 讀取檔案並轉換為字典
        // ...
        return new Dictionary<string, string>();
    }
}
public class Test 
{
    void Start()
    {
        // 註冊自訂格式的翻譯資料
        LangLink.TableParser= new MyTableTxtToDictionary();
        // 設定 LangLink
        LangLink.SetupLangLink();
    }
}
~~~

## 🔧 LangLink 核心 API

LangLink 提供了一些核心 API 來操作語言資料，以下是一些常用的 API：

- `LangLink.SetupLangLink()`：以預設的狀態啟動LangLink。
- `LangLink.SetupLangLinkAsync()`：以非同步的方式啟動LangLink。

### 進階使用

如果你想自己控制 LangLink 的啟動流程，可以使用以下 API：

- `LangLink.LoadCustomLocalization()`： 載入語言資料。
- `LangLink.LoadCustomLocalization(string path)`： 載入指定路徑的語言資料。
- `LangLink.LoadCustomLocalizationAsync()`：非同步載入語言資料。
- `LangLink.LoadCustomLocalizationAsync(string path)`：非同步載入指定路徑的語言資料。
- `LangLink.CreateCustomLocalization(string fileName, string tableTxt)`： 創建自訂的語言資料。
- `AssignTableProvider()`： 向 Unity Localization 註冊表格提供者。

## 🛠️ 安裝方式

### 使用 Unity Package Manager (UPM)

~~~json
"studio.daily.langlink" : "https://github.com/Daily999/langlink.git"
~~~

✅ 依賴套件需求  
LangLink 依賴 Unity Localization 套件。請確認專案已安裝：

Unity Localization（建議版本：1.5.0 以上）  
若尚未安裝，可透過 Package Manager 安裝：

開啟 Unity → Window > Package Manager  
搜尋 "Localization"  
點選安裝

## UniTask 支援

LangLink 支援 UniTask來進行非同步操作，可以提供更高效能的非同步操作。  
請確保在專案中已安裝 UniTask 套件，然後在需要使用非同步操作的地方導入 UniTask 命名空間。

LangLink插件內部使用define符號來切換呼叫的方法，  
define符號 由assembly definition檔案來控制。

~~~csharp
#if LANGLINK_SUPPORT_UNITASK
    // 使用 UniTask
   public static async UniTask<Dictionary<string, string>> LoadCustomLocalizationAsync()
#else
    // 一般方法
    public static async Task<Dictionary<string, string>> LoadCustomLocalizationAsync()
#endif
~~~

兩者方法簽名相同，但回傳型別不同。

# 社群翻譯實作建議

一些實作建議

## 提供翻譯表格

開發者需要向社群提供一個翻譯表格，讓社群能夠在上面進行翻譯。如果表格中有敏感訊息（可能透漏遊戲劇情或彩蛋）可以擬定相關的審核機制。

## 注意管理

社群翻譯的內容是由社群貢獻的，開發者需要注意管理這些翻譯內容，避免出現不當或不正確的翻譯。

## 檔案名稱

載入相同語言的多個版本是可以的，可以在檔名上加上版本來區分，例如：`繁體中文<Daily漢化組>_UI`、`繁體中文<Google機翻>_UI`。  
這樣在遊戲中的語言就會以個別的名稱呈現。

## 保留社群翻譯資訊

社群翻譯來自各方貢獻，為了保留翻譯者的資訊，開發者可以留下一個社群翻譯資訊的語言化Key來讓翻譯者留下自己的資訊。

## 翻譯長度

翻譯的長度可能會影響遊戲的UI，出現UI錯位或重疊的情況在所難免。翻譯者可以盡量控制翻譯的長度，或是利用`Rich Text`來控制格式（如果有開啟這項功能）。
例如 強制換行<br> 調整大小<size=20>

## 語言文化符

Unity Localization支援`CultureInfo`的語言文化符號，某些遊戲可能會使用此資訊來呈現一些資訊（如幣值數字或日期格式），  
LangLink會嘗試將Key值表格的欄位轉換成`CultureInfo`的語言文化符號，並將其傳遞給Unity Localization。  
但這個轉換是有風險的，因為LangLink無法保證所有的Key值都能正確轉換成`CultureInfo`的語言文化符號。  
為確保成功轉換，翻譯提供者可以在[這裡](./CultureCode.md)簡查  

# License
🥳 MIT License. and try support me.
