using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Visualizer3D : MonoBehaviour, IVisualizer
{
    public void Init(string teamName, int teamSize, bool rightTeam, Dictionary<string, RcObject> rcObjects)
    {
        throw new System.NotImplementedException();
    }

    public void AddEnemyTeamMember(string enemyTeamName, int enemyNumber, bool goalie)
    {
        throw new System.NotImplementedException();
    }

    public void ResetVisualPositions(int playerNumber)
    {
        throw new System.NotImplementedException();
    }

    public void SetVisualPosition(string objectName, Vector2 relativePos, float relativeBodyFacingDir)
    {
        throw new System.NotImplementedException();
    }

    public void SetUnknownPlayerPosition(Vector2 relativePos, float relativeBodyFacingDir, bool knownTeam = false, bool enemyTeam = false)
    {
        throw new System.NotImplementedException();
    }

    public void UpdateVisualPositions(int playerNumber)
    {
        throw new System.NotImplementedException();
    }
}
