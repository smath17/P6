using System.Collections.Generic;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using UnityEngine;

public class RoboCupAttackerAgent : Agent
{
    RcPlayer player;
    RcCoach coach;
    
    //rewards
    bool rewardKickTowardsGoal = true;
    bool rewardBallMoveToGoal = true;
    bool penalizeBallMoveToOwnGoal = true;
    
    [Header("References")]
    public RewardDisplay rewardDisplay;
    public ObservationDisplay observationDisplay;
    
    List<string> observationNames = new List<string>();
    List<float> observations = new List<float>();

    float selfDirection;

    int kickBallCount;

    bool ballVisible;
    float ballDirection;
    float ballDistance;
    
    bool goalLeftFlagVisible;
    bool goalRightFlagVisible;
    float goalLeftFlagDirection;
    float goalRightFlagDirection;
    
    bool ownGoalVisible;
    float ownGoalDirection;
    
    bool leftSideVisible;
    float leftSideDirection;
    
    bool rightSideVisible;
    float rightSideDirection;
    
    float bestPlayerDistanceFromBallThisEpisode;
    float bestBallDistanceFromEnemyGoalThisEpisode;
    float worstBallDistanceFromOwnGoalThisEpisode;
    
    bool hasKicked;

    bool defenderVisible;
    float defenderDirection;
    float defenderDistance;
    
    bool goalVisible;
    int goalDirection;
    int goalDistance;

    int dashSpeed = 100;

    public bool printRewards;

    public void SetPlayer(RcPlayer player)
    {
        this.player = player;
    }

    public void SetCoach(RcCoach coach)
    {
        this.coach = coach;
    }

    public override void OnEpisodeBegin()
    {
    }
    
    public void SetSelfInfo(int kickBallCount)
    {
        if (this.kickBallCount < kickBallCount)
        {
            AddReward(0.1f);
            if (rewardKickTowardsGoal && goalLeftFlagVisible && goalRightFlagVisible)
            {
                AddReward(0.3f);
                if (goalLeftFlagDirection < 0 && goalRightFlagDirection > 0)
                {
                    AddReward(0.6f);
                }
            }
            hasKicked = true;
        }

        this.kickBallCount = kickBallCount;
    }
    
    public void SetBallInfo(bool visible, float direction = 0, float distance = 0)
    {
        ballVisible = visible;
        ballDirection = direction;
        ballDistance = distance;
    }
    
    public void SetGoalInfo(bool leftFlagVisible, float leftFlagDirection, bool rightFlagVisible, float rightFlagDirection)
    {
        goalLeftFlagVisible = leftFlagVisible;
        goalLeftFlagDirection = leftFlagDirection;

        goalRightFlagVisible = rightFlagVisible;
        goalRightFlagDirection = rightFlagDirection;
    }
    
    public void SetDefenderInfo(bool visible, float direction = 0, float distance = 0)
    {
        defenderVisible = visible;
        defenderDirection = direction;
        defenderDistance = distance;
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        AddObservation(sensor, "bDir", ballVisible ? ballDirection / 45 : -1);
        AddObservation(sensor, "bDist", ballVisible ? ballDistance / 65 : -1);

        AddObservation(sensor, "enemyGL", goalLeftFlagVisible ? goalLeftFlagDirection / 45 : -1);
        AddObservation(sensor, "enemyGR", goalRightFlagVisible ? goalRightFlagDirection / 45 : -1);

        AddObservation(sensor, "ownG", ownGoalVisible ? ownGoalDirection / 45 : -1);
        AddObservation(sensor, "left", leftSideVisible ? leftSideDirection / 45 : -1);
        AddObservation(sensor, "right", rightSideVisible ? rightSideDirection / 45 : -1);

        AddObservation(sensor, "defDir", defenderVisible ? defenderDirection / 45 : -1);
        AddObservation(sensor, "defDist", defenderVisible ? defenderDistance / 60 : -1);
        
        DisplayObservations();
    }
    
    void AddObservation(VectorSensor sensor, string name, float obs)
    {
        sensor.AddObservation(obs);
        observationNames.Add(name);
        observations.Add(obs);
    }

    void DisplayObservations()
    {
        if (observationDisplay != null)
            observationDisplay.DisplayObservations(observationNames, observations);
        
        observationNames.Clear();
        observations.Clear();
    }

    public override void OnActionReceived(float[] vectorAction)
    {
        int action = Mathf.FloorToInt(vectorAction[0]);

        switch (action)
        {
            case 0: // move
                player.Dash(dashSpeed, 0);
                break;
            
            case 1: // turn left
                player.Turn(-10);
                break;
            
            case 2: // turn right
                player.Turn(10);
                break;
            
            case 3: // kick
                player.Kick(100);
                break;
        }
        
        DoRewards();
    }

