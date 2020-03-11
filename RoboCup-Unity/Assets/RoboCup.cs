using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Unity.Mathematics;
using UnityEngine.Assertions.Comparers;

public class RoboCup : MonoBehaviour
{
    public static RoboCup singleton;

    string[] other = new[]
    {
        "F",
        "P"
    };

    string[] whiteFlags = new[]
    {
        "f t 0",
        "f c t",
        "f b 0",
        "f c b",
        "f b t",
        "f c"
    };
    
    string[] redFlags = new[]
    {
        "f r t",
        "f t r 10",
        "f t r 20",
        "f t r 30",
        "f t r 40",
        "f t r 50",
        "f r t 30",
        "f r t 20",
        "f r t 10",
        "f r t 0",
        "f r b",
        "f r b 10",
        "f r b 20",
        "f r b 30",
        "f p r t",
        "f p r c",
        "f p r b",
        "f g r t",
        "g r",
        "f g r b",
        "f b r 10",
        "f b r 20",
        "f b r 30",
        "f b r 40",
        "f b r 50",
        "f r 0"
    };
    
    string[] blueFlags = new[]
    {
        "f l t",
        "f t l 50",
        "f t l 40",
        "f t l 30",
        "f t l 20",
        "f t l 10",
        "f l t 30",
        "f l t 20",
        "f l t 10",
        "f l t 0",
        "f l b 10",
        "f l b 20",
        "f l b 30",
        "f p l t",
        "f p l c",
        "f p l b",
        "f g l t",
        "g l",
        "f g l b",
        "f l b",
        "f b l 50",
        "f b l 40",
        "f b l 30",
        "f b l 20",
        "f b l 10",
        "f l 0"
    };

    const int teamSize = 11;

    float visualScale = 6f;
    
    [Header("Settings")]
    public string ip = "127.0.0.1";
    public int port = 6000;
    public string teamName = "DefaultTeam";
    public bool singleplayer = false;

    int currentPlayer = 0;

    string enemyTeamName = "\"EnemyTeam\"";
    bool enemyTeamNameFound;
    
    List<RcPlayer> players = new List<RcPlayer>();

    [Header("References")]
    public TextMeshProUGUI currentPlayerText;
    public TextMeshProUGUI serverMessageText;
    public TextMeshProUGUI playerTypeText;
    public TextMeshProUGUI senseText;
    public TextMeshProUGUI seeText;
    public TextMeshProUGUI hearText;
    public RectTransform field;
    public Image selfPlayer;

    [Header("Prefabs")]
    GameObject playerPrefab;
    
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
    
    void Awake()
    {
        if (singleton == null)
            singleton = this;
        else
            Destroy(gameObject);

        playerPrefab          = Resources.Load<GameObject>("prefabs/RC Player");
        
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
        
        CreateVisualObject("b", visBall);
        CreateVisualObject("B", visBallClose);

        foreach (string o in other)
        {
            RectTransform rt = CreateVisualObject(o, visGeneric);
            rt.Find("Text").GetComponent<TextMeshProUGUI>().text = o;
        }
        
        foreach (string flagName in whiteFlags)
        {
            CreateVisualObject(flagName, visFlag);
        }
        
        foreach (string flagName in redFlags)
        {
            CreateVisualObject(flagName, visFlagRed);
        }
        
        foreach (string flagName in blueFlags)
        {
            CreateVisualObject(flagName, visFlagBlue);
        }

        for (int i = 0; i < teamSize+1; i++)
        {
            bool goalie = i == teamSize;
            string goalieText = goalie ? " goalie" : "";
            RectTransform rt = CreateVisualObject($"p \"{teamName}\" {i}{goalieText}", visPlayer);
            rt.Find("PlayerNumber").GetComponent<TextMeshProUGUI>().text = ""+ i;
            rt.Find("Goalie").gameObject.SetActive(goalie);
        }

        for (int i = 0; i < teamSize * 2; i++)
        {
            CreateUnknownPlayer();
        }
        
        for (int i = 0; i < teamSize; i++)
        {
            CreateUnknownPlayer(true, true);
        }
        
        for (int i = 0; i < teamSize; i++)
        {
            CreateUnknownPlayer(true, false);
        }
    }

    void Start()
    {
        StartCoroutine(CreatePlayers());
    }

