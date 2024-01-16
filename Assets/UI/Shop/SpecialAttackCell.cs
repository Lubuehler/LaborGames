using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SpecialAttackCell : MonoBehaviour
{
    [SerializeField] GameObject selectedIndicator;
    private Item item;

    void Update()
    {
        if (LevelController.Instance.localPlayer.GetBehaviour<Weapon>().selectedSpecialAttack == item.itemID)
        {
            GetComponent<Button>().Select();
            selectedIndicator.SetActive(true);
        }
        else
        {
            selectedIndicator.SetActive(false);
        }
    }

    public void Initialize(Item item)
    {
        this.item = item;
        if (item.icon != null)
        {
            GetComponent<Image>().sprite = item.icon;
            GetComponentInChildren<TMP_Text>().enabled = false;
        }
        else
        {
            GetComponentInChildren<TMP_Text>().enabled = true;
            GetComponentInChildren<TMP_Text>().text = item.itemName;
        }

    }

    public void SetItem()
    {
        Weapon localPlayer = NetworkController.Instance.GetLocalPlayerObject().GetComponent<Weapon>();
        localPlayer.RPC_SetSelectedSpecialAttack(item.itemID);
    }
}
