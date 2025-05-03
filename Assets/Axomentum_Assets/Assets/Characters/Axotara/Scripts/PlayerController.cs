using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{
    #region Character Components

    [SerializeField] private Rigidbody2D _rigidBody2D;
    [SerializeField] private Animator _animator;
    private SpriteRenderer _spriteRenderer;
    [SerializeField] private InputAction _inputAction;

    #endregion

    #region Character Values

    [SerializeField] private float moveSpeed;
    [SerializeField] private float jumpForce;
    private bool _isFacingRight = true;
    Vector2 moveDirection = Vector2.zero;

    #endregion

    #region Jump & Ground Check

    [SerializeField] private Transform groundCheckPoint;
    [SerializeField] private float groundCheckRadius = 0.2f;
    [SerializeField] private LayerMask groundLayer;
    private bool isGrounded;

    #endregion
    
    #region Health & Mana System 
    
    [SerializeField] private Image healthBarImage;
    [SerializeField] private float healthBarLerpSpeed = 5f;
    [SerializeField] private HealthManager _healthManager;
    
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
    
    
}
