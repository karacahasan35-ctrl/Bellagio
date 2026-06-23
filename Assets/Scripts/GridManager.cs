using System.Collections.Generic;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    public static GridManager Instance { get; private set; }

    [Header("Grid Dimensions")]
    public int width = 5;
    public int height = 6;
    public float cellSize = 1.2f;

    [Header("Prefabs")]
    public GameObject cellPrefab;
    public GameObject itemPrefab;

    [Header("Spawning Config")]
    public List<ItemData> spawnableItems; // İlk seviye eşyalar (örn: Seviye 1 Çakıl Taşı)

    private List<GridCell> allCells = new List<GridCell>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        SetupDynamicPrefabs();
        GenerateGrid();
        SpawnInitialItems();
    }

    private void GenerateGrid()
    {
        // Grid'i ekranın ortasına hizalamak için başlangıç noktasını hesapla
        float startX = -(width - 1) * cellSize / 2f;
        float startY = -(height - 1) * cellSize / 2f;

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Vector3 spawnPos = new Vector3(startX + x * cellSize, startY + y * cellSize, 0);
                GameObject cellObj = Instantiate(cellPrefab, spawnPos, Quaternion.identity, transform);
                cellObj.SetActive(true);
                cellObj.name = $"Cell_{x}_{y}";

                GridCell cell = cellObj.GetComponent<GridCell>();
                cell.gridX = x;
                cell.gridY = y;
                allCells.Add(cell);
            }
        }
    }

    private void SpawnInitialItems()
    {
        // Başlangıçta birkaç boş hücreye rastgele eşya spawn et
        int initialSpawnCount = 5;
        for (int i = 0; i < initialSpawnCount; i++)
        {
            SpawnItemInRandomCell();
        }
    }

    public void SpawnItemInRandomCell()
    {
        List<GridCell> emptyCells = GetEmptyCells();
        if (emptyCells.Count == 0)
        {
            Debug.LogWarning("Grid dolu! Yeni eşya spawn edilemiyor.");
            return;
        }

        if (spawnableItems == null || spawnableItems.Count == 0)
        {
            Debug.LogError("Spawlanacak eşya listesi (spawnableItems) boş! Lütfen Inspector'dan ekleyin.");
            return;
        }

        // Rastgele boş bir hücre seç
        GridCell randomCell = emptyCells[Random.Range(0, emptyCells.Count)];
        
        // Rastgele bir başlangıç eşyası seç (örn: Seviye 1 Taş veya Seviye 1 Saksı)
        ItemData randomItemData = spawnableItems[Random.Range(0, spawnableItems.Count)];

        // Eşyayı spawn et
        GameObject itemObj = Instantiate(itemPrefab, randomCell.transform.position, Quaternion.identity);
        itemObj.SetActive(true);
        MergeItem mergeItem = itemObj.GetComponent<MergeItem>();
        mergeItem.Initialize(randomItemData, randomCell);
    }

    public GridCell GetCellFromWorldPosition(Vector3 position)
    {
        GridCell closestCell = null;
        float closestDistance = float.MaxValue;

        foreach (var cell in allCells)
        {
            float distance = Vector3.Distance(cell.transform.position, position);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestCell = cell;
            }
        }

        // Eşya eğer hücreye yeterince yakınsa o hücreyi döndür
        if (closestDistance < cellSize * 0.7f)
        {
            return closestCell;
        }

        return null;
    }

    private List<GridCell> GetEmptyCells()
    {
        List<GridCell> emptyCells = new List<GridCell>();
        foreach (var cell in allCells)
        {
            if (cell.IsEmpty)
            {
                emptyCells.Add(cell);
            }
        }
        return emptyCells;
    }

    private void SetupDynamicPrefabs()
    {
        // 1. Eşyaları yükle
        if (spawnableItems == null || spawnableItems.Count == 0)
        {
            spawnableItems = new List<ItemData>(Resources.LoadAll<ItemData>(""));
            Debug.Log($"[GridManager] Loaded {spawnableItems.Count} items from Resources.");
        }

        // 2. Hücre prefab'ını kontrol et ve dinamik olarak oluştur
        if (cellPrefab == null)
        {
            GameObject cellObj = new GameObject("DynamicCellTemplate");
            var sr = cellObj.AddComponent<SpriteRenderer>();
            
            // URP 2D Unlit Material ata (Görünürlük için kritik!)
            Shader unlitShader = Shader.Find("Universal Render Pipeline/2D/Sprite-Unlit-Default");
            if (unlitShader != null)
            {
                sr.material = new Material(unlitShader);
            }
            
            // Hücre görselini oluştur (Açık-koyu kontrastlı glassmorphic kenarlık)
            sr.sprite = CreateCellSprite(new Color(0.12f, 0.12f, 0.15f, 0.6f), new Color(0.23f, 0.23f, 0.28f, 1f));
            sr.sortingOrder = 1; // Arkada dursun
            
            cellObj.AddComponent<GridCell>();
            cellObj.SetActive(false);
            cellPrefab = cellObj;
            Debug.Log("[GridManager] Dynamic cell template generated.");
        }

        // 3. Eşya prefab'ını kontrol et ve dinamik olarak oluştur
        if (itemPrefab == null)
        {
            GameObject itemObj = new GameObject("DynamicItemTemplate");
            var sr = itemObj.AddComponent<SpriteRenderer>();
            
            // URP 2D Unlit Material ata (Görünürlük için kritik!)
            Shader unlitShader = Shader.Find("Universal Render Pipeline/2D/Sprite-Unlit-Default");
            if (unlitShader != null)
            {
                sr.material = new Material(unlitShader);
            }
            
            sr.sortingOrder = 5; // Önde dursun
            
            itemObj.AddComponent<BoxCollider2D>();
            itemObj.AddComponent<MergeItem>();
            
            itemObj.SetActive(false);
            itemPrefab = itemObj;
            Debug.Log("[GridManager] Dynamic item template generated.");
        }
    }

    private Sprite CreateCellSprite(Color backgroundColor, Color borderColor, int borderThickness = 4, int width = 128, int height = 128)
    {
        Texture2D texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
        Color[] pixels = new Color[width * height];
        
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                // Yumuşatılmış kenarlar (Yuvarlatılmış köşe efekti)
                int rx = x - width / 2;
                int ry = y - height / 2;
                float distToCorner = Mathf.Max(Mathf.Abs(rx) - (width / 2 - 16), 0);
                float distToCornerY = Mathf.Max(Mathf.Abs(ry) - (height / 2 - 16), 0);
                float cornerDist = Mathf.Sqrt(distToCorner * distToCorner + distToCornerY * distToCornerY);
                
                if (cornerDist > 16)
                {
                    pixels[y * width + x] = Color.clear;
                }
                else if (cornerDist > 14.5f)
                {
                    // Köşe kenarlığı
                    pixels[y * width + x] = new Color(borderColor.r, borderColor.g, borderColor.b, 16 - cornerDist);
                }
                else if (x < borderThickness || x >= width - borderThickness || y < borderThickness || y >= height - borderThickness || cornerDist > 12)
                {
                    // Düz kenarlık
                    pixels[y * width + x] = borderColor;
                }
                else
                {
                    // İç kısım
                    pixels[y * width + x] = backgroundColor;
                }
            }
        }
        texture.SetPixels(pixels);
        texture.Apply();
        return Sprite.Create(texture, new Rect(0, 0, width, height), new Vector2(0.5f, 0.5f));
    }
}
