using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.TextCore.LowLevel;

/// <summary>
/// 產生專案 UI 專用的 Open Huninn TextMeshPro Font Asset。
/// </summary>
public static class OpenHuninnUIFontAssetGenerator
{
    // 原始字型檔維持單一來源，避免日後重建 Font Asset 時誤用其他字體造成 UI 風格不一致。
    private const string SourceFontPath = "Assets/Font/Open Huninn Font 2.1.ttf";

    // 新字型資產獨立保存，不覆蓋既有資產，降低對現有場景與 Prefab 的影響範圍。
    private const string OutputAssetPath = "Assets/Font/Open Huninn Font 2.1 UI SDF.asset";

    // 這些參數取自既有 Open Huninn Font 2.1 SDF.asset，確保新資產的筆畫粗細與描邊取樣風格一致。
    private const int SamplingPointSize = 90;
    private const int AtlasPadding = 9;
    private const int AtlasWidth = 1024;
    private const int AtlasHeight = 1024;
    private const GlyphRenderMode RenderMode = (GlyphRenderMode)4165;
    private const AtlasPopulationMode PopulationMode = AtlasPopulationMode.Dynamic;

    // 字元清單包含目前 UI 文字、執行期會寫入 UI 的訊息，以及半形與全形 0-9，避免常見數字顯示依賴 fallback。
    private const string RequiredCharacters =
        "一下中人位保值健公分切初到剩力動化卡合名哈啟回圖姆始學家布庫康戲手換擊效教數新旋時束果機次步殊活測無牌特玩環生畫發的第算結試識轉辨遊重閉開間關電面風餘驟點！，２：0123456789０１２３４５６７８９";

    /// <summary>
    /// 從 Unity 選單手動產生 UI 專用 Font Asset。
    /// </summary>
    /// <returns>此選單方法不回傳值。</returns>
    [MenuItem("BabyCareGame/Font/Generate Open Huninn UI Font Asset")]
    public static void GenerateFromMenu()
    {
        // 選單入口與批次模式共用同一套流程，避免兩種產生方式輸出不同資產。
        Generate();
    }

    /// <summary>
    /// 供 Unity batchmode 使用的 Font Asset 產生入口。
    /// </summary>
    /// <returns>此批次入口不回傳值，失敗時會設定非零結束碼。</returns>
    public static void GenerateForBatchMode()
    {
        try
        {
            Generate();
            EditorApplication.Exit(0);
        }
        catch (Exception exception)
        {
            // 批次模式必須用明確結束碼讓外部工具知道產生失敗，避免拿到舊資產誤以為成功。
            Debug.LogError($"[OpenHuninnUIFontAssetGenerator] 產生 Font Asset 失敗：{exception}");
            EditorApplication.Exit(1);
        }
    }

