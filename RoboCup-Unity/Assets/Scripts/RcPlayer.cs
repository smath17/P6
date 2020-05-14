using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;

public class RcPlayer : MonoBehaviour, IPlayer
{   
    IPEndPoint endPoint;
    UdpClient client;
    int playerNumber;
    
    Socket socket;
    byte[] buffer = new byte[6000];

    bool newPortIsSet = false;
    int newPort = 0;

    bool debug = false;

    string teamName;
    string enemyTeamName;

    bool onMainTeam;

    Dictionary<string, RcPerceivedObject> rcObjects = new Dictionary<string, RcPerceivedObject>();
    
    Vector2 prevPlayerPosition = Vector2.zero;
    Vector2 curPlayerPosition = Vector2.zero;
    float prevPlayerAngle = 0f;
    float curPlayerAngle = 0f;
    
    bool positionWasKnown;
    bool positionIsKnown;
    bool angleWasKnown;
    bool angleIsKnown;

    int kickBallCount;

    IVisualizer visualizer;
    
    void Awake()
    {
        CreateRcObjects();
    }

    void CreateRcObjects()
    {
        CreateRcObject("b", RcPerceivedObject.RcObjectType.Ball);
        CreateRcObject("B", RcPerceivedObject.RcObjectType.BallClose);

        foreach (string o in RoboCup.other)
        {
            CreateRcObject(o, RcPerceivedObject.RcObjectType.Flag);
        }
        
        foreach (string flagName in RoboCup.whiteFlags)
        {
            CreateRcObject(flagName, RcPerceivedObject.RcObjectType.Flag);
        }
        
        foreach (string flagName in RoboCup.redFlags)
        {
            CreateRcObject(flagName, RcPerceivedObject.RcObjectType.Flag);
        }
        
        foreach (string flagName in RoboCup.blueFlags)
        {
            CreateRcObject(flagName, RcPerceivedObject.RcObjectType.Flag);
        }

        for (int i = 0; i < RoboCup.FullTeamSize+1; i++)
        {
            bool goalie = i == RoboCup.FullTeamSize;
            CreatePlayerRcObject(false, i, goalie);
        }
    }

    void CreateRcObject(string objectName, RcPerceivedObject.RcObjectType objectType)
    {
        rcObjects.Add(objectName, new RcPerceivedObject(objectName, objectType));
    }

