using UnityEngine;

[CreateAssetMenu(fileName = "NewItemData", menuName = "Bellagio/Item Data")]
public class ItemData : ScriptableObject
{
    [Header("Item Basic Info")]
    public string itemName;
    public string itemChainName;
    public int level = 1;
    public bool isGenerator = false;

    [Header("Visuals")]
    public Sprite itemIcon;
    public Color itemColor = Color.white; // Fener veya saksı için geçici renk ataması

    [Header("Evolution")]
    public ItemData nextLevelItem; // Birleştiğinde dönüşeceği bir sonraki seviye eşyası
}
