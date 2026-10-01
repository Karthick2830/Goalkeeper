using System;
using UnityEngine;
using ExerciseGame.Core.Exercise;
using ExerciseGame.Exercises.StandingLateralObliqueStretch;

namespace ExerciseGame.Core.Exercise
{
    /// <summary>
    /// Generic exercise progression manager.
    ///
    /// Responsibilities:
    /// - Target reps per set
    /// - Target number of sets
    /// - Current rep
    /// - Current set
    /// - Total completed reps
    /// - Start / pause / resume / stop
    /// - Detect completed repetitions
    /// - Detect set completion
    /// - Detect exercise completion
    ///
    /// Validation itself is handled by IExerciseValidator.
    /// </summary>
    public class ExerciseManager : MonoBehaviour
    {
        // =========================================================
        // REFERENCES
        // =========================================================

        [Header("References")]

        [Tooltip(
            "Component implementing IExerciseValidator."
        )]
        [SerializeField]
        private MonoBehaviour validationTesterComponent;


        private IExerciseValidator validationTester;


        // =========================================================
        // EXERCISE TARGET
        // =========================================================

        [Header("Exercise Target")]

        [Tooltip(
            "Number of repetitions required on EACH side in each set."
        )]
        [Min(1)]
        [SerializeField]
        private int targetRepsPerSet = 10;


        [Tooltip(
            "Number of sets required to complete the exercise."
        )]
        [Min(1)]
        [SerializeField]
        private int targetSets = 3;


        // =========================================================
        // STARTUP
        // =========================================================

        [Header("Startup")]

        [Tooltip(
            "Automatically start the exercise when the scene starts."
        )]
        [SerializeField]
        private bool startAutomatically = false;


        // =========================================================
        // DEBUG
        // =========================================================

        [Header("Debug")]

        [SerializeField]
        private bool showDebugLogs = true;


        // =========================================================
        // RUNTIME STATE
        // =========================================================

        private int currentRep;

        private int currentLeftRep;

        private int currentRightRep;

        private int currentSet;

        private int totalCompletedReps;

        private bool isRunning;

        private bool isPaused;

        private bool exerciseCompleted;


        // =========================================================
        // EVENTS
        // =========================================================

        /// <summary>
        /// Fired whenever a complete exercise session is finished.
        ///
        /// Parameters:
        /// 1. Total completed reps
        /// 2. Total target reps
        /// 3. Completed sets
        /// 4. Target sets
        /// </summary>
        public event Action<int, int, int, int>
            OnExerciseCompleted;


        // =========================================================
        // PUBLIC PROPERTIES
        // =========================================================

        public int CurrentRep =>
            currentRep;


        public int CurrentLeftRep =>
            currentLeftRep;


        public int CurrentRightRep =>
            currentRightRep;


        public int TargetRepsPerSide =>
            targetRepsPerSet;


        public int CurrentSet =>
            currentSet;


        public int TotalCompletedReps =>
            totalCompletedReps;


        public int TargetRepsPerSet =>
            targetRepsPerSet;


        public int TargetSets =>
            targetSets;


        public int TotalTargetReps =>
            targetRepsPerSet * 2 * targetSets;


        public bool IsRunning =>
            isRunning;


        public bool IsPaused =>
            isPaused;


        public bool IsExerciseCompleted =>
            exerciseCompleted;


        public IExerciseValidator ValidationTester =>
            validationTester;


        // =========================================================
        // UNITY
        // =========================================================

        private void Awake()
        {
            ResolveValidator();
        }


        private void Start()
        {
            if (startAutomatically)
            {
                StartExercise();
            }
        }


        private void Update()
        {
            if (!isRunning)
                return;

            if (isPaused)
                return;

            if (exerciseCompleted)
                return;

            if (validationTester == null)
                return;


            CheckForRepCompletion();
        }


        // =========================================================
        // RESOLVE VALIDATOR
        // =========================================================

        private void ResolveValidator()
        {
            if (validationTesterComponent == null)
            {
                Debug.LogError(
                    "[ExerciseManager] " +
                    "Validation Tester Component is not assigned."
                );

                return;
            }


            validationTester =
                validationTesterComponent
                    as IExerciseValidator;


            if (validationTester == null)
            {
                Debug.LogError(
                    "[ExerciseManager] " +
                    "Assigned component does not implement " +
                    "IExerciseValidator."
                );
            }
        }


        // =========================================================
        // START EXERCISE
        // =========================================================

