using UnityEngine;
using UnityEngine.UI;

public class GameUIManager : MonoBehaviour
{
    public static GameUIManager Instance { get; private set; }

    private Canvas canvas;
    private Text goldText;
    private Text starText;
    private Text progressText;
    private Text taskDescText;
    private Text taskReqText;
    private Button submitButton;
    private Button toggleViewButton;
    private Text toggleButtonText;

    private bool isMergeBoardView = false; // Varsayılan olarak Avlu (Restorasyon) ekranından başlasın

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
        SetViewMode(isMergeBoardView); // Başlangıç modunu ayarla
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
        // EventSystem oluştur (Tıklamaların algılanması için zorunlu!)
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
        hudBg.color = new Color(0.08f, 0.08f, 0.1f, 0.9f); // Koyu premium renk

        // HUD Yazıları (Altın, Yıldız, İlerleme)
        goldText = CreateText(hudPanelObj, "GoldText", $"ALTIN: 500", new Vector2(-160, 0), Color.yellow, 20);
        starText = CreateText(hudPanelObj, "StarText", $"YILDIZ: 0", new Vector2(0, 0), new Color(0.2f, 0.8f, 1f), 20);
        progressText = CreateText(hudPanelObj, "ProgressText", $"İLERLEME: %0", new Vector2(160, 0), Color.green, 20);

        // 3. Görev Paneli (Sol Üst)
        GameObject taskPanelObj = new GameObject("TaskPanel");
        taskPanelObj.transform.SetParent(canvasObj.transform);
        RectTransform taskRt = taskPanelObj.AddComponent<RectTransform>();
        taskRt.anchorMin = new Vector2(0.5f, 0.5f);
        taskRt.anchorMax = new Vector2(0.5f, 0.5f);
        taskRt.pivot = new Vector2(0.5f, 0.5f);
        // Telefon ekranının üst yarısında dursun
        taskRt.anchoredPosition = new Vector2(0, 180);
        taskRt.sizeDelta = new Vector2(400, 160);

        Image taskBg = taskPanelObj.AddComponent<Image>();
        taskBg.color = new Color(0.12f, 0.12f, 0.16f, 0.85f);
        
        // Görev Detayları
        taskDescText = CreateText(taskPanelObj, "TaskDesc", "Aktif Görev: Yükleniyor...", new Vector2(0, 45), Color.white, 16);
        taskDescText.alignment = TextAnchor.MiddleCenter;
        taskReqText = CreateText(taskPanelObj, "TaskReq", "Gereken: Lvl 1 Taş", new Vector2(0, 10), new Color(0.8f, 0.8f, 0.8f), 14);
        taskReqText.alignment = TextAnchor.MiddleCenter;

        // Görevi Teslim Et Butonu
        GameObject btnObj = new GameObject("SubmitButton");
        btnObj.transform.SetParent(taskPanelObj.transform);
        RectTransform btnRt = btnObj.AddComponent<RectTransform>();
        btnRt.anchoredPosition = new Vector2(0, -40);
        btnRt.sizeDelta = new Vector2(250, 40);

        Image btnImg = btnObj.AddComponent<Image>();
        btnImg.color = new Color(0.15f, 0.6f, 0.3f, 1f); // Yeşil buton
        
        submitButton = btnObj.AddComponent<Button>();
        Text btnText = CreateText(btnObj, "BtnText", "GÖREVİ TAMAMLA", Vector2.zero, Color.white, 14);
        btnText.alignment = TextAnchor.MiddleCenter;
        submitButton.onClick.AddListener(OnSubmitTaskClicked);

        // 4. Ekran Değiştirme Butonu (Alt Kısım)
        GameObject toggleObj = new GameObject("ToggleViewButton");
        toggleObj.transform.SetParent(canvasObj.transform);
        RectTransform toggleRt = toggleObj.AddComponent<RectTransform>();
        toggleRt.anchorMin = new Vector2(0.5f, 0);
        toggleRt.anchorMax = new Vector2(0.5f, 0);
        toggleRt.pivot = new Vector2(0.5f, 0);
        toggleRt.anchoredPosition = new Vector2(0, 50);
        toggleRt.sizeDelta = new Vector2(300, 60);

