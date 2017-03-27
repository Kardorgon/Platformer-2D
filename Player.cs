using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Controller2D))]
public class Player : MonoBehaviour
{

    public float maxJumpHeight = 4;
    public float minJumpHeight = 1;
    public float timeToJumpApex = 1f;
    float accelerationTimeAirborne = .2f;
    float accelerationTimeGrounded = .1f;
    float moveSpeed = 6;

    float gravity;
    float maxJumpvelocity;
    float minJumpVelocity;
    Vector3 velocity;
    float velocityXSmoothing;

    Controller2D controller;


    public float wallSlideSpeedMax = 3;
    public Vector2 wallJumpClimg;
    public Vector2 wallJumpOff;
    public Vector2 wallLeap;
    public float wallStickTime = 0.25f;
    float timeToWallUnstick;
    Vector2 directionalInput;
    bool wallSliding;
    int wallDirX;


    void Start()
    {
        controller = GetComponent<Controller2D>();

        gravity = -(2 * maxJumpHeight) / Mathf.Pow(timeToJumpApex, 2);
        maxJumpvelocity = Mathf.Abs(gravity) * timeToJumpApex;
        minJumpHeight = Mathf.Sqrt(2 * Mathf.Abs(gravity) * minJumpHeight);
        print("Gravity: " + gravity + "  Jump Velocity: " + maxJumpvelocity);
    }

    void Update()
    {
        CalculateVelocity();
        HandleWallSliding();
        wallDirX = (controller.collisions.left) ? -1 : 1;
        controller.Move(velocity * Time.deltaTime, directionalInput);

        //przenieslismy w E10(variable height jump) zeby meic wartosci po poruszeniu postaci
        if (controller.collisions.above || controller.collisions.below)
        {
            if (controller.collisions.slidingDownMaxSlope)
            {
                velocity.y += controller.collisions.slopeNormal.y * -gravity * Time.deltaTime;
            }else
            {
                velocity.y = 0;
            }
        }
    }

    public void SetDirectionalInput(Vector2 input)
    {
        directionalInput = input;
    }
    public void OnJumpInputDown()
    {
        //remodified for wallJumping
        /*if (Input.GetKeyDown(KeyCode.Space) && controller.collisions.below)
        {
            velocity.y = jumpVelocity;
        }*/
        if (wallSliding)
        {
            if (wallDirX == directionalInput.x)
            {
                velocity.x = -wallDirX * wallJumpClimg.x;
                velocity.y = wallJumpClimg.y;
            }
            else if (directionalInput.x == 0)
            {
                velocity.x = -wallDirX * wallJumpOff.x;
                velocity.y = wallJumpOff.y;
            }
            else
            {
                velocity.x = -wallDirX * wallLeap.x;
                velocity.y = wallLeap.y;
            }
        }
        if (controller.collisions.below)
        {
            if (controller.collisions.slidingDownMaxSlope)
            {
                //tutaj robimy handle of jumping from and to maxSlide slopes
                if(directionalInput.x != -Mathf.Sign(controller.collisions.slopeNormal.x)) //not jumping against max slope
                {
                    velocity.y = maxJumpvelocity * controller.collisions.slopeNormal.y;
                    velocity.x = maxJumpvelocity * controller.collisions.slopeNormal.x;
                }
            }else
            {
                velocity.y = maxJumpvelocity;
            }
        }
    }

    public void OnJumpInputUp()
    {
        if (velocity.y > minJumpVelocity)
        {
            velocity.y = minJumpVelocity;
        }
    }


    void HandleWallSliding()
    {
        //wall wall sliding - no ground, touching left or right wall, moving downwards
        wallSliding = false;
        if ((controller.collisions.left || controller.collisions.right) && !controller.collisions.below && velocity.y < 0)
        {
            wallSliding = true;

            if (velocity.y < -wallSlideSpeedMax)
            {
                velocity.y = -wallSlideSpeedMax;
            }
            if (timeToWallUnstick > 0)
            {
                velocityXSmoothing = 0;
                velocity.x = 0;
                if (directionalInput.x != wallDirX && directionalInput.x != 0)
                    timeToWallUnstick -= Time.deltaTime;

                else
                {
                    timeToWallUnstick = wallStickTime;
                }
            }
            else
            {
                timeToWallUnstick = wallStickTime;
            }
        }
    }
    void CalculateVelocity()
    {
        
        float targetVelocityX = directionalInput.x * moveSpeed;
        // now movement is smooth as silk thanks to this!
        velocity.x = Mathf.SmoothDamp(velocity.x, targetVelocityX, ref velocityXSmoothing, (controller.collisions.below) ? accelerationTimeGrounded : accelerationTimeAirborne);
        velocity.y += gravity * Time.deltaTime;
    }
}