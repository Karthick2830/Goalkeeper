using UnityEngine;
using ExerciseGame.Core.Exercise;
using ExerciseGame.Core.Pose;

namespace ExerciseGame.Exercises.StandingLateralObliqueStretch
{
    public class StandingLateralObliqueValidator :
        MonoBehaviour,
        IExerciseValidator
    {
        // =========================================================
        // REFERENCES
        // =========================================================

        [Header("References")]

        [SerializeField]
        private StandingLateralObliqueSettings settings;

        [SerializeField]
        private PoseDataProvider poseProvider;


        // =========================================================
        // READINESS
        // =========================================================

        [Header("Common Pose Readiness")]

        [Range(0.5f, 1f)]
        [SerializeField]
        private float minimumJointVisibility = 0.75f;

        [Range(0.5f, 1f)]
        [SerializeField]
        private float minimumFacingScore = 0.75f;


        // =========================================================
        // DEBUG
        // =========================================================

        [Header("Debug")]

        [SerializeField]
        private bool showDebugLogs = false;


        // =========================================================
        // PUBLIC STATE
        // =========================================================

        public ExerciseState CurrentState
        {
            get;
            private set;
        }

        public bool IsRepCompleted
        {
            get;
            private set;
        }

        public bool IsPoseReady
        {
            get
            {
                return debugReadinessState ==
                       PoseReadinessState.Ready;
            }
        }


        // =========================================================
        // MEASUREMENTS
        // =========================================================

        public float CurrentTorsoAngle
        {
            get;
            private set;
        }

        public float CurrentRightArmOverheadAngle
        {
            get;
            private set;
        }

        public float CurrentLeftArmOverheadAngle
        {
            get;
            private set;
        }

        public float CurrentRightElbowAngle
        {
            get;
            private set;
        }

        public float CurrentLeftElbowAngle
        {
            get;
            private set;
        }


        // =========================================================
        // ARM STATES
        // =========================================================

        public bool IsRightArmOverhead
        {
            get;
            private set;
        }

        public bool IsLeftArmOverhead
        {
            get;
            private set;
        }

        public string ActiveArm
        {
            get;
            private set;
        }

        public string ExpectedBendDirection
        {
            get;
            private set;
        }

        public string NextArm
        {
            get;
            private set;
        }

        public int CompletedReps
        {
            get;
            private set;
        }

        /// <summary>
        /// Direction of the repetition that was most recently completed.
        /// This remains available after the validator resets ActiveArm/ExpectedBendDirection.
        /// </summary>
        public string CompletedRepDirection
        {
            get;
            private set;
        }

        public string DetectedBendDirection
        {
            get
            {
                return DirectionToString(detectedDirection);
            }
        }

        public bool HasStartedBending
        {
            get
            {
                return CurrentState == ExerciseState.Moving ||
                       (CurrentState == ExerciseState.Ready &&
                        Mathf.Abs(CurrentTorsoAngle) > settings.minimumMovementAngle);
            }
        }

        public string CommittedBendDirection
        {
            get
            {
                return DirectionToString(committedDirection);
            }
        }

        // =========================================================
        // DEBUG VALUES
        // =========================================================

        [Header("Runtime Debug")]

        [SerializeField]
        private float debugRightArmAngle;

        [SerializeField]
        private float debugLeftArmAngle;

        [SerializeField]
        private bool debugRightOverhead;

        [SerializeField]
        private bool debugLeftOverhead;

        [SerializeField]
        private string debugActiveArm;

        [SerializeField]
        private string debugNextArm;

        [SerializeField]
        private string debugExpectedBend;

        [SerializeField]
        private string debugDetectedBend;

        [SerializeField]
        private float debugHoldTime;

        [SerializeField]
        private int debugTargetFrames;

        [SerializeField]
        private int debugCompletedReps;


        // =========================================================
        // READINESS DEBUG
        // =========================================================

        [Header("Readiness Debug")]

        [SerializeField]
        private PoseReadinessState debugReadinessState;

        [SerializeField]
        [Range(0f, 1f)]
        private float debugVisibilityScore;

        [SerializeField]
        [Range(0f, 1f)]
        private float debugFacingScore;


        // =========================================================
        // INTERNAL STATE
        // =========================================================

        private float neutralTimer;
        private float targetHoldTimer;

        private int movementStableFrames;
        private int targetStableFrames;

        private int committedDirection;
        private int detectedDirection;

        private float maximumReachedAngle;

        private bool targetSuccessfullyReached;

        private float previousAngle;

        private PoseReadinessValidator readinessValidator;


        // =========================================================
        // UNITY
        // =========================================================

        private void Awake()
        {
            CreateReadinessValidator();
            Reset();
        }


        private void CreateReadinessValidator()
        {
            readinessValidator =
                new PoseReadinessValidator(
                    minimumJointVisibility,
                    minimumFacingScore);
        }


        // =========================================================
        // RESET
        // =========================================================

        public void Reset()
        {
            CurrentState =
                ExerciseState.Waiting;

            IsRepCompleted =
                false;

            CurrentTorsoAngle =
                0f;

            CurrentRightArmOverheadAngle =
                180f;

            CurrentLeftArmOverheadAngle =
                180f;

            CurrentRightElbowAngle =
                0f;

            CurrentLeftElbowAngle =
                0f;

            IsRightArmOverhead =
                false;

            IsLeftArmOverhead =
                false;

            ActiveArm =
                "NONE";

            ExpectedBendDirection =
                "NONE";

            // User can choose either arm for every rep.
            NextArm =
                "ANY";

            CompletedReps =
                0;

            CompletedRepDirection =
                "NONE";

            neutralTimer =
                0f;

            targetHoldTimer =
                0f;

            movementStableFrames =
                0;

            targetStableFrames =
                0;

            committedDirection =
                0;

            detectedDirection =
                0;

            maximumReachedAngle =
                0f;

            targetSuccessfullyReached =
                false;

            previousAngle =
                0f;

            ResetDebug();

            if (readinessValidator == null)
            {
                CreateReadinessValidator();
            }

            readinessValidator.Reset();
        }


        // =========================================================
        // DEBUG RESET
        // =========================================================

        private void ResetDebug()
        {
            debugRightArmAngle = 180f;
            debugLeftArmAngle = 180f;

            debugRightOverhead = false;
            debugLeftOverhead = false;

            debugActiveArm = "NONE";
            debugNextArm = "ANY";
            debugExpectedBend = "NONE";
            debugDetectedBend = "NONE";

            debugHoldTime = 0f;
            debugTargetFrames = 0;
            debugCompletedReps = 0;

            debugReadinessState =
                PoseReadinessState.NoPerson;

            debugVisibilityScore = 0f;
            debugFacingScore = 0f;
        }


        // =========================================================
        // VALIDATE
        // =========================================================

        public ValidationResult Validate(
            PoseFrame pose,
            float deltaTime)
        {
            IsRepCompleted =
                false;


            if (settings == null)
            {
                CurrentState =
                    ExerciseState.Waiting;

                return ValidationResult.Invalid(
                    "Standing Lateral Oblique settings missing.");
            }


            // =====================================================
            // READINESS
            // =====================================================

            if (readinessValidator == null)
            {
                CreateReadinessValidator();
            }

            PoseReadinessResult readiness =
                readinessValidator.Evaluate(
                    pose,
                    deltaTime);

            debugReadinessState =
                readiness.state;

            debugVisibilityScore =
                readiness.visibilityScore;

            debugFacingScore =
                readiness.facingScore;


            if (!readiness.isReady)
            {
                CancelAttempt();

                CurrentState =
                    ExerciseState.Waiting;

                return ValidationResult.Invalid(
                    readiness.feedback);
            }


            // =====================================================
            // MEASUREMENT
            // =====================================================

            if (!StandingLateralObliqueMeasurement
                .TryGetMeasurements(
                    pose,
                    settings.minimumConfidence,

                    out float torsoAngle,
                    out float rightArmAngle,
                    out float leftArmAngle,

                    out float rightElbowAngle,
                    out float leftElbowAngle))
            {
                CurrentState =
                    ExerciseState.Waiting;

                return ValidationResult.Invalid(
                    "Tracking body...");
            }


            // =====================================================
            // STORE
            // =====================================================

            float correctedTorsoAngle =
                settings.invertDirection
                    ? -torsoAngle
                    : torsoAngle;

            CurrentTorsoAngle =
                correctedTorsoAngle;

            CurrentRightArmOverheadAngle =
                rightArmAngle;

            CurrentLeftArmOverheadAngle =
                leftArmAngle;

            CurrentRightElbowAngle =
                rightElbowAngle;

            CurrentLeftElbowAngle =
                leftElbowAngle;


            // =====================================================
            // OVERHEAD
            // =====================================================

            /*
             * NEW CONVENTION
             *
             * Arm down:
             *      ~180°
             *
             * Arm sideways:
             *      ~90°
             *
             * Arm overhead:
             *      ~0°
             *
             * Setting:
             *      0° - 15° = overhead
             */

            IsRightArmOverhead =
                rightArmAngle >=
                settings.minimumOverheadAngle;

            IsLeftArmOverhead =
                leftArmAngle >=
                settings.minimumOverheadAngle;


            debugRightArmAngle =
                rightArmAngle;

            debugLeftArmAngle =
                leftArmAngle;

            debugRightOverhead =
                IsRightArmOverhead;

            debugLeftOverhead =
                IsLeftArmOverhead;

            debugNextArm =
                NextArm;


            // =====================================================
            // WAITING - USER SELECTS ARM
            // =====================================================
            //
            // Either arm can be selected for every repetition.
            // The user is allowed to switch arms before bending.
            // The selected arm is locked only when the actual bend
            // reaches minimumMovementAngle.
            //

            if (CurrentState ==
                ExerciseState.Waiting)
            {
                ActiveArm =
                    "NONE";

                ExpectedBendDirection =
                    "NONE";

                committedDirection =
                    0;

                // -----------------------------------------------
                // RIGHT ARM
                // -----------------------------------------------

                if (IsRightArmOverhead)
                {
                    if (rightElbowAngle <
                        settings.minimumElbowExtensionAngle)
                    {
                        return ValidationResult.Invalid(
                            "Keep your RIGHT arm straight.");
                    }

                    ActiveArm =
                        "RIGHT";

                    ExpectedBendDirection =
                        "LEFT";

                    CurrentState =
                        ExerciseState.Ready;

                    previousAngle =
                        correctedTorsoAngle;

                    debugActiveArm =
                        ActiveArm;

                    debugExpectedBend =
                        ExpectedBendDirection;

                    return ValidationResult.Valid(
                        1f,
                        1f,
                        "RIGHT arm overhead. Bend LEFT.");
                }

                // -----------------------------------------------
                // LEFT ARM
                // -----------------------------------------------

                if (IsLeftArmOverhead)
                {
                    if (leftElbowAngle <
                        settings.minimumElbowExtensionAngle)
                    {
                        return ValidationResult.Invalid(
                            "Keep your LEFT arm straight.");
                    }

                    ActiveArm =
                        "LEFT";

                    ExpectedBendDirection =
                        "RIGHT";

                    CurrentState =
                        ExerciseState.Ready;

                    previousAngle =
                        correctedTorsoAngle;

                    debugActiveArm =
                        ActiveArm;

                    debugExpectedBend =
                        ExpectedBendDirection;

                    return ValidationResult.Valid(
                        1f,
                        1f,
                        "LEFT arm overhead. Bend RIGHT.");
                }

                return ValidationResult.Invalid(
                    "Raise either arm overhead.");
            }


            // =====================================================
            // TORSO
            // =====================================================

            // =========================================================
            // BEND AMOUNT
            // =========================================================
            //
            // Upright is approximately ±180°.
            //
            // 179°  -> 1° bend
            // 170°  -> 10° bend
            // -170° -> 10° bend
            //
            // This gives us the actual amount of lateral bend.
            //

            float bendAngle =
                180f -
                Mathf.Abs(correctedTorsoAngle);

            bendAngle =
                Mathf.Clamp(
                    bendAngle,
                    0f,
                    180f);


            // =====================================================
            // DIRECTION
            // =====================================================

            detectedDirection =
                GetDirection(
                    correctedTorsoAngle);

            debugDetectedBend =
                DirectionToString(
                    detectedDirection);


            // =====================================================
            // READY
            // =====================================================

            if (CurrentState ==
                ExerciseState.Ready)
            {
                // -----------------------------------------------
                // STILL UPRIGHT - ALLOW ARM SWITCHING
                // -----------------------------------------------
                //
                // Example:
                // LEFT overhead -> lower LEFT -> RIGHT overhead
                // -> bend LEFT.
                //
                // The final arm is selected from whichever arm is
                // overhead when the actual bend begins.
                //

                if (bendAngle <=
                    settings.neutralAngle)
                {
                    movementStableFrames =
                        0;

                    committedDirection =
                        0;

                    maximumReachedAngle =
                        0f;

                    if (IsRightArmOverhead &&
                        rightElbowAngle >=
                        settings.minimumElbowExtensionAngle)
                    {
                        ActiveArm =
                            "RIGHT";

                        ExpectedBendDirection =
                            "LEFT";
                    }
                    else if (IsLeftArmOverhead &&
                             leftElbowAngle >=
                             settings.minimumElbowExtensionAngle)
                    {
                        ActiveArm =
                            "LEFT";

                        ExpectedBendDirection =
                            "RIGHT";
                    }
                    else
                    {
                        ActiveArm =
                            "NONE";

                        ExpectedBendDirection =
                            "NONE";
                    }

                    previousAngle =
                        correctedTorsoAngle;

                    debugActiveArm =
                        ActiveArm;

                    debugExpectedBend =
                        ExpectedBendDirection;

                    if (ActiveArm == "NONE")
                    {
                        return ValidationResult.Invalid(
                            "Raise either arm overhead.");
                    }

                    return ValidationResult.Valid(
                        1f,
                        1f,
                        ActiveArm +
                        " arm overhead. Bend " +
                        ExpectedBendDirection +
                        ".");
                }

                // -----------------------------------------------
                // CORRECT DIRECTION
                // -----------------------------------------------

                if (!IsCorrectDirection(
                    correctedTorsoAngle))
                {
                    return ValidationResult.Invalid(
                        "Wrong side. Bend " +
                        ExpectedBendDirection);
                }

                // -----------------------------------------------
                // MINIMUM MOVEMENT
                // -----------------------------------------------

                if (bendAngle <
                    settings.minimumMovementAngle)
                {
                    movementStableFrames =
                        0;

                    return ValidationResult.Valid(
                        0.25f,
                        1f,
                        "Start bending " +
                        ExpectedBendDirection);
                }

                // -----------------------------------------------
                // ACTUAL BEND STARTED - LOCK DIRECTION
                // -----------------------------------------------

                if (committedDirection == 0)
                {
                    committedDirection =
                        detectedDirection;
                }

                if (detectedDirection !=
                    committedDirection)
                {
                    return ValidationResult.Invalid(
                        "Keep bending " +
                        ExpectedBendDirection);
                }

                // -----------------------------------------------
                // MOVEMENT STABILITY
                // -----------------------------------------------

                float frameChange =
                    Mathf.Abs(
                        correctedTorsoAngle -
                        previousAngle);

                previousAngle =
                    correctedTorsoAngle;

                if (frameChange >
                    settings.maximumFrameAngleChange)
                {
                    movementStableFrames =
                        0;

                    return ValidationResult.Invalid(
                        "Move smoothly.");
                }

                movementStableFrames++;

                if (movementStableFrames <
                    settings.movementStableFrames)
                {
                    return ValidationResult.Valid(
                        0.4f,
                        1f,
                        "Keep bending.");
                }

                // -----------------------------------------------
                // START MOVING
                // -----------------------------------------------

                maximumReachedAngle =
                    bendAngle;

                targetStableFrames =
                    0;

                targetHoldTimer =
                    0f;

                targetSuccessfullyReached =
                    false;

                CurrentState =
                    ExerciseState.Moving;

                return ValidationResult.Valid(
                    0.5f,
                    1f,
                    "Keep bending.");
            }


            // =====================================================
            // MOVING
            // =====================================================

            if (CurrentState ==
                ExerciseState.Moving)
            {
                // -----------------------------------------------
                // DIRECTION LOCK
                // -----------------------------------------------

                if (detectedDirection !=
                    committedDirection)
                {
                    return ValidationResult.Invalid(
                        "Keep bending " +
                        ExpectedBendDirection);
                }


                // -----------------------------------------------
                // MAXIMUM REACHED
                // -----------------------------------------------

                if (bendAngle >
                    maximumReachedAngle)
                {
                    maximumReachedAngle =
                            bendAngle;
                }


                // -----------------------------------------------
                // TARGET
                // -----------------------------------------------

                if (bendAngle >=
                    settings.targetAngle)
                {
                    targetStableFrames++;

                    targetHoldTimer +=
                        deltaTime;


                    if (targetStableFrames >=
                            settings.targetStableFrames &&
                        targetHoldTimer >=
                            settings.requiredHoldTime)
                    {
                        targetSuccessfullyReached =
                            true;

                        CurrentState =
                            ExerciseState.Returning;

                        targetStableFrames =
                            0;

                        targetHoldTimer =
                            0f;

                        neutralTimer =
                            0f;


                        if (showDebugLogs)
                        {
                            UnityEngine.Debug.Log(
                                "[Standing Lateral Oblique] " +
                                "TARGET REACHED | Arm: " +
                                ActiveArm +
                                " | Bend: " +
                                ExpectedBendDirection +
                                " | Angle: " +
                                bendAngle.ToString("F1"));
                        }


                        return ValidationResult.Valid(
                            1f,
                            1f,
                            "Great! Return to center.");
                    }


                    return ValidationResult.Valid(
                        0.95f,
                        1f,
                        "Hold.");
                }


                // -----------------------------------------------
                // PREMATURE RETURN
                // -----------------------------------------------

                if (bendAngle <
                    maximumReachedAngle -
                    settings.maximumAngleReversal)
                {
                    CancelAttempt();

                    CurrentState =
                        ExerciseState.Ready;

                    return ValidationResult.Invalid(
                        "Reach the target before returning.");
                }


                float progress =
                    Mathf.InverseLerp(
                        settings.minimumMovementAngle,
                        settings.targetAngle,
                        bendAngle);


                return ValidationResult.Valid(
                    progress,
                    1f,
                    "Keep bending.");
            }


            // =====================================================
            // RETURNING
            // =====================================================

            if (CurrentState ==
                ExerciseState.Returning)
            {
                if (!targetSuccessfullyReached)
                {
                    CancelAttempt();

                    CurrentState =
                        ExerciseState.Ready;

                    return ValidationResult.Invalid(
                        "Target was not reached.");
                }


                // -----------------------------------------------
                // DIRECTION SAFETY
                // -----------------------------------------------

                if (detectedDirection != 0 &&
                    detectedDirection !=
                    committedDirection &&
                    bendAngle >
                    settings.neutralAngle)
                {
                    return ValidationResult.Invalid(
                        "Return straight to center.");
                }


                // -----------------------------------------------
                // RETURN TO CENTER
                // -----------------------------------------------

                if (bendAngle <=
                    settings.neutralAngle)
                {
                    neutralTimer +=
                        deltaTime;


                    if (neutralTimer >=
                        settings.neutralRequiredTime)
                    {
                        // =========================================
                        // REP COMPLETED
                        // =========================================

                        IsRepCompleted =
                            true;

                        // Preserve the side of the completed repetition
                        // before ActiveArm/ExpectedBendDirection are reset.
                        CompletedRepDirection =
                            ExpectedBendDirection;

                        CompletedReps++;


                        if (showDebugLogs)
                        {
                            UnityEngine.Debug.Log(
                                "[Standing Lateral Oblique] " +
                                "REP CONFIRMED | Rep: " +
                                CompletedReps +
                                " | Arm: " +
                                ActiveArm +
                                " | Bend: " +
                                ExpectedBendDirection);
                        }


                        // =========================================
                        // NEXT REP - USER CHOOSES THE ARM
                        // =========================================
                        //
                        // Do not force RIGHT -> LEFT -> RIGHT.
                        // The next repetition starts in Waiting and
                        // the user can choose either arm.
                        //
                        NextArm = "ANY";


                        targetSuccessfullyReached =
                            false;

                        neutralTimer =
                            0f;

                        movementStableFrames =
                            0;

                        targetStableFrames =
                            0;

                        targetHoldTimer =
                            0f;

                        committedDirection =
                            0;

                        maximumReachedAngle =
                            0f;

                        previousAngle =
                            correctedTorsoAngle;


                        ActiveArm =
                            "NONE";

                        ExpectedBendDirection =
                            "NONE";

                        CurrentState =
                            ExerciseState.Waiting;


                        debugNextArm =
                            NextArm;

                        debugActiveArm =
                            "NONE";

                        debugExpectedBend =
                            "NONE";


                        return ValidationResult.Valid(
                            1f,
                            1f,
                            "Rep completed! Raise your " +
                            NextArm +
                            " arm.");
                    }


                    return ValidationResult.Valid(
                        0.9f,
                        1f,
                        "Return to center.");
                }


                neutralTimer =
                    0f;

                return ValidationResult.Valid(
                    0.8f,
                    1f,
                    "Return to center.");
            }


            return ValidationResult.Valid(
                0.5f,
                1f,
                "Continue.");
        }




        // =========================================================
        // DIRECTION
        // =========================================================

        private int GetDirection(float angle)
        {
            // Standing upright
            if (Mathf.Abs(angle) <= settings.neutralAngle)
            {
                return 0;
            }

            // IMPORTANT:
            // With the current MediaPipe camera coordinate setup,
            // positive torso angle = physical LEFT bend
            // negative torso angle = physical RIGHT bend.

            if (angle > 0f)
            {
                return -1; // LEFT
            }

            return 1; // RIGHT
        }


        // =========================================================
        // CORRECT DIRECTION
        // =========================================================

        private bool IsCorrectDirection(
            float angle)
        {
            int detected =
                GetDirection(angle);

            if (detected == 0)
                return false;


            if (ExpectedBendDirection ==
                "LEFT")
            {
                return detected == -1;
            }


            if (ExpectedBendDirection ==
                "RIGHT")
            {
                return detected == 1;
            }


            return false;
        }


        // =========================================================
        // DIRECTION STRING
        // =========================================================

        private string DirectionToString(
            int direction)
        {
            if (direction < 0)
                return "LEFT";

            if (direction > 0)
                return "RIGHT";

            return "NONE";
        }


        // =========================================================
        // CANCEL ATTEMPT
        // =========================================================

        private void CancelAttempt()
        {
            neutralTimer =
                0f;

            targetHoldTimer =
                0f;

            movementStableFrames =
                0;

            targetStableFrames =
                0;

            committedDirection =
                0;

            maximumReachedAngle =
                0f;

            targetSuccessfullyReached =
                false;

            previousAngle =
                0f;

            ActiveArm =
                "NONE";

            ExpectedBendDirection =
                "NONE";

            IsRightArmOverhead =
                false;

            IsLeftArmOverhead =
                false;

            debugActiveArm =
                "NONE";

            debugExpectedBend =
                "NONE";
        }


        // =========================================================
        // CONSUME REP
        // =========================================================

        public void ConsumeRep()
        {
            IsRepCompleted =
                false;
        }
    }
}