using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Unity.Mathematics;

public class RoboCup : MonoBehaviour
{
    public static RoboCup singleton;

    const int teamSize = 11;
    
    [Header("Settings")]
    public string ip = "127.0.0.1";
    public int port = 6000;
    public string teamName = "DefaultTeam";
    public bool singleplayer = false;

    int currentPlayer = 0;

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
    public GameObject visualPlayerPrefab;


    List<RectTransform> visualPlayers = new List<RectTransform>();
    
    void Awake()
    {
        if (singleton == null)
            singleton = this;
        else
            Destroy(gameObject);

        for (int i = 0; i < 64; i++)
        {
            GameObject obj = Instantiate(visualPlayerPrefab);

            RectTransform rt = obj.GetComponent<RectTransform>();
            rt.SetParent(field);
            rt.anchoredPosition = Vector2.zero;
            
            //obj.GetComponent<Image>().color = random color
            
            visualPlayers.Add(rt);
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

    public void SetVisualPosition(float distance, float angle)
    {
        float radians = Mathf.Deg2Rad * -(angle - 90f);
        
        Vector2 newPos = 10f * distance * new Vector2(math.cos(radians), math.sin(radians));
        //Debug.Log(newPos);
        visualPlayers[0].anchoredPosition = newPos;
    }
}
