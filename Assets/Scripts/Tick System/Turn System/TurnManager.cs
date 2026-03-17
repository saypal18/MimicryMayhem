using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TurnManager : MonoBehaviour, ITurnManager
{
    [SerializeField] private int teamsCount = 2;
    [SerializeField] private float turnTime = 0.2f;

    private int currentTeamIndex = -1;
    private List<ITick> turnTicks = new();
    private bool isTransitioning = false;

    private int[] teamPlayerCounts;
    private int currentTeamActionsReceived;
    [SerializeField] private bool log;

    private void Log(string message)
    {
        if (log) Debug.Log($"[{System.DateTime.Now:HH:mm:ss.ff}] [TurnManager] {message}");
    }

    private void LogWarning(string message)
    {
        if (log) Debug.LogWarning($"[{System.DateTime.Now:HH:mm:ss.ff}] [TurnManager] {message}");
    }

    public void Initialize()
    {
        Log("Initializing...");
        StopAllCoroutines();
        isTransitioning = false;
        currentTeamIndex = 0;
        turnTicks.Clear();
        teamPlayerCounts = new int[teamsCount];
        currentTeamActionsReceived = 0;

        for (int i = 0; i < teamsCount; i++)
        {
            TurnedTick teamTick = new TurnedTick();
            int index = i;
            teamTick.OnPlayed += () => OnPlayerPlayed(index);
            turnTicks.Add(teamTick);
        }

        Log($"Initialized with {teamsCount} teams.");
        // Start the first turn (one frame later to ensure everything is ready)
        StartCoroutine(StartFirstTurn());
    }

    private IEnumerator StartFirstTurn()
    {
        yield return null; // Wait one frame
        Log("Starting first turn cycle.");
        CheckAndTriggerTurn();
    }

    private void CheckAndTriggerTurn()
    {
        // Skip empty teams
        int teamsChecked = 0;
        while (teamPlayerCounts[currentTeamIndex] == 0 && teamsChecked < teamsCount)
        {
            Log($"Skipping Team {currentTeamIndex} (0 players registered).");
            currentTeamIndex = (currentTeamIndex + 1) % teamsCount;
            teamsChecked++;
        }

        if (teamsChecked == teamsCount)
        {
            LogWarning("All teams are empty. Waiting for player registration...");
            // All teams are empty, maybe log warning or wait for registration
            return;
        }

        Log($"Starting Turn for Team {currentTeamIndex}. Expecting {teamPlayerCounts[currentTeamIndex]} actions.");
        TriggerCurrentTurn();
    }

    private void TriggerCurrentTurn()
    {
        currentTeamActionsReceived = 0;
        if (currentTeamIndex >= 0 && currentTeamIndex < turnTicks.Count)
        {
            if (turnTicks[currentTeamIndex] is TurnedTick tt)
            {
                Log($"Triggering OnTick for Team {currentTeamIndex}.");
                tt.TriggerTick();
            }
        }
    }

    private void OnPlayerPlayed(int teamIndex)
    {
        if (teamIndex == currentTeamIndex && !isTransitioning)
        {
            currentTeamActionsReceived++;
            Log($"Action received from Team {teamIndex}. Current count: {currentTeamActionsReceived}/{teamPlayerCounts[teamIndex]}");
            if (currentTeamActionsReceived >= teamPlayerCounts[currentTeamIndex])
            {
                Log($"Team {teamIndex} completed all actions. Transitioning in {turnTime}s.");
                isTransitioning = true;
                StartCoroutine(WaitAndProceed());
            }
        }
        else if (teamIndex != currentTeamIndex)
        {
            LogWarning($"Received OnPlayed from Team {teamIndex} while it is currently Team {currentTeamIndex}'s turn. Ignoring.");
        }
    }

    private IEnumerator WaitAndProceed()
    {
        yield return new WaitForSeconds(turnTime);
        currentTeamIndex = (currentTeamIndex + 1) % teamsCount;
        isTransitioning = false;
        Log($"Proceeding to index {currentTeamIndex}.");
        CheckAndTriggerTurn();
    }

    public void RegisterPlayer(int teamIndex)
    {
        if (teamIndex >= 0 && teamIndex < teamsCount)
        {
            teamPlayerCounts[teamIndex]++;
            Log($"Player registered to Team {teamIndex}. New count: {teamPlayerCounts[teamIndex]}");
        }
    }

    public void UnregisterPlayer(int teamIndex)
    {
        if (teamIndex >= 0 && teamIndex < teamsCount)
        {
            teamPlayerCounts[teamIndex] = Mathf.Max(0, teamPlayerCounts[teamIndex] - 1);
            Log($"Player unregistered from Team {teamIndex}. New count: {teamPlayerCounts[teamIndex]}");

            // If the last remaining player for the current team was removed, transition
            if (teamIndex == currentTeamIndex && currentTeamActionsReceived >= teamPlayerCounts[teamIndex] && !isTransitioning)
            {
                Log($"Currently active Team {teamIndex} finished early due to player removal. Transitioning.");
                isTransitioning = true;
                StartCoroutine(WaitAndProceed());
            }
        }
    }

    public int GetCurrentTeamIndex() => currentTeamIndex;
    public int GetTeamCount() => teamsCount;
    public List<ITick> GetTeams() => turnTicks;
}