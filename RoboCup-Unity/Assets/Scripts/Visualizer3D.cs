using System.Collections.Generic;
using UnityEngine;

public class Visualizer3D : MonoBehaviour, IVisualizer
{
    public GameObject playerPrefab;
    public GameObject ballPrefab;
    public GameObject flagPrefab;
    public GameObject unknownPrefab;
    
    [Header("References")]
    public Transform objectParent;

    string teamName;
    int teamSize;
    bool rightTeam;
    
    Dictionary<string, VisualRcObject> visualObjects = new Dictionary<string, VisualRcObject>();
    
    Dictionary<string, ReferenceFlag> referenceFlags = new Dictionary<string, ReferenceFlag>();

    public void Init(string teamName, int teamSize, bool rightTeam, Dictionary<string, RcObject> rcObjects)
    {
        this.teamName = teamName;
        this.teamSize = teamSize;
        this.rightTeam = rightTeam;

        foreach (KeyValuePair<string,RcObject> rcObject in rcObjects)
        {
            GameObject prefab = unknownPrefab;
            
            switch (rcObject.Value.objectType)
            {
                case RcObject.RcObjectType.Unknown:
                    prefab = unknownPrefab;
                    break;
                case RcObject.RcObjectType.UnknownPlayer:
                case RcObject.RcObjectType.UnknownTeamPlayer:
                case RcObject.RcObjectType.UnknownEnemyPlayer:
                case RcObject.RcObjectType.TeamPlayer:
                case RcObject.RcObjectType.EnemyPlayer:
                    prefab = playerPrefab;
                    break;
                case RcObject.RcObjectType.Ball:
                case RcObject.RcObjectType.BallClose:
                    prefab = ballPrefab;
                    break;
                case RcObject.RcObjectType.Flag:
                case RcObject.RcObjectType.FlagClose:
                    prefab = flagPrefab;
                    break;
                default:
                    prefab = unknownPrefab;
                    break;
            }
            
            CreateVisualObject(rcObject.Value.name, prefab);
        }

        foreach (ReferenceFlag referenceFlag in FindObjectsOfType<ReferenceFlag>())
        {
            referenceFlag.flagName = referenceFlag.name;
            
            Vector3 pos3d = referenceFlag.transform.localPosition;
            referenceFlag.flagPos = new Vector2(pos3d.x, pos3d.z);
            
            referenceFlags.Add(referenceFlag.name, referenceFlag);
        }
    }
    
    void CreateVisualObject(string objectName, GameObject prefab)
    {
        GameObject obj = Instantiate(prefab);

        Transform t = obj.transform;
        t.SetParent(objectParent);
        
        VisualRcObject visualObject = new VisualRcObject(t); 
        
        visualObjects.Add(objectName, visualObject);
    }

    public void AddEnemyTeamMember(string enemyTeamName, int enemyNumber, bool goalie)
    {
        string goalieText = goalie ? " goalie" : "";
        string eName = $"p \"{enemyTeamName}\" {enemyNumber}{goalieText}";
        
        CreateVisualObject(eName, playerPrefab);
    }

    public void ResetVisualPositions(int playerNumber)
    {
        foreach (KeyValuePair<string,VisualRcObject> rcObject in visualObjects)
        {
            rcObject.Value.visible = false;
        }
    }

    public void SetVisualPosition(string objectName, Vector2 relativePos, float relativeBodyFacingDir)
    {
        if (visualObjects.ContainsKey(objectName))
        {
            visualObjects[objectName].visible = true;
            visualObjects[objectName].relativePos = relativePos;
        }
    }

    public void SetUnknownPlayerPosition(Vector2 relativePos, float relativeBodyFacingDir, bool knownTeam = false, bool enemyTeam = false)
    {
        //TODO: do do
    }

    public void UpdateVisualPositions(int playerNumber)
    {
        foreach (KeyValuePair<string,VisualRcObject> rcObject in visualObjects)
        {
            if (rcObject.Value.visible && !rcObject.Key.StartsWith("f"))
            {
                rcObject.Value.t.gameObject.SetActive(true);
                rcObject.Value.t.localPosition = new Vector3(rcObject.Value.relativePos.x, 0f, rcObject.Value.relativePos.y);
            }
            else
            {
                rcObject.Value.t.gameObject.SetActive(false);
            }
        }
        
        TrilateratePlayerPosition();
    }

    void TrilateratePlayerPosition()
    {
        List<ReferenceFlag> flagsForCalculation = new List<ReferenceFlag>();
        
        foreach (KeyValuePair<string,VisualRcObject> rcObject in visualObjects)
        {
            if (rcObject.Value.visible)
            {
                if (referenceFlags.ContainsKey(rcObject.Key) && referenceFlags[rcObject.Key].gameObject.activeInHierarchy)
                {
                    referenceFlags[rcObject.Key].relativePos = rcObject.Value.relativePos;
                    flagsForCalculation.Add(referenceFlags[rcObject.Key]);
                }
            }
        }

        if (flagsForCalculation.Count > 1)
        {
            float trueAngle = Vector2.SignedAngle(Vector2.up, flagsForCalculation[0].flagPos - flagsForCalculation[1].flagPos);
            float relativeAngle = Vector2.SignedAngle(Vector2.up, flagsForCalculation[0].relativePos - flagsForCalculation[1].relativePos);
            
            float angle = relativeAngle - trueAngle;
            objectParent.localRotation = Quaternion.Euler(0, angle, 0);

            if (flagsForCalculation.Count > 2)
            {
                objectParent.localPosition = Trilaterate(flagsForCalculation);
            }
            else
                objectParent.localPosition = Vector3.zero;
        }
        else
            objectParent.localPosition = Vector3.zero;
    }
    
    Vector3 Trilaterate(List<ReferenceFlag> flags)
    {
        Vector2 bestPosition = Vector2.zero;
        float lowestError = Mathf.Infinity;

        // minimize error
        for (float x = -55; x < 55; x++)
        {
            for (float y = -35; y < 35; y++)
            {
                Vector2 testPosition = new Vector2(x, y);
                float mse = MeanSquaredError(testPosition, flags);
                if (mse < lowestError)
                {
                    lowestError = mse;
                    bestPosition = testPosition;
                }
            }
        }
        
        return new Vector3(bestPosition.x, 0,bestPosition.y);
    }

    float MeanSquaredError(Vector2 position, List<ReferenceFlag> flags)
    {
        float mse = 0f;

        foreach (ReferenceFlag flag in flags)
        {
            float calculatedDistance = Vector2.Distance(position, flag.flagPos);
            float measuredDistance = Vector2.Distance(Vector2.zero, flag.relativePos);
            mse += Mathf.Pow(calculatedDistance - measuredDistance, 2f);
        }

        return mse / flags.Count;
    }
}

class VisualRcObject
{
    public Transform t;
    public bool visible;
    public Vector2 relativePos;
    public float relativeBodyFacingDir;

    public VisualRcObject(Transform t)
    {
        this.t = t;
        visible = false;
        relativePos = Vector2.zero;
        relativeBodyFacingDir = 0f;
    }
}