using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections.Generic;


public class DialogueHandler : MonoBehaviour
{
    [SerializeField] private string nextScene;
    [SerializeField] private string altScene;
    [SerializeField] private GameObject saveManager;
    [SerializeField] private PauseMenu pauseMenu;
    [SerializeField] private GameObject canvas;
    [SerializeField] private GameObject sprite;
    [SerializeField] private TextMeshProUGUI dialogue;
    [SerializeField] private TextMeshProUGUI charName;
    [SerializeField] private TextMeshProUGUI[] dialogueLog;
    [SerializeField] private TextMeshProUGUI[] nameLog;
    [SerializeField] private GameObject endScreen;
    [SerializeField] private GameObject[] LPbar;
    [SerializeField] private GameObject dialogueBox;
    public GameObject options; // painel com as op��es

    [SerializeField] private Button[] dialogueButtons; // Bot�es de op��o (3)
    private TextMeshProUGUI[] buttonTexts;

    [SerializeField] private DialogueBox[] dialogues;
    private List<DialogueBox> dialoguesLog = new List<DialogueBox>();
    public int dialogueIndex = 0;
    public int currIndex = 0;

    [Header("Item Viewer")]
    [SerializeField] private Item3DViewer itemViewer;
    [SerializeField] public GameObject itemViewerPanel;

    [Header("UI Setas")]
    [SerializeField] private GameObject setaSelecionada;  // seta para op��o selecionada
    [SerializeField] private GameObject setaAvanco;       // seta para avan�ar di�logo

    private int logIndex = 0;
    private int selectedOptionIndex = 0;
    private Sprite lastSprite;
    private Coroutine piscandoSetaCoroutine;

    private Sprite lastBackground;
    private Coroutine fadeBackgroundCoroutine;
    private Image backgroundImage => canvas.GetComponent<Image>();


    void Start()
    {
        string sceneName = SceneManager.GetActiveScene().name;

        if (sceneName.StartsWith("Capitulo1"))
        {
            AudioManager.instance.PlaySound("Dia1");   
        }
        else if (sceneName.StartsWith("Capitulo2") || sceneName.StartsWith("Capitulo3"))
        {
            AudioManager.instance.PlaySound("Dia2");   
        }
        else if (sceneName.StartsWith("Capitulo4"))
        {
            AudioManager.instance.PlaySound("Baile");  
        }
        else
        {
            Debug.LogWarning("Capítulo sem música definida!");
        }
        dialogueIndex = GameManager.instance.index;
        canvas.GetComponent<Image>().sprite = dialogues[dialogueIndex].background;
        Sprite newSprite = dialogues[dialogueIndex].char_sprite;
        sprite.GetComponent<Image>().sprite = newSprite;
        lastSprite = newSprite;
        StartCoroutine(FadeInSprite());

        lastBackground = canvas.GetComponent<Image>().sprite;


        dialogue.text = dialogues[dialogueIndex].text;
        charName.text = dialogues[dialogueIndex].char_name;

        if (dialogues[dialogueIndex].options)
        {
            AtualizarTextoBotoes(dialogueIndex);
        }

        currIndex = dialogueIndex;
        dialoguesLog.Add(dialogues[dialogueIndex]);
        showLog();
        dialogueIndex = dialogues[dialogueIndex].next;

        if (itemViewerPanel != null)
        {
            itemViewerPanel.SetActive(false);
        }

        buttonTexts = new TextMeshProUGUI[dialogueButtons.Length];
        for (int i = 0; i < dialogueButtons.Length; i++)
        {
            buttonTexts[i] = dialogueButtons[i].GetComponentInChildren<TextMeshProUGUI>();
            int index = i;  // captura local para listener
            dialogueButtons[i].onClick.RemoveAllListeners();
            dialogueButtons[i].onClick.AddListener(() => OnOptionClick(index));
        }

        AtualizarSelecaoVisual();
        AtualizarSetaAvanco(true);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            pauseMenu.TogglePause();

        }