        Image toggleImg = toggleObj.AddComponent<Image>();
        toggleImg.color = new Color(0.2f, 0.4f, 0.8f, 1f); // Mavi buton

        toggleViewButton = toggleObj.AddComponent<Button>();
        toggleButtonText = CreateText(toggleObj, "ToggleText", "MERGE ALANINA GİT", Vector2.zero, Color.white, 16);
        toggleButtonText.alignment = TextAnchor.MiddleCenter;
        toggleViewButton.onClick.AddListener(OnToggleViewClicked);
    }

    private void CreateEventSystem()
    {
        if (Object.FindAnyObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
        {
            GameObject esObj = new GameObject("EventSystem");
            esObj.AddComponent<UnityEngine.EventSystems.EventSystem>();
            
            // Yeni Input System için UI modülünü eklemeyi dene, yoksa StandaloneInputModule ekle
            System.Type newUIModule = System.Type.GetType("UnityEngine.InputSystem.UI.InputSystemUIInputModule, Unity.InputSystem");
            if (newUIModule != null)
            {
                esObj.AddComponent(newUIModule);
                Debug.Log("[GameUIManager] EventSystem created with InputSystemUIInputModule.");
            }
            else
            {
                esObj.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
                Debug.Log("[GameUIManager] EventSystem created with StandaloneInputModule.");
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
        if (TaskManager.Instance == null) return;

        goldText.text = $"ALTIN: {TaskManager.Instance.currentGold}";
        starText.text = $"YILDIZ: {TaskManager.Instance.currentStars}";
        
        float progress = TaskManager.Instance.GetRenovationProgress();
        progressText.text = $"İLERLEME: %{Mathf.RoundToInt(progress * 100)}";

        // Görev yazısını güncelle
        RenovationTask activeTask = TaskManager.Instance.GetNextActiveTask();
        if (activeTask != null)
        {
            taskDescText.text = activeTask.description;
            taskReqText.text = $"Gereken: Seviye {activeTask.requiredLevel} {activeTask.requiredChainName}";
            submitButton.gameObject.SetActive(true);
        }
        else
        {
            taskDescText.text = "Tebrikler! Villayı Tamamen Yenilediniz!";
            taskReqText.text = "Tüm görevler tamamlandı.";
            submitButton.gameObject.SetActive(false);
        }
    }

    private void OnToggleViewClicked()
    {
        isMergeBoardView = !isMergeBoardView;
        SetViewMode(isMergeBoardView);
    }

    private void SetViewMode(bool isMerge)
    {
        isMergeBoardView = isMerge;

        // Grid'i ve hücreleri gizle/göster
        if (GridManager.Instance != null)
        {
            GridManager.Instance.SetGridVisibility(isMerge);
        }

        // Buton yazılarını güncelle
        if (isMerge)
        {
            toggleButtonText.text = "VİLLA AVLUSUNA DÖN";
            // Merge ekranında görev paneli daha küçük ve üstte durur
            RectTransform rt = taskDescText.transform.parent.GetComponent<RectTransform>();
            rt.anchoredPosition = new Vector2(0, 320); // Üste taşı
        }
        else
        {
            toggleButtonText.text = "MERGE ALANINA GİT (OYNA)";
            RectTransform rt = taskDescText.transform.parent.GetComponent<RectTransform>();
            rt.anchoredPosition = new Vector2(0, 180); // Avlunun ortasında durur
        }
    }

    private void OnSubmitTaskClicked()
    {
        if (TaskManager.Instance == null || GridManager.Instance == null) return;

        RenovationTask activeTask = TaskManager.Instance.GetNextActiveTask();
        if (activeTask == null) return;

        // Grid üzerindeki seçili nesneyi kontrol et
        MergeItem selected = GridManager.Instance.selectedItem;
        if (selected == null)
        {
            Debug.LogWarning("[GameUIManager] Görevi tamamlamak için önce merge alanından gereksinimleri karşılayan bir eşyaya tıklayıp seçmelisin!");
            return;
        }

        bool success = TaskManager.Instance.TryCompleteTask(activeTask, selected);
        if (success)
        {
            GridManager.Instance.selectedItem = null;
            UpdateHUD();
        }
        else
        {
            Debug.LogWarning("[GameUIManager] Seçili eşya görev gereksinimini karşılamıyor!");
        }
    }
}
