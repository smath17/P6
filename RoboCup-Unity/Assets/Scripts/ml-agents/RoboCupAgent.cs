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

    float rewardLookAtBall = 1f;
    float rewardBallNotVisible = -1f;
    
    float rewardCloseToBall = 1f;
    
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
        
        int dashAmount = 0;
        int turnAmount = 0;
        bool dash = false;
        bool turn = false;
        
        switch (action)
        {
            case 0:
                dash = true;
                dashAmount = 0;
                break;
            
            case 1: 
                dash = true;
                dashAmount = 100;
                break;
            
            case 2:
                turn = true;
                turnAmount = -30;
                break;
            
            case 3:
                turn = true;
                turnAmount = 30;
                break;
        }

        if (dash)
            player.Dash(dashAmount, 0);
        else
            player.Turn(turnAmount);
        
        DoRewards();
    }

    void DoRewards()
    {
        if (ballVisible)
        {
            if (ballDistance < 5 && ballDistance > 0.0)
            {
                SetReward(rewardCloseToBall);
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