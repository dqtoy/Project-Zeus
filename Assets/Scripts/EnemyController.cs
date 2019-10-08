﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyController : CombatController
{

    override protected bool CanAttack(Tile tile)
    {
        if (tile.occupant == null) return false;
        return tile.occupant.GetComponent<PlayerController>() != null;
    }

    // Update is called once per frame
    void Update()
    {
        // For now, the AI will just skip its turns.
        if (!isTurn)
        {
            return;
        }
        if (!isActing)
        {
            FindSelectableTiles();
            Tile choice = AIChooseMove();
            if (choice == null)
            {
                EndTurn();
            }
            else
            {
                if (choice.occupant != null)
                {
                    Action atk = GetComponent<ActionBasicAttack>();
                    atk.BeginAction(choice);
                }
                else
                {
                    Action move = GetComponent<ActionMove>();
                    move.BeginAction(choice);
                }
            }
        }
    }

    // Picks an arbitrary attackable target.
    GameObject pickTarget()
    {
        return transform.parent.GetComponent<TurnManager>().PickArbitraryPC();
    }

    Tile AIChooseMove()
    {
        float bestScore = 0.0f;
        Tile bestChoice = null;
        GameObject target = pickTarget();
        foreach (Tile option in selectableTiles)
        {
            if (EvaluateMove(option, target) > bestScore)
            {
                bestScore = EvaluateMove(option, target);
                bestChoice = option;
            }
        }
        if (bestScore > 0.0f) return bestChoice;
        return null;
    }

    // If the tile can be attacked, returns 100. Otherwise,
    // returns 100 minus its distance from the target.
    float EvaluateMove(Tile tile, GameObject target)
    {
        if (CanAttack(tile)) {
            return 100.0f;
        }
        if (tile.occupant != null) return -1.0f; // Invalid choice, can't move to an occupied tile
        return 100.0f - Vector3.Distance(tile.transform.position, target.transform.position);
    }

}
