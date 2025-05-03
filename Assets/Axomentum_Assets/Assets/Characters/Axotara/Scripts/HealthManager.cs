using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class HealthManager : MonoBehaviour
{
    [Header("Health Settings")]
    [SerializeField] private int maxHealth = 100;
    [SerializeField] private int currentHealth;

    [Header("Events")]
    public UnityEvent onDamaged;
    public UnityEvent onDeath;
    public UnityEvent onHealthChanged;
    
    [Header("UI")]
    public Image healthBarBGImage;
    public Sprite healthBarImage;

    private void Awake()
    {
        currentHealth = maxHealth;
        
    }
    public void ChangeHealthBGImage()
    {
        if (healthBarBGImage != null || currentHealth == 100)
        {
            healthBarBGImage.sprite = healthBarImage;
        }
        
    }
    public void TakeDamage(int damageAmount)
    {
        currentHealth -= damageAmount;
        currentHealth = Mathf.Max(currentHealth, 0);

        onDamaged?.Invoke();
        onHealthChanged?.Invoke();

        if (currentHealth <= 0)
        {
            onDeath?.Invoke();
        }
    }

    public void Heal(int healAmount)
    {
        currentHealth = Mathf.Min(currentHealth + healAmount, maxHealth);
        onHealthChanged?.Invoke();
    }

    public int GetCurrentHealth() => currentHealth;
    public int GetMaxHealth() => maxHealth;


   
}