    /// <summary>
    /// 建立並儲存 UI 專用 Open Huninn TMP Font Asset。
    /// </summary>
    /// <returns>此方法不回傳值；若缺字或資產建立失敗會丟出例外。</returns>
    private static void Generate()
    {
        Font sourceFont = AssetDatabase.LoadAssetAtPath<Font>(SourceFontPath);
        if (sourceFont == null)
        {
            // 原始字型不存在時不能退而使用其他字型，否則會破壞使用者要求的 Open Huninn 風格一致性。
            throw new FileNotFoundException($"找不到原始字型檔：{SourceFontPath}");
        }

        EnsureOutputDirectoryExists(OutputAssetPath);
        DeleteExistingAsset(OutputAssetPath);

        TMP_FontAsset fontAsset = TMP_FontAsset.CreateFontAsset(
            sourceFont,
            SamplingPointSize,
            AtlasPadding,
            RenderMode,
            AtlasWidth,
            AtlasHeight,
            PopulationMode);

        if (fontAsset == null)
        {
            // TMP 可能因字型匯入設定未包含字型資料而建立失敗，必須提早中止避免留下半成品。
            throw new InvalidOperationException("TMP_FontAsset.CreateFontAsset 回傳 null，請確認原始字型 Include Font Data 已啟用。");
        }

        fontAsset.name = Path.GetFileNameWithoutExtension(OutputAssetPath);
        fontAsset.atlasPopulationMode = PopulationMode;
        fontAsset.creationSettings = BuildCreationSettings(sourceFont);

        string normalizedCharacters = NormalizeCharacters(RequiredCharacters);
        if (!fontAsset.TryAddCharacters(normalizedCharacters, out string missingCharacters))
        {
            // 缺字代表原始 Open Huninn 無法提供全部 glyph，保留明確錯誤可避免 UI 上線後才發現方塊字。
            throw new InvalidOperationException($"Open Huninn 原始字型缺少以下 UI 字元：{missingCharacters}");
        }

        AssetDatabase.CreateAsset(fontAsset, OutputAssetPath);
        PersistGeneratedSubAssets(fontAsset);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"[OpenHuninnUIFontAssetGenerator] 已產生 {OutputAssetPath}，字元數：{normalizedCharacters.Length}");
    }

    /// <summary>
    /// 建立與既有 Open Huninn SDF 相符的 TMP 建立設定。
    /// </summary>
    /// <param name="sourceFont">來源 Open Huninn 字型，型別為 Font。</param>
    /// <returns>回傳 FontAssetCreationSettings，用於記錄 Font Asset Creator 參數。</returns>
    private static FontAssetCreationSettings BuildCreationSettings(Font sourceFont)
    {
        string sourceGuid = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(sourceFont));

        // Character Set Selection Mode 7 與既有 Font Asset 相同，代表以自訂字元清單建立。
        return new FontAssetCreationSettings
        {
            sourceFontFileGUID = sourceGuid,
            pointSizeSamplingMode = 0,
            pointSize = SamplingPointSize,
            padding = AtlasPadding,
            packingMode = 0,
            atlasWidth = AtlasWidth,
            atlasHeight = AtlasHeight,
            characterSetSelectionMode = 7,
            characterSequence = NormalizeCharacters(RequiredCharacters),
            referencedFontAssetGUID = string.Empty,
            referencedTextAssetGUID = string.Empty,
            fontStyle = 0,
            fontStyleModifier = 0,
            renderMode = (int)RenderMode,
            includeFontFeatures = false
        };
    }

    /// <summary>
    /// 移除重複字元並保持原始清單順序。
    /// </summary>
    /// <param name="characters">待整理的字元字串，型別為 string。</param>
    /// <returns>回傳去除重複後的字串，型別為 string。</returns>
    private static string NormalizeCharacters(string characters)
    {
        HashSet<char> seenCharacters = new HashSet<char>();
        List<char> orderedCharacters = new List<char>();

        foreach (char character in characters)
        {
            // 以第一次出現的順序保留字元，讓產生結果可預期且便於人工審查清單。
            if (seenCharacters.Add(character))
            {
                orderedCharacters.Add(character);
            }
        }

        return new string(orderedCharacters.ToArray());
    }

    /// <summary>
    /// 確保輸出資產所在目錄已存在。
    /// </summary>
    /// <param name="assetPath">Unity 專案內資產路徑，型別為 string。</param>
    /// <returns>此方法不回傳值。</returns>
    private static void EnsureOutputDirectoryExists(string assetPath)
    {
        string directoryPath = Path.GetDirectoryName(assetPath);
        if (string.IsNullOrEmpty(directoryPath) || Directory.Exists(directoryPath))
        {
            return;
        }

        // 使用系統目錄建立即可保留 Unity 對 Assets 內路徑的匯入流程，之後由 AssetDatabase.Refresh 同步。
        Directory.CreateDirectory(directoryPath);
    }

    /// <summary>
    /// 刪除舊版輸出資產，避免重複建立時 Unity 保留過期 atlas 或材質子資產。
    /// </summary>
    /// <param name="assetPath">Unity 專案內資產路徑，型別為 string。</param>
    /// <returns>此方法不回傳值。</returns>
    private static void DeleteExistingAsset(string assetPath)
    {
        if (AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(assetPath) == null)
        {
            return;
        }

        if (!AssetDatabase.DeleteAsset(assetPath))
        {
            throw new IOException($"無法刪除既有 Font Asset：{assetPath}");
        }
    }

    /// <summary>
    /// 將 TMP 自動建立的材質與 Atlas Texture 保存成 Font Asset 子資產。
    /// </summary>
    /// <param name="fontAsset">已建立的 TMP Font Asset，型別為 TMP_FontAsset。</param>
    /// <returns>此方法不回傳值。</returns>
    private static void PersistGeneratedSubAssets(TMP_FontAsset fontAsset)
    {
        if (fontAsset.material != null)
        {
            // 材質必須成為子資產，否則重新開啟專案時 Font Asset 可能失去 atlas material 參照。
            fontAsset.material.name = $"{fontAsset.name} Atlas Material";
            AssetDatabase.AddObjectToAsset(fontAsset.material, fontAsset);
        }

        foreach (Texture2D atlasTexture in fontAsset.atlasTextures.Where(texture => texture != null))
        {
            // Atlas Texture 也需持久化，才能讓 Unity 序列化已預先加入的 UI 字元圖集。
            atlasTexture.name = $"{fontAsset.name} Atlas";
            AssetDatabase.AddObjectToAsset(atlasTexture, fontAsset);
        }
    }
}
