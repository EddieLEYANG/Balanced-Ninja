using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float lifetime = 5f;
    public GameObject hitEffect;
    
    void Start()
    {
        // Destroy bullet after lifetime
        Destroy(gameObject, lifetime);
    }
    
    void OnTriggerEnter2D(Collider2D other)
    {
        // Skip collision with enemy that shot it
        if (other.CompareTag("Enemy"))
            return;
            
        // Create hit effect if available
        if (hitEffect != null)
        {
            Instantiate(hitEffect, transform.position, Quaternion.identity);
        }
        
        // Player collision is handled by NinjaController
        
        // Destroy the bullet on collision with anything except the enemy
        Destroy(gameObject);
    }
    
    void OnBecameInvisible()
    {
        // Destroy when off screen
        Destroy(gameObject);
    }
} 