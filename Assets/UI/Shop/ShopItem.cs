using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShopItem : MonoBehaviour
{
    [SerializeField]
    private TMP_Text nameField;

    [SerializeField]
    private TMP_Text descriptionField;

    [SerializeField]
    private TMP_Text priceField;

    [SerializeField]
    private Image image;

    [SerializeField]
    private TMP_Text soldField;

    [SerializeField]
    private GameObject collection;

    [SerializeField]
    private Image background;

    private Item item;

    private void Update()
    {
        if (item == null)
        {
            Debug.Log("Item Visual displayed but no Item assigned!");
        }
    }

    public void SetItem(Item item)
    {
        // Common fields
        this.item = item;
        nameField.text = item.itemName;
        priceField.text = item.price.ToString();
        if (ShopSystem.Instance.CanAfford(item.price))
        {
            priceField.color = Color.white;
        } else
        {
            priceField.color = Color.red;
        }
        image.sprite = item.icon;

        // differences
        if (item.itemType == ItemType.Item)
        {
            string description = "";
            foreach (StatModifier modifier in item.modifiers)
            {
                description +=
                    modifier.statName.ToString() + ": " + modifier.value.ToString() + "\n";
            }
            descriptionField.text = description;
        }
        else if (item.itemType == ItemType.SpecialAttack)
        {
            descriptionField.text = item.description;
        }
        background.color = item.color;
    }

    public void Redraw()
    {
        SetItem(item);
    }

    public void OnClick()
    {
        if (ShopSystem.Instance.CanAfford(item.price))
        {
            ShopSystem.Instance.BuyItem(item);
            Sell();
        }
    }

    public void Sell()
    {
        soldField.enabled = true;
        gameObject.SetActive(false);
    }
}