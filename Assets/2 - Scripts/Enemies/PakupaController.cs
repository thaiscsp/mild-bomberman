using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PakupaController : EnemyController
{
    GameObject bomb;
    Vector3 colliderCenter;
    Vector2[] directions = { Vector2.up, Vector2.down, Vector2.left, Vector2.right };
    public float bombRaySize = 6;

    void Start()
    {
        
    }

    public override void Update()
    {
        colliderCenter = GetComponent<BoxCollider2D>().bounds.center;

        ChaseBomb();
        ChangeDirection();

        base.Update();
    }

    public override bool CanChangeDirection(RaycastHit2D[] enemyHit, RaycastHit2D generalHit)
    {
        bool canChangeDirection = false;

        // In RaycastAll() for the enemy, the first collider will always be the game object's own collider (enemy detects itself).
        // If the array has more than one element, the second one is sure to be an enemy other than the game object itself.
        if (bomb == null && (enemyHit.Length > 1 || generalHit.collider != null)) canChangeDirection = true;

        return canChangeDirection;
    }

    public override void ChangeDirection()
    {
        int layers = LayerMask.GetMask("Indestructible", "Destructible");

        // Detects if there is something in front of the enemy
        RaycastHit2D[] enemyHit = Physics2D.BoxCastAll(colliderCenter, Vector2.one * 0.75f, 0, Direction, boxDistance, LayerMask.GetMask("Enemy"));
        float raySize = Direction == Vector2.up || Direction == Vector2.down ? verticalRaySize : horizontalRaySize;
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
                base.Direction = directions[newDirectionIndex];
            }
        }

        Debug.DrawLine(colliderCenter, colliderCenter + Vector3.up * bombRaySize, Color.blue);
        Debug.DrawLine(colliderCenter, colliderCenter + Vector3.down * bombRaySize, Color.blue);
        Debug.DrawLine(colliderCenter, colliderCenter + Vector3.left * bombRaySize, Color.blue);
        Debug.DrawLine(colliderCenter, colliderCenter + Vector3.right * bombRaySize, Color.blue);
        Debug.DrawLine(colliderCenter, colliderCenter + (Vector3)Direction * raySize, Color.red);
    }

    private void ChaseBomb()
    {
        foreach (Vector2 direction in directions)
        {
            RaycastHit2D bombHit = Physics2D.Raycast(colliderCenter, direction, bombRaySize, LayerMask.GetMask("Bomb"));

            if (bombHit.collider != null)
            {
                bomb = bombHit.collider.gameObject;
                base.Direction = direction;
                return; // Won't check the remaining directions
            }
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.gameObject.layer == LayerMask.NameToLayer("Bomb"))
        {
            print("collider with bomb");
            Destroy(collision.collider.gameObject);
        }
    }

}
