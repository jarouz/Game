using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Builds a simple canvas with text placeholders for player stats and inventory.
/// </summary>
public class PlayerStatsUI : MonoBehaviour
{
    [SerializeField] private PlayerStats playerStats;
    [SerializeField] private Canvas targetCanvas;
    [SerializeField] private Font uiFont;

    private Text statsText;
    private Text inventoryText;

    private void Start()
    {
        if (playerStats == null)
        {
            playerStats = FindObjectOfType<PlayerStats>();
        }

        if (playerStats == null)
        {
            Debug.LogError("PlayerStatsUI requires a PlayerStats reference in the scene.");
            enabled = false;
            return;
        }

        EnsureCanvas();
        CreateTextIfNeeded();
        playerStats.StatsChanged += RefreshUI;
        RefreshUI();
    }

    private void OnDestroy()
    {
        if (playerStats != null)
        {
            playerStats.StatsChanged -= RefreshUI;
        }
    }

    /// <summary>
    /// Refreshes both UI text panels using the latest stats and inventory data.
    /// </summary>
    public void RefreshUI()
    {
        if (statsText == null || inventoryText == null)
        {
            return;
        }

        statsText.text = "Player Stats\n" + playerStats.BuildStatsSummary();
        inventoryText.text = "Inventory\n" + playerStats.BuildInventorySummary();
    }

    private void EnsureCanvas()
    {
        if (targetCanvas != null)
        {
            return;
        }

        GameObject canvasObject = new GameObject("PlayerStatsCanvas");
        canvasObject.transform.SetParent(transform, false);

        targetCanvas = canvasObject.AddComponent<Canvas>();
        targetCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasObject.AddComponent<CanvasScaler>();
        canvasObject.AddComponent<GraphicRaycaster>();
    }

    private void CreateTextIfNeeded()
    {
        if (statsText == null)
        {
            statsText = CreateTextElement("StatsText", new Vector2(20f, -20f), TextAnchor.UpperLeft);
        }

        if (inventoryText == null)
        {
            inventoryText = CreateTextElement("InventoryText", new Vector2(-20f, -20f), TextAnchor.UpperRight);
            RectTransform inventoryRect = inventoryText.rectTransform;
            inventoryRect.anchorMin = new Vector2(1f, 1f);
            inventoryRect.anchorMax = new Vector2(1f, 1f);
            inventoryRect.pivot = new Vector2(1f, 1f);
        }
    }

    private Text CreateTextElement(string objectName, Vector2 anchoredPosition, TextAnchor alignment)
    {
        GameObject textObject = new GameObject(objectName);
        textObject.transform.SetParent(targetCanvas.transform, false);

        Text text = textObject.AddComponent<Text>();
        text.font = uiFont != null ? uiFont : Resources.GetBuiltinResource<Font>("Arial.ttf");
        text.fontSize = 18;
        text.alignment = alignment;
        text.horizontalOverflow = HorizontalWrapMode.Overflow;
        text.verticalOverflow = VerticalWrapMode.Overflow;
        text.color = Color.white;

        RectTransform rectTransform = text.rectTransform;
        rectTransform.sizeDelta = new Vector2(320f, 500f);
        rectTransform.anchorMin = new Vector2(0f, 1f);
        rectTransform.anchorMax = new Vector2(0f, 1f);
        rectTransform.pivot = new Vector2(0f, 1f);
        rectTransform.anchoredPosition = anchoredPosition;

        return text;
    }
}
