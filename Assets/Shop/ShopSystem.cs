using System.Linq;
using UnityEngine;

public class ShopSystem : MonoBehaviour
{
    public ItemDatabase itemDatabase;

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
    }

    public void BuyItem(Item item)
    {
        if (CanAfford(item.price))
        {
            LevelController.Instance.localPlayer.coins -= item.price;
            //LevelController.Instance.localPlayer.items.Set(LevelController.Instance.localPlayer.items.Count() + 1, item);
            ApplyItemEffects(item);
        }

    }

    public bool CanAfford(int price)
    {
        return LevelController.Instance.localPlayer.coins >= price;
    }

    private void ApplyItemEffects(Item item)
    {
        foreach(StatModifier modifier in item.modifiers)
        {

            LevelController.Instance.localPlayer.ModifyStat(modifier);
        }
    }
}
