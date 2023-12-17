using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ItemCell : MonoBehaviour
{
    private Item item;

    public void Initialize(Item item)
    {
        this.item = item;
        GetComponentInChildren<TMP_Text>().text = item.itemName;
    }
}
