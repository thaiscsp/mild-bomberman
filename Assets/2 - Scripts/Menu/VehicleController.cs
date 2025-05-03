using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class VehicleController : MonoBehaviour
{
    Rigidbody2D rigidBody;

    public enum Direction { Left, Right };
    public Direction direction;
    public float speed;

    void Start()
    {
        rigidBody = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        Move();
    }

    private void Move()
    {
        if (direction == Direction.Left) rigidBody.velocity = Vector2.left * speed;
        else rigidBody.velocity = Vector2.right * speed;
    }

}
