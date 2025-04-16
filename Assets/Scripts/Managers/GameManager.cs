using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    // Inspector variables
    [Header("Game Objects")]
    public GameObject clock;
    public GameObject timer;
    public GameObject denkyunPrefab;
    public GameObject puropenPrefab;
    public GameObject enemiesParent;
    public GameObject deathExplosionPrefab;

    [Header("UI")]
    public TextMeshProUGUI livesDisplay;
    public TextMeshProUGUI scoreDisplay;

    // Script variables
    Animator clockAnimator;
    Animator timerAnimator;
    public PlayerOneController playerOneController;
    LevelIntroManager levelIntroManager;
    TilemapController tilemapController;

    public int currentLevel = 1;
    public bool ExitSpawned { get; set; }
    public bool EnemySpawnedFromExit { get; set; }
    public int EnemiesRemaining { get; set; }
    float timeLimit = 180f;
    float startTime;
    bool timeLimitReached;
    bool countElapsedTime;

    private void Awake()
    {
        DontDestroyOnLoad(this.gameObject);
        // DontDestroyOnLoad(playerOneController.gameObject);
    }

    private void Start()
    {
        clockAnimator = clock.GetComponent<Animator>();
        timerAnimator = timer.GetComponent<Animator>();
        playerOneController = FindFirstObjectByType<PlayerOneController>();
        levelIntroManager = FindFirstObjectByType<LevelIntroManager>();
        tilemapController = FindFirstObjectByType<TilemapController>();

        playerOneController.gameObject.SetActive(false);

        SetInitialStats();
    }

    private void Update()
    {
        CheckPlayerOneLives();
        SetStatsUIText();
        CheckTimeLimit();
    }

    private void SetInitialStats()
    {
        playerOneController.Lives = 5;
        playerOneController.Score = 0;
    }

    public void CheckPlayerOneLives()
    {
        if (playerOneController.Knockedout)
        {
            playerOneController.Knockedout = false;
            StartCoroutine(ActivateIFrame(playerOneController.gameObject, 100));

            if (playerOneController.Lives > 0)
            {
                Invoke("ReenablePlayerOne", 1);
            }
            else
            {
                Invoke("RestartLevel", 1);
            }
        }
    }

    private IEnumerator ActivateIFrame(GameObject gameObject, int flashCount)
    {
        if (gameObject.name == "Player One")
        {
            gameObject.GetComponent<PlayerOneController>().Invincible = true;
            gameObject.GetComponent<Collider2D>().excludeLayers |= LayerMask.GetMask("Enemy");
        }
        else
        {
            gameObject.GetComponent<EnemyController>().Invincible = true;
        }

        SpriteRenderer spriteRenderer = gameObject.GetComponent<SpriteRenderer>();
        Color originalColor = spriteRenderer.color;

        for (int i = 0; i < flashCount; i++)
        {
            spriteRenderer.color = Color.gray;
            yield return new WaitForSeconds(0.05f);

            spriteRenderer.color = originalColor;
            yield return new WaitForSeconds(0.05f);
        }

        if (gameObject.name == "Player One")
        {
            gameObject.GetComponent<PlayerOneController>().Invincible = false;
            gameObject.GetComponent<Collider2D>().excludeLayers &= ~LayerMask.GetMask("Enemy");
        }
        else
        {
            gameObject.GetComponent<EnemyController>().Invincible = false;
        }
    }

    private void ReenablePlayerOne()
    {
        playerOneController.PlayerCollider.isTrigger = false;
        playerOneController.transform.position = playerOneController.StartPosition;
        playerOneController.Lives--;

        clockAnimator.SetBool("tick", true);
        timerAnimator.Rebind();
        timerAnimator.SetTrigger("deplete");

        playerOneController.gameObject.SetActive(true);

        countElapsedTime = true;
        timeLimitReached = false;
        startTime = Time.time;
    }

    private void RestartLevel()
    {
        levelIntroManager.SetTilemapsColor();
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    private void SetStatsUIText()
    {
        livesDisplay.text = playerOneController.Lives.ToString();
        scoreDisplay.text = playerOneController.Score.ToString();
    }

    private void CheckTimeLimit()
    {
        if (countElapsedTime && !timeLimitReached)
        {
            float elapsedTime = Time.time - startTime;

            if (elapsedTime >= timeLimit)
            {
                countElapsedTime = false;

                clockAnimator.SetBool("tick", false);

                StartCoroutine(playerOneController.DisablePlayerOne(5));
            }
        }
    }

    public void StartNewLevel()
    {
        startTime = Time.time;
        countElapsedTime = true;

        tilemapController.SetAnimatedTileSpeeds(7);

        clockAnimator.SetBool("tick", true);
        timerAnimator.SetTrigger("deplete");

        playerOneController.gameObject.SetActive(true);

        StartCoroutine(ActivateIFrame(playerOneController.gameObject, 100));
        
        if (currentLevel != 3) SpawnEnemies(puropenPrefab, 3, false);
        if (currentLevel == 2) SpawnEnemies(denkyunPrefab, 2, false);
    }

    public void SpawnEnemies(GameObject enemyPrefab, int totalEnemies, bool wasSpawnedFromExit, Vector3? forcedPosition = null)
    {
        Vector2 playerStartPosition = new(1, 11);
        Vector3 offset = new(0, 0.25f, 0); // Trying to adjust the position to the Puropen's middle
        Vector3 position = Vector3.zero;
        List<Vector3> usedPositions = new();
        bool positionChosen = false;
        int placedEnemies = 0;

        while (placedEnemies < totalEnemies)
        {
            if (forcedPosition == null)
            {
                int row = Random.Range(1, tilemapController.totalRows + 1);
                int column = Random.Range(1, tilemapController.totalColumns + 1);
                Vector3 randomPosition = new(column, row);

                if (!usedPositions.Contains(randomPosition) && tilemapController.map[row - 1, column - 1] == tilemapController.backgroundCode)
                {
                    position = randomPosition;
                    positionChosen = true;
                }
            } else
            {
                position = forcedPosition.Value;
                positionChosen = true;
            }

            if (positionChosen)
            {
                GameObject enemy = Instantiate(enemyPrefab, position + offset, Quaternion.identity);
                enemy.GetComponent<EnemyController>().WasSpawnedFromExit = wasSpawnedFromExit;
                enemy.transform.SetParent(enemiesParent.transform);

                if (wasSpawnedFromExit)
                {
                    StartCoroutine(ActivateIFrame(enemy, 100));
                }

                usedPositions.Add(position); // To prevent the same position from being used for different enemies
                EnemiesRemaining++;
                placedEnemies++;

                positionChosen = false; // For the next iteration
            }
        }
    }

    public void GoToNextLevel()
    {
        if (currentLevel < 3) currentLevel++;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

}
