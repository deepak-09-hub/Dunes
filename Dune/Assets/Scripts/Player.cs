using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class Player : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float acceleration = 1f;
    [SerializeField] private float maxSpeed = 45f;
    [SerializeField] private float groundDeceleration = 12f;
    [SerializeField] private float airDeceleration = 2f;

    [Header("Air")]
    [SerializeField] private float airControl = 2f;

    [Header("Dive")]
    [SerializeField] private float diveForce = 20f;
    [SerializeField] private float diveTorque = -80f;

    [Header("Physics")]
    [SerializeField] private float gravity = 2f;

    [Header("Landing")]
    [SerializeField] private float maxImpactSpeed = 14f;
    [SerializeField] private float maxAngleDifference = 55f;

    [Header("Ground Detection")]
    [SerializeField] private LayerMask ground;
    [SerializeField] private float groundCheckRadius = 0.18f;
    [SerializeField] private float groundCheckDistance = 0.3f;

    private Rigidbody2D rb;

    private bool holding;
    private bool grounded;
    private bool wasGrounded;
    private bool stopped;

    // Score line:
    // false = player is allowed to score on the next upward crossing.
    // true  = player has already scored and must cross back down first.
    private bool scoreLinePassed;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        rb.gravityScale = gravity;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
    }

    private void Update()
    {
        holding =
            Input.GetMouseButton(0) ||
            Input.touchCount > 0 ||
            Input.GetKey(KeyCode.Space);
    }

    private void FixedUpdate()
    {
        if (stopped)
            return;

        CheckGround();

        if (grounded)
            GroundMovement();
        else
            AirMovement();

        LimitSpeed();
    }

    // --------------------------------------------------
    // GROUND
    // --------------------------------------------------

    private void GroundMovement()
    {
        if (holding)
        {
            rb.AddForce(
                Vector2.right * acceleration,
                ForceMode2D.Force
            );
        }
        else
        {
            // Lose horizontal speed when released.
            float newSpeed = Mathf.MoveTowards(
                rb.velocity.x,
                0f,
                groundDeceleration * Time.fixedDeltaTime
            );

            rb.velocity = new Vector2(
                newSpeed,
                rb.velocity.y
            );
        }
    }

    // --------------------------------------------------
    // AIR
    // --------------------------------------------------

    private void AirMovement()
    {
        // Small amount of horizontal control in air.
        if (holding)
        {
            rb.AddForce(
                Vector2.right * airControl,
                ForceMode2D.Force
            );

            // Holding in air = dive.
            rb.AddForce(
                Vector2.down * diveForce,
                ForceMode2D.Force
            );

            rb.AddTorque(
                diveTorque,
                ForceMode2D.Force
            );
        }
        else
        {
            // Slight air resistance.
            float newSpeed = Mathf.MoveTowards(
                rb.velocity.x,
                0f,
                airDeceleration * Time.fixedDeltaTime
            );

            rb.velocity = new Vector2(
                newSpeed,
                rb.velocity.y
            );
        }
    }

    // --------------------------------------------------
    // GROUND CHECK
    // --------------------------------------------------

    private void CheckGround()
    {
        wasGrounded = grounded;

        RaycastHit2D hit = Physics2D.CircleCast(
            transform.position,
            groundCheckRadius,
            Vector2.down,
            groundCheckDistance,
            ground
        );

        grounded = hit.collider != null;

        // We just touched the ground after being airborne.
        if (grounded && !wasGrounded)
        {
            CheckLanding(hit.normal);
        }
    }

    // --------------------------------------------------
    // LANDING
    // --------------------------------------------------

    private void CheckLanding(Vector2 surfaceNormal)
    {
        Vector2 velocity = rb.velocity;

        // Current impact speed calculation.
        float impactSpeed = velocity.magnitude;

        if (impactSpeed <= 0f)
            return;

        // Direction along the dune surface.
        Vector2 slopeDirection = new Vector2(
            surfaceNormal.y,
            -surfaceNormal.x
        ).normalized;

        // Always choose the forward-facing slope direction.
        if (slopeDirection.x < 0f)
            slopeDirection = -slopeDirection;

        // Direction the player is travelling when hitting.
        Vector2 landingDirection = velocity.normalized;

        float angleDifference = Vector2.Angle(
            landingDirection,
            slopeDirection
        );

        Debug.Log(
            "LANDING\n" +
            "Impact Speed: " + impactSpeed.ToString("0.00") + "\n" +
            "Landing Angle: " + angleDifference.ToString("0.0") + "°"
        );

        // -----------------------------
        // CRASH CONDITIONS
        // -----------------------------

        if (impactSpeed > maxImpactSpeed)
        {
            GameManager.I?.Crash(
                "HARD LANDING\nImpact: " +
                impactSpeed.ToString("0.0")
            );

            return;
        }

        if (angleDifference > maxAngleDifference)
        {
            GameManager.I?.Crash(
                "BAD LANDING\nAngle: " +
                angleDifference.ToString("0.0") + "°"
            );

            return;
        }

        // Successful landing does NOT give score anymore.
        // Score is now handled by the score-line trigger.
    }

    // --------------------------------------------------
    // SCORE LINE
    // --------------------------------------------------

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (stopped)
            return;

        // Make sure this is the score line.
        if (!other.CompareTag("ScoreLine"))
            return;

        // Moving upward through the line.
        if (rb.velocity.y > 0f)
        {
            // Only score if we haven't already scored
            // during this crossing cycle.
            if (!scoreLinePassed)
            {
                int reward = GameManager.I != null
                    ? GameManager.I.GetScoreLineReward()
                    : 10;

                GameManager.I?.AddScore(reward);

                scoreLinePassed = true;

                Debug.Log(
                    "SCORE LINE CROSSED UPWARD +" +
                    reward
                );
            }
        }
        // Moving downward through the line.
        else if (rb.velocity.y < 0f)
        {
            // Re-arm the score so the next upward crossing
            // can score again.
            scoreLinePassed = false;

            Debug.Log("SCORE LINE RE-ARMED");
        }
    }

    // --------------------------------------------------
    // COLLISION BACKUP
    // --------------------------------------------------

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (stopped)
            return;

        // Make sure this is actually ground.
        if (((1 << collision.gameObject.layer) & ground.value) == 0)
            return;

        if (collision.contactCount == 0)
            return;

        // If the ground check missed the transition,
        // collision detection still catches the landing.
        if (!wasGrounded)
        {
            ContactPoint2D contact = collision.GetContact(0);

            CheckLanding(contact.normal);
        }
    }

    // --------------------------------------------------
    // SPEED
    // --------------------------------------------------

    private void LimitSpeed()
    {
        Vector2 velocity = rb.velocity;

        velocity.x = Mathf.Clamp(
            velocity.x,
            0f,
            maxSpeed
        );

        rb.velocity = velocity;
    }

    // --------------------------------------------------
    // GAME OVER
    // --------------------------------------------------

    public void StopPlayer()
    {
        stopped = true;

        rb.velocity = Vector2.zero;
        rb.angularVelocity = 0f;

        rb.bodyType = RigidbodyType2D.Kinematic;
    }
}
