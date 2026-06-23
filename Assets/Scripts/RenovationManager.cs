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
        
        // Ekranı tam kaplayacak şekilde ölçekle
        sr.transform.localScale = new Vector3(scaleX, scaleY, 1);
        sr.transform.position = new Vector3(0, 0, 5f); // Grid nesnelerinin arkası
    }
}
