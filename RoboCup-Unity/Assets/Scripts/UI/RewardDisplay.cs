using TMPro;
using UnityEngine;

public class RewardDisplay : MonoBehaviour
{
    public Transform stepBar;
    public Transform episodeBar;
    
    public TextMeshProUGUI stepText;
    public TextMeshProUGUI episodeText;
    
    public void DisplayRewards(float step, float episode)
    {
        stepBar.localScale = new Vector3(1,step,1);
        episodeBar.localScale = new Vector3(1,episode,1);

        stepText.text = $"{step}";
        episodeText.text = $"{episode}";
    }
}
