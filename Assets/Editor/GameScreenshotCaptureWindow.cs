using System;
using System.IO;
using UnityEditor;
using UnityEngine;

/// <summary>
/// 提供只在 Unity Editor 使用的 Game View 截圖工具，避免將 Editor 工具列納入上架素材。
/// </summary>
public sealed class GameScreenshotCaptureWindow : EditorWindow
{
    private const int TargetWidth = 1920;
    private const int TargetHeight = 1080;
    private const string DefaultOutputDirectory = "Assets/CapturedScreenshots";
    private const string DefaultFilePrefix = "GameScreenshot";
    private const string OutputDirectoryPreferenceKey = "BabyCareGame.GameScreenshotCapture.OutputDirectory";

    // 輸出路徑保留在 EditorPrefs，讓連續擷取多張素材時不必反覆選擇相同資料夾。
    private string outputDirectory;

    // 檔名前綴只影響素材辨識，不讓使用者輸入的內容直接形成危險或無效的檔名。
    private string filePrefix = DefaultFilePrefix;

    // 透過延遲一幀等待 Game View 取得焦點，確保 ScreenCapture 擷取的是遊戲畫面而不是工具視窗。
    private string pendingCapturePath;
    private bool captureScheduled;

    /// <summary>
    /// 開啟 BabyCareGame 的 Game View 截圖工具視窗。
    /// </summary>
    /// <returns>此方法不回傳值。</returns>
    [MenuItem("BabyCareGame/Tools/Game Screenshot Capture")]
    public static void OpenWindow()
    {
        GetWindow<GameScreenshotCaptureWindow>("Game Screenshot Capture");
    }

    /// <summary>
    /// 載入上次使用的輸出資料夾，並持續重繪視窗以更新 Play Mode 狀態。
    /// </summary>
    private void OnEnable()
    {
        outputDirectory = EditorPrefs.GetString(OutputDirectoryPreferenceKey, DefaultOutputDirectory);
        EditorApplication.update += Repaint;
    }

    /// <summary>
    /// 解除 Editor 更新事件並取消尚未執行的延遲擷取，避免視窗關閉後仍存取已釋放的狀態。
    /// </summary>
    private void OnDisable()
    {
        SavePreferences();
        EditorApplication.update -= Repaint;
        EditorApplication.delayCall -= CaptureFocusedGameView;
        pendingCapturePath = null;
        captureScheduled = false;
    }

    /// <summary>
    /// 繪製輸出路徑、檔名前綴與擷取操作介面。
    /// </summary>
    private void OnGUI()
    {
        EditorGUILayout.LabelField("輸出資料夾", EditorStyles.boldLabel);

        EditorGUILayout.BeginHorizontal();
        outputDirectory = EditorGUILayout.TextField(outputDirectory);
        if (GUILayout.Button("選擇", GUILayout.Width(60f)))
        {
            ChooseOutputDirectory();
        }

        EditorGUILayout.EndHorizontal();

        filePrefix = EditorGUILayout.TextField("檔名前綴", filePrefix);

        EditorGUILayout.Space(8f);
        EditorGUILayout.LabelField("固定輸出解析度", $"{TargetWidth} × {TargetHeight}");
        EditorGUILayout.HelpBox(
            "請先在 Game View 選擇 1920 × 1080，並關閉 Low Resolution Aspect Ratios。工具會自動切回 Game View 後擷取。",
            MessageType.Info);

        using (new EditorGUI.DisabledScope(!EditorApplication.isPlaying || captureScheduled))
        {
            if (GUILayout.Button("擷取 Game View PNG", GUILayout.Height(32f)))
            {
                RequestScreenshotCapture();
            }
        }

        if (!EditorApplication.isPlaying)
        {
            EditorGUILayout.HelpBox("請先進入 Play Mode，才能擷取遊戲畫面。", MessageType.Warning);
        }
    }

    /// <summary>
    /// 開啟資料夾選擇器，並保存使用者選定的輸出路徑。
    /// </summary>
    /// <returns>此方法不回傳值。</returns>
    private void ChooseOutputDirectory()
    {
        string selectedDirectory = EditorUtility.OpenFolderPanel(
            "選擇遊戲截圖輸出資料夾",
            GetAbsoluteOutputDirectory(),
            string.Empty);

        if (string.IsNullOrEmpty(selectedDirectory))
        {
            return;
        }

        outputDirectory = selectedDirectory;
        SavePreferences();
    }

    /// <summary>
    /// 檢查執行狀態、建立輸出路徑，並安排 Game View 取得焦點後再擷取。
    /// </summary>
    /// <returns>此方法不回傳值。</returns>
    private void RequestScreenshotCapture()
    {
        if (!EditorApplication.isPlaying || captureScheduled)
        {
            return;
        }

        string absoluteOutputDirectory;
        try
        {
            absoluteOutputDirectory = GetAbsoluteOutputDirectory();
            if (string.IsNullOrEmpty(absoluteOutputDirectory))
            {
                throw new InvalidOperationException("輸出資料夾不可為空白。請先指定有效資料夾。");
            }

            Directory.CreateDirectory(absoluteOutputDirectory);
            pendingCapturePath = BuildScreenshotPath(absoluteOutputDirectory);
        }
        catch (Exception exception)
        {
            ShowError("建立截圖輸出路徑失敗", exception.Message);
            return;
        }

        if (!FocusGameView())
        {
            pendingCapturePath = null;
            ShowError("無法取得 Game View", "Unity Editor 找不到 Game View，請先開啟 Window > General > Game。");
            return;
        }

        // ScreenCapture 必須在 Game View 成為目前視窗後執行，因此不能在按鈕事件同一幀立即擷取。
        captureScheduled = true;
        EditorApplication.delayCall += CaptureFocusedGameView;
    }

