public interface ICoach
{
    void InitTraining(RoboCupAgent agent, IPlayer player);
    void MoveBall(int x, int y);
    void MovePlayer(int x, int y);
}