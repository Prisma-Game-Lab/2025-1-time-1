using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class IconData
{
    public GameObject gameObject;
    public Vector3 direction;
    public float speed;

    public IconData(GameObject go, Vector3 dir, float spd)
    {
        gameObject = go;
        direction = dir.normalized;
        speed = spd;
    }
}

public enum TipoFala
{
    positivo,
    neutro,
    negativo
}

[System.Serializable]
public class DialogOption
{
    public TipoFala tipo;     
    [TextArea(2, 4)]
    public string fala;
    [TextArea(2, 6)]
    public string resposta;
}


public class BattleManager : MonoBehaviour
{
    [SerializeField] private string winScene;
    [SerializeField] private Camera mainCamera;
    [SerializeField] private GameObject setaSelecionada;
    [SerializeField] private GameObject setaAvanco;
    [SerializeField] private GameObject dialogueBox;
    [SerializeField] private PauseMenu pauseMenu;

    public Image playerHPFillImage;
    public Image enemyHPFillImage;

    public TextMeshProUGUI feedbackText;
    public GameObject dialogueButtonsPanel;
    public Button[] dialogueButtons;

    [Header("Timer UI")]
    [SerializeField] private GameObject timerBarContainer;
    [SerializeField] private Image leftBarFill;
    [SerializeField] private Image rightBarFill;

    private Coroutine turnTimerCoroutine;
    private bool escolhaFeita = false;

    [Header("Reward System")]
    [SerializeField] private int itemNumber;
    [SerializeField] private Item3DViewer itemViewer;
    [SerializeField] private GameObject itemViewerPanel;
    [SerializeField] private Transform itemPrefab;
    [SerializeField] private Camera itemViewerCamera;

    private TextMeshProUGUI[] buttonTexts;

    private int playerHP = 100;
    private int enemyHP = 100;

    private int turn = 1;
    private const int maxTurns = 4;

    private bool opponentVulnerable = false;

    private string tipoEscolhido;
    private string falaEscolhida;

    private Dictionary<string, string> opcoesAtuais = new Dictionary<string, string>();
    private Dictionary<string, string> respostasDasFalas = new Dictionary<string, string>();

    [Header("Battle Filter")]
    public Image filterBackgroundImage;
    public GameObject iconPrefab;
    public Transform iconsContainer;

    public Sprite[] nerdIcons;
    public Sprite[] rebelIcons;
    public Sprite[] actorIcons;

    private List<IconData> activeIcons = new List<IconData>();

    private enum Pretendente { Nerd, Rebelde, Ator }
    private Pretendente pretendenteAtual;

    private int botaoSelecionado = 0;
    private bool aguardandoAvanco = false;
    private bool avancarTexto = false;
    private Coroutine piscandoSetaCoroutine;


    [Header("Opções de Diálogo")]
    [SerializeField]
    private List<DialogOption> opcoesDeDialogo = new List<DialogOption>();

    private HashSet<string> usadasPositivo = new HashSet<string>();
    private HashSet<string> usadasNeutro = new HashSet<string>();
    private HashSet<string> usadasNegativo = new HashSet<string>();

    void Start()
    {
        AudioManager.instance.StopSound("MenuMusic");
        AudioManager.instance.PlaySound("BattleMusic");

        dialogueBox.SetActive(true);
        dialogueButtonsPanel.SetActive(false);

        AtualizarHP();
        StartCoroutine(IniciarPrimeiraFala());

        if (itemViewerPanel != null)
        {
            itemViewerPanel.SetActive(false);
            itemViewerCamera.enabled = false;
        }

        pretendenteAtual = Pretendente.Rebelde;
        AdicionarIcones(5);
    }

