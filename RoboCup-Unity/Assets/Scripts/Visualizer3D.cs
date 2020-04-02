using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class Visualizer3D : MonoBehaviour, IVisualizer
{
    public GameObject objectPrefab;
    
    [Header("References")]
    public Transform objectParent;

    string teamName;
    int teamSize;
    bool rightTeam;
    
    Dictionary<string, Transform> visualObjects = new Dictionary<string, Transform>();
    
    public void Init(string teamName, int teamSize, bool rightTeam, Dictionary<string, RcObject> rcObjects)
    {
        this.teamName = teamName;
        this.teamSize = teamSize;
        this.rightTeam = rightTeam;

        foreach (KeyValuePair<string,RcObject> keyValuePair in rcObjects)
        {
            CreateVisualObject(keyValuePair.Value.name, objectPrefab);
        }
    }
    
    void CreateVisualObject(string objectName, GameObject prefab)
    {
        GameObject obj = Instantiate(prefab);

        Transform t = obj.transform;
        t.SetParent(objectParent);
        
        visualObjects.Add(objectName, t);
    }

    public void AddEnemyTeamMember(string enemyTeamName, int enemyNumber, bool goalie)
    {
        string goalieText = goalie ? " goalie" : "";
        string eName = $"p \"{enemyTeamName}\" {enemyNumber}{goalieText}";
        
        CreateVisualObject(eName, objectPrefab);
    }

    public void ResetVisualPositions(int playerNumber)
    {
        foreach (KeyValuePair<string,Transform> keyValuePair in visualObjects)
        {
            keyValuePair.Value.gameObject.SetActive(false);
        }
    }

    public void SetVisualPosition(string objectName, Vector2 relativePos, float relativeBodyFacingDir)
    {
        if (visualObjects.ContainsKey(objectName))
        {
            visualObjects[objectName].gameObject.SetActive(true);
            visualObjects[objectName].localPosition = new Vector3(relativePos.x, 0, relativePos.y);
        }
    }

    public void SetUnknownPlayerPosition(Vector2 relativePos, float relativeBodyFacingDir, bool knownTeam = false, bool enemyTeam = false)
    {
        //TODO: do do
    }

    public void UpdateVisualPositions(int playerNumber)
    {
        //TODO: do do
    }
}
