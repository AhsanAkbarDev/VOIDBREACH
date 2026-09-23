using UnityEngine;

public class Enemy : MonoBehaviour
{
    [Header("Health")]
    public float health;
    public float maxHealth;
    private bool isDead;

    [Header("Components")]
    public Animator animator;
    private Rigidbody rb;

    [Header("Player References")]
    public GameObject playerObject;
    public Transform player;
    public Movement playerScript;

    [Header("AI Ranges")]
    public float patrolRange;
    public float chaseRange;
    public float attackRange;

    [Header("Patrol Points")]
    public Transform[] spawnPoints;
    public Transform currentSpawnPoint;

    [Header("Movement")]
    public float speed = 5f;

    [Header("Attack")]
    public int damage;
    private bool hasAttacked;

    [Header("Spawner")]
    public Spawner enemySpawner;

    private int currentPatrolPointIndex;

    public enum EnemyStates
    {
        Idle,
        Patrol,
        Chase,
        Attack
    }

    private EnemyStates currentState = EnemyStates.Idle;

    private void Start()
    {
        FindPlayer();
        GetComponents();
        FindPatrolPoints();
        ChooseFirstPatrolPoint();
    }

    private void Update()
    {
        if (playerScript == null || playerScript.playerDead || isDead)
            return;

        if (Time.timeScale == 0)
            return;

        switch (currentState)
        {
            case EnemyStates.Idle:
                Idle();
                break;

            case EnemyStates.Patrol:
                Patrol();
                break;

            case EnemyStates.Chase:
                Chase();
                break;

            case EnemyStates.Attack:
                Attack();
                break;
        }
    }

    private void FindPlayer()
    {
        playerObject = GameObject.FindGameObjectWithTag("Player");

        if (playerObject == null)
        {
            Debug.LogError(
                "Enemy could not find Player. " +
                "Make sure the Player has the Player tag."
            );

            return;
        }

        player = playerObject.transform;
        playerScript = playerObject.GetComponent<Movement>();
    }

    private void GetComponents()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody>();

        maxHealth = health;
    }

    private void FindPatrolPoints()
    {
        GameObject[] pointObjects =
            GameObject.FindGameObjectsWithTag("PatrolPoint");

        spawnPoints = new Transform[pointObjects.Length];

        for (int i = 0; i < pointObjects.Length; i++)
        {
            spawnPoints[i] = pointObjects[i].transform;
        }
    }

    private void ChooseFirstPatrolPoint()
    {
        if (spawnPoints.Length == 0)
        {
            Debug.LogWarning("No PatrolPoints found in the scene.");
            return;
        }

        currentPatrolPointIndex =
            Random.Range(0, spawnPoints.Length);

        currentSpawnPoint =
            spawnPoints[currentPatrolPointIndex];
    }

    private void Idle()
    {
        float distanceToPlayer =
            Vector3.Distance(transform.position, player.position);

        if (distanceToPlayer <= patrolRange)
        {
            animator.SetBool("IsPatrolling", true);
            currentState = EnemyStates.Patrol;
        }
    }

    private void Patrol()
    {
        if (currentSpawnPoint == null)
            return;

        CheckIfItHasReached();

        float distanceToPlayer =
            Vector3.Distance(transform.position, player.position);

        if (distanceToPlayer <= chaseRange)
        {
            animator.SetBool("IsPatrolling", false);
            animator.SetBool("IsChasing", true);

            currentState = EnemyStates.Chase;
        }
        else if (distanceToPlayer >= patrolRange)
        {
            animator.SetBool("IsPatrolling", false);
            currentState = EnemyStates.Idle;
        }
        else
        {
            transform.position =
                Vector3.MoveTowards(
                    transform.position,
                    currentSpawnPoint.position,
                    speed * Time.deltaTime
                );

            transform.LookAt(currentSpawnPoint.position);
        }
    }

    private void CheckIfItHasReached()
    {
        if (currentSpawnPoint == null || spawnPoints.Length == 0)
            return;

        float distance =
            Vector3.Distance(
                transform.position,
                currentSpawnPoint.position
            );

        if (distance > 0.05f)
            return;

        currentPatrolPointIndex++;

        if (currentPatrolPointIndex >= spawnPoints.Length)
        {
            currentPatrolPointIndex = 0;
        }

        currentSpawnPoint =
            spawnPoints[currentPatrolPointIndex];
    }

    private void Chase()
    {
        float distanceToPlayer =
            Vector3.Distance(transform.position, player.position);

        if (distanceToPlayer > chaseRange)
        {
            animator.SetBool("IsChasing", false);
            animator.SetBool("IsPatrolling", true);

            currentState = EnemyStates.Patrol;
        }
        else if (distanceToPlayer <= attackRange)
        {
            animator.SetBool("IsChasing", false);
            currentState = EnemyStates.Attack;
        }
        else
        {
            transform.position =
                Vector3.MoveTowards(
                    transform.position,
                    player.position,
                    speed * Time.deltaTime
                );

            transform.LookAt(player.position);
        }
    }

    private void Attack()
    {
        float distanceToPlayer =
            Vector3.Distance(transform.position, player.position);

        if (distanceToPlayer > attackRange)
        {
            animator.SetBool("IsChasing", true);
            currentState = EnemyStates.Chase;
            return;
        }

        if (hasAttacked)
            return;

        animator.SetTrigger("Attack");
        hasAttacked = true;
    }

    // Called by an animation event when the attack connects.
    public void AttackDelay()
    {
        if (playerScript == null)
            return;

        playerScript.TakeDamage(damage);

        Invoke(
            nameof(AttackCoolDown),
            0.7f
        );
    }

    private void AttackCoolDown()
    {
        hasAttacked = false;
    }

    public void TakeDamage(float damage)
    {
        if (isDead)
            return;

        health -= damage;

        health =
            Mathf.Clamp(
                health,
                0,
                maxHealth
            );

        if (health <= 0)
        {
            Die();
            return;
        }

        animator.SetTrigger("Hurt");
    }

    private void Die()
    {
        isDead = true;

        rb.linearVelocity = Vector3.zero;

        animator.SetBool("IsPatrolling", false);
        animator.SetBool("IsChasing", false);
        animator.SetTrigger("Death");

        if (enemySpawner != null)
        {
            enemySpawner.EnemyDied();
        }
    }

    // Called by the death animation event.
    public void Dead()
    {
        Destroy(gameObject);
    }
}
