using System;
using UnityEngine;

public class FakeCoach : MonoBehaviour, ICoach
{
    RoboCupAgent agent;
    FakePlayer fakePlayer;

    Transform player;
    Transform ball;
    
    void Awake()
    {
        GameObject ballObj = Instantiate(Resources.Load<GameObject>("prefabs/visual3D/Ball"));
        ball = ballObj.transform;
    }

    public void InitTraining(RoboCupAgent agent, IPlayer player)
    {
        this.agent = agent;
        fakePlayer = (FakePlayer) player;

        this.player = fakePlayer.transform;
    }

    public void MoveBall(int x, int y)
    {
        ball.position = new Vector3(x, 0, y);
    }

    public void MovePlayer(string teamName, int unum, int x, int y)
    {
        fakePlayer.Move(x, y);
    }

    public void Recover()
    {
        //recover what? xD
    }

    public void KickOff()
    {
        //yeet
    }

    public void FixedUpdate()
    {
        fakePlayer.UpdatePlayer();

        int ballDistance = (int) (Vector3.Distance(player.position, ball.position));
        int ballDirection = (int)(Vector3.SignedAngle(player.forward, ball.position - player.position, Vector3.up));
        bool ballVisible = ballDirection < 45 && ballDirection > -45;
        
        agent.SetBallInfo(ballVisible, ballDirection, ballDistance);
        agent.Step();
    }
}