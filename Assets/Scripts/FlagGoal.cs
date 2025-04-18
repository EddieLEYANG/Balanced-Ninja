using UnityEngine;
using UnityEngine.SceneManagement;

public class FlagGoal : MonoBehaviour
{
    public int nextLevelIndex = -1; // -1 means load the next scene in build order
    public float transitionDelay = 1.5f;
    public GameObject completionEffect;
    public AudioClip victorySound;
    public string enemyTag = "Enemy";
    
    private Animator animator;
    private AudioSource audioSource;
    private bool levelCompleted = false;

    void Start()
    {
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();
        
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (levelCompleted) return;
        
        if (other.CompareTag("Player") && AreAllEnemiesDefeated())
        {
            CompleteLevelSuccess();
        }
    }
    
    bool AreAllEnemiesDefeated()
    {
        // Count remaining enemies
        GameObject[] enemies = GameObject.FindGameObjectsWithTag(enemyTag);
        return enemies.Length == 0;
    }
    
    void CompleteLevelSuccess()
    {
        levelCompleted = true;
        
        // Play animation if available
        if (animator != null)
        {
            animator.SetTrigger("Activate");
        }
        
        // Play sound if available
        if (victorySound != null)
        {
            audioSource.PlayOneShot(victorySound);
        }
        
        // Spawn effect if available
        if (completionEffect != null)
        {
            Instantiate(completionEffect, transform.position, Quaternion.identity);
        }
        
        // Go to next level after delay
        Invoke("GoToNextLevel", transitionDelay);
    }
    
    void GoToNextLevel()
    {
        int currentIndex = SceneManager.GetActiveScene().buildIndex;
        
        if (nextLevelIndex >= 0)
        {
            // Go to specific level
            SceneManager.LoadScene(nextLevelIndex);
        }
        else
        {
            // Go to next level
            int nextIndex = currentIndex + 1;
            
            // Check if there are more levels
            if (nextIndex < SceneManager.sceneCountInBuildSettings)
            {
                SceneManager.LoadScene(nextIndex);
            }
            else
            {
                // No more levels, go back to first level or menu
                SceneManager.LoadScene(0);
            }
        }
    }
} 