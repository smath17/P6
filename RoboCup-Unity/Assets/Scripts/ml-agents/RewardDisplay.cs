using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RewardDisplay : MonoBehaviour
{
    public Transform bar;
    public TextMeshProUGUI text;
    
    public void DisplayCumulativeReward(float value)
    {
        bar.localScale = new Vector3(1,value,1);
        text.text = $"{value}";
    }
}
