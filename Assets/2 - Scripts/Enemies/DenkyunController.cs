using UnityEngine;

public class DenkyunController : MonoBehaviour
{
    EnemyController enemyController;
    Vector2 currentDirection;

    void Start()
    {
        enemyController = GetComponent<EnemyController>();

        currentDirection = enemyController.Direction;
    }

    void Update()
    {
        ResetAnimation();
    }

    private void ResetAnimation()
    {
        if (currentDirection != enemyController.Direction)
        {
            currentDirection = enemyController.Direction;
            enemyController.Animator.SetTrigger("move");
        }
    }

}
