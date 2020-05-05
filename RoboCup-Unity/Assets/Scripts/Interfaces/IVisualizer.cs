using System.Collections.Generic;
using UnityEngine;

public interface IVisualizer
{
    void Init(string teamName, int teamSize, bool rightTeam, Dictionary<string, RcObject> rcObjects);
    void AddEnemyTeamMember(string enemyTeamName, int enemyNumber, bool goalie);
    void ResetVisualPositions(int playerNumber);
    void SetVisualPosition(string objectName, Vector2 relativePos, float relativeBodyFacingDir);
    void SetUnknownPlayerPosition(Vector2 relativePos, float relativeBodyFacingDir, bool knownTeam = false, bool enemyTeam = false);
    void UpdateVisualPositions(int playerNumber);
}