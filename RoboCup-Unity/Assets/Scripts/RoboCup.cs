using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

public class RoboCup : MonoBehaviour
{
    const int teamSize = 11;
    
    public static RoboCup singleton;
    IVisualizer visualizer;
    bool visualizerInitialized;

    [Header("Settings")]
    public string ip = "127.0.0.1";
    public int port = 6000;
    public string teamName = "DefaultTeam";
    public bool singleplayer;
    public bool trainingMode;
    
    [Header("References")]
    public OverlayInfo overlayInfo;
    public GameObject visualizerObject;
    public RoboCupAgent agent;

    // Tickrate
    float currentTick = 0f;
    float tickTime = 0.1f;

    // Enemy team
    string enemyTeamName = "EnemyTeam";
    bool enemyTeamNameFound;
    
    // Player objects
    GameObject playerPrefab;
    List<RcPlayer> players = new List<RcPlayer>();
    
    Dictionary<string, RcObject> rcObjects = new Dictionary<string, RcObject>();

    #region FlagNames
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
    #endregion

    int currentPlayer = 0;
    bool sendDashInput;
    
    void Awake()
    {
        if (singleton == null)
            singleton = this;
        else
            Destroy(gameObject);
        
        playerPrefab = Resources.Load<GameObject>("prefabs/RC Player");
        visualizer = visualizerObject.GetComponent<IVisualizer>();
        
        CreateRcObjects();
    }

    void CreateRcObjects()
    {
        CreateRcObject("b", RcObject.RcObjectType.Ball);
        CreateRcObject("B", RcObject.RcObjectType.BallClose);

        foreach (string o in other)
        {
            CreateRcObject(o, RcObject.RcObjectType.Flag);
        }
        
        foreach (string flagName in whiteFlags)
        {
            CreateRcObject(flagName, RcObject.RcObjectType.Flag);
        }
        
        foreach (string flagName in redFlags)
        {
            CreateRcObject(flagName, RcObject.RcObjectType.Flag);
        }
        
        foreach (string flagName in blueFlags)
        {
            CreateRcObject(flagName, RcObject.RcObjectType.Flag);
        }

        for (int i = 0; i < teamSize+1; i++)
        {
            bool goalie = i == teamSize;
            CreatePlayerRcObject(false, i, goalie);
        }
    }

    void CreateRcObject(string objectName, RcObject.RcObjectType objectType)
    {
        rcObjects.Add(objectName, new RcObject(objectName, objectType));
    }

    void CreatePlayerRcObject(bool enemyTeam, int playerNumber, bool goalie)
    {
        RcObject.RcObjectType objectType = enemyTeam ? RcObject.RcObjectType.EnemyPlayer : RcObject.RcObjectType.TeamPlayer;
        string tName = enemyTeam ? enemyTeamName : teamName;
        RcObject rcPlayerObj = new RcObject(objectType, tName, playerNumber, goalie);
        rcObjects.Add(rcPlayerObj.name, rcPlayerObj);
        rcObjects[rcPlayerObj.name].isVisible = true;
    }

    void Start()
    {
        StartCoroutine(CreatePlayers());
    }
    
    IEnumerator CreatePlayers()
    {
        if (singleplayer || trainingMode)
        {
            players.Add(CreatePlayer(0, false));
            yield return new WaitForSeconds(0.25f);
            if (trainingMode)
            {
                agent.SetPlayer(players[0]);
                agent.gameObject.SetActive(true);
                //agent.OnEpisodeBegin();
            }
            else
                players[0].Send("(move -20 0)");
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

    void Update()
    {
        PlayerSwitching();

        if (Time.time > currentTick + tickTime)
        {
            currentTick = Time.time;
            
            PlayerControl();
        }
    }

    void PlayerSwitching()
    {
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
            if (Input.GetKeyDown(KeyCode.Alpha0 + i) || Input.GetKeyDown(KeyCode.Keypad0 + i))
                currentPlayer = i-1;
        }

        if (Input.GetKeyDown(KeyCode.Alpha0) || Input.GetKeyDown(KeyCode.Keypad0))
            currentPlayer = 9;

        if (Input.GetKeyDown(KeyCode.G))
            currentPlayer = 10;

        overlayInfo.UpdateCurrentPlayerText(currentPlayer);
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
                players[currentPlayer].Send($"(dash {dashAmount} -90)");

            else if (Input.GetKey(KeyCode.D))
                players[currentPlayer].Send($"(dash {dashAmount} 90)");

            else if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
                players[currentPlayer].Send($"(dash {dashAmount})");

            else if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
                players[currentPlayer].Send($"(dash {dashAmount} 180)");
        }
        
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
    }
    
