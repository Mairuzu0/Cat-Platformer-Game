﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement; //TODO move to it's own script

public class Player : MonoBehaviour
{
    //Config
    [SerializeField] float movementSpeed = 5f;
    [SerializeField] float jumpSpeed = 6f;
    [SerializeField] float climbSpeed = 2f;
    [SerializeField] int health = 3;
    [SerializeField] float damageDelay = 2f;


    //State
    bool isAlive = true;

    //Cached component refernces
    Rigidbody2D myRigidbody;
    Animator myAnimator;
    CapsuleCollider2D myColliderBody;
    BoxCollider2D myColliderFeet;

    float gravityAtStart;

    // Start is called before the first frame update
    void Start()
    {
        myRigidbody = GetComponent<Rigidbody2D>();
        myAnimator = GetComponent<Animator>();
        myColliderBody = GetComponent<CapsuleCollider2D>();
        myColliderFeet = GetComponent<BoxCollider2D>();

        gravityAtStart = myRigidbody.gravityScale;
    }

    // Update is called once per frame
    void Update()
    {
        if (!isAlive)
        {
            return;
        }
        Walk();
        FlipSprite();
        Jump();
        Climb();
        WaterDeath(); //TODO change
        
    }

    void Walk()
    {
        float controlThrow = Input.GetAxis("Horizontal");
        Vector2 playerVelocity = new Vector2(controlThrow * movementSpeed, myRigidbody.velocity.y);
        myRigidbody.velocity = playerVelocity;
        //myRigidbody.velocity = playerVelocity * Time.deltaTime;
        
    }

    void FlipSprite()
    {
        //Check input for horizontal motion
        bool playerHasHorizontalSpeed = Mathf.Abs(myRigidbody.velocity.x) > Mathf.Epsilon;
        //Change Animation State
        myAnimator.SetBool("Walking", playerHasHorizontalSpeed);
        if (playerHasHorizontalSpeed)
        {
            transform.localScale = new Vector2(Mathf.Sign(myRigidbody.velocity.x), 1);
        }
    }

    void Jump()
    {
        if(!myColliderFeet.IsTouchingLayers(LayerMask.GetMask("Ground")))
        {
            myAnimator.SetBool("Jumping", true);
            return;
        }
        else
        {
            myAnimator.SetBool("Jumping", false);
        }

        if (Input.GetButtonDown("Jump"))
        {
            //Change Animation State

            Vector2 jumpVelocity = new Vector2(0f, jumpSpeed);
            myRigidbody.velocity += jumpVelocity;
        }

    }

    void Climb()
    {
        if (!myColliderFeet.IsTouchingLayers(LayerMask.GetMask("Ladder")))
        {
            myAnimator.SetBool("Climbing", false);
            myRigidbody.gravityScale = gravityAtStart;
            return;
        }

        myRigidbody.gravityScale = 0f;

        float controlThrow = Input.GetAxis("Vertical");
        Vector2 climbVelocity = new Vector2(myRigidbody.velocity.x, controlThrow * climbSpeed);
        myRigidbody.velocity = climbVelocity;

        //bool playerHasVerticalSpeed = Mathf.Abs(myRigidbody.velocity.x) > Mathf.Epsilon;
        myAnimator.SetBool("Jumping", false);
        myAnimator.SetBool("Climbing", true);

    }

    public void TakeDamage()
    {
        if (health >= 1)
        {
            StartCoroutine(ProcessDamage());
        }
        else
        {
            FindObjectOfType<SFXPlayer>().DeathMeow();
            FindObjectOfType<GameSession>().ProcessPlayerDeath();
        }
    }

    IEnumerator ProcessDamage()
    {
        yield return new WaitForSecondsRealtime(damageDelay);
        myAnimator.SetTrigger("Damage");
        health--;
    }

    void WaterDeath() //TODO change
    {
        if (myColliderFeet.IsTouchingLayers(LayerMask.GetMask("Water")))
        {
            FindObjectOfType<SFXPlayer>().DeathMeow();
            FindObjectOfType<GameSession>().ProcessPlayerDeath();
        }
    }
}
