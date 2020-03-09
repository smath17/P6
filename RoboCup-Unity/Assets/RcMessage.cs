using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class RcMessage
{
    public enum RcMessageType
    {
        Error,
        Init,
        PlayerParam,
        ServerParam,
        PlayerType,
        Sense,
        See,
        Hear,
        Other
    }

    public RcMessageType MessageType;

    string message;
    MessageObject parsedMessage;

    public RcMessage(string msg)
    {
        message = msg;
        parsedMessage = ParseObject(msg, 0);

        string typeString = parsedMessage.values[0].MObject.values[0].MString;
        switch (typeString)
        {
            case "error":
                MessageType = RcMessageType.Error;
                break;
            case "init":
                MessageType = RcMessageType.Init;
                break;
            case "player_param":
                MessageType = RcMessageType.PlayerParam;
                break;
            case "server_param":
                MessageType = RcMessageType.ServerParam;
                break;
            case "player_type":
                MessageType = RcMessageType.PlayerType;
                break;
            case "sense_body":
                MessageType = RcMessageType.Sense;
                break;
            case "see":
                MessageType = RcMessageType.See;
                break;
            case "hear":
                MessageType = RcMessageType.Hear;
                break;
            default:
                MessageType = RcMessageType.Other;
                break;
        }
    }
    
    public string GetMessage()
    {
        return message;
    }

    public MessageObject GetMessageObject()
    {
        return parsedMessage;
    }

    MessageObject ParseObject(string s, int index)
    {
        int i = index;
        int stringLength = s.IndexOf('\0');

        bool done = false;

        List<MessageObject.StringOrObject> values = new List<MessageObject.StringOrObject>();


        while (i < stringLength && !done && i < 1024)
        {
            char c = s[i];
            switch (c)
            {
                case '(':
                    MessageObject parsedObj = ParseObject(s, i + 1);
                    
                    i = parsedObj.endIndex;
                    
                    MessageObject.StringOrObject structObject = new MessageObject.StringOrObject();
                    structObject.MObject = parsedObj;
                    
                    values.Add(structObject);
                    break;
                
                case ')':
                    done = true;
                    break;
                
                case ' ':
                    break;
                
                default:
                    string parsedString = ParseString(s, i, stringLength);
                    
                    i += parsedString.Length - 1;
                    
                    MessageObject.StringOrObject structString = new MessageObject.StringOrObject();
                    structString.MString = parsedString;
                    
                    values.Add(structString);
                    break;
            }
            i++;
        }

        return new MessageObject(values, i-1);
    }
    
    // Read until ' ' or ')' and return string
    string ParseString(string s, int index, int stringLength)
    {
        StringBuilder stringBuilder = new StringBuilder();
        bool done = false;
        int i = index;

        while (!done && i < stringLength && i < 1024)
        {
            char c = s[i];
            switch (c)
            {
                case '\0':
                    done = true;
                    break;
                case ')':
                    done = true;
                    break;
                case ' ':
                    done = true;
                    break;
                case '(':
                    throw new Exception($"found ( on index {i})\noriginal string: {s}");
                default:
                    stringBuilder.Append(s[i]);
                    break;
            }
            i++;
        }

        return stringBuilder.ToString();
    }

    public override string ToString()
    {
        return parsedMessage.ToString();
    }
}

public class MessageObject
{
    public List<StringOrObject> values;
    public int endIndex;

    public struct StringOrObject
    {
        public string MString;
        public MessageObject MObject;
    }

    public MessageObject(List<StringOrObject> values, int endIndex)
    {
        this.values = values;
        this.endIndex = endIndex;
    }
    
    public override string ToString()
    {
        return PrettyPrint(0);
    }

    public string PrettyPrint(int indentation)
    {
        StringBuilder sb = new StringBuilder();
        for (int i = 0; i < indentation; i++) 
            sb.Append('\t');

        indentation++;

        if (values != null)
        {
            foreach (StringOrObject v in values)
            {
                if (v.MObject != null)
                {
                    for (int i = 0; i < indentation-1; i++) 
                        sb.Append('\t');
                    
                    sb.Append("(\n");
                    sb.Append(v.MObject.PrettyPrint(indentation));
                    sb.Append(")\n");
                }
                else if (v.MString != null)
                    sb.Append(v.MString);
                
                if (values.IndexOf(v) != values.Count-1)
                    sb.Append(" ");
            }   
        }

        return sb.ToString();
    }
}