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
        foreach (KeyValuePair<string, RcObject> rcObject in rcObjects)
        {
            rcObject.Value.prevVisibility = rcObject.Value.curVisibility;
            rcObject.Value.curVisibility = false;
            
            rcObject.Value.prevRelativePos = rcObject.Value.curRelativePos;
            rcObject.Value.prevRelativeBodyFacingDir = rcObject.Value.curRelativeBodyFacingDir;
        }

        List<MessageObject.SeenObjectData> seenObjectsData = messageObject.GetSeenObjects();

        foreach (MessageObject.SeenObjectData data in seenObjectsData)
        {
            UpdateRcObject(data.objectName, data.distance, data.direction, data.bodyFacingDir);
        }

        if (agentTrainer != null)
            agentTrainer.Step();
    }
    
    void UpdateRcObject(string objectName, float distance, float direction, float bodyFacingDir)
    {
        float radDirection = Mathf.Deg2Rad * -(direction - 90f);
        float relativeBodyFacingDir = -(bodyFacingDir);
        
        Vector2 relativePos = distance * new Vector2(Mathf.Cos(radDirection), Mathf.Sin(radDirection));

        if (objectName.Length < 1)
            return;
        
        if (!rcObjects.ContainsKey(objectName))
            rcObjects.Add(objectName, new RcObject(objectName, RcObject.RcObjectType.Unknown));
        
        rcObjects[objectName].curVisibility = true;
        rcObjects[objectName].distance = distance;
        rcObjects[objectName].direction = direction;
        rcObjects[objectName].curRelativeBodyFacingDir = bodyFacingDir;
        
        rcObjects[objectName].curRelativePos = relativePos;
        rcObjects[objectName].curRelativeBodyFacingDir = relativeBodyFacingDir;
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
}