using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    [SerializeField] private GameObject victoryMenu;
    [SerializeField] private GameObject helpMenu;
    [SerializeField] private GameObject creditsMenu;
    [SerializeField] private GameObject restartMenu;
    [SerializeField] private Button helpButton;
    [SerializeField] private Button creditsButton;
    [SerializeField] private Button restartButton;
    [SerializeField] private Image musicButtonImage;
    [SerializeField] private Sprite musicOnSprite, musicOffSprite;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip openSound, selectSound, closeSound;

    private void Start()
    {
        helpButton.onClick.AddListener(() => OpenMenu(helpMenu));
        creditsButton.onClick.AddListener(() => OpenMenu(creditsMenu));
        restartButton.onClick.AddListener(() => OpenMenu(restartMenu));
        victoryMenu.SetActive(false);
        helpMenu.SetActive(false);
        creditsMenu.SetActive(false);
        restartMenu.SetActive(false);

        MusicPlayer musicPlayer = FindObjectOfType<MusicPlayer>();
        SetMusicButtonSprite(musicPlayer);
    }

    public void OpenMenu(GameObject menu)
    {
        menu.SetActive(true);
        audioSource.clip = openSound;
        audioSource.Play();
    }

    public void CloseMenu(GameObject menu){
        menu.SetActive(false);
        audioSource.clip = closeSound;
        audioSource.Play();
    }

    public void RestartGame(){
        SceneManager.LoadScene(0);
    }

    public void ToggleMusic ()
    {
        MusicPlayer musicPlayer = FindObjectOfType<MusicPlayer>();
        musicPlayer.Toggle();
        SetMusicButtonSprite(musicPlayer);
    }

    private void SetMusicButtonSprite(MusicPlayer musicPlayer)
    {
        if (musicPlayer.GetComponent<AudioSource>().isPlaying)
        {
            musicButtonImage.sprite = musicOnSprite;
        }
        else
        {
            musicButtonImage.sprite = musicOffSprite;
        }
    }

    private void OnGameStateChange(GameManager.GameState state)
    {
        switch (state)
        {
            case GameManager.GameState.Victory:
                victoryMenu.SetActive(true);
                break;
        }
    }

    private void OnEnable()
    {
        GameManager.OnGameStateChange += OnGameStateChange;
    }


    private void OnDisable()
    {
        GameManager.OnGameStateChange -= OnGameStateChange;
    }
}