    void Update()
    {
        AtualizarIcones();


        DetectarEntradaDeTeclado();

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Debug.Log("ESC pressionado - tentando pausar");
            pauseMenu.TogglePause();
        }

    }

    void DetectarEntradaDeTeclado()
    {
        if (dialogueButtonsPanel.activeSelf && !escolhaFeita)
        {
            if (Input.GetKeyDown(KeyCode.UpArrow))
            {
                botaoSelecionado = (botaoSelecionado - 1 + dialogueButtons.Length) % dialogueButtons.Length;
                AtualizarSelecaoVisual();
                AudioManager.instance.PlaySound("Click");
            }
            else if (Input.GetKeyDown(KeyCode.DownArrow))
            {
                botaoSelecionado = (botaoSelecionado + 1) % dialogueButtons.Length;
                AtualizarSelecaoVisual();
                AudioManager.instance.PlaySound("Click");
            }
            else if (Input.GetKeyDown(KeyCode.Space))
            {
                Escolher(botaoSelecionado);
            }
        }

        if (aguardandoAvanco && (Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0)))
        {
            avancarTexto = true;
        }
    }

    void AtualizarSelecaoVisual()
    {
        for (int i = 0; i < dialogueButtons.Length; i++)
        {
            ColorBlock colors = dialogueButtons[i].colors;
            colors.normalColor = (i == botaoSelecionado) ? new Color(1f, 0.4f, 0.7f, 1f) : Color.white;
            dialogueButtons[i].colors = colors;
        }

        if (setaSelecionada != null && botaoSelecionado >= 0 && botaoSelecionado < dialogueButtons.Length)
        {
            setaSelecionada.SetActive(true);

            RectTransform setaRT = setaSelecionada.GetComponent<RectTransform>();
            RectTransform btnRT = dialogueButtons[botaoSelecionado].GetComponent<RectTransform>();

            Vector3 posBotao = btnRT.localPosition;
            float deslocamentoX = -btnRT.rect.width / 2 - setaRT.rect.width / 2 - 5f; 

            setaRT.localPosition = new Vector3(posBotao.x + deslocamentoX, posBotao.y, posBotao.z);
        }
        else if (setaSelecionada != null)
        {
            setaSelecionada.SetActive(false);
        }
    }



    private void AdicionarIcones(int quantidade)
    {
        Sprite[] iconsArray = null;
        Color cor = Color.clear;

        switch (pretendenteAtual)
        {
            case Pretendente.Nerd:
                cor = new Color(1f, 0.5f, 0f, 0.25f);
                iconsArray = nerdIcons;
                break;
            case Pretendente.Rebelde:
                cor = new Color(0.8039f, 0.2392f, 0.6039f, 0.25f);
                iconsArray = rebelIcons;
                break;
            case Pretendente.Ator:
                cor = new Color(0.6f, 0f, 1f, 0.25f);
                iconsArray = actorIcons;
                break;
        }

        filterBackgroundImage.color = cor;

        float minDistance = 100f; // Distância mínima entre ícones
        int tentativasMax = 10;

        for (int i = 0; i < quantidade; i++)
        {
            GameObject iconGO = Instantiate(iconPrefab, iconsContainer);
            Image iconImage = iconGO.GetComponent<Image>();
            iconImage.sprite = iconsArray[Random.Range(0, iconsArray.Length)];

            Vector3 posicao = Vector3.zero;
            bool posicaoValida = false;

            for (int tentativa = 0; tentativa < tentativasMax && !posicaoValida; tentativa++)
            {
                float startX = Random.Range(-800f, 800f);
                float startY = -600f - Random.Range(0f, 400f);
                posicao = new Vector3(startX, startY, 0f);

                posicaoValida = true;
                foreach (var icone in activeIcons)
                {
                    if (Vector3.Distance(icone.gameObject.transform.localPosition, posicao) < minDistance)
                    {
                        posicaoValida = false;
                        break;
                    }
                }
            }

            iconGO.transform.localPosition = posicao;

            Vector3 direction = new Vector3(Random.Range(-0.1f, 0.1f), 1f, 0f).normalized;
            float speed = Random.Range(80f, 120f);

            activeIcons.Add(new IconData(iconGO, direction, speed));
        }
    }

    private void AtualizarIcones()
    {
        foreach (var icon in activeIcons)
        {
            icon.gameObject.transform.localPosition += icon.direction * icon.speed * Time.deltaTime;

            Vector3 pos = icon.gameObject.transform.localPosition;

            if (pos.y > 700f)
            {
                pos.y = -600f - Random.Range(0f, 400f);
                pos.x = Random.Range(-800f, 800f);
                icon.gameObject.transform.localPosition = pos;
            }
        }
    }

    void RegistrarBotoes()
    {
        buttonTexts = new TextMeshProUGUI[dialogueButtons.Length];
        for (int i = 0; i < dialogueButtons.Length; i++)
        {
            int index = i;
            buttonTexts[i] = dialogueButtons[i].GetComponentInChildren<TextMeshProUGUI>();
            dialogueButtons[i].onClick.AddListener(() => Escolher(index));
        }
    }

    private DialogOption EscolherOpcaoAleatoriaPorTipo(TipoFala tipo)
    {
        List<DialogOption> disponiveis = new List<DialogOption>();

        foreach (var opcao in opcoesDeDialogo)
        {
            if (opcao.tipo == tipo)
            {
                bool jaUsada = false;
                switch (tipo)
                {
                    case TipoFala.positivo:
                        jaUsada = usadasPositivo.Contains(opcao.fala);
                        break;
                    case TipoFala.neutro:
                        jaUsada = usadasNeutro.Contains(opcao.fala);
                        break;
                    case TipoFala.negativo:
                        jaUsada = usadasNegativo.Contains(opcao.fala);
                        break;
                }

                if (!jaUsada)
                    disponiveis.Add(opcao);
            }
        }

        if (disponiveis.Count == 0)
        {
         
            switch (tipo)
            {
                case TipoFala.positivo:
                    usadasPositivo.Clear();
                    break;
                case TipoFala.neutro:
                    usadasNeutro.Clear();
                    break;
                case TipoFala.negativo:
                    usadasNegativo.Clear();
                    break;
            }
            
            foreach (var opcao in opcoesDeDialogo)
            {
                if (opcao.tipo == tipo)
                    disponiveis.Add(opcao);
            }
        }

        if (disponiveis.Count == 0)
        {
            Debug.LogWarning($"Sem opções para tipo {tipo}");
            return null;
        }

        int indexSorteado = Random.Range(0, disponiveis.Count);
        var escolha = disponiveis[indexSorteado];

        switch (tipo)
        {
            case TipoFala.positivo:
                usadasPositivo.Add(escolha.fala);
                break;
            case TipoFala.neutro:
                usadasNeutro.Add(escolha.fala);
                break;
            case TipoFala.negativo:
                usadasNegativo.Add(escolha.fala);
                break;
        }

        return escolha;
    }


    void Escolher(int index)
    {
        if (escolhaFeita) return;

        AudioManager.instance.PlaySound("Click");

        escolhaFeita = true;

        string textoBotao = buttonTexts[index].text;

        foreach (var kvp in opcoesAtuais)
        {
            if (kvp.Value == textoBotao)
            {
                tipoEscolhido = kvp.Key;
                falaEscolhida = kvp.Value;
                break;
            }
        }

        dialogueButtonsPanel.SetActive(false);
        dialogueBox.SetActive(true);

        StartCoroutine(ExecutarTurno(tipoEscolhido));
    }

    IEnumerator ExecutarTurno(string tipo)
    {
        feedbackText.text = falaEscolhida;
        yield return EsperarAvanco();

        string resposta = ObterRespostaDoOponente(falaEscolhida);

        feedbackText.text += "\n" + resposta;
        yield return EsperarAvanco();

        feedbackText.text = "";
        yield return new WaitForSeconds(0.2f);

        string efeitoTexto = "";

        if (tipo == "positivo")
        {
            int dano = opponentVulnerable ? 50 : 35;
            enemyHP -= dano;
            efeitoTexto = $"Foi uma fala positiva! Causou {dano} de dano ao oponente.";
            opponentVulnerable = false;
        }
        else if (tipo == "neutro")
        {
            int autoDano = 10;
            playerHP -= autoDano;
            efeitoTexto = $"Foi uma fala neutra. Você sofreu {autoDano} de dano por hesitação.";
        }
        else if (tipo == "negativo")
        {
            int danoRecebido = 30;
            playerHP -= danoRecebido;
            efeitoTexto = $"Foi uma fala negativa! Você levou {danoRecebido} de dano no mini combo do oponente.";
        }

        AtualizarHP();

        AdicionarIcones(3);

        feedbackText.text = efeitoTexto;
        yield return EsperarAvanco();

        string[] reacoes = { "atacar", "neutro", "esquisita" };
        string reacao = reacoes[Random.Range(0, reacoes.Length)];

        string textoReacao = "";

        if (reacao == "atacar")
        {
            int dano = 25;
            playerHP -= dano;
            textoReacao = $"O oponente contra-atacou! Você perdeu {dano} de vida.";
            opponentVulnerable = false;
        }
        else if (reacao == "neutro")
        {
            textoReacao = "O oponente ficou em silêncio...";
            opponentVulnerable = false;
        }
        else if (reacao == "esquisita")
        {
            textoReacao = "O oponente teve uma reação esquisita... parece vulnerável!";
            opponentVulnerable = true;
        }

        AtualizarHP();

        feedbackText.text += "\n" + textoReacao;
        yield return EsperarAvanco();

        if (enemyHP <= 0 || playerHP <= 0)
        {
            FinalizarBatalhaPorTurno();
            yield break;
        }

        turn++;

        if (turn > maxTurns)
        {
            yield return new WaitForSeconds(1f);
            FinalizarBatalhaPorTurno();
            yield break;
        }

        AtualizarFalasNosBotoes();
        feedbackText.text = "Seu turno! Escolha uma nova opção.";
        dialogueButtonsPanel.SetActive(true);
    }

    IEnumerator EsperarAvanco()
    {
        aguardandoAvanco = true;
        avancarTexto = false;

        if (setaAvanco != null)
        {
            setaAvanco.SetActive(true);

            if (piscandoSetaCoroutine != null)
                StopCoroutine(piscandoSetaCoroutine);

            piscandoSetaCoroutine = StartCoroutine(PiscandoSeta());
        }

        while (!avancarTexto)
            yield return null;

        AudioManager.instance.PlaySound("Avancar");

        if (setaAvanco != null)
        {
            setaAvanco.SetActive(false);

            if (piscandoSetaCoroutine != null)
            {
                StopCoroutine(piscandoSetaCoroutine);
                piscandoSetaCoroutine = null;
            }
        }

        aguardandoAvanco = false;
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


    string ObterRespostaDoOponente(string fala)
    {
        if (!string.IsNullOrEmpty(fala) && respostasDasFalas.ContainsKey(fala))
            return respostasDasFalas[fala];
        return "...";
    }



    void AtualizarFalasNosBotoes()
    {
        dialogueBox.SetActive(false);
        dialogueButtonsPanel.SetActive(true);

        opcoesAtuais.Clear();
        respostasDasFalas.Clear();

        List<DialogOption> selecionadas = new List<DialogOption>();

        var pos = EscolherOpcaoAleatoriaPorTipo(TipoFala.positivo);
        var neu = EscolherOpcaoAleatoriaPorTipo(TipoFala.neutro);
        var neg = EscolherOpcaoAleatoriaPorTipo(TipoFala.negativo);

        if (pos != null) selecionadas.Add(pos);
        if (neu != null) selecionadas.Add(neu);
        if (neg != null) selecionadas.Add(neg);

        for (int i = 0; i < selecionadas.Count; i++)
        {
            var temp = selecionadas[i];
            int rand = Random.Range(i, selecionadas.Count);
            selecionadas[i] = selecionadas[rand];
            selecionadas[rand] = temp;
        }

        int count = Mathf.Min(dialogueButtons.Length, selecionadas.Count);

        for (int i = 0; i < count; i++)
        {
            buttonTexts[i].text = selecionadas[i].fala;
            opcoesAtuais[selecionadas[i].tipo.ToString()] = selecionadas[i].fala;
            respostasDasFalas[selecionadas[i].fala] = selecionadas[i].resposta;
        }

        if (turnTimerCoroutine != null)
            StopCoroutine(turnTimerCoroutine);
        turnTimerCoroutine = StartCoroutine(IniciarContagemRegressiva());
    }



    IEnumerator IniciarContagemRegressiva()
    {
        float duracao = 10f;
        float tempoRestante = duracao;
        escolhaFeita = false;

        timerBarContainer.SetActive(true);

        while (tempoRestante > 0f)
        {
            if (escolhaFeita)
            {
                leftBarFill.fillAmount = 0f;
                rightBarFill.fillAmount = 0f;
                timerBarContainer.SetActive(false);
                yield break;
            }

            tempoRestante -= Time.deltaTime;
            float progress = tempoRestante / duracao;

            leftBarFill.fillAmount = progress;
            rightBarFill.fillAmount = progress;

            yield return null;
        }

        tipoEscolhido = "neutro";
        falaEscolhida = "Você ficou em silêncio...";
        dialogueButtonsPanel.SetActive(false);
        dialogueBox.SetActive(true);


        leftBarFill.fillAmount = 0f;
        rightBarFill.fillAmount = 0f;
        timerBarContainer.SetActive(false);

        StartCoroutine(ExecutarTurno(tipoEscolhido));
    }

    IEnumerator IniciarPrimeiraFala()
    {
        feedbackText.text = "A batalha começou!";
        yield return EsperarAvanco();

        dialogueBox.SetActive(false);
        dialogueButtonsPanel.SetActive(true);

        RegistrarBotoes();
        AtualizarFalasNosBotoes();

        botaoSelecionado = 0;
        AtualizarSelecaoVisual();
    }

    (string, string) RemoverParDeFala(Dictionary<string, string> dict)
    {
        if (dict.Count == 0) return ("???", "???");
        int index = Random.Range(0, dict.Count);
        var enumerator = dict.GetEnumerator();
        for (int i = 0; i <= index; i++) enumerator.MoveNext();

        string fala = enumerator.Current.Key;
        string resposta = enumerator.Current.Value;
        dict.Remove(fala);
        return (fala, resposta);
    }


    void AtualizarHP()
    {
        playerHP = Mathf.Max(0, playerHP);
        enemyHP = Mathf.Max(0, enemyHP);

        float playerFill = (float)playerHP / 100f;
        float enemyFill = (float)enemyHP / 100f;

        playerHPFillImage.fillAmount = playerFill;
        enemyHPFillImage.fillAmount = enemyFill;
    }

    void FinalizarBatalhaPorTurno()
    {
        dialogueButtonsPanel.SetActive(false);

        if (playerHP <= 0)
        {
            feedbackText.text = "Você perdeu a batalha!";
            StartCoroutine(ReturnToMenuAfterDelay());
        }
        else if (enemyHP <= 0)
        {
            feedbackText.text = "Você venceu a batalha!";
            ShowRewardItem();
        }
        else if (playerHP > enemyHP)
        {
            enemyHP = 0;
            AtualizarHP();
            feedbackText.text = "Você venceu a batalha!";
            ShowRewardItem();
        }
        else
        {
            playerHP = 0;
            AtualizarHP();
            feedbackText.text = "Você perdeu a batalha!";
            StartCoroutine(ReturnToMenuAfterDelay());
        }
    }

    void ShowRewardItem()
    {
        if (itemViewer != null && itemPrefab != null && itemViewerPanel != null)
        {
            GameManager.instance.itemsAcquired[itemNumber - 1] = true;
            itemViewerPanel.SetActive(true);
            itemViewer.gameObject.SetActive(true);
            mainCamera.enabled = false;
            itemViewerCamera.enabled = true;
            itemViewer.onViewFinished.RemoveAllListeners();
            itemViewer.onViewFinished.AddListener(OnRewardViewFinished);
            itemViewer.ShowItem();
        }
        else
        {
            Debug.LogWarning($"Missing references - itemViewer: {itemViewer}, itemPrefab: {itemPrefab}, itemViewerPanel: {itemViewerPanel}");
            StartCoroutine(ReturnToMenuAfterDelay());
        }
    }

    void OnRewardViewFinished()
    {
        if (itemViewerPanel != null)
            itemViewerPanel.SetActive(false);
        StartCoroutine(ReturnToMenuAfterDelay());
    }

    IEnumerator ReturnToMenuAfterDelay()
    {
        yield return new WaitForSeconds(3f);
        if (string.IsNullOrEmpty(winScene))
        {
            Debug.LogWarning("winScene not set in BattleManager! Defaulting to 'Menu'");
            winScene = "Menu";
        }
        mainCamera.enabled = true;
        itemViewerCamera.enabled = false;
        SceneManager.LoadScene(winScene);
    }
}
