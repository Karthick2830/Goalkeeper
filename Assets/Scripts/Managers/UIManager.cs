using System.Collections;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using ExerciseGame.Core.Exercise;

public class UIManager : MonoBehaviour
{
    // =========================================================
    // PANELS
    // =========================================================

    [Header("Panels")]

    [SerializeField]
    private GameObject pausePanel;

    [SerializeField]
    private GameObject mainMenuPanel;

    [SerializeField]
    private GameObject gameOverPanel;

    // Existing panel reused as final exercise result panel
    [SerializeField]
    private GameObject winPanel;

    // IMPORTANT:
    // This panel is now the GAMEPLAY EXERCISE HUD.
    // It should remain ACTIVE during gameplay.
    [SerializeField]
    private GameObject livesPanel;

    [SerializeField]
    private GameObject countdownPanel;

    [SerializeField]
    private GameObject levelsPanel;

    [SerializeField]
    private GameObject settingsPanel;

    [SerializeField]
    private GameObject settingsPanelFromGame;


    // =========================================================
    // GAMEPLAY HUD
    // =========================================================

    [Header("Exercise HUD")]

    [Tooltip("Current save count during gameplay.")]
    [SerializeField]
    private TextMeshProUGUI score;

    [Tooltip("Left-side repetitions / target repetitions per side.")]
    [SerializeField]
    private TextMeshProUGUI repProgressText;

    [Tooltip("Right-side repetitions / target repetitions per side.")]
    [SerializeField]
    private TextMeshProUGUI rightRepProgressText;

    [Tooltip("Current set / target sets.")]
    [SerializeField]
    private TextMeshProUGUI setProgressText;



    [SerializeField]
    private TextMeshProUGUI streakText;


    // =========================================================
    // RESULT PANEL
    // =========================================================

    [Header("Exercise Result")]

    [Tooltip("Reps completed text on the existing Win Panel.")]
    [SerializeField]
    private TextMeshProUGUI winScoreText;

    [Tooltip("Sets completed text on the existing Win Panel.")]
    [SerializeField]
    private TextMeshProUGUI highScoreText;

    [Tooltip("Total saves text on the existing Win Panel.")]
    [SerializeField]
    private TextMeshProUGUI finalScoreText;

    [Tooltip("Result title / message.")]
    [SerializeField]
    private TextMeshProUGUI winHighScoreText;


    // =========================================================
    // COUNTDOWN
    // =========================================================

    [Header("Countdown")]

    [SerializeField]
    private TMP_Text countdownText;


    // =========================================================
    // SCENE REFERENCES
    // =========================================================

    [Header("Scene References")]

    [SerializeField]
    private GameManager gameManager;

    [SerializeField]
    private ExerciseManager exerciseManager;

    [SerializeField]
    private PlayerMovement player;

    [SerializeField]
    private BallController ballController;

    [SerializeField]
    private SettingsManager settingsManager;

    [SerializeField]
    private GameObject kicker;

    [SerializeField]
    private ExerciseGuidanceManager exerciseGuidanceManager;


    // =========================================================
    // SCREEN FLASH
    // =========================================================

    [Header("Screen Flash")]

    [SerializeField]
    private Image flashImage;


    // =========================================================
    // STATE
    // =========================================================

    private static bool skipMenu = false;

    private Coroutine _flashCoroutine;

    private Coroutine _resumeCoroutine;


    private static readonly Color SaveColor =
        new Color(
            0f,
            1f,
            0f,
            0.45f
        );


    // =========================================================
    // START
    // =========================================================

