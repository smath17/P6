using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

public class RoboCup : MonoBehaviour
{
    public enum RoboCupMode {ManualControlFullTeam, ManualControlSinglePlayer, Training, AgentSinglePlayer, Agent1V1, AgentFullTeam, Agent2Teams}
    
    public const int FullTeamSize = 11;
    public const int Port = 6000;
    public const int OfflineCoachPort = 6001;
    public const int OnlineCoachPort = 6002;
    
    public static RoboCup singleton;
    Visualizer3D visualizer;
    bool visualizerInitialized;

    [Header("Connection")]
    public string ip = "127.0.0.1";
    
    [Header("Team")]
    public string teamName = "DefaultTeam";
    public bool reconnect;

    [Header("Settings")]
    public RoboCupMode roboCupMode;
    public bool seriousMode = true;

    [Header("References")]
    public AgentTrainer agentTrainer;
    public OverlayInfo overlayInfo;
    public GameObject visualizerObject;
    public LocationCalculator locationCalculator;
    public GameObject agentPrefab;

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

    List<RcAgent> team1Agents = new List<RcAgent>();
    List<RcAgent> team2Agents = new List<RcAgent>();

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
        visualizer = visualizerObject.GetComponent<Visualizer3D>();
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
            
            case RoboCupMode.AgentSinglePlayer:
                team1Setup.Add(new PlayerSetupInfo(false, -20, 0));
                break;
            
            case RoboCupMode.Agent1V1:
                team1Setup.Add(new PlayerSetupInfo(false, -20, 0));
                team2Setup.Add(new PlayerSetupInfo(false, 20, 0));
                break;
            
            case RoboCupMode.AgentFullTeam:
                for (int i = 0; i < FullTeamSize-1; i++)
                    team1Setup.Add(new PlayerSetupInfo(false, -20, i*5 - 30));
        
                team1Setup.Add(new PlayerSetupInfo(true, -50, 20));
                break;
            
            case RoboCupMode.Agent2Teams:
                for (int i = 0; i < FullTeamSize-1; i++)
                    team1Setup.Add(new PlayerSetupInfo(false, -20, i*5 - 30));
        
                team1Setup.Add(new PlayerSetupInfo(true, -50, 20));
                
                for (int i = 0; i < FullTeamSize-1; i++)
                    team2Setup.Add(new PlayerSetupInfo(false, -20, i*5 - 30));
        
                team2Setup.Add(new PlayerSetupInfo(true, -50, 20));
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
            RcPlayer player = CreatePlayer(playerIndex, setupInfo.goalie, setupInfo.x, setupInfo.y, true, reconnect);
            team1.Add(player);
            playerIndex++;
            
            if (roboCupMode == RoboCupMode.AgentSinglePlayer ||
                roboCupMode == RoboCupMode.Agent1V1 ||
                roboCupMode == RoboCupMode.AgentFullTeam ||
                roboCupMode == RoboCupMode.Agent2Teams)
            {
                GameObject agentObj = Instantiate(agentPrefab);
                RcAgent agent = agentObj.GetComponent<RcAgent>();
                agent.SetPlayer(player);
                agent.SetRealMatch();
                agentObj.SetActive(true);
                
                team1Agents.Add(agent);
            }
            
