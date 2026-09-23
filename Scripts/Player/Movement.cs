using UnityEngine;

public class Movement : MonoBehaviour
{
    [Header("Movement")]
    public float speed;
    public float smoothness;

    [Header("Health")]
    public float health;
    public float maxHealth;
    public bool playerDead;

    [Header("Game State")]
    public bool powerSurgeActivated;

    private Rigidbody rb;
    private Animator animator;
    private Vector3 spawnPoint;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();

        maxHealth = health;
        spawnPoint = transform.position;
    }

    private void Update()
    {
        if (playerDead || Time.timeScale == 0f)
            return;

        MouseRotation();
    }

    private void FixedUpdate()
    {
        if (playerDead || Time.timeScale == 0f)
            return;

        Move();
    }

    private void MouseRotation()
    {
        Plane groundPlane =
            new Plane(
                Vector3.up,
                transform.position
            );

        Ray ray =
            Camera.main.ScreenPointToRay(
                Input.mousePosition
            );

        if (!groundPlane.Raycast(
            ray,
            out float rayDistance))
        {
            return;
        }

        Vector3 mouseWorldPosition =
            ray.GetPoint(rayDistance);

        Vector3 direction =
            mouseWorldPosition -
            transform.position;

        direction.y = 0f;

        if (direction.sqrMagnitude <= 0.001f)
            return;

        Quaternion targetRotation =
            Quaternion.LookRotation(direction);

        rb.rotation = targetRotation;
    }

    private void Move()
    {
        float horizontal =
            Input.GetAxis("Horizontal");

        float vertical =
            Input.GetAxis("Vertical");

        animator.SetFloat(
            "MoveX",
            horizontal
        );

        animator.SetFloat(
            "MoveY",
            vertical
        );

        Vector3 moveDirection =
            transform.forward * vertical +
            transform.right * horizontal;

        if (moveDirection.sqrMagnitude > 1f)
        {
            moveDirection.Normalize();
        }

        Vector3 targetVelocity =
            moveDirection * speed;

        targetVelocity.y =
            rb.linearVelocity.y;

        Vector3 smoothMovement =
            Vector3.Lerp(
                rb.linearVelocity,
                targetVelocity,
                smoothness
            );

        smoothMovement.y =
            rb.linearVelocity.y;

        rb.linearVelocity =
            smoothMovement;
    }

    public void TakeDamage(int damage)
    {
        if (playerDead)
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
        playerDead = true;

        rb.linearVelocity =
            Vector3.zero;

        animator.SetTrigger("Death");
    }

    public void Respawn()
    {
        transform.position =
            spawnPoint;

        rb.linearVelocity =
            Vector3.zero;

        health = maxHealth;
        playerDead = false;
    }
}
