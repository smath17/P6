using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;

public class RoboCupAgent : Agent
{
    bool ballVisible;
    float ballDistance;
    float ballDirection;

    RcPlayer rcPlayer;
    RcCoach rcCoach;

    public void SetPlayer(RcPlayer player)
    {
        rcPlayer = player;
    }
    
    public void SetCoach(RcCoach coach)
    {
        rcCoach = coach;
    }
    
    public override void OnEpisodeBegin()
    {
        //Debug.LogError("OnEpisodeBegin called");
        if (rcPlayer != null)
        {
            int playerX = -20;
            int playerY = 0;
            
            rcPlayer.Send($"(move {playerX} {playerY})");

            if (rcCoach != null)
            {
                int ballX = Random.Range(-10, 10) + playerX;
                int ballY = Random.Range(-10, 10) + playerY;

                rcCoach.Send($"(move (ball) {ballX} {ballY})");
            }
        }
    }

    public void SetBallInfo(bool visible, float distance = 0, float direction = 0)
    {
        ballVisible = visible;
        ballDistance = distance;
        ballDirection = direction;
    }
    
    public override void CollectObservations(VectorSensor sensor)
    {
        //sensor.AddObservation(ballVisible);
        //sensor.AddObservation(ballDistance);
        
        if (ballVisible)
            sensor.AddObservation(ballDirection / 45);
        else
            sensor.AddObservation(-1);
    }
    
    public override void OnActionReceived(float[] vectorAction)
    {
        if (ballVisible)
        {
            if (ballDirection < 5 && ballDirection > -5)
            {
                SetReward(1.0f);
                EndEpisode();
            }
        }
        else
        {
            SetReward(-0.5f);
        }

        int turnAmount = (int) (vectorAction[0] * 180f);
        
        rcPlayer.Send($"(turn {turnAmount})");
    }

    public override void Heuristic(float[] actionsOut)
    {
        actionsOut[0] = Input.GetAxis("Horizontal");
    }
}