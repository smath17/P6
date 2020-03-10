using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;

public class RcPlayer : MonoBehaviour
{   
    IPEndPoint endPoint;
    UdpClient client;
    int playerNumber;
    
    Socket socket;
    byte[] buffer = new byte[6000];

    bool newPortIsSet = false;
    int newPort = 0;

    bool debug = false;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.K))
        {
            debug = !debug;
        }
    }

    public void Init(int pNum)
    {
        playerNumber = pNum;
        
        endPoint = new IPEndPoint(IPAddress.Parse(RoboCup.singleton.ip), RoboCup.singleton.port);

        socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        socket.Bind(new IPEndPoint(IPAddress.Any, 0));
        socket.Blocking = false;
        
        StartCoroutine(Poll());
    }

    public void Send(string text)
    {
        Byte[] sendBytes = Encoding.ASCII.GetBytes(text + '\0');
        //Debug.Log($"player {playerNumber} sending (port {endPoint.Port}): {text}");
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
        
        RoboCup.singleton.DisplayText(msg, rcMessage.MessageType);
    }

    void See(MessageObject messageObject)
    {
        if (messageObject.values.Count < 3)
        {
            Debug.LogWarning("not enough values");
            return;
        }
        
        RoboCup.singleton.ResetVisualPositions(playerNumber);
        
        List<MessageObject> seenObjects = new List<MessageObject>();
        for (int i = 2; i < messageObject.values.Count; i++)
        {
            if (messageObject.values[i].MObject != null)
                seenObjects.Add(messageObject.values[i].MObject);
        }

        foreach (MessageObject seenObject in seenObjects)
        {
            //string objectName = seenObject.values[0].MObject.values[0].MString;

            if (seenObject.values.Count > 0)
            {
                string objectName = seenObject.values[0].MObject.SimplePrint();

                float distance = 100;
                if (seenObject.values.Count > 1)
                    distance = float.Parse(seenObject.values[1].MString,NumberStyles.Float,CultureInfo.InvariantCulture);

                float angle = 0;
                if (seenObject.values.Count > 2)
                    angle = float.Parse(seenObject.values[2].MString);
            
                RoboCup.singleton.SetVisualPosition(playerNumber, objectName, distance, angle);
            }
        }
        
        RoboCup.singleton.UpdateVisualPositions(playerNumber);
    }
}