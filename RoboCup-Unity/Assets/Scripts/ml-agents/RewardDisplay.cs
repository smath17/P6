using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RewardDisplay : MonoBehaviour
{
    public Transform bar;
    public TextMeshProUGUI text;

    public List<Image> icons;
    
    public void DisplayCumulativeReward(float value)
    {
        bar.localScale = new Vector3(1,value,1);
        text.text = $"{value}";
        
        foreach (Image img in icons)
        {
            img.enabled = false;
        }
    }

    public void DisplayIcon(int icon)
    {
        if (icon < icons.Count)
            icons[icon].enabled = true;
    }
}
