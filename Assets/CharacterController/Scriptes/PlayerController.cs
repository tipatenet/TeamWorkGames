using System;
using UnityEngine;


//PlayerController

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed;

    public float groundDrag;

    public float jumpForce;
    public float jumpCooldown;
    public float airMultiplier;
    bool readyToJump;

    [Header("Keybinds")]
    public KeyCode jumpKey = KeyCode.Space;

    [Header("Ground Check")]
    public float playerHeight;
    public LayerMask Ground;
    [SerializeField]
    bool Grounded;

    [Header("Slope Handling")]
    public float maxSlopAngle;
    private RaycastHit slopeHit;
    private bool exitingSlope;

    public Transform orientation;

    float horizontalInput;
    float verticalInput;

    Vector3 moveDirection;

    Rigidbody rb;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
        readyToJump = true;
    }

    private void Update()
    {
        //ground Check
        Grounded = Physics.Raycast(transform.position, Vector3.down, playerHeight * 0.5f +0.2f,Ground);

        MyInput();
        SpeedControl();

        //Drag
        if (Grounded)
        {
            rb.linearDamping = groundDrag;
        }
        else
        {
            rb.linearDamping = 0;
        }
    }

    private void FixedUpdate()
    {
        MovePlayer();
    }

    //Fonction pour gérer les Input et récup les valeurs des vecteurs
    private void MyInput()
    {
        Vector2 input = PlayerInputHandler.Instance.MoveInput;
        horizontalInput = input.x;
        verticalInput = input.y;

        if (PlayerInputHandler.Instance.JumpPressed && readyToJump && Grounded)
        {
            readyToJump = false;
            Jump();
            Invoke(nameof(resetJump), jumpCooldown);
        }
    }

    //Fonction pour gérer les movement 
    private void MovePlayer()
    {
        //Movement Direction :
        moveDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;

        //on slope
        if (OnSlope() && !exitingSlope)
        {
            rb.AddForce(GetSlopeMoveDirection() * moveSpeed * 20f, ForceMode.Force);

            if(rb.linearVelocity.y > 0)
            {
                rb.AddForce(Vector3.down * 80f, ForceMode.Force);
            }
        }

        //on ground
        else if (Grounded) {
            rb.AddForce(moveDirection * moveSpeed * 10f, ForceMode.Force);
        }

        //in air
        else if (!Grounded)
        {
            rb.AddForce(moveDirection * moveSpeed * 10f * airMultiplier, ForceMode.Force);

        }

        rb.useGravity = !OnSlope();
    }

    //Fonction pour éviter que la vitesse max soit dépasser

    private void SpeedControl()
    {
        //limit speed on slopes
        if (OnSlope() && !exitingSlope)
        {
            if(rb.linearVelocity.magnitude > moveSpeed)
            {
                rb.linearVelocity = rb.linearVelocity.normalized * moveSpeed;
            }
        }
        //limit speed on ground and air

        else
        {
            Vector3 flatVel = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);

            //limit speed
            if (flatVel.magnitude > moveSpeed)
            {
                Vector3 limitedVel = flatVel.normalized * moveSpeed;
                rb.linearVelocity = new Vector3(limitedVel.x, rb.linearVelocity.y, limitedVel.z);
            }
        }

    }

    //Fonction pour gerer le saut

    private void Jump()
    {
        exitingSlope = true;

        //reset y velocity
        rb.linearVelocity = new Vector3(rb.linearVelocity.x,0f,rb.linearVelocity.z);

        rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);
    }

    private void resetJump()
    {
        readyToJump = true;

        exitingSlope = false;
    }


    //Fonction pour gerer les mouvements sur les ramps
    private bool OnSlope()
    {
        if (Physics.Raycast(transform.position, Vector3.down, out slopeHit, playerHeight * 0.5f + 0.3f))
        {
            float angle = Vector3.Angle(Vector3.up, slopeHit.normal);
            return angle < maxSlopAngle && angle != 0;
        }
        return false;
    }

    private Vector3 GetSlopeMoveDirection()
    {
        return Vector3.ProjectOnPlane(moveDirection, slopeHit.normal).normalized;
    }
}
