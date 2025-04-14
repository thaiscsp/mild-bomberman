using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;

public class PlayerOneController : MonoBehaviour
{
    Animator animator;
    Rigidbody2D rigidBody;
    SFXManager sfxManager;
    public Collider2D PlayerCollider { get; private set; }
    // public PlayerOneMap InputActions { get; private set; }
    public PlayerOneMap InputActions { get; private set; }
    public SpriteRenderer SpriteRenderer { get; private set; }

    Vector2 walkInputs;
    public Vector3 StartPosition { get; private set; } = new(1, 11.3f, 0);

    public bool Invincible { private get; set; }
    public bool Knockedout { get; set; }
    public int Lives { get; set; }
    public int Score { get; set; }
    public float speed = 3f;
    public int BombsRemaining { get; set; }
    public int TotalBombs { get; set; } = 1;
    bool canPlaceBomb = true;
    public GameObject bombPrefab;
    public Tilemap backgroundTilemap;
    public Tilemap redLightsTilemap;
    public GameObject explosionsParent;
    public int explosionRadius = 1;

    private void Awake()
    {
        InputActions = new PlayerOneMap();
    }

    private void OnEnable()
    {
        InputActions.Enable();
    }

    private void OnDisable()
    {
        InputActions.Disable();
    }

    private void Start()
    {
        animator = GetComponent<Animator>();

        if (SceneManager.GetActiveScene().name != "World Map")
        {
            PlayerCollider = GetComponent<Collider2D>();
            rigidBody = GetComponent<Rigidbody2D>();
            SpriteRenderer = GetComponent<SpriteRenderer>();
            sfxManager = FindFirstObjectByType<SFXManager>();

            BombsRemaining = TotalBombs;
        }
        else
        {
            animator.SetTrigger("blink");
        }
    }

    private void Update()
    {
        if (SceneManager.GetActiveScene().name != "World Map")
        {
            walkInputs = InputActions.PlayerOne.Walk.ReadValue<Vector2>();

            Move();
            SetAnimationParameters();
            Flip();
            CheckBombPressed();
        }
    }

    private void Move()
    {
        rigidBody.velocity = speed * walkInputs;
    }

    private void SetAnimationParameters()
    {
        animator.SetBool("isWalkingUp", walkInputs.y == 1);
        animator.SetBool("isWalkingDown", walkInputs.y == -1);
        animator.SetBool("isWalkingSide", walkInputs.x != 0);
    }

    private void Flip()
    {
        if (walkInputs.x != 0)
            SpriteRenderer.flipX = walkInputs.x > 0;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Enemy") && !Invincible)
        {
            StartCoroutine(DisablePlayerOne());
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Explosion") && !Invincible)
        {
            StartCoroutine(DisablePlayerOne());
        }
    }

    public IEnumerator DisablePlayerOne(float delay = 0)
    {
        yield return new WaitForSeconds(delay);

        sfxManager.PlayClip(sfxManager.bombermanDies);

        InputActions.Disable();
        PlayerCollider.isTrigger = true;
        transform.position = transform.position + new Vector3(0, 0.3f, 0);
        animator.SetTrigger("knockout");

        yield return new WaitForSeconds(1);

        // Lives--;
        Knockedout = true;
        gameObject.SetActive(false);
    }

    private void CheckBombPressed()
    {
        bool bombPressed = InputActions.PlayerOne.Bomb.IsPressed();

        Vector2 bombPosition = new(Mathf.Round(transform.position.x), Mathf.Round(transform.position.y));
        Collider2D collider = Physics2D.OverlapBox(bombPosition, Vector2.one * 0.75f, 0, LayerMask.GetMask("Bomb"));

        if (bombPressed && BombsRemaining > 0 && collider == null && canPlaceBomb)
        {
            StartCoroutine(SetCanPlaceBomb());
            PlaceBomb(bombPosition);
        }
    }

    // Sets a timer before the next bomb placement, otherwise all of them are used at once
    private IEnumerator SetCanPlaceBomb()
    {
        canPlaceBomb = false;
        yield return new WaitForSeconds(0.25f);
        canPlaceBomb = true;
    }

    private void PlaceBomb(Vector2 bombPosition)
    {
        sfxManager.PlayClip(sfxManager.placeBomb);

        GameObject bomb = Instantiate(bombPrefab, bombPosition, Quaternion.identity);
        bomb.GetComponent<BombController>().BombPosition = bombPosition;
        BombsRemaining--;
    }

}