    private void Start()
    {
        // -----------------------------------------------------
        // GUIDANCE OFF WHILE IN MENU
        // -----------------------------------------------------

        if (exerciseGuidanceManager != null)
        {
            exerciseGuidanceManager.StopGuidance();
        }


        // -----------------------------------------------------
        // NORMAL MENU
        // -----------------------------------------------------

        if (!skipMenu)
        {
            Time.timeScale = 0f;

            pausePanel?.SetActive(false);

            gameOverPanel?.SetActive(false);

            winPanel?.SetActive(false);

            countdownPanel?.SetActive(false);

            settingsPanel?.SetActive(false);

            settingsPanelFromGame?.SetActive(false);

            levelsPanel?.SetActive(false);

            // IMPORTANT:
            // This panel is the gameplay HUD.
            // It is hidden in menu and enabled when gameplay starts.
            livesPanel?.SetActive(false);

            mainMenuPanel?.SetActive(true);
        }


        // -----------------------------------------------------
        // SKIP MENU
        // -----------------------------------------------------

        else
        {
            Time.timeScale = 0f;

            pausePanel?.SetActive(false);

            gameOverPanel?.SetActive(false);

            winPanel?.SetActive(false);

            countdownPanel?.SetActive(false);

            settingsPanel?.SetActive(false);

            settingsPanelFromGame?.SetActive(false);

            levelsPanel?.SetActive(true);

            livesPanel?.SetActive(false);

            mainMenuPanel?.SetActive(false);
        }


        // -----------------------------------------------------
        // INITIAL HUD
        // -----------------------------------------------------

        UpdateExerciseHUD();


        if (streakText != null)
        {
            streakText.gameObject.SetActive(false);
        }


        // -----------------------------------------------------
        // GAME MANAGER EVENTS
        // -----------------------------------------------------

        if (gameManager != null)
        {
            gameManager.OnSaveScored +=
                HandleSaveScored;

            gameManager.OnStreakMilestone +=
                ShowStreakText;
        }


        // -----------------------------------------------------
        // EXERCISE MANAGER EVENTS
        // -----------------------------------------------------

        if (exerciseManager != null)
        {
            exerciseManager.OnExerciseCompleted +=
                HandleExerciseCompleted;
        }
    }


    // =========================================================
    // UPDATE
    // =========================================================

    private void Update()
    {
        // Update exercise HUD while the exercise is active.
        if (exerciseManager != null &&
            exerciseManager.IsRunning &&
            !exerciseManager.IsPaused)
        {
            UpdateExerciseHUD();
        }
    }


    // =========================================================
    // EXERCISE HUD
    // =========================================================

    private void UpdateExerciseHUD()
    {
        if (exerciseManager != null)
        {
            // -------------------------------------------------
            // REP
            // -------------------------------------------------

            if (repProgressText != null)
            {
                repProgressText.text =
                    "LEFT  " +
                    exerciseManager.CurrentLeftRep +
                    " / " +
                    exerciseManager.TargetRepsPerSide;
            }


            // -------------------------------------------------
            // RIGHT REP
            // -------------------------------------------------

            if (rightRepProgressText != null)
            {
                rightRepProgressText.text =
                    "RIGHT  " +
                    exerciseManager.CurrentRightRep +
                    " / " +
                    exerciseManager.TargetRepsPerSide;
            }


            // -------------------------------------------------
            // SET
            // -------------------------------------------------

            if (setProgressText != null)
            {
                setProgressText.text =
                    "SETS  " +
                    exerciseManager.CurrentSet +
                    " / " +
                    exerciseManager.TargetSets;
            }
        }


        // -----------------------------------------------------
        // SAVES
        // -----------------------------------------------------

        int saves = 0;

        if (gameManager != null)
        {
            saves = gameManager.TotalSaves;
        }


        if (score != null)
        {
            score.text =
                saves.ToString();
        }



    }


    // =========================================================
    // SAVE
    // =========================================================

    public void ScoreIncrease()
    {
        FlashScreen(
            SaveColor
        );


        if (gameManager != null)
        {
            gameManager.RegisterSave();
        }


        UpdateExerciseHUD();
    }


    // =========================================================
    // MISSED BALL
    // =========================================================
    //
    // IMPORTANT:
    // A missed ball does NOT cause game over.
    //
    // It only resets the save streak.
    //
    // =========================================================

    public void Losegoal()
    {
        if (gameManager != null)
        {
            gameManager.RegisterGoal();
        }


        if (VFXManager.instance != null)
        {
            VFXManager.instance
                .PlayGroundTouchEffect();
        }


        UpdateExerciseHUD();
    }


    // =========================================================
    // SAVE EVENT
    // =========================================================

    private void HandleSaveScored(
        int totalSaves)
    {
        UpdateExerciseHUD();
    }


    // =========================================================
    // EXERCISE COMPLETED
    // =========================================================

