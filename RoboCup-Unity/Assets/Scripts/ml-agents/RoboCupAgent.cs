using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Random = UnityEngine.Random;

public class RoboCupAgent : Agent
{
    RoboCup.TrainingScenario trainingScenario;
    bool offlineTraining;

    IPlayer player;
    ICoach coach;
    
    [Header("Settings")]
    public bool resetBallEachEpisode = true;
    
    int playerStartX = -20;
    int playerStartY = 0;

    int stepsPerEpisode = 100;
    int stepsLeftInCurrentEpisode;

    float rewardLookAtBall = 1f;
    float rewardBallNotVisible = -1f;
    
    bool ballVisible;
    float ballDistance;
    float ballDirection;
    
    public void SetOfflineTraining(bool offline)
    {
        offlineTraining = offline;
    }
    
    public void SetTrainingScenario(RoboCup.TrainingScenario scenario)
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
    
    void Awake()
    {
        coach.InitTraining(this, player);
        coach.KickOff();
    }
    
    public override void OnEpisodeBegin()
    {
        player.Move(playerStartX, playerStartY);
        
        stepsLeftInCurrentEpisode = stepsPerEpisode;

        switch (trainingScenario)
        {
            case RoboCup.TrainingScenario.LookAtBall:
                BeginLookAtBall();
                break;
            case RoboCup.TrainingScenario.RunTowardsBall:
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
        
        coach.MovePlayer(playerStartX, playerStartY);
        coach.Recover();
        
        if (resetBallEachEpisode)
            coach.MoveBall(ballX, ballY);
    }

    public void SetBallInfo(bool visible, float distance = 0, float direction = 0)
    {
        ballVisible = visible;
        ballDistance = distance;
        ballDirection = direction;
    }

    public void Step()
    {
        if (offlineTraining)
            RequestDecision();
        
        stepsLeftInCurrentEpisode--;
        if (stepsLeftInCurrentEpisode < 1)
            EndEpisode();
    }
    
    public override void CollectObservations(VectorSensor sensor)
    {
        switch (trainingScenario)
        {
            case RoboCup.TrainingScenario.LookAtBall:
                if (ballVisible)
                    sensor.AddObservation(ballDirection / 45);
                else
                    sensor.AddObservation(-1);
                break;
            case RoboCup.TrainingScenario.RunTowardsBall:
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
            case RoboCup.TrainingScenario.LookAtBall:
                ActionLookAtBall(vectorAction);
                break;
            case RoboCup.TrainingScenario.RunTowardsBall:
                ActionRunTowardsBall(vectorAction);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    void ActionLookAtBall(float[] vectorAction)
    {
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

        int turnAmount = (int) (vectorAction[0] * 180f);

        player.Turn(turnAmount);
    }
    
    public void ActionRunTowardsBall(float[] vectorAction)
    {
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
                
        int dashAmount;
        if (vectorAction[0] < -0.5)
            dashAmount = -100;
        else if (vectorAction[0] > 0.5)
            dashAmount = 100;
        else
            dashAmount = 0;

        player.Dash(dashAmount, 0);
    }

    public override void Heuristic(float[] actionsOut)
    {
        actionsOut[0] = Input.GetAxis("Horizontal");
    }
}