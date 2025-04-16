using UnityEngine;

public class PuropenController : MonoBehaviour
{
    EnemyController enemyController;

    private void Start()
    {
        enemyController = GetComponent<EnemyController>();
    }

    private void Update()
    {
        ChangeAnimation();
        Flip();
    }

    private void ChangeAnimation()
    {
        enemyController.animator.SetBool("isWalkingUp", enemyController.direction == Vector2.up);
        enemyController.animator.SetBool("isWalkingDown", enemyController.direction == Vector2.down);
        enemyController.animator.SetBool("isWalkingSide", enemyController.direction == Vector2.left || enemyController.direction == Vector2.right);
    }

    private void Flip()
    {
        enemyController.SpriteRenderer.flipX = enemyController.direction == Vector2.right;
    }

}