        if (options.activeSelf)
        {
            int totalOptions = dialogueButtons != null ? dialogueButtons.Length : 0;

            if (totalOptions > 0)
            {
                if (Input.GetKeyDown(KeyCode.UpArrow))
                {
                    selectedOptionIndex = (selectedOptionIndex - 1 + totalOptions) % totalOptions;
                    AtualizarSelecaoVisual();
                    AudioManager.instance.PlaySound("Click");
                }
                else if (Input.GetKeyDown(KeyCode.DownArrow))
                {
                    selectedOptionIndex = (selectedOptionIndex + 1) % totalOptions;
                    AtualizarSelecaoVisual();
                    AudioManager.instance.PlaySound("Click");
                }
                else if (Input.GetKeyDown(KeyCode.Space))
                {
                    options.SetActive(false);
                    dialogueBox.SetActive(true);
                    OnOptionClick(selectedOptionIndex);
                    AudioManager.instance.PlaySound("Click");
                    AtualizarSetaAvanco(true);
                }
            }
            else
            {
                Debug.LogWarning("dialogueButtons est� vazio ou n�o atribu�do!");
            }
        }
        else if (Input.GetKeyDown(KeyCode.Space))
        {
            AudioManager.instance.PlaySound("Avancar");
            NextDialogue();
            AtualizarSetaAvanco(true);
        }
    }


    void AtualizarTextoBotoes(int indexDialogo)
    {
        if (dialogues == null || dialogues.Length == 0)
        {
            Debug.LogError("Array 'dialogues' est� vazio ou null!");
            return;
        }

        if (indexDialogo < 0 || indexDialogo >= dialogues.Length)
        {
            Debug.LogError($"Index de di�logo inv�lido: {indexDialogo}");
            return;
        }

        var dialogoAtual = dialogues[indexDialogo];

        if (dialogoAtual.option == null || dialogoAtual.option.Length == 0)
        {
            Debug.LogError("As op��es do di�logo atual est�o vazias ou null!");
            return;
        }

        // Garantindo que temos o mesmo n�mero de bot�es e op��es
        int quantidade = Mathf.Min(dialogueButtons.Length, dialogoAtual.option.Length);

        for (int i = 0; i < quantidade; i++)
        {
            TextMeshProUGUI textoBotao = dialogueButtons[i].GetComponentInChildren<TextMeshProUGUI>();
            if (textoBotao == null)
            {
                Debug.LogError($"Texto do bot�o {i} n�o encontrado!");
                continue;
            }

            textoBotao.text = dialogoAtual.option[i].opText;
        }
    }


    void AtualizarSelecaoVisual()
    {
        Color rosa = new Color(1f, 0.4f, 0.7f, 1f);
        Color branco = Color.white;

        for (int i = 0; i < dialogueButtons.Length; i++)
        {
            var colors = dialogueButtons[i].colors;
            colors.normalColor = (i == selectedOptionIndex) ? rosa : branco;
            colors.highlightedColor = colors.normalColor;
            colors.selectedColor = colors.normalColor;
            dialogueButtons[i].colors = colors;
        }

        if (setaSelecionada != null && selectedOptionIndex >= 0 && selectedOptionIndex < dialogueButtons.Length)
        {
            setaSelecionada.SetActive(true);

            RectTransform setaRT = setaSelecionada.GetComponent<RectTransform>();
            RectTransform btnRT = dialogueButtons[selectedOptionIndex].GetComponent<RectTransform>();

            Vector3 posBotao = btnRT.localPosition;
            float deslocamentoX = -btnRT.rect.width / 2 - setaRT.rect.width / 2 - 5f; // igual no BattleManager

            setaRT.localPosition = new Vector3(posBotao.x + deslocamentoX, posBotao.y, posBotao.z);
        }
        else if (setaSelecionada != null)
        {
            setaSelecionada.SetActive(false);
        }
    }

    void AtualizarSetaAvanco(bool ativar)
    {
        if (setaAvanco == null) return;

        if (ativar)
        {
            setaAvanco.SetActive(true);

            if (piscandoSetaCoroutine != null)
                StopCoroutine(piscandoSetaCoroutine);

            piscandoSetaCoroutine = StartCoroutine(PiscandoSeta());
        }
        else
        {
            setaAvanco.SetActive(false);
            if (piscandoSetaCoroutine != null)
            {
                StopCoroutine(piscandoSetaCoroutine);
                piscandoSetaCoroutine = null;
            }
        }
    }

    IEnumerator PiscandoSeta()
    {
        Image setaImg = setaAvanco.GetComponent<Image>();
        while (setaAvanco.activeSelf)
        {
            setaImg.color = new Color(1, 1, 1, 1);
            yield return new WaitForSeconds(0.5f);
            setaImg.color = new Color(1, 1, 1, 0);
            yield return new WaitForSeconds(0.5f);
        }
    }

    void FinalOption(int index)
    {
        switch (index)
        {
            case 0:
                if (GameManager.instance.lovePoints[0] > 0)
                {
                    SceneManager.LoadScene("Capitulo3.R");
                }
                SceneManager.LoadScene("Capitulo3.R.1");
                break;
            case 1:
                if (GameManager.instance.lovePoints[1] > 0)
                {
                    SceneManager.LoadScene("Capitulo3.H");
                }
                SceneManager.LoadScene("Capitulo3.H.1");
                break;
            case 2:
                if (GameManager.instance.lovePoints[2] > 0)
                {
                    SceneManager.LoadScene("Capitulo3.A");
                }
                SceneManager.LoadScene("Capitulo3.A.1");
                break;
        }
    }

    void OnOptionClick(int index)
    {
        if (dialogues[currIndex].final_option)
        {
            FinalOption(index);
        }
        dialogueIndex = dialogues[currIndex].option[index].next;
        dialogues[currIndex].text = dialogues[currIndex].option[index].opText;
        addPoints(index);
        dialogues[currIndex].char_name = "Anna";
        dialoguesLog.Add(dialogues[currIndex]);

        if (dialogues[currIndex].hasReward && itemViewer != null && itemViewerPanel != null)
        {
            itemViewerPanel.SetActive(true);
            //itemViewer.ShowItem();
        }
    }

    public void NextDialogue()
    {
        AtualizarSetaAvanco(false);

        if (dialogueIndex < 0)
        {
            LevelLoader levelLoader = FindObjectOfType<LevelLoader>();
            if (levelLoader != null)
            {
                levelLoader.LoadScene(altScene);
            }
            else
            {
                SceneManager.LoadScene(altScene);
            }
        }
        if (dialogueIndex < dialogues.Length)
        {
            Sprite newBackground = dialogues[dialogueIndex].background;
            bool fadeBG = dialogues[dialogueIndex].fadeInBackground;

            if (newBackground != lastBackground)
            {
                backgroundImage.sprite = newBackground;

                if (fadeBG)
                {
                    if (fadeBackgroundCoroutine != null)
                        StopCoroutine(fadeBackgroundCoroutine);

                    fadeBackgroundCoroutine = StartCoroutine(FadeInBackground());
                }

                lastBackground = newBackground;
            }
            else
            {
                backgroundImage.sprite = newBackground;
            }

            Sprite newSprite = dialogues[dialogueIndex].char_sprite;

            bool vaiDarFade = dialogues[dialogueIndex].fadeInCharacter;

            if (newSprite != lastSprite)
            {
                sprite.GetComponent<Image>().sprite = newSprite;

                if (vaiDarFade)
                    StartCoroutine(FadeInSprite());

                lastSprite = newSprite;
            }
            else
            {
                sprite.GetComponent<Image>().sprite = newSprite;
            }

            dialogue.text = dialogues[dialogueIndex].text;
            charName.text = dialogues[dialogueIndex].char_name;
            currIndex = dialogueIndex;

            if (dialogues[dialogueIndex].options)
            {
                AtualizarTextoBotoes(dialogueIndex);
                options.SetActive(true);
                dialogueBox.SetActive(false);
                selectedOptionIndex = 0;
                AtualizarSelecaoVisual();
                AtualizarSetaAvanco(false); // N�o mostra a seta de avan�o enquanto op��es est�o abertas
                return;
            }
            dialoguesLog.Add(dialogues[dialogueIndex]);
            if ((dialoguesLog.Count % 3 == 1) && (dialoguesLog.Count != 1))
            {
                logIndex += 1;
            }

            showLog();

            if (dialogues[dialogueIndex].chapter_end)
            {
                endScreen.SetActive(true);
                for (int i = 0; i < 3; ++i)
                {
                    int lp = GameManager.instance.lovePoints[i];
                    RectTransform bar = LPbar[i].GetComponent<RectTransform>();
                    if (lp > 0)
                    {
                        LPbar[i].GetComponent<RectTransform>().sizeDelta = new Vector2((bar.sizeDelta.x * lp / 10), bar.sizeDelta.y);
                    }
                    else
                    {
                        LPbar[i].GetComponent<RectTransform>().sizeDelta = new Vector2(0, bar.sizeDelta.y);
                    }
                }
                saveManager.GetComponent<SaveManager>().chapterEnd = true;
            }

            dialogueIndex = dialogues[dialogueIndex].next;
        }
        else
        {
            LevelLoader levelLoader = FindObjectOfType<LevelLoader>();
            if (levelLoader != null)
            {
                levelLoader.LoadScene(nextScene);
            }
            else
            {
                SceneManager.LoadScene(nextScene);
            }
        }
    }

    public void selectOption(int index)
    {
        options.SetActive(false);
        dialogueBox.SetActive(true);
        selectedOptionIndex = index;
        OnOptionClick(selectedOptionIndex);
        AudioManager.instance.PlaySound("Click");
        AtualizarSetaAvanco(true);
        NextDialogue();
    }

    private void showLog()
    {
        for (int i = 0; i < dialogueLog.Length; ++i)
        {
            int index = i + (logIndex * 3);
            if (index < dialoguesLog.Count)
            {
                dialogueLog[i].text = dialoguesLog[index].text;
                nameLog[i].text = dialoguesLog[index].char_name;
            }
            else
            {
                dialogueLog[i].text = "";
                nameLog[i].text = "";
            }
        }
    }

    public void addPoints(int index)
    {
        int idPretendente = 0;
        switch (dialogues[currIndex].char_name)
        {
            case "Rebeca":
                idPretendente = 0;
                break;
            case "Henrique":
                idPretendente = 1;
                break;
            case "Antoine":
                idPretendente = 2;
                break;
        }

        GameManager.instance.AddLovePoints(idPretendente, (int)dialogues[currIndex].option[index].reaction);
    }

    public void updateLog(int qtd)
    {
        logIndex += qtd;
        if (logIndex < 0)
        {
            logIndex = 0;
            return;
        }
        if ((logIndex * 3) > (dialoguesLog.Count -1))
        {
            logIndex -= 1;
            return;
        }
        showLog();
    }

    public void loadNextScene(string scene)
    {
        SceneManager.LoadScene(scene);
    }

    IEnumerator FadeInSprite(float duration = 0.75f)
    {
        if (sprite == null) yield break;

        CanvasGroup cg = sprite.GetComponent<CanvasGroup>();
        if (cg == null)
            cg = sprite.AddComponent<CanvasGroup>();

        cg.alpha = 0f;

        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            cg.alpha = Mathf.Lerp(0f, 1f, t / duration);
            yield return null;
        }

        cg.alpha = 1f;
    }

    IEnumerator FadeInBackground(float duration = 0.75f)
    {
        CanvasGroup cg = canvas.GetComponent<CanvasGroup>();
        if (cg == null)
            cg = canvas.AddComponent<CanvasGroup>();

        cg.alpha = 0f;
        float t = 0f;

        while (t < duration)
        {
            t += Time.deltaTime;
            cg.alpha = Mathf.Lerp(0f, 1f, t / duration);
            yield return null;
        }

        cg.alpha = 1f;
    }

}
