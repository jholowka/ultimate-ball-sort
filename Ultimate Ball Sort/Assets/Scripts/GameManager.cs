using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;

public class GameManager : MonoBehaviour
{
    public enum GameState {GameLoad, SettingUp, Active, Victory}
    public GameState currentGameState {get; private set;} = GameState.GameLoad;
    [SerializeField] private BallSpawner[] ballSpawners;
    [SerializeField] private GameObject[] ballPrefabs;
    [Tooltip("Only spawn in multiples of 5")]
    [SerializeField] private int ballsToSpawn = 35;
    [SerializeField] private float spawnDelay = 0.25f;
    [SerializeField] private TextMeshProUGUI timerText;
    public float timePassed { get; private set; }
    private int spawnerIndex;
    private Queue<int> randomizedBallIndices;
    public int winThres { get; private set; }
    public static Action<GameState> OnGameStateChange;


    // Start is called before the first frame update
    void Start()
    {
        timerText.text = "0m 0s";
        SetGameState(GameState.SettingUp);
        GenerateRandomizedBallIndices();
        StartCoroutine(SpawnBallCoroutine());
        WinDetector[] winDetectors = FindObjectsOfType<WinDetector>();
        winThres = ballsToSpawn / winDetectors.Length;
    }

    private void Update()
    {
        if (currentGameState == GameState.Active)
        {
            // Run timer
            timePassed += Time.deltaTime;
            timerText.text = GetTimePassed();
        }
    }

    public string GetTimePassed()
    {
        int minutes = Mathf.FloorToInt(timePassed / 60);
        int seconds = Mathf.FloorToInt(timePassed % 60);
        return $"{minutes}m {seconds}s";
    }

    public string GetBestTimeAsString()
    {
        float bestTime = PlayerPrefs.GetFloat("BestTime", float.MaxValue);
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

    #region State Machine
    public void SetGameState(GameState newState){
        if (newState == currentGameState) return;
        currentGameState = newState;

        OnGameStateChange?.Invoke(currentGameState);
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
}
