using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShopItem : MonoBehaviour
{
    [SerializeField] private TMP_Text nameField;
    [SerializeField] private TMP_Text descriptionField;
    [SerializeField] private TMP_Text priceField;
    [SerializeField] private Image image;
    [SerializeField] private TMP_Text soldField;
    [SerializeField] private GameObject collection;

    private Item item;

    private void Update()
    {
        if(item == null)
        {
            Debug.Log("Item Visual displayed but no Item assigned!");
        }
    }

    public void SetItem(Item item)
    {
        this.item = item;
        nameField.text = item.itemName;
        string description = "";
        foreach (StatModifier modifier in item.modifiers)
        {
            description += modifier.statName.ToString() + ": " + modifier.value.ToString() + "\n";
        }
        descriptionField.text = description;
        priceField.text = item.price.ToString();
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

        collection.SetActive(false);
    }
}
