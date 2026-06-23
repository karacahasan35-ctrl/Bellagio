using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(BoxCollider2D))]
public class MergeItem : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    public ItemData itemData;
    public GridCell currentCell;

    private SpriteRenderer spriteRenderer;
    private BoxCollider2D boxCollider;
    private Vector3 offset;
    private Vector3 originalPosition;
    private bool isDragging = false;
    private Coroutine snapCoroutine;

    private void Awake()
    {
        EnsureComponents();
    }

    private void EnsureComponents()
    {
        if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
        if (boxCollider == null) boxCollider = GetComponent<BoxCollider2D>();
    }

    private void Start()
    {
        UpdateVisuals();
    }

    public void Initialize(ItemData data, GridCell cell)
    {
        EnsureComponents();
        itemData = data;
        currentCell = cell;
        if (cell != null)
        {
            cell.occupiedItem = this;
            transform.position = cell.transform.position;
        }
        UpdateVisuals();
    }

    public void UpdateVisuals()
    {
        EnsureComponents();
        if (itemData != null)
        {
            spriteRenderer.color = itemData.itemColor;
            spriteRenderer.sprite = RestorationSpriteFactory.GetSprite(itemData.itemChainName, itemData.level, itemData.isGenerator);
            
            // Collider boyutunu sprite sınırlarına göre otomatik ayarla
            if (spriteRenderer.sprite != null && boxCollider != null)
            {
                boxCollider.size = spriteRenderer.sprite.rect.size / spriteRenderer.sprite.pixelsPerUnit;
            }
        }
    }

    private Sprite CreateCircleSprite(int radius = 64)
    {
        int size = radius * 2;
        Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
        Color[] pixels = new Color[size * size];
        
        // Şeffaf arka plan
        for (int i = 0; i < pixels.Length; i++) pixels[i] = Color.clear;
        
        float center = radius - 0.5f;
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dx = x - center;
                float dy = y - center;
                // Dairenin içi
                if (dx * dx + dy * dy <= radius * radius)
                {
                    // Kenarları hafif yumuşatılmış daire (Anti-aliasing etkisi)
                    float dist = Mathf.Sqrt(dx * dx + dy * dy);
                    if (dist > radius - 1.5f)
                    {
                        pixels[y * size + x] = new Color(1f, 1f, 1f, radius - dist);
                    }
                    else
                    {
                        pixels[y * size + x] = Color.white;
                    }
                }
            }
        }
        texture.SetPixels(pixels);
        texture.Apply();
        
        Sprite sprite = Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f));
        sprite.name = "TempCircleSprite";
        return sprite;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (snapCoroutine != null) StopCoroutine(snapCoroutine);

        if (GridManager.Instance != null)
        {
            GridManager.Instance.selectedItem = this;
        }

        // JENERATÖR mekaniği
        if (itemData != null && itemData.isGenerator)
        {
            isDragging = false;
            if (GridManager.Instance != null)
            {
                GridManager.Instance.SpawnItemFromGenerator(this);
            }
            transform.localScale = Vector3.one * 0.85f;
            StartCoroutine(ScaleBackToNormal());
            return;
        }

        isDragging = true;
        originalPosition = currentCell != null ? currentCell.transform.position : transform.position;
        transform.localScale = Vector3.one * 1.15f;

        Vector3 pointerWorldPos = GetWorldPositionOfPointer(eventData);
        offset = transform.position - pointerWorldPos;

        spriteRenderer.sortingOrder = 10;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (isDragging)
        {
            Vector3 pointerWorldPos = GetWorldPositionOfPointer(eventData);
            transform.position = new Vector3(pointerWorldPos.x + offset.x, pointerWorldPos.y + offset.y, transform.position.z);
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (!isDragging) return;
        isDragging = false;
        spriteRenderer.sortingOrder = 5;
        transform.localScale = Vector3.one;

        // 1. Hotspot (RestorationTarget) kontrolü yap!
        RestorationTarget target = GetRestorationTargetUnderMouse();
        if (target != null)
        {
            if (target.TryCompleteWithItem(this))
            {
                // Görev tamamlandı, eşya yok edildi.
                return;
            }
        }

        // 2. Birleşme olmadığı için orijinal slotuna geri dön
        SnapTo(originalPosition);
    }

    private Vector3 GetWorldPositionOfPointer(PointerEventData eventData)
    {
        Vector3 screenPoint = new Vector3(eventData.position.x, eventData.position.y, 0f);
        screenPoint.z = Camera.main.WorldToScreenPoint(transform.position).z;
        return Camera.main.ScreenToWorldPoint(screenPoint);
    }

    private RestorationTarget GetRestorationTargetUnderMouse()
    {
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 mouseWorldPos2D = new Vector2(mouseWorldPos.x, mouseWorldPos.y);
        RaycastHit2D[] hits = Physics2D.RaycastAll(mouseWorldPos2D, Vector2.zero);
        foreach (var hit in hits)
        {
            if (hit.collider != null)
            {
                RestorationTarget target = hit.collider.GetComponent<RestorationTarget>();
                if (target != null)
                {
                    return target;
                }
            }
        }
        return null;
    }

    private bool CanMergeWith(MergeItem other)
    {
        if (other == null) return false;
        if (itemData == null || other.itemData == null) return false;

        // Aynı zincirden mi, aynı seviyede mi ve sonraki seviyesi tanımlı mı?
        return itemData.itemChainName == other.itemData.itemChainName &&
               itemData.level == other.itemData.level &&
               itemData.nextLevelItem != null;
    }

    public void Upgrade(ItemData nextLevel)
    {
        itemData = nextLevel;
        UpdateVisuals();
        
        // Birleşme efekti için küçük bir büyüme küçülme animasyonu
        transform.localScale = Vector3.one * 1.3f;
        StartCoroutine(ScaleBackToNormal());
    }

    private IEnumerator ScaleBackToNormal()
    {
        float duration = 0.2f;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            transform.localScale = Vector3.Lerp(transform.localScale, Vector3.one, elapsed / duration);
            yield return null;
        }
        transform.localScale = Vector3.one;
    }

    private void SnapTo(Vector3 targetPosition)
    {
        if (snapCoroutine != null) StopCoroutine(snapCoroutine);
        snapCoroutine = StartCoroutine(SmoothSnap(targetPosition));
    }

    private IEnumerator SmoothSnap(Vector3 targetPosition)
    {
        float speed = 15f;
        while (Vector3.Distance(transform.position, targetPosition) > 0.01f)
        {
            transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * speed);
            yield return null;
        }
        transform.position = targetPosition;
    }

    private Vector3 GetMouseWorldPosition()
    {
        Vector3 mousePoint = Input.mousePosition;
        // Kamera derinliğini ayarla
        mousePoint.z = Camera.main.WorldToScreenPoint(transform.position).z;
        return Camera.main.ScreenToWorldPoint(mousePoint);
    }
}
