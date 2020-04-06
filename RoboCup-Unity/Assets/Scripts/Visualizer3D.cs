using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Visualizer3D : MonoBehaviour, IVisualizer
{
    public GameObject unknownPrefab;

    public GameObject flagPrefab;
    public GameObject ballPrefab;

    public GameObject teamPlayerPrefab;
    public GameObject enemyPlayerPrefab;
    public GameObject unknownPlayerPrefab;

    public bool interpolate;
    
    [Header("References")]
    public Transform objectParent;

    string teamName;
    int teamSize;
    bool rightTeam;
    
    Dictionary<string, VisualRcObject> visualObjects = new Dictionary<string, VisualRcObject>();
    
    Dictionary<string, ReferenceFlag> referenceFlags = new Dictionary<string, ReferenceFlag>();
    
    List<VisualRcObject> unknownPlayers = new List<VisualRcObject>();
    List<VisualRcObject> unknownTeamPlayers = new List<VisualRcObject>();
    List<VisualRcObject> unknownEnemyPlayers = new List<VisualRcObject>();

    int unknownPlayerIndex;
    int unknownTeamPlayerIndex;
    int unknownEnemyPlayerIndex;
    
    Vector2 prevPlayerPosition = Vector2.zero;
    Vector2 curPlayerPosition = Vector2.zero;
    float prevPlayerAngle = 0f;
    float curPlayerAngle = 0f;
    
    bool positionWasKnown;
    bool positionIsKnown;
    bool angleWasKnown;
    bool angleIsKnown;

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
                    prefab = unknownPlayerPrefab;
                    break;
                case RcObject.RcObjectType.TeamPlayer:
                case RcObject.RcObjectType.UnknownTeamPlayer:
                    prefab = teamPlayerPrefab;
                    break;
                case RcObject.RcObjectType.EnemyPlayer:
                case RcObject.RcObjectType.UnknownEnemyPlayer:
                    prefab = enemyPlayerPrefab;
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
            
            Transform t = CreateVisualObject(rcObject.Value.name, prefab);

            if (rcObject.Value.playerNumber != 0)
            {
                string goalieText = rcObject.Value.goalie ? " G" : "";
                t.Find("text").GetComponent<TextMeshPro>().text = rcObject.Value.playerNumber + goalieText;
            }
        }

        foreach (ReferenceFlag referenceFlag in FindObjectsOfType<ReferenceFlag>())
        {
            referenceFlag.flagName = referenceFlag.name;
            
            Vector3 pos3d = referenceFlag.transform.localPosition;
            referenceFlag.flagPos = new Vector2(pos3d.x, pos3d.z);
            
            referenceFlags.Add(referenceFlag.name, referenceFlag);
        }
        
        for (int i = 0; i < teamSize * 2; i++)
        {
            CreateUnknownPlayer(false);
        }
        
        for (int i = 0; i < teamSize; i++)
        {
            CreateUnknownPlayer(true);
        }
        
        for (int i = 0; i < teamSize; i++)
        {
            CreateUnknownPlayer(true, true);
        }
    }
    
    Transform CreateVisualObject(string objectName, GameObject prefab)
    {
        GameObject obj = Instantiate(prefab);

        Transform t = obj.transform;
        t.SetParent(objectParent);
        
        VisualRcObject visualObject = new VisualRcObject(t); 
        
        visualObjects.Add(objectName, visualObject);

        return t;
    }
    
    void CreateUnknownPlayer(bool knownTeam = false, bool myTeam = false)
    {
        GameObject obj;
        
        if (knownTeam)
        {
            if (myTeam)
                obj = Instantiate(teamPlayerPrefab);
            else
                obj = Instantiate(enemyPlayerPrefab);
        }
        else
            obj = Instantiate(unknownPlayerPrefab);

        Transform t = obj.transform;
        t.SetParent(objectParent);

        VisualRcObject visualObject = new VisualRcObject(t); 

        if (knownTeam)
        {
            if (myTeam)
                unknownTeamPlayers.Add(visualObject);
            else
                unknownEnemyPlayers.Add(visualObject);
        }
        else
            unknownPlayers.Add(visualObject);
    }

    public void AddEnemyTeamMember(string enemyTeamName, int enemyNumber, bool goalie)
    {
        string goalieText = goalie ? " goalie" : "";
        string eName = $"p \"{enemyTeamName}\" {enemyNumber}{goalieText}";
        
        Transform t = CreateVisualObject(eName, enemyPlayerPrefab);

        string gText = goalie ? " G" : "";
        t.Find("text").GetComponent<TextMeshPro>().text = enemyNumber + gText;
    }

    public void ResetVisualPositions(int playerNumber)
    {
        foreach (KeyValuePair<string,VisualRcObject> rcObject in visualObjects)
        {
            rcObject.Value.prevRelativePos = rcObject.Value.curRelativePos;
            rcObject.Value.prevRelativeBodyFacingDir = rcObject.Value.curRelativeBodyFacingDir;

            rcObject.Value.prevVisibility = rcObject.Value.curVisibility;
            rcObject.Value.curVisibility = false;
        }

        unknownPlayerIndex = 0;
        foreach (VisualRcObject unknownPlayer in unknownPlayers)
        {
            unknownPlayer.t.gameObject.SetActive(false);
        }
        
        unknownTeamPlayerIndex = 0;
        foreach (VisualRcObject unknownTeamPlayer in unknownTeamPlayers)
        {
            unknownTeamPlayer.t.gameObject.SetActive(false);
        }
        
        unknownEnemyPlayerIndex = 0;
        foreach (VisualRcObject unknownEnemyPlayer in unknownEnemyPlayers)
        {
            unknownEnemyPlayer.t.gameObject.SetActive(false);
        }
    }

    public void SetVisualPosition(string objectName, Vector2 relativePos, float relativeBodyFacingDir)
    {
        if (visualObjects.ContainsKey(objectName))
        {
            visualObjects[objectName].curVisibility = true;
            visualObjects[objectName].curRelativePos = relativePos;
        }
    }

    public void SetUnknownPlayerPosition(Vector2 relativePos, float relativeBodyFacingDir, bool knownTeam = false, bool enemyTeam = false)
    {
        Transform t = null;
        
        if (knownTeam)
        {
            if (enemyTeam)
            {
                t = unknownEnemyPlayers[unknownEnemyPlayerIndex].t;
                unknownEnemyPlayerIndex++;
            }
            else
            {
                t = unknownTeamPlayers[unknownTeamPlayerIndex].t;
                unknownTeamPlayerIndex++;
            }
        }
        else
        {
            t = unknownPlayers[unknownPlayerIndex].t;
            unknownPlayerIndex++;
        }
        
        t.gameObject.SetActive(true);
        t.localPosition = new Vector3(relativePos.x, 0f, relativePos.y);
        t.localRotation = Quaternion.Euler(0,0,relativeBodyFacingDir);
    }

    public void UpdateVisualPositions(int playerNumber)
    {
        foreach (KeyValuePair<string,VisualRcObject> rcObject in visualObjects)
        {
            if (rcObject.Value.curVisibility && !rcObject.Key.StartsWith("f"))
            {
                rcObject.Value.t.gameObject.SetActive(true);

                if (interpolate && rcObject.Value.prevVisibility)
                {
                    StartCoroutine(
                        InterpolateObject(rcObject.Value.t,
                            rcObject.Value.prevRelativePos, rcObject.Value.curRelativePos,
                            rcObject.Value.prevRelativeBodyFacingDir, rcObject.Value.curRelativeBodyFacingDir)
                    );
                }
                else
                {
                    rcObject.Value.t.localPosition = new Vector3(rcObject.Value.curRelativePos.x, 0f, rcObject.Value.curRelativePos.y);
                }
            }
            else
            {
                rcObject.Value.t.gameObject.SetActive(false);
            }
        }
        
        TrilateratePlayerPosition();

        if (interpolate && positionWasKnown && positionIsKnown && angleWasKnown && angleIsKnown)
        {
            StartCoroutine(
                InterpolateObject(objectParent,
                    prevPlayerPosition, curPlayerPosition,
                    prevPlayerAngle, curPlayerAngle)
                );
        }
        else
        {
            objectParent.localPosition = new Vector3(curPlayerPosition.x, 0f, curPlayerPosition.y);
            objectParent.localRotation = Quaternion.Euler(0, curPlayerAngle, 0);
        }
    }
    
    IEnumerator InterpolateObject(Transform obj, Vector2 prevPos, Vector2 curPos, float prevAngle, float curAngle)
    {
        float t = 0;
        float duration = 0.1f;
        float mult = 1 / duration;

        while (t < duration)
        {
            t += Time.deltaTime;
            
            Vector2 pos = Vector2.Lerp(prevPos, curPos, t * mult);
            float angle = Mathf.LerpAngle(prevAngle, curAngle, t * mult);
            
            obj.localPosition = new Vector3(pos.x, 0f, pos.y);
            obj.localRotation = Quaternion.Euler(0, angle, 0);
            
            yield return null;
        }
    }

    void TrilateratePlayerPosition()
    {
        prevPlayerPosition = curPlayerPosition;
        prevPlayerAngle = curPlayerAngle;
        
        positionWasKnown = positionIsKnown;
        angleWasKnown = angleIsKnown;
        
        positionIsKnown = false;
        angleIsKnown = false;
        
        List<ReferenceFlag> flagsForCalculation = new List<ReferenceFlag>();
        
        foreach (KeyValuePair<string,VisualRcObject> rcObject in visualObjects)
        {
            if (rcObject.Value.curVisibility)
            {
                if (referenceFlags.ContainsKey(rcObject.Key) && referenceFlags[rcObject.Key].gameObject.activeInHierarchy)
                {
                    referenceFlags[rcObject.Key].relativePos = rcObject.Value.curRelativePos;
                    flagsForCalculation.Add(referenceFlags[rcObject.Key]);
                }
            }
        }

        if (flagsForCalculation.Count > 1)
        {
            float trueAngle = Vector2.SignedAngle(Vector2.up, flagsForCalculation[0].flagPos - flagsForCalculation[1].flagPos);
            float relativeAngle = Vector2.SignedAngle(Vector2.up, flagsForCalculation[0].relativePos - flagsForCalculation[1].relativePos);

            curPlayerAngle = relativeAngle - trueAngle;
            angleIsKnown = true;

            if (flagsForCalculation.Count > 2)
            {
                curPlayerPosition = Trilaterate(flagsForCalculation);
            }
            //else
            //    playerPosition = Vector2.zero;
        }
        //else
        //    playerPosition = Vector2.zero;
    }
    
    Vector2 Trilaterate(List<ReferenceFlag> flags)
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
                    positionIsKnown = true;
                }
            }
        }
        
        return bestPosition;
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

    public bool prevVisibility;
    public bool curVisibility;

    public Vector2 prevRelativePos;
    public Vector2 curRelativePos;

    public float prevRelativeBodyFacingDir;
    public float curRelativeBodyFacingDir;

    public VisualRcObject(Transform t)
    {
        this.t = t;
        prevVisibility = false;
        curVisibility = false;
        prevRelativePos = Vector2.zero;
        curRelativePos = Vector2.zero;
        prevRelativeBodyFacingDir = 0f;
        curRelativeBodyFacingDir = 0f;
    }
}