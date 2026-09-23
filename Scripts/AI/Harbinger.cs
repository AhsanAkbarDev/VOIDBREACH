using UnityEngine;

public class Harbinger : MonoBehaviour
{
    [Header("Health")]
    public float health = 500f;
    private float maxHealth;
    private bool isDead;

    [Header("Components")]
    public Animator animator;
    private Rigidbody rb;

    [Header("Player References")]
    public GameObject playerObject;
    public Transform player;
    public Movement playerScript;

    [Header("AI Ranges")]
    public float patrolRange = 30f;
    public float chaseRange = 15f;
    public float attackRange = 5f;

    [Header("Movement")]
    public float patrolSpeed = 1.5f;
    public float chaseSpeed = 3f;

    [Header("Patrol")]
    public Transform[] patrolPoints;
    public Transform currentPatrolPoint;
    private int currentPatrolIndex;

    [Header("Ground Rupture")]
    public int groundRuptureDamage = 40;
    public float shockwaveRadius = 5f;
    public float attackCooldown = 4f;

    private bool attackOnCooldown;
    private bool isAttacking;

    [Header("Spawner")]
    public Spawner enemySpawner;

    public enum HarbingerState
    {
        Idle,
        Patrol,
        Chase,
        Attack
    }

    private HarbingerState currentState = HarbingerState.Idle;

    private void Start()
    {
        FindPlayer();
        GetComponents();
        FindPatrolPoints();
        ChooseFirstPatrolPoint();
    }

    private void Update()
    {
        if (playerScript == null ||
            playerScript.playerDead ||
            isDead ||
            Time.timeScale == 0)
        {
            return;
        }

        switch (currentState)
        {
            case HarbingerState.Idle:
                Idle();
                break;

            case HarbingerState.Patrol:
                Patrol();
                break;

            case HarbingerState.Chase:
                Chase();
                break;

            case HarbingerState.Attack:
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
                "Harbinger could not find Player. " +
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

        patrolPoints = new Transform[pointObjects.Length];

        for (int i = 0; i < pointObjects.Length; i++)
        {
            patrolPoints[i] = pointObjects[i].transform;
        }
    }

    private void ChooseFirstPatrolPoint()
    {
        if (patrolPoints.Length == 0)
        {
            Debug.LogWarning(
                "Harbinger found no PatrolPoints."
            );

            return;
        }

        currentPatrolIndex =
            Random.Range(0, patrolPoints.Length);

        currentPatrolPoint =
            patrolPoints[currentPatrolIndex];
    }

    private void Idle()
    {
        if (DistanceFromPlayer() <= patrolRange)
        {
            animator.SetBool("IsPatrolling", true);
            currentState = HarbingerState.Patrol;
        }
    }

    private void Patrol()
    {
        if (currentPatrolPoint == null)
            return;

        float distance = DistanceFromPlayer();

        if (distance <= chaseRange)
        {
            animator.SetBool("IsPatrolling", false);
            animator.SetBool("IsChasing", true);

            currentState = HarbingerState.Chase;
            return;
        }

        if (distance > patrolRange)
        {
            animator.SetBool("IsPatrolling", false);
            currentState = HarbingerState.Idle;
            return;
        }

        transform.position =
            Vector3.MoveTowards(
                transform.position,
                currentPatrolPoint.position,
                patrolSpeed * Time.deltaTime
            );

        LookAtTarget(currentPatrolPoint.position);
        CheckPatrolPoint();
    }

    private void CheckPatrolPoint()
    {
        if (currentPatrolPoint == null ||
            patrolPoints.Length == 0)
        {
            return;
        }

        float distance =
            Vector3.Distance(
                transform.position,
                currentPatrolPoint.position
            );

        if (distance > 0.1f)
            return;

        currentPatrolIndex++;

        if (currentPatrolIndex >= patrolPoints.Length)
        {
            currentPatrolIndex = 0;
        }

        currentPatrolPoint =
            patrolPoints[currentPatrolIndex];
    }

    private void Chase()
    {
        float distance = DistanceFromPlayer();

        if (distance > chaseRange)
        {
            animator.SetBool("IsChasing", false);
            animator.SetBool("IsPatrolling", true);

            currentState = HarbingerState.Patrol;
            return;
        }

        if (distance <= attackRange &&
            !attackOnCooldown)
        {
            animator.SetBool("IsChasing", false);
            currentState = HarbingerState.Attack;
            return;
        }

        transform.position =
            Vector3.MoveTowards(
                transform.position,
                player.position,
                chaseSpeed * Time.deltaTime
            );

        LookAtTarget(player.position);
    }

    private void Attack()
    {
        if (isAttacking)
            return;

        if (DistanceFromPlayer() > attackRange)
        {
            animator.SetBool("IsChasing", true);
            currentState = HarbingerState.Chase;
            return;
        }

        LookAtTarget(player.position);

        isAttacking = true;
        attackOnCooldown = true;

        animator.SetTrigger("GroundRupture");
    }

    // Animation event called when the Ground Rupture hits the floor.
    public void GroundRuptureImpact()
    {
        Collider[] hits =
            Physics.OverlapSphere(
                transform.position,
                shockwaveRadius
            );

        foreach (Collider hit in hits)
        {
            if (!hit.CompareTag("Player"))
                continue;

            Movement movement =
                hit.GetComponent<Movement>();

            if (movement != null)
            {
                movement.TakeDamage(
                    groundRuptureDamage
                );

                // Prevent multiple player colliders
                // from applying damage more than once.
                break;
            }
        }
    }

    // Animation event called near the end of Ground Rupture.
    public void GroundRuptureFinished()
    {
        isAttacking = false;

        animator.SetBool("IsChasing", true);
        currentState = HarbingerState.Chase;

        Invoke(
            nameof(ResetGroundRupture),
            attackCooldown
        );
    }

    private void ResetGroundRupture()
    {
        attackOnCooldown = false;
    }

    public void TakeDamage(float damage)
    {
        if (isDead)
            return;

        health -= damage;
        health = Mathf.Clamp(health, 0, maxHealth);

        if (health <= 0)
        {
            Die();
            return;
        }

        // Ground Rupture cannot be interrupted
        // by the normal hurt animation.
        if (!isAttacking)
        {
            animator.SetTrigger("Hurt");
        }
    }

    private void Die()
    {
        isDead = true;

        CancelInvoke();

        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
        }

        animator.SetBool("IsPatrolling", false);
        animator.SetBool("IsChasing", false);
        animator.ResetTrigger("GroundRupture");
        animator.SetTrigger("Death");

        if (enemySpawner != null)
        {
            enemySpawner.EnemyDied();
        }
    }

    // Animation event called at the end of the death animation.
    public void Dead()
    {
        Destroy(gameObject);
    }

    private float DistanceFromPlayer()
    {
        return Vector3.Distance(
            transform.position,
            player.position
        );
    }

    private void LookAtTarget(Vector3 target)
    {
        // Keep rotation horizontal.
        target.y = transform.position.y;

        transform.LookAt(target);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(
            transform.position,
            shockwaveRadius
        );
    }
}
