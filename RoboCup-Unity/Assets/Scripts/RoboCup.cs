using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

public class RoboCup : MonoBehaviour
{
    public enum RoboCupMode {ManualControlFullTeam, ManualControlSinglePlayer, Training}
    
    public const int FullTeamSize = 11;
    public const int Port = 6000;
    public const int OfflineCoachPort = 6001;
    public const int OnlineCoachPort = 6002;
    
    public static RoboCup singleton;
    IVisualizer visualizer;
    bool visualizerInitialized;

    [Header("Connection")]
    public string ip = "127.0.0.1";
    
    [Header("Team")]
    public string teamName = "DefaultTeam";

    [Header("Settings")]
    public RoboCupMode roboCupMode;

    [Header("References")]
    public AgentTrainer agentTrainer;
    public OverlayInfo overlayInfo;
    public GameObject visualizerObject;
    public LocationCalculator locationCalculator;

    // Tickrate
    float currentTick = 0f;
    float tickTime = 0.1f;

    // Enemy team
    string enemyTeamName = "EnemyTeam";
    bool enemyTeamNameFound;
    
    // Player objects
    GameObject playerPrefab;
    GameObject coachPrefab;
    List<RcPlayer> team1 = new List<RcPlayer>();
    List<RcPlayer> team2 = new List<RcPlayer>();
    
    RcCoach coach;
    
    //Dictionary<string, RcObject> rcObjects = new Dictionary<string, RcObject>();
    
    List<PlayerSetupInfo> team1Setup = new List<PlayerSetupInfo>();
    List<PlayerSetupInfo> team2Setup = new List<PlayerSetupInfo>();

    #region FlagNames
    public static string[] other = new[]
    {
        "F",
        "P"
    };

    public static string[] whiteFlags = new[]
    {
        "f t 0",
        "f c t",
        "f b 0",
        "f c b",
        "f b t",
        "f c"
    };
    
    public static string[] redFlags = new[]
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
    
    public static string[] blueFlags = new[]
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
    #endregion

    bool visualizeMainTeam = true;
    int currentPlayer = 0;
    bool sendDashInput;
    
    void Awake()
    {
        if (singleton == null)
            singleton = this;
        else
            Destroy(gameObject);

        playerPrefab = Resources.Load<GameObject>("prefabs/RC Player");
        coachPrefab = Resources.Load<GameObject>("prefabs/RC Coach");
        visualizer = visualizerObject.GetComponent<IVisualizer>();
    }
    
    void Start()
    {
        // Setup teams
        switch (roboCupMode)
        {
            case RoboCupMode.ManualControlFullTeam:
                for (int i = 0; i < FullTeamSize-1; i++)
                    team1Setup.Add(new PlayerSetupInfo(false, -20, i*5 - 30));
        
                team1Setup.Add(new PlayerSetupInfo(true, -50, 20));
                break;
            
            case RoboCupMode.ManualControlSinglePlayer:
                team1Setup.Add(new PlayerSetupInfo(false, -20, 0));
                break;
            
            case RoboCupMode.Training:
                agentTrainer.SetupTeams();
                team1Setup = agentTrainer.GetTeam1Setup();
                team2Setup = agentTrainer.GetTeam2Setup();
                break;
        }
        
        // Create players based on setup
        StartCoroutine(CreatePlayers());
    }
    
    IEnumerator CreatePlayers()
    {
        int playerIndex = 0;
        foreach (PlayerSetupInfo setupInfo in team1Setup)
        {
            team1.Add(CreatePlayer(playerIndex, setupInfo.goalie, setupInfo.x, setupInfo.y, true));
            playerIndex++;
            yield return new WaitForSeconds(0.2f);
        }
        
        playerIndex = 0;
        foreach (PlayerSetupInfo setupInfo in team2Setup)
        {
            team2.Add(CreatePlayer(playerIndex, setupInfo.goalie, setupInfo.x, setupInfo.y, false));
            playerIndex++;
            yield return new WaitForSeconds(0.2f);
        }
        
        if (roboCupMode == RoboCupMode.Training)
            coach = CreateCoach(false);
        
        yield return new WaitForSeconds(0.2f);
        
        OnInitialized();
    }

