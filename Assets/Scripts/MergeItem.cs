using System.Collections;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(BoxCollider2D))]
public class MergeItem : MonoBehaviour
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
        spriteRenderer = GetComponent<SpriteRenderer>();
        boxCollider = GetComponent<BoxCollider2D>();
    }

    private void Start()
    {
        UpdateVisuals();
    }

    public void Initialize(ItemData data, GridCell cell)
    {
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
        if (itemData != null)
        {
            spriteRenderer.sprite = itemData.itemIcon;
            spriteRenderer.color = itemData.itemColor;
            
            // Eğer görsel yoksa geçici olarak beyaz bir daire/kare üzerinde seviye rengi kullanırız
            if (spriteRenderer.sprite == null)
            {
                // Unity 6 Default Sprite kullanmak üzere boş bırakabiliriz
            }
        }
    }

    private void OnMouseDown()
    {
        if (snapCoroutine != null) StopCoroutine(snapCoroutine);

        isDragging = true;
        originalPosition = currentCell != null ? currentCell.transform.position : transform.position;
        
        // Fare ile nesnenin merkezi arasındaki mesafeyi kaydet
        Vector3 mouseWorldPos = GetMouseWorldPosition();
        offset = transform.position - mouseWorldPos;

        // Nesneyi katman olarak öne taşı
        spriteRenderer.sortingOrder = 10;
    }

    private void OnMouseDrag()
    {
        if (isDragging)
        {
            Vector3 mouseWorldPos = GetMouseWorldPosition();
            transform.position = new Vector3(mouseWorldPos.x + offset.x, mouseWorldPos.y + offset.y, transform.position.z);
        }
    }

    private void OnMouseUp()
    {
        isDragging = false;
        spriteRenderer.sortingOrder = 5; // Normal katmana geri al

        // GridManager üzerinden üzerine bırakılan hücreyi bul
        GridCell targetCell = GridManager.Instance.GetCellFromWorldPosition(transform.position);

        if (targetCell != null)
        {
            if (targetCell == currentCell)
            {
                // Aynı hücreye bırakıldı, sadece yerine oturt
                SnapTo(originalPosition);
            }
            else if (targetCell.IsEmpty)
            {
                // Boş hücreye bırakıldı, eski hücreyi temizle, yeniye geç
                currentCell.ClearCell();
                targetCell.AssignItem(this);
                SnapTo(targetCell.transform.position);
            }
            else
            {
                // Dolu hücreye bırakıldı, birleşme kontrolü yap
                MergeItem otherItem = targetCell.occupiedItem;

                if (CanMergeWith(otherItem))
                {
                    // Birleştir
                    currentCell.ClearCell();
                    otherItem.Upgrade(itemData.nextLevelItem);
                    Destroy(gameObject);
                }
                else
                {
                    // Birleşemiyor, eski yerine geri dön
                    SnapTo(originalPosition);
                }
            }
        }
        else
        {
            // Grid dışına bırakıldı, eski yerine dön
            SnapTo(originalPosition);
        }
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
