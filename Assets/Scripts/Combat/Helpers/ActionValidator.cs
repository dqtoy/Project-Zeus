﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Mixin for special moves used in PlayerController and EnemyController
public class ActionValidator : CombatController
{

    private void RegisterMoves()
    {
        // Finds the special moves components attached to the gameobject and adds them to special moves.
        // Importantly, special moves NOT attached to the game object are skipped.
        if (GetComponent<ActionBullRush>() != null) specialMoves.Add(GetComponent<ActionBullRush>());
        if (GetComponent<ActionRage>() != null) specialMoves.Add(GetComponent<ActionRage>());
        if (GetComponent<ActionSlaughter>() != null) specialMoves.Add(GetComponent<ActionSlaughter>());
        if (GetComponent<ActionRegenerate>() != null) specialMoves.Add(GetComponent<ActionRegenerate>());
        if (GetComponent<ActionEmpower>() != null) specialMoves.Add(GetComponent<ActionEmpower>());
        if (GetComponent<ActionMultiAttack>() != null) specialMoves.Add(GetComponent<ActionMultiAttack>());
        if (GetComponent<ActionOffhandAttack>() != null) specialMoves.Add(GetComponent<ActionOffhandAttack>());
        if (GetComponent<ActionLifeOrDeath>() != null) specialMoves.Add(GetComponent<ActionLifeOrDeath>());
        if (GetComponent<ActionPetrify>() != null) specialMoves.Add(GetComponent<ActionPetrify>());
        if (GetComponent<ActionSnakeBite>() != null) specialMoves.Add(GetComponent<ActionSnakeBite>());
        if (GetComponent<ActionTerrify>() != null) specialMoves.Add(GetComponent<ActionTerrify>());
        if (GetComponent<ActionTailSweep>() != null) specialMoves.Add(GetComponent<ActionTailSweep>());
    }

    override protected void Start()
    {
        RegisterMoves();
        base.Start();
    }

    private bool ActivateSelfSpecialMove(Action action, bool displayReason)
    {
        // For PCs, simply begin the action.
        if (displayReason) action.BeginAction(null);
        else selectedAction = action;
        return true;
    }

    private bool TargetChargeSpecialMove(Action action, bool displayReason)
    {
        if (FindSelectableChargeTiles(action.MIN_AP_COST))
        {
            selectedAction = action;
            return true;
        }
        if (displayReason) creatureMechanics.DisplayPopup("Nothing in range");
        return false;
    }

    private bool TargetMeleeSpecialMove(Action action, bool displayReason)
    {
        if (FindSelectableAttackTiles(action.MIN_AP_COST))
        {
            selectedAction = action;
            return true;
        }
        if (displayReason) creatureMechanics.DisplayPopup("Nothing in range");
        return false;
    }

    private bool TargetRangedSpecialMove(Action action, bool displayReason)
    {
        if (FindSelectableRangedAttackTiles(action.MIN_AP_COST))
        {
            selectedAction = action;
            return true;
        }
        if (displayReason) creatureMechanics.DisplayPopup("Nothing in range");
        return false;
    }

    // Returns false if invalid special action (e.g. not enough concentration).
    // Only call this for special abilities: basic attacks can be assumed to always be valid.
    protected bool IsValid(Action action, bool displayReason)
    {
        if (action.IsCoolingDown())
        {
            if (displayReason) creatureMechanics.DisplayPopup("Cooling down");
            return false;
        }
        if (actionPoints < action.MIN_AP_COST)
        {
            if (displayReason) creatureMechanics.DisplayPopup("Not enough AP");
            return false;
        }
        if (creatureMechanics.currentConcentration < action.CONCENTRATION_COST)
        {
            if (displayReason) creatureMechanics.DisplayPopup("Not enough concentration");
            return false;
        }
        return true;
    }

    // Returns false if target selection fails.
    protected bool FindAllValidTargets(Action action, bool displayReason)
    {
        switch (action.TARGET_TYPE)
        {
            case Action.TargetType.SELF_ONLY:
                return ActivateSelfSpecialMove(action, displayReason);
            case Action.TargetType.CHARGE:
                return TargetChargeSpecialMove(action, displayReason);
            case Action.TargetType.MELEE:
                return TargetMeleeSpecialMove(action, displayReason);
            case Action.TargetType.RANGED:
                return TargetRangedSpecialMove(action, displayReason);
        }
        return false;
    }
}
