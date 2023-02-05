using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class GameMode : MonoBehaviour
{
    public static GameMode Instance = null;

    public static bool IsGameOver
    {
        get
        {
            if (Instance)
            {
                return Instance.isGameOver;
            }

            return false;
        }
    }

    public static int Score
    {
        get
        {
            if (Instance)
            {
                return Instance.score;
            }

            return 0;
        }
    }

    [Header("UI")]
    [SerializeField] TextMeshProUGUI lblScore;
    [SerializeField] TextMeshProUGUI lblScoreGameOver;
    [SerializeField] CanvasGroup panelInGame;
    [SerializeField] CanvasGroup panelGameOver;

    private int score = 0;
    private bool isGameOver = false;
    private string gameOverMessageFormat;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }

        GameStart();
    }

    private void Update()
    {
        InputHandler();
    }

    private void InputHandler()
    {
        if (!isGameOver)
        {
            return;
        }

        if (Input.GetKeyDown(KeyCode.Return))
        {
            RestartGame();
        }
    }

    public void GameStart()
    {
        isGameOver = false;
        gameOverMessageFormat = lblScoreGameOver.text;
        lblScore.GetComponent<CanvasGroup>().alpha = 1.0f;
        SetScore(0);
        panelInGame.alpha = 1.0f;
        panelGameOver.alpha = 0.0f;
    }

    public void GameOver()
    {
        isGameOver = true;
        lblScore.GetComponent<CanvasGroup>().alpha = 0.0f;
        lblScoreGameOver.text = string.Format(gameOverMessageFormat, score);
        panelInGame.alpha = 0.0f;
        panelGameOver.alpha = 1.0f;
    }

    public void SetScore(int value)
    {
        score = value;
        lblScore.text = string.Format("{0}", score);
    }

    public static void IncreaseScore(int total = 1)
    {
        if (Instance)
        {
            var result = (Score + total);
            Instance.SetScore(result);
        }
    }

    public static void ForceGameOver()
    {
        if (Instance)
        {
            Instance.GameOver();
        }
    }

    public static void RestartGame()
    {
        var currentScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(currentScene.buildIndex);
    }
}

