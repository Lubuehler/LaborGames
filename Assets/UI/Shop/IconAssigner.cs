using UnityEngine;
using System.Collections.Generic;
using System.IO;

[ExecuteInEditMode]
public class IconAssigner : MonoBehaviour
{
    public ItemDatabase database;
    public string iconsFolderPath = "Icons"; // Path to the folder with icons

    [SerializeField]
    public Color tier1Color;

    [SerializeField]
    public Color tier2Color;

    [SerializeField]
    public Color tier3Color;

    [SerializeField]
    public Color specialAbilityColor;

    void Start()
    {
        AssignIcons();
        AssignColors();
    }

    void AssignIcons()
    {
        foreach (Item item in database.items)
        {
            string iconName = GetIconNameForItem(item);
            Sprite icon = Resources.Load<Sprite>($"{iconsFolderPath}/{iconName}");
            if (icon != null)
            {
                item.icon = icon;
            }
            else
            {
                Debug.LogWarning("Icon not found for item: " + item.itemName);
                Debug.LogWarning($"{iconsFolderPath}/{iconName}.png");
            }
        }
    }

    void AssignColors()
    {
        foreach (Item item in database.items)
        {
            switch (item.tier)
            {
                case 1:
                    item.color = tier1Color;
                    break;
                case 2:
                    item.color = tier2Color;
                    break;
                case 3:
                    item.color = tier3Color;
                    break;
            }
            if (item.itemType == ItemType.SpecialAttack)
            {
                item.color = specialAbilityColor;
            }
            print(item.color);
        }
    }

    string GetIconNameForItem(Item item)
    {
        // Implement your logic to determine the icon name based on the item
        // For example, it could be the item name or a specific naming convention you follow
        return item.itemID.ToString();
    }
}
