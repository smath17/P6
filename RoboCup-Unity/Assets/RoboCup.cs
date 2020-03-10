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
        "f b l 10"
    };

    const int teamSize = 11;

    float visualScale = Screen.height / 150f;
    
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

    [Header("Prefabs")]
    public GameObject playerPrefab;
    public GameObject playerEnemyPrefab;
    public GameObject visualPlayerPrefab;
    public GameObject visualBallPrefab;
    public GameObject visualFlagPrefab;
    public GameObject visualFlagRedPrefab;
    public GameObject visualFlagBluePrefab;
    
    Dictionary<string, VisualObject> visualObjects = new Dictionary<string, VisualObject>();
    
    void Awake()
    {
        if (singleton == null)
            singleton = this;
        else
            Destroy(gameObject);
        
        CreateVisualObject("b", visualBallPrefab);

        foreach (string flagName in whiteFlags)
        {
            CreateVisualObject(flagName, visualFlagPrefab);
        }
        
        foreach (string flagName in redFlags)
        {
            CreateVisualObject(flagName, visualFlagRedPrefab);
        }
        
        foreach (string flagName in blueFlags)
        {
            CreateVisualObject(flagName, visualFlagBluePrefab);
        }

        for (int i = 0; i < 11; i++)
        {
            RectTransform rt = CreateVisualObject($"p \"{teamName}\" {i}", visualPlayerPrefab);
            rt.Find("PlayerNumber").GetComponent<TextMeshProUGUI>().text = ""+i;
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

    public void ResetVisualPositions(int subjectPlayer)
    {
        if (subjectPlayer != currentPlayer)
            return;
        
        foreach (VisualObject vObj in visualObjects.Values)
        {
            vObj.visibleLastStep = vObj.visibleThisStep;
            vObj.visibleThisStep = false;
            vObj.lastPos = vObj.newPos;
        }
    }

    public void SetVisualPosition(int subjectPlayer, string objectName, float distance, float angle)
    {
        if (subjectPlayer != currentPlayer)
            return;
        
        float radians = Mathf.Deg2Rad * -(angle - 90f);
        
        Vector2 newPos = visualScale * distance * new Vector2(math.cos(radians), math.sin(radians));
        //Debug.Log(newPos);

        if (objectName.StartsWith("p "))
        {
            if (!objectName.StartsWith("p \"" + teamName + "\""))
            {
                if (!visualObjects.ContainsKey(objectName))
                {
                    Regex regex = new Regex("p \"[0-z]*\" ([0-9]{1,2})( goalie)?");
                    if (regex.Match(objectName).Success)
                    {
                        int enemyNumber = int.Parse(regex.Match(objectName).Result("$1"));
                        bool goalie = regex.Match(objectName).Result($"2").Length > 0;

                        RectTransform rt = CreateVisualObject(objectName, playerEnemyPrefab);
                        rt.Find("PlayerNumber").GetComponent<TextMeshProUGUI>().text = ""+ enemyNumber + ((goalie) ? " G" : "");
                    }
                }
            }
        }

        if (visualObjects.ContainsKey(objectName))
        {
            visualObjects[objectName].visibleThisStep = true;
            visualObjects[objectName].newPos = newPos;
        }
        else
        {
            Debug.LogWarning(objectName);
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
                if (vObj.visibleLastStep)
                    StartCoroutine(SmoothMove(vObj.rt, vObj.newPos));
                else
                    vObj.rt.anchoredPosition = vObj.newPos;
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
}

class VisualObject
{
    public RectTransform rt;
    public bool visibleLastStep;
    public bool visibleThisStep;
    public Vector2 lastPos;
    public Vector2 newPos;

    public VisualObject(RectTransform rt)
    {
        this.rt = rt;
        visibleLastStep = false;
        visibleThisStep = false;
        lastPos = Vector2.zero;
        newPos = Vector2.zero;
    }
}
