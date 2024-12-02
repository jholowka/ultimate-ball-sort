using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ConfirmResetMenu : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI modeText;

    // Start is called before the first frame update
    void Start()
    {
        modeText.text = PlayerPrefs.GetString(GameManager.KEY_GAME_MODE, GameManager.VALUE_GAME_MODE_RACED);
    }
}
