using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SpecialAttackCell : MonoBehaviour
{
    private Item item;

    void Update()
    {
        if(LevelController.Instance.localPlayer.GetBehaviour<Weapon>().selectedSpecialAttack == item.itemID)
        {
            GetComponent<Button>().Select();
        }
    }

    public void Initialize(Item item)
    {
        this.item = item;
        GetComponentInChildren<TMP_Text>().text = item.itemName;
    }

    public void SetItem()
    {
        Weapon localPlayer = NetworkController.Instance.GetLocalPlayerObject().GetComponent<Weapon>();
        localPlayer.RPC_SetSelectedSpecialAttack(item.itemID);
    }
}