    void OnInitialized()
    {
        switch (roboCupMode)
        {
            case RoboCupMode.ManualControlFullTeam:
            case RoboCupMode.ManualControlSinglePlayer:
                break;
            
            case RoboCupMode.Training:
                agentTrainer.Init();
                break;
        }
    }

    RcPlayer CreatePlayer(int playerNumber, bool goalie, int x, int y, bool mainTeam = true)
    {
        GameObject p = Instantiate(playerPrefab);
        
        RcPlayer rcPlayer = p.GetComponent<RcPlayer>();
        rcPlayer.Init(mainTeam, playerNumber, goalie, x, y);

        return rcPlayer;
    }
    
    RcCoach CreateCoach(bool online)
    {
        GameObject c = Instantiate(coachPrefab);
        
        RcCoach rcCoach = c.GetComponent<RcCoach>();
        rcCoach.Init(online);
        
        if (online)
            rcCoach.Send($"(init {teamName} (version 16))");
        else
            rcCoach.Send($"(init (version 16))");

        return rcCoach;
    }

    void Update()
    {
        switch (roboCupMode)
        {
            case RoboCupMode.ManualControlFullTeam:
            case RoboCupMode.ManualControlSinglePlayer:
                PlayerSwitching();

                if (Time.time > currentTick + tickTime)
                {
                    currentTick = Time.time;
            
                    PlayerControl();
                }
                break;
            
            case RoboCupMode.Training:
                break;
        }
    }

    void PlayerSwitching()
    {
        if (Input.GetKeyDown(KeyCode.Q))
            PreviousPlayer();
        else if (Input.GetKeyDown(KeyCode.E))
            NextPlayer();

        for (int i = 1; i < 10; i++)
            if (Input.GetKeyDown(KeyCode.Alpha0 + i) || Input.GetKeyDown(KeyCode.Keypad0 + i))
                SwitchPlayer(i-1);

        if (Input.GetKeyDown(KeyCode.Alpha0) || Input.GetKeyDown(KeyCode.Keypad0))
            SwitchPlayer(9);

        if (Input.GetKeyDown(KeyCode.G))
            SwitchPlayer(10);

        if (Input.GetKeyDown(KeyCode.T))
        {
            visualizeMainTeam = !visualizeMainTeam;
            OnSwitchPlayer();
        }
    }

    void PreviousPlayer()
    {
        currentPlayer--;
        if (currentPlayer < 0)
            currentPlayer = FullTeamSize-1;
        
        OnSwitchPlayer();
    }

    void NextPlayer()
    {
        currentPlayer++;
        if (currentPlayer > FullTeamSize-1)
            currentPlayer = 0;
        
        OnSwitchPlayer();
    }

    void SwitchPlayer(int newPlayer)
    {
        currentPlayer = newPlayer;
        OnSwitchPlayer();
    }

    void OnSwitchPlayer()
    {
        overlayInfo.UpdateCurrentPlayerText(currentPlayer);
        visualizer.ResetVisualPositions();
    }

