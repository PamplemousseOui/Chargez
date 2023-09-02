using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Bonus/Add shield")]
public class AddShieldBonus : IBonusData
{
    public override void ApplyEffect()
    {
        base.ApplyEffect();
        var player = GameManager.player;
        player.AddShield();
    }
}
