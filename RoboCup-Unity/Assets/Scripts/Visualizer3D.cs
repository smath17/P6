using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Visualizer3D : MonoBehaviour
{
    [Header("Settings")]
    public bool interpolate;
    public bool showFlags;
    
    [Header("Prefabs")]
    public GameObject unknownPrefab;

    public GameObject flagPrefab;
    public GameObject ballPrefab;

    public GameObject teamPlayerPrefab;
    public GameObject enemyPlayerPrefab;
    public GameObject unknownPlayerPrefab;
    
    [Header("References")]
    public Transform objectParent;
    public Transform unknownObjectParent;
    
    Dictionary<string, VisualRcObject> visualObjects = new Dictionary<string, VisualRcObject>();
    
    List<VisualRcObject> unknownPlayers = new List<VisualRcObject>();
    List<VisualRcObject> unknownTeamPlayers = new List<VisualRcObject>();
    List<VisualRcObject> unknownEnemyPlayers = new List<VisualRcObject>();

    int unknownPlayerIndex;
    int unknownTeamPlayerIndex;
    int unknownEnemyPlayerIndex;

    RcPlayer currentPlayer;

    public void Init()
    {
        foreach (KeyValuePair<string, RcPerceivedObject> rcObject in currentPlayer.GetRcObjects())
        {
            GameObject prefab = unknownPrefab;
            
            switch (rcObject.Value.objectType)
            {
                case RcPerceivedObject.RcObjectType.Unknown:
                    prefab = unknownPrefab;
                    break;
                case RcPerceivedObject.RcObjectType.UnknownPlayer:
                    prefab = unknownPlayerPrefab;
                    break;
                case RcPerceivedObject.RcObjectType.TeamPlayer:
                case RcPerceivedObject.RcObjectType.UnknownTeamPlayer:
                    prefab = teamPlayerPrefab;
                    break;
                case RcPerceivedObject.RcObjectType.EnemyPlayer:
                case RcPerceivedObject.RcObjectType.UnknownEnemyPlayer:
                    prefab = enemyPlayerPrefab;
                    break;
                case RcPerceivedObject.RcObjectType.Ball:
                case RcPerceivedObject.RcObjectType.BallClose:
                    prefab = ballPrefab;
                    break;
                case RcPerceivedObject.RcObjectType.Flag:
                case RcPerceivedObject.RcObjectType.FlagClose:
                    prefab = flagPrefab;
                    break;
                default:
                    prefab = unknownPrefab;
                    break;
            }
            
            Transform t = CreateVisualObject(rcObject.Value.name, prefab);

            if (t != null && rcObject.Value.playerNumber != 0)
            {
                string goalieText = rcObject.Value.goalie ? " G" : "";
                t.Find("text").GetComponent<TextMeshPro>().text = rcObject.Value.playerNumber + goalieText;
            }
        }
        
        for (int i = 0; i < RoboCup.FullTeamSize * 2; i++)
        {
            CreateUnknownPlayer(false);
        }
        
        for (int i = 0; i < RoboCup.FullTeamSize; i++)
        {
            CreateUnknownPlayer(true);
        }
        
        for (int i = 0; i < RoboCup.FullTeamSize; i++)
        {
            CreateUnknownPlayer(true, true);
        }
    }

    public void SetPlayer(RcPlayer player)
    {
        currentPlayer = player;
    }

    Transform CreateVisualObject(string objectName, GameObject prefab)
    {
        if (visualObjects.ContainsKey(objectName))
            return null;
        
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
        t.SetParent(unknownObjectParent);

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

    public void AddEnemyTeamMember(RcPlayer player, string enemyTeamName, int enemyNumber, bool goalie)
    {
        if (currentPlayer == null)
            return;
        
        if (currentPlayer != player)
            return;
        
        string goalieText = goalie ? " goalie" : "";
        string eName = $"p \"{enemyTeamName}\" {enemyNumber}{goalieText}";
        
        Transform t = CreateVisualObject(eName, enemyPlayerPrefab);

        if (t != null)
        {
            string gText = goalie ? " G" : "";
            t.Find("text").GetComponent<TextMeshPro>().text = enemyNumber + gText;
        }
    }

    public void ResetVisualPositions(RcPlayer player)
    {
        if (currentPlayer == null)
            return;
        
        if (currentPlayer != player)
            return;
        
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

    public void SetVisualPosition(RcPlayer player, string objectName, Vector2 relativePos, float relativeBodyFacingDir)
    {
        if (currentPlayer == null)
            return;
        
        if (currentPlayer != player)
            return;
        
        if (visualObjects.ContainsKey(objectName))
        {
            visualObjects[objectName].curVisibility = true;
            visualObjects[objectName].curRelativePos = relativePos;
        }
    }

    public void SetUnknownPlayerPosition(RcPlayer player, Vector2 relativePos, float relativeBodyFacingDir, bool knownTeam = false, bool enemyTeam = false)
    {
        if (currentPlayer == null)
            return;
        
        if (currentPlayer != player)
            return;
        
        Transform t = null;
        
        if (knownTeam)
        {
            if (enemyTeam)
            {
                if (unknownEnemyPlayerIndex > unknownEnemyPlayers.Count - 1)
                    unknownEnemyPlayerIndex = 0;
                t = unknownEnemyPlayers[unknownEnemyPlayerIndex].t;
                unknownEnemyPlayerIndex++;
            }
            else
            {
                if (unknownTeamPlayerIndex > unknownTeamPlayers.Count-1)
                    unknownTeamPlayerIndex = 0;
                t = unknownTeamPlayers[unknownTeamPlayerIndex].t;
                unknownTeamPlayerIndex++;
            }
        }
        else
        {
            if (unknownPlayerIndex > unknownPlayers.Count-1)
                unknownPlayerIndex = 0;
            t = unknownPlayers[unknownPlayerIndex].t;
            unknownPlayerIndex++;
        }
        
        t.gameObject.SetActive(true);
        t.localPosition = new Vector3(relativePos.x, 0f, relativePos.y);
        t.localRotation = Quaternion.Euler(0,0,relativeBodyFacingDir);
    }

    public void UpdateVisualPositions(RcPlayer player)
    {
        if (currentPlayer == null)
            return;
        
        if (currentPlayer != player)
            return;
        
        foreach (KeyValuePair<string,VisualRcObject> rcObject in visualObjects)
        {
            if (rcObject.Value.curVisibility && (!rcObject.Key.StartsWith("f") || showFlags))
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

        float curPlayerAngle = currentPlayer.GetCalculatedAngle();
        float prevPlayerAngle = currentPlayer.GetCalculatedAngle(true);
        
        Vector2 curPlayerPosition = currentPlayer.GetCalculatedPosition();
        Vector2 prevPlayerPosition = currentPlayer.GetCalculatedPosition(true);
        
        if (interpolate && currentPlayer.ReadyToInterpolate())
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
        
        unknownObjectParent.localPosition = new Vector3(curPlayerPosition.x, 0f, curPlayerPosition.y);
        unknownObjectParent.localRotation = Quaternion.Euler(0, curPlayerAngle, 0);
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