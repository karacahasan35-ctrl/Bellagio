using UnityEngine;
using UnityEngine.UI;

public class GameUIManager : MonoBehaviour
{
    public static GameUIManager Instance { get; private set; }

    private Canvas canvas;
    private Text goldText;
    private Text starText;
    private Text progressText;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        CreateDynamicUI();
        
        if (TaskManager.Instance != null)
        {
            TaskManager.Instance.OnStateChanged += UpdateHUD;
        }

        UpdateHUD();
        
        // Grid ve restorasyon hedeflerini her zaman görünür kıl
        if (GridManager.Instance != null)
        {
            GridManager.Instance.SetGridVisibility(true);
        }
    }

    private void OnDestroy()
    {
        if (TaskManager.Instance != null)
        {
            TaskManager.Instance.OnStateChanged -= UpdateHUD;
        }
    }

    private void CreateDynamicUI()
    {
        // EventSystem oluştur (Tıklamaların algılanması için)
        CreateEventSystem();

        // 1. Canvas oluştur
        GameObject canvasObj = new GameObject("UICanvas");
        canvasObj.transform.SetParent(transform);
        canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasObj.AddComponent<CanvasScaler>();
        canvasObj.AddComponent<GraphicRaycaster>();

        // 2. Üst HUD Paneli
        GameObject hudPanelObj = new GameObject("HUDPanel");
        hudPanelObj.transform.SetParent(canvasObj.transform);
        RectTransform hudRt = hudPanelObj.AddComponent<RectTransform>();
        hudRt.anchorMin = new Vector2(0, 1);
        hudRt.anchorMax = new Vector2(1, 1);
        hudRt.pivot = new Vector2(0.5f, 1);
        hudRt.anchoredPosition = new Vector2(0, 0);
        hudRt.sizeDelta = new Vector2(0, 80);
        
        Image hudBg = hudPanelObj.AddComponent<Image>();
        hudBg.color = new Color(0.08f, 0.08f, 0.1f, 0.9f); // Premium koyu metalik renk

        // HUD Yazıları (Altın, Yıldız, İlerleme)
        goldText = CreateText(hudPanelObj, "GoldText", "ALTIN: 500", new Vector2(-160, 0), Color.yellow, 20);
        starText = CreateText(hudPanelObj, "StarText", "YILDIZ: 0", new Vector2(0, 0), new Color(0.2f, 0.8f, 1f), 20);
        progressText = CreateText(hudPanelObj, "ProgressText", "İLERLEME: %0", new Vector2(160, 0), Color.green, 20);
    }

    private void CreateEventSystem()
    {
        if (Object.FindAnyObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
        {
            GameObject esObj = new GameObject("EventSystem");
            esObj.AddComponent<UnityEngine.EventSystems.EventSystem>();
            
            System.Type newUIModule = System.Type.GetType("UnityEngine.InputSystem.UI.InputSystemUIInputModule, Unity.InputSystem");
            if (newUIModule != null)
            {
                esObj.AddComponent(newUIModule);
            }
            else
            {
                esObj.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
            }
        }
    }

    private Text CreateText(GameObject parent, string name, string content, Vector2 pos, Color color, int fontSize)
    {
        GameObject textObj = new GameObject(name);
        textObj.transform.SetParent(parent.transform);
        
        RectTransform rt = textObj.AddComponent<RectTransform>();
        rt.anchoredPosition = pos;
        rt.sizeDelta = new Vector2(380, 50);

        Text txt = textObj.AddComponent<Text>();
        txt.text = content;
        txt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        txt.color = color;
        txt.fontSize = fontSize;
        txt.alignment = TextAnchor.MiddleCenter;
        return txt;
    }

    private void UpdateHUD()
    {
        if (TaskManager.Instance == null || goldText == null) return;

        goldText.text = $"ALTIN: {TaskManager.Instance.currentGold}";
        starText.text = $"YILDIZ: {TaskManager.Instance.currentStars}";
        
        float progress = TaskManager.Instance.GetRenovationProgress();
        progressText.text = $"İLERLEME: %{Mathf.RoundToInt(progress * 100)}";
    }
}
