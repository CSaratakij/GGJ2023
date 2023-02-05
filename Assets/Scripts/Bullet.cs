using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Bullet : MonoBehaviour
{
    [SerializeField] private float moveSpeed;
    private Rigidbody2D rigid;

    private void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();
    }

    private void FixedUpdate()
    {
        var facing = (transform.localScale.x > 0.0f) ? 1.0f : -1.0f;
        var dir = Vector2.right * facing;
        rigid.velocity = (dir * moveSpeed);
    }

    private void OnTriggerEnter2D(Collider2D collider)
    {
        Debug.Log($"Collide with : {collider.gameObject.tag}");
        //GameMode.EmitAudio(GameMode.SFX.BulletHit);
        gameObject.SetActive(false);
    }
}

