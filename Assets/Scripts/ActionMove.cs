﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActionMove : Action
{

    [SerializeField] private float moveSpeed = 2;

    protected Stack<Tile> path = new Stack<Tile>();

    protected int phase = 0;

    // Extra tiles at the end of the action
    protected int reserve_tiles = 0;

    // Update is called once per frame
    void Update()
    {
        if (!inProgress)
        {
            return;
        }
        if (phase == 1)
        {
            Move();
        }
        else
        {
            phase = 0;
            EndAction();
        }
    }

    protected void Move()
    {
        if (path.Count > reserve_tiles)
        {
            Tile tile = path.Peek();
            Vector3 target = tile.transform.position;
            
            target.y = 0.08f;

            if (Vector3.Distance(transform.position, target) >= 0.1f)
            {
                CharacterController characterController = GetComponent<CharacterController>();
                Vector3 direction = CalculateDirection(target);
                Vector3 velocity = SetHorizontalVelocity(direction);

                transform.forward = direction;
                characterController.Move(velocity * Time.deltaTime);
            }
            else
            {
                // Center of tile reached
                transform.position = target;
                spentActionPoints += 1;
                path.Pop();
            }
        }
        else
        {
            phase = 2;
        }
    }

    override public void BeginAction(Tile targetTile)
    {
        phase = 1;
        CalculatePath(targetTile);
        base.BeginAction(targetTile);
    }

    private void CalculatePath(Tile targetTile)
    {
        path.Clear();
        targetTile.isTarget = true;

        Tile next = targetTile;
        while (next != null)
        {
            path.Push(next);
            next = next.parent;
        }
    }

    protected Vector3 CalculateDirection(Vector3 target)
    {
        Vector3 direction = target - transform.position;
        return direction.normalized;
    }

    private Vector3 SetHorizontalVelocity(Vector3 direction)
    {
        return direction * moveSpeed;
    }
}
