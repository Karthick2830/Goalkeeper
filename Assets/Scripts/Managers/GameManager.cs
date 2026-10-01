using System;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    // =========================================================
    // SCORING
    // =========================================================

    [Header("Scoring")]

    [SerializeField]
    private int savesPerStreakTick = 3;


    // =========================================================
    // PUBLIC STATE
    // =========================================================

    public int TotalSaves { get; private set; }

    public int SaveStreak { get; private set; }

    public int BestScore { get; private set; }

    public bool IsGameStarted { get; private set; }


    // =========================================================
    // EVENTS
    // =========================================================

    public Action OnGameStarted;

    public event Action<int> OnSaveScored;

    public event Action<int> OnStreakMilestone;


    // =========================================================
    // SINGLETON
    // =========================================================

    public static GameManager instance;


    // =========================================================
    // UNITY
    // =========================================================

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;

        InitGame();
    }


    private void Start()
    {
        OnGameStarted += HandleGameStart;

        if (AudioManager.instance != null)
        {
            AudioManager.instance.PlayBgm();
        }
    }


    private void OnDestroy()
    {
        OnGameStarted -= HandleGameStart;

        if (instance == this)
        {
            instance = null;
        }
    }


    // =========================================================
    // GAME START
    // =========================================================

    public void HandleGameStart()
    {
        IsGameStarted = true;
    }


    // =========================================================
    // INITIALIZE
    // =========================================================

    public void InitGame()
    {
        TotalSaves = 0;

        SaveStreak = 0;

        BestScore =
            PlayerPrefs.GetInt(
                "HighScore",
                0
            );

        IsGameStarted = false;
    }


    // =========================================================
    // REGISTER SAVE
    // =========================================================

    public void RegisterSave()
    {
        SaveStreak++;

        TotalSaves++;


        OnSaveScored?.Invoke(
            TotalSaves
        );


        if (savesPerStreakTick > 0 &&
            SaveStreak % savesPerStreakTick == 0)
        {
            OnStreakMilestone?.Invoke(
                SaveStreak
            );
        }
    }


    // =========================================================
    // REGISTER MISSED BALL
    // =========================================================
    //
    // IMPORTANT:
    //
    // A missed ball is NOT a game over anymore.
    //
    // It only resets the current save streak.
    //
    // The exercise continues.
    //
    // =========================================================

    public void RegisterGoal()
    {
        SaveStreak = 0;
    }


    // =========================================================
    // HIGH SCORE
    // =========================================================

    public int CommitHighScore()
    {
        int previousHighScore =
            PlayerPrefs.GetInt(
                "HighScore",
                0
            );


        if (TotalSaves > previousHighScore)
        {
            PlayerPrefs.SetInt(
                "HighScore",
                TotalSaves
            );

            PlayerPrefs.Save();
        }


        BestScore =
            Mathf.Max(
                TotalSaves,
                previousHighScore
            );


        return BestScore;
    }
}