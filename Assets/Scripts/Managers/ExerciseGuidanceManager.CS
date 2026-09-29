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
    // LEFT SIDE GUIDANCE
    // =========================================================
    //
    // LEFT BALL
    //
    // Raise RIGHT hand
    // Bend LEFT
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
    // RIGHT SIDE GUIDANCE
    // =========================================================
    //
    // RIGHT BALL
    //
    // Raise LEFT hand
    // Bend RIGHT
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
        "Raise your right hand";

    [SerializeField]
    private string bendLeftText =
        "Bend to the left";

    [SerializeField]
    private string raiseLeftHandText =
        "Raise your left hand";

    [SerializeField]
    private string bendRightText =
        "Bend to the right";


    // =========================================================
    // STATE
    // =========================================================

    private string currentShotDirection =
        "NONE";


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
    }


    // =========================================================
    // SHOT PREPARING
    // =========================================================

    private void HandleShotPreparing(
        string direction)
    {
        currentShotDirection =
            direction;


        if (direction == "LEFT")
        {
            ShowGuidance(
                raiseRightHandImage,
                raiseRightHandText,
                raiseRightHandVoice
            );

            return;
        }


        if (direction == "RIGHT")
        {
            ShowGuidance(
                raiseLeftHandImage,
                raiseLeftHandText,
                raiseLeftHandVoice
            );

            return;
        }
    }


    // =========================================================
    // BALL KICKED
    // =========================================================

    private void HandleBallKick()
    {
        if (currentShotDirection == "LEFT")
        {
            ShowGuidance(
                bendLeftImage,
                bendLeftText,
                bendLeftVoice
            );

            return;
        }


        if (currentShotDirection == "RIGHT")
        {
            ShowGuidance(
                bendRightImage,
                bendRightText,
                bendRightVoice
            );

            return;
        }
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


        // Stop the previous instruction.
        guidanceAudioSource.Stop();


        // Play the new ElevenLabs clip.
        guidanceAudioSource.PlayOneShot(
            clip
        );
    }


    // =========================================================
    // CLEAR GUIDANCE
    // =========================================================

    public void ClearGuidance()
    {
        if (guidanceImage != null)
        {
            guidanceImage.enabled = false;
        }


        if (guidanceText != null)
        {
            guidanceText.text = "";
            guidanceText.enabled = false;
        }


        if (guidanceAudioSource != null)
        {
            guidanceAudioSource.Stop();
        }


        currentShotDirection =
            "NONE";
    }
}