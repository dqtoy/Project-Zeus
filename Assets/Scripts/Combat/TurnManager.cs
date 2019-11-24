﻿#pragma warning disable 0649

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurnManager : MonoBehaviour
{
    #region Turn Manager Events

    public delegate void SetUpPlayersDelegate(IReadOnlyList<GameObject> players);
    public delegate void TurnBeginDelegate(int indexCurrentPlayer);
    public static event SetUpPlayersDelegate OnSetUpPlayers = players => { };
    public static event TurnBeginDelegate OnTurnBegin = index => { };
    
    #endregion
    
    
    System.Random rng;
    [SerializeField] private GameObject combatCamera;
    private List<GameObject> combatants = new List<GameObject>();
    private int moveIdx = -1;
    private bool enemyTurn = false;
    private bool frozen = false;
    private bool gameOver = false;
    private GUIPanel panel = null;

    void Start()
    {
        panel = GameObject.FindObjectOfType<GUIPanel>();
        rng = new System.Random();
        foreach (Transform child in transform)
        {
            if (child != transform)
            {
                combatants.Add(child.gameObject);
            }
        }
        
        combatants.Sort(new SortCombatantDescendant());
        OnSetUpPlayers.Invoke(combatants);
    }

    CreatureMechanics GetCurrentCreatureMechanics()
    {
        if (moveIdx == -1) return null;
        if (combatants[moveIdx] == null) return null;
        return combatants[moveIdx].GetComponent<CreatureMechanics>();
    }

    CombatController GetCurrentCombatController()
    {
        if (moveIdx == -1) return null;
        if (combatants[moveIdx] == null) return null;
        if (combatants[moveIdx].GetComponent<PlayerController>() != null)
        {
            enemyTurn = false;
            return combatants[moveIdx].GetComponent<PlayerController>();
        }
        if (combatants[moveIdx].GetComponent<EnemyController>() != null)
        {
            enemyTurn = true;
            return combatants[moveIdx].GetComponent<EnemyController>();
        }
        return null;
    }

    void EndDefeat()
    {
        MusicManager m = GameObject.Find("MusicManager").GetComponent<MusicManager>();
        gameOver = true;
        GetCurrentCombatController().isTurn = false;
        m.SetDefeat();
    }

    void EndVictory()
    {
        MusicManager m = GameObject.Find("MusicManager").GetComponent<MusicManager>();
        gameOver = true;
        GetCurrentCombatController().isTurn = false;
        m.SetVictory();
    }

    bool EnemyWon()
    {
        foreach (GameObject pick in combatants)
        {
            if (pick == null) continue;
            if (pick.GetComponent<PlayerController>() != null) return false;
        }
        return true;
    }

    bool PlayerWon()
    {
        foreach (GameObject pick in combatants)
        {
            if (pick == null) continue;
            if (pick.GetComponent<EnemyController>() != null) return false;
        }
        return true;
    }

    public bool CheckCombatOver()
    {
        if (PlayerWon())
        {
            EndVictory();
            return true;
        }
        if (EnemyWon())
        {
            EndDefeat();
            return true;
        }
        ClearZonesOfControl();
        SetZonesOfControl();
        return false;
    }

    public List<CombatController> AllLivingEnemies()
    {
        List<CombatController> r = new List<CombatController>();
        foreach (GameObject pick in combatants)
        {
            if (pick == null) continue;
            if (pick.GetComponent<EnemyController>() != null && pick.GetComponent<EnemyController>().Dead() == false)
            {
                r.Add(pick.GetComponent<EnemyController>());
            }
        }
        return r;
    }

    public List<CombatController> AllLivingPCs()
    {
        List<CombatController> r = new List<CombatController>();
        foreach (GameObject pick in combatants)
        {
            if (pick == null) continue;
            if (pick.GetComponent<PlayerController>() != null && pick.GetComponent<PlayerController>().Dead() == false)
            {
                r.Add(pick.GetComponent<PlayerController>());
            }
        }
        return r;
    }

    // Picks an arbitrary/random Player controlled character
    public GameObject PickArbitraryPC()
    {
        List<CombatController> pcs = AllLivingPCs();
        if (pcs.Count > 0) return pcs[rng.Next(pcs.Count)].gameObject;
        return null;
    }

    void ClearZonesOfControl()
    {
        foreach (Tile tile in FindObjectsOfType<Tile>())
        {
            tile.SetIsZoneOfControl(false);
        }
    }

    void SetZonesOfControl()
    {
        foreach (GameObject combatant in combatants)
        {
            if (combatant == null) continue;
            CombatController opponent = null;
            if (enemyTurn) opponent = combatant.GetComponent<PlayerController>();
            if (!enemyTurn) opponent = combatant.GetComponent<EnemyController>();
            if (opponent == null) continue;
            opponent.AssignZonesOfControl();
        }
    }

    void BeginTurn()
    {
        CombatController controller = GetCurrentCombatController();
        ClearZonesOfControl();
        SetZonesOfControl();
        controller.BeginTurn();
        DisplayCurrentCreatureStats();
        OnTurnBegin.Invoke(moveIdx);
    }

    public void DisplayCreatureStats(GameObject creature)
    {
        panel.DisplayStats(creature.GetComponent<CreatureMechanics>());
    }

    public void DisplayCurrentCreatureStats()
    {
        panel.DisplayStats(GetCurrentCreatureMechanics());
    }

    private IEnumerator BeginTurnAfterDelay(float fDuration)
    {
        frozen = true;
        float elapsed = 0f;
        while (elapsed < fDuration)
        {
            elapsed += Time.deltaTime;
            yield return null;
        }
        frozen = false;
        BeginTurn();
        yield break;
    }

    void AdvanceToNextTurn()
    {
        moveIdx = (moveIdx + 1) % combatants.Count;
        if (GetCurrentCombatController() != null)
        {
            combatCamera.GetComponent<CombatCamera>().ZoomNear(GetCurrentCombatController());
            StartCoroutine(BeginTurnAfterDelay(0.25f));
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (frozen || gameOver) return;
        if (GetCurrentCombatController() == null || !GetCurrentCombatController().isTurn)
        {
            AdvanceToNextTurn();
        }
    }
}
