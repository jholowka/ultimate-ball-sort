using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;
using System.Timers;

public class GameManager : MonoBehaviour
{
    public enum GameState { GameLoad, SettingUp, Active, Victory, Failed }
    public GameState currentGameState {get; private set;} = GameState.GameLoad;
    [SerializeField] private UIManager uiManager;
    [SerializeField] private BallSpawner[] ballSpawners;
    [SerializeField] private GameObject[] ballPrefabs;
    [Tooltip("Only spawn in multiples of 5")]
    [SerializeField] private int ballsToSpawn = 35;
    [SerializeField] private float spawnDelay = 0.25f;
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private Animator boardAnim;
    public float timePassed { get; private set; }
    private int spawnerIndex;
    private Queue<int> randomizedBallIndices;
    public int winThres { get; private set; }
    public static Action<GameState> OnGameStateChange;

    public const string KEY_BEST_TIME_RACED = "BestTime";
    public const string KEY_BEST_TIME_3M = "Timed (3m)";
    public const string KEY_BEST_TIME_2M = "Timed (2m)";
    public const string KEY_BEST_TIME_1M = "Timed (1m)";

    public const string KEY_GAME_MODE = "GameMode";
    public const string VALUE_GAME_MODE_RACED = "Raced";
    public const string VALUE_GAME_MODE_TIMED3 = "Timed (3m)";
    public const string VALUE_GAME_MODE_TIMED2 = "Timed (2m)";
    public const string VALUE_GAME_MODE_TIMED1 = "Timed (1m)";


    // Start is called before the first frame update
    void Start()
    {
        SetGameState(GameState.SettingUp);
        GenerateRandomizedBallIndices();
        StartCoroutine(SpawnBallCoroutine());
        WinDetector[] winDetectors = FindObjectsOfType<WinDetector>();
        winThres = ballsToSpawn / winDetectors.Length;
        SetTimerText();
    }

    private void SetTimerText()
    {
        string gameMode = PlayerPrefs.GetString(KEY_GAME_MODE, VALUE_GAME_MODE_RACED);
         switch (gameMode)
        {
            case VALUE_GAME_MODE_RACED:
                timerText.text = "0m 0s";
                break;

            case VALUE_GAME_MODE_TIMED3:
                timerText.text = "3m 0s";
                timePassed = 180f;
                break;

            case VALUE_GAME_MODE_TIMED2:
                timerText.text = "2m 0s";
                timePassed = 120f;
                break;

            case VALUE_GAME_MODE_TIMED1:
                timerText.text = "1m 0s";
                timePassed = 60f;
                break;
        }
    }

    private void Update()
    {
        if (currentGameState == GameState.Active)
        {
            // Run timer
            string gameMode = PlayerPrefs.GetString(KEY_GAME_MODE, VALUE_GAME_MODE_RACED);
            switch (gameMode)
            {
                case VALUE_GAME_MODE_RACED:
                    timePassed += Time.deltaTime;
                    timerText.text = GetTimePassed();
                    break;

                default:
                    timePassed -= Time.deltaTime;
                    timerText.text = GetTimePassed();
                    if (timePassed < 30f)
                    {
                        timerText.color = Color.red;
                    }

                    if (timePassed < 0f){
                        SetGameState(GameState.Failed);
                        timePassed = 0f;
                        timerText.text = "OUT OF TIME!";
                    }
                    break;
            }
        }
    }

    public string GetTimePassed()
    {
        int minutes = Mathf.FloorToInt(timePassed / 60);
        int seconds = Mathf.FloorToInt(timePassed % 60);
        return $"{minutes}m {seconds}s";
    }

    public string GetBestTimeAsString(string key)
    {
        float bestTime = PlayerPrefs.GetFloat(key, float.MaxValue);
        int minutes = Mathf.FloorToInt(bestTime / 60);
        int seconds = Mathf.FloorToInt(bestTime % 60);
        return $"{minutes}m {seconds}s";
    }

    private void GenerateRandomizedBallIndices()
    {
        // Ensure ballsToSpawn is a multiple of the number of prefabs
        if (ballsToSpawn % ballPrefabs.Length != 0)
        {
            Debug.LogError("ballsToSpawn must be a multiple of the number of ballPrefabs.");
            return;
        }

        // Calculate the number of balls per prefab
        int ballsPerPrefab = ballsToSpawn / ballPrefabs.Length;

        // Create a list with the required number of indices for each prefab
        List<int> ballIndices = new List<int>();
        for (int i = 0; i < ballPrefabs.Length; i++)
        {
            for (int j = 0; j < ballsPerPrefab; j++)
            {
                ballIndices.Add(i);
            }
        }

        // Shuffle the indices
        ShuffleList(ballIndices);

        // Store the shuffled indices in a queue for easy access
        randomizedBallIndices = new Queue<int>(ballIndices);
    }

