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

    void Awake()
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
                team1Setup.Add(new PlayerSetupInfo(true, -30, 0));
                
                RoboCup.singleton.SetEnemyTeamName("Attacker");
                team2Setup.Add(new PlayerSetupInfo(false, 10, 0));
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
                
                break;
        }
    }

    public void Step()
    {
        switch (trainingScenario)
        {
            case TrainingScenario.LookAtBall:
            case TrainingScenario.RunTowardsBall:
                
                if (RoboCup.singleton.GetRcObject("b").isVisible)
                    defaultAgent.SetBallInfo(true, RoboCup.singleton.GetRcObject("b").direction, RoboCup.singleton.GetRcObject("b").distance);
                else
                    defaultAgent.SetBallInfo(false);
                
                defaultAgent.RequestDecision();
                defaultAgent.Step();
                
                break;
            
            case TrainingScenario.AttackVsDefend:
                //defenderAgent.SetSelfInfo();
                //defenderAgent.SetBallInfo();
                //defenderAgent.SetAttackerInfo();
                defenderAgent.RequestDecision();
                
                //attackerAgent.SetSelfInfo();
                //attackerAgent.SetBallInfo();
                //attackerAgent.SetGoalInfo();
                //attackerAgent.SetDefenderInfo();
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