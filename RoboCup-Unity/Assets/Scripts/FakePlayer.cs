using UnityEngine;

public class FakePlayer : MonoBehaviour, IPlayer
{
    float nextRotation;
    
    public void Move(int x, int y)
    {
        transform.position = new Vector3(x, 0, y);
    }

    public void Dash(int amount, int direction)
    {
        throw new System.NotImplementedException();
    }

    public void Turn(int amount)
    {
        nextRotation = amount;
    }

    public void Kick(int power)
    {
        throw new System.NotImplementedException();
    }

    public void Catch()
    {
        throw new System.NotImplementedException();
    }

    public void UpdatePlayer()
    {
        transform.Rotate(Vector3.up, nextRotation);
        nextRotation = 0;
    }
}