using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;

public class Player : MonoBehaviour
{
    // Config variables
    [SerializeField] float runSpeed     = 5f;
    [SerializeField] float jumpSpeed    = 25f;
    [SerializeField] float climbSpeed   = 5f;
    [SerializeField] Vector2 deathKick  = new Vector2(25f, 25f);

    // State
    bool isAlive = true;

    // Cached component references
    Animator anim;
    Rigidbody2D rb2d;
    CapsuleCollider2D body2d;
    BoxCollider2D feet2d;
    float gravityScaleAtStart;

    // Messages then methods
    void Start()
    {
        anim    = GetComponent<Animator>();
        rb2d    = GetComponent<Rigidbody2D>();
        body2d  = GetComponent<CapsuleCollider2D>();
        feet2d  = GetComponent<BoxCollider2D>();
        gravityScaleAtStart = rb2d.gravityScale;
    }

    // Update is called once per frame
    void Update()
    {
        if (!isAlive) { return; }

        Run();
        ClimbLadder();
        Jump();
        FlipSprite();
        Die();
    }

    private void Run()
    {
        float controlThrow  = CrossPlatformInputManager.GetAxis("Horizontal");   // value is btwn -1 to +1
        Vector2 playerVel   = new Vector2(controlThrow * runSpeed, rb2d.velocity.y);
        rb2d.velocity       = playerVel;

        bool playerHasHorizontalSpeed = Mathf.Abs(rb2d.velocity.x) > Mathf.Epsilon;
        anim.SetBool("IsRunning", playerHasHorizontalSpeed);
    }

    private void ClimbLadder()
    {
        if (!body2d.IsTouchingLayers(LayerMask.GetMask("Climbing")))
        {
            anim.SetBool("IsClimbing", false);
            rb2d.gravityScale = gravityScaleAtStart;
            return;
        }

        float controlThrow  = CrossPlatformInputManager.GetAxis("Vertical");
        Vector2 playerVel = new Vector2(rb2d.velocity.x, controlThrow * climbSpeed);
        rb2d.velocity = playerVel;
        rb2d.gravityScale = 0f;

        bool playerHasVerticalSpeed = Mathf.Abs(rb2d.velocity.y) > Mathf.Epsilon;
        anim.SetBool("IsClimbing", playerHasVerticalSpeed);
    }

    private void Jump()
    {
        if (!feet2d.IsTouchingLayers(LayerMask.GetMask("Ground")))
        {
            return;
        }

        if (CrossPlatformInputManager.GetButtonDown("Jump"))
        {
            Vector2 jumpVel = new Vector2(0f, jumpSpeed);
            rb2d.velocity   += jumpVel;
        }
    }

    private void FlipSprite()
    {
        // If player moving horizontally
        // Reverse current scaling of x-axis

        bool playerHasHorizontalSpeed = Mathf.Abs(rb2d.velocity.x) > Mathf.Epsilon;
        if (playerHasHorizontalSpeed)
        {
            transform.localScale = new Vector2(Mathf.Sign(rb2d.velocity.x), 1f);
        }
    }

    private void Die()
    {
        if (body2d.IsTouchingLayers(LayerMask.GetMask("Enemy", "Hazards")))
        {
            anim.SetTrigger("TriggerDead");
            rb2d.velocity = deathKick;
            isAlive = false;
            FindObjectOfType<GameSession>().ProcessPlayerDeath();
        }
    }
}
