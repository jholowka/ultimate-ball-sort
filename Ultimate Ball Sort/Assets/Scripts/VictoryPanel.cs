using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class VictoryPanel : MonoBehaviour
{
    [SerializeField] private GameManager gameManager;
    [SerializeField] private Button restartButton;
    [SerializeField] private TextMeshProUGUI modeText;
    [SerializeField] private TextMeshProUGUI timeText;
    [SerializeField] private TextMeshProUGUI bestTimeText;
    [SerializeField] private Color timePassedColour = Color.blue;
    [SerializeField] private Color newBestColour = Color.green;
    [SerializeField] private Color oldBestColour = Color.red;
    [SerializeField] private AudioClip victorySound;
    [SerializeField] private AudioClip restartSound;

    public void Start()
    {
        AudioSource.PlayClipAtPoint(victorySound, Camera.main.transform.position, 1f);
        string gameMode = PlayerPrefs.GetString(GameManager.KEY_GAME_MODE, GameManager.VALUE_GAME_MODE_RACED);
        modeText.text = gameMode;
        if (gameMode == GameManager.VALUE_GAME_MODE_RACED)
        {
            SetRacedBestTime();
        }
        else
        {
            SetTimedBestTime(gameMode);
        }

        restartButton.onClick.AddListener(() => RestartGame());
    }

    private void SetRacedBestTime()
    {
        string hexColor = ColorUtility.ToHtmlStringRGB(timePassedColour);
        timeText.text = $"Time Taken: <color=#{hexColor}>{gameManager.GetTimePassed()}</color>";

        float savedBestTime = PlayerPrefs.GetFloat(GameManager.KEY_BEST_TIME_RACED, float.MaxValue);

        string bestColour = string.Empty;
        if (gameManager.timePassed < savedBestTime)
        {
            bestColour = ColorUtility.ToHtmlStringRGB(newBestColour);
            PlayerPrefs.SetFloat(GameManager.KEY_BEST_TIME_RACED, gameManager.timePassed);
        }
        else
        {
            bestColour = ColorUtility.ToHtmlStringRGB(oldBestColour);
        }

        bestTimeText.text = $"Best Time: <color=#{bestColour}>{gameManager.GetBestTimeAsString(GameManager.KEY_BEST_TIME_RACED)}</color>";
    }

    private void SetTimedBestTime(string key)
    {
        string hexColor = ColorUtility.ToHtmlStringRGB(timePassedColour);
        timeText.text = $"Time Remaining: <color=#{hexColor}>{gameManager.GetTimePassed()}</color>";

        float savedBestTime = PlayerPrefs.GetFloat(key, float.MaxValue);

        string bestColour = string.Empty;
        if (gameManager.timePassed > savedBestTime)
        {
            bestColour = ColorUtility.ToHtmlStringRGB(newBestColour);
            PlayerPrefs.SetFloat(key, gameManager.timePassed);
        }
        else
        {
            bestColour = ColorUtility.ToHtmlStringRGB(oldBestColour);
        }

        bestTimeText.text = $"Best Time: <color=#{bestColour}>{gameManager.GetBestTimeAsString(key)}</color>";
    }

    private void RestartGame()
    {
        AudioSource.PlayClipAtPoint(victorySound, Camera.main.transform.position, 1f);
        SceneManager.LoadScene(0);
    }
}
