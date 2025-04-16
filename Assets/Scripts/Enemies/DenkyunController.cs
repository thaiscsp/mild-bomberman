using UnityEngine;

public class DenkyunController : MonoBehaviour
{
    EnemyController enemyController;
    Vector2 currentDirection;

    void Start()
    {
        enemyController = GetComponent<EnemyController>();

        currentDirection = enemyController.direction;
    }

    void Update()
    {
        ResetAnimation();
    }

    private void ResetAnimation()
    {
        if (currentDirection != enemyController.direction)
        {
            currentDirection = enemyController.direction;
            enemyController.animator.SetTrigger("move");
        }
    }

}
