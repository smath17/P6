using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using UnityEngine;

public class RoboCupKickerAgent : Agent
{
    AgentTrainer agentTrainer;
    
    IPlayer player;
    ICoach coach;
    
    [Header("Settings")]
    public RewardDisplay rewardDisplay;
    
    bool ballVisible;
    float ballDistance;
    float ballDirection;
    
    bool goalVisible;
    float goalDirection;
    //float goalDistance;

    float bestPlayerDistanceFromBallThisEpisode;
    float bestBallDistanceFromGoalThisEpisode;

    int kickBallCount;

    bool realMatch;

    int ballX = -50;
    int ballY = -30;
    
    int playerX = 0;
    int playerY = 0;

    public void SetRealMatch()
    {
        realMatch = true;
    }
    
    public void SetAgentTrainer(AgentTrainer agentTrainer)
    {
        this.agentTrainer = agentTrainer;
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
        if (!realMatch)
        {
            ballX += 10;
            if (ballX > 50)
            {
                ballX = -50;
                
                ballY += 10;
                if (ballY > 30)
                {
                    ballY = -30;

                    playerX += 10;
                    if (playerX > 50)
                    {
                        playerX = -50;

                        playerY += 10;
                        if (playerY > 30)
                        {
                            playerY = -30;
                        }
                    }
                }
            }

            int direction = Random.Range(-180, 180);

            bestPlayerDistanceFromBallThisEpisode = Mathf.Infinity;
            bestBallDistanceFromGoalThisEpisode = Mathf.Infinity;
        
            coach.MovePlayer(RoboCup.singleton.teamName, 1, playerX, playerY, direction);
            coach.Recover();
        
            coach.MoveBall(ballX, ballY);
        
            Vector2 ballPos = new Vector2(ballX, ballY);
            Vector2 playerPos = new Vector2(playerX, playerY);
            float distance = (ballPos - playerPos).magnitude;
        
            agentTrainer.SetEpisodeLength(((int)distance + 1) * 8);
        }
    }
    
    
    
    public void SetSelfInfo(int kickBallCount)
    {
        if (this.kickBallCount < kickBallCount)
        {
            if (ballVisible && goalVisible)
            {
                AddReward(0.5f);
                if (ballDirection < goalDirection + 10 && ballDirection > goalDirection - 10)
                {
                    AddReward(0.5f);
                    Debug.LogWarning("Good Kick");
                }
            }
        }


        this.kickBallCount = kickBallCount;
        
    }
    
    public void SetBallInfo(bool visible, float direction = 0, float distance = 0)
    {
        ballVisible = visible;
        ballDirection = direction;
        ballDistance = distance;
    }
    
    public void SetGoalInfo(bool visible, float direction = 0, float distance = 0)
    {
        goalVisible = visible;
        goalDirection = direction;
        //goalDistance = distance;
    }
    
    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(ballVisible ? ballDirection / 45 : -1);
        sensor.AddObservation(ballVisible ? ballDistance / 65 : -1);
        
        sensor.AddObservation(goalVisible ? goalDirection / 45 : -1);
        //sensor.AddObservation(goalVisible ? goalDistance : -1);
    }
    
    public override void OnActionReceived(float[] vectorAction)
    {
        int action = Mathf.FloorToInt(vectorAction[0]);
        
        switch (action)
        {
            //case 0:
            //    player.Dash(0, 0);
            //    break;
            
            case 0:
                if (ballVisible && ballDistance < 1)
                    player.Kick(100);
                else
                    player.Dash(100, 0);
                break;
            
            case 1:
                player.Turn(-30);
                break;
            
            case 2:
                player.Turn(30);
                break;
            
            //case 3:
            //    player.Kick(100);
            //    break;
        }
        
        DoRewards();
    }
    
    public override void Heuristic(float[] actionsOut)
    {
        actionsOut[0] = 0;

        if (Input.GetAxis("Vertical") > 0.5f)
            actionsOut[0] = 0;

        if (Input.GetAxis("Horizontal") < -0.5f)
            actionsOut[0] = 1;
        
        if (Input.GetAxis("Horizontal") > 0.5f)
            actionsOut[0] = 2;
        
        //if (Input.GetKeyDown(KeyCode.Space))
        //    actionsOut[0] = 3;
    }
    
    void DoRewards()
    {
        if (ballVisible)
        {
            if (ballDistance >= 0.7 && ballDistance < bestPlayerDistanceFromBallThisEpisode - 2.5f)
            {
                bestPlayerDistanceFromBallThisEpisode = ballDistance;
                float distanceReward = (ballDistance - 50) / (0.7f - 50);
                if (distanceReward > 1)
                {
                    distanceReward = 1;
                }

                distanceReward *= 0.25f;

                if (distanceReward > 0)
                {
                    AddReward(distanceReward);
                }
            } 
        }
        else
        {
            SetReward(-0.1f);
        }
        
        if (rewardDisplay != null)
            rewardDisplay.DisplayRewards(GetReward(), GetCumulativeReward());
    }

    public void OnBallMoved(float distanceFromGoal)
    {
        if (distanceFromGoal < bestBallDistanceFromGoalThisEpisode - 2.5f)
        {
            bestBallDistanceFromGoalThisEpisode = distanceFromGoal;
            float ballMoveReward = (distanceFromGoal - 50) / (0.7f - 50);
            if (ballMoveReward > 1)
            {
                ballMoveReward = 1;
            }

            ballMoveReward *= 0.75f;

            if (ballMoveReward > 0)
            {
                AddReward(ballMoveReward);
            }
        } 
    }

    public void OnScored()
    {
        AddReward(1f);
        EndEpisode();
    }
    
    public void OnOutOfField()
    {
        AddReward(-0.5f);
        EndEpisode();
    }
    
    public void OnBallOutOfField()
    {
        AddReward(-1f);
        EndEpisode();
    }
}
