using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Random = UnityEngine.Random;

public class RoboCupAgent : Agent
{
    AgentTrainer.TrainingScenario trainingScenario;

    IPlayer player;
    ICoach coach;
    
    [Header("Settings")]
    public bool resetBallEachEpisode = true;
    
    bool ballVisible;
    float ballDistance;
    float ballDirection;

    public void SetTrainingScenario(AgentTrainer.TrainingScenario scenario)
    {
        trainingScenario = scenario;
    }

    public void SetPlayer(IPlayer player)
    {
        this.player = player;
    }
    
    public void SetCoach(ICoach coach)
    {
        this.coach = coach;
    }
    
    public override void OnEpisodeBegin()
    {
        int ballX = Random.Range(-52, 52);
        int ballY = Random.Range(-32, 32);
        
        int playerStartX = Random.Range(-52, 52);
        int playerStartY = Random.Range(-32, 32);
        
        coach.MovePlayer(RoboCup.singleton.teamName, 1, playerStartX, playerStartY);
        coach.Recover();
        
        if (resetBallEachEpisode)
            coach.MoveBall(ballX, ballY);
    }

    public void SetBallInfo(bool visible, float direction = 0, float distance = 0)
    {
        ballVisible = visible;
        ballDirection = direction;
        ballDistance = distance;
    }
    
    public override void CollectObservations(VectorSensor sensor)
    {

        if (ballVisible)
        {
            sensor.AddObservation(ballDistance);
            sensor.AddObservation(ballDirection / 45);
        }
        else
        {
            sensor.AddObservation(-1);
            sensor.AddObservation(-1);
        }

    }
    
    public override void OnActionReceived(float[] vectorAction)
    {
        int action = Mathf.FloorToInt(vectorAction[0]);
        
        switch (action)
        {
            case 0:
                break;
            
            case 1: 
                player.Dash(100, 0);
                break;
            
            case 2:
                player.Turn(-30);
                break;
            
            case 3:
                player.Turn(30);
                break;
        }
        
        DoRewards();
    }

    void DoRewards()
    {
        if (ballVisible)
        {
            if (ballDistance >= 0.7)
            {
                float reward = 1 / ballDistance;
                if (reward > 1)
                    reward = 1;
                
                SetReward(reward);
            } 
        }
    }

    public override void Heuristic(float[] actionsOut)
    {
        actionsOut[0] = 0;

        if (Input.GetAxis("Vertical") > 0.5f)
            actionsOut[0] = 1;

        if (Input.GetAxis("Horizontal") < -0.5f)
            actionsOut[0] = 2;
        
        if (Input.GetAxis("Horizontal") > 0.5f)
            actionsOut[0] = 3;
    }
}