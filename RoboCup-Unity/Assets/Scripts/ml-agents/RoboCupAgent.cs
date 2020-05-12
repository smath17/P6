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
    
    int playerStartX = -20;
    int playerStartY = 0;

    float rewardLookAtBall = 1f;
    float rewardBallNotVisible = -1f;
    
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
        player.Move(playerStartX, playerStartY);

        switch (trainingScenario)
        {
            case AgentTrainer.TrainingScenario.LookAtBall:
                BeginLookAtBall();
                break;
            case AgentTrainer.TrainingScenario.RunTowardsBall:
                BeginRunTowardsBall();
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
    
    void BeginLookAtBall()
    {
        int ballX = Random.Range(-10, 10) + playerStartX;
        int ballY = Random.Range(-10, 10) + playerStartY;

        if (resetBallEachEpisode)
            coach.MoveBall(ballX, ballY);
    }

    void BeginRunTowardsBall()
    {
        int ballX = ballX = Random.Range(5, 30);
        int ballY = 0;
        
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
        switch (trainingScenario)
        {
            case AgentTrainer.TrainingScenario.LookAtBall:
                if (ballVisible)
                    sensor.AddObservation(ballDirection / 45);
                else
                    sensor.AddObservation(-1);
                break;
            case AgentTrainer.TrainingScenario.RunTowardsBall:
                if (ballVisible)
                    sensor.AddObservation(ballDistance);
                else
                    sensor.AddObservation(-1);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
    
    public override void OnActionReceived(float[] vectorAction)
    {
        switch (trainingScenario)
        {
            case AgentTrainer.TrainingScenario.LookAtBall:
                ActionLookAtBall(vectorAction);
                break;
            case AgentTrainer.TrainingScenario.RunTowardsBall:
                ActionRunTowardsBall(vectorAction);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
        
        DoRewards();
    }

    void ActionLookAtBall(float[] vectorAction)
    {
        int action = Mathf.FloorToInt(vectorAction[0]);
        
        int turnAmount = 0;
        
        switch (action)
        {
            case 0: 
                turnAmount = 0;
                break;
            
            case 1: 
                turnAmount = -30;
                break;
            
            case 2: 
                turnAmount = 30;
                break;
        }

        //int turnAmount = (int) (vectorAction[0] * 180f);
        player.Turn(turnAmount);
    }
    
    public void ActionRunTowardsBall(float[] vectorAction)
    {
        int action = Mathf.FloorToInt(vectorAction[0]);
        
        int dashAmount = 0;
        
        switch (action)
        {
            case 0: 
                dashAmount = 0;
                break;
            
            case 1: 
                dashAmount = -100;
                break;
            
            case 2: 
                dashAmount = 100;
                break;
        }

        player.Dash(dashAmount, 0);
    }
    
    void DoRewards()
    {
        switch (trainingScenario)
        {
            case AgentTrainer.TrainingScenario.LookAtBall:
                if (ballVisible)
                {
                    if (ballDirection < 5 && ballDirection > -5)
                    {
                        SetReward(rewardLookAtBall);
                    }
                }
                else
                {
                    SetReward(rewardBallNotVisible);
                }
                break;
            case AgentTrainer.TrainingScenario.RunTowardsBall:
                if (ballVisible)
                {
                    if (ballDistance < 0.7 && ballDistance > 0.0)
                    {
                        SetReward(1.0f);
                    }
                }
                else
                {
                    SetReward(-0.5f);
                }
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    public override void Heuristic(float[] actionsOut)
    {
        actionsOut[0] = Input.GetAxis("Horizontal");
    }
}