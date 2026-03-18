using System;
using UnityEngine;

/// <summary>
/// Item categories supported by the sample dungeon inventory system.
/// </summary>
public enum InventoryItemType
{
    Weapon,
    Armor,
    Potion,
    Gold
}

/// <summary>
/// Equipment slots for wearable items.
/// </summary>
public enum EquipmentSlot
{
    None,
    Weapon,
    Armor
}

/// <summary>
/// Simple serializable item definition used for inventory, equipment, and map pickups.
/// </summary>
[Serializable]
public class InventoryItem
{
    public string ItemName = "New Item";
    public InventoryItemType ItemType = InventoryItemType.Potion;
    public EquipmentSlot EquipmentSlot = EquipmentSlot.None;
    public int HealthBonus;
    public int StrengthBonus;
    public int DefenseBonus;
    public int AgilityBonus;
    public int GoldAmount;
    public int HealAmount;
    public Sprite Icon;
    public Color WorldColor = Color.white;

    /// <summary>
    /// Returns true if the item can be equipped into a gear slot.
    /// </summary>
    public bool IsEquippable => EquipmentSlot != EquipmentSlot.None;

    /// <summary>
    /// Returns a short readable description for UI text.
    /// </summary>
    public string GetSummary()
    {
        if (ItemType == InventoryItemType.Gold)
        {
            return ItemName + " (+" + GoldAmount + " gold)";
        }

        if (ItemType == InventoryItemType.Potion)
        {
            return ItemName + " (heal " + HealAmount + ")";
        }

        return ItemName + " (+STR " + StrengthBonus + ", +DEF " + DefenseBonus + ", +AGI " + AgilityBonus + ")";
    }
}
