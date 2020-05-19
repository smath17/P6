using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

public class AgentTrainer : MonoBehaviour
{
    public enum TrainingScenario
    {
        RunTowardsBall,
        RunTowardsBallAndKick,
        AttackDefendAttackOnly,
        AttackDefend
    }
    
    [Header("Settings")]
    public TrainingScenario trainingScenario;
    
    [Header("Agents")]
    public RoboCupAgent defaultAgent;
    public RoboCupDefenderAgent defenderAgent;
    public RoboCupAttackerAgent attackerAgent;
    public RoboCupKickerAgent kickerAgent;

    [Header("Other")]
    public GameObject offlineVisualPlayer;

    List<PlayerSetupInfo> team1Setup = new List<PlayerSetupInfo>();
    List<PlayerSetupInfo> team2Setup = new List<PlayerSetupInfo>();

    bool initialized;
    
    int stepsPerEpisode = 50;
    int stepsLeftInCurrentEpisode;

    RcCoach coach;

    // AttackVsDefend
    RcPlayer defender;
    RcPlayer attacker;
    RcPlayer kicker;
    
    bool prevBallInGoalArea;
    bool curBallInGoalArea;

    public void SetupTeams()
    {
        switch (trainingScenario)
        {
            
            case TrainingScenario.RunTowardsBall:
                RoboCup.singleton.SetTeamName("RunTowardsBall");
                team1Setup.Add(new PlayerSetupInfo(false, -20, 0));
                break;
            
            case TrainingScenario.RunTowardsBallAndKick:
                RoboCup.singleton.SetTeamName("RunAndKick");
                team1Setup.Add(new PlayerSetupInfo(false, -20, 0));
                break;
            
            case TrainingScenario.AttackDefend:
            case TrainingScenario.AttackDefendAttackOnly:
                RoboCup.singleton.SetTeamName("Defender");
                team1Setup.Add(new PlayerSetupInfo(true, -50, 0));
                
                RoboCup.singleton.SetEnemyTeamName("Attacker");
                team2Setup.Add(new PlayerSetupInfo(false, -50, 0));
                break;
        }
    }

    public void Init()
    {
        coach = RoboCup.singleton.GetCoach();
        
        switch (trainingScenario)
        {
            case TrainingScenario.RunTowardsBall:

                defaultAgent.SetTrainingScenario(trainingScenario);
        
                defaultAgent.SetPlayer(RoboCup.singleton.GetPlayer(0));
                defaultAgent.SetCoach(coach);
                defaultAgent.gameObject.SetActive(true);
                
                break;
            
            case TrainingScenario.RunTowardsBallAndKick:
                
                kicker = RoboCup.singleton.GetPlayer(0, true);
                
                kickerAgent.SetAgentTrainer(this);
                kickerAgent.SetPlayer(kicker);
                kickerAgent.SetCoach(coach);
                kickerAgent.gameObject.SetActive(true);
                
                break;
            
            case TrainingScenario.AttackDefend:
            case TrainingScenario.AttackDefendAttackOnly:
                
                defender = RoboCup.singleton.GetPlayer(0, true);
                if (trainingScenario != TrainingScenario.AttackDefendAttackOnly)
                {
                    defenderAgent.SetPlayer(defender);
                    defenderAgent.SetCoach(coach);
                    defenderAgent.gameObject.SetActive(true);
                }
                
                attacker = RoboCup.singleton.GetPlayer(0, false);
                attackerAgent.SetPlayer(attacker);
                attackerAgent.SetCoach(coach);
                attackerAgent.gameObject.SetActive(true);
                
                break;
        }

        coach.InitTraining(this);
        
        initialized = true;
        
        OnEpisodeBegin();
    }

    public void OnEpisodeBegin()
    {
        stepsLeftInCurrentEpisode = stepsPerEpisode;
        
        switch (trainingScenario)
        {
            case TrainingScenario.RunTowardsBall:
                defaultAgent.OnEpisodeBegin();
                break;
            
            case TrainingScenario.RunTowardsBallAndKick:
                kickerAgent.OnEpisodeBegin();
                break;
            
            case TrainingScenario.AttackDefend:
            case TrainingScenario.AttackDefendAttackOnly:
                int goalieY = Random.Range(-10, 10);
                
                coach.MovePlayer(RoboCup.singleton.GetTeamName(), 1, -50, goalieY, 0);
                coach.MovePlayer(RoboCup.singleton.GetEnemyTeamName(), 1, -20, 0, 180);
                
                coach.MoveBall(-25, 0);
                coach.Recover();
                break;
        }
    }

