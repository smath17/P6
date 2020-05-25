using System.Collections.Generic;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using UnityEngine;
using Random = UnityEngine.Random;

public class RoboCupKickerAgent : Agent, RcAgent
{
    public enum TrainingPhase {NoTraining, CloseToGoalKick, CloseToGoalMoveAndKick, FarFromGoalMoveAndKick, RandomPositionsMoveAndKick, OnlyKick, OnlyMove}
    
    AgentTrainer agentTrainer;
    
    RcPlayer player;
    RcCoach coach;
    
    RewardDisplay rewardDisplay;
    ObservationDisplay observationDisplay;

    [Header("Settings")]
    public TrainingPhase trainingPhase = TrainingPhase.CloseToGoalKick;
    public bool rewardMoveToBall;
    public bool rewardKickTowardsGoal;
    public bool rewardBallMoveToGoal;
    public bool penalizeBallNotVisible;
    public bool penalizeBallMoveToOwnGoal;
    
    List<string> observationNames = new List<string>();
    List<float> observations = new List<float>();
    
    bool ballVisible;
    float ballDistance;
    float ballDirection;
    
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

    int kickBallCount;

    bool hasKicked;

    [SerializeField] int goalsScoredCurPos;
    [SerializeField] int goalsScoredOverall;
    [SerializeField] int ballsOOBCurPos;
    [SerializeField] int ballsOOBOverall;

    bool changePosition = true;

    bool initialized = false;
    bool realMatch;

    int ballX = 20;
    int ballY = 0;
    
    int playerX = 0;
    int playerY = 0;

    Vector2[] startPositions = new[]
    {
        new Vector2(40, 0),
        new Vector2(35, 0),
        new Vector2(30, 0),
        new Vector2(25, 0),
        new Vector2(20, 0),
        
        new Vector2(40, 10),
        new Vector2(40, -10),
        new Vector2(35, 10),
        new Vector2(35, -10),
        new Vector2(30, 10),
        new Vector2(30, -10),
        new Vector2(25, 10),
        new Vector2(25, -10),
        
        new Vector2(40, 15),
        new Vector2(40, -15),
        new Vector2(35, 15),
        new Vector2(35, -15),
        new Vector2(30, 15),
        new Vector2(30, -15),
        
        new Vector2(40, 20),
        new Vector2(40, -20),
        new Vector2(35, 20),
        new Vector2(35, -20),
        new Vector2(30, 20),
        new Vector2(30, -20),
    };

    int positionIndex;

    void Awake()
    {
        rewardDisplay = FindObjectOfType<RewardDisplay>();
        observationDisplay = FindObjectOfType<ObservationDisplay>();
    }

    public void Init(bool realMatch)
    {
        initialized = true;
        this.realMatch = realMatch;
        coach = RoboCup.singleton.GetCoach();
    }
    
    public void SetAgentTrainer(AgentTrainer agentTrainer)
    {
        this.agentTrainer = agentTrainer;
        agentTrainer.SetEpisodeLength(50);
    }
    
    public void SetPlayer(RcPlayer player)
    {
        this.player = player;
    }
    
