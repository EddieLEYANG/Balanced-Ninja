using UnityEngine;

public class EnemyShooter : MonoBehaviour
{
    public GameObject bulletPrefab;
    public Transform shootPoint;
    public float shootInterval = 3f;
    public float bulletSpeed = 3f;
    public Vector2 shootDirection = Vector2.left;
    public AudioClip shootSound;
    
    private float shootTimer;
    private Animator animator;
    private AudioSource audioSource;
    private SpriteRenderer spriteRenderer;

    void Start()
    {
        shootTimer = Random.Range(0f, shootInterval); // Randomize initial shoot time
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        audioSource = GetComponent<AudioSource>();
        
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        
        // Ensure the sprite is facing the right direction
        if (spriteRenderer != null)
        {
            spriteRenderer.flipX = (shootDirection.x > 0);
        }
        
        // Calculate shoot point if not set
        if (shootPoint == null)
        {
            shootPoint = transform;
        }
    }

    void Update()
    {
        shootTimer -= Time.deltaTime;
        
        if (shootTimer <= 0)
        {
            Shoot();
            shootTimer = shootInterval;
        }
    }
    
    void Shoot()
    {
        // Play animation if available
        if (animator != null)
        {
            animator.SetTrigger("Shoot");
        }
        
        // Play sound if available
        if (shootSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(shootSound);
        }
        
        // Instantiate bullet
        GameObject bullet = Instantiate(bulletPrefab, shootPoint.position, Quaternion.identity);
        
        // Set bullet direction and speed
        Rigidbody2D bulletRb = bullet.GetComponent<Rigidbody2D>();
        if (bulletRb != null)
        {
            bulletRb.velocity = shootDirection.normalized * bulletSpeed;
        }
        
        // Rotate bullet to face the direction it's moving
        float angle = Mathf.Atan2(shootDirection.y, shootDirection.x) * Mathf.Rad2Deg;
        bullet.transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
    }
} 