    private void HandleExerciseCompleted(
        int totalCompletedReps,
        int totalTargetReps,
        int completedSets,
        int targetSets)
    {
        Debug.Log(
            "[UIManager] EXERCISE COMPLETED | " +
            $"Reps: {totalCompletedReps}/{totalTargetReps} | " +
            $"Sets: {completedSets}/{targetSets} | " +
            $"Saves: {GetTotalSaves()}"
        );


        // -----------------------------------------------------
        // STOP GUIDANCE
        // -----------------------------------------------------

        if (exerciseGuidanceManager != null)
        {
            exerciseGuidanceManager.StopGuidance();
        }


        // -----------------------------------------------------
        // STOP BALL
        // -----------------------------------------------------

        if (ballController != null)
        {
            ballController.StopBall();
            ballController.enabled = false;
        }


        // -----------------------------------------------------
        // STOP PLAYER
        // -----------------------------------------------------

        if (player != null)
        {
            player.enabled = false;
        }


        // -----------------------------------------------------
        // HIDE GAMEPLAY HUD
        // -----------------------------------------------------

        livesPanel?.SetActive(false);


        // -----------------------------------------------------
        // RESULT PANEL
        // -----------------------------------------------------

        if (winScoreText != null)
        {
            winScoreText.text =
                "REPS  " +
                totalCompletedReps +
                " / " +
                totalTargetReps;
        }


        if (highScoreText != null)
        {
            highScoreText.text =
                "SETS  " +
                completedSets +
                " / " +
                targetSets;
        }


        if (finalScoreText != null)
        {
            finalScoreText.text =
                "SAVES  " +
                GetTotalSaves();
        }


        if (winHighScoreText != null)
        {
            winHighScoreText.text =
                "EXERCISE COMPLETED";
        }


        // -----------------------------------------------------
        // SHOW RESULT
        // -----------------------------------------------------

        Time.timeScale = 0f;

        winPanel?.SetActive(true);
    }


    // =========================================================
    // TOTAL SAVES
    // =========================================================

    private int GetTotalSaves()
    {
        if (gameManager == null)
        {
            return 0;
        }

        return gameManager.TotalSaves;
    }


    // =========================================================
    // DIFFICULTY
    // =========================================================

    public void Beginner()
    {
        SetDifficulty(0);
    }


    public void Intermediate()
    {
        SetDifficulty(1);
    }


    public void Difficult()
    {
        SetDifficulty(2);
    }


    // =========================================================
    // START EXERCISE
    // =========================================================

    public void SetDifficulty(
        int level)
    {
        PlayerPrefs.SetInt(
            "Level",
            level
        );


        // -----------------------------------------------------
        // RESET GAME MANAGER
        // -----------------------------------------------------

        if (gameManager != null)
        {
            gameManager.InitGame();

            gameManager.OnGameStarted?.Invoke();
        }


        // -----------------------------------------------------
        // RESET EXERCISE
        // -----------------------------------------------------

        if (exerciseManager != null)
        {
            exerciseManager.ResetExercise();
        }


        // -----------------------------------------------------
        // HIDE MENUS
        // -----------------------------------------------------

        mainMenuPanel?.SetActive(false);

        levelsPanel?.SetActive(false);

        pausePanel?.SetActive(false);

        gameOverPanel?.SetActive(false);

        winPanel?.SetActive(false);

        settingsPanel?.SetActive(false);

        settingsPanelFromGame?.SetActive(false);


        // -----------------------------------------------------
        // SHOW EXERCISE HUD
        // -----------------------------------------------------

        // IMPORTANT:
        // This replaces the old Lives UI.
        livesPanel?.SetActive(true);


        // -----------------------------------------------------
        // RESET HUD
        // -----------------------------------------------------

        if (score != null)
        {
            score.text = "0";
        }


        if (repProgressText != null)
        {
            repProgressText.text =
                "LEFT  0 / " +
                (exerciseManager != null
                    ? exerciseManager.TargetRepsPerSide
                    : 0);
        }


        if (rightRepProgressText != null)
        {
            rightRepProgressText.text =
                "RIGHT  0 / " +
                (exerciseManager != null
                    ? exerciseManager.TargetRepsPerSide
                    : 0);
        }


        if (setProgressText != null)
        {
            setProgressText.text =
                "SETS  1 / " +
                (exerciseManager != null
                    ? exerciseManager.TargetSets
                    : 0);
        }





        // -----------------------------------------------------
        // AUDIO
        // -----------------------------------------------------

        if (AudioManager.instance != null)
        {
            AudioManager.instance.StopBgm();

            AudioManager.instance
                .PlayCrowdShoutAmbience();
        }


        // -----------------------------------------------------
        // TIME
        // -----------------------------------------------------

        Time.timeScale = 1f;


        // -----------------------------------------------------
        // ENABLE PLAYER
        // -----------------------------------------------------

        if (player != null)
        {
            player.enabled = true;
        }


        // -----------------------------------------------------
        // START EXERCISE
        // -----------------------------------------------------

        if (exerciseManager != null)
        {
            exerciseManager.StartExercise();
        }


        // -----------------------------------------------------
        // START GUIDANCE
        // -----------------------------------------------------

        if (exerciseGuidanceManager != null)
        {
            exerciseGuidanceManager.StartGuidance();
        }


        // -----------------------------------------------------
        // START BALL
        // -----------------------------------------------------

        if (ballController != null)
        {
            ballController.gameObject.SetActive(true);

            ballController.enabled = true;

            ballController.ResetGame();
        }


        // -----------------------------------------------------
        // FINAL HUD UPDATE
        // -----------------------------------------------------

        UpdateExerciseHUD();
    }