        public void StartExercise()
        {
            if (validationTester == null)
            {
                ResolveValidator();
            }


            if (validationTester == null)
            {
                Debug.LogError(
                    "[ExerciseManager] " +
                    "Cannot start exercise. " +
                    "No IExerciseValidator assigned."
                );

                return;
            }


            currentRep = 0;

            currentLeftRep = 0;

            currentRightRep = 0;

            currentSet = 1;

            totalCompletedReps = 0;

            isRunning = true;

            isPaused = false;

            exerciseCompleted = false;


            validationTester.Reset();


            if (showDebugLogs)
            {
                Debug.Log(
                    "[ExerciseManager] ============================="
                );

                Debug.Log(
                    "[ExerciseManager] EXERCISE STARTED"
                );

                Debug.Log(
                    $"[ExerciseManager] " +
                    $"Set: {currentSet}/{targetSets}"
                );

                Debug.Log(
                    $"[ExerciseManager] " +
                    $"Rep: {currentRep}/{targetRepsPerSet}"
                );

                Debug.Log(
                    $"[ExerciseManager] " +
                    $"Total: {totalCompletedReps}/{TotalTargetReps}"
                );

                Debug.Log(
                    "[ExerciseManager] ============================="
                );
            }
        }


        // =========================================================
        // PAUSE
        // =========================================================

        public void PauseExercise()
        {
            if (!isRunning)
                return;

            if (exerciseCompleted)
                return;

            if (isPaused)
                return;


            isPaused = true;


            if (validationTesterComponent != null)
            {
                validationTesterComponent.SendMessage(
                    "PauseValidation",
                    SendMessageOptions.DontRequireReceiver
                );
            }


            if (showDebugLogs)
            {
                Debug.Log(
                    "[ExerciseManager] Exercise paused."
                );
            }
        }


        // =========================================================
        // RESUME
        // =========================================================

        public void ResumeExercise()
        {
            if (!isRunning)
                return;

            if (exerciseCompleted)
                return;

            if (!isPaused)
                return;


            isPaused = false;


            if (validationTesterComponent != null)
            {
                validationTesterComponent.SendMessage(
                    "ResumeValidation",
                    SendMessageOptions.DontRequireReceiver
                );
            }


            if (showDebugLogs)
            {
                Debug.Log(
                    "[ExerciseManager] Exercise resumed."
                );
            }
        }


        // =========================================================
        // TOGGLE PAUSE
        // =========================================================

        public void TogglePause()
        {
            if (isPaused)
            {
                ResumeExercise();
            }
            else
            {
                PauseExercise();
            }
        }


        // =========================================================
        // END EXERCISE
        // =========================================================

        public void EndExercise()
        {
            if (!isRunning)
                return;


            isRunning = false;

            isPaused = false;


            if (validationTesterComponent != null)
            {
                validationTesterComponent.SendMessage(
                    "PauseValidation",
                    SendMessageOptions.DontRequireReceiver
                );
            }


            if (showDebugLogs)
            {
                Debug.Log(
                    "[ExerciseManager] ============================="
                );

                Debug.Log(
                    "[ExerciseManager] EXERCISE ENDED"
                );

                Debug.Log(
                    $"[ExerciseManager] " +
                    $"Set: {currentSet}/{targetSets}"
                );

                Debug.Log(
                    $"[ExerciseManager] " +
                    $"Rep: {currentRep}/{targetRepsPerSet}"
                );

                Debug.Log(
                    $"[ExerciseManager] " +
                    $"Total Reps: {totalCompletedReps}/{TotalTargetReps}"
                );

                Debug.Log(
                    "[ExerciseManager] ============================="
                );
            }
        }


        // =========================================================
        // REP DETECTION
        // =========================================================

        private void CheckForRepCompletion()
        {
            if (!validationTester.IsRepCompleted)
                return;


            // The validator must tell us which side was actually
            // completed. This is captured before ConsumeRep().
            StandingLateralObliqueValidator obliqueValidator =
                validationTester as StandingLateralObliqueValidator;

            string completedDirection =
                obliqueValidator != null
                    ? obliqueValidator.CompletedRepDirection
                    : "NONE";


            // -----------------------------------------------------
            // COUNT THE CORRECT SIDE
            // -----------------------------------------------------

            if (completedDirection == "LEFT")
            {
                if (currentLeftRep < targetRepsPerSet)
                    currentLeftRep++;
            }
            else if (completedDirection == "RIGHT")
            {
                if (currentRightRep < targetRepsPerSet)
                    currentRightRep++;
            }
            else
            {
                Debug.LogWarning(
                    "[ExerciseManager] Rep completed but no LEFT/RIGHT direction was supplied.");

                validationTester.ConsumeRep();
                return;
            }


            // CurrentRep is the total number of valid repetitions
            // completed in this set across both sides.
            currentRep =
                currentLeftRep + currentRightRep;

            totalCompletedReps++;


            if (showDebugLogs)
            {
                Debug.Log(
                    "[ExerciseManager] REP COMPLETED | " +
                    "Side: " + completedDirection);

                Debug.Log(
                    $"[ExerciseManager] " +
                    $"Set {currentSet}/{targetSets} | " +
                    $"Left {currentLeftRep}/{targetRepsPerSet} | " +
                    $"Right {currentRightRep}/{targetRepsPerSet}");

                Debug.Log(
                    $"[ExerciseManager] TOTAL REPS: " +
                    $"{totalCompletedReps}/{TotalTargetReps}");
            }


            validationTester.ConsumeRep();

            CheckSetCompletion();
        }


