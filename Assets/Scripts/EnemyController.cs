using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class EnemyController : MonoBehaviour
{
    [Header("General")]
    [SerializeField] private bool isFacingRight = true;
    [SerializeField] private float moveSpeed = 10.0f;
    [SerializeField] private float gravity = -10.0f;
    [SerializeField] private Transform redirectionTestOrigin;
    [SerializeField] private Vector2 redirectionTestSize;
    [SerializeField] private LayerMask redirectionLayer;

    [Header("Debuff")]
    [SerializeField] private int freezeDebuffResistance = 3;
    [SerializeField] private float freezeDebufDuration = 3.0f;

    [Header("Visual")]
    [SerializeField] private Transform parentNormalVisual;
    [SerializeField] private Transform parentFreezeVisual;

    public enum Debuff
    {
        None,
        Freeze
    }

    public Debuff DebuffState => debuffState;

    private int currentFreezeDebuffResistance = 0;
    private bool isAlive = true;
    private bool isDebuffStateChange = false;
    private Debuff previousDebuffState;
    private Debuff debuffState;
    private Rigidbody2D rigid;

#if UNITY_EDITOR
    void OnDrawGizmos()
    {
        if (redirectionTestOrigin)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(redirectionTestOrigin.transform.position, redirectionTestSize);
        }
    }
#endif

    private void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();
    }

    private void OnEnable()
    {
        isAlive = true;

        //currentMoveDirection = (isFacingRight) ? Vector2.right : Vector2.left;
        currentFreezeDebuffResistance = freezeDebuffResistance;

        SetFacing(isFacingRight);
        UpdateGameObjectState(debuffState);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Destroyer"))
        {
            Dead();
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Bullet"))
        {
            TakeFreezeDebuff();
        }
    }

    private void LateUpdate()
    {
#if UNITY_EDITOR
        if (Input.GetKeyDown(KeyCode.H))
        {
            TakeFreezeDebuff();
        }
#endif
    }

    private void FixedUpdate()
    {
        CheckRedirectionBox();
        MovementHandler();
    }

    private void CheckRedirectionBox()
    {
        if (isAlive)
        {
            var hitCollider = Physics2D.OverlapBox(redirectionTestOrigin.transform.position, redirectionTestSize, 0.0f, redirectionLayer);
            bool isHitSomething = (hitCollider != null);

            if (isHitSomething)
            {
                SetFacing(!isFacingRight);
            }
        }
    }

    private void MovementHandler()
    {
        if (!isAlive)
        {
            return;
        }

        bool shouldResetHorizontalMovementDuringFreeze = (isDebuffStateChange && (debuffState == Debuff.Freeze));

        if (shouldResetHorizontalMovementDuringFreeze)
        {
            rigid.velocity = new Vector2(0.0f, rigid.velocity.y);
            isDebuffStateChange = false;
            return;
        }

        if (debuffState != Debuff.Freeze)
        {
            rigid.velocity = new Vector2(transform.localScale.x * moveSpeed, rigid.velocity.y);
        }
    }

    private void UpdateGameObjectState(Debuff debuffState)
    {
        bool isFreezeDebuff = (debuffState == Debuff.Freeze);

        parentNormalVisual.gameObject.SetActive(!isFreezeDebuff);
        parentFreezeVisual.gameObject.SetActive(isFreezeDebuff);

        gameObject.layer = (isFreezeDebuff) ? LayerMask.NameToLayer("IceBlock") : LayerMask.NameToLayer("Enemy");
    }

    private void SetFacing(bool isFacingRight)
    {
        var newLocalScale = transform.localScale;
        newLocalScale.x = (isFacingRight) ? 1.0f : -1.0f;

        transform.localScale = newLocalScale;
        this.isFacingRight = isFacingRight;
    }

    public void TakeFreezeDebuff(int amount = 1)
    {
        int totalResistance = (currentFreezeDebuffResistance - amount);
        totalResistance = Mathf.Clamp(totalResistance, 0, freezeDebuffResistance);

        currentFreezeDebuffResistance = totalResistance;
        bool shouldApplyFreezeDebuff = (currentFreezeDebuffResistance <= 0);

        previousDebuffState = debuffState;
        debuffState = (shouldApplyFreezeDebuff) ? Debuff.Freeze : Debuff.None;

        if (previousDebuffState != debuffState)
        {
            isDebuffStateChange = true;
        }

        UpdateGameObjectState(debuffState);
    }

    public void Dead()
    {
        if (debuffState == Debuff.Freeze)
        {
            GameMode.IncreaseScore();
        }

        isAlive = false;
        Destroy(gameObject);
    }
}

