using System.Collections.Generic;
using UnityEngine;

public class AgentTrainer : MonoBehaviour
{
    public enum TrainingScenario
    {
        MoveToBall,
        KickToGoal,
        RunTowardsBallAndKick,
        AttackDefend
    }
    
    TrainingScenario trainingScenario;

    [Header("Other")]
    public GameObject offlineVisualPlayer;

    List<PlayerSetupInfo> team1Setup = new List<PlayerSetupInfo>();
    List<PlayerSetupInfo> team2Setup = new List<PlayerSetupInfo>();

    bool initialized;
    
    int stepsPerEpisode = 2000;
    int stepsLeftInCurrentEpisode;

    RcCoach coach;
    
    bool prevBallInGoalArea;
    bool curBallInGoalArea;

    AudioSource source;
    AudioEvent fail;
    AudioEvent win;
    AudioEvent timeout;
    
    void Awake()
    {
        source = GetComponent<AudioSource>();
        fail = Resources.Load<AudioEvent>("audio/fail");
        win = Resources.Load<AudioEvent>("audio/win");
        timeout = Resources.Load<AudioEvent>("audio/timeout");
    }

    public void SetupTeams(TrainingScenario trainingScenario)
    {
        this.trainingScenario = trainingScenario;
        
        switch (trainingScenario)
        {
            case TrainingScenario.MoveToBall:
                RoboCup.singleton.SetTeamName("MoveToBall");
                team1Setup.Add(new PlayerSetupInfo(false, -20, 0));
                break;
            
            case TrainingScenario.KickToGoal:
                RoboCup.singleton.SetTeamName("KickToGoal");
                team1Setup.Add(new PlayerSetupInfo(false, -20, 0));
                break;
            
            case TrainingScenario.RunTowardsBallAndKick:
                RoboCup.singleton.SetTeamName("RunAndKick");
                team1Setup.Add(new PlayerSetupInfo(false, -20, 0));
                break;
            
            case TrainingScenario.AttackDefend:
                RoboCup.singleton.SetTeamName("Defender");
                team1Setup.Add(new PlayerSetupInfo(true, -50, 0));
                
                RoboCup.singleton.SetEnemyTeamName("Attacker");
                team2Setup.Add(new PlayerSetupInfo(false, -50, 0));
                break;
        }
    }

    public void Init(List<RcAgent> team1Agents, List<RcAgent> team2Agents)
    {
        coach = RoboCup.singleton.GetCoach();
        
        coach.KickOff();

        foreach (RcAgent team1Agent in team1Agents)
        {
            team1Agent.Init(false);
            team1Agent.EndEpisode();
        }
        
        foreach (RcAgent team2Agent in team2Agents)
        {
            team2Agent.Init(false);
            team2Agent.EndEpisode();
        }
        
        initialized = true;
        
        OnEpisodeBegin();
        
        RoboCup.singleton.SetTrainerReady();
    }

    public void OnEpisodeBegin()
    {
        stepsLeftInCurrentEpisode = stepsPerEpisode;
    }

    // Returns true if the agent should make a decision afterwards,
    // and false if the current episode has just ended
    public bool TrainingStep(List<RcAgent> team1Agents, List<RcAgent> team2Agents)
    {
        if (!initialized)
            return false;
        
        stepsLeftInCurrentEpisode--;
        if (stepsLeftInCurrentEpisode < 1)
        {
            if (!RoboCup.singleton.seriousMode)
                timeout.Play(source);

            foreach (RcAgent team1Agent in team1Agents)
            {
                team1Agent.EndEpisode();
            }
            
            foreach (RcAgent team2Agent in team2Agents)
            {
                team2Agent.EndEpisode();
            }
            
            OnEpisodeBegin();
            return false;
        }

        switch (trainingScenario)
        {
            case TrainingScenario.MoveToBall:
            case TrainingScenario.KickToGoal:
            case TrainingScenario.RunTowardsBallAndKick:
                return KickerStep((RoboCupKickerAgent)team1Agents[0]);

            case TrainingScenario.AttackDefend:
                DefenderStep((RoboCupDefenderAgent)team1Agents[0]);
                break;

        }

        return true;
    }

