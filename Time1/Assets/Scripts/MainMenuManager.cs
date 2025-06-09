using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections; // ← necessário para Coroutine

public class MainMenuManager : MonoBehaviour
{
    [Header("Panels")]
    public GameObject mainMenuPanel;
    public GameObject optionsPanel;

    [Header("Options Controls")]
    public Slider volumeSlider;
    public Toggle fullscreenToggle;

    private float currentVolume = 1f;

    void Awake()
    {
        StartCoroutine(Start());
    }

    IEnumerator Start()
    {
        yield return new WaitForSeconds(0.1f); // espera o AudioManager inicializar
        AudioManager.instance.PlaySound("MenuMusic");

        volumeSlider.value = currentVolume = 1f; // força o slider a começar no 1
        SetVolume(currentVolume);

        fullscreenToggle.isOn = Screen.fullScreen;

        optionsPanel.SetActive(false);
        mainMenuPanel.SetActive(true);
    }

    // Funções dos botões do menu principal
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

    public void SetVolume(float volume)
    {
        currentVolume = volume;
        AudioListener.volume = volume;
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
