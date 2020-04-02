using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class OverlayInfo : MonoBehaviour
{
    [Header("References")]
    public TextMeshProUGUI currentPlayerText;
    public TextMeshProUGUI serverMessageText;
    public TextMeshProUGUI playerTypeText;
    public TextMeshProUGUI senseText;
    public TextMeshProUGUI seeText;
    public TextMeshProUGUI hearText;

    public void DisplayText(string txt, RcMessage.RcMessageType type)
    {
        switch (type)
        {
            case RcMessage.RcMessageType.PlayerType:
                playerTypeText.text = txt;
                break;
            case RcMessage.RcMessageType.Sense:
                senseText.text = txt;
                break;
            case RcMessage.RcMessageType.See:
                seeText.text = txt;
                break;
            case RcMessage.RcMessageType.Hear:
                hearText.text = txt;
                break;
            case RcMessage.RcMessageType.PlayerParam:
                //ignore
                break;
            case RcMessage.RcMessageType.ServerParam:
                //ignore
                break;
            default:
                serverMessageText.text = txt;
                break;
        }
    }
    
    public void UpdateCurrentPlayerText(int currentPlayer)
    {
        currentPlayerText.text = $"Current Player: {currentPlayer+1} {(currentPlayer == 10 ? "(goalie)" : "")}";
    }
}
