public interface IPlayer
{
    void Move(int x, int y);
    void Dash(int amount, int direction);
    void Turn(int amount);
    void Kick(int power);
}