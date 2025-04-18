using UnityEngine;

public class EnemyPatrol : MonoBehaviour
{
    public Transform pointA;
    public Transform pointB;
    public float speed = 2f;
    public float waitTime = 0.5f;
    public bool startRight = true;
    
    private Transform currentTarget;
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private Animator animator;
    private bool isWaiting = false;
    private float waitCounter = 0f;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
        
        // Set the initial target
        currentTarget = startRight ? pointB : pointA;
        
        // Initial facing direction
        UpdateFacingDirection();
    }

    void Update()
    {
        if (isWaiting)
        {
            waitCounter -= Time.deltaTime;
            if (waitCounter <= 0)
            {
                isWaiting = false;
                // Switch targets
                currentTarget = (currentTarget == pointA) ? pointB : pointA;
                UpdateFacingDirection();
            }
            return;
        }
        
        // Move towards the current target
        Vector2 targetPosition = currentTarget.position;
        Vector2 currentPosition = transform.position;
        
        // Calculate movement direction
        Vector2 direction = (targetPosition - currentPosition).normalized;
        
        // Move the enemy
        rb.velocity = direction * speed;
        
        // Check if we've reached the target
        float distance = Vector2.Distance(currentPosition, targetPosition);
        if (distance < 0.1f)
        {
            // Stop and wait
            rb.velocity = Vector2.zero;
            isWaiting = true;
            waitCounter = waitTime;
            
            // Trigger animation if available
            if (animator != null)
            {
                animator.SetBool("IsMoving", false);
            }
        }
        else
        {
            // Set animation state if available
            if (animator != null)
            {
                animator.SetBool("IsMoving", true);
            }
        }
    }
    
    void UpdateFacingDirection()
    {
        if (spriteRenderer != null)
        {
            // Flip the sprite based on movement direction
            spriteRenderer.flipX = (currentTarget == pointA);
        }
    }
    
    // For debugging in the editor
    void OnDrawGizmosSelected()
    {
        if (pointA != null && pointB != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(pointA.position, 0.2f);
            Gizmos.DrawWireSphere(pointB.position, 0.2f);
            Gizmos.DrawLine(pointA.position, pointB.position);
        }
    }
} 