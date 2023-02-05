using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(CapsuleCollider2D))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 10.0f;
    [SerializeField] private float jumpSpeed = 10.0f;
    [SerializeField] private float dashSpeed = 10.0f;
    [SerializeField] private float dashDuration = 0.5f;
    [SerializeField] private float dashCooldown = 1.0f;
    [SerializeField] private float gravity = -10.0f;
    [SerializeField] private float gravityMultipilerWhenJump = 0.5f;
    [SerializeField] private float gravityMultipilerWhenFalling = 1.0f;
    [SerializeField] private Vector2 groundTestSize = Vector2.one;
    [SerializeField] private Transform groundTestOrigin;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private LayerMask hurtLayer;

    [Header("Attack")]
    [SerializeField] private float meleeCooldown = 1.0f;
    [SerializeField] private float shootCooldown = 1.0f;
    [SerializeField] private int bulletPoolSize = 30;
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private LayerMask enemyLayer;

    [Header("Animation")]
    [SerializeField] private Animator anim;

    private Vector2 inputDirection;
    private Vector2 moveDirection;
    private Rigidbody2D rigid;
    private CapsuleCollider2D capsuleCollider2D;

    private bool isAlive = true;
    private bool isDashAvailable = true;
    private bool isPressedJump;
    private bool isPressedDash;
    private bool isPressedMelee;
    private bool isPressedShoot;
    private bool isJump;
    private bool isDash;
    private bool isGrounded;
    private bool isFacingRight = true;
    private float dashTimer;
    private float dashCooldownTimer;

    private GameObject[] bulletPools;

    private void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();
        capsuleCollider2D = GetComponent<CapsuleCollider2D>();

        if (bulletPrefab)
        {
            bulletPools = new GameObject[bulletPoolSize];

            for (int i = 0; i < bulletPoolSize; ++i)
            {
                bulletPools[i] = Instantiate(bulletPrefab, Vector3.zero, Quaternion.identity);
                bulletPools[i].gameObject.SetActive(false);
            }
        }
    }

#if UNITY_EDITOR
    void OnDrawGizmos()
    {
        if (groundTestOrigin)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(groundTestOrigin.transform.position, groundTestSize);
        }
    }
#endif

    private void Update()
    {
        InputHandler();
        FacingHandler();
        AnimationHandler();
    }

    private void FixedUpdate()
    {
        CheckHurtBox();
        MovementHandler();
    }

    private void InputHandler()
    {
        if (!isAlive)
        {
            inputDirection = Vector2.zero;
        }

        isPressedJump = Input.GetButtonDown("Jump");
        isPressedDash = Input.GetButtonDown("Dash");
        isPressedMelee = Input.GetButtonDown("Melee");
        isPressedShoot = Input.GetButtonDown("Shoot");

        // Movement
        inputDirection.x = Input.GetAxisRaw("Horizontal");

        if (inputDirection.magnitude > 1.0f)
        {
            inputDirection.Normalize();
        }

        if (isPressedJump && isGrounded)
        {
            isJump = true;
            isPressedJump = false;
            rigid.velocity = new Vector2(rigid.velocity.x, jumpSpeed);
        }

        // Dash
        bool isDashTimeout = (Time.time > dashTimer);
        bool isDashCooldownTimeout = (Time.time > dashCooldownTimer);

        bool canDash = isGrounded && (isDashTimeout && isDashCooldownTimeout && isDashAvailable);

        if (isGrounded)
        {
            isDashAvailable = true;
        }

        if (isDash)
        {
            if (isDash && isDashTimeout)
            {
                isDash = false;
            }
        }
        else
        {
            if (isPressedDash && canDash)
            {
                isPressedDash = false;
                isDash = true;
                isDashAvailable = false;

                dashTimer = (Time.time + dashDuration);
                dashCooldownTimer = (dashTimer + dashCooldown);

                rigid.velocity = new Vector2(dashSpeed, rigid.velocity.y);
            }
        }

        // Shoot

        // Melee

    }

    private void FacingHandler()
    {
        if (!isGrounded)
        {
            return;
        }

        if (inputDirection.x > 0.0f && !isFacingRight)
        {
            isFacingRight = true;
            SetFacing(isFacingRight);
        }
        else if (inputDirection.x < 0.0f && isFacingRight)
        {
            isFacingRight = false;
            SetFacing(isFacingRight);
        }

        void SetFacing(bool isFacingRight)
        {
            var newLocalScale = transform.localScale;
            newLocalScale.x = (isFacingRight) ? 1.0f : -1.0f;
            transform.localScale = newLocalScale;
        }
    }

    private void MovementHandler()
    {
        if (!isAlive)
        {
            rigid.velocity = Vector2.zero;
            return;
        }

        var hitCollider = Physics2D.OverlapBox(groundTestOrigin.transform.position, groundTestSize, 0.0f, groundLayer);
        isGrounded = (hitCollider != null);

        if (isDash)
        {
            moveDirection = (transform.localScale.x > 0.0f) ? Vector2.right : Vector2.left;
            var v = new Vector2(moveDirection.x * dashSpeed, 0.0f);
            rigid.velocity = v;
        }
        else
        {
            moveDirection = inputDirection;

            if (isGrounded)
            {
                var v = new Vector2(moveDirection.x * moveSpeed, rigid.velocity.y);
                rigid.velocity = v;
            }
            else
            {
                if (isJump && rigid.velocity.y < -0.1f)
                {
                    isJump = false;
                }

                if (isJump)
                {
                    rigid.velocity += new Vector2(0, (gravity * gravityMultipilerWhenJump) * Time.fixedDeltaTime);
                }
                else
                {
                    rigid.velocity += new Vector2(0, (gravity * gravityMultipilerWhenFalling) * Time.fixedDeltaTime);
                }
            }
        }
    }

    private void AnimationHandler()
    {
        if (Time.frameCount % 2 != 0.0f)
        {
            return;
        }

        if (!anim)
        {
            return;
        }
    }

    private void CheckHurtBox()
    {
        if (isAlive)
        {
            var hitCollider = Physics2D.OverlapBox(groundTestOrigin.transform.position, groundTestSize, 0.0f, hurtLayer); // TODO : actual hurtbox
            bool isHitSomething = (hitCollider != null);

            if (isHitSomething)
            {
                Dead();
            }
        }
    }

    private void Dead()
    {
        isAlive = false;
        capsuleCollider2D.enabled = false;
        GameMode.ForceGameOver();
    }
}

