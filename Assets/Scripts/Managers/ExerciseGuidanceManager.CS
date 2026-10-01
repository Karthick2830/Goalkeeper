using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ExerciseGuidanceManager : MonoBehaviour
{
    // =========================================================
    // REFERENCES
    // =========================================================

    [Header("References")]

    [SerializeField]
    private BallController ballController;

    [SerializeField]
    private Image guidanceImage;

    [SerializeField]
    private TMP_Text guidanceText;

    [SerializeField]
    private AudioSource guidanceAudioSource;


    // =========================================================
    // TIMING
    // =========================================================

    [Header("Timing")]

    [SerializeField]
    private float preparationDelay = 2f;


    // =========================================================
    // LEFT BALL GUIDANCE
    // =========================================================
    //
    // LEFT BALL
    //
    // Raise RIGHT hand
    // Bend LEFT
    //
    // =========================================================

    [Header("LEFT Ball Guidance")]

    [SerializeField]
    private Sprite raiseRightHandImage;

    [SerializeField]
    private Sprite bendLeftImage;

    [SerializeField]
    private AudioClip raiseRightHandVoice;

    [SerializeField]
    private AudioClip bendLeftVoice;


    // =========================================================
    // RIGHT BALL GUIDANCE
    // =========================================================
    //
    // RIGHT BALL
    //
    // Raise LEFT hand
    // Bend RIGHT
    //
    // =========================================================

    [Header("RIGHT Ball Guidance")]

    [SerializeField]
    private Sprite raiseLeftHandImage;

    [SerializeField]
    private Sprite bendRightImage;

    [SerializeField]
    private AudioClip raiseLeftHandVoice;

    [SerializeField]
    private AudioClip bendRightVoice;


    // =========================================================
    // TEXT
    // =========================================================

    [Header("Guidance Text")]

    [SerializeField]
    private string raiseRightHandText =
        "Get ready! Right hand up!";

    [SerializeField]
    private string bendLeftText =
        "Here it comes! Bend LEFT!";

    [SerializeField]
    private string raiseLeftHandText =
        "Get ready! Left hand up!";

    [SerializeField]
    private string bendRightText =
        "Here it comes! Bend RIGHT!";


    // =========================================================
    // STATE
    // =========================================================

    private string currentShotDirection =
        "NONE";

    private Coroutine guidanceCoroutine;

    // Guidance is disabled until UIManager explicitly
    // starts the gameplay guidance.
    private bool guidanceActive = false;


    // =========================================================
    // UNITY
    // =========================================================

    private void Awake()
    {
        if (ballController == null)
        {
            ballController =
                FindFirstObjectByType<BallController>();
        }

        // Make sure guidance is initially hidden.
        HideGuidanceVisuals();
    }


    private void OnEnable()
    {
        if (ballController != null)
        {
            ballController.onShotPreparing +=
                HandleShotPreparing;

            ballController.onBallKick +=
                HandleBallKick;
        }
    }


    private void OnDisable()
    {
        if (ballController != null)
        {
            ballController.onShotPreparing -=
                HandleShotPreparing;

            ballController.onBallKick -=
                HandleBallKick;
        }

        StopGuidanceCoroutine();
    }


    // =========================================================
    // START GUIDANCE
    // =========================================================
    //
    // UIManager calls this when the actual gameplay begins.
    //
    // =========================================================

    public void StartGuidance()
    {
        guidanceActive = true;

        StopGuidanceCoroutine();

        currentShotDirection =
            "NONE";

        // Clear anything left from a previous game.
        HideGuidanceVisuals();
    }


    // =========================================================
    // STOP GUIDANCE
    // =========================================================
    //
    // Useful when:
    // - Game over
    // - Win
    // - Returning to menu
    // - Restarting
    //
    // =========================================================

    public void StopGuidance()
    {
        guidanceActive = false;

        StopGuidanceCoroutine();

        currentShotDirection =
            "NONE";

        HideGuidanceVisuals();
    }


    // =========================================================
    // SHOT PREPARING
    // =========================================================

    private void HandleShotPreparing(
        string direction)
    {
        // -----------------------------------------------------
        // IGNORE SHOTS BEFORE GAMEPLAY STARTS
        // -----------------------------------------------------

        if (!guidanceActive)
            return;


        currentShotDirection =
            direction;


        // -----------------------------------------------------
        // STOP PREVIOUS GUIDANCE
        // -----------------------------------------------------

        StopGuidanceCoroutine();


        // -----------------------------------------------------
        // SHOW HAND PREPARATION
        // -----------------------------------------------------

        if (direction == "LEFT")
        {
            ShowGuidance(
                raiseRightHandImage,
                raiseRightHandText,
                raiseRightHandVoice
            );
        }
        else if (direction == "RIGHT")
        {
            ShowGuidance(
                raiseLeftHandImage,
                raiseLeftHandText,
                raiseLeftHandVoice
            );
        }


        // -----------------------------------------------------
        // START PREPARATION TIMER
        // -----------------------------------------------------

        guidanceCoroutine =
            StartCoroutine(
                PrepareNextShot()
            );
    }


    // =========================================================
    // PREPARE NEXT SHOT
    // =========================================================

    private IEnumerator PrepareNextShot()
    {
        // -----------------------------------------------------
        // WAIT FOR PLAYER TO PREPARE
        // -----------------------------------------------------

        yield return new WaitForSeconds(
            preparationDelay
        );


        // -----------------------------------------------------
        // SAFETY CHECK
        // -----------------------------------------------------

        if (!guidanceActive ||
            ballController == null)
        {
            guidanceCoroutine = null;
            yield break;
        }


        // -----------------------------------------------------
        // SHOW BEND GUIDANCE
        // -----------------------------------------------------
        //
        // IMPORTANT:
        //
        // Bend instruction appears BEFORE
        // kicker animation starts.
        //
        // -----------------------------------------------------

        if (currentShotDirection == "LEFT")
        {
            ShowGuidance(
                bendLeftImage,
                bendLeftText,
                bendLeftVoice
            );
        }
        else if (currentShotDirection == "RIGHT")
        {
            ShowGuidance(
                bendRightImage,
                bendRightText,
                bendRightVoice
            );
        }


        // -----------------------------------------------------
        // START KICKER + BALL SEQUENCE
        // -----------------------------------------------------
        //
        // BallController starts the kicker animation here.
        //
        // It preserves the existing 2.1 second delay
        // between kicker animation and actual ball launch.
        //
        // -----------------------------------------------------

        ballController.ExecutePreparedShot();


        guidanceCoroutine = null;
    }


    // =========================================================
    // BALL KICKED
    // =========================================================

    private void HandleBallKick()
    {
        if (!guidanceActive)
            return;

        // The bend guidance is already displayed.
        //
        // Nothing needs to be changed here.
        //
        // This event remains available for future
        // synchronization if required.
    }


    // =========================================================
    // SHOW GUIDANCE
    // =========================================================

    private void ShowGuidance(
        Sprite image,
        string text,
        AudioClip voice)
    {
        // -----------------------------------------------------
        // IMAGE
        // -----------------------------------------------------

        if (guidanceImage != null)
        {
            guidanceImage.sprite =
                image;

            guidanceImage.enabled =
                image != null;
        }


        // -----------------------------------------------------
        // TEXT
        // -----------------------------------------------------

        if (guidanceText != null)
        {
            guidanceText.text =
                text;

            guidanceText.enabled =
                !string.IsNullOrEmpty(text);
        }


        // -----------------------------------------------------
        // VOICE
        // -----------------------------------------------------

        PlayVoice(voice);
    }


    // =========================================================
    // VOICE
    // =========================================================

    private void PlayVoice(
        AudioClip clip)
    {
        if (guidanceAudioSource == null)
            return;

        if (clip == null)
            return;


        // Stop previous guidance voice.
        guidanceAudioSource.Stop();


        // Play ElevenLabs guidance.
        guidanceAudioSource.PlayOneShot(
            clip
        );
    }


    // =========================================================
    // HIDE GUIDANCE VISUALS
    // =========================================================

    private void HideGuidanceVisuals()
    {
        if (guidanceImage != null)
        {
            guidanceImage.enabled =
                false;
        }

        if (guidanceText != null)
        {
            guidanceText.text =
                "";

            guidanceText.enabled =
                false;
        }

        if (guidanceAudioSource != null)
        {
            guidanceAudioSource.Stop();
        }
    }


    // =========================================================
    // STOP GUIDANCE COROUTINE
    // =========================================================

    private void StopGuidanceCoroutine()
    {
        if (guidanceCoroutine != null)
        {
            StopCoroutine(
                guidanceCoroutine
            );

            guidanceCoroutine = null;
        }
    }


    // =========================================================
    // CLEAR GUIDANCE
    // =========================================================

    public void ClearGuidance()
    {
        StopGuidance();

        guidanceActive = false;
    }
}