using UnityEngine;
using UnityEngine.UI;

public class GameUIManager : MonoBehaviour
{
    public static GameUIManager Instance { get; private set; }

    public static string selectedCharacter = ""; // "Hasan" or "Hazal"
    
    private Canvas canvas;
    private Text goldText;
    private Text starText;
    private Text progressText;
    private GameObject tooltipPanelObj;
    private Text tooltipTextComponent;
    
    // Start Menu / Character Select UI
    private GameObject startMenuPanel;
    private Image avatarPreviewImage;
    private Text transcriptText;
    private Button startRestorationButton;
    private Image leylaCardBg;
    private Image canCardBg;

    // HUD / Gameplay UI
    private GameObject marketPanel;
    private Image gameAvatarImage;
    private Text gameSpeechText;
    private GameObject speechBubbleObj;
    private Text marketStatusText;

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
        CreateEventSystem();

        // 1. Canvas oluştur
        GameObject canvasObj = new GameObject("UICanvas");
        canvasObj.transform.SetParent(transform);
        canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080, 1920);
        scaler.matchWidthOrHeight = 0.5f;
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
        hudBg.color = new Color(0.08f, 0.08f, 0.1f, 0.95f); // Premium metalik koyu renk

        // HUD Sol Panel (Altın, Yıldız, İlerleme)
        GameObject leftHud = new GameObject("LeftHUD");
        leftHud.transform.SetParent(hudPanelObj.transform);
        RectTransform leftRt = leftHud.AddComponent<RectTransform>();
        leftRt.anchorMin = new Vector2(0, 0);
        leftRt.anchorMax = new Vector2(0.7f, 1);
        leftRt.offsetMin = Vector2.zero;
        leftRt.offsetMax = Vector2.zero;

        HorizontalLayoutGroup leftLayout = leftHud.AddComponent<HorizontalLayoutGroup>();
        leftLayout.padding = new RectOffset(15, 0, 0, 0);
        leftLayout.spacing = 15;
        leftLayout.childAlignment = TextAnchor.MiddleLeft;
        leftLayout.childControlWidth = false;
        leftLayout.childControlHeight = false;
        leftLayout.childForceExpandWidth = false;
        leftLayout.childForceExpandHeight = false;

        // HUD Yazıları (Altın ve Yıldız Simgeleri ile ve İlerleme)
        CreateHUDIconGroup(leftHud, "GoldGroup", "GoldIcon", out goldText, "Gold");
        CreateHUDIconGroup(leftHud, "StarGroup", "StarIcon", out starText, "Star");

        // Progress Group
        GameObject progressGroup = new GameObject("ProgressGroup");
        progressGroup.transform.SetParent(leftHud.transform);
        RectTransform progressRt = progressGroup.AddComponent<RectTransform>();
        progressRt.sizeDelta = new Vector2(110, 40);

        progressText = CreateText(progressGroup, "ProgressText", "İLERLEME: %0", Vector2.zero, Color.green, 16);
        progressText.alignment = TextAnchor.MiddleLeft;
        RectTransform progressTextRt = progressText.GetComponent<RectTransform>();
        progressTextRt.anchorMin = Vector2.zero;
        progressTextRt.anchorMax = Vector2.one;
        progressTextRt.offsetMin = Vector2.zero;
        progressTextRt.offsetMax = Vector2.zero;

        // Tooltip Paneli (Canvas üzerinde)
        tooltipPanelObj = new GameObject("TooltipPanel");
        tooltipPanelObj.transform.SetParent(canvasObj.transform);
        RectTransform tooltipRt = tooltipPanelObj.AddComponent<RectTransform>();
        tooltipRt.anchorMin = new Vector2(0, 0);
        tooltipRt.anchorMax = new Vector2(0, 0);
        tooltipRt.pivot = new Vector2(0.5f, 1f);
        tooltipRt.sizeDelta = new Vector2(80, 26);
        
        Image tooltipBg = tooltipPanelObj.AddComponent<Image>();
        tooltipBg.color = new Color(0f, 0f, 0f, 0.85f);
        
        GameObject tooltipTxtObj = new GameObject("TooltipText");
        tooltipTxtObj.transform.SetParent(tooltipPanelObj.transform);
        RectTransform tTxtRt = tooltipTxtObj.AddComponent<RectTransform>();
        tTxtRt.anchorMin = Vector2.zero;
        tTxtRt.anchorMax = Vector2.one;
        tTxtRt.offsetMin = Vector2.zero;
        tTxtRt.offsetMax = Vector2.zero;
        
        tooltipTextComponent = tooltipTxtObj.AddComponent<Text>();
        tooltipTextComponent.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        tooltipTextComponent.fontSize = 13;
        tooltipTextComponent.color = Color.white;
        tooltipTextComponent.alignment = TextAnchor.MiddleCenter;
        
        tooltipPanelObj.SetActive(false);

        // HUD Sağ Panel (Market ve Yardım Butonları)
        GameObject rightHud = new GameObject("RightHUD");
        rightHud.transform.SetParent(hudPanelObj.transform);
        RectTransform rightRt = rightHud.AddComponent<RectTransform>();
        rightRt.anchorMin = new Vector2(0.7f, 0);
        rightRt.anchorMax = new Vector2(1, 1);
        rightRt.offsetMin = Vector2.zero;
        rightRt.offsetMax = Vector2.zero;

        HorizontalLayoutGroup rightLayout = rightHud.AddComponent<HorizontalLayoutGroup>();
        rightLayout.padding = new RectOffset(0, 15, 0, 0);
        rightLayout.spacing = 10;
        rightLayout.childAlignment = TextAnchor.MiddleRight;
        rightLayout.childControlWidth = false;
        rightLayout.childControlHeight = false;
        rightLayout.childForceExpandWidth = false;
        rightLayout.childForceExpandHeight = false;

        // MARKET Butonu
        GameObject marketBtnObj = new GameObject("MarketButton");
        marketBtnObj.transform.SetParent(rightHud.transform);
        RectTransform marketRt = marketBtnObj.AddComponent<RectTransform>();
        marketRt.sizeDelta = new Vector2(90, 40);

        Image marketImg = marketBtnObj.AddComponent<Image>();
        marketImg.color = new Color(0.15f, 0.6f, 0.3f, 1f); // Yeşil buton
        
        Button marketButton = marketBtnObj.AddComponent<Button>();
        Text marketBtnText = CreateText(marketBtnObj, "MarketBtnText", "MARKET", Vector2.zero, Color.white, 14);
        marketBtnText.alignment = TextAnchor.MiddleCenter;
        RectTransform mTxtRt = marketBtnText.GetComponent<RectTransform>();
        mTxtRt.anchorMin = Vector2.zero;
        mTxtRt.anchorMax = Vector2.one;
        mTxtRt.offsetMin = Vector2.zero;
        mTxtRt.offsetMax = Vector2.zero;
        marketButton.onClick.AddListener(ShowMarketPanel);

        // YARDIM Butonu (?)
        GameObject helpBtnObj = new GameObject("HelpButton");
        helpBtnObj.transform.SetParent(rightHud.transform);
        RectTransform helpRt = helpBtnObj.AddComponent<RectTransform>();
        helpRt.sizeDelta = new Vector2(40, 40);

        Image helpImg = helpBtnObj.AddComponent<Image>();
        helpImg.color = new Color(0.2f, 0.2f, 0.25f, 1f);
        
        Button helpButton = helpBtnObj.AddComponent<Button>();
        Text helpText = CreateText(helpBtnObj, "HelpText", "?", Vector2.zero, Color.white, 20);
        helpText.alignment = TextAnchor.MiddleCenter;
        RectTransform hTxtRt = helpText.GetComponent<RectTransform>();
        hTxtRt.anchorMin = Vector2.zero;
        hTxtRt.anchorMax = Vector2.one;
        hTxtRt.offsetMin = Vector2.zero;
        hTxtRt.offsetMax = Vector2.zero;
        helpButton.onClick.AddListener(ShowStartMenu);

        // 3. Karakter Diyalog Paneli (Üst HUD Barının Hemen Altında Tam Genişlikte Yer Alacak)
        speechBubbleObj = new GameObject("SpeechBubblePanel");
        speechBubbleObj.transform.SetParent(canvasObj.transform);
        RectTransform bubbleRt = speechBubbleObj.AddComponent<RectTransform>();
        bubbleRt.anchorMin = new Vector2(0, 1);
        bubbleRt.anchorMax = new Vector2(1, 1);
        bubbleRt.pivot = new Vector2(0.5f, 1);
        bubbleRt.anchoredPosition = new Vector2(0, -80); // HUD panelinin (80px) hemen altında
        bubbleRt.sizeDelta = new Vector2(0, 60);

        Image bubbleBg = speechBubbleObj.AddComponent<Image>();
        bubbleBg.color = new Color(0.12f, 0.12f, 0.15f, 0.9f); // Sleek koyu gri

        // Karakter Küçük Avatarı
        GameObject gameAvatarObj = new GameObject("GameAvatar");
        gameAvatarObj.transform.SetParent(speechBubbleObj.transform);
        RectTransform gaRt = gameAvatarObj.AddComponent<RectTransform>();
        gaRt.anchorMin = new Vector2(0, 0.5f);
        gaRt.anchorMax = new Vector2(0, 0.5f);
        gaRt.pivot = new Vector2(0, 0.5f);
        gaRt.anchoredPosition = new Vector2(15, 0);
        gaRt.sizeDelta = new Vector2(46, 46);
        gameAvatarImage = gameAvatarObj.AddComponent<Image>();

        // Diyalog Balonu Yazısı
        GameObject speechTextObj = new GameObject("SpeechText");
        speechTextObj.transform.SetParent(speechBubbleObj.transform);
        RectTransform stRt = speechTextObj.AddComponent<RectTransform>();
        stRt.anchorMin = new Vector2(0, 0);
        stRt.anchorMax = new Vector2(1, 1);
        stRt.offsetMin = new Vector2(75, 5);
        stRt.offsetMax = new Vector2(-15, -5);
        gameSpeechText = speechTextObj.AddComponent<Text>();
        gameSpeechText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        gameSpeechText.fontSize = 13;
        gameSpeechText.color = new Color(0.9f, 0.9f, 0.9f, 1f);
        gameSpeechText.alignment = TextAnchor.MiddleLeft;
        gameSpeechText.text = "Çizim rulosunu kullanarak malzemeleri yerleştir.";

        // Başlangıçta karakter seçilmediği için bu paneli gizliyoruz
        speechBubbleObj.SetActive(false);

        // 4. MARKET PANELİ (Popup)
        CreateMarketPanel(canvasObj);

        // 5. GİRİŞ VE KARAKTER SEÇİM EKRANI OVERLAY
        CreateStartMenuPanel(canvasObj);
    }

    private void CreateMarketPanel(GameObject canvasObj)
    {
        marketPanel = new GameObject("MarketPanel");
        marketPanel.transform.SetParent(canvasObj.transform);
        RectTransform marketRt = marketPanel.AddComponent<RectTransform>();
        marketRt.anchorMin = new Vector2(0.5f, 0.5f);
        marketRt.anchorMax = new Vector2(0.5f, 0.5f);
        marketRt.pivot = new Vector2(0.5f, 0.5f);
        marketRt.anchoredPosition = Vector2.zero;
        marketRt.sizeDelta = new Vector2(460, 360);

        Image bg = marketPanel.AddComponent<Image>();
        bg.color = new Color(0.1f, 0.1f, 0.14f, 0.98f); // Koyu arka plan

        CreateText(marketPanel, "MarketTitle", "DEKORASYON DÜKKANI", new Vector2(0, 140), new Color(0.9f, 0.75f, 0.15f, 1f), 22);

        marketStatusText = CreateText(marketPanel, "MarketStatusText", "Bahçeye eklemek için dekor satın alabilirsiniz.", new Vector2(0, 105), Color.gray, 14);

        // Dekor 1: Saksı
        CreateMarketItem(marketPanel, "Saksı", "FlowerPot", "100 Altın", new Vector2(0, 40), () => TryBuyDecoration("FlowerPot", 100, 0));
        // Dekor 2: Bank
        CreateMarketItem(marketPanel, "Bahçe Bankı", "Bench", "200 Altın", new Vector2(0, -20), () => TryBuyDecoration("Bench", 200, 0));
        // Dekor 3: Fener
        CreateMarketItem(marketPanel, "Tarihi Fener", "Lantern", "150 Altın + 10 Yıldız", new Vector2(0, -80), () => TryBuyDecoration("Lantern", 150, 10));

        // Kapat Butonu
        GameObject closeBtn = new GameObject("CloseButton");
        closeBtn.transform.SetParent(marketPanel.transform);
        RectTransform closeRt = closeBtn.AddComponent<RectTransform>();
        closeRt.anchoredPosition = new Vector2(0, -140);
        closeRt.sizeDelta = new Vector2(180, 40);
        Image closeImg = closeBtn.AddComponent<Image>();
        closeImg.color = new Color(0.7f, 0.2f, 0.2f, 1f); // Kırmızı
        Button closeButton = closeBtn.AddComponent<Button>();
        Text closeText = CreateText(closeBtn, "CloseText", "KAPAT", Vector2.zero, Color.white, 14);
        closeText.alignment = TextAnchor.MiddleCenter;
        closeButton.onClick.AddListener(HideMarketPanel);

        marketPanel.SetActive(false);
    }

    private void CreateMarketItem(GameObject parent, string label, string spriteChain, string costText, Vector2 pos, System.Action onBuy)
    {
        GameObject itemRow = new GameObject($"MarketItem_{label}");
        itemRow.transform.SetParent(parent.transform);
        RectTransform rt = itemRow.AddComponent<RectTransform>();
        rt.anchoredPosition = pos;
        rt.sizeDelta = new Vector2(400, 50);

        // İkon
        GameObject iconObj = new GameObject("Icon");
        iconObj.transform.SetParent(itemRow.transform);
        RectTransform iconRt = iconObj.AddComponent<RectTransform>();
        iconRt.anchorMin = new Vector2(0, 0.5f);
        iconRt.anchorMax = new Vector2(0, 0.5f);
        iconRt.pivot = new Vector2(0, 0.5f);
        iconRt.anchoredPosition = new Vector2(10, 0);
        iconRt.sizeDelta = new Vector2(40, 40);
        Image iconImg = iconObj.AddComponent<Image>();
        iconImg.sprite = RestorationSpriteFactory.GetSprite(spriteChain, 1, false);

        // İsim ve Fiyat
        Text info = CreateText(itemRow, "InfoText", $"{label} ({costText})", new Vector2(40, 0), Color.white, 14);
        info.alignment = TextAnchor.MiddleLeft;

        // Satın Al Butonu
        GameObject buyBtn = new GameObject("BuyButton");
        buyBtn.transform.SetParent(itemRow.transform);
        RectTransform buyRt = buyBtn.AddComponent<RectTransform>();
        buyRt.anchorMin = new Vector2(1, 0.5f);
        buyRt.anchorMax = new Vector2(1, 0.5f);
        buyRt.pivot = new Vector2(1, 0.5f);
        buyRt.anchoredPosition = new Vector2(-10, 0);
        buyRt.sizeDelta = new Vector2(100, 34);
        Image buyImg = buyBtn.AddComponent<Image>();
        buyImg.color = new Color(0.2f, 0.5f, 0.8f, 1f); // Mavi
        Button button = buyBtn.AddComponent<Button>();
        Text btnTxt = CreateText(buyBtn, "BtnTxt", "SATIN AL", Vector2.zero, Color.white, 12);
        btnTxt.alignment = TextAnchor.MiddleCenter;
        button.onClick.AddListener(() => onBuy?.Invoke());
    }

    private void TryBuyDecoration(string type, int goldCost, int starCost)
    {
        if (TaskManager.Instance == null || RenovationManager.Instance == null) return;

        if (TaskManager.Instance.currentGold >= goldCost && TaskManager.Instance.currentStars >= starCost)
        {
            // Satın alımı gerçekleştir
            TaskManager.Instance.currentGold -= goldCost;
            TaskManager.Instance.currentStars -= starCost;

            RenovationManager.Instance.BuyDecoration(type);
            marketStatusText.text = $"Muhteşem! {type} başarıyla bahçeye yerleştirildi.";
            marketStatusText.color = Color.green;

            UpdateHUD();
            
            // Karakter diyaloğunu güncelle
            SetSpeechText(selectedCharacter == "Hasan" 
                ? "Harika bir seçim! Bahçemiz gittikçe zenginleşiyor." 
                : "Harika! Yeni dekorasyon villanın estetiğine çok uydu.");
        }
        else
        {
            marketStatusText.text = "Hata: Yetersiz Altın veya Yıldız!";
            marketStatusText.color = Color.red;
        }
    }

    private void CreateStartMenuPanel(GameObject canvasObj)
    {
        startMenuPanel = new GameObject("StartMenuPanel");
        startMenuPanel.transform.SetParent(canvasObj.transform);
        RectTransform menuRt = startMenuPanel.AddComponent<RectTransform>();
        menuRt.anchorMin = Vector2.zero;
        menuRt.anchorMax = Vector2.one;
        menuRt.offsetMin = Vector2.zero;
        menuRt.offsetMax = Vector2.zero;

        Image menuBg = startMenuPanel.AddComponent<Image>();
        menuBg.color = new Color(0.06f, 0.06f, 0.08f, 0.99f); // Tam kapalı premium arka plan

        // Logo
        CreateText(startMenuPanel, "GameTitle", "BELLAGIO", new Vector2(0, 240), new Color(0.9f, 0.75f, 0.15f, 1f), 38);
        CreateText(startMenuPanel, "GameSubtitle", "MİMARİ RESTORASYON SIMÜLATÖRÜ", new Vector2(0, 195), Color.gray, 13);

        // Mimar Seçim Başlığı
        CreateText(startMenuPanel, "SelectTitle", "Restorasyon Mimarını Seçiniz:", new Vector2(0, 140), Color.white, 16);

        // LEYLA SEÇİM KARTI (Hazal)
        GameObject leylaCard = new GameObject("LeylaCard");
        leylaCard.transform.SetParent(startMenuPanel.transform);
        RectTransform lcRt = leylaCard.AddComponent<RectTransform>();
        lcRt.anchoredPosition = new Vector2(-110, 40);
        lcRt.sizeDelta = new Vector2(170, 130);
        leylaCardBg = leylaCard.AddComponent<Image>();
        leylaCardBg.color = new Color(0.15f, 0.15f, 0.2f, 1f); // Seçilmemiş kart rengi
        Button leylaBtn = leylaCard.AddComponent<Button>();
        leylaBtn.onClick.AddListener(() => OnSelectCharacter("Hazal"));

        GameObject leylaAvatar = new GameObject("Avatar");
        leylaAvatar.transform.SetParent(leylaCard.transform);
        RectTransform laRt = leylaAvatar.AddComponent<RectTransform>();
        laRt.anchoredPosition = new Vector2(0, 20);
        laRt.sizeDelta = new Vector2(60, 60);
        Image laImg = leylaAvatar.AddComponent<Image>();
        laImg.sprite = RestorationSpriteFactory.GetSprite("AvatarHazal", 1, false);
        CreateText(leylaCard, "Name", "Mimar Hazal", new Vector2(0, -25), Color.white, 14);
        CreateText(leylaCard, "Role", "25 Yaşında (Evli)", new Vector2(0, -45), Color.gray, 11);

        // CAN SEÇİM KARTI (Hasan)
        GameObject canCard = new GameObject("CanCard");
        canCard.transform.SetParent(startMenuPanel.transform);
        RectTransform ccRt = canCard.AddComponent<RectTransform>();
        ccRt.anchoredPosition = new Vector2(110, 40);
        ccRt.sizeDelta = new Vector2(170, 130);
        canCardBg = canCard.AddComponent<Image>();
        canCardBg.color = new Color(0.15f, 0.15f, 0.2f, 1f);
        Button canBtn = canCard.AddComponent<Button>();
        canBtn.onClick.AddListener(() => OnSelectCharacter("Hasan"));

        GameObject canAvatar = new GameObject("Avatar");
        canAvatar.transform.SetParent(canCard.transform);
        RectTransform caRt = canAvatar.AddComponent<RectTransform>();
        caRt.anchoredPosition = new Vector2(0, 20);
        caRt.sizeDelta = new Vector2(60, 60);
        Image caImg = canAvatar.AddComponent<Image>();
        caImg.sprite = RestorationSpriteFactory.GetSprite("AvatarHasan", 1, false);
        CreateText(canCard, "Name", "Mimar Hasan", new Vector2(0, -25), Color.white, 14);
        CreateText(canCard, "Role", "25 Yaşında (Evli)", new Vector2(0, -45), Color.gray, 11);

        // TRANSCRIPT DIYALOG ALANI
        GameObject speechObj = new GameObject("WelcomeSpeechBox");
        speechObj.transform.SetParent(startMenuPanel.transform);
        RectTransform spRt = speechObj.AddComponent<RectTransform>();
        spRt.anchoredPosition = new Vector2(0, -75);
        spRt.sizeDelta = new Vector2(400, 80);
        Image spImg = speechObj.AddComponent<Image>();
        spImg.color = new Color(0.1f, 0.1f, 0.12f, 1f);

        transcriptText = CreateText(speechObj, "Transcript", "Restorasyonu yönetecek mimarınızı seçin...", Vector2.zero, new Color(0.9f, 0.9f, 0.9f, 1f), 12);
        transcriptText.alignment = TextAnchor.MiddleCenter;
        RectTransform tRt = transcriptText.GetComponent<RectTransform>();
        tRt.sizeDelta = new Vector2(380, 70);

        // BAŞLA BUTONU (Karakter seçilene kadar aktif değildir)
        GameObject playBtnObj = new GameObject("StartPlayButton");
        playBtnObj.transform.SetParent(startMenuPanel.transform);
        RectTransform playRt = playBtnObj.AddComponent<RectTransform>();
        playRt.anchoredPosition = new Vector2(0, -165);
        playRt.sizeDelta = new Vector2(280, 55);

        Image playImg = playBtnObj.AddComponent<Image>();
        playImg.color = new Color(0.2f, 0.25f, 0.3f, 1f); // Pasif gri buton

        startRestorationButton = playBtnObj.AddComponent<Button>();
        Text playText = CreateText(playBtnObj, "PlayText", "RESTORASYONA BAŞLA", Vector2.zero, Color.white, 16);
        playText.alignment = TextAnchor.MiddleCenter;
        startRestorationButton.interactable = false;
        
        startRestorationButton.onClick.AddListener(HideStartMenu);
    }

    private void OnSelectCharacter(string character)
    {
        selectedCharacter = character;
        startRestorationButton.interactable = true;
        startRestorationButton.GetComponent<Image>().color = new Color(0.15f, 0.6f, 0.3f, 1f); // Aktif yeşil buton

        if (character == "Hazal")
        {
            leylaCardBg.color = new Color(0.15f, 0.6f, 0.3f, 0.8f); // Seçildi yeşil
            canCardBg.color = new Color(0.15f, 0.15f, 0.2f, 1f); // Sıfırla can
            transcriptText.text = "Hazal: \"Eşim Hasan ile birlikte Ege'deki bu güzel bahçeli villayı restore etmek için sabırsızlanıyoruz! 25 yaşındayız ve her ikimiz de mimarız. Hassas mühendislik kuralları ve doğru malzemelerle bu harika evi eski ihtişamına kavuşturacağız. Hazırsanız başlayalım!\"";
        }
        else
        {
            canCardBg.color = new Color(0.15f, 0.6f, 0.3f, 0.8f); // Seçildi yeşil
            leylaCardBg.color = new Color(0.15f, 0.15f, 0.2f, 1f); // Sıfırla leyla
            transcriptText.text = "Hasan: \"Eşim Hazal ile birlikte bu projede çalışmak harika! 25 yaşındayız ve Ege'nin bu eşsiz mimarisini canlandırmak bizim en büyük hayalimiz. Çizim rulosuna (blueprint) tıklayarak aletleri üretip villamızın bahçesindeki hedeflere sürükle ve bize katıl!\"";
        }

        // Oyun içi avatarları ve konuşmaları ayarla
        gameAvatarImage.sprite = RestorationSpriteFactory.GetSprite($"Avatar{character}", 1, false);
        gameSpeechText.text = character == "Hasan" 
            ? "Çizim rulosuna tıklayarak alet üret ve hedeflere sürükle!" 
            : "Çizim rulosunu kullanarak malzemeleri yerleştir.";
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

        goldText.text = TaskManager.Instance.currentGold.ToString();
        starText.text = TaskManager.Instance.currentStars.ToString();
        
        float progress = TaskManager.Instance.GetRenovationProgress();
        progressText.text = $"İLERLEME: %{Mathf.RoundToInt(progress * 100)}";
    }

    public void ShowTooltip(string text, Vector3 position)
    {
        if (tooltipPanelObj != null && tooltipTextComponent != null)
        {
            tooltipPanelObj.SetActive(true);
            tooltipTextComponent.text = text;
            tooltipPanelObj.transform.position = position + new Vector3(0f, -40f, 0f);
        }
    }

    public void HideTooltip()
    {
        if (tooltipPanelObj != null)
        {
            tooltipPanelObj.SetActive(false);
        }
    }

    private GameObject CreateHUDIconGroup(GameObject parent, string name, string iconChainName, out Text valueText, string tooltipText)
    {
        GameObject group = new GameObject(name);
        group.transform.SetParent(parent.transform);
        RectTransform rt = group.AddComponent<RectTransform>();
        rt.sizeDelta = new Vector2(80, 40);

        // Icon
        GameObject iconObj = new GameObject("Icon");
        iconObj.transform.SetParent(group.transform);
        RectTransform iconRt = iconObj.AddComponent<RectTransform>();
        iconRt.anchorMin = new Vector2(0, 0.5f);
        iconRt.anchorMax = new Vector2(0, 0.5f);
        iconRt.pivot = new Vector2(0, 0.5f);
        iconRt.anchoredPosition = Vector2.zero;
        iconRt.sizeDelta = new Vector2(30, 30);
        Image img = iconObj.AddComponent<Image>();
        img.sprite = RestorationSpriteFactory.GetSprite(iconChainName, 1, false);

        // Tooltip handler
        var tooltip = iconObj.AddComponent<UITooltipHandler>();
        tooltip.tooltipText = tooltipText;

        // Text
        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(group.transform);
        RectTransform textRt = textObj.AddComponent<RectTransform>();
        textRt.anchorMin = new Vector2(0, 0.5f);
        textRt.anchorMax = new Vector2(0, 0.5f);
        textRt.pivot = new Vector2(0, 0.5f);
        textRt.anchoredPosition = new Vector2(35, 0);
        textRt.sizeDelta = new Vector2(45, 30);
        valueText = textObj.AddComponent<Text>();
        valueText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        valueText.fontSize = 18;
        valueText.color = Color.white;
        valueText.alignment = TextAnchor.MiddleLeft;

        return group;
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
            speechBubbleObj.SetActive(true); // Giriş yapınca diyalog kutusu açılsın
        }
    }

    public void ShowMarketPanel()
    {
        if (marketPanel != null)
        {
            marketStatusText.text = "Bahçeye eklemek için dekor satın alabilirsiniz.";
            marketStatusText.color = Color.gray;
            marketPanel.SetActive(true);
        }
    }

    public void HideMarketPanel()
    {
        if (marketPanel != null)
        {
            marketPanel.SetActive(false);
        }
    }

    public void SetSpeechText(string text)
    {
        if (gameSpeechText != null)
        {
            gameSpeechText.text = text;
        }
    }
}
