using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using UnityEngine;

public class RoboCupAttackerAgent : Agent
{
    IPlayer player;
    ICoach coach;

    float selfPositionX;
    float selfPositionY;
    float selfDirection;

    bool ballVisible;
    int ballDirection;
    int ballDistance;

    bool defenderVisible;
    int defenderDirection;
    int defenderDistance;
    
    bool goalVisible;
    int goalDirection;
    int goalDistance;

    int playerStartX = -20;
    int playerStartY = 0;

    int dashSpeed = 100;

    public bool printRewards;

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
        coach.MoveBall(-25, 0);
        coach.MovePlayer(RoboCup.singleton.GetEnemyTeamName(), 1, playerStartX, playerStartY, 180);
        coach.Recover();
    }
    
    public void SetSelfInfo(float positionX, float positionY, float direction)
    {
        selfPositionX = positionX;
        selfPositionY = positionY;
        selfDirection = direction;
    }
    
    public void SetBallInfo(bool visible, int direction = 0, int distance = 0)
    {
        if (ballVisible && !visible)
            OnLostBall();
        
        ballVisible = visible;
        ballDirection = direction;
        ballDistance = distance;
    }
    
    public void SetGoalInfo(bool visible, int direction = 0, int distance = 0)
    {
        goalVisible = visible;
        goalDirection = direction;
        goalDistance = distance;
    }
    
    public void SetDefenderInfo(bool visible, int direction = 0, int distance = 0)
    {
        defenderVisible = visible;
        defenderDirection = direction;
        defenderDistance = distance;
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(selfPositionX);
        sensor.AddObservation(selfPositionY);
        sensor.AddObservation(selfDirection);

        sensor.AddObservation(ballVisible ? ballDirection : -1);
        sensor.AddObservation(ballVisible ? ballDistance : -1);
        
        sensor.AddObservation(goalVisible ? goalDirection : -1);
        sensor.AddObservation(goalVisible ? goalDistance : -1);

        sensor.AddObservation(defenderVisible ? defenderDirection : -1);
        sensor.AddObservation(defenderVisible ? defenderDistance : -1);
    }

    public override void OnActionReceived(float[] vectorAction)
    {
        int action = Mathf.FloorToInt(vectorAction[0]);

        switch (action)
        {
            case 0: // do nothing
                break;
            
            case 1: // move left
                player.Dash(dashSpeed, -90);
                break;
            
            case 2: // move right
                player.Dash(dashSpeed, 90);
                break;
            
            case 3: // move up
                player.Dash(dashSpeed, 0);
                break;
            
            case 4: // move down
                player.Dash(dashSpeed, 180);
                break;
            
            case 5: // turn left
                player.Turn(-10);
                break;
            
            case 6: // turn right
                player.Turn(10);
                break;
            
            case 7: // kick
                player.Kick(100);
                break;
        }
    }

    public void OnTimePassed()
    {
        AddReward(-0.1f, "Time Passed");
    }

    public void OnKickedBall()
    {
        AddReward(0.2f, "Kicked Ball");
    }
    
    public void OnBallMovedLeft()
    {
        AddReward(0.1f, "Ball Moved Left");
    }

    public void OnLostBall()
    {
        AddReward(-0.2f, "Lost Ball");
    }
    
    public void OnEnteredGoalArea()
    {
        AddReward(-0.5f, "Entered Goal Area");
    }

    public void OnScored()
    {
        AddReward(1f, "Scored");
        EndEpisode();
    }

    public void OnFailedToScore()
    {
        AddReward(-1f, "Failed To Score");
        EndEpisode();
    }

    public void OnTimeOut()
    {
        EndEpisode();
    }

    void AddReward(float reward, string reason)
    {
        if (printRewards)
            Debug.LogWarning($"{name} {reward} ({reason})");
        base.AddReward(reward);
    }
    
    void SetReward(float reward, string reason)
    {
        if (printRewards)
            Debug.LogWarning($"{name} ={reward} ({reason})");
        base.SetReward(reward);
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
}