    public override void OnEpisodeBegin()
    {
        if (!initialized)
            return;
        
        hasKicked = false;

        if (changePosition)
        {
            goalsScoredCurPos = 0;
            ballsOOBCurPos = 0;
        }
        
        if (!realMatch)
        {
            switch (trainingPhase)
            {
                case TrainingPhase.CloseToGoalKick:
                    if (changePosition)
                    {
                        ballX = (int)startPositions[positionIndex].x;
                        ballY = (int)startPositions[positionIndex].y;
                        playerX = ballX - 1;
                        playerY = ballY;

                        positionIndex++;
                        if (positionIndex > startPositions.Length - 1)
                            positionIndex = 0;

                        changePosition = false;
                    }
                    break;
                
                case TrainingPhase.CloseToGoalMoveAndKick:
                    if (changePosition)
                    {
                        ballX = (int)startPositions[positionIndex].x;
                        ballY = (int)startPositions[positionIndex].y;
                        playerX = ballX - 5;
                        playerY = ballY + ballY/2;

                        positionIndex++;
                        if (positionIndex > startPositions.Length - 1)
                            positionIndex = 0;

                        changePosition = false;
                    }
                    break;
                
                case TrainingPhase.FarFromGoalMoveAndKick:
                    if (changePosition)
                    {
                        ballX -= 10;
                        if (ballX < 10)
                        {
                            ballX = 45;
                
                            ballY += 10;
                            if (ballY > 30)
                            {
                                ballY = -30;
                            }
                        }

                        playerX = ballX - 5;
                        playerY = ballY;

                        changePosition = false;
                    }
                    break;
                
                case TrainingPhase.OnlyKick:
                    ballX += 10;
                    if (ballX > 50)
                    {
                        ballX = -50;
                
                        ballY += 10;
                        if (ballY > 30)
                        {
                            ballY = -30;
                        }
                    }
                    
                    playerX = ballX - 1;
                    playerY = ballY;
                    break;
                
                case TrainingPhase.RandomPositionsMoveAndKick:
                case TrainingPhase.OnlyMove:
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
                    break;
            }

            int direction = Random.Range(-180, 180);

            bestPlayerDistanceFromBallThisEpisode = Mathf.Infinity;
            bestBallDistanceFromEnemyGoalThisEpisode = Mathf.Infinity;
            worstBallDistanceFromOwnGoalThisEpisode = Mathf.Infinity;
        
            coach.MovePlayer(RoboCup.singleton.teamName, 1, playerX, playerY, direction);
            coach.Recover();
        
            coach.MoveBall(ballX, ballY);
        
            Vector2 ballPos = new Vector2(ballX, ballY);
            Vector2 playerPos = new Vector2(playerX, playerY);
            float distance = (ballPos - playerPos).magnitude;

            switch (trainingPhase)
            {
                case TrainingPhase.CloseToGoalKick:
                case TrainingPhase.OnlyKick:
                    agentTrainer.SetEpisodeLength(50);
                    break;
                
                case TrainingPhase.CloseToGoalMoveAndKick:
                case TrainingPhase.FarFromGoalMoveAndKick:
                    agentTrainer.SetEpisodeLength(75);
                    break;
                
                case TrainingPhase.RandomPositionsMoveAndKick:
                case TrainingPhase.OnlyMove:
                    agentTrainer.SetEpisodeLength(((int)distance + 1) * 8);
                    break;
            }
        }
    }
    
    public void SetSelfInfo(int kickBallCount)
    {
        if (this.kickBallCount < kickBallCount)
        {
            if (rewardKickTowardsGoal && goalLeftFlagVisible && goalRightFlagVisible)
            {
                AddReward(0.1f);
                if (goalLeftFlagDirection < 0 && goalRightFlagDirection > 0)
                {
                    AddReward(0.5f);
                }
            }
            hasKicked = true;
        }

        this.kickBallCount = kickBallCount;
        
    }

    public void SetOpponentInfo(bool visible, float direction, float distance)
    {
        // Not Implemented
    }

    public void SetBallInfo(bool visible, float direction = 0, float distance = 0)
    {
        ballVisible = visible;
        ballDirection = direction;
        ballDistance = distance;

        if (trainingPhase == TrainingPhase.OnlyMove && RoboCup.singleton.trainingScenario == AgentTrainer.TrainingScenario.MoveToBall)
        {
            if (visible && ballDistance < 1)
            {
                EndEpisode();
                agentTrainer.OnEpisodeBegin();
            }
        }
    }
    
