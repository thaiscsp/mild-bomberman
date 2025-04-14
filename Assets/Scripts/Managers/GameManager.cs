using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;

public class GameManager : MonoBehaviour
{
    // Inspector variables
    [Header("Game Objects")]
    public GameObject clock;
    public GameObject timer;
    public GameObject puropenPrefab;
    public GameObject enemiesParent;
    public GameObject scenario;

    [Header("Tilemaps")]
    public Tilemap backgroundTilemap;
    public Tilemap buildingsTilemap;
    public Tilemap redLightsTilemap;

    [Header("UI")]
    public GameObject levelDisplay;
    public TextMeshProUGUI livesDisplay;
    public TextMeshProUGUI scoreDisplay;

    // Script variables
    Animator clockAnimator;
    Animator timerAnimator;
    PlayerOneController playerOneController;
    TilemapController tilemapController;

    Vector3 levelDisplayMiddlePos = new(6.969f, 5.625f, 0);
    Vector3 levelDisplayFinalPos = new(19.22f, 5.625f, 0);
    Vector3 scenarioFinalPos = new(0.5f, 0.5f, 0);
    public bool ExitSpawned { get; set; }
    public bool EnemySpawnedFromExit { get; set; }
    public int EnemiesRemaining { get; set; }
    float timeLimit = 180f;
    float startTime;
    bool timeLimitReached;
    bool countElapsedTime;
    
    private void Start()
    {
        RetrieveComponents();

        playerOneController.gameObject.SetActive(false);

        SetInitialStats();
        SetTilemapsColor();
        StartCoroutine(MoveLevelDisplayRight(levelDisplayMiddlePos));
    }

    private void Update()
    {
        CheckPlayerOneLives();
        SetStatsUIText();
        CheckTimeLimit();
    }

    private void RetrieveComponents()
    {
        clockAnimator = clock.GetComponent<Animator>();
        timerAnimator = timer.GetComponent<Animator>();
        playerOneController = FindFirstObjectByType<PlayerOneController>();
        tilemapController = FindFirstObjectByType<TilemapController>();
    }

    private void SetInitialStats()
    {
        playerOneController.Lives = 5;
        playerOneController.Score = 0;
    }

    private void SetTilemapsColor()
    {
        buildingsTilemap.color = Color.black;
        redLightsTilemap.color = Color.black;
        backgroundTilemap.color = Color.black;
    }

    private IEnumerator MoveLevelDisplayRight(Vector3 position)
    {
        while (levelDisplay.transform.position != position)
        {
            levelDisplay.transform.position = Vector3.MoveTowards(levelDisplay.transform.localPosition, position, 20 * Time.deltaTime);
            yield return null;
        }

        if (position == levelDisplayMiddlePos)
        {
            StartCoroutine(MoveScenarioUp());
            yield return new WaitForSeconds(0.1f);
            StartCoroutine(ChangeScenarioColor());
        }
        else if (position == levelDisplayFinalPos)
        {
            yield return new WaitForSeconds(0.25f);
            StartNewLevel();
        }
    }

    private IEnumerator MoveScenarioUp()
    {
        while (scenario.transform.position != scenarioFinalPos)
        {
            scenario.transform.position = Vector3.MoveTowards(scenario.transform.position, scenarioFinalPos, 20 * Time.deltaTime);
            yield return null;
        }

        yield return new WaitForSeconds(1);
        StartCoroutine(MoveLevelDisplayRight(levelDisplayFinalPos));
    }

    private IEnumerator ChangeScenarioColor()
    {
        float t = Mathf.Clamp01(Time.time * Time.deltaTime);

        while (t < 1)
        {
            backgroundTilemap.color = Color.Lerp(backgroundTilemap.color, Color.white, t);
            buildingsTilemap.color = Color.Lerp(buildingsTilemap.color, Color.white, t);
            redLightsTilemap.color = Color.Lerp(redLightsTilemap.color, Color.white, t);
            t = Mathf.Clamp01(Time.time * Time.deltaTime);
            // print(t);
            yield return null;
        }
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
            gameObject.GetComponent<PuropenController>().Invincible = true;
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
            gameObject.GetComponent<PuropenController>().Invincible = false;
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
        SetTilemapsColor();
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

    private void StartNewLevel()
    {
        startTime = Time.time;
        countElapsedTime = true;

        tilemapController.SetAnimatedTileSpeeds(7);

        clockAnimator.SetBool("tick", true);
        timerAnimator.SetTrigger("deplete");

        playerOneController.gameObject.SetActive(true);

        StartCoroutine(ActivateIFrame(playerOneController.gameObject, 100));
        SpawnEnemies(puropenPrefab, 3, false);
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
                enemy.GetComponent<PuropenController>().WasSpawnedFromExit = wasSpawnedFromExit;
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

}
