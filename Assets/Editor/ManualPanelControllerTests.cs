using NUnit.Framework;
using UI;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 驗證說明書控制器的面板切換、翻頁與頁面邊界行為。
/// </summary>
public sealed class ManualPanelControllerTests
{
    private GameObject root;
    private GameObject settingPanel;
    private GameObject manualPanel;
    private GameObject previousButton;
    private GameObject nextButton;
    private Image pageImage;
    private ManualPanelController controller;
    private Texture2D firstTexture;
    private Texture2D secondTexture;
    private Sprite firstPage;
    private Sprite secondPage;

    /// <summary>
    /// 為每個測試建立獨立的暫時 UI，避免修改目前開啟的場景或 Prefab。
    /// </summary>
    [SetUp]
    public void SetUp()
    {
        root = new GameObject("ManualPanelControllerTests");
        settingPanel = CreateChild("SettingPanel");
        manualPanel = CreateChild("ManualPanel");
        previousButton = CreateChild("PreviousButton");
        nextButton = CreateChild("NextButton");

        GameObject imageObject = new GameObject(
            "PageImage",
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(Image));
        imageObject.transform.SetParent(root.transform);
        pageImage = imageObject.GetComponent<Image>();

        controller = root.AddComponent<ManualPanelController>();
        AssignReference("settingPanel", settingPanel);
        AssignReference("manualPanel", manualPanel);
        AssignReference("pageImage", pageImage);
        AssignReference("previousButton", previousButton);
        AssignReference("nextButton", nextButton);

        // 使用不同 Sprite 實例，才能確定翻頁真的替換了顯示內容。
        firstTexture = new Texture2D(1, 1);
        secondTexture = new Texture2D(1, 1);
        firstPage = Sprite.Create(firstTexture, new Rect(0, 0, 1, 1), Vector2.zero);
        secondPage = Sprite.Create(secondTexture, new Rect(0, 0, 1, 1), Vector2.zero);
    }

    /// <summary>
    /// 清除測試建立的 Unity 物件，避免污染後續測試與目前編輯器狀態。
    /// </summary>
    [TearDown]
    public void TearDown()
    {
        Object.DestroyImmediate(root);
        Object.DestroyImmediate(firstPage);
        Object.DestroyImmediate(secondPage);
        Object.DestroyImmediate(firstTexture);
        Object.DestroyImmediate(secondTexture);
    }

    /// <summary>
    /// 驗證多頁說明書會切換面板、重設第一頁、保護頁面邊界並正確返回。
    /// </summary>
    [Test]
    public void OpenAndNavigate_UpdatesPanelsPageAndBoundaryButtons()
    {
        AssignPages(firstPage, secondPage);

        controller.OpenManual();

        Assert.That(settingPanel.activeSelf, Is.False);
        Assert.That(manualPanel.activeSelf, Is.True);
        Assert.That(pageImage.sprite, Is.SameAs(firstPage));
        Assert.That(previousButton.activeSelf, Is.False);
        Assert.That(nextButton.activeSelf, Is.True);

        controller.ShowNextPage();

        Assert.That(pageImage.sprite, Is.SameAs(secondPage));
        Assert.That(previousButton.activeSelf, Is.True);
        Assert.That(nextButton.activeSelf, Is.False);

        // 最後一頁再次前進必須保持原頁，避免陣列索引越界。
        controller.ShowNextPage();
        Assert.That(pageImage.sprite, Is.SameAs(secondPage));

        controller.CloseManual();
        controller.OpenManual();

        // 每次重新開啟都必須回到第一頁，避免沿用上一次閱讀進度。
        Assert.That(pageImage.sprite, Is.SameAs(firstPage));
        Assert.That(previousButton.activeSelf, Is.False);
        Assert.That(nextButton.activeSelf, Is.True);

        controller.ShowPreviousPage();
        Assert.That(pageImage.sprite, Is.SameAs(firstPage));

        controller.CloseManual();

        Assert.That(settingPanel.activeSelf, Is.True);
        Assert.That(manualPanel.activeSelf, Is.False);
    }

    /// <summary>
    /// 驗證尚未從 Inspector 指派圖片時仍可安全開啟空白說明介面。
    /// </summary>
    [Test]
    public void OpenWithoutPages_ClearsImageAndHidesNavigationButtons()
    {
        AssignPages();
        pageImage.sprite = firstPage;

        controller.OpenManual();

        Assert.That(pageImage.sprite, Is.Null);
        Assert.That(previousButton.activeSelf, Is.False);
        Assert.That(nextButton.activeSelf, Is.False);
    }

    /// <summary>
    /// 建立掛在測試根物件下的暫時子物件。
    /// </summary>
    /// <param name="name">子物件名稱。</param>
    /// <returns>建立完成的子物件。</returns>
    private GameObject CreateChild(string name)
    {
        GameObject child = new GameObject(name);
        child.transform.SetParent(root.transform);
        return child;
    }

    /// <summary>
    /// 將 UI 參考寫入控制器的私有序列化欄位。
    /// </summary>
    /// <param name="propertyName">欄位名稱。</param>
    /// <param name="value">Inspector 等效參考值。</param>
    private void AssignReference(string propertyName, Object value)
    {
        SerializedObject serializedController = new SerializedObject(controller);
        SerializedProperty property = serializedController.FindProperty(propertyName);
        Assert.That(property, Is.Not.Null, $"找不到 {propertyName} 序列化欄位。");
        property.objectReferenceValue = value;
        serializedController.ApplyModifiedPropertiesWithoutUndo();
    }

    /// <summary>
    /// 依 Inspector 陣列格式設定說明書圖片順序。
    /// </summary>
    /// <param name="pages">依顯示順序排列的頁面圖片。</param>
    private void AssignPages(params Sprite[] pages)
    {
        SerializedObject serializedController = new SerializedObject(controller);
        SerializedProperty pageArray = serializedController.FindProperty("manualPages");
        Assert.That(pageArray, Is.Not.Null, "找不到 manualPages 序列化欄位。");
        pageArray.arraySize = pages.Length;

        for (int index = 0; index < pages.Length; index++)
        {
            // 直接寫入 SerializedProperty 可測到實際 Inspector 使用的欄位，而不是測試專用 API。
            pageArray.GetArrayElementAtIndex(index).objectReferenceValue = pages[index];
        }

        serializedController.ApplyModifiedPropertiesWithoutUndo();
    }
}