    public void SetGoalInfo(bool leftFlagVisible, float leftFlagDirection, bool rightFlagVisible, float rightFlagDirection)
    {
        goalLeftFlagVisible = leftFlagVisible;
        goalLeftFlagDirection = leftFlagDirection;

        goalRightFlagVisible = rightFlagVisible;
        goalRightFlagDirection = rightFlagDirection;
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

    public RcPlayer GetPlayer()
    {
        return player;
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

    public override void CollectDiscreteActionMasks(DiscreteActionMasker actionMasker)
    {
        switch (trainingPhase)
        {
            case TrainingPhase.CloseToGoalKick:
            case TrainingPhase.OnlyKick:
                if (hasKicked)
                    actionMasker.SetMask(0, new int[4]{1,2,3,4}); // Can't do anything after kicking ball
                else
                    actionMasker.SetMask(0, new int[2]{0,1}); // Not able to stand still or dash
                break;
            
            case TrainingPhase.CloseToGoalMoveAndKick:
                if (hasKicked)
                    actionMasker.SetMask(0, new int[4]{1,2,3,4}); // Can't do anything after kicking ball
                break;
                
            case TrainingPhase.OnlyMove:
                actionMasker.SetMask(0, new int[2]{0,4}); // Can't stand still or kick
                break;
        }
    }


    public override void OnActionReceived(float[] vectorAction)
    {
        int action = Mathf.FloorToInt(vectorAction[0]);
        
        switch (action)
        {
            case 0:
                //player.Dash(0, 0);
                break;
            
            case 1:
                //if (ballVisible && ballDistance < 1)
                //    player.Kick(100);
                //else
                //if (ballVisible && ballDistance < 10)
                //    player.Dash(10 + (9 * (int)ballDistance), 0);
                //else
                    player.Dash(100, 0);
                break;
            
            case 2:
                player.Turn(-15);
                break;
            
            case 3:
                player.Turn(15);
                break;
            
            case 4:
                player.Kick(100);
                ////Penalty for kicking nothing
                //if (ballVisible || ballDistance > 1f)
                //    AddReward(-0.05f);
                break;
        }
        
        DoRewards();
        
        Academy.Instance.StatsRecorder.Add("RoboCup/GoalsScored", goalsScoredOverall);
        Academy.Instance.StatsRecorder.Add("RoboCup/BallsOutOfBounds", ballsOOBOverall);
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
        
        if (Input.GetKeyDown(KeyCode.Space))
            actionsOut[0] = 4;
    }
    
    void DoRewards()
    {
        if (ballVisible)
        {
            if (rewardMoveToBall && ballDistance >= 0.7 && ballDistance < bestPlayerDistanceFromBallThisEpisode - 2.5f)
            {
                bestPlayerDistanceFromBallThisEpisode = ballDistance;
                float distanceReward = (ballDistance - 50) / (0.7f - 50);
                if (distanceReward > 1)
                {
                    distanceReward = 1;
                }
        
                distanceReward *= 0.05f;
        
                if (distanceReward > 0)
                {
                    AddReward(distanceReward);
                }
            } 
        }
        else
        {
            if (penalizeBallNotVisible)
                AddReward(-0.02f);
        }
        
        if (rewardDisplay != null)
            rewardDisplay.DisplayRewards(GetReward(), GetCumulativeReward());
    }

    public void OnBallMovedRight(float distanceFromGoal)
    {
        if (!rewardBallMoveToGoal)
            return;
        
        if (distanceFromGoal < bestBallDistanceFromEnemyGoalThisEpisode - 2.5f)
        {
            bestBallDistanceFromEnemyGoalThisEpisode = distanceFromGoal;
            float ballMoveReward = (distanceFromGoal - 100) / (0.7f - 100);
            if (ballMoveReward > 1)
            {
                ballMoveReward = 1;
            }
        
            ballMoveReward *= 0.15f;
        
            if (ballMoveReward > 0)
            {
                AddReward(ballMoveReward);
            }
        } 
    }
    
    public void OnBallMovedLeft(float distanceFromGoal)
    {
        if (!penalizeBallMoveToOwnGoal)
            return;
        
        if (distanceFromGoal < worstBallDistanceFromOwnGoalThisEpisode - 2.5f)
        {
            worstBallDistanceFromOwnGoalThisEpisode = distanceFromGoal;
            float ballMoveReward = (distanceFromGoal - 100) / (0.7f - 100);
            if (ballMoveReward > 1)
            {
                ballMoveReward = 1;
            }
        
            ballMoveReward *= 0.1f;
        
            if (ballMoveReward > 0)
            {
                AddReward(-ballMoveReward);
            }
        } 
    }

    public void OnBallEnteredLeftSide()
    {
        switch (trainingPhase)
        {
            case TrainingPhase.CloseToGoalKick:
            case TrainingPhase.CloseToGoalMoveAndKick:
            case TrainingPhase.FarFromGoalMoveAndKick:
                AddReward(-1f);
                EndEpisode();
                agentTrainer.OnEpisodeBegin();
                break;
        }
    }

    public void OnScored()
    {
        goalsScoredCurPos++;
        goalsScoredOverall++;

        if (goalsScoredCurPos > 9)
            changePosition = true;

        if (trainingPhase != TrainingPhase.NoTraining ||
            trainingPhase != TrainingPhase.OnlyKick ||
            trainingPhase != TrainingPhase.OnlyMove)
        {
        
            if (goalsScoredOverall > 300)
                trainingPhase = TrainingPhase.CloseToGoalMoveAndKick;

            if (goalsScoredOverall > 600)
                trainingPhase = TrainingPhase.FarFromGoalMoveAndKick;
        
            if (goalsScoredOverall > 1000)
                trainingPhase = TrainingPhase.RandomPositionsMoveAndKick;
        }
        
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
        ballsOOBCurPos++;
        ballsOOBOverall++;
        
        AddReward(-1f);
        EndEpisode();
    }
}
