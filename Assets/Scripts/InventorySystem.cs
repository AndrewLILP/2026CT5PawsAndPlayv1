using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Manages the player's inventory of items (food and ball).
///
/// Items are collected via OnTriggerEnter on ItemPickup colliders.
/// The player can hold up to maxFood food items and one ball.
///
/// Attach to: Player GameObject
/// </summary>
public class InventorySystem : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private int maxFood = 3; // Maximum food items player can carry

    // ── Private inventory ─────────────────────────────────────────────────────
    private int  foodCount = 0;
    private bool hasBall   = false;

    // ── Public queries ────────────────────────────────────────────────────────
    public bool HasFood() => foodCount > 0;
    public bool HasBall() => hasBall;
    public int  FoodCount => foodCount;

    // ── Public modifiers ──────────────────────────────────────────────────────

    /// <summary> Add food to inventory. Returns false if at capacity. </summary>
    public bool AddFood()
    {
        if (foodCount >= maxFood) return false;
        foodCount++;
        return true;
    }

    /// <summary> Consume one food item. </summary>
    public void UseFood()
    {
        if (foodCount > 0) foodCount--;
    }

    /// <summary> Add ball to inventory. </summary>
    public void AddBall() => hasBall = true;

    /// <summary> Remove ball from inventory (when thrown). </summary>
    public void UseBall() => hasBall = false;

    // ── Pickup detection ──────────────────────────────────────────────────────

    /// <summary>
    /// Called when player walks over a pickup item.
    /// ItemPickup objects should have isTrigger = true and call this.
    /// </summary>
    private void OnTriggerEnter(Collider other)
    {
        ItemPickup pickup = other.GetComponent<ItemPickup>();
        if (pickup == null) return;

        bool collected = false;

        if (pickup.itemType == ItemPickup.ItemType.Food)
            collected = AddFood();
        else if (pickup.itemType == ItemPickup.ItemType.Ball)
        {
            AddBall();
            collected = true;
        }

        if (collected)
            Destroy(other.gameObject); // Remove item from scene
    }
}
