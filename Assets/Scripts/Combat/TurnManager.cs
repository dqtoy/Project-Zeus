﻿#pragma warning disable 0649

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class TurnManager : MonoBehaviour
{
    #region Turn Manager Events

    //TODO: Maybe replace these events with the Signals Architecture.
    public delegate void SetUpPlayersDelegate(IReadOnlyList<CombatController> players);
    public delegate void TurnBeginDelegate(int indexCurrentPlayer);
    public static event SetUpPlayersDelegate OnSetUpPlayers = players => { };
    public static event TurnBeginDelegate OnTurnBegin = index => { };
    
    #endregion
    
    System.Random rng;
    [SerializeField] private GameObject combatCamera;
    
    private List<CombatController> combatants = new List<CombatController>();
    private int moveIdx = -1;
    private bool enemyTurn = false;
    private bool frozen = false;
    private bool gameOver = false;
    private GUIPanel panel = null;

    void Start()
    {
        panel = GameObject.FindObjectOfType<GUIPanel>();
        rng = new System.Random();
        
        combatants = GetComponentsInChildren<CombatController>().ToList();
        combatants.Sort(new SortCombatantDescendant());
        OnSetUpPlayers.Invoke(combatants);
    }

    CreatureMechanics GetCurrentCreatureMechanics()
    {
        if (moveIdx == -1) return null;
        if (combatants[moveIdx] == null) return null;
        return combatants[moveIdx].GetComponent<CreatureMechanics>();
    }

    // Also sets enemyTurn appropriately.
    CombatController GetCurrentCombatController()
    {
        if (moveIdx == -1) return null;
        if (combatants[moveIdx] == null) return null;
        if (combatants[moveIdx].IsPC() && !combatants[moveIdx].Dead())
        {
            enemyTurn = false;
            return combatants[moveIdx];
        }
        if (combatants[moveIdx].IsEnemy() && !combatants[moveIdx].Dead())
        {
            enemyTurn = true;
            return combatants[moveIdx];
        }
        return null;
    }

    void EndDefeat()
    {
        MusicManager m = GameObject.Find("MusicManager").GetComponent<MusicManager>();
        gameOver = true;
        if (GetCurrentCombatController() != null)
            GetCurrentCombatController().isTurn = false;
        m.SetDefeat();
    }

    void EndVictory()
    {
        MusicManager m = GameObject.Find("MusicManager").GetComponent<MusicManager>();
        gameOver = true;
        if (GetCurrentCombatController() != null)
            GetCurrentCombatController().isTurn = false;
        m.SetVictory();
    }

    bool EnemyWon()
    {
        foreach (CombatController pick in combatants)
        {
            if (pick == null) continue;
            if (pick.IsPC() && !pick.Dead()) return false;
        }
        return true;
    }

    bool PlayerWon()
    {
        foreach (CombatController pick in combatants)
        {
            if (pick == null) continue;
            if (pick.IsEnemy() && !pick.Dead()) return false;
        }
        return true;
    }

    // Run whenever a creature dies.
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
        // Dead units should cease exerting zone of control.
        ClearZonesOfControl();
        SetZonesOfControl();
        return false;
    }

    public List<CombatController> AllLivingEnemies()
    {
        List<CombatController> r = new List<CombatController>();
        foreach (CombatController pick in combatants)
        {
            if (pick == null) continue;
            if (pick.IsEnemy() && !pick.Dead())
            {
                r.Add(pick);
            }
        }
        return r;
    }

    public List<CombatController> AllLivingPCs()
    {
        List<CombatController> r = new List<CombatController>();
        foreach (CombatController pick in combatants)
        {
            if (pick == null) continue;
            if (pick.IsPC() && !pick.Dead())
            {
                r.Add(pick);
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
        foreach (CombatController combatant in combatants)
        {
            if (combatant == null) continue;
            if (enemyTurn && combatant.IsEnemy()) continue;
            if (!enemyTurn && combatant.IsPC()) continue;
            combatant.AssignZonesOfControl();
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
