using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using UnityEngine;

public class RoboCupDefenderAgent : Agent
{
    RcPlayer player;
    RcCoach coach;

    float selfPositionX;
    float selfPositionY;
    float selfDirection;

    bool ballVisible;
    int ballDirection;
    int ballDistance;
    
    bool attackerVisible;
    int attackerDirection;
    int attackerDistance;
    
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
    
    public void SetAttackerInfo(bool visible, int direction = 0, int distance = 0)
    {
        attackerVisible = visible;
        attackerDirection = direction;
        attackerDistance = distance;
    }
    
    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(selfPositionX);
        sensor.AddObservation(selfPositionY);
        sensor.AddObservation(selfDirection);

        sensor.AddObservation(ballVisible ? ballDirection : -1);
        sensor.AddObservation(ballVisible ? ballDistance : -1);
        
        sensor.AddObservation(attackerVisible ? attackerDirection : -1);
        sensor.AddObservation(attackerVisible ? attackerDistance : -1);
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
            
            case 7: // catch
                player.Catch();
                break;
        }

    }
    
    public void OnLostBall()
    {
        AddReward(-0.2f);
    }

    public void OnLeftGoalArea()
    {
        AddReward(-0.5f);
    }

    public void OnDefended()
    {
        SetReward(1f);
        EndEpisode();
    }
    
    public void OnFailedToDefend()
    {
        SetReward(-1f);
        EndEpisode();
    }

    void AddReward(float reward, string reason)
    {
        if (printRewards)
            Debug.LogWarning($"AddReward: {reward}");
        base.AddReward(reward);
    }
    
    void SetReward(float reward, string reason)
    {
        if (printRewards)
            Debug.LogWarning($"SetReward: {reward}");
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