    void Update()
    {
        // Player switching
        if (Input.GetKeyDown(KeyCode.Q))
            currentPlayer--;
        else if (Input.GetKeyDown(KeyCode.E))
            currentPlayer++;
        
        if (currentPlayer > teamSize-1)
            currentPlayer = 0;
        if (currentPlayer < 0)
            currentPlayer = teamSize-1;

        for (int i = 1; i < 10; i++)
        {
            if (Input.GetKeyDown(KeyCode.Alpha0 + i))
                currentPlayer = i-1;
        }

        if (Input.GetKeyDown(KeyCode.Alpha0))
            currentPlayer = 9;

        if (Input.GetKeyDown(KeyCode.G))
            currentPlayer = 10;

        currentPlayerText.text = $"Current Player: {currentPlayer+1} {(currentPlayer == 10 ? "(goalie)" : "")}";

        int dashAmount = (Input.GetKey(KeyCode.LeftShift)) ? 100 : 50;
        int turnAmount = (Input.GetKey(KeyCode.LeftShift)) ? 30 : 15;
        int kickAmount = (Input.GetKey(KeyCode.LeftShift)) ? 100 : 50;
        
        // Player control
        if (Input.GetKeyDown(KeyCode.Space))
            players[currentPlayer].Send($"(kick {kickAmount} 0)");

        else if (Input.GetKeyDown(KeyCode.LeftControl))
            players[currentPlayer].Send("(catch 0)");

        else if (Input.GetKeyDown(KeyCode.T))
            players[currentPlayer].Send("(tackle 0)");
        
        else if (Input.GetKey(KeyCode.LeftArrow))
            players[currentPlayer].Send($"(turn -{turnAmount})");

        else if (Input.GetKey(KeyCode.RightArrow))
            players[currentPlayer].Send($"(turn {turnAmount})");
        
        else if (Input.GetKey(KeyCode.A))
            players[currentPlayer].Send($"(dash {dashAmount} -90)");

        else if (Input.GetKey(KeyCode.D))
            players[currentPlayer].Send($"(dash {dashAmount} 90)");

        else if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
            players[currentPlayer].Send($"(dash {dashAmount})");

        else if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
            players[currentPlayer].Send($"(dash {dashAmount} 180)");
    }
    
    IEnumerator CreatePlayers()
    {
        if (singleplayer)
        {
            players.Add(CreatePlayer(0, false));
            yield return new WaitForSeconds(0.25f);
            players[0].Send("(move -50 20)");
        }
        else
        {
            for (int i = 0; i < teamSize-1; i++)
            {
                players.Add(CreatePlayer(i, false));
                yield return new WaitForSeconds(0.2f);
            }
        
            players.Add(CreatePlayer(teamSize-1, true));
            yield return new WaitForSeconds(0.25f);

            for (int i = 0; i < teamSize-1; i++)
            {
                players[i].Send($"(move -20 {i*5 - 30})");
                yield return new WaitForSeconds(0.2f);
            }
        
            players[teamSize-1].Send("(move -50 20)");
        }
    }

    RcPlayer CreatePlayer(int playerNumber, bool goalie)
    {
        GameObject p = Instantiate(playerPrefab);
        
        RcPlayer rcPlayer = p.GetComponent<RcPlayer>();
        rcPlayer.Init(playerNumber);
        
        string goalieString = (goalie) ? " (goalie)" : "";
        rcPlayer.Send($"(init {teamName} (version 16){goalieString})");

        return rcPlayer;
    }

    public void DisplayText(string txt, RcMessage.RcMessageType type)
    {
        switch (type)
        {
            case RcMessage.RcMessageType.PlayerType:
                playerTypeText.text = txt;
                break;
            case RcMessage.RcMessageType.Sense:
                senseText.text = txt;
                break;
            case RcMessage.RcMessageType.See:
                seeText.text = txt;
                break;
            case RcMessage.RcMessageType.Hear:
                hearText.text = txt;
                break;
            case RcMessage.RcMessageType.PlayerParam:
                //ignore
                break;
            case RcMessage.RcMessageType.ServerParam:
                //ignore
                break;
            case RcMessage.RcMessageType.Init:
                if (txt.Contains("r"))
                    selfPlayer.color = Color.red;
                else if (txt.Contains("l"))
                    selfPlayer.color = Color.blue;
                break;
            default:
                serverMessageText.text = txt;
                break;
        }
    }

