using UnityEngine;

public class RenovationManager : MonoBehaviour
{
    public static RenovationManager Instance { get; private set; }

    public SpriteRenderer messyRenderer;
    public SpriteRenderer cleanRenderer;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        // 1. Harabe avlu görselini yükle ve ayarla
        if (messyRenderer == null)
        {
            GameObject messyObj = new GameObject("MessyCourtyard");
            messyObj.transform.SetParent(transform);
            messyRenderer = messyObj.AddComponent<SpriteRenderer>();
            messyRenderer.sprite = Resources.Load<Sprite>("messy_courtyard");
            messyRenderer.sortingOrder = -2; // En arkada dursun
            
            Shader unlitShader = Shader.Find("Universal Render Pipeline/2D/Sprite-Unlit-Default");
            if (unlitShader != null) messyRenderer.material = new Material(unlitShader);
            
            ScaleToFitScreen(messyRenderer);
        }

        // 2. Temiz avlu görselini yükle ve ayarla
        if (cleanRenderer == null)
        {
            GameObject cleanObj = new GameObject("CleanCourtyard");
            cleanObj.transform.SetParent(transform);
            cleanRenderer = cleanObj.AddComponent<SpriteRenderer>();
            cleanRenderer.sprite = Resources.Load<Sprite>("clean_courtyard");
            cleanRenderer.sortingOrder = -1; // Harabe olanın hemen önünde, grid hücrelerinin arkasında
            
            Shader unlitShader = Shader.Find("Universal Render Pipeline/2D/Sprite-Unlit-Default");
            if (unlitShader != null) cleanRenderer.material = new Material(unlitShader);
            
            ScaleToFitScreen(cleanRenderer);
        }

        // TaskManager olaylarına bağlan
        if (TaskManager.Instance != null)
        {
            TaskManager.Instance.OnStateChanged += UpdateVisuals;
        }

        UpdateVisuals();
    }

    private void OnDestroy()
    {
        if (TaskManager.Instance != null)
        {
            TaskManager.Instance.OnStateChanged -= UpdateVisuals;
        }
    }

    public void UpdateVisuals()
    {
        if (TaskManager.Instance == null || cleanRenderer == null) return;

        float progress = TaskManager.Instance.GetRenovationProgress();
        
        // Temiz ekranın şeffaflığını (alpha) tamamlanma yüzdesine göre ayarla
        Color c = cleanRenderer.color;
        cleanRenderer.color = new Color(c.r, c.g, c.b, progress);
        
        Debug.Log($"[RenovationManager] Avlu temizlik ilerlemesi: %{progress * 100} (Şeffaflık: {progress})");
    }

    public bool hasPot = false;
    public bool hasBench = false;
    public bool hasLantern = false;

    public void BuyDecoration(string type)
    {
        if (type == "FlowerPot")
        {
            hasPot = true;
            SpawnDecoration(type, "FlowerPot", new Vector3(-2.0f, -1.0f, 0f));
        }
        else if (type == "Bench")
        {
            hasBench = true;
            SpawnDecoration(type, "Bench", new Vector3(0f, -0.6f, 0f));
        }
        else if (type == "Lantern")
        {
            hasLantern = true;
            SpawnDecoration(type, "Lantern", new Vector3(2.0f, 0.5f, 0f));
        }
        UpdateVisuals();
    }

    private void SpawnDecoration(string type, string spriteChain, Vector3 position)
    {
        Transform oldDec = transform.Find($"Dec_{type}");
        if (oldDec != null) Destroy(oldDec.gameObject);

        GameObject decObj = new GameObject($"Dec_{type}");
        decObj.transform.SetParent(transform);
        decObj.transform.position = position;
        decObj.transform.localScale = Vector3.one * 0.9f;

        var sr = decObj.AddComponent<SpriteRenderer>();
        sr.sprite = RestorationSpriteFactory.GetSprite(spriteChain, 1, false);
        sr.sortingOrder = 8; // Slotların ve avlunun ortasında bir katman

        Shader unlitShader = Shader.Find("Universal Render Pipeline/2D/Sprite-Unlit-Default");
        if (unlitShader != null) sr.material = new Material(unlitShader);

        // Pop Animasyonu
        decObj.transform.localScale = Vector3.zero;
        StartCoroutine(ScaleUpDecoration(decObj.transform));
    }

    private System.Collections.IEnumerator ScaleUpDecoration(Transform t)
    {
        float elapsed = 0f;
        float duration = 0.5f;
        Vector3 targetScale = Vector3.one * 0.9f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / duration;
            if (t != null)
            {
                t.localScale = Vector3.Lerp(Vector3.zero, targetScale, progress);
            }
            yield return null;
        }
        if (t != null) t.localScale = targetScale;
    }

    private void ScaleToFitScreen(SpriteRenderer sr)
    {
        if (sr == null || sr.sprite == null) return;
        
        Camera cam = Camera.main;
        if (cam == null) cam = Object.FindAnyObjectByType<Camera>();
        if (cam == null) return;

        float cameraHeight = cam.orthographicSize * 2;
        float cameraWidth = cameraHeight * cam.aspect;
        
        Vector2 spriteSize = sr.sprite.bounds.size;
        
        float scaleX = cameraWidth / spriteSize.x;
        float scaleY = cameraHeight / spriteSize.y;
        
        sr.transform.localScale = new Vector3(scaleX, scaleY, 1);
        sr.transform.position = new Vector3(0, 0, 5f);
    }
}
