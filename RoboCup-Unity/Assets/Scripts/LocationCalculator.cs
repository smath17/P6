using System;
using System.Collections.Generic;
using UnityEngine;

public class LocationCalculator : MonoBehaviour
{
    Dictionary<string, ReferenceFlag> referenceFlags = new Dictionary<string, ReferenceFlag>();
    
    void Awake()
    {
        foreach (ReferenceFlag referenceFlag in FindObjectsOfType<ReferenceFlag>())
        {
            referenceFlag.flagName = referenceFlag.name;
            
            Vector3 pos3d = referenceFlag.transform.localPosition;
            referenceFlag.flagPos = new Vector2(pos3d.x, pos3d.z);
            
            referenceFlags.Add(referenceFlag.name, referenceFlag);

            referenceFlag.gameObject.SetActive(false);
        }
    }

    public void TrilateratePlayerPosition(RcPlayer player, List<MessageObject.SeenObjectData> seenObjectsData)
    {
        List<ReferenceFlag> flagsForCalculation = new List<ReferenceFlag>();
        
        foreach (MessageObject.SeenObjectData seenObject in seenObjectsData)
        {
            if (seenObject.distance < 1)
                continue;

            if (referenceFlags.ContainsKey(seenObject.objectName))
            {
                Vector2 relativePos = RcPerceivedObject.CalculateRelativePos(seenObject.distance, seenObject.direction);
                referenceFlags[seenObject.objectName].relativePos = relativePos;
                flagsForCalculation.Add(referenceFlags[seenObject.objectName]);
            }
        }

        if (flagsForCalculation.Count > 1)
        {
            int maxAnglesToCompare = 10;
            int anglesToCompare = Mathf.Min(flagsForCalculation.Count, maxAnglesToCompare);
            
            for (int i = 0; i < anglesToCompare - 2; i++)
            {
                float trueAngle = Vector2.SignedAngle(Vector2.up, flagsForCalculation[i].flagPos - flagsForCalculation[i+1].flagPos);
                float relativeAngle = Vector2.SignedAngle(Vector2.up, flagsForCalculation[i].relativePos - flagsForCalculation[i+1].relativePos);

                float newAngle = relativeAngle - trueAngle;

                if (i == 0)
                    player.SetCalculatedAngle(newAngle);
                else
                    player.SetCalculatedAngle(newAngle, true);
            }

            if (flagsForCalculation.Count > 2)
            {
                player.SetCalculatedPosition(Trilaterate(flagsForCalculation));
            }
        }
    }
    
    Vector2 Trilaterate(List<ReferenceFlag> flags)
    {
        float h = 20f;
        
        float x = 0f;
        float y = 0f;
        
        for (int i = 0; i < 20; i++)
        {
            Vector2 right = new Vector2(x + h, y);
            Vector2 left = new Vector2(x - h, y);
            Vector2 up = new Vector2(x, y + h);
            Vector2 down = new Vector2(x, y - h);
            
            float mse = MeanSquaredError(new Vector2(x, y), flags);
            float mseR = MeanSquaredError(right, flags);
            float mseL = MeanSquaredError(left, flags);
            float mseU = MeanSquaredError(up, flags);
            float mseD = MeanSquaredError(down, flags);

            if (mseR < mse)
                x += h;
            else if (mseL < mse)
                x -= h;

            if (mseU < mse)
                y += h;
            else if (mseD < mse)
                y -= h;

            h *= 0.75f;
        }
        
        return new Vector2(x, y);
        
        //Vector2 bestPosition = Vector2.zero;
        //float lowestError = Mathf.Infinity;
        //
        ////minimize error
        //for (float x = -55; x < 55; x++)
        //{
        //    for (float y = -35; y < 35; y++)
        //    {
        //        Vector2 testPosition = new Vector2(x, y);
        //        float mse = MeanSquaredError(testPosition, flags);
        //        if (mse < lowestError)
        //        {
        //            lowestError = mse;
        //            bestPosition = testPosition;
        //            positionIsKnown = true;
        //        }
        //    }
        //}
        //
        //return bestPosition;
    }

    float MeanSquaredError(Vector2 position, List<ReferenceFlag> flags)
    {
        float mse = 0f;

        foreach (ReferenceFlag flag in flags)
        {
            float calculatedDistance = Vector2.Distance(position, flag.flagPos);
            float measuredDistance = Vector2.Distance(Vector2.zero, flag.relativePos);
            mse += Mathf.Pow(calculatedDistance - measuredDistance, 2f);
        }

        return mse / flags.Count;
    }
}