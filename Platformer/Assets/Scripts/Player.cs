using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour

{   //Objects needed for mouse aiming.
    public Transform targetTransform;
    public LayerMask mouseAimMask;

    //Settings for movement speed and jump height are located in the unity player object script settings.
    //Changing settings here will not have an effect.
    public float walkSpeed = 5.5f;
    public float jumpHeight = 5.5f;
    public float railSpeed = 30f;

    //Objects used to check player is grounded.
    public Transform groundCheckTransform;
    public float groundCheckRadius = 0.2f;
    public LayerMask groundMask;  //Calls Ground layer. All objects player can jump on need to be in the ground layer.
    private bool isGrounded;

    public Transform wallCheckRightTransform;
    public Transform wallCheckLeftTransform;
    public float wallCheckRadius;
    public LayerMask wallMask;
    private bool isWallRiding, canWallRideRight, canWallRideLeft;

    public Transform railCheck;
    public Transform railEndPoint;
    public Transform railEndCheck;
    public float railCheckRadius, railEndCheckRadius;
    public LayerMask railMask, railEndMask;
    private bool isRailRiding, canRailRide, railEndReached;


    //Calls bullet object & sets origin point to empty object muzzle attached to the barrel of the gun.
    public GameObject bulletPrefab;
    public Transform muzzleTransform;

    //Settings for recoil animation & body parts effected
    public AnimationCurve recoilCurve;
    public float recoilDuration = 0.25f;
    public float recoilMaxRotation = 45f;
    private float recoilTimer;
    public Transform rightLowerArm;
    public Transform rightHand;

    //Character controls 
    private bool jumpKeyWasPressed;
    private bool wallRideKeyWasPressed;
    private bool railRideKeyWasPressed;
    private bool railRideKeyWasReleased;
    private bool railJumpKeyWasPressed;
    private float inputMovement;
    private Rigidbody rigidBody;

    //Calls Camera and Animator needed for procedural animations such as look aim and recoil.
    private Animator animator;
    private Camera mainCamera;
    
    

    private int FacingSign
    {
        //Checks character facing direction.
        get
        {
            Vector3 perp = Vector3.Cross(transform.forward, Vector3.forward);
            float dir = Vector3.Dot(perp, transform.up);
            return dir > 0f ? -1 : dir < 0f ? 1 : 0;   
        }
            
    }
    

    // Start is called before the first frame update.
    void Start()
    {
        rigidBody = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();
        mainCamera = Camera.main;
    }

    // Update is called once per frame rendered (FPS rendered not displayed).
    void Update()
    {
        //Checks if player is on the ground prior to jumping. Can later implement wallride check.
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded && !isRailRiding) 
        {
            jumpKeyWasPressed = true;
        }

        if (Input.GetKeyDown(KeyCode.Space) && (canWallRideRight || canWallRideLeft))
        {
            wallRideKeyWasPressed = true;
        }

        if (Input.GetKeyDown(KeyCode.Space) && isRailRiding)
        {
            railJumpKeyWasPressed = true;
        }
        
        if (Input.GetKey (KeyCode.Mouse1) && canRailRide )
        {
            railRideKeyWasPressed = true;
        }

       if (Input.GetKeyUp (KeyCode.Mouse1) && isRailRiding)
        {
            railRideKeyWasReleased = true;
        }

        inputMovement = Input.GetAxis("Horizontal");

        //Facing with mouse aim
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, Mathf.Infinity, mouseAimMask))

        {
            targetTransform.position = hit.point;
        }
        
        if (Input.GetButtonDown("Fire1"))
        {
            Fire();
        }
    }

    private void Fire()
    {
        recoilTimer = Time.time;

        var go = Instantiate(bulletPrefab);
        go.transform.position = muzzleTransform.position;
        var bullet = go.GetComponent<Bullet>();
        bullet.Fire(go.transform.position, muzzleTransform.eulerAngles, gameObject.layer);
    }
    private void LateUpdate()
    {
        //recoil animation
        if (recoilTimer < 0)
        {
            return;
        }
        float curveTime = (Time.time - recoilTimer) / recoilDuration;
        if (curveTime > 1f)
        {
            recoilTimer = -1;
        }
        else
        {
            rightLowerArm.Rotate(Vector3.forward, recoilCurve.Evaluate(curveTime) * recoilMaxRotation, Space.Self);
        }
    }
        //Fixed update refreshes on physics engine refreshrate at a locked 100Hz.
    private void FixedUpdate()
    {
        if (jumpKeyWasPressed)
        {
            //animator.SetTrigger("takeOff");
            //animator.SetBool("isJumping", true);
            rigidBody.velocity = new Vector3(rigidBody.velocity.x, 0, 0);
            rigidBody.AddForce(Vector3.up * Mathf.Sqrt(jumpHeight * -1 * Physics.gravity.y), ForceMode.VelocityChange);
            jumpKeyWasPressed = false;
        }

        

        //Movement System.
        rigidBody.velocity = new Vector3(inputMovement * walkSpeed, rigidBody.velocity.y, 0);  
        animator.SetFloat("speed", (FacingSign * rigidBody.velocity.x) / walkSpeed);

        //Facing direction.
        rigidBody.MoveRotation(Quaternion.Euler(new Vector3( 0, 90 * Mathf.Sign(targetTransform.position.x - transform.position.x), 0)));

        //Ground Check.
        isGrounded = Physics.CheckSphere(groundCheckTransform.position, groundCheckRadius, groundMask, QueryTriggerInteraction.Ignore);
        animator.SetBool("isGrounded", isGrounded);

        //Wall Check for left of player. Need to find way to make canWallRide = both sides to fix bug.
        canWallRideLeft = Physics.CheckSphere(wallCheckLeftTransform.position, wallCheckRadius, wallMask, QueryTriggerInteraction.Ignore);
        canWallRideRight = Physics.CheckSphere(wallCheckRightTransform.position, wallCheckRadius, wallMask, QueryTriggerInteraction.Ignore);
        //animator.SetBool("isWallRiding", isWallRiding);  Implement once wallride animation is created.
        //dont forget to Create bool in Animator with proper transitions.


        //Rail Check
        canRailRide = Physics.CheckSphere(railCheck.position, railCheckRadius, railMask, QueryTriggerInteraction.Collide);
        railEndReached = Physics.CheckSphere(railEndCheck.position, railEndCheckRadius, railEndMask, QueryTriggerInteraction.Collide);
        //animator.SetBool("isRailRiding", isRailRiding); Implement once RailRiding animation is created.
        // dont forget to Create bool in Animator with proper transitions.
        
      
        if (wallRideKeyWasPressed && !isGrounded && (canWallRideLeft || canWallRideRight))
        {
            
            isWallRiding = true;
            if (isWallRiding)
            {
                //Same as Jump for now
                rigidBody.velocity = new Vector3(rigidBody.velocity.x, 0, 0);
                rigidBody.AddForce(Vector3.up * Mathf.Sqrt(jumpHeight * -1 * Physics.gravity.y - 10), ForceMode.VelocityChange);
                //animator.SetTrigger("takeOff");
                //animator.SetBool("isJumping", true);
            }
            wallRideKeyWasPressed = false;

        }

        if (canRailRide && railRideKeyWasPressed)
        {
            isRailRiding = true;
            //animator.SetBool("isJumping", false);
            //animator.SetBool("isRailRiding", true);
            if (isRailRiding)
            {


                //rigidBody.velocity = new Vector3(rigidBody.velocity.x, 0, 0);
                //rigidBody.AddForce(Vector3.MoveTowards(rigidBody.position, railEndPoint.position , railSpeed * Time.deltaTime), ForceMode.VelocityChange);
                rigidBody.isKinematic = true;
            }

            else


                rigidBody.isKinematic = false;

                if (railJumpKeyWasPressed)
                {
                    //animator.SetTrigger("takeOff");
                    //animator.SetBool("isJumping", true);
                    isRailRiding = false;
                    rigidBody.isKinematic = false;
                    rigidBody.velocity = new Vector3(rigidBody.velocity.x, 0, 0);
                    rigidBody.AddForce(Vector3.up * Mathf.Sqrt(jumpHeight  * 3 * -1 * Physics.gravity.y), ForceMode.VelocityChange);


                    
                }
            railJumpKeyWasPressed = false;


            if (railRideKeyWasReleased)
                {
                    railRideKeyWasPressed = false;
                    rigidBody.isKinematic = false;
                    isRailRiding = false;


                    // if (rigidBody.velocity <= 1f)
                    // {

                    //}


                }

                if (railEndReached)
                {
                    railRideKeyWasPressed = false;
                    rigidBody.isKinematic = false;
                    isRailRiding = false;

                }
            

            




            }
           
            
            
            
        

        
    }

    private void OnAnimatorIK()
    {
        //Weapon Aim at Target IK.
        animator.SetIKPositionWeight(AvatarIKGoal.RightHand, 1);  //Takes control of right hand Weight of 1 is 100%.
        animator.SetIKPosition(AvatarIKGoal.RightHand, targetTransform.position); //Aims it at cursor location.

        //Look at Target IK.
        animator.SetLookAtWeight(1);
        animator.SetLookAtPosition(targetTransform.position);  //Head Tracks Mouse Pointer.
    }
}
