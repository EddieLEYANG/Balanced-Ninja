using UnityEngine;

public class SpikeTrap : MonoBehaviour
{
    [Header("Trap Settings")]
    public bool isLethal = true; // If false, will only damage player
    public int damageAmount = 1; // Used if isLethal is false
    
    [Header("Animation Settings")]
    public bool hasAnimation = true;
    public float activationDelay = 0.2f; // Delay before spikes become active after animation
    public float retractDelay = 0.5f; // How long spikes stay extended
    
    [Header("Behavior Pattern")]
    public TrapBehaviorType behaviorType = TrapBehaviorType.TimedCycle;
    public float timeBetweenActivations = 2.0f;
    public bool startActive = false;
    public bool activateOnPlayerNear = false;
    public float playerDetectionRadius = 2.0f;
    
    [Header("Audio")]
    public AudioClip activationSound;
    public AudioClip retractSound;
    
    // Components
    private Animator animator;
    private AudioSource audioSource;
    private BoxCollider2D damageCollider;
    private SpriteRenderer spriteRenderer;
    
    // State variables
    private bool isExtended = false;
    private bool isHazardActive = false;
    private float timer = 0f;
    private string playerTag = "Player";
    private LayerMask playerLayer;

    // Activation states for the trap
    public enum TrapBehaviorType
    {
        TimedCycle,      // Activates on a fixed timer
        PlayerProximity, // Activates when player is nearby
        OneTimeTriggered, // Activates once when triggered by player
        AlwaysActive     // Always dangerous (static spikes)
    }
    
    void Awake()
    {
        // Get components
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();
        damageCollider = GetComponent<BoxCollider2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        
        // Create audio source if not present
        if (audioSource == null && (activationSound != null || retractSound != null))
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.spatialBlend = 1.0f; // 3D sound
        }
        
        // Get player layer
        playerLayer = LayerMask.GetMask("Player");
        
        // Ensure this object is properly tagged
        gameObject.tag = "Hazard";
        
        // Try to set layer to Hazard (layer must exist in project)
        try 
        {
            gameObject.layer = LayerMask.NameToLayer("Hazard");
        }
        catch
        {
            Debug.LogWarning("Hazard layer not found. Please create a Hazard layer in your project.");
        }
    }
    
    void Start()
    {
        // Disable damage collider initially, unless it starts active
        SetHazardActive(startActive);
        
        // If using Always Active behavior, set the trap active immediately
        if (behaviorType == TrapBehaviorType.AlwaysActive)
        {
            SetHazardActive(true);
            ShowSpikes(true);
        }
        
        // Initialize timer for timed cycles
        if (startActive && behaviorType == TrapBehaviorType.TimedCycle)
        {
            timer = retractDelay; // Start in active state
            isExtended = true;
            ShowSpikes(true);
        }
        else
        {
            timer = timeBetweenActivations; // Start in inactive state
        }
    }
    
    void Update()
    {
        switch (behaviorType)
        {
            case TrapBehaviorType.TimedCycle:
                UpdateTimedCycle();
                break;
                
            case TrapBehaviorType.PlayerProximity:
                CheckPlayerProximity();
                break;
                
            case TrapBehaviorType.AlwaysActive:
                // Nothing to update for always active traps
                break;
                
            case TrapBehaviorType.OneTimeTriggered:
                // This is handled by OnTriggerEnter2D
                break;
        }
    }
    
    void UpdateTimedCycle()
    {
        // Update timer
        timer -= Time.deltaTime;
        
        if (timer <= 0)
        {
            if (isExtended)
            {
                // Retract spikes
                isExtended = false;
                ShowSpikes(false);
                SetHazardActive(false);
                
                // Reset timer for next activation
                timer = timeBetweenActivations;
                
                // Play retract sound
                if (retractSound != null && audioSource != null)
                {
                    audioSource.PlayOneShot(retractSound);
                }
            }
            else
            {
                // Extend spikes
                isExtended = true;
                ShowSpikes(true);
                
                // Activate hazard after a short delay
                if (activationDelay > 0)
                {
                    Invoke("ActivateHazard", activationDelay);
                }
                else
                {
                    SetHazardActive(true);
                }
                
                // Reset timer for retraction
                timer = retractDelay;
                
                // Play activation sound
                if (activationSound != null && audioSource != null)
                {
                    audioSource.PlayOneShot(activationSound);
                }
            }
        }
    }
    
    void CheckPlayerProximity()
    {
        if (activateOnPlayerNear && playerDetectionRadius > 0)
        {
            // Check for player in radius
            Collider2D playerCollider = Physics2D.OverlapCircle(transform.position, playerDetectionRadius, playerLayer);
            
            if (playerCollider != null)
            {
                // Player is in range
                if (!isExtended)
                {
                    // Activate the trap
                    isExtended = true;
                    ShowSpikes(true);
                    
                    // Activate hazard after delay
                    if (activationDelay > 0)
                    {
                        Invoke("ActivateHazard", activationDelay);
                    }
                    else
                    {
                        SetHazardActive(true);
                    }
                    
                    // Play activation sound
                    if (activationSound != null && audioSource != null)
                    {
                        audioSource.PlayOneShot(activationSound);
                    }
                }
            }
            else
            {
                // Player is not in range
                if (isExtended)
                {
                    // Start retraction after delay
                    Invoke("RetractSpikes", retractDelay);
                }
            }
        }
    }
    
    void RetractSpikes()
    {
        isExtended = false;
        ShowSpikes(false);
        SetHazardActive(false);
        
        // Play retract sound
        if (retractSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(retractSound);
        }
    }
    
    void OnTriggerEnter2D(Collider2D other)
    {
        // For one-time triggered traps
        if (behaviorType == TrapBehaviorType.OneTimeTriggered && !isExtended && other.CompareTag(playerTag))
        {
            // Activate the trap
            isExtended = true;
            ShowSpikes(true);
            
            // Activate hazard after delay
            if (activationDelay > 0)
            {
                Invoke("ActivateHazard", activationDelay);
            }
            else
            {
                SetHazardActive(true);
            }
            
            // Play activation sound
            if (activationSound != null && audioSource != null)
            {
                audioSource.PlayOneShot(activationSound);
            }
        }
    }
    
    void ShowSpikes(bool show)
    {
        // Update animation if we have one
        if (hasAnimation && animator != null)
        {
            animator.SetBool("IsExtended", show);
        }
        // Otherwise just update the sprite if needed
        else if (spriteRenderer != null)
        {
            // Could switch sprites here if you have different sprites
        }
    }
    
    void ActivateHazard()
    {
        SetHazardActive(true);
    }
    
    void SetHazardActive(bool active)
    {
        isHazardActive = active;
        
        // Enable/disable the damage collider
        if (damageCollider != null)
        {
            damageCollider.enabled = active;
        }
    }
    
    // Draw the detection radius in the editor
    void OnDrawGizmosSelected()
    {
        if (activateOnPlayerNear && playerDetectionRadius > 0)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, playerDetectionRadius);
        }
    }
} 