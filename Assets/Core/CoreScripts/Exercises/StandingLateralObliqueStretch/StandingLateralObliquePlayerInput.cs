using UnityEngine;
using ExerciseGame.Exercises.StandingLateralObliqueStretch;

public class StandingLateralObliquePlayerInput : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private StandingLateralObliqueValidator validator;
    [SerializeField] private PlayerMovement player;

    [Header("Testing")]
    [SerializeField] private bool allowKeyboardFallback = true;

    private string lastDirection = "NONE";

    private void Awake()
    {
        if (validator == null)
            validator =
                FindFirstObjectByType<StandingLateralObliqueValidator>();

        if (player == null)
            player =
                FindFirstObjectByType<PlayerMovement>();
    }

    private void OnEnable()
    {
        if (player != null)
            player.OnReturnedToCenter += ResetInput;
    }

    private void OnDisable()
    {
        if (player != null)
            player.OnReturnedToCenter -= ResetInput;
    }

    private void Update()
    {
        if (validator == null || player == null)
            return;

        HandleExerciseInput();

        if (allowKeyboardFallback)
            HandleKeyboardInput();
    }

    private void HandleExerciseInput()
    {
        // Use the direction committed by the validator once
        // the actual bend has started.
        string direction =
            validator.CommittedBendDirection;

        if (direction == "NONE")
            return;

        if (direction == lastDirection)
            return;

        if (direction == "LEFT")
        {
            player.TriggerLeftDive();
            lastDirection = "LEFT";

            Debug.Log(
                "[Oblique Input] LEFT bend -> LEFT DIVE");

            return;
        }

        if (direction == "RIGHT")
        {
            player.TriggerRightDive();
            lastDirection = "RIGHT";

            Debug.Log(
                "[Oblique Input] RIGHT bend -> RIGHT DIVE");
        }
    }

    private void HandleKeyboardInput()
    {
        if (Input.GetKeyDown(KeyCode.A)     ||
            Input.GetKeyDown(KeyCode.LeftArrow))
        {
            player.TriggerLeftDive();
        }

        if (Input.GetKeyDown(KeyCode.D) ||
            Input.GetKeyDown(KeyCode.RightArrow))
        {
            player.TriggerRightDive();
        }
    }

    public void ResetInput()
    {
        lastDirection = "NONE";
    }
}
