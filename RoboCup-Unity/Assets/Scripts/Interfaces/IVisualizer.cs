using System.Collections.Generic;
using UnityEngine;

public interface IVisualizer
{
    void Init();
    void SetPlayer(RcPlayer player);
    void AddEnemyTeamMember(RcPlayer player, string enemyTeamName, int enemyNumber, bool goalie);
    void ResetVisualPositions(RcPlayer player);
    void SetVisualPosition(RcPlayer player, string objectName, Vector2 relativePos, float relativeBodyFacingDir);
    void SetUnknownPlayerPosition(RcPlayer player, Vector2 relativePos, float relativeBodyFacingDir, bool knownTeam = false, bool enemyTeam = false);
    void UpdateVisualPositions(RcPlayer player);
}