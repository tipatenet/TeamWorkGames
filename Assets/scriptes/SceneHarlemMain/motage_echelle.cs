using UnityEngine;

public class motage_echelle : MonoBehaviour
{
    public float climbSpeed = 3f;

    private Rigidbody rb;
    private PlayerController playerController;
    private bool onLadder = false;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        playerController = GetComponent<PlayerController>();
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Ladder"))
        {
            onLadder = true;
            playerController.enabled = false;
            rb.useGravity = false;
            rb.linearVelocity = Vector3.zero;
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Ladder"))
        {
            onLadder = false;
            playerController.enabled = true;
            rb.useGravity = true;
        }
    }

    void FixedUpdate()
    {
        if (onLadder)
        {
            Vector2 input = PlayerInputHandler.Instance.MoveInput;
            Vector3 velocity = rb.linearVelocity;
            velocity.y = input.y * climbSpeed;
            rb.linearVelocity = velocity;
        }
    }
}