    public void Step()
    {
        if (!initialized)
            return;
        
        stepsLeftInCurrentEpisode--;
        if (stepsLeftInCurrentEpisode < 1)
        {
            switch (trainingScenario)
            {
                case TrainingScenario.RunTowardsBall:
                    defaultAgent.EndEpisode();
                    break;
                
                case TrainingScenario.RunTowardsBallAndKick:
                    kickerAgent.EndEpisode();
                    break;
            
                case TrainingScenario.AttackDefend:
                case TrainingScenario.AttackDefendAttackOnly:
                    attackerAgent.EndEpisode();
                    if (trainingScenario != TrainingScenario.AttackDefendAttackOnly)
                        defenderAgent.EndEpisode();
                    break;
            }
            OnEpisodeBegin();
            return;
        }

        switch (trainingScenario)
        {
            case TrainingScenario.RunTowardsBall:

                RcPerceivedObject ball = RoboCup.singleton.GetPlayer(0, true).GetRcObject("b");
                
                if (ball.curVisibility)
                    defaultAgent.SetBallInfo(true, ball.direction, ball.distance);
                else
                    defaultAgent.SetBallInfo(false);

                defaultAgent.RequestDecision();
                
                break;
            
            case TrainingScenario.RunTowardsBallAndKick:
                
                RcPerceivedObject kickerBall = kicker.GetRcObject("b");
                if (kickerBall.curVisibility)
                    kickerAgent.SetBallInfo(true, kickerBall.direction, kickerBall.distance);
                else
                    kickerAgent.SetBallInfo(false);
                
                RcPerceivedObject kickerGoal = kicker.GetRcObject("g r");
                if (kickerGoal != null)
                    kickerAgent.SetGoalInfo(kickerGoal.curVisibility, kickerGoal.direction, kickerGoal.distance);
                
                kickerAgent.SetSelfInfo(kicker.GetKickBallCount());
                kickerAgent.RequestDecision();
                
                RcObject kickerCoachBall = coach.GetRcObject("b");
                if (kickerCoachBall != null)
                {
                    if (kickerCoachBall.delta.x > 0.1f)
                    {
                        Vector2 ballPos = new Vector2(kickerCoachBall.position.x, kickerCoachBall.position.y);
                        RcObject kickerCoachGoal = coach.GetRcObject("g r");
                        Vector2 goalPos = new Vector2(kickerCoachGoal.position.x, kickerCoachGoal.position.y);

                        kickerAgent.OnBallMoved((ballPos - goalPos).magnitude);
                    }
                    
                    // Ball enters goal
                    if (kickerCoachBall.position.y < 10 && kickerCoachBall.position.y > -10 && kickerCoachBall.position.x > 52)
                    {
                        kickerAgent.OnScored();
                        OnEpisodeBegin();
                        return;
                    }

                    if (kickerCoachBall.position.y < -32 || kickerCoachBall.position.y > 32 ||
                        kickerCoachBall.position.x > 52 || kickerCoachBall.position.x < -52)
                    {
                        kickerAgent.OnBallOutOfField();
                        OnEpisodeBegin();
                        return;
                    }

                }
                
                RcObject kickerCoachPlayer = coach.GetRcObject("p \"RunAndKick\" 1");
                if (kickerCoachPlayer != null)
                {
                    if (kickerCoachPlayer.position.y < -32 || kickerCoachPlayer.position.y > 32 ||
                        kickerCoachPlayer.position.x > 52 || kickerCoachPlayer.position.x < -52)
                    {
                        kickerAgent.OnOutOfField();
                        OnEpisodeBegin();
                        return;
                    }
                }
                
                break;
            
            case TrainingScenario.AttackDefend:
            case TrainingScenario.AttackDefendAttackOnly:


                if (trainingScenario != TrainingScenario.AttackDefendAttackOnly)
                {
                    // Defender Observations
                    defenderAgent.SetSelfInfo(defender.GetCalculatedPosition().x, defender.GetCalculatedPosition().y, defender.GetCalculatedAngle());
                    
                    RcPerceivedObject defenderBall = defender.GetRcObject("b");
                    if (defenderBall != null)
                        defenderAgent.SetBallInfo(defenderBall.curVisibility, (int)defenderBall.direction, (int)defenderBall.distance);
                    
                    RcPerceivedObject defenderOpponent = defender.GetRcObject("p \"Attacker\" 1");
                    if (defenderOpponent != null)
                        defenderAgent.SetAttackerInfo(defenderOpponent.curVisibility, (int)defenderOpponent.direction, (int)defenderOpponent.distance);
                }
                
                // Attacker Observations
                attackerAgent.SetSelfInfo(attacker.GetCalculatedPosition().x, attacker.GetCalculatedPosition().y, attacker.GetCalculatedAngle(), attacker.GetKickBallCount());
                
                RcPerceivedObject attackerBall = attacker.GetRcObject("b");
                if (attackerBall != null)
                {
                    attackerAgent.SetBallInfo(attackerBall.curVisibility, (int)attackerBall.direction, (int)attackerBall.distance);
                    if (!attackerBall.curVisibility)
                        attackerAgent.OnBallNotVisible();
                    else if (attackerBall.distance < 0.7f)
                        attackerAgent.OnBallWithinRange();
                }
                
                RcPerceivedObject attackerGoal = attacker.GetRcObject("g l");
                if (attackerGoal != null)
                    attackerAgent.SetGoalInfo(attackerGoal.curVisibility, (int)attackerGoal.direction, (int)attackerGoal.distance);
                
                RcPerceivedObject attackerOpponent = attacker.GetRcObject("p \"Defender\" 1 goalie");
                if (attackerOpponent != null)
                    attackerAgent.SetDefenderInfo(attackerOpponent.curVisibility, (int)attackerOpponent.direction, (int)attackerOpponent.distance);
                
                // Rewards
                if (trainingScenario != TrainingScenario.AttackDefendAttackOnly)
                {
                    if (defender.GetCalculatedPosition().y > 20 ||
                        defender.GetCalculatedPosition().y < -20 ||
                        defender.GetCalculatedPosition().x > -35 ||
                        defender.GetCalculatedPosition().x < -52)
                        defenderAgent.OnLeftGoalArea();
                }
                
                attackerAgent.OnTimePassed();
                
                if (attacker.GetCalculatedPosition().y < 20 &&
                    attacker.GetCalculatedPosition().y > -20 &&
                    attacker.GetCalculatedPosition().x < -35 &&
                    attacker.GetCalculatedPosition().x > -52)
                    attackerAgent.OnEnteredGoalArea();

                if (attacker.GetCalculatedAngle() > 0)
                    attackerAgent.OnLookRight();
                
                
                RcObject coachBall = coach.GetRcObject("b");
                if (coachBall != null)
                {
                    if (coachBall.delta.x < 0)
                        attackerAgent.OnBallMovedLeft();
                    
                    prevBallInGoalArea = curBallInGoalArea;
                    curBallInGoalArea = coachBall.position.y < 10 && coachBall.position.y > -10 && coachBall.position.x < -46 && coachBall.position.x > -52;
            
                    // Ball enters goal
                    if (coachBall.position.y < 10 && coachBall.position.y > -10 && coachBall.position.x < -52)
                    {
                        attackerAgent.OnScored();
                        if (trainingScenario != TrainingScenario.AttackDefendAttackOnly)
                            defenderAgent.OnFailedToDefend();
                        
                        coach.Goal(false);
                        OnEpisodeBegin();
                        return;
                    }

                    // Ball exits goal area
                    if (prevBallInGoalArea && !curBallInGoalArea)
                    {
                        attackerAgent.OnFailedToScore();
                        if (trainingScenario != TrainingScenario.AttackDefendAttackOnly)
                            defenderAgent.OnDefended();
                        
                        OnEpisodeBegin();
                        return;
                    }
                    
                }

                // Request Decisions
                attackerAgent.RequestDecision();
                if (trainingScenario != TrainingScenario.AttackDefendAttackOnly)
                    defenderAgent.RequestDecision();
                
                break;
        }
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
    }
}