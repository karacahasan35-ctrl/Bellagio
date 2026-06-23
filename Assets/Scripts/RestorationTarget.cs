using UnityEngine;

public class RestorationTarget : MonoBehaviour
{
    public string taskId;
    public string requiredChainName;
    public int requiredLevel;
    public string targetDescription;

    private SpriteRenderer bgRenderer;
    private SpriteRenderer iconRenderer;
    private TextMesh labelText;
    private bool isCompleted = false;

    private void Start()
    {
        // 1. Daire arka planı oluştur
        GameObject bgObj = new GameObject("Background");
        bgObj.transform.SetParent(transform);
        bgObj.transform.localPosition = Vector3.zero;
        bgRenderer = bgObj.AddComponent<SpriteRenderer>();
        bgRenderer.sortingOrder = 3;
        
        Shader unlitShader = Shader.Find("Universal Render Pipeline/2D/Sprite-Unlit-Default");
        if (unlitShader != null) bgRenderer.material = new Material(unlitShader);
        
        // Sarı/altın temalı kenarlık
        bgRenderer.sprite = CreateCircularProgressSprite(new Color(0.9f, 0.75f, 0.15f, 1f));

        // 2. Gereken alet/malzeme ikonunu oluştur
        GameObject iconObj = new GameObject("Icon");
        iconObj.transform.SetParent(transform);
        iconObj.transform.localPosition = Vector3.zero;
        iconObj.transform.localScale = Vector3.one * 0.6f;
        iconRenderer = iconObj.AddComponent<SpriteRenderer>();
        iconRenderer.sortingOrder = 4;
        
        if (unlitShader != null) iconRenderer.material = new Material(unlitShader);
        
        // Sprite fabrikasından ikonu al
        iconRenderer.sprite = RestorationSpriteFactory.GetSprite(requiredChainName, requiredLevel, false);
        iconRenderer.color = new Color(1f, 1f, 1f, 0.75f); // Yarı şeffaf

        // 3. Etiket yazısını oluştur (TextMesh)
        GameObject labelObj = new GameObject("Label");
        labelObj.transform.SetParent(transform);
        labelObj.transform.localPosition = new Vector3(0f, -0.8f, 0f);
        labelText = labelObj.AddComponent<TextMesh>();
        labelText.text = targetDescription;
        labelText.fontSize = 24;
        labelText.characterSize = 0.05f;
        labelText.anchor = TextAnchor.UpperCenter;
        labelText.alignment = TextAlignment.Center;
        labelText.color = Color.white;
        
        // TextMesh için unlit shader ata (karanlıkta düzgün gözükmesi için)
        var textRenderer = labelObj.GetComponent<MeshRenderer>();
        textRenderer.sortingOrder = 6;
        if (unlitShader != null) textRenderer.material.shader = unlitShader;

        // 4. Collider ekle (tıklama ve üzerine bırakma tespiti için)
        var col = gameObject.AddComponent<CircleCollider2D>();
        col.radius = 0.6f;
        col.isTrigger = true;
    }

