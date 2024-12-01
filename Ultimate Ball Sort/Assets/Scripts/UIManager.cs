using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    [SerializeField] private GameManager gameManager;
    [SerializeField] private GameObject victoryMenu;
    [SerializeField] private GameObject helpMenu;
    [SerializeField] private GameObject creditsMenu;
    [SerializeField] private GameObject restartMenu;
    [SerializeField] private GameObject settingsMenu;
    [SerializeField] private GameObject confirmResetMenu;
    [field: SerializeField] public GameObject failureMenu { get; private set; }
    [SerializeField] private Button helpButton;
    [SerializeField] private Button creditsButton;
    [SerializeField] private Button restartButton;
    [SerializeField] private Button settingsButton;
    [SerializeField] private Image musicButtonImage;
    [SerializeField] private Sprite musicOnSprite, musicOffSprite;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip openSound, selectSound, closeSound, confirmSound;
    [field: SerializeField] public AudioClip survivorsReadySound { get; private set; }
    [field: SerializeField] public AudioClip thatsHowSound { get; private set; }
    [field: SerializeField] public AudioClip pullSound { get; private set; }
    [field: SerializeField] public AudioClip splashSound { get; private set; }
    private string gameModeOnOpenSettings;

    private void Start()
    {
        helpButton.onClick.AddListener(() => OpenMenu(helpMenu));
        creditsButton.onClick.AddListener(() => OpenMenu(creditsMenu));
        restartButton.onClick.AddListener(() => OpenMenu(restartMenu));
        settingsButton.onClick.AddListener(() => OpenSettingsMenu());
        victoryMenu.SetActive(false);
        helpMenu.SetActive(false);
        creditsMenu.SetActive(false);
        restartMenu.SetActive(false);
        settingsMenu.SetActive(false);
        confirmResetMenu.SetActive(false);
        failureMenu.SetActive(false);

        MusicPlayer musicPlayer = FindObjectOfType<MusicPlayer>();
        SetMusicButtonSprite(musicPlayer);
    }

    private void OpenSettingsMenu(){
        gameModeOnOpenSettings = PlayerPrefs.GetString(GameManager.KEY_GAME_MODE, GameManager.VALUE_GAME_MODE_RACED);
        OpenMenu(settingsMenu);
    }

    public void OpenMenu(GameObject menu, bool playSound=true)
    {
        menu.SetActive(true);
        if (playSound) audioSource.clip = openSound;
        if (playSound) audioSource.Play();
    }

    public void CloseSettingsMenu(){
        string gameModeNow = PlayerPrefs.GetString(GameManager.KEY_GAME_MODE, GameManager.VALUE_GAME_MODE_RACED);
        if (gameModeNow != gameModeOnOpenSettings)
        {
            SceneManager.LoadScene(0);
        }

        CloseMenu(settingsMenu);
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

    public void PlaySelectSound()
    {
        audioSource.clip = selectSound;
        audioSource.Play();
    }

    public void PlayConfirmSound(){
        audioSource.clip = confirmSound;
        audioSource.Play();
    }

    public void PlayClip(AudioClip clip){
        audioSource.clip = clip;
        audioSource.Play();
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
