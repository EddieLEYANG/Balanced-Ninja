using UnityEngine;

public class GearTrap : MonoBehaviour
{
    [Header("Movement Settings")]
    public bool isMoving = true;
    public float moveDistance = 1f;
    public float moveSpeed = 1f;
    public float delayTime = 2f;
    public Vector3 moveDirection = Vector3.up; // Customizable movement direction
    
    [Header("Behavior")]
    public bool startActive = false;
    public bool shouldRotate = true;
    public float rotationSpeed = 120f; // Degrees per second
    
    [Header("Audio")]
    public AudioClip movementSound;
    public bool loopAudio = true;
    
    private Vector3 startPosition;
    private Vector3 endPosition;
    private float moveTimer = 0f;
    private float delayTimer = 0f;
    private bool isMovingToEnd = true;
    private bool isDelaying = false;
    private Animator animator;
    private AudioSource audioSource;
    
    void Awake()
    {
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();
        
        // Set up audio if needed
        if (audioSource == null && movementSound != null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.loop = loopAudio;
            audioSource.clip = movementSound;
            audioSource.spatialBlend = 1.0f; // 3D sound
        }
        
        // Try to set tag to "Trap" if not already set
        if (tag != "Trap" && tag != "Hazard")
        {
            tag = "Trap";
        }
    }
    
    void Start()
    {
        // Calculate start and end positions
        startPosition = transform.position;
        
        // Normalize direction to ensure consistent movement distance
        Vector3 normalizedDirection = moveDirection.normalized;
        endPosition = startPosition + normalizedDirection * moveDistance;
        
        // Set initial state
        if (startActive && animator != null)
        {
            animator.SetBool("IsActive", true);
        }
        
        // Initialize delay timer based on startActive
        if (startActive)
        {
            isDelaying = true;
            delayTimer = delayTime;
        }
        
        // Start audio if we're moving and have sound
        if (isMoving && audioSource != null && movementSound != null && loopAudio)
        {
            audioSource.Play();
        }
    }
    
    void Update()
    {
        // Handle rotation if enabled
        if (shouldRotate)
        {
            transform.Rotate(0, 0, rotationSpeed * Time.deltaTime);
        }
        
        if (!isMoving) return;
        
        // Handle delay between movements
        if (isDelaying)
        {
            delayTimer -= Time.deltaTime;
            if (delayTimer <= 0)
            {
                isDelaying = false;
                
                // Toggle trap state if there's an animator
                if (animator != null)
                {
                    animator.SetBool("IsActive", isMovingToEnd);
                }
                
                // Play sound if we have one and it's not looping
                if (audioSource != null && movementSound != null && !loopAudio)
                {
                    audioSource.PlayOneShot(movementSound);
                }
            }
            return;
        }
        
        // Move the trap
        moveTimer += Time.deltaTime;
        float percentComplete = moveTimer / moveSpeed;
        
        if (percentComplete >= 1.0f)
        {
            // Reset for next movement
            moveTimer = 0f;
            isDelaying = true;
            delayTimer = delayTime;
            isMovingToEnd = !isMovingToEnd;
            return;
        }
        
        // Calculate position
        Vector3 targetPos = isMovingToEnd ? endPosition : startPosition;
        Vector3 startPos = isMovingToEnd ? startPosition : endPosition;
        transform.position = Vector3.Lerp(startPos, targetPos, percentComplete);
    }
    
    void OnDrawGizmosSelected()
    {
        // Show movement path in editor
        if (Application.isPlaying)
        {
            // Use calculated positions during play
            Gizmos.color = Color.red;
            Gizmos.DrawLine(startPosition, endPosition);
            Gizmos.DrawWireSphere(startPosition, 0.1f);
            Gizmos.DrawWireSphere(endPosition, 0.1f);
        }
        else
        {
            // Calculate for editor view
            Vector3 start = transform.position;
            Vector3 normalizedDirection = moveDirection.normalized;
            Vector3 end = start + normalizedDirection * moveDistance;
            
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(start, end);
            Gizmos.DrawWireSphere(start, 0.1f);
            Gizmos.DrawWireSphere(end, 0.1f);
        }
    }
} 