using System.Collections.Generic;
using UnityEngine;

public interface IVisualizer
{
    void Init();
    void SetPlayer(RcPlayer player);
    void AddEnemyTeamMember(string enemyTeamName, int enemyNumber, bool goalie);
    void ResetVisualPositions();
    void SetVisualPosition(string objectName, Vector2 relativePos, float relativeBodyFacingDir);
    void SetUnknownPlayerPosition(Vector2 relativePos, float relativeBodyFacingDir, bool knownTeam = false, bool enemyTeam = false);
    void UpdateVisualPositions();
}