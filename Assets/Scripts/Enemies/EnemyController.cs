using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EnemyController : MonoBehaviour
{
    GameManager gameManager;
    GameObject deathExplosion;
    PlayerOneController playerOneController;
    Rigidbody2D rigidBody;

    public Animator Animator { get; private set; }
    SpriteRenderer spriteRenderer;
    public Vector2 Direction { get; private set; } = Vector2.down;
    public bool Invincible { private get; set; }
    public bool WasSpawnedFromExit { private get; set; }

    public float topRaySize = 0.55f;
    public float bottomRaySize = 0.55f;
    public float leftRaySize = 0.55f;
    public float rightRaySize = 0.55f;
    public float speed = 1.5f;
    public int lives = 1;
    public int points = 100;
    public bool hasDirectionalAnimations;

    private void Start()
    {
        Animator = GetComponent<Animator>();
        gameManager = FindFirstObjectByType<GameManager>();
        playerOneController = FindFirstObjectByType<PlayerOneController>();
        rigidBody = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Update()
    {
        Move();
        ChangeDirection();
        ChangeAnimation();
        Flip();
    }

    private void OnDisable()
    {
        Invoke("DestroyEnemy", 0.33f);
    }

    private void Move()
    {
        if (lives > 0) rigidBody.velocity = Direction * speed;
        else rigidBody.velocity = Vector2.zero;
    }

    private bool CanChangeDirection(RaycastHit2D[] enemyHit, RaycastHit2D generalHit)
    {
        bool canChangeDirection = false;

        // In RaycastAll() for the enemy, the first collider will always be the game object's own collider (enemy detects itself).
        // If the array has more than one element, the second one is sure to be an enemy other than the game object itself.
        if (enemyHit.Length > 1 || generalHit.collider != null) canChangeDirection = true;

        return canChangeDirection;
    }

    private void ChangeDirection()
    {
        Vector2[] directions = { Vector2.up, Vector2.down, Vector2.left, Vector2.right };
        Vector3 colliderCenter = GetComponent<BoxCollider2D>().bounds.center;
        int layers = LayerMask.GetMask("Bomb", "Indestructible", "Destructible");

        // Detects if there is something in front of the enemy
        RaycastHit2D[] enemyHit = Physics2D.BoxCastAll(colliderCenter, Vector2.one * 0.75f, 0, Direction, 0.5f, LayerMask.GetMask("Enemy"));
        float raySize = Direction == Vector2.up ? topRaySize : Direction == Vector2.down ? bottomRaySize : Direction == Vector2.left ? leftRaySize : rightRaySize;
        RaycastHit2D generalHit = Physics2D.Raycast(colliderCenter, Direction, raySize, layers);

        if (CanChangeDirection(enemyHit, generalHit)) // If there is...
        {
            bool[] availableDirections = new bool[directions.Length];

            for (int i = 0; i < directions.Length; i++)
            {
                // A Box Cast will ensure the enemy moves along the grid
                generalHit = Physics2D.BoxCast(colliderCenter, Vector2.one * 0.85f, 0, directions[i], 1f, layers);
                availableDirections[i] = generalHit.collider == null; // A direction will be available if the boxcast didn't hit anything
            }

            if (availableDirections.Count(x => x == false) == directions.Length) return; // Stop if there are no available directions
            else // Finds a new direction for the enemy to move to otherwise
            {
                List<int> availableDirectionIndexes = new List<int>();

                for (int i = 0; i < availableDirections.Length; i++)
                {
                    if (availableDirections[i])
                        availableDirectionIndexes.Add(i);
                }

                int randIndex = Random.Range(0, availableDirectionIndexes.Count);
                int newDirectionIndex = availableDirectionIndexes[randIndex];
                Direction = directions[newDirectionIndex];
            }
        }

        Debug.DrawLine(colliderCenter, colliderCenter + (Vector3)Direction * raySize, Color.red);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Explosion") && !Invincible)
        {
            lives--;

            StartCoroutine(FlashOnDamage());
            if (lives == 0) StartCoroutine(DisableEnemy());
        }
    }

    private IEnumerator FlashOnDamage()
    {
        float flashTime = 0.05f;
        int flashCount = 10;

        for (int i = 0; i < flashCount; i++)
        {
            spriteRenderer.color = Color.gray;
            yield return new WaitForSeconds(flashTime);

            spriteRenderer.color = Color.white;
            yield return new WaitForSeconds(flashTime);
        }
    }

    private IEnumerator DisableEnemy()
    {
        Animator.speed = 0;

        yield return new WaitForSeconds(1);

        deathExplosion = Instantiate(gameManager.deathExplosionPrefab, transform.position, Quaternion.identity);

        gameManager.EnemiesRemaining--;
        playerOneController.Score += points;

        if (WasSpawnedFromExit)
        {
            gameManager.EnemySpawnedFromExit = false;
        }

        gameObject.SetActive(false);
    }


    private void DestroyEnemy()
    {
        Destroy(deathExplosion);
        Destroy(gameObject);
    }

    private void ChangeAnimation()
    {
        if (hasDirectionalAnimations)
        {
            if (Direction == Vector2.up) Animator.Play("WalkUp");
            else if (Direction == Vector2.down) Animator.Play("WalkDown");
            else if (Direction == Vector2.left || Direction == Vector2.right) Animator.Play("WalkSide");

            //Animator.SetBool("isWalkingUp", Direction == Vector2.up);
            //Animator.SetBool("isWalkingDown", Direction == Vector2.down);
            //Animator.SetBool("isWalkingSide", Direction == Vector2.left || Direction == Vector2.right);
        }
    }

    private void Flip()
    {
        if (hasDirectionalAnimations) spriteRenderer.flipX = Direction == Vector2.right;
    }

}
