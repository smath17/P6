using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAgents;
using MLAgents.Sensors;

public class RoboCupAgent : Agent
{
    public override void OnEpisodeBegin()
    {
    }
    
    public override void CollectObservations(VectorSensor sensor)
    {
    }
    
    public override void OnActionReceived(float[] vectorAction)
    {
        //SetReward(1.0f);
        //EndEpisode();
    }
    
    public override float[] Heuristic()
    {
        var action = new float[2];
        action[0] = Input.GetAxis("Horizontal");
        action[1] = Input.GetAxis("Vertical");
        return action;
    }
}