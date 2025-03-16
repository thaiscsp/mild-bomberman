using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PuropenController : MonoBehaviour
{
    Animator animator;
    GameManager gameManager;
    PlayerOneController playerOneController;
    Rigidbody2D rigidBody;
    SpriteRenderer spriteRenderer;

    Color originalColor;
    Vector2 direction = Vector2.down;

    bool wasHit;

    public bool Invincible { private get; set; }
    public bool WasSpawnedFromExit { private get; set; }

    public float raySize;
    public float speed = 1.5f;
    public int points;

    private void Start()
    {
        animator = GetComponent<Animator>();
        gameManager = FindFirstObjectByType<GameManager>();
        playerOneController = FindFirstObjectByType<PlayerOneController>();
        rigidBody = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        
        originalColor = spriteRenderer.color;
    }

    private void Update()
    {
        Move();
        SetAnimationParameters();
        Flip();
        ChangeDirection();
    }

    private void Move()
    {
        if (!wasHit)
        {
            rigidBody.velocity = direction * speed;
        }
    }

    private void SetAnimationParameters()
    {
        animator.SetBool("isWalkingUp", direction == Vector2.up);
        animator.SetBool("isWalkingDown", direction == Vector2.down);
        animator.SetBool("isWalkingSide", direction == Vector2.left || direction == Vector2.right);
    }

    private void Flip()
    {
        spriteRenderer.flipX = direction == Vector2.right;
    }

    private bool CanChangeDirection(RaycastHit2D[] enemyHit, RaycastHit2D generalHit)
    {
        bool canChangeDirection = false;

        // In RaycastAll() for the enemy, the first collider will always be the game object's own collider (enemy detects itself).
        // If the array has more than one element, the second one is sure to be an enemy other than the game object itself.
        if (enemyHit.Length > 1)
        {
            canChangeDirection = true;
        } else if (generalHit.collider!= null)
        {
            canChangeDirection = true;
        }

        return canChangeDirection;
    }

    private void ChangeDirection()
    {
        Vector2[] directions = { Vector2.up, Vector2.down, Vector2.left, Vector2.right };
        Vector3 offset = new(0, 0.25f, 0); // Because the Puropen's middle (in the transform) is too high
        int layers = LayerMask.GetMask("Bomb", "Building", "RedLight");

        // Detects if there is something in front of the Puropen
        // RaycastHit2D[] enemyHit = Physics2D.RaycastAll(transform.position - offset, direction, 1, LayerMask.GetMask("Enemy"));
        RaycastHit2D[] enemyHit = Physics2D.BoxCastAll(transform.position - offset, Vector2.one *  0.75f, 0, direction, 1, LayerMask.GetMask("Enemy"));
        RaycastHit2D generalHit = Physics2D.Raycast(transform.position - offset, direction, raySize, layers);

        if (CanChangeDirection(enemyHit, generalHit)) // If there is...
        {
            bool[] availableDirections = new bool[directions.Length];

            for (int i = 0; i < directions.Length; i++)
            {
                // A Box Cast will ensure the Puropen moves along the grid
                generalHit = Physics2D.BoxCast(transform.position - offset, Vector2.one * 0.85f, 0, directions[i], 1f, layers);
                availableDirections[i] = generalHit.collider == null; // A direction will be available if the boxcast didn't hit anything
            }

            if (availableDirections.Count(x => x == false) == directions.Length) // Stop if there are no available directions
            {
                return;
            }
            else // Finds a new direction for the Puropen to move to otherwise
            {
                List<int> availableDirectionIndexes = new List<int>();

                for (int i = 0; i < availableDirections.Length; i++)
                {
                    if (availableDirections[i])
                        availableDirectionIndexes.Add(i);
                }

                int randIndex = Random.Range(0, availableDirectionIndexes.Count);
                int newDirectionIndex = availableDirectionIndexes[randIndex];
                direction = directions[newDirectionIndex];
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Explosion") && !Invincible)
        {
            // Stops movement
            wasHit = true;
            rigidBody.velocity = Vector2.zero;

            StartCoroutine(FlashOnDamage());
            StartCoroutine(DestroyPuropen());
        }
    }

    private IEnumerator FlashOnDamage()
    {
        float flashTime = 0.05f;
        int flashCount = 10;

        for (int i = 0; i < flashCount; i++)
        {
            spriteRenderer.color = Color.blue;
            yield return new WaitForSeconds(flashTime);

            spriteRenderer.color = originalColor;
            yield return new WaitForSeconds(flashTime);
        }
    }

    private IEnumerator DestroyPuropen()
    {
        yield return new WaitForSeconds(1);

        gameManager.EnemiesRemaining--;
        playerOneController.Score += points;

        if (WasSpawnedFromExit)
        {
            gameManager.EnemySpawnedFromExit = false;
        }

        Destroy(gameObject);
    }

}