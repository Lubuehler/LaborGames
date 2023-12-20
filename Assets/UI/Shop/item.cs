using Fusion;
using System;
using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
public class StatModifier
{
    public string statName;
    public int value;
}

[System.Serializable]
public enum ItemType
{
    Item,
    SpecialAttack
}

[System.Serializable]
public class Item
{
    public int itemID;
    public string itemName;
    //public Sprite icon;
    public int price;
    public string description;
    public Color color;
    public List<StatModifier> modifiers;
    public ItemType itemType;
}