    private void ShuffleList(List<int> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int randomIndex = UnityEngine.Random.Range(0, i + 1);
            int temp = list[i];
            list[i] = list[randomIndex];
            list[randomIndex] = temp;
        }
    }

    private IEnumerator SpawnBallCoroutine()
    {
        WaitForSeconds delay = new WaitForSeconds(spawnDelay);
        for (int i = 0; i < ballsToSpawn; i++)
        {
            SpawnNewBall();
            yield return delay;
        }

        SetGameState(GameState.Active);
    }

     private void SpawnNewBall()
    {
        if (randomizedBallIndices == null || randomizedBallIndices.Count == 0)
        {
            Debug.LogError("Randomized ball indices not initialized or empty.");
            return;
        }

        // Get the next ball index from the randomized queue
        int ballIndex = randomizedBallIndices.Dequeue();

        // Spawn the ball using the current spawner
        ballSpawners[spawnerIndex].SpawnBall(ballPrefabs[ballIndex]);

        // Cycle through the spawner indices
        spawnerIndex = (spawnerIndex + 1) % ballSpawners.Length;
    }

    #region Game State Machine
    public void SetGameState(GameState newState){
        if (newState == currentGameState) return;
        currentGameState = newState;

        switch (newState)
        {
            case GameState.Active:
                uiManager.PlayClip(uiManager.survivorsReadySound);
                break;

            case GameState.Victory:
                uiManager.PlayClip(uiManager.thatsHowSound);
                break;

            case GameState.Failed:
                StartCoroutine(DelayedFail());
                break;
        }

        OnGameStateChange?.Invoke(currentGameState);
    }

    private IEnumerator DelayedFail()
    {
        Ball[] balls = FindObjectsOfType<Ball>();
        foreach (Ball ball in balls)
        {
            ball.DisableDrag();
        }

        boardAnim.SetTrigger("Pull");
        uiManager.PlayClip(uiManager.pullSound);
        yield return new WaitForSeconds(1f);
        uiManager.PlayClip(uiManager.splashSound);
        yield return new WaitForSeconds(0.5f);
        uiManager.OpenMenu(uiManager.failureMenu, playSound:false);
    }

    private int columnsComplete;
    private void SetColumnComplete()
    {
        columnsComplete++;
        WinDetector[] winDetectors = FindObjectsOfType<WinDetector>();
        if (columnsComplete >= winDetectors.Length){
            SetGameState(GameState.Victory);
        }
    }

    private void OnEnable()
    {
        WinDetector.OnColumnComplete += SetColumnComplete;
    }

    private void OnDisable()
    {
        WinDetector.OnColumnComplete -= SetColumnComplete;
    }
    #endregion

    #region Game Mode State Machine
    public void SetGameMode(int id)
    {
        switch (id)
        {
            case 0:
                PlayerPrefs.SetString(KEY_GAME_MODE, "Raced");
                break;

            case 1:
                PlayerPrefs.SetString(KEY_GAME_MODE, "Timed (3m)");
                break;

            case 2:
                PlayerPrefs.SetString(KEY_GAME_MODE, "Timed (2m)");
                break;

            case 3:
                PlayerPrefs.SetString(KEY_GAME_MODE, "Timed (1m)");
                break;
        }
    }

    public void ConfirmScoreReset()
    {
        string gameMode = PlayerPrefs.GetString(KEY_GAME_MODE, VALUE_GAME_MODE_RACED);
        switch (gameMode)
        {
            case VALUE_GAME_MODE_RACED:
                PlayerPrefs.DeleteKey(KEY_BEST_TIME_RACED);
                break;

            case VALUE_GAME_MODE_TIMED3:
                PlayerPrefs.DeleteKey(KEY_BEST_TIME_3M);
                break;

            case VALUE_GAME_MODE_TIMED2:
                PlayerPrefs.DeleteKey(KEY_BEST_TIME_2M);
                break;

            case VALUE_GAME_MODE_TIMED1:
                PlayerPrefs.DeleteKey(KEY_BEST_TIME_1M);
                break;
        }

        uiManager.PlayConfirmSound();
    }
    #endregion
}
