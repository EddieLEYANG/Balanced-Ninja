using UnityEngine;
using UnityEngine.SceneManagement;

public class NinjaController : MonoBehaviour
{
    [Header("Layer Settings")]
    public LayerMask enemyLayers;
    public LayerMask platformLayers;
    public LayerMask hazardLayers;
    
    [Header("Enemy Interaction")]
    public float enemyBounceForce = 5f;
    public AudioClip killSound;
    public float enemyDetectionRadius = 0.5f;

    [Header("Wall Interaction")]
    public float minWallBounceForce = 1f;
    public float maxWallBounceForce = 5f;
    public float wallBounceMultiplier = 1.2f;
    public float velocityDeadzone = 0.5f; // Below this speed, no bounce occurs
    public float maxBounceVelocity = 8f; // Velocity at which max bounce is applied
    public bool addUpwardForce = true;
    public float upwardForceAmount = 2f;
    public AudioClip wallBounceSound;
    public bool forceWallBounces = false; // Set to true if bounces aren't working reliably

    [Header("Death")]
    public GameObject deathEffect;
    public AudioClip deathSound;
    public float respawnDelay = 2f;
    
    [Header("Debug")]
    public bool showDebugInfo = false;
    public bool showBounceGizmos = false;

    private Rigidbody2D rb;
    private Animator animator;
    private AudioSource audioSource;
    private bool isDead = false;
    private Vector2 lastWallNormal;
    private float lastBounceTime;
    private float bounceCooldown = 0.1f;
    private int defaultLayer;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();
        defaultLayer = gameObject.layer;
        
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        // Enable debug mode if bounces aren't working
        if (forceWallBounces)
        {
            showDebugInfo = true;
            Debug.Log("Force Wall Bounces mode is enabled!");
        }
    }

    void Update()
    {
        // Add continuous collision check for more reliable detection
        if (!isDead)
        {
            CheckEnemyCollisions();
            
            // Debug velocity for troubleshooting
            if (showDebugInfo && Time.frameCount % 30 == 0)
            {
                Debug.Log($"Current Velocity: {rb.velocity.magnitude:F2} ({rb.velocity})");
            }
        }
    }

    void CheckEnemyCollisions()
    {
        // Only check if enemies layer mask is set
        if (enemyLayers.value == 0) return;
        
        // Use layer mask for enemy detection
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(
            transform.position, 
            enemyDetectionRadius > 0 ? enemyDetectionRadius : GetComponent<Collider2D>().bounds.extents.x * 1.2f,
            enemyLayers
        );
        
        foreach (Collider2D enemy in hitEnemies)
        {
            // Skip self-collision
            if (enemy.gameObject == gameObject) continue;
            
            // Kill the enemy
            KillEnemy(enemy.gameObject);
        }
    }

    void KillEnemy(GameObject enemy)
    {
        // Play kill sound
        if (killSound != null)
        {
            audioSource.PlayOneShot(killSound);
        }
        
        // Apply a small bounce
        rb.AddForce(Vector2.up * enemyBounceForce, ForceMode2D.Impulse);
        
        // Trigger animation if available
        if (animator != null)
        {
            animator.SetTrigger("Kill");
        }
        
        // Destroy the enemy
        Destroy(enemy);
        
        if (showDebugInfo)
        {
            Debug.Log($"Killed enemy: {enemy.name}");
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        HandleCollision(collision);
    }
    
    // Also check on collision stay for more reliable detection
    void OnCollisionStay2D(Collision2D collision)
    {
        // Only process if we're moving fast enough to bounce
        if (rb.velocity.magnitude > velocityDeadzone)
        {
            HandleCollision(collision);
        }
    }
    
    void HandleCollision(Collision2D collision)
    {
        if (isDead) return;

        GameObject collisionObject = collision.gameObject;
        int collisionLayer = collisionObject.layer;
        
        if (showDebugInfo)
        {
            Debug.Log($"Collision with {collisionObject.name} (Layer {collisionLayer})");
        }
        
        // Check if the collision is with an enemy layer
        if (IsInLayerMask(collisionLayer, enemyLayers))
        {
            KillEnemy(collisionObject);
        }
        // Check if the collision is with a platform layer
        else if (IsInLayerMask(collisionLayer, platformLayers))
        {
            BounceOffWall(collision);
        }
        // Check if collision is with a hazard
        else if (IsInLayerMask(collisionLayer, hazardLayers))
        {
            Die();
        }
        // For any other non-player collision, assume it's a wall
        else if (collisionLayer != defaultLayer)
        {
            if (showDebugInfo)
            {
                Debug.Log($"Bouncing off default layer object: {collisionObject.name}");
            }
            BounceOffWall(collision);
        }
        // Force wall bounces if enabled
        else if (forceWallBounces)
        {
            Debug.LogWarning($"Force bouncing off: {collisionObject.name}");
            BounceOffWall(collision);
        }
    }

    bool IsInLayerMask(int layer, LayerMask layerMask)
    {
        // Handle the case where no layer mask was set (0)
        if (layerMask.value == 0) return false;
        return ((1 << layer) & layerMask) != 0;
    }

    void BounceOffWall(Collision2D collision)
    {
        // Ensure we don't bounce too frequently
        if (Time.time - lastBounceTime < bounceCooldown) return;
        
        // Get contact point
        ContactPoint2D contact = collision.GetContact(0);
        lastWallNormal = contact.normal;
        
        // Calculate impact velocity (how fast we're moving toward the wall)
        Vector2 currentVelocity = rb.velocity;
        float impactSpeed = Vector2.Dot(-lastWallNormal, currentVelocity);
        
        if (showDebugInfo)
        {
            Debug.Log($"Impact velocity: {impactSpeed:F2}, Deadzone: {velocityDeadzone:F2}");
        }
        
        // Don't bounce if impact speed is below the deadzone and not forcing bounces
        if (impactSpeed < velocityDeadzone && !forceWallBounces)
        {
            // Still stop sliding along the wall by applying the normal force
            Vector2 normalForce = lastWallNormal * Vector2.Dot(currentVelocity, -lastWallNormal);
            rb.velocity += normalForce;
            
            if (showDebugInfo)
            {
                Debug.Log($"Below deadzone, applying normal force: {normalForce}");
            }
            return;
        }
        
        // If forcing bounces, ensure we have a minimum impact speed
        if (forceWallBounces && impactSpeed < velocityDeadzone)
        {
            impactSpeed = velocityDeadzone;
        }
        
        // Calculate bounce force based on impact velocity
        float bounceIntensity = Mathf.InverseLerp(velocityDeadzone, maxBounceVelocity, impactSpeed);
        float scaledBounceForce = Mathf.Lerp(minWallBounceForce, maxWallBounceForce, bounceIntensity);
        
        // Calculate bounce direction (reflect velocity across normal)
        Vector2 bounceDirection = Vector2.Reflect(currentVelocity.normalized, lastWallNormal);
        
        // Make sure bounce direction is not too parallel to the surface
        float parallelness = Vector2.Dot(bounceDirection, lastWallNormal);
        if (parallelness < 0.1f)
        {
            // Adjust direction to bounce more away from the wall
            bounceDirection = (bounceDirection + lastWallNormal * 0.5f).normalized;
            
            if (showDebugInfo)
            {
                Debug.Log("Adjusted bounce direction to avoid wall parallel movement");
            }
        }
        
        // Apply bounce force scaled by current speed and multiplier
        Vector2 bounceForce = bounceDirection * scaledBounceForce * wallBounceMultiplier;
        
        // Apply the bounce
        rb.velocity = Vector2.zero; // Reset velocity before applying new force
        rb.AddForce(bounceForce, ForceMode2D.Impulse);
        
        // Optional: Add upward force to help player stay airborne
        if (addUpwardForce)
        {
            rb.AddForce(Vector2.up * upwardForceAmount * bounceIntensity, ForceMode2D.Impulse);
        }
        
        // Play wall bounce sound if available (volume based on intensity)
        if (wallBounceSound != null)
        {
            audioSource.PlayOneShot(wallBounceSound, bounceIntensity);
        }
        
        // Update last bounce time
        lastBounceTime = Time.time;
        
        // Trigger animation if available
        if (animator != null)
        {
            animator.SetTrigger("Bounce");
            
            // Try to set bounce intensity parameter if it exists
            // Unity doesn't have HasParameter, so we need to try-catch
            try
            {
                animator.SetFloat("BounceIntensity", bounceIntensity);
            }
            catch
            {
                // Parameter doesn't exist, just ignore
            }
        }
        
        if (showDebugInfo)
        {
            Debug.Log($"BOUNCE: {collision.gameObject.name} - Impact: {impactSpeed:F2}, Force: {scaledBounceForce:F2}, Direction: {bounceDirection}");
        }
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (isDead) return;

        int collisionLayer = collision.gameObject.layer;
        
        // Check against hazard layers (like spikes or bullets)
        if (IsInLayerMask(collisionLayer, hazardLayers))
        {
            Die();
        }
        // Also check tags for backward compatibility
        else if (collision.CompareTag("Trap") || collision.CompareTag("Bullet"))
        {
            Die();
        }
        // Check for enemy layers in triggers
        else if (IsInLayerMask(collisionLayer, enemyLayers))
        {
            KillEnemy(collision.gameObject);
        }
    }

    void Die()
    {
        if (isDead) return;
        
        isDead = true;
        
        // Play death sound
        if (deathSound != null)
        {
            audioSource.PlayOneShot(deathSound);
        }

        // Spawn death effect if available
        if (deathEffect != null)
        {
            Instantiate(deathEffect, transform.position, Quaternion.identity);
        }

        // Trigger death animation if available
        if (animator != null)
        {
            animator.SetTrigger("Die");
        }

        // Disable player movement by freezing rigidbody
        rb.velocity = Vector2.zero;
        rb.isKinematic = true;
        
        // Hide the player
        GetComponent<SpriteRenderer>().enabled = false;
        
        // Disable collider
        GetComponent<Collider2D>().enabled = false;
        
        // Restart the current level after a delay
        Invoke("RestartLevel", respawnDelay);
        
        if (showDebugInfo)
        {
            Debug.Log("Player died");
        }
    }

    void RestartLevel()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
    
    // Visual debugging to help set up collision detection
    void OnDrawGizmosSelected()
    {
        // Display the enemy detection radius
        Gizmos.color = Color.red;
        float radius = enemyDetectionRadius > 0 ? 
            enemyDetectionRadius : 
            (GetComponent<Collider2D>() != null ? GetComponent<Collider2D>().bounds.extents.x * 1.2f : 0.5f);
        Gizmos.DrawWireSphere(transform.position, radius);
        
        // Display bounce debugging when enabled
        if (showBounceGizmos && Application.isPlaying && !isDead)
        {
            // Draw the last wall normal
            Gizmos.color = Color.green;
            Gizmos.DrawRay(transform.position, lastWallNormal * 2);
            
            // Draw the current velocity
            Gizmos.color = Color.blue;
            if (rb != null)
            {
                Gizmos.DrawRay(transform.position, rb.velocity.normalized * 2);
            }
        }
    }
} 