    /// <summary>
    /// 在 Game View 取得焦點後驗證解析度並輸出 PNG 截圖。
    /// </summary>
    /// <returns>此方法不回傳值。</returns>
    private void CaptureFocusedGameView()
    {
        EditorApplication.delayCall -= CaptureFocusedGameView;
        captureScheduled = false;

        string capturePath = pendingCapturePath;
        pendingCapturePath = null;

        if (!EditorApplication.isPlaying || string.IsNullOrEmpty(capturePath))
        {
            return;
        }

        if (Screen.width != TargetWidth || Screen.height != TargetHeight)
        {
            ShowError(
                "Game View 解析度不符合要求",
                $"目前擷取尺寸為 {Screen.width} × {Screen.height}，請將 Game View 設為 {TargetWidth} × {TargetHeight} 後重試。");
            return;
        }

        ScreenCapture.CaptureScreenshot(capturePath, 1);
        Debug.Log($"[GameScreenshotCapture] 已送出 {TargetWidth} × {TargetHeight} 截圖：{capturePath}");
        ShowNotification(new GUIContent($"已儲存截圖：{Path.GetFileName(capturePath)}"));
    }

    /// <summary>
    /// 將 Game View 置於目前視窗，讓 Unity 的 ScreenCapture API 擷取正確畫面。
    /// </summary>
    /// <returns>找到並聚焦 Game View 時回傳 true，否則回傳 false。</returns>
    private static bool FocusGameView()
    {
        // GameView 在 Unity 2022.3 沒有公開的強型別 API；只在 Editor 工具內透過既有內部型別聚焦視窗。
        Type gameViewType = typeof(EditorWindow).Assembly.GetType("UnityEditor.GameView");
        if (gameViewType == null)
        {
            return false;
        }

        EditorWindow gameView = EditorWindow.GetWindow(gameViewType, false, "Game", true);
        if (gameView == null)
        {
            return false;
        }

        gameView.Focus();
        return true;
    }

    /// <summary>
    /// 將輸出路徑轉為可供檔案 API 使用的絕對路徑。
    /// </summary>
    /// <returns>回傳截圖輸出資料夾的絕對路徑，型別為 string。</returns>
    private string GetAbsoluteOutputDirectory()
    {
        string path = outputDirectory == null ? string.Empty : outputDirectory.Trim();
        if (string.IsNullOrEmpty(path))
        {
            return string.Empty;
        }

        if (!Path.IsPathRooted(path))
        {
            path = Path.Combine(GetProjectRootPath(), path);
        }

        return Path.GetFullPath(path);
    }

    /// <summary>
    /// 取得目前 Unity 專案根目錄，供 Assets 相對路徑轉換使用。
    /// </summary>
    /// <returns>回傳 Unity 專案根目錄的絕對路徑，型別為 string。</returns>
    private static string GetProjectRootPath()
    {
        return Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
    }

    /// <summary>
    /// 建立不覆蓋既有檔案的 PNG 檔案路徑。
    /// </summary>
    /// <param name="directoryPath">輸出資料夾絕對路徑，型別為 string。</param>
    /// <returns>回傳新的截圖檔案絕對路徑，型別為 string。</returns>
    private string BuildScreenshotPath(string directoryPath)
    {
        string safePrefix = SanitizeFileName(filePrefix);
        string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss_fff");
        string path = Path.Combine(directoryPath, $"{safePrefix}_{timestamp}.png");
        int duplicateIndex = 1;

        while (File.Exists(path))
        {
            path = Path.Combine(directoryPath, $"{safePrefix}_{timestamp}_{duplicateIndex}.png");
            duplicateIndex++;
        }

        return path;
    }

    /// <summary>
    /// 移除檔名前綴中的無效字元，避免輸出路徑因使用者輸入而失敗。
    /// </summary>
    /// <param name="value">使用者輸入的檔名前綴，型別為 string。</param>
    /// <returns>回傳可用於檔案名稱的字串，型別為 string。</returns>
    private static string SanitizeFileName(string value)
    {
        string sanitizedValue = string.IsNullOrWhiteSpace(value) ? DefaultFilePrefix : value.Trim();
        foreach (char invalidCharacter in Path.GetInvalidFileNameChars())
        {
            sanitizedValue = sanitizedValue.Replace(invalidCharacter, '_');
        }

        return string.IsNullOrWhiteSpace(sanitizedValue) ? DefaultFilePrefix : sanitizedValue;
    }

    /// <summary>
    /// 保存視窗設定，讓下次開啟工具時延續使用者的輸出位置。
    /// </summary>
    /// <returns>此方法不回傳值。</returns>
    private void SavePreferences()
    {
        if (!string.IsNullOrWhiteSpace(outputDirectory))
        {
            EditorPrefs.SetString(OutputDirectoryPreferenceKey, outputDirectory.Trim());
        }
    }

    /// <summary>
    /// 顯示統一格式的 Editor 錯誤訊息，避免失敗時只留下難以追查的例外堆疊。
    /// </summary>
    /// <param name="title">錯誤視窗標題，型別為 string。</param>
    /// <param name="message">錯誤原因與處理方式，型別為 string。</param>
    /// <returns>此方法不回傳值。</returns>
    private static void ShowError(string title, string message)
    {
        Debug.LogError($"[GameScreenshotCapture] {title}：{message}");
        EditorUtility.DisplayDialog(title, message, "確定");
    }
}
