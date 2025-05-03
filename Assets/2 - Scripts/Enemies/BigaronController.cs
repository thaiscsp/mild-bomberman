using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BigaronController : MonoBehaviour
{
    Animator animator;
    GameObject scenario;
    PlayerOneController playerOneController;
    SFXManager sfxManager;
    Vector3 target;
    bool takingDamage, moving, hammering;
    bool canMove = true;
    int lives = 8;
    string lastChosenAxis;

    public GameObject bigStonePrefab;
    public float speed = 5;

    void Start()
    {
        animator = GetComponent<Animator>();
        playerOneController = FindFirstObjectByType<PlayerOneController>();
        scenario = FindFirstObjectByType<TilemapController>().gameObject;
        sfxManager = FindFirstObjectByType<SFXManager>();
    }

    void Update()
    {
        StartCoroutine(Move());
        StartCoroutine(Hammer());
        GoToTarget();
        DestroyBigaron();
    }

    private IEnumerator Move()
    {
        if (!moving)
        {
            moving = true;

            float xDistance = Mathf.Abs(transform.position.x - playerOneController.transform.position.x);
            float yDistance = Mathf.Abs(transform.position.y - playerOneController.transform.position.y);

            if (xDistance < yDistance)
            {
                if (lastChosenAxis != "x") ChooseTargetOnAxis("x");
                else ChooseTargetOnAxis("y");
            }
            else if (yDistance <= xDistance)
            {
                if (lastChosenAxis != "y") ChooseTargetOnAxis("y");
                else ChooseTargetOnAxis("x");
            }

            yield return new WaitForSeconds(Random.Range(10f, 20f));

            moving = false;
        }
    }

    private void GoToTarget()
    {
        if (target != Vector3.zero && canMove)
        {
            transform.position = Vector3.MoveTowards(transform.position, target, speed * Time.deltaTime);

            if (Vector3.Distance(transform.position, target) < 0.1f) ForceNewTarget();
        }
    }

    private void ForceNewTarget()
    {
        target = Vector3.zero;
        moving = false;
    }

    private void ChooseTargetOnAxis(string axis)
    {
        target = axis == "x" ? new(playerOneController.transform.position.x, transform.position.y, 0) : new(transform.position.x, playerOneController.transform.position.y, 0);
        lastChosenAxis = axis;
    }

    private IEnumerator Hammer()
    {
        if (!hammering)
        {
            hammering = true;

            yield return new WaitForSeconds(6); // Idle time before hammering

            animator.Play("Hammer");
            canMove = false;
            yield return new WaitForSeconds(2);

            animator.Play("Lift");
            yield return new WaitForSeconds(0.083f);

            animator.Play("Idle");
            canMove = true;

            hammering = false;
        }  
    }
    
    // Called in event in "Hammer" animation
    public IEnumerator ShakeScreen()
    {
        sfxManager.PlayClip(sfxManager.bigaronsHammer);

        Vector3 originalPosition = scenario.transform.position;
        Vector3 downTarget = scenario.transform.position - new Vector3(0, 0.5f, 0);

        while (scenario.transform.position != downTarget)
        {
            scenario.transform.position = Vector3.MoveTowards(scenario.transform.position, downTarget, 7 * Time.deltaTime);
            yield return null;
        }

        while (scenario.transform.position != originalPosition)
        {
            scenario.transform.position = Vector3.MoveTowards(scenario.transform.position, originalPosition, 7 * Time.deltaTime);
            yield return null;
        }
    }

    public void SpawnStones()
    {
        List<Vector2> usedForces = new();
        int rockAmount = Random.Range(4, 7);
        
        for (int i = 0; i < rockAmount; i++)
        {
            GameObject stone = Instantiate(bigStonePrefab, transform.position - new Vector3(0, 3, 0), Quaternion.identity);

            Vector2 force = new(Random.Range(-2f, 2f), Random.Range(2f, 5f));

            while (usedForces.Contains(force)) force = new(Random.Range(-2, 2), Random.Range(-1, 1));

            stone.GetComponent<StoneController>().Force = force;
        }
    }

    public IEnumerator TakeDamage()
    {
        if (!takingDamage)
        {
            takingDamage = true;

            print("damage taken");
            lives--;
            canMove = false;
            yield return new WaitForSeconds(1);
            canMove = true;

            takingDamage = false;
        }
    }

    private void DestroyBigaron()
    {
        if (lives == 0)
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Explosion"))
        {
            StartCoroutine(TakeDamage());
        }
    }

}
