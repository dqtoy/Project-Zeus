﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActionRegenerate : Action
{
    override public int CONCENTRATION_COST { get { return 3; } }
    override public int MIN_AP_COST { get { return 2; } }
    override public TargetType TARGET_TYPE { get { return TargetType.SELF_ONLY; } }

    private IEnumerator WaitForSpellCastAnimations(float fDuration)
    {
        yield return new WaitForSeconds(fDuration);
        currentPhase = Phase.NONE;
        EndAction();
        yield break;
    }

    // Update is called once per frame
    void Update()
    {
        if (!inProgress)
        {
            return;
        }
        if (currentPhase == Phase.NONE)
        {
            mechanics.Animate("IsCastingSpell");
            spentActionPoints += MIN_AP_COST;
            currentPhase = Phase.CASTING;
            new StatusEffect(
                StatusEffect.EffectType.REGENERATION,
                2,
                mechanics
            );
            StartCoroutine(WaitForSpellCastAnimations(1.0f));
        }
    }

}
