using System.Collections.Generic;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    public static GridManager Instance { get; private set; }

    [Header("Grid Dimensions")]
    public int width = 5;
    public int height = 4;
    public float cellSize = 0.9f;

    [Header("Prefabs")]
    public GameObject cellPrefab;
    public GameObject itemPrefab;

    [Header("Spawning Config")]
    public List<ItemData> spawnableItems; // İlk seviye eşyalar (örn: Seviye 1 Çakıl Taşı)

    private List<GridCell> allCells = new List<GridCell>();
    [HideInInspector] public MergeItem selectedItem;

    private ItemData toolboxData;
    private ItemData toolLvl1;
    private ItemData toolLvl2;
    private ItemData toolLvl3;
    private ItemData toolLvl4;
    private ItemData materialLvl1;
    private ItemData materialLvl2;
    private ItemData materialLvl3;

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
        SpawnRestorationTargets();
    }

    private void GenerateGrid()
    {
        // Grid'i ekranın alt yarısına hizalamak için başlangıç noktasını hesapla (yOffset = -2.2f)
        float startX = -(width - 1) * cellSize / 2f;
        float startY = -(height - 1) * cellSize / 2f - 2.2f;

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
        // Başlangıçta sadece grid'in tam ortasına 1 adet "Restorasyon Alet Çantası" yerleştir
        GridCell centerCell = GetCell(width / 2, height / 2);
        if (centerCell == null && allCells.Count > 0)
        {
            centerCell = allCells[allCells.Count / 2];
        }

        if (centerCell != null)
        {
            GameObject itemObj = Instantiate(itemPrefab, centerCell.transform.position, Quaternion.identity);
            itemObj.SetActive(true);
            MergeItem mergeItem = itemObj.GetComponent<MergeItem>();
            mergeItem.Initialize(toolboxData, centerCell);
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
        CreateRealisticItems();

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

    public void SetGridVisibility(bool visible)
    {
        foreach (var cell in allCells)
        {
            if (cell != null)
            {
                cell.gameObject.SetActive(visible);
                if (cell.occupiedItem != null)
                {
                    cell.occupiedItem.gameObject.SetActive(visible);
                }
            }
        }
    }

    private void CreateRealisticItems()
    {
        // 1. Toolbox (Generator)
        toolboxData = ScriptableObject.CreateInstance<ItemData>();
        toolboxData.name = "ToolboxData";
        toolboxData.itemName = "Restorasyon Alet Çantası";
        toolboxData.itemChainName = "Toolbox";
        toolboxData.level = 1;
        toolboxData.isGenerator = true;
        toolboxData.itemColor = Color.white;
        toolboxData.nextLevelItem = null;

        // 2. Tools
        toolLvl1 = ScriptableObject.CreateInstance<ItemData>();
        toolLvl1.name = "ToolLvl1";
        toolLvl1.itemName = "Restorasyon Fırçası";
        toolLvl1.itemChainName = "Tool";
        toolLvl1.level = 1;
        toolLvl1.isGenerator = false;
        toolLvl1.itemColor = Color.white;

        toolLvl2 = ScriptableObject.CreateInstance<ItemData>();
        toolLvl2.name = "ToolLvl2";
        toolLvl2.itemName = "Harç Malası";
        toolLvl2.itemChainName = "Tool";
        toolLvl2.level = 2;
        toolLvl2.isGenerator = false;
        toolLvl2.itemColor = Color.white;

        toolLvl3 = ScriptableObject.CreateInstance<ItemData>();
        toolLvl3.name = "ToolLvl3";
        toolLvl3.itemName = "Taşçı Çekici";
        toolLvl3.itemChainName = "Tool";
        toolLvl3.level = 3;
        toolLvl3.isGenerator = false;
        toolLvl3.itemColor = Color.white;

        toolLvl4 = ScriptableObject.CreateInstance<ItemData>();
        toolLvl4.name = "ToolLvl4";
        toolLvl4.itemName = "Hassas Iskarpela";
        toolLvl4.itemChainName = "Tool";
        toolLvl4.level = 4;
        toolLvl4.isGenerator = false;
        toolLvl4.itemColor = Color.white;

        toolLvl1.nextLevelItem = toolLvl2;
        toolLvl2.nextLevelItem = toolLvl3;
        toolLvl3.nextLevelItem = toolLvl4;
        toolLvl4.nextLevelItem = null;

        // 3. Materials
        materialLvl1 = ScriptableObject.CreateInstance<ItemData>();
        materialLvl1.name = "MaterialLvl1";
        materialLvl1.itemName = "Doğal Kireç Harcı Kovası";
        materialLvl1.itemChainName = "Material";
        materialLvl1.level = 1;
        materialLvl1.isGenerator = false;
        materialLvl1.itemColor = Color.white;

        materialLvl2 = ScriptableObject.CreateInstance<ItemData>();
        materialLvl2.name = "MaterialLvl2";
        materialLvl2.itemName = "Restorasyon Karosu";
        materialLvl2.itemChainName = "Material";
        materialLvl2.level = 2;
        materialLvl2.isGenerator = false;
        materialLvl2.itemColor = Color.white;

        materialLvl3 = ScriptableObject.CreateInstance<ItemData>();
        materialLvl3.name = "MaterialLvl3";
        materialLvl3.itemName = "Yontulmuş Mermer Blok";
        materialLvl3.itemChainName = "Material";
        materialLvl3.level = 3;
        materialLvl3.isGenerator = false;
        materialLvl3.itemColor = Color.white;

        materialLvl1.nextLevelItem = materialLvl2;
        materialLvl2.nextLevelItem = materialLvl3;
        materialLvl3.nextLevelItem = null;

        // Set spawnable items
        spawnableItems = new List<ItemData> { toolLvl1, materialLvl1 };
    }

    public void SpawnItemFromGenerator(MergeItem generator)
    {
        if (generator == null || generator.currentCell == null) return;

        List<GridCell> emptyNeighbors = GetEmptyNeighborCells(generator.currentCell);
        GridCell targetCell = null;

        if (emptyNeighbors.Count > 0)
        {
            targetCell = emptyNeighbors[UnityEngine.Random.Range(0, emptyNeighbors.Count)];
        }
        else
        {
            List<GridCell> allEmpty = GetEmptyCells();
            if (allEmpty.Count > 0)
            {
                targetCell = allEmpty[UnityEngine.Random.Range(0, allEmpty.Count)];
            }
        }

        if (targetCell == null)
        {
            Debug.LogWarning("Grid tamamen dolu! Yeni eşya üretilemiyor.");
            return;
        }

        ItemData itemToSpawn = UnityEngine.Random.value < 0.6f ? toolLvl1 : materialLvl1;

        GameObject itemObj = Instantiate(itemPrefab, targetCell.transform.position, Quaternion.identity);
        itemObj.SetActive(true);
        MergeItem mergeItem = itemObj.GetComponent<MergeItem>();
        mergeItem.Initialize(itemToSpawn, targetCell);
    }

    private List<GridCell> GetEmptyNeighborCells(GridCell center)
    {
        List<GridCell> neighbors = new List<GridCell>();
        foreach (var cell in allCells)
        {
            if (cell != null && cell.IsEmpty)
            {
                int dx = Mathf.Abs(cell.gridX - center.gridX);
                int dy = Mathf.Abs(cell.gridY - center.gridY);
                if (dx <= 1 && dy <= 1 && (dx != 0 || dy != 0))
                {
                    neighbors.Add(cell);
                }
            }
        }
        return neighbors;
    }

    public GridCell GetCell(int x, int y)
    {
        foreach (var cell in allCells)
        {
            if (cell != null && cell.gridX == x && cell.gridY == y)
            {
                return cell;
            }
        }
        return null;
    }

    public void SpawnRestorationTargets()
    {
        // Temizle (varsa eski hedefleri sil)
        foreach (var oldTarget in Object.FindObjectsByType<RestorationTarget>(FindObjectsSortMode.None))
        {
            Destroy(oldTarget.gameObject);
        }

        // Görevler ve koordinatları
        // t1: "Karoların Kalıntılarını Temizle", Tool Lvl 1 (Brush)
        CreateTargetObject("t1", "Tool", 1, "Tozları Süpür", new Vector3(-1.5f, 2.5f, 0f));
        // t2: "Çatlak Duvarları Kireç Harcıyla Doldur", Material Lvl 1 (Mortar)
        CreateTargetObject("t2", "Material", 1, "Çatlakları Doldur", new Vector3(1.5f, 2.5f, 0f));
        // t3: "Zemini Restorasyon Karosu ile Döşe", Material Lvl 2 (Tile)
        CreateTargetObject("t3", "Material", 2, "Karoları Döşe", new Vector3(-1.0f, 0.8f, 0f));
        // t4: "Kemer Sütunlarını Mermerle Yenile", Material Lvl 3 (Marble)
        CreateTargetObject("t4", "Material", 3, "Sütunları Onar", new Vector3(1.0f, 0.8f, 0f));
    }

    private void CreateTargetObject(string taskId, string chain, int lvl, string desc, Vector3 pos)
    {
        if (TaskManager.Instance != null)
        {
            RenovationTask task = TaskManager.Instance.allTasks.Find(t => t.taskId == taskId);
            if (task != null && task.isCompleted) return;
        }

        GameObject targetObj = new GameObject($"RestorationTarget_{taskId}");
        targetObj.transform.position = pos;
        
        RestorationTarget target = targetObj.AddComponent<RestorationTarget>();
        target.taskId = taskId;
        target.requiredChainName = chain;
        target.requiredLevel = lvl;
        target.targetDescription = desc;
    }
}
