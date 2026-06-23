using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class RenovationTask
{
    public string taskId;
    public string description;
    public string requiredChainName;
    public int requiredLevel;
    public int goldReward;
    public int starReward;
    public bool isCompleted;

    public RenovationTask(string id, string desc, string chain, int lvl, int gold, int star)
    {
        taskId = id;
        description = desc;
        requiredChainName = chain;
        requiredLevel = lvl;
        goldReward = gold;
        starReward = star;
        isCompleted = false;
    }
}

public class TaskManager : MonoBehaviour
{
    public static TaskManager Instance { get; private set; }

    public List<RenovationTask> allTasks = new List<RenovationTask>();
    
    // Oyuncu Ekonomisi
    public int currentGold = 500;
    public int currentStars = 0;

    public event System.Action OnStateChanged;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeTasks();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void InitializeTasks()
    {
        // Seviye seviye gelecek 4 temel restorasyon görevi
        allTasks.Add(new RenovationTask("t1", "Yoldaki Çakıl Taşlarını Süpür", "Pebble", 1, 100, 10));
        allTasks.Add(new RenovationTask("t2", "Kırık Bahçe Duvarını Onar", "ChiseledStone", 2, 200, 20));
        allTasks.Add(new RenovationTask("t3", "Tarihi Havuzdaki Yosunları Temizle", "Pebble", 1, 150, 15));
        allTasks.Add(new RenovationTask("t4", "Hasar Görmüş Kemer Sütunlarını Onar", "ChiseledStone", 2, 300, 30));
    }

    public RenovationTask GetNextActiveTask()
    {
        foreach (var task in allTasks)
        {
            if (!task.isCompleted)
            {
                return task;
            }
        }
        return null;
    }

    public float GetRenovationProgress()
    {
        if (allTasks.Count == 0) return 0f;
        int completedCount = 0;
        foreach (var task in allTasks)
        {
            if (task.isCompleted) completedCount++;
        }
        return (float)completedCount / allTasks.Count;
    }

    public bool TryCompleteTask(RenovationTask task, MergeItem selectedItem)
    {
        if (task == null || selectedItem == null) return false;

        // Eşya gereksinimlerini kontrol et
        if (selectedItem.itemData.itemChainName == task.requiredChainName && 
            selectedItem.itemData.level == task.requiredLevel)
        {
            // Görevi tamamla
            task.isCompleted = true;
            currentGold += task.goldReward;
            currentStars += task.starReward;

            // Eşyayı yok et ve hücresini temizle
            GridCell cell = selectedItem.currentCell;
            if (cell != null)
            {
                cell.ClearCell();
            }
            Destroy(selectedItem.gameObject);

            Debug.Log($"[TaskManager] Task completed! Gold +{task.goldReward}, Stars +{task.starReward}");
            
            OnStateChanged?.Invoke();
            return true;
        }

        return false;
    }
}
