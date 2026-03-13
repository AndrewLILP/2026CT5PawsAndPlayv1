using UnityEngine;

/// <summary>
/// Marks a GameObject as a collectible item.
/// Place on food or ball GameObjects in the scene.
/// Requires a Collider set to isTrigger = true.
/// InventorySystem.OnTriggerEnter reads this component.
/// </summary>
public class ItemPickup : MonoBehaviour
{
    public enum ItemType { Food, Ball }

    [Tooltip("What type of item this pickup represents")]
    public ItemType itemType = ItemType.Food;
}
