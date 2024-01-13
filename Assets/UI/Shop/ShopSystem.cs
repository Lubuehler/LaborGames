using System.Linq;
using UnityEngine;
using System.Collections.Generic;
using System;


public class ShopSystem : MonoBehaviour
{
    public List<Item> allItems;
    public List<Item> availableItems;
    [SerializeField] private ItemDatabase itemDatabase;
   

    public event Action<int> OnSpecialAttacksChanged;

    public static ShopSystem Instance;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(this.gameObject);
        }
        allItems.AddRange(itemDatabase.items);
        availableItems.AddRange(itemDatabase.items);


    }

    public void BuyItem(Item item)
    {
        if (CanAfford(item.price))
        {
            LevelController.Instance.localPlayer.coins -= item.price;
            if (item.itemType == ItemType.SpecialAttack)
            {
                LevelController.Instance.localPlayer.GetBehaviour<Weapon>().RPC_AddSpecialAttack(item.itemID);
            }
            else if (item.itemType == ItemType.Item)
            {
                LevelController.Instance.localPlayer.RPC_ApplyItem(item.itemID);
                LevelController.Instance.localPlayer.RPC_AddItemEffect(item.itemID);
            }
            OnSpecialAttacksChanged.Invoke(item.itemID);
        }
    }

    public bool BuyService(int price)
    {
        if (CanAfford(price))
        {
            LevelController.Instance.localPlayer.coins -= price;
            return true;
        }
        return false;

    }

    public bool CanAfford(int price)
    {
        return LevelController.Instance.localPlayer.coins >= price;
    }

    public void ResetItemPool()
    {
        allItems.AddRange(itemDatabase.items);
        availableItems.AddRange(itemDatabase.items);
    }
}
