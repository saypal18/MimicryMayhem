using System.Collections.Generic;
public interface ITurnManager
{
    /// <summary>Initializes the turn manager, starting with the first team.</summary>
    void Initialize();

    int GetCurrentTeamIndex();
    int GetTeamCount();

    List<ITick> GetTeams();
    void RegisterPlayer(int teamIndex);
    void UnregisterPlayer(int teamIndex);
}