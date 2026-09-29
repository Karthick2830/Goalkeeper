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

        if (guidanceCoroutine != null)
        {
            StopCoroutine(guidanceCoroutine);
            guidanceCoroutine = null;
        }
    }


    // =========================================================
    // SHOT PREPARING
    // =========================================================

    private void HandleShotPreparing(
        string direction)
    {
        currentShotDirection =
            direction;


        // -----------------------------------------------------
        // STOP PREVIOUS GUIDANCE
        // -----------------------------------------------------

        if (guidanceCoroutine != null)
        {
            StopCoroutine(guidanceCoroutine);
            guidanceCoroutine = null;
        }


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

        if (ballController == null)
        {
            guidanceCoroutine = null;
            yield break;
        }


        // -----------------------------------------------------
        // SHOW BEND GUIDANCE FIRST
        // -----------------------------------------------------
        //
        // IMPORTANT:
        //
        // The bend instruction appears BEFORE
        // the kicker animation starts.
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
        // NOW START KICKER + KICK SEQUENCE
        // -----------------------------------------------------
        //
        // BallController starts the kicker animation here.
        //
        // The existing 2.1 second animation-to-ball timing
        // is preserved.
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
        // The bend guidance is already visible.
        //
        // Nothing needs to change here.
        //
        // This event remains available for any future
        // guidance/gameplay synchronization.
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


        guidanceAudioSource.Stop();


        guidanceAudioSource.PlayOneShot(
            clip
        );
    }


    // =========================================================
    // CLEAR GUIDANCE
    // =========================================================

    public void ClearGuidance()
    {
        if (guidanceCoroutine != null)
        {
            StopCoroutine(guidanceCoroutine);
            guidanceCoroutine = null;
        }


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


        currentShotDirection =
            "NONE";
    }
}