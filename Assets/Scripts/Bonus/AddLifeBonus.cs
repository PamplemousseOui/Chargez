using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Bonus/Add life")]
public class AddLifeBonus : IBonusData
{
    public int value = 2;
    public override void ApplyEffect()
    {
        base.ApplyEffect();
        var player = GameManager.player;

        player.healthComponent.Heal(value);
    }
}
