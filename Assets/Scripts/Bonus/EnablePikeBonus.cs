using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Bonus/Enable pike")]
public class EnablePikeBonus : IBonusData
{
    public override void ApplyEffect()
    {
        base.ApplyEffect();
        var player = GameManager.player;
        player.EnablePike();
    }
}
