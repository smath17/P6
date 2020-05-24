public interface RcAgent
{
    void SetAgentTrainer(AgentTrainer agentTrainer);
    void SetPlayer(RcPlayer player);
    void Init(bool realMatch);
    void RequestDecision();
    void SetBallInfo(bool visible, float direction, float distance);
    void SetGoalInfo(bool leftPoleVisible, float leftPoleDirection, bool rightPoleVisible, float rightPoleDirection);
    void SetSelfInfo(int kickedBallCount);
    void SetOpponentInfo(bool visible, float direction, float distance);
    void SetOwnGoalInfo(bool visible, float direction);
    void SetLeftSideInfo(bool visible, float direction);
    void SetRightSideInfo(bool visible, float direction);
    void EndEpisode();
    RcPlayer GetPlayer();
}
