using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;

public class ObservationDisplay : MonoBehaviour
{
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI obsText;
    
    public void DisplayObservations(List<string> observationNames, List<float> observations)
    {
        StringBuilder nameSb = new StringBuilder();
        foreach (string observationName in observationNames)
        {
            nameSb.Append(observationName);
            nameSb.Append(": ");
            nameSb.Append("\n");
        }
        if (nameText != null)
            nameText.text = nameSb.ToString();
        
        StringBuilder obsSb = new StringBuilder();
        foreach (float observation in observations)
        {
            obsSb.Append(observation);
            obsSb.Append("\n");
        }
        if (obsText != null)
            obsText.text = obsSb.ToString();
    }
}