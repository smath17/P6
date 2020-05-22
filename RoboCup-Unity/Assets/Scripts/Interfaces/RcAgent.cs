﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface RcAgent
{
    void SetBallInfo(bool ballCurVisibility, float ballDirection, float ballDistance);
    void SetGoalInfo(bool kickerGoalLeftVisible, float kickerGoalLeftDir, bool kickerGoalRightVisible, float kickerGoalRightDir);
    void SetSelfInfo(int getKickBallCount);
    void SetPlayer(RcPlayer player);
    void SetRealMatch();
    void RequestDecision();
}
