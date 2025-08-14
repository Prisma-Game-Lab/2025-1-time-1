using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

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

    void Awake()
    {
     

        pausePanel.SetActive(false);
        optionsPanel.SetActive(false);

        volumeSlider.onValueChanged.RemoveAllListeners();

        float loadedVolume = PlayerPrefs.GetFloat("volume", 1f);
      

        if (loadedVolume == 0f)
        {
         
            loadedVolume = 1f;
        }

        volumeSlider.value = loadedVolume;
        SetVolume(loadedVolume);

        volumeSlider.onValueChanged.AddListener(SetVolume);

        fullscreenToggle.onValueChanged.RemoveAllListeners();
        fullscreenToggle.isOn = Screen.fullScreen;
        fullscreenToggle.onValueChanged.AddListener(SetFullscreen);
    }

    void Start()
    {
       
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePause();
        }
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
            int num = i;
            if (num == 0)
            {
                num = 1;
            }
            if (GameManager.instance.itemsAcquired[i])
            {
                string texturePath = "Textures/texture" + (i + 1);
                itemText = itemNames[i] + " - encontrado no capitulo " + num;
                texture = Resources.Load<Texture2D>(texturePath);
            }
            else
            {
                string texturePath = "Textures/textureAlt" + (i + 1);
                itemText = "?????? - encontrado no capitulo " + num;
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
        PlayerPrefs.SetFloat("volume", volume);
        PlayerPrefs.Save();
       
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