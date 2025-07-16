using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class DialogueHandler : MonoBehaviour
{
    [SerializeField] private string nextScene;
    [SerializeField] private GameObject saveManager;
    [SerializeField] private GameObject canvas;
    [SerializeField] private GameObject sprite;
    [SerializeField] private TextMeshProUGUI dialogue;
    [SerializeField] private TextMeshProUGUI charName;
    [SerializeField] private GameObject endScreen;
    [SerializeField] private GameObject[] LPbar;
    [SerializeField] private GameObject dialogueBox;
    public GameObject options; // painel com as opções

    [SerializeField] private Button[] dialogueButtons; // Botões de opção (3)
    private TextMeshProUGUI[] buttonTexts;

    [SerializeField] private DialogueBox[] dialogues;
    public int dialogueIndex = 0;
    public int currIndex = 0;

    [Header("Item Viewer")]
    [SerializeField] private Item3DViewer itemViewer;
    [SerializeField] public GameObject itemViewerPanel;

    [Header("UI Setas")]
    [SerializeField] private GameObject setaSelecionada;  // seta para opção selecionada
    [SerializeField] private GameObject setaAvanco;       // seta para avançar diálogo

    private int selectedOptionIndex = 0;
    private Sprite lastSprite;
    private Coroutine piscandoSetaCoroutine;

    void Start()
    {
        dialogueIndex = GameManager.instance.index;
        canvas.GetComponent<Image>().sprite = dialogues[dialogueIndex].background;
        Sprite newSprite = dialogues[dialogueIndex].char_sprite;
        sprite.GetComponent<Image>().sprite = newSprite;
        lastSprite = newSprite;
        StartCoroutine(FadeInSprite());

        dialogue.text = dialogues[dialogueIndex].text;
        charName.text = dialogues[dialogueIndex].char_name;

        if (dialogues[dialogueIndex].options)
        {
            AtualizarTextoBotoes(dialogueIndex);
        }

        currIndex = dialogueIndex;
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
                Debug.LogWarning("dialogueButtons está vazio ou não atribuído!");
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
            Debug.LogError("Array 'dialogues' está vazio ou null!");
            return;
        }

        if (indexDialogo < 0 || indexDialogo >= dialogues.Length)
        {
            Debug.LogError($"Index de diálogo inválido: {indexDialogo}");
            return;
        }

        var dialogoAtual = dialogues[indexDialogo];

        if (dialogoAtual.option == null || dialogoAtual.option.Length == 0)
        {
            Debug.LogError("As opções do diálogo atual estão vazias ou null!");
            return;
        }

        // Garantindo que temos o mesmo número de botões e opções
        int quantidade = Mathf.Min(dialogueButtons.Length, dialogoAtual.option.Length);

        for (int i = 0; i < quantidade; i++)
        {
            TextMeshProUGUI textoBotao = dialogueButtons[i].GetComponentInChildren<TextMeshProUGUI>();
            if (textoBotao == null)
            {
                Debug.LogError($"Texto do botão {i} não encontrado!");
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

    void OnOptionClick(int index)
    {
        dialogueIndex = dialogues[currIndex].option[index].next;
        addPoints(index);

        if (dialogues[currIndex].hasReward && itemViewer != null && itemViewerPanel != null)
        {
            itemViewerPanel.SetActive(true);
            //itemViewer.ShowItem();
        }
    }

    public void NextDialogue()
    {
        AtualizarSetaAvanco(false);

        if (dialogueIndex < dialogues.Length)
        {
            canvas.GetComponent<Image>().sprite = dialogues[dialogueIndex].background;
            Sprite newSprite = dialogues[dialogueIndex].char_sprite;

            if (newSprite != lastSprite)
            {
                sprite.GetComponent<Image>().sprite = newSprite;
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
                AtualizarSetaAvanco(false); // Não mostra a seta de avanço enquanto opções estão abertas
                return;
            }

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
            SceneManager.LoadScene(nextScene);
        }
    }

    public void addPoints(int index)
    {
        int idPretendente = 0;
        switch (dialogues[currIndex].char_name)
        {
            case "Rebelde":
                idPretendente = 0;
                break;
            case "Nerd":
                idPretendente = 1;
                break;
            case "Ator":
                idPretendente = 2;
                break;
        }

        GameManager.instance.AddLovePoints(idPretendente, (int)dialogues[currIndex].option[index].reaction);
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
}
