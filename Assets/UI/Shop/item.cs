using Fusion;
using System;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class StatModifier
{
    public string statName;
    public float value;
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
    public Sprite icon;
    public int price;
    public string description;
    public List<StatModifier> modifiers;
    public ItemType itemType;
    public int tier;
    public Color color = Color.black;

    // public static int counter = 0;
}