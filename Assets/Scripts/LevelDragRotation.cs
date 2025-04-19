using UnityEngine;

public class LevelDragRotation : MonoBehaviour
{
    [Header("References")]
    public Rigidbody2D levelRb;
    public Camera mainCamera;

    [Header("Rotation Settings")]
    public float rotationSensitivity = 1.0f;
    public float maxRotationSpeed = 100f;
    public float damping = 5f;
    public float snapBackSpeed = 2f;
    public bool useRotationLimits = false;
    public float maxRotationAngle = 45f;
    public bool autoSnapBack = false;
    
    [Header("Physics Settings")]
    public bool stabilizeChildren = true;
    public float stabilizationForce = 10f;
    public bool useFixedUpdateForRotation = true;
    
    [Header("Debug")]
    public bool showDebugInfo = false;

    private bool isDragging = false;
    private Vector3 lastMousePos;
    private float targetRotation;
    private Vector2 screenCenter;
    private float rotationVelocity; // For SmoothDamp
    private float lastFrameRotation;

    void Start()
    {
        if (levelRb == null)
            levelRb = GetComponent<Rigidbody2D>();
            
        if (mainCamera == null)
            mainCamera = Camera.main;
            
        // Initialize to current rotation
        targetRotation = levelRb ? levelRb.rotation : 0f;
        lastFrameRotation = targetRotation;
        
        // Get screen center for better rotation calculations
        UpdateScreenCenter();
        
        // Configure Rigidbody2D for more stable rotations
        if (levelRb != null)
        {
            levelRb.interpolation = RigidbodyInterpolation2D.Interpolate;
            levelRb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            
            // Set these for the level root to improve physics stability
            Physics2D.queriesHitTriggers = true;
            Physics2D.queriesStartInColliders = false;
            Physics2D.callbacksOnDisable = false;
        }
    }

    void Update()
    {
        // Handle input in Update for responsive controls
        HandleInput();
        
        // Only apply rotation in Update if not using FixedUpdate
        if (!useFixedUpdateForRotation)
        {
            ApplyRotation();
        }
        
        if (showDebugInfo && Time.frameCount % 60 == 0)
        {
            Debug.Log($"Rotation: {(levelRb ? levelRb.rotation : 0):F2}, Target: {targetRotation:F2}, RotVelocity: {rotationVelocity:F2}");
        }
    }

    void FixedUpdate()
    {
        // Apply rotation in FixedUpdate for more stable physics
        if (useFixedUpdateForRotation)
        {
            ApplyRotation();
        }
        
        // Stabilize physics objects if needed
        if (stabilizeChildren)
        {
            StabilizePhysicsObjects();
        }
    }
    
    void UpdateScreenCenter()
    {
        screenCenter = new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);
    }
    
    void HandleInput()
    {
        if (Input.GetMouseButtonDown(0))
        {
            isDragging = true;
            lastMousePos = Input.mousePosition;
            
            // Re-calculate screen center in case resolution changed
            UpdateScreenCenter();
        }
        else if (Input.GetMouseButtonUp(0))
        {
            isDragging = false;
            
            // Auto snap back to zero if enabled
            if (autoSnapBack)
            {
                targetRotation = 0f;
            }
        }
        
        if (isDragging)
        {
            Vector3 currentMousePos = Input.mousePosition;
            if (currentMousePos == lastMousePos) return;
            
            // Get vectors from screen center for better rotation calculation
            Vector2 prevVec = ((Vector2)lastMousePos - screenCenter).normalized;
            Vector2 currVec = ((Vector2)currentMousePos - screenCenter).normalized;
            
            // Calculate rotation amount based on the angle between the two vectors
            float deltaAngle = Vector2.SignedAngle(prevVec, currVec);
            
            // Apply sensitivity
            deltaAngle *= rotationSensitivity;
            
            // Clamp to max rotation speed per frame
            deltaAngle = Mathf.Clamp(deltaAngle, -maxRotationSpeed * Time.deltaTime, maxRotationSpeed * Time.deltaTime);
            
            // Update target rotation
            targetRotation += deltaAngle;
            
            // Apply rotation limits if enabled
            if (useRotationLimits)
            {
                targetRotation = Mathf.Clamp(targetRotation, -maxRotationAngle, maxRotationAngle);
            }
            
            lastMousePos = currentMousePos;
        }
    }
    
    void ApplyRotation()
    {
        if (levelRb == null) return;
        
        // Skip if the difference is too small
        if (Mathf.Abs(levelRb.rotation - targetRotation) <= 0.01f)
        {
            rotationVelocity = 0f;
            return;
        }
            
        float currentRotation = levelRb.rotation;
        float newRotation;
        
        // Calculate time delta based on whether we're in FixedUpdate or Update
        float timeDelta = useFixedUpdateForRotation ? Time.fixedDeltaTime : Time.deltaTime;
        
        if (isDragging || autoSnapBack)
        {
            // Use SmoothDamp for more stable rotation
            float speedFactor = isDragging ? damping : snapBackSpeed;
            newRotation = Mathf.SmoothDamp(currentRotation, targetRotation, ref rotationVelocity, 
                                          1.0f / speedFactor, Mathf.Infinity, timeDelta);
        }
        else
        {
            // Keep the current target when not dragging, but dampen any residual motion
            newRotation = Mathf.SmoothDamp(currentRotation, targetRotation, ref rotationVelocity, 
                                          0.2f, Mathf.Infinity, timeDelta);
        }
        
        // Apply the rotation - use this instead of setting rotation directly
        levelRb.MoveRotation(newRotation);
        
        // Track rotation for calculating angular velocity
        lastFrameRotation = newRotation;
    }
    
    void StabilizePhysicsObjects()
    {
        if (levelRb == null) return;
        
        // Find all rigidbodies that are children of this level
        Rigidbody2D[] childBodies = GetComponentsInChildren<Rigidbody2D>();
        
        foreach (Rigidbody2D rb in childBodies)
        {
            // Skip the level rigidbody itself
            if (rb == levelRb) continue;
            
            // Skip if object is not enabled
            if (!rb.gameObject.activeInHierarchy) continue;
            
            // Apply additional gravity force in the direction of world down
            // This helps objects stay in place better during rotation
            rb.AddForce(Vector2.down * stabilizationForce, ForceMode2D.Force);
            
            // Reduce angular velocity to prevent spinning objects
            if (Mathf.Abs(rb.angularVelocity) > 0.1f)
            {
                rb.angularVelocity *= 0.95f;
            }
            
            // Ensure the dynamic objects are set to continuous collision detection
            if (rb.bodyType == RigidbodyType2D.Dynamic)
            {
                rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            }
        }
    }
}
