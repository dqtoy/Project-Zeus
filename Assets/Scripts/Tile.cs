﻿using System.Collections.Generic;
using UnityEngine;

public class Tile : MonoBehaviour
{
    public bool isWalkable = true;
    public bool isCurrent = false;
    public bool isTarget = false;
    public bool isSelectable = false;

    public List<Tile> adjacentTileList = new List<Tile>();

    // Needed for breadth-first search
    public bool wasVisited = false;
    public Tile parent = null;
    public int distance = 0;

    void Start()
    {
        
    }

    void Update()
    {
        if (isCurrent)
        {
            GetComponent<Renderer>().material.color = Color.magenta;
        }
        else if (isTarget)
        {
            GetComponent<Renderer>().material.color = Color.green;
        }
        else if (isSelectable)
        {
            GetComponent<Renderer>().material.color = Color.red;
        }
        else
        {
            GetComponent<Renderer>().material.color = Color.white;
        }
    }

    public void FindNeighbors()
    {
        Reset();

        CheckTile(Vector3.forward);
        CheckTile(Vector3.back);
        CheckTile(Vector3.right);
        CheckTile(Vector3.left);
    }

    public void Reset()
    {
        isCurrent = false;
        isTarget = false;
        isSelectable = false;

        adjacentTileList.Clear();

        wasVisited = false;
        parent = null;
        distance = 0;
    }

    private void CheckTile(Vector3 direction)
    {
        Vector3 halfExtents = new Vector3(0.25f, 0.25f, 0.25f);
        Collider[] colliders = Physics.OverlapBox(transform.position + direction, halfExtents);

        foreach (Collider item in colliders)
        {
            Tile tile = item.GetComponent<Tile>();

            if (tile == null || !tile.isWalkable) continue;
            
            RaycastHit hit;
            // If there is an obstacle on the tile, ignore it
            if (Physics.Raycast(tile.transform.position, Vector3.up, out hit, 1)) continue;

            adjacentTileList.Add(tile);
        }
    }
}