    // =========================================================
    // START BUTTON
    // =========================================================

    public void StartButton()
    {
        mainMenuPanel?.SetActive(false);

        levelsPanel?.SetActive(true);
    }


    // =========================================================
    // SHOW MENU
    // =========================================================

    public void ShowMenu()
    {
        if (exerciseGuidanceManager != null)
        {
            exerciseGuidanceManager.StopGuidance();
        }


        if (exerciseManager != null)
        {
            exerciseManager.EndExercise();
        }


        if (ballController != null)
        {
            ballController.StopBall();
        }


        livesPanel?.SetActive(false);

        pausePanel?.SetActive(false);

        gameOverPanel?.SetActive(false);

        winPanel?.SetActive(false);

        mainMenuPanel?.SetActive(false);

        levelsPanel?.SetActive(true);
    }


    // =========================================================
    // QUIT
    // =========================================================

    public void QuitButton()
    {
        Application.Quit();
    }


    // =========================================================
    // PAUSE
    // =========================================================

    public void ShowPause()
    {
        if (pausePanel == null)
        {
            return;
        }


        if (exerciseManager != null)
        {
            exerciseManager.PauseExercise();
        }


        if (AudioManager.instance != null)
        {
            AudioManager.instance.DisableSource();
        }


        pausePanel.SetActive(true);

        pausePanel.transform.localScale =
            Vector3.zero;


        Time.timeScale = 0f;


        if (player != null)
        {
            player.enabled = false;
        }


        if (ballController != null)
        {
            ballController.enabled = false;
        }


        pausePanel.transform.DOKill();


        pausePanel.transform
            .DOScale(
                Vector3.one,
                0.5f
            )
            .SetEase(
                Ease.OutBack,
                3.5f
            )
            .SetUpdate(true);
    }


    // =========================================================
    // RESUME
    // =========================================================

    public void ResumeButton()
    {
        if (AudioManager.instance != null)
        {
            AudioManager.instance.EnableSource();
        }


        if (pausePanel != null)
        {
            Time.timeScale = 1f;


            pausePanel.transform.DOKill();


            pausePanel.transform
                .DOScale(
                    Vector3.zero,
                    0.35f
                )
                .SetEase(
                    Ease.InBack,
                    3.5f
                )
                .SetUpdate(true)
                .OnComplete(() =>
                {
                    pausePanel.SetActive(false);

                    if (settingsPanel != null)
                    {
                        settingsPanel.SetActive(false);
                    }

                    _resumeCoroutine =
                        StartCoroutine(
                            ResumeCountdown()
                        );
                });
        }
        else
        {
            _resumeCoroutine =
                StartCoroutine(
                    ResumeCountdown()
                );
        }
    }


    // =========================================================
    // RESTART
    // =========================================================

    public void RestartButton()
    {
        int level =
            PlayerPrefs.GetInt(
                "Level",
                0
            );


        SetDifficulty(level);
    }


    // =========================================================
    // HOME
    // =========================================================

    public void HomeButton()
    {
        if (exerciseGuidanceManager != null)
        {
            exerciseGuidanceManager.StopGuidance();
        }


        if (exerciseManager != null)
        {
            exerciseManager.EndExercise();
        }


        if (ballController != null)
        {
            ballController.StopBall();
        }


        skipMenu = false;


        SceneManager.LoadScene(
            SceneManager
                .GetActiveScene()
                .buildIndex
        );
    }


    // =========================================================
    // SETTINGS
    // =========================================================

    public void ShowSettings()
    {
        settingsPanel?.SetActive(true);
    }


