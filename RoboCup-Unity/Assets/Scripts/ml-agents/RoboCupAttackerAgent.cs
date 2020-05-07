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

    int playerStartX = 10;
    int playerStartY = 0;

    int dashSpeed = 100;

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
        coach.MoveBall(0, 0);
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
        if (vectorAction[0] < -0.5) // left
        {
            if (vectorAction[1] < -0.5) // down
                player.Dash(dashSpeed, -135); // south west
            else if (vectorAction[1] > 0.5) // up
                player.Dash(dashSpeed, -45); // north west
            else
                player.Dash(dashSpeed, -90); // west

        }
        else if (vectorAction[0] > 0.5) // right
        {
            if (vectorAction[1] < -0.5) // down
                player.Dash(dashSpeed, 135); // south east
            else if (vectorAction[1] > 0.5) // up
                player.Dash(dashSpeed, 45); // north east
            else
                player.Dash(dashSpeed, 90); // east
        }
        else
        {
            if (vectorAction[1] < -0.5) // down
                player.Dash(dashSpeed, 180); // south
            else if (vectorAction[1] > 0.5) // up
                player.Dash(dashSpeed, 0); // north
            //else
            //    do nothing
        }
        
        if (vectorAction[2] < -0.5) // turn left
            player.Turn(-10);
        else if (vectorAction[0] > 0.5) // turn right
            player.Turn(10);
        
        if (vectorAction[3] > 0.5f)
            player.Kick(100);
    }

    public void OnScored()
    {
        SetReward(1f);
    }

    public void OnFailedToScore()
    {
        SetReward(-1f);
    }

    public override void Heuristic(float[] actionsOut)
    {
        actionsOut[0] = Input.GetKey(KeyCode.A) ? -1f : Input.GetKey(KeyCode.D) ? 1f : 0f;
        actionsOut[1] = Input.GetAxis("Vertical");
        actionsOut[2] = Input.GetAxis("Horizontal");
        actionsOut[3] = Input.GetKey(KeyCode.Space) ? 1f : 0f;
    }
}