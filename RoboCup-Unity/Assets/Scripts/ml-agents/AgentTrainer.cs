using System;
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

    public void Init()
    {
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
    }

    public void Step()
    {
        if (RoboCup.singleton.GetRcObject("b").isVisible)
            defaultAgent.SetBallInfo(true, RoboCup.singleton.GetRcObject("b").direction, RoboCup.singleton.GetRcObject("b").distance);
        else
            defaultAgent.SetBallInfo(false);
                
        defaultAgent.RequestDecision();
        defaultAgent.Step();
    }
}