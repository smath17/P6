using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface RcAgent
{
    void SetPlayer(RcPlayer player);
    void SetRealMatch();
    void RequestDecision();
    void SetBallInfo(bool visible, float direction, float distance);
    void SetGoalInfo(bool leftPoleVisible, float leftPoleDirection, bool rightPoleVisible, float rightPoleDirection);
    void SetSelfInfo(int kickedBallCount);
    void SetOwnGoalInfo(bool visible, float direction);
    void SetLeftSideInfo(bool visible, float direction);
    void SetRightSideInfo(bool visible, float direction);
}