            yield return new WaitForSeconds(0.2f);
        }
        
        playerIndex = 0;
        foreach (PlayerSetupInfo setupInfo in team2Setup)
        {
            RcPlayer player = CreatePlayer(playerIndex, setupInfo.goalie, setupInfo.x, setupInfo.y, false, reconnect);
            team2.Add(player);
            playerIndex++;
            
            if (roboCupMode == RoboCupMode.AgentSinglePlayer ||
                roboCupMode == RoboCupMode.Agent1V1 ||
                roboCupMode == RoboCupMode.AgentFullTeam ||
                roboCupMode == RoboCupMode.Agent2Teams)
            {
                GameObject agentObj = Instantiate(agentPrefab);
                RcAgent agent = agentObj.GetComponent<RcAgent>();
                agent.SetPlayer(player);
                agent.SetRealMatch();
                agentObj.SetActive(true);
                
                team2Agents.Add(agent);
            }
            
            yield return new WaitForSeconds(0.2f);
        }
        
        if (roboCupMode == RoboCupMode.Training)
            coach = CreateCoach(false, reconnect);
        
        if (roboCupMode == RoboCupMode.AgentSinglePlayer ||
            roboCupMode == RoboCupMode.Agent1V1 ||
            roboCupMode == RoboCupMode.AgentFullTeam ||
            roboCupMode == RoboCupMode.Agent2Teams)
            coach = CreateCoach(true, reconnect);
        
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
            
            case RoboCupMode.AgentSinglePlayer:
            case RoboCupMode.Agent1V1:
            case RoboCupMode.AgentFullTeam:
            case RoboCupMode.Agent2Teams:
                coach.InitMatch();
                break;
        }
        
        OnSwitchPlayer();
    }

    RcPlayer CreatePlayer(int playerNumber, bool goalie, int x, int y, bool mainTeam = true, bool reconnect = false)
    {
        GameObject p = Instantiate(playerPrefab);
        p.name = $"RcPlayer_{(mainTeam ? "L" : "R")}_{playerNumber}";
        
        RcPlayer rcPlayer = p.GetComponent<RcPlayer>();
        rcPlayer.Init(mainTeam, playerNumber, goalie, x, y, reconnect, playerNumber);

        return rcPlayer;
    }
    
    RcCoach CreateCoach(bool online, bool reconnect = false)
    {
        GameObject c = Instantiate(coachPrefab);
        
        RcCoach rcCoach = c.GetComponent<RcCoach>();
        rcCoach.Init(online);
        
        if (online)
            rcCoach.Send($"({(reconnect ? "reconnect" : "init")} {teamName} (version 16))");
        else
            rcCoach.Send($"({(reconnect ? "reconnect" : "init")} (version 16))");

        return rcCoach;
    }

    void Update()
    {
        PlayerSwitching();
        
        switch (roboCupMode)
        {
            case RoboCupMode.ManualControlFullTeam:
            case RoboCupMode.ManualControlSinglePlayer:
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

    public void StepAgents()
    {
        for (int i = 0; i < team1Agents.Count; i++)
        {
            RcPerceivedObject ball = team1[i].GetRcObject("b");
            if (ball != null)
               team1Agents[i].SetBallInfo(ball.curVisibility, ball.direction, ball.distance);

            RcPerceivedObject kickerGoalLeft = team1[i].GetRcObject("f g r t");
            RcPerceivedObject kickerGoalRight = team1[i].GetRcObject("f g r b");

            if (kickerGoalLeft != null && kickerGoalRight != null)
                team1Agents[i].SetGoalInfo(kickerGoalLeft.curVisibility, kickerGoalLeft.direction, kickerGoalRight.curVisibility, kickerGoalRight.direction);

            RcPerceivedObject kickerOwnGoal = team1[i].GetRcObject("g l");
            if (kickerOwnGoal != null)
                team1Agents[i].SetOwnGoalInfo(kickerOwnGoal.curVisibility, kickerOwnGoal.direction);
                
            RcPerceivedObject kickerLeftSide = team1[i].GetRcObject("f t 0");
            if (kickerLeftSide != null)
                team1Agents[i].SetLeftSideInfo(kickerLeftSide.curVisibility, kickerLeftSide.direction);
                
            RcPerceivedObject kickerRightSide = team1[i].GetRcObject("f b 0");
            if (kickerRightSide != null)
                team1Agents[i].SetRightSideInfo(kickerRightSide.curVisibility, kickerRightSide.direction);
                
            team1Agents[i].SetSelfInfo(team1[i].GetKickBallCount());
            team1Agents[i].RequestDecision();
        }
        
        for (int i = 0; i < team2Agents.Count; i++)
        {
            RcPerceivedObject ball = team2[i].GetRcObject("b");
            if (ball != null)
                team2Agents[i].SetBallInfo(ball.curVisibility, ball.direction, ball.distance);

            RcPerceivedObject kickerGoalLeft = team2[i].GetRcObject("f g l b");
            RcPerceivedObject kickerGoalRight = team2[i].GetRcObject("f g l t");

            if (kickerGoalLeft != null && kickerGoalRight != null)
                team2Agents[i].SetGoalInfo(kickerGoalLeft.curVisibility, kickerGoalLeft.direction, kickerGoalRight.curVisibility, kickerGoalRight.direction);

            RcPerceivedObject kickerOwnGoal = team2[i].GetRcObject("g r");
            if (kickerOwnGoal != null)
                team2Agents[i].SetOwnGoalInfo(kickerOwnGoal.curVisibility, kickerOwnGoal.direction);
                
            RcPerceivedObject kickerLeftSide = team2[i].GetRcObject("f b 0");
            if (kickerLeftSide != null)
                team2Agents[i].SetLeftSideInfo(kickerLeftSide.curVisibility, kickerLeftSide.direction);
                
            RcPerceivedObject kickerRightSide = team2[i].GetRcObject("f t 0");
            if (kickerRightSide != null)
                team2Agents[i].SetRightSideInfo(kickerRightSide.curVisibility, kickerRightSide.direction);
                
            team2Agents[i].SetSelfInfo(team2[i].GetKickBallCount());
            team2Agents[i].RequestDecision();
        }
    }

    void PlayerSwitching()
    {
        if (Input.GetKeyDown(KeyCode.Z))
            PreviousPlayer();
        else if (Input.GetKeyDown(KeyCode.X))
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
            SwitchTeam();
        }
    }

    void PreviousPlayer()
    {
        currentPlayer--;
        if (currentPlayer < 0)
            currentPlayer = visualizeMainTeam ? team1.Count-1 : team2.Count-1;
        
        OnSwitchPlayer();
    }

    void NextPlayer()
    {
        currentPlayer++;
        if ((visualizeMainTeam && currentPlayer > team1.Count-1) || (!visualizeMainTeam && currentPlayer > team2.Count-1))
            currentPlayer = 0;
        
        OnSwitchPlayer();
    }

    void SwitchTeam()
    {
        if (team1.Count < 1 && team2.Count < 1)
            return;
        
        visualizeMainTeam = !visualizeMainTeam;

        if ((visualizeMainTeam && currentPlayer > team1.Count - 1) ||
            (!visualizeMainTeam && currentPlayer > team2.Count - 1))
            currentPlayer = visualizeMainTeam ? team1.Count - 1 : team2.Count - 1;
        
        OnSwitchPlayer();
    }

    void SwitchPlayer(int newPlayer)
    {
        currentPlayer = newPlayer;
        OnSwitchPlayer();
    }

    void OnSwitchPlayer()
    {
        overlayInfo.UpdateCurrentPlayerText(currentPlayer, visualizeMainTeam);

        RcPlayer newPlayer = visualizeMainTeam ? team1[currentPlayer] : team2[currentPlayer];
        
        visualizer.SetPlayer(newPlayer);
        visualizer.ResetVisualPositions(newPlayer);
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
                team1[currentPlayer].Dash(dashAmount, -90);

            else if (Input.GetKey(KeyCode.D))
                team1[currentPlayer].Dash(dashAmount, 90);

            else if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
                team1[currentPlayer].Dash(dashAmount, 0);

            else if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
                team1[currentPlayer].Dash(dashAmount, 180);
        }
        
        if (Input.GetKey(KeyCode.Space))
            team1[currentPlayer].Kick(kickAmount);

        else if (Input.GetKeyDown(KeyCode.LeftControl))
            team1[currentPlayer].Catch();

        else if (Input.GetKeyDown(KeyCode.T))
            team1[currentPlayer].Send("(tackle 0)");
    
        else if (Input.GetKey(KeyCode.LeftArrow))
            team1[currentPlayer].Turn(-turnAmount);

        else if (Input.GetKey(KeyCode.RightArrow))
            team1[currentPlayer].Turn(turnAmount);
    }
    
    public void ReceiveMessage(string txt, RcMessage.RcMessageType type)
    {
        overlayInfo.DisplayText(txt, type);
    }

    public Visualizer3D InitVisualizer(RcPlayer player)
    {
        if (!visualizerInitialized)
        {
            visualizer.SetPlayer(player);
            visualizer.Init();
            visualizerInitialized = true;
        }

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
