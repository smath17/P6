using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Visualizer2D : MonoBehaviour, IVisualizer
{
    [Header("References")]
    public Image selfPlayer;

    GameObject visGeneric;
    GameObject visPlayer;
    GameObject visPlayerEnemy;
    GameObject visPlayerUnknown;
    GameObject visPlayerUnknownTeam;
    GameObject visPlayerUnknownEnemy;
    GameObject visBall;
    GameObject visBallClose;
    GameObject visFlag;
    GameObject visFlagRed;
    GameObject visFlagBlue;
    
    Dictionary<string, VisualObject> visualObjects = new Dictionary<string, VisualObject>();

    List<VisualObject> unknownPlayers = new List<VisualObject>();
    List<VisualObject> unknownTeamPlayers = new List<VisualObject>();
    List<VisualObject> unknownEnemyPlayers = new List<VisualObject>();
    
    int unknownPlayerIndex;
    int unknownTeamPlayerIndex;
    int unknownEnemyPlayerIndex;
    
    float visualScale = 6f;

    string teamName;

    public void Init(string teamName, int teamSize, bool rightTeam, Dictionary<string, RcObject> rcObjects)
    {
        visGeneric            = Resources.Load<GameObject>("prefabs/visual/generic");
        visPlayer             = Resources.Load<GameObject>("prefabs/visual/player");
        visPlayerEnemy        = Resources.Load<GameObject>("prefabs/visual/playerEnemy");
        visPlayerUnknown      = Resources.Load<GameObject>("prefabs/visual/playerUnknown");
        visPlayerUnknownTeam  = Resources.Load<GameObject>("prefabs/visual/playerUnknownTeam");
        visPlayerUnknownEnemy = Resources.Load<GameObject>("prefabs/visual/playerUnknownEnemy");
        visBall               = Resources.Load<GameObject>("prefabs/visual/ball");
        visBallClose          = Resources.Load<GameObject>("prefabs/visual/ballClose");
        visFlag               = Resources.Load<GameObject>("prefabs/visual/flag");
        visFlagRed            = Resources.Load<GameObject>("prefabs/visual/flagRed");
        visFlagBlue           = Resources.Load<GameObject>("prefabs/visual/flagBlue");
        
        this.teamName = teamName;
        
        if (rightTeam)
            selfPlayer.color = Color.red;
        else
            selfPlayer.color = Color.blue;
        
        foreach (KeyValuePair<string,RcObject> keyValuePair in rcObjects)
        {
            switch (keyValuePair.Value.objectType)
            {
                case RcObject.RcObjectType.Unknown:
                    RectTransform rt = CreateVisualObject(keyValuePair.Value.name, visGeneric);
                    rt.Find("Text").GetComponent<TextMeshProUGUI>().text = keyValuePair.Value.name;
                    break;
                
                case RcObject.RcObjectType.UnknownPlayer:
                    CreateUnknownPlayer();
                    break;
                
                case RcObject.RcObjectType.UnknownTeamPlayer:
                    CreateUnknownPlayer(true, true);
                    break;
                
                case RcObject.RcObjectType.UnknownEnemyPlayer:
                    CreateUnknownPlayer(true, false);
                    break;
                
                case RcObject.RcObjectType.TeamPlayer:
                    RectTransform teamPlayerRt = CreateVisualObject(keyValuePair.Value.name, visPlayer);
                    teamPlayerRt.Find("PlayerNumber").GetComponent<TextMeshProUGUI>().text = ""+ keyValuePair.Value.playerNumber;
                    teamPlayerRt.Find("Goalie").gameObject.SetActive(keyValuePair.Value.goalie);
                    break;
                
                case RcObject.RcObjectType.EnemyPlayer:
                    break;
                
                case RcObject.RcObjectType.Ball:
                    CreateVisualObject("b", visBall);
                    break;
                
                case RcObject.RcObjectType.BallClose:
                    CreateVisualObject("B", visBallClose);
                    break;
                
                case RcObject.RcObjectType.Flag:
                    if (keyValuePair.Value.name.Contains("r"))
                        CreateVisualObject(keyValuePair.Value.name, visFlagRed);
                    else if (keyValuePair.Value.name.Contains("b"))
                        CreateVisualObject(keyValuePair.Value.name, visFlagBlue);
                    else
                        CreateVisualObject(keyValuePair.Value.name, visFlag);
                    break;
                
                default:
                    throw new ArgumentOutOfRangeException();
            }
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

    public void AddEnemyTeamMember(string enemyTeamName, int enemyNumber, bool goalie)
    {
        string goalieText = goalie ? " goalie" : "";
        string eName = $"p \"{enemyTeamName}\" {enemyNumber}{goalieText}";
        
        RectTransform enemyPlayerRt = CreateVisualObject(eName, visPlayerEnemy);
        enemyPlayerRt.Find("PlayerNumber").GetComponent<TextMeshProUGUI>().text = ""+ enemyNumber;
        enemyPlayerRt.Find("Goalie").gameObject.SetActive(goalie);
    }

    public void UpdateVisualPositions(int subjectPlayer)
    {
        foreach (VisualObject vObj in visualObjects.Values)
        {
            vObj.rt.gameObject.SetActive(vObj.visibleThisStep);

            if (vObj.visibleThisStep)
            {
                Transform dirTransform = vObj.rt.Find("Direction");
                
                if (vObj.visibleLastStep)
                {
                    StartCoroutine(SmoothMove(vObj.rt, vObj.newPos * visualScale));
                    if (dirTransform != null)
                        dirTransform.localRotation = Quaternion.Euler(0,0,vObj.newDir);
                        //StartCoroutine(SmoothRotate(dirTransform, vObj.newDir));
                }
                else
                {
                    vObj.rt.anchoredPosition = vObj.newPos * visualScale;
                    if (dirTransform != null)
                        dirTransform.localRotation = Quaternion.Euler(0,0,vObj.newDir);
                }
            }
        }
    }

    IEnumerator SmoothMove(RectTransform rect, Vector2 newPos)
    {
        Vector2 origPos = rect.anchoredPosition;

        float t = 0;
        float duration = 0.1f;
        float mult = 1 / duration;

        while (t < duration)
        {
            t += Time.deltaTime;
            rect.anchoredPosition = Vector2.Lerp(origPos, newPos, t * mult);
            yield return null;
        }
    }
    
    IEnumerator SmoothRotate(Transform dirTransform, float bodyFacingDir)
    {
        float origDir = dirTransform.localRotation.eulerAngles.z;

        float t = 0;
        float duration = 0.1f;
        float mult = 1 / duration;

        while (t < duration)
        {
            t += Time.deltaTime;
            dirTransform.localRotation = Quaternion.Euler(0, 0, Mathf.Lerp(origDir, bodyFacingDir, t * mult));
            yield return null;
        }
    }

    RectTransform CreateVisualObject(string objectName, GameObject prefab)
    {
        GameObject obj = Instantiate(prefab);

        RectTransform rt = obj.GetComponent<RectTransform>();
        rt.SetParent(transform);
        rt.anchoredPosition = Vector2.zero;
        
        VisualObject vObj = new VisualObject(rt);
        
        visualObjects.Add(objectName, vObj);
        return rt;
    }
    
    RectTransform CreateUnknownPlayer(bool knownTeam = false, bool myTeam = false)
    {
        GameObject obj;
        
        if (knownTeam)
        {
            if (myTeam)
                obj = Instantiate(visPlayerUnknownTeam);
            else
                obj = Instantiate(visPlayerUnknownEnemy);
        }
        else
            obj = Instantiate(visPlayerUnknown);

        RectTransform rt = obj.GetComponent<RectTransform>();
        rt.SetParent(transform);
        rt.anchoredPosition = Vector2.zero;
        
        VisualObject vObj = new VisualObject(rt);

        if (knownTeam)
        {
            if (myTeam)
                unknownTeamPlayers.Add(vObj);
            else
                unknownEnemyPlayers.Add(vObj);
        }
        else
            unknownPlayers.Add(vObj);
        return rt;
    }

    public void ResetVisualPositions(int subjectPlayer)
    {
        foreach (VisualObject vObj in visualObjects.Values)
        {
            vObj.visibleLastStep = vObj.visibleThisStep;
            vObj.visibleThisStep = false;
            vObj.lastPos = vObj.newPos;
            vObj.lastDir = vObj.newDir;
        }

        unknownPlayerIndex = 0;
        unknownTeamPlayerIndex = 0;
        unknownEnemyPlayerIndex = 0;
        
        foreach (VisualObject unknownPlayer in unknownPlayers)
        {
            unknownPlayer.rt.gameObject.SetActive(false);
        }
        
        foreach (VisualObject unknownTeamPlayer in unknownTeamPlayers)
        {
            unknownTeamPlayer.rt.gameObject.SetActive(false);
        }
        
        foreach (VisualObject unknownEnemyPlayer in unknownEnemyPlayers)
        {
            unknownEnemyPlayer.rt.gameObject.SetActive(false);
        }
    }

    public void SetVisualPosition(string objectName, Vector2 relativePos, float relativeBodyFacingDir)
    {
        visualObjects[objectName].visibleThisStep = true;
        visualObjects[objectName].newPos = relativePos;
        visualObjects[objectName].newDir = relativeBodyFacingDir;
    }

    public void SetUnknownPlayerPosition(Vector2 relativePos, float relativeBodyFacingDir, bool knownTeam = false, bool enemyTeam = false)
    {
        if (knownTeam)
        {
            if (enemyTeam)
            {
                unknownEnemyPlayers[unknownEnemyPlayerIndex].rt.gameObject.SetActive(true);
                unknownEnemyPlayers[unknownEnemyPlayerIndex].rt.anchoredPosition = relativePos * visualScale;
                unknownEnemyPlayers[unknownEnemyPlayerIndex].rt.localRotation = Quaternion.Euler(0,0,relativeBodyFacingDir);

                unknownEnemyPlayerIndex++;
            }
            else
            {
                unknownTeamPlayers[unknownTeamPlayerIndex].rt.gameObject.SetActive(true);
                unknownTeamPlayers[unknownTeamPlayerIndex].rt.anchoredPosition = relativePos * visualScale;
                unknownTeamPlayers[unknownTeamPlayerIndex].rt.localRotation = Quaternion.Euler(0,0,relativeBodyFacingDir);

                unknownTeamPlayerIndex++;
            }
        }
        else
        {
            unknownPlayers[unknownPlayerIndex].rt.gameObject.SetActive(true);
            unknownPlayers[unknownPlayerIndex].rt.anchoredPosition = relativePos * visualScale;
            unknownPlayers[unknownPlayerIndex].rt.localRotation = Quaternion.Euler(0,0,relativeBodyFacingDir);

            unknownPlayerIndex++;
        }
    }
}

class VisualObject
{
    public RectTransform rt;
    public bool visibleLastStep;
    public bool visibleThisStep;
    public Vector2 lastPos;
    public Vector2 newPos;
    public float lastDir;
    public float newDir;

    public VisualObject(RectTransform rt)
    {
        this.rt = rt;
        visibleLastStep = false;
        visibleThisStep = false;
        lastPos = Vector2.zero;
        newPos = Vector2.zero;
    }
}