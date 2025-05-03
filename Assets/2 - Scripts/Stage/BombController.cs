using System.Collections;
using System.Data.Common;
using UnityEngine;
using UnityEngine.Tilemaps;

public class BombController : MonoBehaviour
{
    PlayerOneController playerOneController;
    GameManager gameManager;
    SFXManager sfxManager;
    Collider2D bombCollider;
    TilemapController tilemapController;
    public int exitChance = 10;
    public int itemChance = 50;
    int remainingDestructibles;
    public Vector3 BombPosition { get; set; }

    [Header("Bombs")]
    private float bombFuseTime = 2.9f;

    [Header("Explosion")]
    public GameObject[] burnedDestructiblePrefabs;
    GameObject burnedDestructiblePrefab;
    public GameObject[] explosionPrefabs; // Order in array: up, down, left, right, middle, up middle, right middle
    private float explosionTime = 1.0f;

    [Header("Items")]
    public GameObject burntItemPrefab;
    public GameObject exitPrefab;
    public GameObject[] itemPrefabs;

    bool forceExplosion;
    bool ignitedAnotherBomb;

    private void Start()
    {
        RetrieveComponents();
        burnedDestructiblePrefab = burnedDestructiblePrefabs[DataManager.instance.level - 1];
        StartCoroutine(ShowExplosionMiddle(BombPosition, bombFuseTime));
    }

    private void Update()
    {
        if (forceExplosion)
        {
            forceExplosion = false;
            StopAllCoroutines();
            StartCoroutine(ShowExplosionMiddle(BombPosition, 0.15f));
        }
    }

    private void RetrieveComponents()
    {
        bombCollider = GetComponent<Collider2D>();
        gameManager = FindFirstObjectByType<GameManager>();
        playerOneController = FindFirstObjectByType<PlayerOneController>();
        tilemapController = FindFirstObjectByType<TilemapController>();
        remainingDestructibles = tilemapController.totalDes;
        sfxManager = FindFirstObjectByType<SFXManager>();
    }

    public IEnumerator ShowExplosionMiddle(Vector2 bombPosition, float bombFuseTime)
    {
        yield return new WaitForSeconds(bombFuseTime);
        sfxManager.PlayClip(sfxManager.bombExplodes);

        bombCollider.enabled = false; // So that the enemy won't be stopped from walking after spawning from the exit (where the bomb still is after exploding)
        GameObject explosion = Instantiate(explosionPrefabs[4], bombPosition, Quaternion.identity);
        explosion.transform.SetParent(playerOneController.explosionsParent.transform);
        StartCoroutine(DestroyExplosion(explosion));

        ShowExplosionRadius(bombPosition);

        yield return new WaitForSeconds(explosionTime);
        playerOneController.BombsRemaining++;
        Destroy(gameObject);
    }

    private void ShowExplosionRadius(Vector2 bombPosition)
    {
        Vector2[] directions = { Vector2.up, Vector2.down, Vector2.left, Vector2.right };
        int layers = LayerMask.GetMask("Bomb", "Destructible", "Indestructible", "Exit", "Explosion", "Item");

        for (int i = 0; i < directions.Length; i++)
        {
            for (int j = 1; j <= DataManager.instance.explosionRadius; j++)
            {
                GameObject explosionPrefab = null;

                // Checks if there is an obstacle at the position where the explosion will the placed
                Vector2 explosionPosition = bombPosition + directions[i] * j;
                Collider2D collider = Physics2D.OverlapBox(explosionPosition, Vector2.one * 0.75f, 0, layers);

                if (collider == null) // If there isn't...
                {
                    if (j == DataManager.instance.explosionRadius)
                    {
                        explosionPrefab = explosionPrefabs[i]; // Displays the corner explosions
                    }
                    else
                    {
                        // Displays the radius middle explosions
                        if (directions[i] == Vector2.up || directions[i] == Vector2.down)
                        {
                            explosionPrefab = explosionPrefabs[5]; // Up middle
                        }
                        else
                        {
                            explosionPrefab = explosionPrefabs[6]; // Right middle
                        }
                    }

                    // Places the explosion and sets a "timer" to destroy it
                    GameObject explosion = Instantiate(explosionPrefab, explosionPosition, Quaternion.identity);
                    explosion.transform.SetParent(playerOneController.explosionsParent.transform);
                    StartCoroutine(DestroyExplosion(explosion));
                }
                else // If there is...
                {
                    // Performs an action according to the detected obstacle
                    if (collider.gameObject.layer == LayerMask.NameToLayer("Destructible"))
                    {
                        StartCoroutine(DestroyDestructible(explosionPosition));
                    } else if (collider.gameObject.layer == LayerMask.NameToLayer("Item"))
                    {
                        StartCoroutine(DestroyItem(collider.gameObject, explosionPosition));
                    } else if (collider.gameObject.layer == LayerMask.NameToLayer("Bomb") && !collider.GetComponent<BombController>().ignitedAnotherBomb)
                    {
                        ignitedAnotherBomb = true; // So that the bomb that ignited another won't have it's forceExplosion state set to true by the other one
                        collider.GetComponent<BombController>().forceExplosion = true;
                    } else if (collider.gameObject.layer == LayerMask.NameToLayer("Exit") && !gameManager.EnemySpawnedFromExit)
                    {
                        Vector3 position = new(Mathf.RoundToInt(collider.transform.position.x), Mathf.RoundToInt(collider.transform.position.y), 0);
                        gameManager.SpawnEnemies(gameManager.puropenPrefab, 1, true, position);
                        gameManager.EnemySpawnedFromExit = true;
                    }

                    break; // Stops the explosion for the whole direction
                }
            }
        }
    }

