using System;
using System.Collections;
using UnityEngine;

public class BallController : MonoBehaviour
{
    [Header("Ball Settings")]

    [SerializeField]
    private float forwardForce = 18f;

    [SerializeField]
    private float upwardForce = 10f;

    [SerializeField]
    private float startingZPos = 78f;

    [SerializeField]
    private float curveForce = 14f;

    [SerializeField]
    private float nextKickDelay = 1f;


    [Header("References")]

    [SerializeField]
    private GameObject kicker;

    [SerializeField]
    private Animator ballKickAnimator;

    [SerializeField]
    private PlayerMovement player;

    [SerializeField]
    private UIManager uiManager;


    private static readonly int KickHash =
        Animator.StringToHash("Kick");

    private static readonly int defeatHash =
        Animator.StringToHash("Defeat");


    // =========================================================
    // BALL LANES
    // =========================================================
    //
    // -10 = LEFT
    //  10 = RIGHT
    //
    // Sequence:
    //
    // LEFT
    // RIGHT
    // LEFT
    // RIGHT
    // =========================================================

    private readonly float[] lanes =
    {
        -10f,
        10f
    };


    private int nextLaneIndex = 0;


    // =========================================================
    // INTERNAL
    // =========================================================

    private Rigidbody rb;

    private bool isResetting;

    private Vector3 startPosition;

    private Vector3 kickerStartPosition;
    private Quaternion kickerStartRotation;
    private Vector3 kickerStartScale;

    private Vector3 direction;

    private Coroutine activeCoroutine;

    private bool isGameOver;

    private bool _pendingSave;

    private bool shouldCurve = false;


    // =========================================================
    // PUBLIC
    // =========================================================

    public float laneX;

    public float curveDirection;


    // Called when the ball actually starts moving.
    public Action onBallKick;


    // Called BEFORE the kicker animation starts.
    //
    // Sends:
    // "LEFT"
    // "RIGHT"
    //
    // The ExerciseGuidanceManager uses this to tell the
    // player which hand to raise.
    public Action<string> onShotPreparing;


    // =========================================================
    // START
    // =========================================================

    void Start()
    {
        rb = GetComponent<Rigidbody>();


        startPosition = new Vector3(
            0f,
            transform.position.y,
            startingZPos
        );


        if (kicker != null)
        {
            kickerStartPosition =
                kicker.transform.position;

            kickerStartRotation =
                kicker.transform.localRotation;

            kickerStartScale =
                kicker.transform.localScale;
        }


        transform.position =
            startPosition;


        // First shot = LEFT.
        nextLaneIndex = 0;


        activeCoroutine =
            StartCoroutine(BallWait());
    }


    // =========================================================
    // START BALL
    // =========================================================

    public void StartBall()
    {
        if (isGameOver ||
            activeCoroutine != null)
            return;


        activeCoroutine =
            StartCoroutine(BallWait());
    }


    // =========================================================
    // RESET GAME
    // =========================================================

    public void ResetGame()
    {
        if (activeCoroutine != null)
        {
            StopCoroutine(activeCoroutine);
            activeCoroutine = null;
        }


        isGameOver = false;

        isResetting = false;

        _pendingSave = false;


        // Start sequence from LEFT.
        nextLaneIndex = 0;


        rb.linearVelocity =
            Vector3.zero;

        rb.angularVelocity =
            Vector3.zero;


        transform.position =
            startPosition;

        transform.localScale =
            Vector3.one;


        if (kicker != null)
        {
            kicker.SetActive(true);

            kicker.transform.position =
                kickerStartPosition;

            kicker.transform.localRotation =
                kickerStartRotation;

            kicker.transform.localScale =
                kickerStartScale;
        }


        activeCoroutine =
            StartCoroutine(BallWait());
    }


    // =========================================================
    // SAVE / GOAL
    // =========================================================

    public void RegisterSave()
    {
        _pendingSave = true;
    }


    public void RegisterGoal()
    {
        _pendingSave = false;
    }


    // =========================================================
    // GET UPCOMING SIDE
    // =========================================================

    private string GetUpcomingShotDirection()
    {
        if (lanes[nextLaneIndex] < 0f)
            return "LEFT";

        return "RIGHT";
    }


    // =========================================================
    // SHOOT
    // =========================================================

    void Shoot()
    {
        if (isGameOver)
            return;


        // -----------------------------------------------------
        // SELECT SIDE
        // -----------------------------------------------------

        laneX =
            lanes[nextLaneIndex];


        // Prepare next side.
        nextLaneIndex++;

        if (nextLaneIndex >= lanes.Length)
        {
            nextLaneIndex = 0;
        }


        // -----------------------------------------------------
        // BALL STARTED
        // -----------------------------------------------------

        // This event is used by the exercise guidance system
        // to change from:
        //
        // "Raise your hand"
        //
        // to:
        //
        // "Bend LEFT/RIGHT"
        //
        onBallKick?.Invoke();


        rb.useGravity = true;


        VFXManager.instance.PlayTrailEffect();


        player?.ResetSaveGuard();


        // Tell goalkeeper which lane the ball is targeting.
        player?.CanCatch();


        // -----------------------------------------------------
        // CURVE
        // -----------------------------------------------------

        shouldCurve =
            Mathf.Abs(laneX) == 10f;


        curveDirection =
            Mathf.Sign(laneX);


        // -----------------------------------------------------
        // SHOOT
        // -----------------------------------------------------

        if (shouldCurve)
        {
            float wideAimX =
                laneX +
                (curveDirection * 5f);


            direction =
                new Vector3(
                    wideAimX -
                    transform.position.x,

                    0f,

                    27f
                ).normalized;


            rb.AddForce(
                direction *
                forwardForce,

                ForceMode.Impulse
            );


            rb.AddForce(
                Vector3.up *
                (upwardForce - 1f),

                ForceMode.Impulse
            );
        }
        else
        {
            direction =
                new Vector3(
                    laneX -
                    transform.position.x,

                    0f,

                    27f
                ).normalized;


            rb.AddForce(
                direction *
                forwardForce,

                ForceMode.Impulse
            );


            rb.AddForce(
                Vector3.up *
                upwardForce,

                ForceMode.Impulse
            );
        }


        AudioManager.instance.PlayKick();
    }


