using UnityEngine;

public static class AppBootstrapper
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void OnGameStart()
    {
        Debug.Log("[Bootstrapper] Initializing Bellagio Game...");

        // 1. Kamerayı bul ve dikey mobil ekran için yapılandır (Karanlık tema)
        Camera mainCam = Camera.main;
        if (mainCam == null)
        {
            mainCam = Object.FindAnyObjectByType<Camera>();
        }

        if (mainCam != null)
        {
            mainCam.orthographic = true;
            mainCam.orthographicSize = 5f; // Grid alanına uygun zoom derecesi
            mainCam.clearFlags = CameraClearFlags.SolidColor;
            
            // Premium koyu gri/siyah arka plan rengi (#121214)
            mainCam.backgroundColor = new Color(0.07f, 0.07f, 0.08f, 1f); 
            Debug.Log("[Bootstrapper] Camera configured with dark mode background.");
        }
        else
        {
            // Kamera yoksa otomatik oluştur
            GameObject camObj = new GameObject("Main Camera");
            mainCam = camObj.AddComponent<Camera>();
            camObj.AddComponent<AudioListener>();
            mainCam.tag = "MainCamera";
            mainCam.orthographic = true;
            mainCam.orthographicSize = 5f;
            mainCam.clearFlags = CameraClearFlags.SolidColor;
            mainCam.backgroundColor = new Color(0.07f, 0.07f, 0.08f, 1f);
            camObj.transform.position = new Vector3(0, 0, -10f);
            Debug.Log("[Bootstrapper] Main Camera created procedurally.");
        }

        // 2. Sahnede GridManager var mı kontrol et, yoksa oluştur
        GridManager gridManager = Object.FindAnyObjectByType<GridManager>();
        if (gridManager == null)
        {
            GameObject gmObj = new GameObject("GridManager");
            gridManager = gmObj.AddComponent<GridManager>();
            Debug.Log("[Bootstrapper] GridManager component created procedurally.");
        }

        // 3. TaskManager oluştur
        TaskManager taskManager = Object.FindAnyObjectByType<TaskManager>();
        if (taskManager == null)
        {
            GameObject tmObj = new GameObject("TaskManager");
            taskManager = tmObj.AddComponent<TaskManager>();
            Debug.Log("[Bootstrapper] TaskManager created procedurally.");
        }

        // 4. RenovationManager oluştur
        RenovationManager renovationManager = Object.FindAnyObjectByType<RenovationManager>();
        if (renovationManager == null)
        {
            GameObject rmObj = new GameObject("RenovationManager");
            renovationManager = rmObj.AddComponent<RenovationManager>();
            Debug.Log("[Bootstrapper] RenovationManager created procedurally.");
        }

        // 5. GameUIManager oluştur
        GameUIManager uiManager = Object.FindAnyObjectByType<GameUIManager>();
        if (uiManager == null)
        {
            GameObject uiObj = new GameObject("GameUIManager");
            uiManager = uiObj.AddComponent<GameUIManager>();
            Debug.Log("[Bootstrapper] GameUIManager created procedurally.");
        }
    }
}