    public void ShowSettingsFromGame()
    {
        settingsPanelFromGame?.SetActive(true);


        if (AudioManager.instance != null)
        {
            AudioManager.instance.DisableSource();
        }


        Time.timeScale = 0f;
    }


    // =========================================================
    // RESUME FROM SETTINGS
    // =========================================================

    public void ResumeFromSettings()
    {
        settingsPanelFromGame?.SetActive(false);


        Time.timeScale = 1f;


        if (AudioManager.instance != null)
        {
            AudioManager.instance.EnableSource();
        }


        _resumeCoroutine =
            StartCoroutine(
                ResumeCountdown()
            );
    }


    // =========================================================
    // NEW GAME
    // =========================================================

    public void NewGame()
    {
        skipMenu = true;


        SceneManager.LoadScene(
            SceneManager
                .GetActiveScene()
                .buildIndex
        );
    }


    // =========================================================
    // SCREEN FLASH
    // =========================================================

    private void FlashScreen(
        Color color)
    {
        if (flashImage == null)
        {
            return;
        }


        if (_flashCoroutine != null)
        {
            StopCoroutine(
                _flashCoroutine
            );
        }


        _flashCoroutine =
            StartCoroutine(
                FlashCoroutine(color)
            );
    }


    private IEnumerator FlashCoroutine(
        Color color)
    {
        flashImage.color =
            color;


        flashImage.gameObject
            .SetActive(true);


        float duration = 0.35f;

        float timer = 0f;


        while (timer < duration)
        {
            timer +=
                Time.unscaledDeltaTime;


            float alpha =
                Mathf.Lerp(
                    color.a,
                    0f,
                    timer / duration
                );


            flashImage.color =
                new Color(
                    color.r,
                    color.g,
                    color.b,
                    alpha
                );


            yield return null;
        }


        flashImage.gameObject
            .SetActive(false);
    }


    // =========================================================
    // STREAK
    // =========================================================

    private void ShowStreakText(
        int streak)
    {
        if (streakText == null)
        {
            return;
        }


        if (AudioManager.instance != null)
        {
            AudioManager.instance
                .PlayStreakSound();
        }


        streakText.DOKill();


        streakText.text =
            streak +
            " SAVES!";


        streakText.gameObject
            .SetActive(true);


        streakText.transform.DOKill();


        streakText.transform.localScale =
            Vector3.one;


        streakText.transform
            .DOPunchScale(
                Vector3.one * 0.4f,
                0.3f,
                6,
                0.5f
            );


        streakText
            .DOFade(
                1f,
                0f
            )
            .OnComplete(() =>
                streakText
                    .DOFade(
                        0f,
                        0.6f
                    )
                    .SetDelay(0.8f)
                    .OnComplete(() =>
                        streakText.gameObject
                            .SetActive(false)
                    )
            );
    }


    // =========================================================
    // RESUME COUNTDOWN
    // =========================================================

    private IEnumerator ResumeCountdown()
    {
        pausePanel?.SetActive(false);

        countdownPanel?.SetActive(true);


        Time.timeScale = 0f;


        if (player != null)
        {
            player.enabled = false;
        }


        if (ballController != null)
        {
            ballController.enabled = false;
        }


        string[] counts =
        {
            "3",
            "2",
            "1"
        };


        foreach (string count in counts)
        {
            if (countdownText != null)
            {
                countdownText.text =
                    count;
            }


            yield return
                new WaitForSecondsRealtime(1f);
        }


        _resumeCoroutine = null;


        countdownPanel?.SetActive(false);


        Time.timeScale = 1f;


        if (ballController != null)
        {
            ballController.enabled = true;
        }


        if (player != null)
        {
            player.enabled = true;
        }


        if (exerciseManager != null)
        {
            exerciseManager.ResumeExercise();
        }


        UpdateExerciseHUD();
    }


    // =========================================================
    // DESTROY
    // =========================================================

    private void OnDestroy()
    {
        if (gameManager != null)
        {
            gameManager.OnSaveScored -=
                HandleSaveScored;

            gameManager.OnStreakMilestone -=
                ShowStreakText;
        }


        if (exerciseManager != null)
        {
            exerciseManager.OnExerciseCompleted -=
                HandleExerciseCompleted;
        }


        if (_flashCoroutine != null)
        {
            StopCoroutine(
                _flashCoroutine
            );
        }


        if (_resumeCoroutine != null)
        {
            StopCoroutine(
                _resumeCoroutine
            );
        }


        Time.timeScale = 1f;
    }
}