    // =========================================================
    // CURVE PHYSICS
    // =========================================================

    private void FixedUpdate()
    {
        if (shouldCurve)
        {
            rb.AddForce(
                Vector3.right *
                -curveDirection *
                curveForce,

                ForceMode.Force
            );
        }
    }


    // =========================================================
    // STOP BALL
    // =========================================================

    public void StopBall()
    {
        isGameOver = true;


        if (activeCoroutine != null)
        {
            StopCoroutine(activeCoroutine);
            activeCoroutine = null;
        }


        rb.linearVelocity =
            Vector3.zero;

        rb.angularVelocity =
            Vector3.zero;
    }


    // =========================================================
    // BALL WAIT
    // =========================================================

    IEnumerator BallWait()
    {
        // -----------------------------------------------------
        // DETERMINE UPCOMING SIDE
        // -----------------------------------------------------

        string upcomingDirection =
            GetUpcomingShotDirection();


        // -----------------------------------------------------
        // EXERCISE GUIDANCE
        // -----------------------------------------------------
        //
        // This happens BEFORE the kicker starts kicking.
        //
        // LEFT ball:
        //     Raise RIGHT hand
        //
        // RIGHT ball:
        //     Raise LEFT hand
        // -----------------------------------------------------

        onShotPreparing?.Invoke(
            upcomingDirection
        );


        // -----------------------------------------------------
        // RESET KICKER
        // -----------------------------------------------------

        if (kicker != null)
        {
            kicker.transform.position =
                kickerStartPosition;

            kicker.transform.localRotation =
                kickerStartRotation;
        }


        // -----------------------------------------------------
        // KICK ANIMATION
        // -----------------------------------------------------

        if (ballKickAnimator != null)
        {
            ballKickAnimator.Play(
                "Kick",
                0,
                0f
            );


            ballKickAnimator.SetTrigger(
                KickHash
            );
        }


        // -----------------------------------------------------
        // WAIT FOR KICK
        // -----------------------------------------------------

        yield return new WaitForSeconds(
            2.1f
        );


        if (!isGameOver)
        {
            Shoot();
        }
    }


    // =========================================================
    // COLLISION
    // =========================================================

    void OnCollisionEnter(Collision collision)
    {
        if (isResetting)
            return;


        if (collision.gameObject.CompareTag("Player"))
        {
            AudioManager.instance
                .PlayBallHitImpact();


            ballKickAnimator?.SetTrigger(
                defeatHash
            );


            if (player != null &&
                !player.canHeader)
            {
                rb.AddForce(
                    direction * -5f,

                    ForceMode.Impulse
                );
            }
            else
            {
                rb.AddForce(
                    direction * -10f,

                    ForceMode.Impulse
                );
            }


            TriggerReset();
        }
    }


    // =========================================================
    // TRIGGER RESET
    // =========================================================

    public void TriggerReset()
    {
        if (isResetting)
            return;


        isResetting = true;


        if (activeCoroutine != null)
        {
            StopCoroutine(activeCoroutine);
            activeCoroutine = null;
        }


        activeCoroutine =
            StartCoroutine(
                ResetBall()
            );
    }


    // =========================================================
    // RESET BALL
    // =========================================================

    private IEnumerator ResetBall()
    {
        // Stop curve force.
        shouldCurve = false;


        // Wait after collision.
        yield return new WaitForSeconds(
            1.3f
        );


        // -----------------------------------------------------
        // SCORE
        // -----------------------------------------------------

        if (_pendingSave)
        {
            _pendingSave = false;

            uiManager?.ScoreIncrease();
        }


        // -----------------------------------------------------
        // RESET PHYSICS
        // -----------------------------------------------------

        rb.linearVelocity =
            Vector3.zero;

        rb.angularVelocity =
            Vector3.zero;


        // -----------------------------------------------------
        // RESET KICKER
        // -----------------------------------------------------

        if (kicker != null)
        {
            kicker.transform.position =
                kickerStartPosition;

            kicker.transform.localRotation =
                kickerStartRotation;
        }


        // -----------------------------------------------------
        // RESET BALL
        // -----------------------------------------------------

        transform.position =
            startPosition;


        isResetting = false;


        // -----------------------------------------------------
        // WAIT BEFORE NEXT KICK
        // -----------------------------------------------------

        yield return new WaitForSeconds(
            nextKickDelay
        );


        // -----------------------------------------------------
        // NEXT SHOT
        // -----------------------------------------------------

        if (!isGameOver)
        {
            activeCoroutine =
                StartCoroutine(
                    BallWait()
                );
        }
        else
        {
            activeCoroutine = null;
        }
    }
}