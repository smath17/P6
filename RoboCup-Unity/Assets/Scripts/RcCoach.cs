using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;

public class RcCoach : MonoBehaviour
{
    IPEndPoint endPoint;
    UdpClient client;
    
    Socket socket;
    byte[] buffer = new byte[6000];

    bool newPortIsSet = false;
    int newPort = 0;

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
        Debug.Log($"Coach received: {rcMessage}");
    }
}