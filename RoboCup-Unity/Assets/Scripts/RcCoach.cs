using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;

public class RcCoach : MonoBehaviour, ICoach
{
    IPEndPoint endPoint;
    UdpClient client;
    
    Socket socket;
    byte[] buffer = new byte[6000];

    bool newPortIsSet = false;
    int newPort = 0;
    
    Dictionary<string, RcObject> rcObjects = new Dictionary<string, RcObject>();

    AgentTrainer agentTrainer;
    bool reportSeeToRoboCup;

    public void Init(bool online)
    {
        endPoint = new IPEndPoint(IPAddress.Parse(RoboCup.singleton.ip), online ? RoboCup.OnlineCoachPort : RoboCup.OfflineCoachPort);

        socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        socket.Bind(new IPEndPoint(IPAddress.Any, 0));
        socket.Blocking = false;
        
        StartCoroutine(Poll());
    }

    public void Send(string text)
    {
        Byte[] sendBytes = Encoding.ASCII.GetBytes(text + '\0');
        Debug.Log($"coach sending (port {endPoint.Port}): {text}");
        socket.SendTo(sendBytes, endPoint);
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
                        Debug.Log($"coach using port: {newPort}");
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
            case RcMessage.RcMessageType.See:
                See(rcMessage.GetMessageObject().values[0].MObject);
                break;
            
            default:
                Debug.Log($"Coach received: {rcMessage}");
                break;
        }
    }
    
    void See(MessageObject messageObject)
    {
        List<MessageObject.CoachSeenObjectData> seenObjectsData = messageObject.GetSeenObjectsCoach();

        foreach (MessageObject.CoachSeenObjectData data in seenObjectsData)
        {
            UpdateRcObject(data.objectName, data.x, data.y, data.deltaX, data.deltaY, data.bodyAngle, data.neckAngle);
        }

        if (agentTrainer != null)
            agentTrainer.Step();
        
        if (reportSeeToRoboCup)
            RoboCup.singleton.StepAgents();
    }
    
    void UpdateRcObject(string objectName, float x, float y, float deltaX, float deltaY, float bodyAngle, float neckAngle)
    {
        if (objectName.Length < 1)
            return;
        
        if (!rcObjects.ContainsKey(objectName))
            rcObjects.Add(objectName, new RcObject(objectName));
        
        Vector2 position = new Vector2(x, y);
        Vector2 delta = new Vector2(deltaX, deltaY);
        
        rcObjects[objectName].position = position;
        rcObjects[objectName].delta = delta;
        rcObjects[objectName].bodyAngle = bodyAngle;
        rcObjects[objectName].neckAngle = neckAngle;
    }

    public RcObject GetRcObject(string objName)
    {
        if (rcObjects.ContainsKey(objName))
            return rcObjects[objName];
        else
            return null;
    }
    
    public void InitTraining(AgentTrainer trainer)
    {
        agentTrainer = trainer;
        Send("(eye on)");
        KickOff();
    }

    public void InitMatch()
    {
        reportSeeToRoboCup = true;
        Send("(eye on)");
    }

    public void MoveBall(int x, int y)
    {
        Send($"(move (ball) {x} {y})");
    }

    public void MovePlayer(string teamName, int unum, int x, int y)
    {
        Send($"(move (player {teamName} {unum}) {x} {y})");
    }
    
    public void MovePlayer(string teamName, int unum, int x, int y, int direction)
    {
        Send($"(move (player {teamName} {unum}) {x} {y} {direction})");
    }

    public void Recover()
    {
        Send("(recover)");
    }

    public void KickOff()
    {
        Send("(start)");
    }

    public void Goal(bool rightSide)
    {
        Send($"(referee goal_{(rightSide ? "r" : "l")})");
    }
}

public class RcObject
{
    public string name;
    public Vector2 position;
    public Vector2 delta;
    public float bodyAngle;
    public float neckAngle;

    public RcObject(string name)
    {
        this.name = name;
    }
}