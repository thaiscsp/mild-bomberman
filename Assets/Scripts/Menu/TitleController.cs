using UnityEngine;

public class TitleController : MonoBehaviour
{
    Vector2 target;
    float moveTitleAt;
    public float speed, xTarget; // Title Top: 7.09f, Title Bottom: 7.06f
    

    void Start()
    {
        moveTitleAt = Time.time + 5;
        target = new(xTarget, transform.position.y);
    }

    void Update()
    {
        Move();
    }

    private void Move()
    {
        if (transform.position.x != xTarget && Time.time >= moveTitleAt)
        {
            transform.position = Vector2.MoveTowards(transform.position, target, speed * Time.deltaTime);
        }
    }

}
