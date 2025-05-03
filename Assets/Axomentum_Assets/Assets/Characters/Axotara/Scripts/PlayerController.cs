using System;
using UnityEngine;
using UnityEngine.InputSystem;

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
    private bool _isFacingRight = false;
    Vector2 moveDirection = Vector2.zero;  

    #endregion

    #region Jump & Ground Check

    [SerializeField] private Transform groundCheckPoint;
    [SerializeField] private float groundCheckRadius = 0.2f;
    [SerializeField] private LayerMask groundLayer;
    private bool isGrounded;

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
    }

    
    private void FixedUpdate()
    {
        _spriteRenderer.flipX = _isFacingRight;
        _animator.SetBool("pGrounded", isGrounded);
        isGrounded = Physics2D.OverlapCircle(groundCheckPoint.position, groundCheckRadius, groundLayer);
        moveDirection = _inputAction.ReadValue<Vector2>();
        _rigidBody2D.linearVelocity = new Vector2(moveDirection.x * moveSpeed * 10, _rigidBody2D.linearVelocity.y);

        if (_rigidBody2D.linearVelocity.x > 0 || _rigidBody2D.linearVelocity.y < 0)
        {
            _animator.SetFloat("pSpeed", 1);
            
        }

        if (_rigidBody2D.linearVelocity.x == 0)
        {
            _animator.SetFloat("pSpeed", 0);
        }
        
        
        
        
    }

    void Jump()
    {
        if (isGrounded)
        {
            _spriteRenderer.flipX = _isFacingRight; // true = right, false = left
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

        if (keyPressed == "d")
        {
            _isFacingRight = true;
   
        }

        if (keyPressed == "a")
        {
            _isFacingRight = false;
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
}