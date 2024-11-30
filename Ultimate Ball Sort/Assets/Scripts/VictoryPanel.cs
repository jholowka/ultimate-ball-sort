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
        string hexColor = ColorUtility.ToHtmlStringRGB(timePassedColour);
        timeText.text = $"Time: <color=#{hexColor}>{gameManager.GetTimePassed()}</color>";
        float savedBestTime = PlayerPrefs.GetFloat("BestTime", float.MaxValue);

        string bestColour = string.Empty;
        if (gameManager.timePassed < savedBestTime)
        {
            bestColour = ColorUtility.ToHtmlStringRGB(newBestColour);
            PlayerPrefs.SetFloat("BestTime", gameManager.timePassed);
        }
        else
        {
            bestColour = ColorUtility.ToHtmlStringRGB(oldBestColour);
        }

        bestTimeText.text = $"Best Time: <color=#{bestColour}>{gameManager.GetBestTimeAsString()}</color>";
        restartButton.onClick.AddListener(() => RestartGame());
    }

    private void RestartGame()
    {
        AudioSource.PlayClipAtPoint(victorySound, Camera.main.transform.position, 1f);
        SceneManager.LoadScene(0);
    }
}
