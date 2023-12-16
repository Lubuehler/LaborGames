using System.Linq;
using UnityEngine;
using System.Collections.Generic;
using System;

public class ShopSystem : MonoBehaviour
{

    public List<Item> items;
    [SerializeField] private ItemDatabase itemDatabase;

    public event Action OnSpecialAbilitiesChanged;

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
        this.items.AddRange(itemDatabase.items);
    }

    public void BuyItem(Item item)
    {
        if (CanAfford(item.price))
        {
            LevelController.Instance.localPlayer.coins -= item.price;
            LevelController.Instance.localPlayer.RPC_ApplyItem(item.itemID);
            OnSpecialAbilitiesChanged.Invoke();
            if (item.itemType == ItemType.SpecialAttack)
            {
                items.Remove(item);
            }
        }
    }

    public bool CanAfford(int price)
    {
        return LevelController.Instance.localPlayer.coins >= price;
    }

    public void ResetItemPool()
    {
        this.items.AddRange(itemDatabase.items);
    }
}
