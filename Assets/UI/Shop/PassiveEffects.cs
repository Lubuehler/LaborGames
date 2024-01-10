using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System;

public interface IEffect
{
    void EnhanceEffect();
    void ReduceEffect();
}


public class PassiveItemEffectManager
{
    private Dictionary<Type, IEffect> effects = new Dictionary<Type, IEffect>();

    public void AddOrEnhanceEffect(IEffect effect)
    {
        if (effect == null)
        {
            return;
        }
        Type effectType = effect.GetType();
        if (effects.ContainsKey(effectType))
        {
            effects[effectType].EnhanceEffect();
        }
        else
        {
            effects.Add(effectType, effect);
        }
    }
}
