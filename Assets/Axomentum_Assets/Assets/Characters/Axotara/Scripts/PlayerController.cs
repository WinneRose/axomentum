using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.SceneManagement;

public class PlayerController : MonoBehaviour
{
    #region Character Components

    [Header("Character Components")]
    [SerializeField] private Rigidbody2D _rigidBody2D;
    [SerializeField] private Animator _animator;
    [SerializeField] private SpriteRenderer _spriteRenderer;
    [SerializeField] private InputAction _inputAction;

    #endregion

    #region Character Values

    [Header("Character Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float jumpForce = 10f;
    private bool _isFacingRight = true;
    private Vector2 moveDirection = Vector2.zero;

    #endregion

    #region Jump & Ground Check

    [Header("Jump & Ground Check")]
    [SerializeField] private Transform groundCheckPoint;
    [SerializeField] private float groundCheckRadius = 0.2f;
    [SerializeField] private LayerMask groundLayer;
    public bool isGrounded;
    public bool canDoubleJump;

    #endregion

    #region Dash

    [Header("Dash Settings")]
    [SerializeField] private float dashForce = 20f;
    [SerializeField] private float dashCooldown = 1.5f;
    private float lastDashTime = -Mathf.Infinity;
    private bool isDashing;
    public float DashCooldown => dashCooldown;
    public float LastDashTime => lastDashTime;
    
    [Header("Dash Visual")]
    [SerializeField] private GameObject dashParticlePrefab;
    [SerializeField] private float dashParticleLifetime = 0.5f;

    #endregion

    #region Health & Mana System

    [Header("Health & UI")]
    [SerializeField] private Image healthBarImage;
    [SerializeField] private float healthBarLerpSpeed = 5f;
    [SerializeField] private HealthManager _healthManager;

    [Header("Particle")]
    [SerializeField] private GameObject damageParticlePS;
    [SerializeField] private float damageParticleLifetime = 0.5f;
    #endregion

    #region Moving Platform

    private string movingPlatformTag = "IceBlock";
    private Transform currentPlatform = null;

    #endregion

    private void OnEnable()
    {
        _inputAction.Enable();
        _inputAction.started += OnMoveKeyStarted;
        _inputAction.canceled += OnMoveKeyCanceled;
        
    }

    private void OnDisable()
    {
        _inputAction.Disable();
        _inputAction.started -= OnMoveKeyStarted;
        _inputAction.canceled -= OnMoveKeyCanceled;
    }

    private void Awake()
    {
        _rigidBody2D = GetComponent<Rigidbody2D>();
        _animator = GetComponent<Animator>();
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _healthManager = GetComponent<HealthManager>();
        
        HealthInitialized();
        
        // Subscribe to health change
        if (_healthManager != null)
        {
            _healthManager.onHealthChanged.AddListener(HealthInitialized);
        }
    }

    private void FixedUpdate()
    {

        if (isDashing)
        {
            Dash();
            isDashing = false;
            return;
        }
        
        
        isGrounded = Physics2D.OverlapCircle(groundCheckPoint.position, groundCheckRadius, groundLayer);
        _animator.SetBool("pGrounded", isGrounded);
        

        moveDirection = _inputAction.ReadValue<Vector2>();
        _rigidBody2D.linearVelocity = new Vector2(moveDirection.x * moveSpeed * 10, _rigidBody2D.linearVelocity.y);

        // Handle flipping based on movement
        // Flip for left-facing default sprite
        if (moveDirection.x > 0.01f)
            _isFacingRight = true;
        else if (moveDirection.x < -0.01f)
            _isFacingRight = false;

        _spriteRenderer.flipX = _isFacingRight;  // ← This is now correct for left-facing base


        // Animation speed state
        _animator.SetFloat("pSpeed", Mathf.Abs(moveDirection.x));
        
        
        
    }

    private void Update()
    {
        if (healthBarImage != null && _healthManager != null)
        {
            float targetFill = (float)_healthManager.GetCurrentHealth() / _healthManager.GetMaxHealth();
            healthBarImage.fillAmount = Mathf.Lerp(healthBarImage.fillAmount, targetFill, Time.deltaTime * healthBarLerpSpeed);
        }
    }

    [ContextMenu("Test: Take 20 Damage")]
    public void TestTakeDamage()
    {
        ReceiveDamage(20);
    }
    
    public void ReceiveDamage(int amount)
    {
        _healthManager.TakeDamage(amount);
        Debug.Log("Received Damage: " + amount);
    }

    void Jump()
    {
        if (isGrounded)
        {
            _rigidBody2D.AddForce(Vector2.up * jumpForce * 10, ForceMode2D.Impulse);
            _animator.SetTrigger("pJump");
        }
        else if (canDoubleJump)
        {
            _rigidBody2D.linearVelocity = new Vector2(_rigidBody2D.linearVelocity.x, 0); // reset vertical
            _rigidBody2D.AddForce(Vector2.up * jumpForce * 10, ForceMode2D.Impulse);
            _animator.SetTrigger("pJump");
            
            canDoubleJump = false;
            ReceiveDamage(2);
        }
    }

    void Crouch()
    {
        _animator.SetBool("pCrouching", true);
    }

    private void OnMoveKeyStarted(InputAction.CallbackContext context)
    {
        string keyPressed = context.control.name;

        if (keyPressed == "w")
        {
            Jump();
        }

        if (keyPressed == "s")
        {
            Crouch();
        }
    }

    private void OnMoveKeyCanceled(InputAction.CallbackContext context)
    {
        string keyReleased = context.control.name;

        if (keyReleased == "s")
            _animator.SetBool("pCrouching", false);
    }

    private void OnDrawGizmosSelected()
    {
        if (groundCheckPoint != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(groundCheckPoint.position, groundCheckRadius);
        }
    }

    public void HealthInitialized()
    {
        if (healthBarImage != null && _healthManager != null)
        {
            healthBarImage.fillAmount = (float)_healthManager.GetCurrentHealth() / _healthManager.GetMaxHealth();
        }
    }
    
    public bool TryDash()
    {
        if (Time.time >= lastDashTime + dashCooldown)
        {
            isDashing = true;
            lastDashTime = Time.time;
            return true;
        }
        return false;
    }

    private void Dash()
    {
        Vector2 dashDirection = _isFacingRight ? Vector2.right : Vector2.left;

        // Reset horizontal momentum before dash
        _rigidBody2D.linearVelocity = new Vector2(0, _rigidBody2D.linearVelocity.y);

        // Instant velocity assignment
        _rigidBody2D.linearVelocity = new Vector2(dashDirection.x * dashForce * 10, _rigidBody2D.linearVelocity.y);

        Debug.Log("Player dashed & Took Damage");
        ReceiveDamage(20);
        
        if (dashParticlePrefab != null)
        {
            GameObject particle = Instantiate(dashParticlePrefab, transform.position, Quaternion.identity);
            Destroy(particle, dashParticleLifetime);
        }
    }
    
    
    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag(movingPlatformTag))
        {
            ContactPoint2D[] contacts = collision.contacts;
            bool landedOnTop = false;
            foreach (ContactPoint2D contact in contacts)
            {
                if (contact.normal.y > 0.5f)
                {
                    landedOnTop = true;
                    break;
                }
            }

            if (landedOnTop)
            {
                transform.SetParent(collision.transform);
                currentPlatform = collision.transform;
                IceBlockController blockController = collision.gameObject.GetComponent<IceBlockController>();
                if (blockController != null)
                {
                    blockController.ActivateBlock();
                }
            }
        }
    }

    void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.transform == currentPlatform)
        {
            currentPlatform = null;
        }
    }
    
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.name == "ElementPortal") // Or use tag for flexibility
        {
            int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
            SceneManager.LoadScene(currentSceneIndex + 1); // Load next scene in build
        }
    }
    
    public void DamageParticle()
    {
        GameObject ps = Instantiate(damageParticlePS, transform.position, Quaternion.identity);
        ps.transform.SetParent(transform);
        StartCoroutine(DestroyAfterSeconds(ps));
    }

    private IEnumerator DestroyAfterSeconds(GameObject ps)
    {
        yield return new WaitForSeconds(damageParticleLifetime);
        Destroy(ps);
    }

    
    
}
