using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PauseMenu : MonoBehaviour
{
    [Header("Panels")]
    public GameObject pausePanel;
    public GameObject optionsPanel;

    [Header("Options Controls")]
    public Slider volumeSlider;
    public Toggle fullscreenToggle;

    [Header("Item Menu")]
    public RawImage[] itemIcons;
    public RawImage[] itemImages;
    public TextMeshProUGUI[] itemTexts;
    public string[] itemNames;

    private bool isPaused = false;

    void Start()
    {
        pausePanel.SetActive(false);
        optionsPanel.SetActive(false);

        float volume = AudioManager.globalVolume;
        volumeSlider.value = volume;
        fullscreenToggle.isOn = Screen.fullScreen;

        volumeSlider.onValueChanged.AddListener(SetVolume);
        fullscreenToggle.onValueChanged.AddListener(SetFullscreen);

        AudioManager.instance.SetMusicVolume(volume);
    }

    public void PauseGame()
    {
        pausePanel.SetActive(true);
        Time.timeScale = 0f;
        isPaused = true;
        string itemText;
        Texture2D texture;
        for (int i = 0; i < itemImages.Length; i++)
        {
            if (GameManager.instance.itemsAcquired[i])
            {
                string texturePath = "Textures/texture" + (i + 1);
                itemText = itemNames[i] + " - encontrado no capitulo " + (i + 1);
                texture = Resources.Load<Texture2D>(texturePath);
            }
            else
            {
                string texturePath = "Textures/textureAlt" + (i + 1);
                itemText = "?????? - encontrado no capitulo " + (i + 1);
                texture = Resources.Load<Texture2D>(texturePath);
            }
            itemIcons[i].texture = texture;
            itemImages[i].texture = texture;
            itemTexts[i].text = itemText;
        }
    }

    public void ResumeGame()
    {
        pausePanel.SetActive(false);
        optionsPanel.SetActive(false);
        Time.timeScale = 1f;
        isPaused = false;
    }

    public void QuitToMainMenu()
    {
        Time.timeScale = 1f;
        UnityEngine.SceneManagement.SceneManager.LoadScene("Menu");
    }

    public void OpenOptions()
    {
        optionsPanel.SetActive(true);
        pausePanel.SetActive(false);
    }

    public void CloseOptions()
    {
        optionsPanel.SetActive(false);
        pausePanel.SetActive(true);
    }

    public void SetVolume(float volume)
    {
        AudioManager.instance.SetMusicVolume(volume);
    }

    public void SetFullscreen(bool isFullscreen)
    {
        Screen.fullScreen = isFullscreen;
    }

    public void TogglePause()
    {
        if (isPaused)
            ResumeGame();
        else
            PauseGame();
    }

}