    void PlayerControl()
    {
        sendDashInput = !sendDashInput;
        
        int dashAmount = (Input.GetKey(KeyCode.LeftShift)) ? 100 : 50;
        int turnAmount = (Input.GetKey(KeyCode.LeftShift)) ? 30 : 10;
        int kickAmount = (Input.GetKey(KeyCode.LeftShift)) ? 100 : 50;

        if (sendDashInput)
        {
            if (Input.GetKey(KeyCode.A))
                team1[currentPlayer].Send($"(dash {dashAmount} -90)");

            else if (Input.GetKey(KeyCode.D))
                team1[currentPlayer].Send($"(dash {dashAmount} 90)");

            else if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
                team1[currentPlayer].Send($"(dash {dashAmount})");

            else if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
                team1[currentPlayer].Send($"(dash {dashAmount} 180)");
        }
        
        if (Input.GetKeyDown(KeyCode.Space))
            team1[currentPlayer].Send($"(kick {kickAmount} 0)");

        else if (Input.GetKeyDown(KeyCode.LeftControl))
            team1[currentPlayer].Send("(catch 0)");

        else if (Input.GetKeyDown(KeyCode.T))
            team1[currentPlayer].Send("(tackle 0)");
    
        else if (Input.GetKey(KeyCode.LeftArrow))
            team1[currentPlayer].Send($"(turn -{turnAmount})");

        else if (Input.GetKey(KeyCode.RightArrow))
            team1[currentPlayer].Send($"(turn {turnAmount})");
    }
    
    public void ReceiveMessage(string txt, RcMessage.RcMessageType type)
    {
        overlayInfo.DisplayText(txt, type);
    }

    public void InitVisualizer(RcPlayer player)
    {
        if (!visualizerInitialized)
        {
            visualizer.SetPlayer(player);
            visualizer.Init();
            visualizerInitialized = true;
        }
    }

    public void ResetVisualPositions(bool mainTeam, int playerNumber)
    {
        if (visualizeMainTeam == mainTeam && playerNumber == currentPlayer)
        {
            visualizer.ResetVisualPositions();
        }
    }

    public void UpdateVisualPositions(bool mainTeam, int playerNumber)
    {
        if (visualizeMainTeam == mainTeam && playerNumber == currentPlayer)
        {
            visualizer.UpdateVisualPositions();

            // Online training
            if (roboCupMode == RoboCupMode.Training)
            {
                agentTrainer.Step();
            }
        }
    }

    public IVisualizer GetVisualizer()
    {
        return visualizer;
    }

    public RcPlayer GetPlayer(int number, bool mainTeam = true)
    {
        return mainTeam ? team1[number] : team2[number];
    }
    
    public RcCoach GetCoach()
    {
        return coach;
    }

    public string GetTeamName()
    {
        return teamName;
    }
    
    public string GetEnemyTeamName()
    {
        return enemyTeamName;
    }

    public void SetTeamName(string newName)
    {
        teamName = newName;
    }

    public void SetEnemyTeamName(string newName)
    {
        enemyTeamName = newName;
    }
}

public class PlayerSetupInfo
{
    public bool goalie;
    public int x;
    public int y;

    public PlayerSetupInfo(bool goalie, int x, int y)
    {
        this.goalie = goalie;
        this.x = x;
        this.y = y;
    }
}

public class RcObject
{
    public enum RcObjectType
    {
        Unknown,
        UnknownPlayer,
        UnknownTeamPlayer,
        UnknownEnemyPlayer,
        TeamPlayer,
        EnemyPlayer,
        Ball,
        BallClose,
        Flag,
        FlagClose
    }

    public string name;
    public RcObjectType objectType;

    public string teamName;
    public int playerNumber;
    public bool goalie;

    public bool prevVisibility;
    public bool curVisibility;
    
    public float distance;
    public float direction;
    
    public Vector2 prevRelativePos;
    public Vector2 curRelativePos;

    public float prevRelativeBodyFacingDir;
    public float curRelativeBodyFacingDir;

    public RcObject(string name, RcObjectType objectType)
    {
        this.name = name;
        this.objectType = objectType;
    }
    public RcObject(RcObjectType objectType, string teamName, int playerNumber, bool goalie)
    {
        this.objectType = objectType;
        
        this.teamName = teamName;
        this.playerNumber = playerNumber;
        this.goalie = goalie;
        
        string goalieText = goalie ? " goalie" : "";
        name = $"p \"{teamName}\" {playerNumber}{goalieText}";
    }
}
