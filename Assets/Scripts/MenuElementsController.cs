using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuElementsController : MonoBehaviour
{
    Rigidbody2D rigidBody;
    public enum ElementType { Arrow, Title, Vehicle };
    public ElementType elementType;
    public float speed;

    [Header("Title")]
    public float xTarget; // Title Top: 7.09f, Title Bottom: 7.06f
    Vector2 target;
    float moveTitleAt;

    [Header("Vehicle")]
    public Direction direction;
    public enum Direction { Left, Right };

    [Header("Arrow")]
    public Transform[] arrowPositions;
    int currentIndex = 0;

    void Start()
    {
        SetInitialVariables();
    }

    void Update()
    {
        MoveHorizontally();
        SwitchPosition();
    }

    private void SetInitialVariables()
    {
        switch (elementType)
        {
            case ElementType.Arrow:
                transform.position = arrowPositions[currentIndex].position;
                break;

            case ElementType.Title:
                moveTitleAt = Time.time + 5;
                target = new(xTarget, transform.position.y);
                break;

            case ElementType.Vehicle:
                rigidBody = GetComponent<Rigidbody2D>();
                break;
        }
    }

    private void MoveHorizontally()
    {
        if (elementType == ElementType.Vehicle)
        {
            if (direction == Direction.Left) rigidBody.velocity = Vector2.left * speed;
            else rigidBody.velocity = Vector2.right * speed;
        }
        else if (elementType == ElementType.Title && transform.position.x != xTarget && Time.time >= moveTitleAt)
        {
            transform.position = Vector2.MoveTowards(transform.position, target, speed * Time.deltaTime);
        }
    }

    private void SwitchPosition()
    {
        if (elementType == ElementType.Arrow && Input.anyKeyDown)
        {
            if (currentIndex == 0 && Input.GetKeyDown(KeyCode.Z)) SceneManager.LoadScene("Stage 1-1");
            else
            {
                if (Input.GetKeyDown(KeyCode.S)) currentIndex = (currentIndex + 1) % arrowPositions.Length;
                else if (Input.GetKeyDown(KeyCode.W)) currentIndex = (currentIndex - 1 + arrowPositions.Length) % arrowPositions.Length;

                transform.position = arrowPositions[currentIndex].position;
            }
        }
    }

}
