using UnityEngine;
using UnityEngine.UI;

public class GameUIManager : MonoBehaviour
{
    public static GameUIManager Instance { get; private set; }

    private Canvas canvas;
    private Text goldText;
    private Text starText;
    private Text progressText;
    private GameObject startMenuPanel;

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
        
        // Grid ve hedefleri varsayılan olarak görünür kıl
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

        // Soru İşareti Yardım Butonu (HUD'un sağ köşesine eklenir)
        GameObject helpBtnObj = new GameObject("HelpButton");
        helpBtnObj.transform.SetParent(hudPanelObj.transform);
        RectTransform helpRt = helpBtnObj.AddComponent<RectTransform>();
        helpRt.anchorMin = new Vector2(1, 0.5f);
        helpRt.anchorMax = new Vector2(1, 0.5f);
        helpRt.pivot = new Vector2(1, 0.5f);
        helpRt.anchoredPosition = new Vector2(-20, 0);
        helpRt.sizeDelta = new Vector2(40, 40);

        Image helpImg = helpBtnObj.AddComponent<Image>();
        helpImg.color = new Color(0.2f, 0.2f, 0.25f, 1f); // Koyu gri yuvarlak buton arka planı
        
        Button helpButton = helpBtnObj.AddComponent<Button>();
        Text helpText = CreateText(helpBtnObj, "HelpText", "?", Vector2.zero, Color.white, 20);
        helpText.alignment = TextAnchor.MiddleCenter;
        
        helpButton.onClick.AddListener(ShowStartMenu);

        // 3. Giriş ve Tutorial Ekranı (Start Menu Overlay)
        startMenuPanel = new GameObject("StartMenuPanel");
        startMenuPanel.transform.SetParent(canvasObj.transform);
        RectTransform menuRt = startMenuPanel.AddComponent<RectTransform>();
        menuRt.anchorMin = Vector2.zero;
        menuRt.anchorMax = Vector2.one;
        menuRt.offsetMin = Vector2.zero;
        menuRt.offsetMax = Vector2.zero;

        Image menuBg = startMenuPanel.AddComponent<Image>();
        menuBg.color = new Color(0.06f, 0.06f, 0.08f, 0.98f); // Koyu premium arka plan

        // Logo / Başlık
        CreateText(startMenuPanel, "GameTitle", "BELLAGIO", new Vector2(0, 240), new Color(0.9f, 0.75f, 0.15f, 1f), 36);
        CreateText(startMenuPanel, "GameSubtitle", "Teknik Restorasyon Simülatörü", new Vector2(0, 195), new Color(0.7f, 0.7f, 0.7f, 1f), 16);

        // Kutu Arka Planı (Tutorial Box)
        GameObject boxObj = new GameObject("TutorialBox");
        boxObj.transform.SetParent(startMenuPanel.transform);
        RectTransform boxRt = boxObj.AddComponent<RectTransform>();
        boxRt.anchoredPosition = new Vector2(0, 10);
        boxRt.sizeDelta = new Vector2(420, 260);
        Image boxImg = boxObj.AddComponent<Image>();
        boxImg.color = new Color(0.12f, 0.12f, 0.16f, 0.9f);

        // Kutu Kenarlığı
        GameObject borderObj = new GameObject("Border");
        borderObj.transform.SetParent(boxObj.transform);
        RectTransform borderRt = borderObj.AddComponent<RectTransform>();
        borderRt.anchorMin = Vector2.zero;
        borderRt.anchorMax = Vector2.one;
        borderRt.offsetMin = new Vector2(2, 2);
        borderRt.offsetMax = new Vector2(-2, -2);
        Image borderImg = borderObj.AddComponent<Image>();
        borderImg.color = new Color(0.2f, 0.2f, 0.25f, 1f);

        // İç Dolgu
        GameObject contentBgObj = new GameObject("ContentBg");
        contentBgObj.transform.SetParent(borderObj.transform);
        RectTransform contentBgRt = contentBgObj.AddComponent<RectTransform>();
        contentBgRt.anchorMin = Vector2.zero;
        contentBgRt.anchorMax = Vector2.one;
        contentBgRt.offsetMin = new Vector2(2, 2);
        contentBgRt.offsetMax = new Vector2(-2, -2);
        Image contentBgImg = contentBgObj.AddComponent<Image>();
        contentBgImg.color = new Color(0.12f, 0.12f, 0.16f, 1f);

        // Nasıl Oynanır Başlığı
        CreateText(contentBgObj, "TutHeader", "NASIL OYNANIR?", new Vector2(0, 100), new Color(0.9f, 0.75f, 0.15f, 1f), 18);

        // Adımlar
        Text step1Text = CreateText(contentBgObj, "Step1", "👜 1. Alet Çantasına tıklayarak Fırça ve Kireç Harcı üretin.", new Vector2(0, 50), Color.white, 14);
        step1Text.alignment = TextAnchor.MiddleLeft;
        RectTransform s1Rt = step1Text.GetComponent<RectTransform>();
        s1Rt.sizeDelta = new Vector2(360, 40);

        Text step2Text = CreateText(contentBgObj, "Step2", "🔄 Aynı seviyedeki alet/malzemeleri birleştirip geliştirin.", new Vector2(0, 0), Color.white, 14);
        step2Text.alignment = TextAnchor.MiddleLeft;
        RectTransform s2Rt = step2Text.GetComponent<RectTransform>();
        s2Rt.sizeDelta = new Vector2(360, 40);

        Text step3Text = CreateText(contentBgObj, "Step3", "📍 Malzemeyi üstteki Restorasyon Çemberlerine bırakarak villayı onarın!", new Vector2(0, -50), Color.white, 14);
        step3Text.alignment = TextAnchor.MiddleLeft;
        RectTransform s3Rt = step3Text.GetComponent<RectTransform>();
        s3Rt.sizeDelta = new Vector2(360, 40);

        // Başla Butonu
        GameObject playBtnObj = new GameObject("PlayButton");
        playBtnObj.transform.SetParent(startMenuPanel.transform);
        RectTransform playRt = playBtnObj.AddComponent<RectTransform>();
        playRt.anchoredPosition = new Vector2(0, -180);
        playRt.sizeDelta = new Vector2(280, 55);

        Image playImg = playBtnObj.AddComponent<Image>();
        playImg.color = new Color(0.15f, 0.6f, 0.3f, 1f); // Yeşil buton

        Button playButton = playBtnObj.AddComponent<Button>();
        Text playText = CreateText(playBtnObj, "PlayText", "RESTORASYONA BAŞLA", Vector2.zero, Color.white, 16);
        playText.alignment = TextAnchor.MiddleCenter;
        
        playButton.onClick.AddListener(HideStartMenu);
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

    public void ShowStartMenu()
    {
        if (startMenuPanel != null)
        {
            startMenuPanel.SetActive(true);
        }
    }

    public void HideStartMenu()
    {
        if (startMenuPanel != null)
        {
            startMenuPanel.SetActive(false);
        }
    }
}
