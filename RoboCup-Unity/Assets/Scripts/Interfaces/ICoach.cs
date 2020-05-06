public interface ICoach
{
    void InitTraining(RoboCupAgent agent, IPlayer player);
    void MoveBall(int x, int y);
    void MovePlayer(string teamName, int unum, int x, int y);
    void Recover();
    void KickOff();
}