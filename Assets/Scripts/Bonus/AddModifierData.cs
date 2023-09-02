using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Bonus/Modifier")]
public class AddModifierData : IBonusData
{
    public Modifier modifier;
    
    public override void ApplyEffect()
    {
        base.ApplyEffect();
        var player = GameManager.player;

        var currModifier = player.modifiers.Find(x => x.name == modifier.name);
        if (currModifier != null)
        {
            currModifier.value += modifier.value;
        }
        else GameManager.player.modifiers.Add(new Modifier(modifier));
    }
}