    bool KickerStep(RoboCupKickerAgent agent)
    {
        RcObject kickerCoachBall = coach.GetRcObject("b");
        if (kickerCoachBall != null)
        {
            if (kickerCoachBall.delta.x > 0.1f)
            {
                Vector2 ballPos = new Vector2(kickerCoachBall.position.x, kickerCoachBall.position.y);
                RcObject kickerCoachGoal = coach.GetRcObject("g r");
                Vector2 goalPos = new Vector2(kickerCoachGoal.position.x, kickerCoachGoal.position.y);

                agent.OnBallMovedRight((ballPos - goalPos).magnitude);
            }
            
            if (kickerCoachBall.delta.x < -0.1f)
            {
                Vector2 ballPos = new Vector2(kickerCoachBall.position.x, kickerCoachBall.position.y);
                RcObject kickerCoachGoal = coach.GetRcObject("g l");
                Vector2 goalPos = new Vector2(kickerCoachGoal.position.x, kickerCoachGoal.position.y);

                agent.OnBallMovedLeft((ballPos - goalPos).magnitude);
                
                if (ballPos.x < 0)
                    agent.OnBallEnteredLeftSide();
            }
            
            // Ball enters goal
            if (kickerCoachBall.position.y < 7 && kickerCoachBall.position.y > -7 && kickerCoachBall.position.x > 52)
            {
                if (!RoboCup.singleton.seriousMode)
                    win.Play(source);
                
                agent.OnScored();
                agent.EndEpisode();
                OnEpisodeBegin();
                return false;
            }

            if (kickerCoachBall.position.y < -32 || kickerCoachBall.position.y > 32 ||
                kickerCoachBall.position.x > 52 || kickerCoachBall.position.x < -52)
            {
                if (!RoboCup.singleton.seriousMode)
                    fail.Play(source);
                
                agent.OnBallOutOfField();
                agent.EndEpisode();
                OnEpisodeBegin();
                return false;
            }

        }
        
        RcObject kickerCoachPlayer = coach.GetRcObject($"p \"{RoboCup.singleton.GetTeamName()}\" 1");
        if (kickerCoachPlayer != null)
        {
            if (kickerCoachPlayer.position.y < -32 || kickerCoachPlayer.position.y > 32 ||
                kickerCoachPlayer.position.x > 52 || kickerCoachPlayer.position.x < -52)
            {
                agent.OnOutOfField();
                agent.EndEpisode();
                OnEpisodeBegin();
                return false;
            }
        }

        return true;
    }

    void DefenderStep(RoboCupDefenderAgent agent)
    {
        // Defender Observations
        RcPerceivedObject defenderOpponent = agent.GetPlayer().GetRcObject("p \"Attacker\" 1");
        if (defenderOpponent != null)
            agent.SetOpponentInfo(defenderOpponent.curVisibility, (int)defenderOpponent.direction, (int)defenderOpponent.distance);
        
        // Rewards
        if (agent.GetPlayer().GetCalculatedPosition().y > 20 ||
            agent.GetPlayer().GetCalculatedPosition().y < -20 ||
            agent.GetPlayer().GetCalculatedPosition().x > -35 ||
            agent.GetPlayer().GetCalculatedPosition().x < -52)
            agent.OnLeftGoalArea();
    }

    bool AttackerStep(RoboCupAttackerAgent agent)
    {
        // Attacker Observations
        RcPerceivedObject attackerOpponent = agent.GetPlayer().GetRcObject("p \"Defender\" 1 goalie");
        if (attackerOpponent != null)
            agent.SetOpponentInfo(attackerOpponent.curVisibility, attackerOpponent.direction, attackerOpponent.distance);
        
        RcObject attackerCoachBall = coach.GetRcObject("b");
        if (attackerCoachBall != null)
        {
            if (attackerCoachBall.delta.x > 0.1f)
            {
                Vector2 ballPos = new Vector2(attackerCoachBall.position.x, attackerCoachBall.position.y);
                RcObject attackerCoachGoal = coach.GetRcObject("g l");
                Vector2 goalPos = new Vector2(attackerCoachGoal.position.x, attackerCoachGoal.position.y);

                agent.OnBallMovedRight((ballPos - goalPos).magnitude);
            }
            
            if (attackerCoachBall.delta.x < -0.1f)
            {
                Vector2 ballPos = new Vector2(attackerCoachBall.position.x, attackerCoachBall.position.y);
                RcObject attackerCoachGoal = coach.GetRcObject("g r");
                Vector2 goalPos = new Vector2(attackerCoachGoal.position.x, attackerCoachGoal.position.y);

                agent.OnBallMovedLeft((ballPos - goalPos).magnitude);
                
                if (ballPos.x < 0)
                    agent.OnBallEnteredLeftSide();
            }
            
            // Ball enters goal
            if (attackerCoachBall.position.y < 7 && attackerCoachBall.position.y > -7 && attackerCoachBall.position.x > 52)
            {
                if (!RoboCup.singleton.seriousMode)
                    win.Play(source);
                
                agent.OnScored();
                agent.EndEpisode();
                OnEpisodeBegin();
                return false;
            }

            if (attackerCoachBall.position.y < -32 || attackerCoachBall.position.y > 32 ||
                attackerCoachBall.position.x > 52 ||attackerCoachBall.position.x < -52)
            {
                if (!RoboCup.singleton.seriousMode)
                    fail.Play(source);
                
                agent.OnBallOutOfField();
                agent.EndEpisode();
                OnEpisodeBegin();
                return false;
            }

        }
        
        // Rewards
        agent.OnTimePassed();
        
        if (agent.GetPlayer().GetCalculatedPosition().y < 20 &&
            agent.GetPlayer().GetCalculatedPosition().y > -20 &&
            agent.GetPlayer().GetCalculatedPosition().x < -35 &&
            agent.GetPlayer().GetCalculatedPosition().x > -52)
            agent.OnEnteredGoalArea();

        if (agent.GetPlayer().GetCalculatedAngle() > 0)
            agent.OnLookRight();

        return true;
    }

    public List<PlayerSetupInfo> GetTeam1Setup()
    {
        return team1Setup;
    }
    
    public List<PlayerSetupInfo> GetTeam2Setup()
    {
        return team2Setup;
    }

    public void SetEpisodeLength(int steps)
    {
        stepsPerEpisode = steps;
        stepsLeftInCurrentEpisode = stepsPerEpisode;
    }
}