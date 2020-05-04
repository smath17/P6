using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAgents;
using MLAgents.Sensors;

public class RoboCupAgent : Agent
{
    bool ballVisible;
    float ballDistance;
    float ballDirection;

    RcPlayer rcPlayer;

    public void SetPlayer(RcPlayer player)
    {
        rcPlayer = player;
    }
    
    public override void OnEpisodeBegin()
    {
        //Debug.LogError("OnEpisodeBegin called");
        if (rcPlayer != null)
            rcPlayer.Send("(move -20 0)");
    }

    public void SetBallInfo(bool visible, float distance = 0, float direction = 0)
    {
        ballVisible = visible;
        ballDistance = distance;
        ballDirection = direction;
    }
    
    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(ballVisible);
        sensor.AddObservation(ballDistance);
        sensor.AddObservation(ballDirection);
    }
    
    public override void OnActionReceived(float[] vectorAction)
    {
        if (ballVisible && ballDirection < 5 && ballDirection > -5)
        {
            SetReward(1.0f);
            //EndEpisode();
        }

        int turnAmount = (int) (vectorAction[0] * 180f);
        
        rcPlayer.Send($"(turn {turnAmount})");
    }
    
    public override float[] Heuristic()
    {
        var action = new float[2];
        action[0] = Input.GetAxis("Horizontal");
        action[1] = Input.GetAxis("Vertical");
        return action;
    }
}