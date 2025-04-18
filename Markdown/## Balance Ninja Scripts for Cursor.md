## Balance Ninja Scripts for Cursor

### 1. LevelTiltController.cs
```csharp
using UnityEngine;

public class LevelTiltController : MonoBehaviour
{
    public float tiltSpeed = 5f;
    private Vector3 lastMousePosition;
    private bool isDragging;

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            lastMousePosition = Input.mousePosition;
            isDragging = true;
        }
        else if (Input.GetMouseButtonUp(0))
        {
            isDragging = false;
        }

        if (isDragging)
        {
            Vector3 delta = Input.mousePosition - lastMousePosition;
            float rotationZ = -delta.x * tiltSpeed * Time.deltaTime;
            transform.Rotate(0f, 0f, rotationZ);
            lastMousePosition = Input.mousePosition;
        }
    }
}
```

### 2. NinjaController.cs
```csharp
using UnityEngine;

public class NinjaController : MonoBehaviour
{
    private Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Enemy"))
        {
            Destroy(collision.gameObject);
        }
        else if (collision.gameObject.CompareTag("Trap"))
        {
            // Handle death or respawn
            Debug.Log("Ninja died!");
        }
    }
}
```

### 3. EnemyPatrol.cs
```csharp
using UnityEngine;

public class EnemyPatrol : MonoBehaviour
{
    public float speed = 2f;
    public Vector2 pointA;
    public Vector2 pointB;

    private Vector2 target;

    void Start()
    {
        target = pointB;
    }

    void Update()
    {
        transform.position = Vector2.MoveTowards(transform.position, target, speed * Time.deltaTime);
        if ((Vector2)transform.position == pointA) target = pointB;
        if ((Vector2)transform.position == pointB) target = pointA;
    }
}
```

### 4. EnemyShooter.cs
```csharp
using UnityEngine;

public class EnemyShooter : MonoBehaviour
{
    public GameObject bulletPrefab;
    public Transform shootPoint;
    public float shootInterval = 2f;
    public float bulletSpeed = 2f;

    void Start()
    {
        InvokeRepeating("Shoot", shootInterval, shootInterval);
    }

    void Shoot()
    {
        GameObject bullet = Instantiate(bulletPrefab, shootPoint.position, Quaternion.identity);
        bullet.GetComponent<Rigidbody2D>().velocity = Vector2.left * bulletSpeed;
    }
}
```

### 5. Bullet.cs
```csharp
using UnityEngine;

public class Bullet : MonoBehaviour
{
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            // Handle player hit
            Debug.Log("Player hit by bullet!");
        }
        Destroy(gameObject);
    }
}
```

### 6. FlagGoal.cs
```csharp
using UnityEngine;

public class FlagGoal : MonoBehaviour
{
    public GameObject[] enemies;

    void Update()
    {
        enemies = GameObject.FindGameObjectsWithTag("Enemy");
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && enemies.Length == 0)
        {
            Debug.Log("Level Complete!");
            // Load next level here
        }
    }
}
```

### Tags Required in Unity
- Player (for the Ninja)
- Enemy (for all enemies)
- Trap (for spikes)
- Bullet
- Flag

### Notes
- Attach `LevelTiltController` to the root of the level GameObject.
- Attach `NinjaController` to the player.
- Set proper tags and colliders.
- Use Rigidbody2D on Ninja and Bullets.
- Adjust bullet direction/rotation if needed.
