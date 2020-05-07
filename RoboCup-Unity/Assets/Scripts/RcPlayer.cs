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

    Dictionary<string, RcObject> rcObjects = new Dictionary<string, RcObject>();
    
    Vector2 prevPlayerPosition = Vector2.zero;
    Vector2 curPlayerPosition = Vector2.zero;
    float prevPlayerAngle = 0f;
    float curPlayerAngle = 0f;
    
    bool positionWasKnown;
    bool positionIsKnown;
    bool angleWasKnown;
    bool angleIsKnown;

    void Awake()
    {
        CreateRcObjects();
    }

    void CreateRcObjects()
    {
        CreateRcObject("b", RcObject.RcObjectType.Ball);
        CreateRcObject("B", RcObject.RcObjectType.BallClose);

        foreach (string o in RoboCup.other)
        {
            CreateRcObject(o, RcObject.RcObjectType.Flag);
        }
        
        foreach (string flagName in RoboCup.whiteFlags)
        {
            CreateRcObject(flagName, RcObject.RcObjectType.Flag);
        }
        
        foreach (string flagName in RoboCup.redFlags)
        {
            CreateRcObject(flagName, RcObject.RcObjectType.Flag);
        }
        
        foreach (string flagName in RoboCup.blueFlags)
        {
            CreateRcObject(flagName, RcObject.RcObjectType.Flag);
        }

        for (int i = 0; i < RoboCup.FullTeamSize+1; i++)
        {
            bool goalie = i == RoboCup.FullTeamSize;
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
        rcObjects[rcPlayerObj.name].curVisibility = true;
    }
    
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.K))
        {
            debug = !debug;
        }
    }

    public void Init(bool mainTeam, int pNum, bool goalie, int x = 0, int y = 0)
    {
        playerNumber = pNum;
        
        endPoint = new IPEndPoint(IPAddress.Parse(RoboCup.singleton.ip), RoboCup.Port);

        socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        socket.Bind(new IPEndPoint(IPAddress.Any, 0));
        socket.Blocking = false;

        onMainTeam = mainTeam;
        teamName = onMainTeam ? RoboCup.singleton.GetTeamName() : RoboCup.singleton.GetEnemyTeamName();
        
        string goalieString = (goalie) ? " (goalie)" : "";
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
                Debug.Log("Initialized");
                RoboCup.singleton.InitVisualizer(this);
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
                //process sense
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

    void See(MessageObject messageObject)
    {
        foreach (KeyValuePair<string, RcObject> rcObject in rcObjects)
        {
            rcObject.Value.prevVisibility = rcObject.Value.curVisibility;
            rcObject.Value.curVisibility = false;
            
            rcObject.Value.prevRelativePos = rcObject.Value.curRelativePos;
            rcObject.Value.prevRelativeBodyFacingDir = rcObject.Value.curRelativeBodyFacingDir;
        }
        
        RoboCup.singleton.ResetVisualPositions(onMainTeam, playerNumber);
        
        if (messageObject.values.Count > 2)
        {
            List<MessageObject> seenObjects = new List<MessageObject>();
            for (int i = 2; i < messageObject.values.Count; i++)
            {
                if (messageObject.values[i].MObject != null)
                    seenObjects.Add(messageObject.values[i].MObject);
            }

            foreach (MessageObject seenObject in seenObjects)
            {
                if (seenObject.values.Count > 0)
                {
                    string objectName = seenObject.values[0].MObject.SimplePrint();

                    float distance = 100;
                    if (seenObject.values.Count > 1)
                        float.TryParse(seenObject.values[1].MString,NumberStyles.Float, CultureInfo.InvariantCulture, out distance);

                    float direction = 0;
                    if (seenObject.values.Count > 2)
                        float.TryParse(seenObject.values[2].MString,NumberStyles.Float, CultureInfo.InvariantCulture, out direction);

                    float distChange = 0;
                    if (seenObject.values.Count > 3)
                        float.TryParse(seenObject.values[3].MString,NumberStyles.Float, CultureInfo.InvariantCulture, out distChange);
                    
                    float dirChange = 0;
                    if (seenObject.values.Count > 4)
                        float.TryParse(seenObject.values[4].MString,NumberStyles.Float, CultureInfo.InvariantCulture, out dirChange);
                    
                    float bodyFacingDir = 0;
                    if (seenObject.values.Count > 5)
                        float.TryParse(seenObject.values[5].MString,NumberStyles.Float, CultureInfo.InvariantCulture, out bodyFacingDir);
                    
                    UpdateRcObject(objectName, distance, direction, bodyFacingDir);
                }
            }
        }

        prevPlayerPosition = curPlayerPosition;
        prevPlayerAngle = curPlayerAngle;
        
        positionWasKnown = positionIsKnown;
        angleWasKnown = angleIsKnown;
        
        positionIsKnown = false;
        angleIsKnown = false;
        
        RoboCup.singleton.locationCalculator.TrilateratePlayerPosition(this, rcObjects);

        RoboCup.singleton.UpdateVisualPositions(onMainTeam, playerNumber);
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
                        RoboCup.singleton.SetEnemyTeamName(enemyTeamName);
 
                        // if there is a player number, assume enemy player and add to dict
                        if (regex.Match(objectName).Result("$2").Length > 0)
                        {
                            int enemyNumber = int.Parse(regex.Match(objectName).Result("$2"));
                            bool goalie = regex.Match(objectName).Result("$3").Length > 0;
                            
                            CreatePlayerRcObject(true, enemyNumber, goalie);
                            RoboCup.singleton.GetVisualizer().AddEnemyTeamMember(enemyTeamName, enemyNumber, goalie);
                        }
                        else // if there is no player number call setposition method depending on team
                        {
                            uniqueObject = false;
                            
                            if (tName.Equals(teamName))
                                RoboCup.singleton.GetVisualizer().SetUnknownPlayerPosition(relativePos, relativeBodyFacingDir, true);
                            else
                                RoboCup.singleton.GetVisualizer().SetUnknownPlayerPosition(relativePos, relativeBodyFacingDir, true, true);
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
                RoboCup.singleton.GetVisualizer().SetUnknownPlayerPosition(relativePos, relativeBodyFacingDir);
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
        
                //rcObjects[objectName].visibleThisStep = true;
                rcObjects[objectName].curRelativePos = relativePos;
                rcObjects[objectName].curRelativeBodyFacingDir = relativeBodyFacingDir;
                
                RoboCup.singleton.GetVisualizer().SetVisualPosition(objectName, relativePos, relativeBodyFacingDir);
            }
            else
            {
                if (!objectName.Equals("l r") && !objectName.Equals("l l") && !objectName.Equals("l t") && !objectName.Equals("l b") && !objectName.Equals("G"))
                    Debug.LogWarning($"unique object not in dict: {objectName}");
            }
        }
    }
    
    public Dictionary<string, RcObject> GetRcObjects()
    {
        return rcObjects;
    }
    
    public RcObject GetRcObject(string objName)
    {
        if (rcObjects.ContainsKey(objName))
            return rcObjects[objName];
        else
            return null;
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