    void CreatePlayerRcObject(bool enemyTeam, int playerNumber, bool goalie)
    {
        RcPerceivedObject.RcObjectType objectType = enemyTeam ? RcPerceivedObject.RcObjectType.EnemyPlayer : RcPerceivedObject.RcObjectType.TeamPlayer;
        string tName = enemyTeam ? enemyTeamName : teamName;
        RcPerceivedObject rcPlayerObj = new RcPerceivedObject(objectType, tName, playerNumber, goalie);
        rcObjects.Add(rcPlayerObj.name, rcPlayerObj);
        rcObjects[rcPlayerObj.name].curVisibility = true;
    }
    
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.K))
        {
            debug = !debug;
        }
    }

    public void Init(bool mainTeam, int pNum, bool goalie, int x = 0, int y = 0, bool reconnect = false, int unum = 1)
    {
        playerNumber = pNum;
        
        endPoint = new IPEndPoint(IPAddress.Parse(RoboCup.singleton.ip), RoboCup.Port);

        socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        socket.Bind(new IPEndPoint(IPAddress.Any, 0));
        socket.Blocking = false;

        onMainTeam = mainTeam;
        teamName = onMainTeam ? RoboCup.singleton.GetTeamName() : RoboCup.singleton.GetEnemyTeamName();
        
        string goalieString = (goalie) ? " (goalie)" : "";
        
        if (reconnect)
            Send($"(reconnect {teamName} {unum+1})");
        else
            Send($"(init {teamName} (version 16){goalieString})");

        StartCoroutine(Poll());
        
        StartCoroutine(MoveAfterInit(x, y));
    }

    public void Send(string text)
    {
        Byte[] sendBytes = Encoding.ASCII.GetBytes(text + '\0');
        Debug.Log($"player {playerNumber} sending (port {endPoint.Port}): {text}");
        socket.SendTo(sendBytes, endPoint);
    }

    IEnumerator MoveAfterInit(int x, int y)
    {
        yield return new WaitForSeconds(0.25f);
        Send($"(move {x} {y})");
    }
    
    IEnumerator Poll () {
        while (true) {
            yield return null;
            //Debug.Log("polling..");
            if (socket.Poll(0, SelectMode.SelectRead)) {
                EndPoint remoteEndPoint = new IPEndPoint(IPAddress.Any, 0);
                int bytesReceived = socket.ReceiveFrom(buffer, ref remoteEndPoint);
                if (bytesReceived > 0)
                {
                    string receiveString = Encoding.ASCII.GetString(buffer);
                    GetMessage(receiveString);

                    if (!newPortIsSet)
                    {
                        newPort = ((IPEndPoint) remoteEndPoint).Port;
                        endPoint = new IPEndPoint(IPAddress.Parse(RoboCup.singleton.ip), newPort);
                        newPortIsSet = true;
                        Debug.Log($"player {playerNumber} using port: {newPort}");
                    }
                }
            }
        }
    }
    
    public void GetMessage(string msg)
    {
        RcMessage rcMessage = new RcMessage(msg);

        switch (rcMessage.MessageType)
        {
            case RcMessage.RcMessageType.Error:
                Debug.Log($"ERROR: {rcMessage}");
                break;
            
            case RcMessage.RcMessageType.Init:
            case RcMessage.RcMessageType.Reconnect:
                Debug.Log("Initialized");
                visualizer = RoboCup.singleton.InitVisualizer(this);
                break;
            
            case RcMessage.RcMessageType.PlayerParam:
                Debug.Log("Player parameters received");
                break;
            
            case RcMessage.RcMessageType.ServerParam:
                Debug.Log("Server parameters received");
                break;
            
            case RcMessage.RcMessageType.PlayerType:
                //process player_type
                break;
            
            case RcMessage.RcMessageType.Sense:
                Sense(rcMessage.GetMessageObject().values[0].MObject);
                break;
            
            case RcMessage.RcMessageType.See:
                See(rcMessage.GetMessageObject().values[0].MObject);
                break;
            
            case RcMessage.RcMessageType.Hear:
                //process hear
                break;
            
            case RcMessage.RcMessageType.Other:
                Debug.LogWarning($"received unknown message: {rcMessage}");
                break;
        }
        
        RoboCup.singleton.ReceiveMessage(msg, rcMessage.MessageType);
    }

    void Sense(MessageObject messageObject)
    {
        MessageObject.SenseBodyData senseBodyData = messageObject.GetSenseBody();
        kickBallCount = senseBodyData.kick;
    }

    void See(MessageObject messageObject)
    {
        foreach (KeyValuePair<string, RcPerceivedObject> rcObject in rcObjects)
        {
            rcObject.Value.prevVisibility = rcObject.Value.curVisibility;
            rcObject.Value.curVisibility = false;
            
            rcObject.Value.prevRelativePos = rcObject.Value.curRelativePos;
            rcObject.Value.prevRelativeBodyFacingDir = rcObject.Value.curRelativeBodyFacingDir;
        }
        
        visualizer.ResetVisualPositions(this);
        
        prevPlayerPosition = curPlayerPosition;
        prevPlayerAngle = curPlayerAngle;
        
        positionWasKnown = positionIsKnown;
        angleWasKnown = angleIsKnown;
        
        positionIsKnown = false;
        angleIsKnown = false;
        
        List<MessageObject.SeenObjectData> seenObjectsData = messageObject.GetSeenObjects();
        
        foreach (MessageObject.SeenObjectData data in seenObjectsData)
        {
            UpdateRcObject(data.objectName, data.distance, data.direction, data.bodyFacingDir);
        }
        
        RoboCup.singleton.locationCalculator.TrilateratePlayerPosition(this, seenObjectsData);

        visualizer.UpdateVisualPositions(this);
    }

    public void SetCalculatedAngle(float angle, bool blendWithPreviousAngle = false)
    {
        angleIsKnown = true;

        if (blendWithPreviousAngle)
            Mathf.LerpAngle(curPlayerAngle, angle, 0.5f);
        else
            curPlayerAngle = angle;
    }

    public void SetCalculatedPosition(Vector2 position)
    {
        positionIsKnown = true;
        curPlayerPosition = position;
    }
    
    public float GetCalculatedAngle(bool previous = false)
    {
        return previous ? prevPlayerAngle : curPlayerAngle;
    }

    public Vector2 GetCalculatedPosition(bool previous = false)
    {
        return previous ? prevPlayerPosition : curPlayerPosition;
    }
    
    public bool ReadyToInterpolate()
    {
        return positionWasKnown && positionIsKnown && angleWasKnown && angleIsKnown;
    }

    void UpdateRcObject(string objectName, float distance, float direction, float bodyFacingDir)
    {
        float relativeBodyFacingDir = -(bodyFacingDir);

        Vector2 relativePos = RcPerceivedObject.CalculateRelativePos(distance, direction);

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
                        //RoboCup.singleton.SetEnemyTeamName(enemyTeamName);
 
                        // if there is a player number, assume enemy player and add to dict
                        if (regex.Match(objectName).Result("$2").Length > 0)
                        {
                            int enemyNumber = int.Parse(regex.Match(objectName).Result("$2"));
                            bool goalie = regex.Match(objectName).Result("$3").Length > 0;
                            
                            CreatePlayerRcObject(true, enemyNumber, goalie);
                            visualizer.AddEnemyTeamMember(this, enemyTeamName, enemyNumber, goalie);
                        }
                        else // if there is no player number call setposition method depending on team
                        {
                            uniqueObject = false;
                            
                            if (tName.Equals(teamName))
                                visualizer.SetUnknownPlayerPosition(this, relativePos, relativeBodyFacingDir, true);
                            else
                                visualizer.SetUnknownPlayerPosition(this, relativePos, relativeBodyFacingDir, true, true);
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
                visualizer.SetUnknownPlayerPosition(this, relativePos, relativeBodyFacingDir);
            }
        }

        if (uniqueObject)
        {
            if (rcObjects.ContainsKey(objectName))
            {
                rcObjects[objectName].curVisibility = true;
                rcObjects[objectName].distance = distance;
                rcObjects[objectName].direction = direction;
                rcObjects[objectName].curRelativeBodyFacingDir = bodyFacingDir;
        
                rcObjects[objectName].curRelativePos = relativePos;
                rcObjects[objectName].curRelativeBodyFacingDir = relativeBodyFacingDir;
                
                visualizer.SetVisualPosition(this, objectName, relativePos, relativeBodyFacingDir);
            }
            else
            {
                if (!objectName.Equals("l r") && !objectName.Equals("l l") && !objectName.Equals("l t") && !objectName.Equals("l b") && !objectName.Equals("G"))
                    Debug.LogWarning($"unique object not in dict: {objectName}");
            }
        }
    }
    
    public Dictionary<string, RcPerceivedObject> GetRcObjects()
    {
        return rcObjects;
    }
    
    public RcPerceivedObject GetRcObject(string objName)
    {
        if (rcObjects.ContainsKey(objName))
            return rcObjects[objName];
        else
            return null;
    }

    public int GetKickBallCount()
    {
        return kickBallCount;
    }

    public void Move(int x, int y)
    {
        Send($"(move {x} {y})");
    }

    public void Dash(int amount, int direction)
    {
        Send($"(dash {amount} {direction})");
    }

    public void Turn(int amount)
    {
        Send($"(turn {amount})");
    }

    public void Kick(int power)
    {
        Send($"(kick {power} 0)");
    }

    public void Catch()
    {
        Send("(catch 0)");
    }
}


public class RcPerceivedObject
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

    public RcPerceivedObject(string name, RcObjectType objectType)
    {
        this.name = name;
        this.objectType = objectType;
    }
    public RcPerceivedObject(RcObjectType objectType, string teamName, int playerNumber, bool goalie)
    {
        this.objectType = objectType;
        
        this.teamName = teamName;
        this.playerNumber = playerNumber;
        this.goalie = goalie;
        
        string goalieText = goalie ? " goalie" : "";
        name = $"p \"{teamName}\" {playerNumber}{goalieText}";
    }

    public static Vector2 CalculateRelativePos(float distance, float direction)
    {
        float radDirection = Mathf.Deg2Rad * -(direction - 90f);
        
        return distance * new Vector2(Mathf.Cos(radDirection), Mathf.Sin(radDirection));
    }
}