    public void ReceiveMessage(string txt, RcMessage.RcMessageType type)
    {
        if (type == RcMessage.RcMessageType.Init)
        {
            if (!visualizerInitialized)
            {
                visualizer.Init(teamName, teamSize, txt.Contains("r"), rcObjects);
                visualizerInitialized = true;
            }
        }
        
        overlayInfo.DisplayText(txt, type);
    }

    public void ResetVisualPositions(int playerNumber)
    {
        if (playerNumber == currentPlayer)
        {
            foreach (KeyValuePair<string,RcObject> rcObject in rcObjects)
                rcObject.Value.isVisible = false;
            
            visualizer.ResetVisualPositions(playerNumber);
        }
    }

    public void SetVisualPosition(int playerNumber, string objectName, float distance, float direction, float bodyFacingDir)
    {
        if (playerNumber == currentPlayer)
        {
            float radDirection = Mathf.Deg2Rad * -(direction - 90f);
            float relativeBodyFacingDir = -(bodyFacingDir);
            
            Vector2 relativePos = distance * new Vector2(Mathf.Cos(radDirection), Mathf.Sin(radDirection));

            bool uniqueObject = true;

            if (objectName.Length < 1)
                return;
            
            // object is a player
            if (objectName[0] == 'p')
            {
                // more info than just "p"
                if (objectName.Length > 1)
                {
                    // not an already known specific player
                    if (!rcObjects.ContainsKey(objectName))
                    {
                        Regex regex = new Regex("p \"([/-_a-zA-Z0-9]+)\"(?: ([0-9]{1,2})( goalie)?)?");
                        
                        if (regex.Match(objectName).Success)
                        {
                            string tName = regex.Match(objectName).Result("$1");
                            enemyTeamName = tName;
     
                            // if there is a player number, assume enemy player and add to dict
                            if (regex.Match(objectName).Result("$2").Length > 0)
                            {
                                int enemyNumber = int.Parse(regex.Match(objectName).Result("$2"));
                                bool goalie = regex.Match(objectName).Result("$3").Length > 0;
                                
                                CreatePlayerRcObject(true, enemyNumber, goalie);
                                visualizer.AddEnemyTeamMember(enemyTeamName, enemyNumber, goalie);
                            }
                            else // if there is no player number call setposition method depending on team
                            {
                                uniqueObject = false;
                                
                                if (tName.Equals(teamName))
                                    visualizer.SetUnknownPlayerPosition(relativePos, relativeBodyFacingDir, true);
                                else
                                    visualizer.SetUnknownPlayerPosition(relativePos, relativeBodyFacingDir, true, true);
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
                    visualizer.SetUnknownPlayerPosition(relativePos, relativeBodyFacingDir);
                }
            }

            if (uniqueObject)
            {
                if (rcObjects.ContainsKey(objectName))
                {
                    rcObjects[objectName].isVisible = true;
                    rcObjects[objectName].distance = distance;
                    rcObjects[objectName].direction = direction;
                    rcObjects[objectName].bodyFacingDir = bodyFacingDir;
            
                    //rcObjects[objectName].visibleThisStep = true;
                    rcObjects[objectName].relativePos = relativePos;
                    rcObjects[objectName].relativeBodyFacingDir = relativeBodyFacingDir;
                    
                    visualizer.SetVisualPosition(objectName, relativePos, relativeBodyFacingDir);
                }
                else
                {
                    if (!objectName.Equals("l r") && !objectName.Equals("l l") && !objectName.Equals("l t") && !objectName.Equals("l b"))
                        Debug.LogWarning($"unique object not in dict: {objectName}");
                }
            }
        }
    }

    public void UpdateVisualPositions(int playerNumber)
    {
        if (playerNumber == currentPlayer)
        {
            visualizer.UpdateVisualPositions(playerNumber);

            if (trainingMode)
            {
                if (rcObjects["b"].isVisible)
                    agent.SetBallInfo(true, rcObjects["b"].distance, rcObjects["b"].direction);
                else
                    agent.SetBallInfo(false);
                
                agent.RequestDecision();
            }
        }
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

    public bool isVisible;
    public float distance;
    public float direction;
    public float bodyFacingDir;

    public Vector2 relativePos;
    public float relativeBodyFacingDir;

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
