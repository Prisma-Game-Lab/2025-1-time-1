using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuManager : MonoBehaviour
{
    [Header("Panels")]
    public GameObject mainMenuPanel;
    public GameObject optionsPanel;

    [Header("Options Controls")]
    public Slider musicSlider;
    public Toggle fullscreenToggle;

    void Start()
    {
        // Inicializa os controles com os valores atuais
        AudioManager.instance.PlaySound("MenuMusic");
        musicSlider.value = 0.4f; // valor inicial padrão (igual ao volume do Sound no Inspector)
        fullscreenToggle.isOn = Screen.fullScreen;

        musicSlider.onValueChanged.AddListener(SetMusicVolume);

        optionsPanel.SetActive(false);
        mainMenuPanel.SetActive(true);
    }

    // Fun��es dos bot�es do menu principal
    public void StartNewGame()
    {
        SceneManager.LoadScene(1);
    }

    public void LoadGame()
    {
        Debug.Log("Load Game clicked");
    }

    public void OpenOptions()
    {
        optionsPanel.SetActive(true);
        mainMenuPanel.SetActive(false);
    }

    public void CloseOptions()
    {
        optionsPanel.SetActive(false);
        mainMenuPanel.SetActive(true);
    }

    // Fun��es dos controles do painel op��es
    public void SetMusicVolume(float value)
    {
        AudioManager.instance.SetMusicVolume(value);
    }

    public void SetFullscreen(bool isFullscreen)
    {
        Screen.fullScreen = isFullscreen;
    }

    public void QuitGame()
    {
        Debug.Log("Quit clicked");
        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}
