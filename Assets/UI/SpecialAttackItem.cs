using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpecialAttackItem : MonoBehaviour
{
    private Item item;

    public void Initialize(Item item)
    {
        this.item = item;
    }

    public void SetItem()
    {
        Player localPlayer = NetworkController.Instance.GetLocalPlayerObject().GetComponent<Player>();
        localPlayer.RPC_SetSelectedSpecialAttack(item.id);
    }
}
