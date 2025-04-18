using UnityEngine;

public class Trap : MonoBehaviour
{
    public bool isMoving = false;
    public float moveDistance = 1f;
    public float moveSpeed = 1f;
    public float delayTime = 2f;
    public bool startActive = false;
    
    private Vector3 startPosition;
    private Vector3 endPosition;
    private float moveTimer = 0f;
    private float delayTimer = 0f;
    private bool isMovingToEnd = true;
    private bool isDelaying = false;
    private Animator animator;
    
    void Start()
    {
        animator = GetComponent<Animator>();
        startPosition = transform.position;
        endPosition = startPosition + Vector3.up * moveDistance;
        
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
    }
    
    void Update()
    {
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
} 