using System.Collections.Generic;
using System.Text;
using UnityEngine;

/// <summary>
/// Stores player stats, handles leveling, and manages a lightweight inventory/equipment system.
/// </summary>
public class PlayerStats : MonoBehaviour
{
    [Header("Base Stats")]
    [SerializeField] private int maxHealth = 20;
    [SerializeField] private int currentHealth = 20;
    [SerializeField] private int strength = 5;
    [SerializeField] private int defense = 3;
    [SerializeField] private int agility = 3;
    [SerializeField] private int level = 1;
    [SerializeField] private int experience;
    [SerializeField] private int experienceToNextLevel = 10;
    [SerializeField] private int gold;

    [Header("Level Up Bonuses")]
    [SerializeField] private int healthPerLevel = 5;
    [SerializeField] private int strengthPerLevel = 2;
    [SerializeField] private int defensePerLevel = 1;
    [SerializeField] private int agilityPerLevel = 1;

    private readonly List<InventoryItem> inventory = new List<InventoryItem>();
    private readonly Dictionary<EquipmentSlot, InventoryItem> equippedItems = new Dictionary<EquipmentSlot, InventoryItem>();

    /// <summary>
    /// Raised whenever stats, equipment, or inventory change so the UI can refresh.
    /// </summary>
    public event System.Action StatsChanged;

    public int MaxHealth => maxHealth;
    public int CurrentHealth => currentHealth;
    public int Strength => strength + GetEquippedBonus(item => item.StrengthBonus);
    public int Defense => defense + GetEquippedBonus(item => item.DefenseBonus);
    public int Agility => agility + GetEquippedBonus(item => item.AgilityBonus);
    public int Level => level;
    public int Experience => experience;
    public int ExperienceToNextLevel => experienceToNextLevel;
    public int Gold => gold;
    public IReadOnlyList<InventoryItem> Inventory => inventory;
    public IReadOnlyDictionary<EquipmentSlot, InventoryItem> EquippedItems => equippedItems;

    /// <summary>
    /// Adds experience and processes as many level-ups as needed.
    /// </summary>
    public void AddExperience(int amount)
    {
        experience += Mathf.Max(0, amount);

        while (experience >= experienceToNextLevel)
        {
            experience -= experienceToNextLevel;
            LevelUp();
        }

        NotifyChanged();
    }

    /// <summary>
    /// Adds an item to inventory or converts it directly to gold when appropriate.
    /// </summary>
    public void AddItem(InventoryItem item)
    {
        if (item == null)
        {
            return;
        }

        if (item.ItemType == InventoryItemType.Gold)
        {
            gold += Mathf.Max(0, item.GoldAmount);
            NotifyChanged();
            return;
        }

        inventory.Add(item);

        // Auto-equip basic gear so stat bonuses are immediately visible during testing.
        if (item.IsEquippable)
        {
            EquipItem(item);
        }
        else if (item.ItemType == InventoryItemType.Potion && item.HealAmount > 0)
        {
            Heal(item.HealAmount);
        }

        NotifyChanged();
    }

    /// <summary>
    /// Equips a weapon or armor item and updates bonuses.
    /// </summary>
    public bool EquipItem(InventoryItem item)
    {
        if (item == null || !item.IsEquippable)
        {
            return false;
        }

        equippedItems[item.EquipmentSlot] = item;
        NotifyChanged();
        return true;
    }

    /// <summary>
    /// Returns a short multi-line summary for UI display.
    /// </summary>
    public string BuildStatsSummary()
    {
        StringBuilder builder = new StringBuilder();
        builder.AppendLine("Level: " + level);
        builder.AppendLine("XP: " + experience + " / " + experienceToNextLevel);
        builder.AppendLine("Health: " + currentHealth + " / " + maxHealth);
        builder.AppendLine("Strength: " + Strength);
        builder.AppendLine("Defense: " + Defense);
        builder.AppendLine("Agility: " + Agility);
        builder.AppendLine("Gold: " + gold);
        return builder.ToString();
    }

    /// <summary>
    /// Returns a readable list of all inventory contents and equipped gear.
    /// </summary>
    public string BuildInventorySummary()
    {
        StringBuilder builder = new StringBuilder();
        builder.AppendLine("Equipped:");

        foreach (EquipmentSlot slot in new[] { EquipmentSlot.Weapon, EquipmentSlot.Armor })
        {
            if (equippedItems.TryGetValue(slot, out InventoryItem equippedItem))
            {
                builder.AppendLine("- " + slot + ": " + equippedItem.GetSummary());
            }
            else
            {
                builder.AppendLine("- " + slot + ": None");
            }
        }

        builder.AppendLine();
        builder.AppendLine("Inventory:");

        if (inventory.Count == 0)
        {
            builder.AppendLine("- Empty");
        }
        else
        {
            foreach (InventoryItem item in inventory)
            {
                builder.AppendLine("- " + item.GetSummary());
            }
        }

        return builder.ToString();
    }

    private void LevelUp()
    {
        level++;
        maxHealth += healthPerLevel;
        currentHealth = maxHealth;
        strength += strengthPerLevel;
        defense += defensePerLevel;
        agility += agilityPerLevel;
        experienceToNextLevel += 5;
    }

    private void Heal(int amount)
    {
        currentHealth = Mathf.Min(maxHealth, currentHealth + Mathf.Max(0, amount));
    }

    private int GetEquippedBonus(System.Func<InventoryItem, int> selector)
    {
        int total = 0;
        foreach (InventoryItem item in equippedItems.Values)
        {
            total += selector(item);
        }

        return total;
    }

    private void NotifyChanged()
    {
        StatsChanged?.Invoke();
    }
}