    private IEnumerator DestroyExplosion(GameObject explosion)
    {
        yield return new WaitForSeconds(explosionTime);
        Destroy(explosion);
    }

    private IEnumerator DestroyItem(GameObject item, Vector2 explosionPosition)
    {
        Destroy(item);

        GameObject burntItem = Instantiate(burntItemPrefab, explosionPosition, Quaternion.identity);
        yield return new WaitForSeconds(1f);
        Destroy(burntItem);
    }
    
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            bombCollider.isTrigger = false;
        }
    }

    private IEnumerator DestroyDestructible(Vector2 explosionPosition)
    {
        bool forceExitSpawn = false;

        ReplaceTiles(explosionPosition);

        // Instantiates the burned destructible
        GameObject burnedDestructible = Instantiate(burnedDestructiblePrefab, explosionPosition, Quaternion.identity);
        yield return new WaitForSeconds(explosionTime);
        Destroy(burnedDestructible);

        // If this is the last destructible and the exit still hasn't spawned...
        if (remainingDestructibles == 1 && !gameManager.ExitSpawned) forceExitSpawn = true;

        bool exitJustSpawned = SpawnExit(explosionPosition, forceExitSpawn);

        // Spawns an item if an exit didn't spawn before
        if (!exitJustSpawned) SpawnItem(explosionPosition);

        remainingDestructibles--;
    }

    private void ReplaceTiles(Vector2 explosionPosition)
    {
        Vector3Int tilePosition = new(Mathf.RoundToInt(explosionPosition.x) - 1, Mathf.RoundToInt(explosionPosition.y) - 1, 0);

        // Removes the destructible tile
        tilemapController.destructiblesTilemap.SetTile(tilePosition, null);

        // Removes the destructible shadow tile below the destructible
        if (tilePosition.y > 0) tilemapController.backgroundTilemap.SetTile(tilePosition - new Vector3Int(0, 1), tilemapController.Background);

        // Replaces the tile where the destructible was burnt (according to what is on top)
        bool hasDesOnTop = tilemapController.destructiblesTilemap.GetTile(tilePosition + new Vector3Int(0, 1)) != null;
        bool hasIndesOnTop = tilemapController.indestructiblesTilemap.GetTile(tilePosition + new Vector3Int(0, 1)) != null;

        Tile tile = hasDesOnTop ? tilemapController.DesShadow : hasIndesOnTop && (tilePosition.y < 10 || (tilePosition.y == 10 && (DataManager.instance.level < 3 || DataManager.instance.level > 5))) ? tilemapController.IndesShadow : tilemapController.Background;
        tilemapController.backgroundTilemap.SetTile(tilePosition, tile);
    }

    private void SpawnItem(Vector2 position)
    {
        float chance = Random.Range(0, 101);

        if (chance < itemChance)
        {
            int itemIndex = Random.Range(0, itemPrefabs.Length);
            Instantiate(itemPrefabs[itemIndex], position, Quaternion.identity);
        }
    }

    private bool SpawnExit(Vector2 position, bool forceExitSpawn)
    {
        bool exitJustSpawned = false;
        float chance = Random.Range(0, 101);

        if ((chance < exitChance || forceExitSpawn) && !gameManager.ExitSpawned)
        {
            Instantiate(exitPrefab, position, Quaternion.identity);
            exitJustSpawned = true;
            gameManager.ExitSpawned = true;
        }

        return exitJustSpawned;
    }

}
