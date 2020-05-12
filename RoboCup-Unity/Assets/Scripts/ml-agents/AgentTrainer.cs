using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class AgentTrainer : MonoBehaviour
{
    public enum TrainingScenario
    {
        LookAtBall,
        RunTowardsBall,
        AttackDefendAttackOnly,
        AttackDefend
    }
    
    [Header("Settings")]
    public TrainingScenario trainingScenario;
    bool offlineTraining = false;
    
    [Header("Agents")]
    public RoboCupAgent defaultAgent;
    public RoboCupDefenderAgent defenderAgent;
    public RoboCupAttackerAgent attackerAgent;
    
    [Header("Other")]
    public GameObject offlineVisualPlayer;

    List<PlayerSetupInfo> team1Setup = new List<PlayerSetupInfo>();
    List<PlayerSetupInfo> team2Setup = new List<PlayerSetupInfo>();

    bool initialized;
    
    int stepsPerEpisode = 100;
    int stepsLeftInCurrentEpisode;

    RcCoach coach;

    // AttackVsDefend
    RcPlayer defender;
    RcPlayer attacker;
    
    bool prevBallInGoalArea;
    bool curBallInGoalArea;

    public void SetupTeams()
    {
        switch (trainingScenario)
        {
            case TrainingScenario.LookAtBall:
                RoboCup.singleton.SetTeamName("LookAtBall");
                team1Setup.Add(new PlayerSetupInfo(false, -20, 0));
                break;
            
            case TrainingScenario.RunTowardsBall:
                RoboCup.singleton.SetTeamName("RunTowardsBall");
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
            case TrainingScenario.LookAtBall:
            case TrainingScenario.RunTowardsBall:
        
                defaultAgent.SetOfflineTraining(offlineTraining);
                defaultAgent.SetTrainingScenario(trainingScenario);
        
                if (offlineTraining)
                {
                    GameObject fakeCoach = Instantiate(Resources.Load<GameObject>("prefabs/Fake Coach"));
            
                    defaultAgent.SetCoach(fakeCoach.GetComponent<FakeCoach>());
                    defaultAgent.SetPlayer(offlineVisualPlayer.AddComponent<FakePlayer>());
            
                    defaultAgent.gameObject.SetActive(true);
                }
                else
                {
                    defaultAgent.SetPlayer(RoboCup.singleton.GetPlayer(0));
                    defaultAgent.SetCoach(coach);
                    defaultAgent.gameObject.SetActive(true);
                }
                
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
                
                coach.InitTraining(this);
                
                break;
        }

        initialized = true;
        
        OnEpisodeBegin();
    }

    public void OnEpisodeBegin()
    {
        switch (trainingScenario)
        {
            case TrainingScenario.LookAtBall:
                break;
            
            case TrainingScenario.RunTowardsBall:
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
            stepsLeftInCurrentEpisode = stepsPerEpisode;
            switch (trainingScenario)
            {
                case TrainingScenario.LookAtBall:
                    break;
            
                case TrainingScenario.RunTowardsBall:
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
            case TrainingScenario.LookAtBall:
            case TrainingScenario.RunTowardsBall:

                RcObject ball = RoboCup.singleton.GetPlayer(0, true).GetRcObject("b");
                
                if (ball.curVisibility)
                    defaultAgent.SetBallInfo(true, ball.direction, ball.distance);
                else
                    defaultAgent.SetBallInfo(false);
                
                defaultAgent.RequestDecision();
                defaultAgent.Step();
                
                break;
            
            case TrainingScenario.AttackDefend:
            case TrainingScenario.AttackDefendAttackOnly:


                if (trainingScenario != TrainingScenario.AttackDefendAttackOnly)
                {
                    // Defender Observations
                    defenderAgent.SetSelfInfo(defender.GetCalculatedPosition().x, defender.GetCalculatedPosition().y, defender.GetCalculatedAngle());
                    
                    RcObject defenderBall = defender.GetRcObject("b");
                    if (defenderBall != null)
                        defenderAgent.SetBallInfo(defenderBall.curVisibility, (int)defenderBall.direction, (int)defenderBall.distance);
                    
                    RcObject defenderOpponent = defender.GetRcObject("p \"Attacker\" 1");
                    if (defenderOpponent != null)
                        defenderAgent.SetAttackerInfo(defenderOpponent.curVisibility, (int)defenderOpponent.direction, (int)defenderOpponent.distance);
                }
                
                // Attacker Observations
                attackerAgent.SetSelfInfo(attacker.GetCalculatedPosition().x, attacker.GetCalculatedPosition().y, attacker.GetCalculatedAngle());
                
                RcObject attackerBall = attacker.GetRcObject("b");
                if (attackerBall != null)
                    attackerAgent.SetBallInfo(attackerBall.curVisibility, (int)attackerBall.direction, (int)attackerBall.distance);
                
                RcObject attackerGoal = attacker.GetRcObject("g l");
                if (attackerGoal != null)
                    attackerAgent.SetGoalInfo(attackerGoal.curVisibility, (int)attackerGoal.direction, (int)attackerGoal.distance);
                
                RcObject attackerOpponent = attacker.GetRcObject("p \"Defender\" 1 goalie");
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
                    if (coachBall.curRelativePos.x < coachBall.prevRelativePos.x)
                        attackerAgent.OnBallMovedLeft();
                    
                    prevBallInGoalArea = curBallInGoalArea;
                    curBallInGoalArea = coachBall.curRelativePos.y < 10 && coachBall.curRelativePos.y > -10 && coachBall.curRelativePos.x < -46 && coachBall.curRelativePos.x > -52;
            
                    // Ball exits goal area
                    if (prevBallInGoalArea && !curBallInGoalArea)
                    {
                        attackerAgent.OnFailedToScore();
                        if (trainingScenario != TrainingScenario.AttackDefendAttackOnly)
                            defenderAgent.OnDefended();
                        
                        OnEpisodeBegin();
                        return;
                    }

                    // Ball enters goal
                    if (coachBall.curRelativePos.y < 10 && coachBall.curRelativePos.y > -10 && coachBall.curRelativePos.x < -52)
                    {
                        attackerAgent.OnScored();
                        if (trainingScenario != TrainingScenario.AttackDefendAttackOnly)
                            defenderAgent.OnFailedToDefend();
                        
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
}