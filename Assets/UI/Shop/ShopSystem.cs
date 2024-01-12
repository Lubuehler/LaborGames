using UnityEngine;
using System.Collections.Generic;
using System;

public class ShopSystem : MonoBehaviour
{
    public List<Item> allItems;
    public List<Item> availableItems;
    [SerializeField] private ItemDatabase itemDatabase;
    private Dictionary<int, Func<IEffect>> itemEffectMappings;

    [SerializeField] private GameObject aoeEffectExplosionPrefab;
    [SerializeField] private LayerMask enemyLayer;

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

        itemEffectMappings = new Dictionary<int, Func<IEffect>>()
        {
            {3085, () => new MultiTargetAttackEffect(LevelController.Instance.localPlayer.weapon) },
            {3074, () => new AoEDamageEffect(LevelController.Instance.localPlayer.weapon, aoeEffectExplosionPrefab, enemyLayer) },
            {3087, () => new RicochetEffect(LevelController.Instance.localPlayer.weapon) }
        // ... other mappings
        };
    }

    public void BuyItem(Item item)
    {
        if (CanAfford(item.price))
        {
            LevelController.Instance.localPlayer.coins -= item.price;
            if (item.itemType == ItemType.SpecialAttack)
            {
                LevelController.Instance.localPlayer.GetBehaviour<Weapon>().RPC_AddSpecialAttack(item.itemID);
                availableItems.Remove(item);
            }
            else if (item.itemType == ItemType.Item)
            {
                LevelController.Instance.localPlayer.RPC_ApplyItem(item.itemID);
                if (itemEffectMappings.ContainsKey(item.itemID))
                {
                    LevelController.Instance.localPlayer.passiveItemEffectManager.AddOrEnhanceEffect(itemEffectMappings[item.itemID]());
                }
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

    public IEffect GetEffectForItem(int itemId)
    {
        if (itemEffectMappings.TryGetValue(itemId, out var effectCreator))
        {
            return effectCreator();
        }
        return null;
    }
}