    private Sprite CreateCircularProgressSprite(Color color, int radius = 64)
    {
        int size = radius * 2;
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        Color[] pixels = new Color[size * size];
        for (int i = 0; i < pixels.Length; i++) pixels[i] = Color.clear;
        
        float center = radius - 0.5f;
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dx = x - center;
                float dy = y - center;
                float d = Mathf.Sqrt(dx * dx + dy * dy);
                if (d <= radius && d >= radius - 3)
                {
                    pixels[y * size + x] = color;
                }
                else if (d < radius - 3)
                {
                    pixels[y * size + x] = new Color(color.r, color.g, color.b, 0.25f);
                }
            }
        }
        tex.SetPixels(pixels);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f));
    }

    public bool TryCompleteWithItem(MergeItem item)
    {
        if (isCompleted) return false;

        // Eşya tipi ve seviyesi uyuşuyor mu?
        if (item.itemData.itemChainName == requiredChainName && item.itemData.level == requiredLevel)
        {
            isCompleted = true;
            
            // TaskManager'a bildir
            RenovationTask task = TaskManager.Instance.allTasks.Find(t => t.taskId == taskId);
            if (task != null)
            {
                bool success = TaskManager.Instance.TryCompleteTask(task, item);
                if (success)
                {
                    // Karakter diyalog geribildirimi tetikle
                    TriggerDialogueFeedback(taskId);

                    // Particle Efekti & Yok Etme Animasyonu
                    SpawnCompletionParticles();
                    Destroy(gameObject, 0.5f);
                    return true;
                }
            }
        }

        // Hatalı eşyada sallanma efekti (geribildirim)
        StartCoroutine(ShakeEffect());
        return false;
    }

    private void TriggerDialogueFeedback(string taskId)
    {
        if (GameUIManager.Instance == null) return;

        string charName = GameUIManager.selectedCharacter;
        string feedback = "";

        if (charName == "Hasan")
        {
            if (taskId == "t1") feedback = "Harika! Zemin kalıntılarını süpürüp temizledik.";
            else if (taskId == "t2") feedback = "Çatlakları kireç harcıyla doldurdum, taş gibi oldu!";
            else if (taskId == "t3") feedback = "Zemine karolar döşendi, çok estetik duruyor.";
            else if (taskId == "t4") feedback = "Sütunlar mermer bloklarla sapasağlam oldu.";
            else if (taskId == "t5") feedback = "Musluk takıldı! Antik havuzumuzda su akıyor artık!";
        }
        else // Hazal
        {
            if (taskId == "t1") feedback = "Toz ve molozlar temizlendi, yüzey hassas onarıma hazır.";
            else if (taskId == "t2") feedback = "Doğal harç çatlakları kapattı, yapısal bütünlük korundu.";
            else if (taskId == "t3") feedback = "Restorasyon karosu tarihi dokuyu mükemmel yansıtıyor.";
            else if (taskId == "t4") feedback = "Hasarlı sütun başlığı yontulmuş mermerle yenilendi.";
            else if (taskId == "t5") feedback = "Su tesisatı yenilendi, antik çeşme çalışıyor.";
        }

        GameUIManager.Instance.SetSpeechText(feedback);
    }

    private System.Collections.IEnumerator ShakeEffect()
    {
        Vector3 originalPos = transform.localPosition;
        float elapsed = 0f;
        float duration = 0.2f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float offset = Mathf.Sin(elapsed * 40f) * 0.1f;
            transform.localPosition = originalPos + new Vector3(offset, 0, 0);
            yield return null;
        }
        transform.localPosition = originalPos;
    }

    private void SpawnCompletionParticles()
    {
        GameObject parentObj = new GameObject("BurstParticles");
        parentObj.transform.position = transform.position;

        for (int i = 0; i < 15; i++)
        {
            GameObject p = new GameObject("P");
            p.transform.SetParent(parentObj.transform);
            p.transform.localPosition = Vector3.zero;
            p.transform.localScale = Vector3.one * 0.15f;

            var sr = p.AddComponent<SpriteRenderer>();
            sr.sprite = CreateStarSprite();
            sr.color = new Color(1f, 0.85f, 0.2f, 1f); // Altın sarısı yıldızlar
            sr.sortingOrder = 20;

            Shader unlitShader = Shader.Find("Universal Render Pipeline/2D/Sprite-Unlit-Default");
            if (unlitShader != null) sr.material = new Material(unlitShader);

            Destroy(p, 0.5f);
            
            Vector2 dir = Random.insideUnitCircle.normalized * Random.Range(1.5f, 3.5f);
            StartCoroutine(AnimateParticle(p.transform, dir));
        }

        Destroy(parentObj, 0.6f);
    }

    private Sprite CreateStarSprite()
    {
        int size = 16;
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        Color[] pixels = new Color[size * size];
        for (int i = 0; i < pixels.Length; i++) pixels[i] = Color.clear;

        for (int i = 0; i < size; i++)
        {
            pixels[i * size + size / 2] = Color.white;
            pixels[(size / 2) * size + i] = Color.white;
        }
        tex.SetPixels(pixels);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f));
    }

    private System.Collections.IEnumerator AnimateParticle(Transform t, Vector2 dir)
    {
        float elapsed = 0f;
        float duration = 0.5f;
        Vector3 startPos = t.position;
        Vector3 endPos = startPos + new Vector3(dir.x, dir.y, 0f);
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / duration;
            if (t != null)
            {
                t.position = Vector3.Lerp(startPos, endPos, progress);
                t.localScale = Vector3.Lerp(Vector3.one * 0.15f, Vector3.zero, progress);
            }
            yield return null;
        }
    }
}
