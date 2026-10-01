using System.Collections;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

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

    [SerializeField]
    private GameObject winPanel;

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
    // HUD TEXT
    // =========================================================

    [Header("HUD Text")]

    [SerializeField]
    private TextMeshProUGUI score;

    [SerializeField]
    private TextMeshProUGUI finalScoreText;

    [SerializeField]
    private TextMeshProUGUI winScoreText;

    [SerializeField]
    private TMP_Text countdownText;

    [SerializeField]
    private TextMeshProUGUI streakText;

    [SerializeField]
    private TextMeshProUGUI highScoreText;

    [SerializeField]
    private TextMeshProUGUI winHighScoreText;


    // =========================================================
    // LIVES ICONS
    // =========================================================

    [Header("Lives Icons")]

    [SerializeField]
    private GameObject ball1;

    [SerializeField]
    private GameObject ball2;

    [SerializeField]
    private GameObject ball3;


    // =========================================================
    // SCENE REFERENCES
    // =========================================================

    [Header("Scene References")]

    [SerializeField]
    private GameManager gameManager;

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

    private static bool skipMenu = false;

    private static readonly Color SaveColor =
        new Color(0f, 1f, 0f, 0.45f);

    private static readonly Color GoalColor =
        new Color(1f, 0f, 0f, 0.45f);


    // =========================================================
    // COROUTINES
    // =========================================================

    private Coroutine _flashCoroutine;

    private Coroutine _fadeOutCoroutine;

    private Coroutine _holdGameCoroutine;

    private Coroutine _resumeCoroutine;


    // =========================================================
    // START
    // =========================================================

    void Start()
    {
        // -----------------------------------------------------
        // GUIDANCE MUST NOT PLAY DURING MENU
        // -----------------------------------------------------

        exerciseGuidanceManager?.StopGuidance();


        if (!skipMenu)
        {
            Time.timeScale = 0f;

            pausePanel?.SetActive(false);
            gameOverPanel?.SetActive(false);
            winPanel?.SetActive(false);
            countdownPanel?.SetActive(false);
            settingsPanel?.SetActive(false);
            levelsPanel?.SetActive(false);
            livesPanel?.SetActive(false);

            mainMenuPanel?.SetActive(true);

            ball1?.SetActive(true);
            ball2?.SetActive(true);
            ball3?.SetActive(true);
        }
        else
        {
            Time.timeScale = 0f;

            pausePanel?.SetActive(false);
            gameOverPanel?.SetActive(false);
            winPanel?.SetActive(false);
            countdownPanel?.SetActive(false);
            settingsPanel?.SetActive(false);

            levelsPanel?.SetActive(true);

            livesPanel?.SetActive(false);

            mainMenuPanel?.SetActive(false);

            ball1?.SetActive(true);
            ball2?.SetActive(true);
            ball3?.SetActive(true);
        }


        if (score != null)
            score.text = "0";


        if (gameManager != null)
        {
            gameManager.OnSaveScored +=
                HandleSaveScored;

            gameManager.OnStreakMilestone +=
                ShowStreakText;

            gameManager.OnLiveLost +=
                HandleLiveLost;

            gameManager.OnWin +=
                HandleWin;
        }
    }


    // =========================================================
    // SCORE
    // =========================================================

    // Called by BallController when ball resets after a save.
    public void ScoreIncrease()
    {
        FlashScreen(SaveColor);

        gameManager.RegisterSave();
    }


    // Called by GoalLine when ball crosses the goal line.
    public void Losegoal()
    {
        gameManager?.RegisterGoal();

        VFXManager.instance.PlayGroundTouchEffect();
    }


    private void HandleSaveScored(
        int totalSaves)
    {
        if (score != null)
            score.text =
                totalSaves.ToString();
    }


    // =========================================================
    // LIVES
    // =========================================================

    private void HandleLiveLost(
        int livesRemaining)
    {
        if (livesRemaining == 2)
        {
            ball3?.SetActive(false);
        }
        else if (livesRemaining == 1)
        {
            ball2?.SetActive(false);
        }
        else if (livesRemaining <= 0)
        {
            ball1?.SetActive(false);

            // Stop exercise guidance immediately.
            exerciseGuidanceManager?.StopGuidance();

            ballController?.StopBall();

            if (ballController != null)
                ballController.enabled = false;

            if (player != null)
                player.enabled = false;

            _holdGameCoroutine =
                StartCoroutine(
                    HoldGame()
                );
        }
    }


    // =========================================================
    // WIN
    // =========================================================

    private void HandleWin(
        int totalSaves,
        int best)
    {
        // Stop exercise guidance.
        exerciseGuidanceManager?.StopGuidance();

        ballController?.StopBall();

        if (ballController != null)
            ballController.enabled = false;

        if (player != null)
            player.enabled = false;


        if (winScoreText != null)
        {
            winScoreText.text =
                totalSaves + " SAVES";
        }


        if (winHighScoreText != null)
        {
            winHighScoreText.text =
                "BEST: " +
                best.ToString("N0");
        }


        Time.timeScale = 0f;

        livesPanel.SetActive(false);

        winPanel?.SetActive(true);
    }


    // =========================================================
    // FADE OUT
    // =========================================================

    private IEnumerator FadeOutGameObjects()
    {
        exerciseGuidanceManager?.StopGuidance();

        ballController?.StopBall();

        if (ballController != null)
            ballController.enabled = false;

        if (player != null)
            player.enabled = false;


        GameObject[] fadeObjects = null;

        float duration = 1f;

        float timer = 0f;


        Vector3[] initialScales =
            new Vector3[fadeObjects.Length];


        for (int i = 0;
             i < fadeObjects.Length;
             i++)
        {
            if (fadeObjects[i] != null)
            {
                initialScales[i] =
                    fadeObjects[i]
                        .transform
                        .localScale;
            }
        }


        while (timer < duration)
        {
            timer +=
                Time.unscaledDeltaTime;

            float s =
                1f -
                timer / duration;


            for (int i = 0;
                 i < fadeObjects.Length;
                 i++)
            {
                if (fadeObjects[i] != null)
                {
                    fadeObjects[i]
                        .transform
                        .localScale =
                        initialScales[i] * s;
                }
            }

            yield return null;
        }


        for (int i = 0;
             i < fadeObjects.Length;
             i++)
        {
            fadeObjects[i]?.SetActive(false);
        }


        _holdGameCoroutine =
            StartCoroutine(
                HoldGame()
            );
    }


    // =========================================================
    // HOLD GAME
    // =========================================================

    private IEnumerator HoldGame()
    {
        yield return new WaitForSeconds(2f);

        AudioManager.instance.PlayGameOver();

        ShowGameOverPanel();
    }


    // =========================================================
    // GAME OVER PANEL
    // =========================================================

    private void ShowGameOverPanel()
    {
        int totalSaves =
            gameManager != null
                ? gameManager.TotalSaves
                : 0;


        int index =
            PlayerPrefs.GetInt(
                "Level",
                0
            );


        livesPanel.SetActive(false);

        Time.timeScale = 0f;


        if (finalScoreText != null)
        {
            finalScoreText.text =
                totalSaves + " SAVES";
        }


        if (highScoreText != null)
        {
            highScoreText.text =
                "TARGET: " +
                gameManager
                    .winTargets[index]
                    .ToString("N0");
        }


        gameOverPanel?.SetActive(true);
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
    // SET DIFFICULTY / ENTER GAME
    // =========================================================

    public void SetDifficulty(int level)
    {
        PlayerPrefs.SetInt(
            "Level",
            level
        );


        // -----------------------------------------------------
        // INITIALIZE GAME
        // -----------------------------------------------------

        gameManager?.InitGame();

        gameManager.OnGameStarted?.Invoke();


        // -----------------------------------------------------
        // GAME AUDIO
        // -----------------------------------------------------

        AudioManager.instance.StopBgm();

        AudioManager.instance.PlayCrowdShoutAmbience();


        // -----------------------------------------------------
        // HIDE TRANSITION PANELS
        // -----------------------------------------------------

        winPanel?.SetActive(false);
        gameOverPanel?.SetActive(false);
        levelsPanel?.SetActive(false);


        // -----------------------------------------------------
        // SHOW GAME HUD
        // -----------------------------------------------------

        livesPanel?.SetActive(true);


        if (score != null)
            score.text = "0";


        ball1?.SetActive(true);
        ball2?.SetActive(true);
        ball3?.SetActive(true);


        // -----------------------------------------------------
        // ENABLE PLAYER
        // -----------------------------------------------------

        if (player != null)
            player.enabled = true;


        // -----------------------------------------------------
        // START GAME
        // -----------------------------------------------------

        Time.timeScale = 1f;


        // -----------------------------------------------------
        // START EXERCISE GUIDANCE FIRST
        // -----------------------------------------------------
        //
        // IMPORTANT:
        //
        // This MUST happen BEFORE ResetGame().
        //
        // ResetGame() immediately starts BallWait(),
        // which fires onShotPreparing().
        //
        // -----------------------------------------------------

        exerciseGuidanceManager?.StartGuidance();


        // -----------------------------------------------------
        // ENABLE AND RESET BALL
        // -----------------------------------------------------

        if (ballController != null)
        {
            ballController.gameObject.SetActive(true);

            ballController.enabled = true;

            ballController.ResetGame();
        }
    }


    // =========================================================
    // MAIN MENU → LEVEL SELECT
    // =========================================================

    // From main menu "Play" button.
    // No scene reload needed.

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
        exerciseGuidanceManager?.StopGuidance();

        mainMenuPanel?.SetActive(false);

        levelsPanel?.SetActive(true);

        pausePanel?.SetActive(false);

        livesPanel.SetActive(false);
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
            return;


        AudioManager.instance.DisableSource();


        pausePanel.SetActive(true);

        pausePanel.transform.localScale =
            Vector3.zero;


        Time.timeScale = 0f;


        if (player != null)
            player.enabled = false;


        if (ballController != null)
            ballController.enabled = false;


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
        AudioManager.instance.EnableSource();


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

                    settingsPanel.SetActive(false);

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
        exerciseGuidanceManager?.StopGuidance();

        skipMenu = false;

        SceneManager.LoadScene(
            SceneManager
                .GetActiveScene()
                .buildIndex
        );
    }


    // =========================================================
    // RESUME FROM SETTINGS
    // =========================================================

    public void ResumeFromSettings()
    {
        settingsPanelFromGame?.SetActive(false);

        Time.timeScale = 1f;

        AudioManager.instance.EnableSource();

        StartCoroutine(
            ResumeCountdown()
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

        AudioManager.instance.DisableSource();

        Time.timeScale = 0f;
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
            return;


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


            float a =
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
                    a
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
            return;


        AudioManager.instance
            .PlayStreakSound();


        streakText.DOKill();


        streakText.text =
            streak + " SAVES!";


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
            player.enabled = false;


        if (ballController != null)
            ballController.enabled = false;


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
            ballController.enabled = true;


        if (player != null)
            player.enabled = true;
    }


    // =========================================================
    // DESTROY
    // =========================================================

    void OnDestroy()
    {
        if (gameManager != null)
        {
            gameManager.OnSaveScored -=
                HandleSaveScored;

            gameManager.OnStreakMilestone -=
                ShowStreakText;

            gameManager.OnLiveLost -=
                HandleLiveLost;

            gameManager.OnWin -=
                HandleWin;
        }


        if (_flashCoroutine != null)
            StopCoroutine(
                _flashCoroutine
            );


        if (_fadeOutCoroutine != null)
            StopCoroutine(
                _fadeOutCoroutine
            );


        if (_holdGameCoroutine != null)
            StopCoroutine(
                _holdGameCoroutine
            );


        if (_resumeCoroutine != null)
        {
            StopCoroutine(
                _resumeCoroutine
            );

            Time.timeScale = 1f;


            if (ballController != null)
                ballController.enabled = true;


            if (player != null)
                player.enabled = true;
        }
    }
}