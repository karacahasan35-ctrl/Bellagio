using UnityEngine;

public class GridCell : MonoBehaviour
{
    public int gridX;
    public int gridY;
    public MergeItem occupiedItem;

    public bool IsEmpty => occupiedItem == null;

    public void AssignItem(MergeItem item)
    {
        occupiedItem = item;
        if (item != null)
        {
            item.currentCell = this;
        }
    }

    public void ClearCell()
    {
        occupiedItem = null;
    }
}
