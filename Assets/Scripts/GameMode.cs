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
    [SerializeField] private TextMeshProUGUI lblScore;
    [SerializeField] private TextMeshProUGUI lblScoreGameOver;
    [SerializeField] private CanvasGroup panelInGame;
    [SerializeField] private CanvasGroup panelGameOver;

    [Header("Sound")]
    /*
    [SerializeField] private AudioClip sfxJump;
    [SerializeField] private AudioClip sfxShoot;
    [SerializeField] private AudioClip sfxBulletHit;
    [SerializeField] private AudioClip sfxMelee;
    [SerializeField] private AudioClip sfxDash;
    [SerializeField] private AudioClip sfxDead;
    [SerializeField] private AudioClip sfxEnemyDead;
    */
    [SerializeField] private AudioClip[] sfxLists;

    public enum SFX
    {
        Jump,
        Shoot,
        BulletHit,
        Melee,
        Dash,
        Dead,
        EnemyDead,
        Total = 8
    }

    private int score = 0;
    private bool isGameOver = false;
    private string gameOverMessageFormat;
    private AudioSource[] audioSources;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }

        audioSources = new AudioSource[30];

        for (int i = 0; i < audioSources.Length; ++i)
        {
            var source = gameObject.AddComponent<AudioSource>();
            source.playOnAwake = false;
            audioSources[i] = source;
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
        GameMode.EmitAudio(GameMode.SFX.Dead);
    }

    public void SetScore(int value)
    {
        score = value;
        lblScore.text = string.Format("{0}", score);
    }

    public void PlayAudio(SFX sfx, float volume = 1.0f)
    {
        if (sfxLists == null)
        {
            return;
        }

        var clip = sfxLists[(int)sfx];
        PlayAudio(clip, volume);
    }

    public void PlayAudio(AudioClip audioClip, float volume = 1.0f)
    {
        for (int i = 0; i < audioSources.Length; ++i)
        {
            var isBusy = audioSources[i].isPlaying;

            if (isBusy)
            {
                continue;
            }

            audioSources[i].PlayOneShot(audioClip, volume);
        }
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

    public static void EmitAudio(SFX sfx, float volume = 1.0f)
    {
        if (Instance)
        {
            Instance.PlayAudio(sfx, volume);
        }
    }
}

