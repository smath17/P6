using System;
using System.Collections.Generic;
using UnityEngine;

public class AgentTrainer : MonoBehaviour
{
    public enum TrainingScenario
    {
        LookAtBall,
        RunTowardsBall,
        AttackVsDefend
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
            
            case TrainingScenario.AttackVsDefend:
                RoboCup.singleton.SetTeamName("Defender");
                team1Setup.Add(new PlayerSetupInfo(true, -50, 0));
                
                RoboCup.singleton.SetEnemyTeamName("Attacker");
                team2Setup.Add(new PlayerSetupInfo(false, -50, 0));
                break;
        }
    }

    public void Init()
    {
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
                    defaultAgent.SetCoach(RoboCup.singleton.GetCoach());
                    defaultAgent.gameObject.SetActive(true);
                }
                
                break;
            
            case TrainingScenario.AttackVsDefend:
                
                defenderAgent.SetPlayer(RoboCup.singleton.GetPlayer(0, true));
                defenderAgent.SetCoach(RoboCup.singleton.GetCoach());
                defenderAgent.gameObject.SetActive(true);
                
                attackerAgent.SetPlayer(RoboCup.singleton.GetPlayer(0, false));
                attackerAgent.SetCoach(RoboCup.singleton.GetCoach());
                attackerAgent.gameObject.SetActive(true);
                
                RoboCup.singleton.GetCoach().KickOff();
                
                break;
        }
    }

    public void Step()
    {
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
            
            case TrainingScenario.AttackVsDefend:
                RcPlayer defender = RoboCup.singleton.GetPlayer(0, true);
                
                defenderAgent.SetSelfInfo(defender.GetCalculatedPosition().x, defender.GetCalculatedPosition().y, defender.GetCalculatedAngle());
                
                RcObject defenderBall = defender.GetRcObject("b");
                if (defenderBall != null)
                    defenderAgent.SetBallInfo(defenderBall.curVisibility, (int)defenderBall.direction, (int)defenderBall.distance);
                
                RcObject defenderOpponent = defender.GetRcObject("p \"Attacker\" 1");
                if (defenderOpponent != null)
                    defenderAgent.SetAttackerInfo(defenderOpponent.curVisibility, (int)defenderOpponent.direction, (int)defenderOpponent.distance);
                
                defenderAgent.RequestDecision();
                
                
                RcPlayer attacker = RoboCup.singleton.GetPlayer(0, false);
                
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
                
                attackerAgent.RequestDecision();
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