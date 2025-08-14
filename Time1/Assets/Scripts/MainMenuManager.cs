using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.EventSystems;

public class MainMenuManager : MonoBehaviour
{
    [Header("Panels")]
    public GameObject mainMenuPanel;
    public GameObject mainMenuVisuals;
    public GameObject optionsPanel;
    public GameObject creditsPanel;

    [Header("Options Controls")]
    public Slider volumeSlider;
    public Toggle fullscreenToggle;

    private float currentVolume = 1f;

    private Button[] menuButtons;
    private int selectedMenuIndex = 0;

    private Button[] optionButtons;
    private int selectedOptionIndex = 0;

    private Button[] creditButtons;
    private int selectedCreditIndex = 0;

    private float inputCooldown = 0.2f;
    private float lastInputTime = -10f;

    void Awake()
    {
        optionsPanel.SetActive(false);
        creditsPanel.SetActive(false);
        mainMenuPanel.SetActive(true);
        mainMenuVisuals.SetActive(true);

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

        InicializarPainel(mainMenuPanel, ref menuButtons, ref selectedMenuIndex);
    }

    void Start()
    {
        
    }

    void Update()
    {
        if (Time.time - lastInputTime < inputCooldown) return;

        if (mainMenuPanel.activeSelf)
        {
            NavegarPainel(menuButtons, ref selectedMenuIndex);

            if (Input.GetKeyDown(KeyCode.Space))
            {
                menuButtons[selectedMenuIndex].onClick.Invoke();
                lastInputTime = Time.time;
            }
        }
        else if (optionsPanel.activeSelf)
        {
            NavegarPainel(optionButtons, ref selectedOptionIndex);

            if (Input.GetKeyDown(KeyCode.Escape))
            {
                CloseOptions();
                lastInputTime = Time.time;
            }
        }
        else if (creditsPanel.activeSelf)
        {
            NavegarPainel(creditButtons, ref selectedCreditIndex);

            if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.Space))
            {
                AudioManager.instance.PlaySound("Click");
                CloseCredits();
                lastInputTime = Time.time;
            }
        }
    }

    void NavegarPainel(Button[] botoes, ref int selectedIndex)
    {
        if (botoes == null || botoes.Length == 0) return;

        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            selectedIndex = (selectedIndex - 1 + botoes.Length) % botoes.Length;
            AtualizarSelecao(botoes, selectedIndex);
            AudioManager.instance.PlaySound("Click");
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            selectedIndex = (selectedIndex + 1) % botoes.Length;
            AtualizarSelecao(botoes, selectedIndex);
            AudioManager.instance.PlaySound("Click");
        }
    }

    void AtualizarSelecao(Button[] botoes, int selectedIndex)
    {
        for (int i = 0; i < botoes.Length; i++)
        {
            var btn = botoes[i];
            var txt = btn.GetComponentInChildren<Text>();

            if (i == selectedIndex)
            {
                EventSystem.current.SetSelectedGameObject(btn.gameObject);
                if (txt != null) txt.color = Color.white;
            }
            else
            {
                if (txt != null) txt.color = Color.black;
            }
        }
    }

    void InicializarPainel(GameObject painel, ref Button[] botoes, ref int index)
    {
        botoes = painel.GetComponentsInChildren<Button>(true);
        index = 0;
        AtualizarSelecao(botoes, index);
    }

    public void StartNewGame()
    {
        GameManager.instance.index = 0;
        GameManager.instance.timePlayed = Time.time;
        SceneManager.LoadScene(1);
    }

    public void LoadGame()
    {
     
    }

    public void OpenOptions()
    {
        optionsPanel.SetActive(true);
        mainMenuPanel.SetActive(false);
        mainMenuVisuals.SetActive(true);
        InicializarPainel(optionsPanel, ref optionButtons, ref selectedOptionIndex);
        lastInputTime = Time.time;
    }

    public void CloseOptions()
    {
        optionsPanel.SetActive(false);
        mainMenuPanel.SetActive(true);
        mainMenuVisuals.SetActive(true);
        InicializarPainel(mainMenuPanel, ref menuButtons, ref selectedMenuIndex);
        lastInputTime = Time.time;
    }

    public void OpenCredits()
    {
        creditsPanel.SetActive(true);
        mainMenuPanel.SetActive(false);
        mainMenuVisuals.SetActive(false);
        InicializarPainel(creditsPanel, ref creditButtons, ref selectedCreditIndex);
        lastInputTime = Time.time;
    }

    public void CloseCredits()
    {
        creditsPanel.SetActive(false);
        mainMenuPanel.SetActive(true);
        mainMenuVisuals.SetActive(true);
        InicializarPainel(mainMenuPanel, ref menuButtons, ref selectedMenuIndex);
        lastInputTime = Time.time;
    }

    public void SetVolume(float volume)
    {
        currentVolume = volume;
        AudioManager.instance.SetMusicVolume(volume);
        PlayerPrefs.SetFloat("volume", volume);
        PlayerPrefs.Save();
        
    }

    public void SetFullscreen(bool isFullscreen)
    {
        Screen.fullScreen = isFullscreen;
    }

    public void QuitGame()
    {
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}