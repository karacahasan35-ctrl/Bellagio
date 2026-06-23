using System.Collections.Generic;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    public static GridManager Instance { get; private set; }

    [Header("Grid Dimensions")]
    public int width = 5;
    public int height = 1;
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
    private ItemData faucetData;

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
        SpawnScatteredItems();
        SpawnRestorationTargets();
    }

    private void SpawnScatteredItems()
    {
        // 5 başlangıç restorasyon malzemesini bahçenin zeminindeki doğal noktalara dağıtılmış olarak yerleştir
        Vector3[] scatterPositions = new Vector3[]
        {
            new Vector3(-2.3f, -2.5f, 0f), // Sol taraftaki ev girişinin yanı (Fırça)
            new Vector3(1.8f, -2.3f, 0f),  // Çeşmenin sağ tarafı (Kireç Harcı Kovası)
            new Vector3(-1.2f, -3.4f, 0f), // Sol alt patika zemin (Restorasyon Karosu)
            new Vector3(2.2f, -3.2f, 0f),  // Sağ alt bahçe patikası zemin (Mermer Blok)
            new Vector3(0.5f, -1.8f, 0f)   // Çeşmenin hemen yanı (Antik Musluk)
        };

        ItemData[] itemsToScatter = new ItemData[]
        {
            toolLvl1,      // Restorasyon Fırçası
            materialLvl1,  // Kireç Harcı Kovası
            materialLvl2,  // Restorasyon Karosu
            materialLvl3,  // Mermer Blok
            faucetData     // Antik Musluk
        };

        for (int i = 0; i < itemsToScatter.Length; i++)
        {
            GameObject itemObj = Instantiate(itemPrefab, scatterPositions[i], Quaternion.identity);
            itemObj.SetActive(true);
            MergeItem mergeItem = itemObj.GetComponent<MergeItem>();
            mergeItem.Initialize(itemsToScatter[i], null);
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
        // 1. BlueprintRoll (Generator)
        toolboxData = ScriptableObject.CreateInstance<ItemData>();
        toolboxData.name = "BlueprintData";
        toolboxData.itemName = "Çizim Rulosu";
        toolboxData.itemChainName = "BlueprintRoll";
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

        // 4. Faucet
        faucetData = ScriptableObject.CreateInstance<ItemData>();
        faucetData.name = "FaucetData";
        faucetData.itemName = "Antik Bakır Musluk";
        faucetData.itemChainName = "Faucet";
        faucetData.level = 1;
        faucetData.isGenerator = false;
        faucetData.itemColor = Color.white;
        faucetData.nextLevelItem = null;

        // Set spawnable items
        spawnableItems = new List<ItemData> { toolLvl1, materialLvl1, materialLvl2, materialLvl3, faucetData };
    }

    public void SpawnItemFromGenerator(MergeItem generator)
    {
        List<GridCell> emptySlots = GetEmptyCells();
        if (emptySlots.Count == 0)
        {
            Debug.LogWarning("Tepsi dolu! Yeni alet üretilemiyor.");
            return;
        }

        // İlk boş slotu seç
        GridCell targetCell = emptySlots[0];

        // Rastgele restorasyon eşyası seç
        ItemData itemToSpawn = GetRandomRestorationItem();

        GameObject itemObj = Instantiate(itemPrefab, targetCell.transform.position, Quaternion.identity);
        itemObj.SetActive(true);
        MergeItem mergeItem = itemObj.GetComponent<MergeItem>();
        mergeItem.Initialize(itemToSpawn, targetCell);
    }

    private ItemData GetRandomRestorationItem()
    {
        float r = UnityEngine.Random.value;
        if (r < 0.25f) return toolLvl1;          // Fırça
        else if (r < 0.50f) return materialLvl1; // Harç Kovası
        else if (r < 0.70f) return materialLvl2; // Karo
        else if (r < 0.85f) return materialLvl3; // Mermer
        else return faucetData;                 // Musluk
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
        // t5: "Antik Çeşmenin Musluğunu Yenile", Faucet Lvl 1 (Faucet)
        CreateTargetObject("t5", "Faucet", 1, "Musluğu Yenile", new Vector3(0f, 1.8f, 0f));
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