        // =========================================================
        // SET COMPLETION
        // =========================================================

        private void CheckSetCompletion()
        {
            // A set is complete only when BOTH sides reach the target.
            if (currentLeftRep < targetRepsPerSet ||
                currentRightRep < targetRepsPerSet)
            {
                return;
            }


            if (showDebugLogs)
            {
                Debug.Log(
                    $"[ExerciseManager] " +
                    $"SET {currentSet} COMPLETED."
                );
            }


            // =====================================================
            // ALL SETS COMPLETED
            // =====================================================

            if (currentSet >= targetSets)
            {
                CompleteExercise();

                return;
            }


            // =====================================================
            // NEXT SET
            // =====================================================

            StartNextSet();
        }


        // =========================================================
        // START NEXT SET
        // =========================================================

        private void StartNextSet()
        {
            currentSet++;

            currentRep = 0;

            currentLeftRep = 0;

            currentRightRep = 0;

            validationTester.Reset();


            if (showDebugLogs)
            {
                Debug.Log(
                    "[ExerciseManager] ============================="
                );

                Debug.Log(
                    "[ExerciseManager] NEXT SET STARTED"
                );

                Debug.Log(
                    $"[ExerciseManager] " +
                    $"Set: {currentSet}/{targetSets}"
                );

                Debug.Log(
                    $"[ExerciseManager] " +
                    $"Rep: {currentRep}/{targetRepsPerSet}"
                );

                Debug.Log(
                    $"[ExerciseManager] " +
                    $"Total: {totalCompletedReps}/{TotalTargetReps}"
                );

                Debug.Log(
                    "[ExerciseManager] ============================="
                );
            }
        }


        // =========================================================
        // COMPLETE EXERCISE
        // =========================================================

        private void CompleteExercise()
        {
            exerciseCompleted = true;

            isRunning = false;

            isPaused = false;


            if (validationTesterComponent != null)
            {
                validationTesterComponent.SendMessage(
                    "PauseValidation",
                    SendMessageOptions.DontRequireReceiver
                );
            }


            if (showDebugLogs)
            {
                Debug.Log(
                    "[ExerciseManager] ============================="
                );

                Debug.Log(
                    "[ExerciseManager] EXERCISE COMPLETED"
                );

                Debug.Log(
                    $"[ExerciseManager] " +
                    $"Total Reps: " +
                    $"{totalCompletedReps}/{TotalTargetReps}"
                );

                Debug.Log(
                    $"[ExerciseManager] " +
                    $"Sets: {targetSets}/{targetSets}"
                );

                Debug.Log(
                    "[ExerciseManager] ============================="
                );
            }


            // =====================================================
            // NOTIFY UI
            // =====================================================

            OnExerciseCompleted?.Invoke(
                totalCompletedReps,
                TotalTargetReps,
                targetSets,
                targetSets
            );
        }


        // =========================================================
        // TARGET CONFIGURATION
        // =========================================================

        public void SetTargetReps(int reps)
        {
            targetRepsPerSet =
                Mathf.Max(1, reps);
        }


        public void SetTargetSets(int sets)
        {
            targetSets =
                Mathf.Max(1, sets);
        }


        public void SetExerciseTarget(
            int reps,
            int sets)
        {
            targetRepsPerSet =
                Mathf.Max(1, reps);

            targetSets =
                Mathf.Max(1, sets);
        }


        // =========================================================
        // RESET
        // =========================================================

        public void ResetExercise()
        {
            currentRep = 0;

            currentLeftRep = 0;

            currentRightRep = 0;

            currentSet = 1;

            totalCompletedReps = 0;

            isRunning = false;

            isPaused = false;

            exerciseCompleted = false;


            if (validationTester != null)
            {
                validationTester.Reset();
            }


            if (showDebugLogs)
            {
                Debug.Log(
                    "[ExerciseManager] Exercise reset."
                );
            }
        }


        // =========================================================
        // REP PROGRESS
        // =========================================================

        public float GetRepProgress()
        {
            if (targetRepsPerSet <= 0)
                return 0f;


            return Mathf.Clamp01(
                (float)currentRep /
                (targetRepsPerSet * 2)
            );
        }


        // =========================================================
        // SET PROGRESS
        // =========================================================

        public float GetSetProgress()
        {
            if (targetSets <= 0)
                return 0f;


            return Mathf.Clamp01(
                (float)(currentSet - 1) /
                targetSets
            );
        }


        // =========================================================
        // OVERALL PROGRESS
        // =========================================================

        public float GetOverallProgress()
        {
            if (TotalTargetReps <= 0)
                return 0f;


            return Mathf.Clamp01(
                (float)totalCompletedReps /
                TotalTargetReps
            );
        }
    }
}