    RectTransform CreateVisualObject(string objectName, GameObject prefab)
    {
        GameObject obj = Instantiate(prefab);

        RectTransform rt = obj.GetComponent<RectTransform>();
        rt.SetParent(field);
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
        rt.SetParent(field);
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
        if (subjectPlayer != currentPlayer)
            return;
        
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

    public void SetVisualPosition(int subjectPlayer, string objectName, float distance, float direction, float bodyFacingDir)
    {
        if (subjectPlayer != currentPlayer)
            return;
        
        float radDirection = Mathf.Deg2Rad * -(direction - 90f);
        float bodyDirection = -(bodyFacingDir);
        
        Vector2 newPos = visualScale * distance * new Vector2(math.cos(radDirection), math.sin(radDirection));

        bool uniqueObject = true;

        // object is a player
        if (objectName[0] == 'p')
        {
            // more info than just "p"
            if (objectName.Length > 1)
            {
                // not an already known specific player
                if (!visualObjects.ContainsKey(objectName))
                {
                    Regex regex = new Regex("p \"([/-_a-zA-Z0-9]+)\"(?: ([0-9]{1,2})( goalie)?)?");
                    
                    if (regex.Match(objectName).Success)
                    {
                        string tName = regex.Match(objectName).Result("$1");
 
                        // if there is a player number, assume enemy player and add to dict
                        if (regex.Match(objectName).Result("$2").Length > 0)
                        {
                            int enemyNumber = int.Parse(regex.Match(objectName).Result("$2"));
                            bool goalie = regex.Match(objectName).Result("$3").Length > 0;

                            RectTransform rt = CreateVisualObject(objectName, visPlayerEnemy);
                            rt.Find("PlayerNumber").GetComponent<TextMeshProUGUI>().text = ""+ enemyNumber;
                            rt.Find("Goalie").gameObject.SetActive(goalie);
                        }
                        else // if there is no player number call setposition method depending on team
                        {
                            uniqueObject = false;
                            
                            if (tName.Equals(teamName))
                                SetUnknownPlayerPosition(newPos, bodyDirection, true, true);
                            else
                                SetUnknownPlayerPosition(newPos, bodyDirection, true, false);
                        }
                    }
                    else
                    {
                        Debug.LogError($"regex did not match: {objectName}");
                    }
                }
            }
            else // unknown player
            {
                uniqueObject = false;
                SetUnknownPlayerPosition(newPos, bodyDirection);
            }
        }

        if (uniqueObject)
        {
            if (visualObjects.ContainsKey(objectName))
            {
                visualObjects[objectName].visibleThisStep = true;
                visualObjects[objectName].newPos = newPos;
                visualObjects[objectName].newDir = bodyDirection;
            }
            else
            {
                Debug.LogWarning($"unique object not in dict: {objectName}");
            }
        }
    }

    void SetUnknownPlayerPosition(Vector2 newPos, float bodyFacingDir, bool knownTeam = false, bool myTeam = false)
    {
        if (knownTeam)
        {
            if (myTeam)
            {
                unknownTeamPlayers[unknownTeamPlayerIndex].rt.gameObject.SetActive(true);
                unknownTeamPlayers[unknownTeamPlayerIndex].rt.anchoredPosition = newPos;
                unknownTeamPlayers[unknownTeamPlayerIndex].rt.localRotation = Quaternion.Euler(0,0,bodyFacingDir);

                unknownTeamPlayerIndex++;
            }
            else
            {
                unknownEnemyPlayers[unknownEnemyPlayerIndex].rt.gameObject.SetActive(true);
                unknownEnemyPlayers[unknownEnemyPlayerIndex].rt.anchoredPosition = newPos;
                unknownEnemyPlayers[unknownEnemyPlayerIndex].rt.localRotation = Quaternion.Euler(0,0,bodyFacingDir);

                unknownEnemyPlayerIndex++;
            }
        }
        else
        {
            unknownPlayers[unknownPlayerIndex].rt.gameObject.SetActive(true);
            unknownPlayers[unknownPlayerIndex].rt.anchoredPosition = newPos;
            unknownPlayers[unknownPlayerIndex].rt.localRotation = Quaternion.Euler(0,0,bodyFacingDir);

            unknownPlayerIndex++;
        }
    }
    
    public void UpdateVisualPositions(int subjectPlayer)
    {
        if (subjectPlayer != currentPlayer)
            return;
        
        foreach (VisualObject vObj in visualObjects.Values)
        {
            vObj.rt.gameObject.SetActive(vObj.visibleThisStep);

            if (vObj.visibleThisStep)
            {
                Transform dirTransform = vObj.rt.Find("Direction");
                
                if (vObj.visibleLastStep)
                {
                    StartCoroutine(SmoothMove(vObj.rt, vObj.newPos));
                    if (dirTransform != null)
                        dirTransform.localRotation = Quaternion.Euler(0,0,vObj.newDir);
                        //StartCoroutine(SmoothRotate(dirTransform, vObj.newDir));
                }
                else
                {
                    vObj.rt.anchoredPosition = vObj.newPos;
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