    void DoRewards()
    {
        if(!ballVisible)
            OnBallNotVisible();
        
        if (rewardDisplay != null)
            rewardDisplay.DisplayRewards(GetReward(), GetCumulativeReward());
    }
    
    
    public void OnBallWithinRange()
    {
        //AddReward(0.2f, "Ball Within Range");
    }

    public void OnKickedBall()
    {
        //AddReward(0.2f, "Kicked Ball");
    }
    
    public void OnBallMovedLeft(float distanceFromGoal)
    {
        if (!penalizeBallMoveToOwnGoal)
            return;
        
        if (distanceFromGoal < worstBallDistanceFromOwnGoalThisEpisode - 2.5f)
        {
            worstBallDistanceFromOwnGoalThisEpisode = distanceFromGoal;
            float ballMoveReward = (distanceFromGoal - 50) / (0.7f - 50);
            if (ballMoveReward > 1)
            {
                ballMoveReward = 1;
            }
        
            ballMoveReward *= 0.1f;
        
            if (ballMoveReward > 0)
            {
                //AddReward(-ballMoveReward);
            }
        } 
    }

    public void OnScored()
    {
        AddReward(1f, "Scored");
        EndEpisode();
    }

    public void OnTimePassed()
    {
        //AddReward(-0.1f, "Time Passed");
    }

    public void OnBallNotVisible()
    {
        AddReward(-0.02f, "Ball Not Visible");
    }
    
    public void OnLookRight()
    {
        //AddReward(-0.5f, "Looked To The Right");
    }
    
    public void OnEnteredGoalArea()
    {
        //AddReward(-0.5f, "Entered Goal Area");
    }

    public void OnFailedToScore()
    {
        //AddReward(-1f, "Failed To Score");
        EndEpisode();
    }

    void AddReward(float reward, string reason)
    {
        if (printRewards)
            Debug.LogWarning($"{name} {reward} ({reason})");
        
        base.AddReward(reward);

        if (rewardDisplay != null)
            rewardDisplay.DisplayRewards(GetReward(), GetCumulativeReward());
    }
    
    void SetReward(float reward, string reason)
    {
        if (printRewards)
            Debug.LogWarning($"{name} ={reward} ({reason})");

        base.SetReward(reward);
        
        if (rewardDisplay != null)
            rewardDisplay.DisplayRewards(GetReward(), GetCumulativeReward());
    }

    public override void Heuristic(float[] actionsOut)
    {
        actionsOut[0] = 0;
        
        if (Input.GetKey(KeyCode.Q))
            actionsOut[0] = 1;
        
        if (Input.GetKey(KeyCode.E))
            actionsOut[0] = 2;
        
        if (Input.GetAxis("Vertical") > 0.5f)
            actionsOut[0] = 3;
        
        if (Input.GetAxis("Vertical") < -0.5f)
            actionsOut[0] = 4;

        if (Input.GetAxis("Horizontal") < -0.5f)
            actionsOut[0] = 5;
        
        if (Input.GetAxis("Horizontal") > 0.5f)
            actionsOut[0] = 6;
        
        if (Input.GetKey(KeyCode.Space))
            actionsOut[0] = 7;
    }

    public void SetOwnGoalInfo(bool visible, float direction = 0)
    {
        ownGoalVisible = visible;
        ownGoalDirection = direction;
    }

    public void SetLeftSideInfo(bool visible, float direction = 0)
    {
        leftSideVisible = visible;
        leftSideDirection = direction;
    }

    public void SetRightSideInfo(bool visible, float direction = 0)
    {
        rightSideVisible = visible;
        rightSideDirection = direction;
    }

    public void OnBallMovedRight(float distanceFromGoal)
    {
        if (!rewardBallMoveToGoal)
            return;
                
        if (distanceFromGoal < bestBallDistanceFromEnemyGoalThisEpisode - 2.5f)
        {
            bestBallDistanceFromEnemyGoalThisEpisode = distanceFromGoal;
            float ballMoveReward = (distanceFromGoal - 50) / (0.7f - 50);
            if (ballMoveReward > 1)
            {
                ballMoveReward = 1;
            }
        
            ballMoveReward *= 0.15f;
        
            if (ballMoveReward > 0)
            {
                //AddReward(ballMoveReward);
            }
        } 
    }

    public void OnBallEnteredLeftSide()
    {
        //AddReward(-0.1f);
    }

    public void OnBallOutOfField()
    {
        //AddReward(-1f);
                EndEpisode();
    }
}