using System;
using UnityEngine;

public class CombinedAgent : MonoBehaviour, RcAgent
{
    public enum AgentState {MoveToBall, KickToGoal}

    [SerializeField]
    AgentState currentState;
    
    RcPlayer player;
    RcCoach coach;

    public GameObject mover;
    public GameObject kicker;
    
    RcAgent moveAgent;
    RcAgent kickAgent;

    bool initialized;
    bool realMatch;

    void Awake()
    {
        moveAgent = mover.GetComponent<RcAgent>();
        kickAgent = kicker.GetComponent<RcAgent>();
    }

    public void SetAgentTrainer(AgentTrainer agentTrainer)
    {
        moveAgent.SetAgentTrainer(agentTrainer);
        kickAgent.SetAgentTrainer(agentTrainer);
    }

    public void SetPlayer(RcPlayer player)
    {
        this.player = player;
        moveAgent.SetPlayer(player);
        kickAgent.SetPlayer(player);
    }

    public void Init(bool realMatch)
    {
        initialized = true;
        this.realMatch = realMatch;
        coach = RoboCup.singleton.GetCoach();
        
        moveAgent.Init(realMatch);
        kickAgent.Init(realMatch);
    }

    public void RequestDecision()
    {
        if (player.IsKicking())
            currentState = AgentState.MoveToBall;
        
        switch (currentState)
        {
            case AgentState.MoveToBall:
                moveAgent.RequestDecision();
                break;
            case AgentState.KickToGoal:
                kickAgent.RequestDecision();
                break;
        }
    }
    
    public void SetSelfInfo(int kickBallCount)
    {
        moveAgent.SetSelfInfo(kickBallCount);
        kickAgent.SetSelfInfo(kickBallCount);
    }

    public void SetOpponentInfo(bool visible, float direction, float distance)
    {
        moveAgent.SetOpponentInfo(visible, direction, distance);
        kickAgent.SetOpponentInfo(visible, direction, distance);
    }

    public void SetBallInfo(bool visible, float direction = 0, float distance = 0)
    {
        if (visible)
        {
            if (distance < 1)
                currentState = AgentState.KickToGoal;
            else
                currentState = AgentState.MoveToBall;
        }
        
        moveAgent.SetBallInfo(visible, direction, distance);
        kickAgent.SetBallInfo(visible, direction, distance);
    }
    
    public void SetGoalInfo(bool leftFlagVisible, float leftFlagDirection, bool rightFlagVisible, float rightFlagDirection)
    {
        moveAgent.SetGoalInfo(leftFlagVisible, leftFlagDirection, rightFlagVisible, rightFlagDirection);
        kickAgent.SetGoalInfo(leftFlagVisible, leftFlagDirection, rightFlagVisible, rightFlagDirection);
    }
    
    public void SetOwnGoalInfo(bool visible, float direction = 0)
    {
        moveAgent.SetOwnGoalInfo(visible, direction);
        kickAgent.SetOwnGoalInfo(visible, direction);
    }
    
    public void SetLeftSideInfo(bool visible, float direction = 0)
    {
        moveAgent.SetLeftSideInfo(visible, direction);
        kickAgent.SetLeftSideInfo(visible, direction);
    }
    
    public void SetRightSideInfo(bool visible, float direction = 0)
    {
        moveAgent.SetRightSideInfo(visible, direction);
        kickAgent.SetRightSideInfo(visible, direction);
    }

    public void EndEpisode()
    {
        moveAgent.EndEpisode();
        kickAgent.EndEpisode();
    }

    public RcPlayer GetPlayer()
    {
        return player;
    }
}
