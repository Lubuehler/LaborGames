using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ItemCell : MonoBehaviour
{
    [SerializeField]
    private Image icon;

    [SerializeField]
    private Image background;
    private Item item;

    public void Initialize(Item item)
    {
        this.item = item;
        if (item.icon == null)
        {
            GetComponentInChildren<TMP_Text>().text = item.itemName;
        }
        else
        {
            icon.sprite = item.icon;
        }
        background.color = item.color;
    }
}
