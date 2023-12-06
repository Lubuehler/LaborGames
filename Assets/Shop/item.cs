using Fusion;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class StatModifier: INetworkStruct
{
    public string statName;
    public int value;
}

[System.Serializable]
public class Item: INetworkStruct
{
    public string itemName;
    //public Sprite icon;
    public int price;
    public List<StatModifier> modifiers;
}