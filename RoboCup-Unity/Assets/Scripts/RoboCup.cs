using System.Collections;
using System.Collections.Generic;
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
    public GameObject team1AgentPrefab;
    public bool team1AgentsEnabled = true;
    public GameObject team2AgentPrefab;
    public bool team2AgentsEnabled = true;
    public AgentTrainer.TrainingScenario trainingScenario;
    public bool seriousMode = true;

    [Header("References")]
    public AgentTrainer agentTrainer;
    public OverlayInfo overlayInfo;
    public GameObject visualizerObject;
    public LocationCalculator locationCalculator;

    // Tickrate (for manual control)
    float currentTick = 0f;
    float tickTime = 0.1f;

    // Enemy team
    string enemyTeamName = "EnemyTeam";
    bool enemyTeamNameFound;
    
    // Player and coach objects
    GameObject playerPrefab;
    GameObject coachPrefab;
    List<RcPlayer> team1 = new List<RcPlayer>();
    List<RcPlayer> team2 = new List<RcPlayer>();
    RcCoach coach;
    
    // Team setup information
    List<PlayerSetupInfo> team1Setup = new List<PlayerSetupInfo>();
    List<PlayerSetupInfo> team2Setup = new List<PlayerSetupInfo>();

    // Agent objects
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

    bool trainerReady = false;
    
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
        // Setup team compositions
        switch (roboCupMode)
        {
            case RoboCupMode.ManualControlFullTeam:
            case RoboCupMode.AgentFullTeam:
                // Full team of players (1 goalie)
                for (int i = 0; i < FullTeamSize-1; i++)
                    team1Setup.Add(new PlayerSetupInfo(false, -20, i*5 - 30));
        
                team1Setup.Add(new PlayerSetupInfo(true, -50, 20));
                break;
            
            case RoboCupMode.ManualControlSinglePlayer:
            case RoboCupMode.AgentSinglePlayer:
                // Single player
                team1Setup.Add(new PlayerSetupInfo(false, -20, 0));
                break;
            
            case RoboCupMode.Training:
                // Use AgentTrainer's teams
                agentTrainer.SetupTeams(trainingScenario);
                team1Setup = agentTrainer.GetTeam1Setup();
                team2Setup = agentTrainer.GetTeam2Setup();
                break;
            
            case RoboCupMode.Agent1V1:
                // 2 Teams of 1
                team1Setup.Add(new PlayerSetupInfo(false, -20, 0));
                team2Setup.Add(new PlayerSetupInfo(false, 20, 0));
                break;
            
            case RoboCupMode.Agent2Teams:
                // 2 Full teams (1 goalie each)
                for (int i = 0; i < FullTeamSize-1; i++)
                    team1Setup.Add(new PlayerSetupInfo(false, -20, i*5 - 30));
        
                team1Setup.Add(new PlayerSetupInfo(true, -50, 20));
                
                for (int i = 0; i < FullTeamSize-1; i++)
                    team2Setup.Add(new PlayerSetupInfo(false, -20, i*5 - 30));
        
                team2Setup.Add(new PlayerSetupInfo(true, -50, 20));
                break;
        }
        
        // Create players based on team setup, and create Coach
        StartCoroutine(Initialize());
    }
    
    IEnumerator Initialize()
    {
        // Setup team 1 Players
        int playerIndex = 0;
        foreach (PlayerSetupInfo setupInfo in team1Setup)
        {
            SetupPlayer(setupInfo, playerIndex, true);
            playerIndex++;
            yield return new WaitForSeconds(0.2f);
        }
        
        // Setup team 2 players
        playerIndex = 0;
        foreach (PlayerSetupInfo setupInfo in team2Setup)
        {
            SetupPlayer(setupInfo, playerIndex, false);
            playerIndex++;
            yield return new WaitForSeconds(0.2f);
        }
        
        // Setup offline coach for training
        if (roboCupMode == RoboCupMode.Training)
            coach = CreateCoach(false, reconnect);
        else // Setup online coach for regular match
            coach = CreateCoach(true, reconnect);
        
        yield return new WaitForSeconds(0.2f);

        coach.Init();
        
        yield return new WaitForSeconds(0.2f);
        
        if (roboCupMode == RoboCupMode.Training)
            agentTrainer.Init(team1Agents, team2Agents);
        else
        {
            foreach (RcAgent team1Agent in team1Agents)
            {
                team1Agent.Init(true);
            }
            foreach (RcAgent team2Agent in team2Agents)
            {
                team2Agent.Init(true);
            }
        }
        
        OnInitialized();
    }

    // Sets up a player and adds it to a team
    // (Also creates an RcAgent if applicable)
    void SetupPlayer(PlayerSetupInfo setupInfo, int playerNumber, bool mainTeam)
    {
        RcPlayer player = CreatePlayer(playerNumber, setupInfo.goalie, setupInfo.x, setupInfo.y, mainTeam, reconnect);
        
        if (mainTeam)
            team1.Add(player);
        else
            team2.Add(player);
            
        if (roboCupMode != RoboCupMode.ManualControlSinglePlayer && roboCupMode != RoboCupMode.ManualControlFullTeam)
        {
            GameObject agentObj = Instantiate(mainTeam ? team1AgentPrefab : team2AgentPrefab);
            RcAgent agent = agentObj.GetComponent<RcAgent>();
            agent.SetPlayer(player);
            agent.SetAgentTrainer(agentTrainer);
            
            if (mainTeam)
                team1Agents.Add(agent);
            else
                team2Agents.Add(agent);
        }
    }
    
    // Creates an RcPlayer which corresponds to a player in the Soccer Simulator
    RcPlayer CreatePlayer(int playerNumber, bool goalie, int x, int y, bool mainTeam = true, bool reconnect = false)
    {
        GameObject p = Instantiate(playerPrefab);
        p.name = $"RcPlayer_{(mainTeam ? "L" : "R")}_{playerNumber}";
        
        RcPlayer rcPlayer = p.GetComponent<RcPlayer>();
        rcPlayer.Init(mainTeam, playerNumber, goalie, x, y, reconnect, playerNumber);

        return rcPlayer;
    }
    
    // Creates an RcCoach which corresponds to a coach in the Soccer Simulator
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

    void OnInitialized()
    {
        OnSwitchPlayer();
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
        }
    }

    // Used to update agents
    public void StepAgents()
    {
        bool requestDecisions = true;
        
        if (roboCupMode == RoboCupMode.Training)
        {
            if (!trainerReady)
                return;
            
            // if TrainingStep returns true, request decision
            requestDecisions = agentTrainer.TrainingStep(team1Agents, team2Agents);
        }
        
        // Team 1 agents
        if (team1AgentsEnabled)
        {
            using (System.IO.StreamWriter file =
                new System.IO.StreamWriter(@"C:\Users\mrynk\Documents\GitHub\P6\team1.txt", true))
            {
                file.WriteLine(team1[0].GetStamina().ToString("0.#"));
            }
            for (int i = 0; i < team1Agents.Count; i++)
            {
                if (requestDecisions)
                    StepAgent(team1Agents[i], team1[i], true);
            }
        }
        
        // Team 2 agents
        if (team2AgentsEnabled)
        {
            for (int i = 0; i < team2Agents.Count; i++)
            {
                if (requestDecisions)
                    StepAgent(team2Agents[i], team2[i], false);
            }
        }
    }

    // Updates agent observations, manages stamina, and requests new decisions
    void StepAgent(RcAgent agent, RcPlayer player, bool mainTeam)
    {
        // Update info used for observations
        
        RcPerceivedObject ball = player.GetRcObject("b");
        if (ball != null)
            agent.SetBallInfo(ball.curVisibility, ball.direction, ball.distance);

        RcPerceivedObject goalLeft = player.GetRcObject(mainTeam ? "f g r t" : "f g l b");
        RcPerceivedObject goalRight = player.GetRcObject(mainTeam ? "f g r b" : "f g l t");

        if (goalLeft != null && goalRight != null)
            agent.SetGoalInfo(goalLeft.curVisibility, goalLeft.direction,
                goalRight.curVisibility, goalRight.direction);

        RcPerceivedObject ownGoal = player.GetRcObject(mainTeam ? "g l" : "g r");
        if (ownGoal != null)
            agent.SetOwnGoalInfo(ownGoal.curVisibility, ownGoal.direction);

        RcPerceivedObject leftSide = player.GetRcObject(mainTeam ? "f t 0" : "f b 0");
        if (leftSide != null)
            agent.SetLeftSideInfo(leftSide.curVisibility, leftSide.direction);

        RcPerceivedObject rightSide = player.GetRcObject(mainTeam ? "f b 0" : "f t 0");
        if (rightSide != null)
            agent.SetRightSideInfo(rightSide.curVisibility, rightSide.direction);

        agent.SetSelfInfo(player.GetKickBallCount());

        
        // Manage stamina for the current RcPlayer
        StaminaManagement(player, ball);
            
        // Check if time to recover or new action
        if (!player.recovering)
            agent.RequestDecision();
    }

    // Stamina management only for trained models
    void StaminaManagement(RcPlayer player, RcPerceivedObject ball)
    {
        player.recovering = false;
        
        // Recover after goal
        if (player.GetGameStatus().StartsWith("goal_l") || player.GetGameStatus().StartsWith("goal_r") || player.recoverAtStart)
        {
            // recover at start if under 3000 stamina
            player.recoverAtStart = player.GetStamina() < 3000;
            // Recover stamina == no action request
            player.recovering = true;
        }

        // Recover depending on ball pos
        if (ball != null)
            // Add up distance
            player.recoverDistanceToBall += ball.distance;
        // If sum distance is above 100, recover stamina
        if (player.recoverDistanceToBall > 100 && player.GetStamina() < 8000)
        {
            player.recoverDistanceToBall -= 100;
            // Recover stamina == no action request
            player.recovering = true;
        }
    }

    // Switching player viewpoint (and control if using manual control)
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

    // Manual player control
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

    public void SetTrainerReady()
    {
        trainerReady = true;
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
