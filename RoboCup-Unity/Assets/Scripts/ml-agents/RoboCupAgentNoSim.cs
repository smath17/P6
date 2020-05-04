using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Random = UnityEngine.Random;

//TODO: Merge this into RoboCupAgent.cs and make it work with/without SoccerSim
public class RoboCupAgentNoSim : Agent
{
    bool ballVisible;
    float ballDistance;
    float ballDirection;

    [Header("Settings")]
    public bool resetBallEachEpisode = true;
    
    public Transform player;
    
    Transform ball;

    float nextRotation;
    
    int playerX = -20;
    int playerY = 0;

    int stepsPerEpisode = 100;
    int stepsLeftInCurrentEpisode;

    float rewardLookAtBall = 1f;
    float rewardBallNotVisible = -1f;

    void Awake()
    {
        GameObject ballObj = Instantiate(Resources.Load<GameObject>("prefabs/visual3D/Ball"));
        ball = ballObj.transform;
        
        if (player != null)
        {
            player.position = new Vector3(playerX, 0, playerY);
        }
    }

    void ResetBallPosition()
    {
        if (ball != null)
        {
            int ballX = Random.Range(-10, 10) + playerX;
            int ballY = Random.Range(-10, 10) + playerY;

            ball.position = new Vector3(ballX, 0, ballY);
        }
    }

    public override void OnEpisodeBegin()
    {
        stepsLeftInCurrentEpisode = stepsPerEpisode;
        
        if (resetBallEachEpisode)
            ResetBallPosition();
    }

    public void FixedUpdate()
    {
        player.Rotate(Vector3.up, nextRotation);
        nextRotation = 0;
        
        ballDirection = (int)(Vector3.SignedAngle(player.forward, ball.position - player.position, Vector3.up));
        ballVisible = ballDirection < 45 && ballDirection > -45;

        RequestDecision();
        
        stepsLeftInCurrentEpisode--;
        if (stepsLeftInCurrentEpisode < 1)
            EndEpisode();
    }
    
    public override void CollectObservations(VectorSensor sensor)
    {
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
                SetReward(rewardLookAtBall);
            }
        }
        else
        {
            SetReward(rewardBallNotVisible);
        }

        int turnAmount = (int) (vectorAction[0] * 180f);

        nextRotation = turnAmount;
    }

    public override void Heuristic(float[] actionsOut)
    {
        actionsOut[0] = Input.GetAxis("Horizontal");
    }
}