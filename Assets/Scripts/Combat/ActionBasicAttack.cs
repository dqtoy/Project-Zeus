﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActionBasicAttack : ActionMove
{

    override protected void Start()
    {
        // Save one tile at the end of the movement path:
        // this is the tile containing the target enemy.
        reserve_tiles = 1;
        base.Start();
    }

    // Update is called once per frame
    void Update()
    {
        if (!inProgress)
        {
            return;
        }
        if (phase == PHASE_MOVING)
        {
            Move();
        }
        else if (phase == PHASE_ATTACKING)
        {
            AttackPhase();
        }
        else
        {
            phase = PHASE_NONE;
        }
    }

    void ResolveAttack(GameObject target)
    {
        spentActionPoints += 4;
        ObjectStats targetStats = target.GetComponent<CreatureStats>();
        if (targetStats == null) targetStats = target.GetComponent<ObjectStats>();
        GetComponent<CreatureStats>().PerformAttack(targetStats);
    }

    private IEnumerator WaitForAttackAnimations(float fDuration)
    {
        float elapsed = 0f;
        while (elapsed < fDuration)
        {
            elapsed += Time.deltaTime;
            yield return null;
        }
        EndAction();
        yield break;
    }

    void AttackPhase()
    {
        if (path.Count == 1)
        {
            Tile targetTile = path.Pop();
            Vector3 direction = CalculateDirection(targetTile.transform.position);
            direction.y = 0f;
            transform.forward = direction;
            ResolveAttack(targetTile.occupant);
        }
        else
        {
            phase = PHASE_NONE;
            StartCoroutine(WaitForAttackAnimations(1.0f));
        }
    }

}
