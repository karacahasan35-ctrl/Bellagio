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
}
