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

public class BattleManager : MonoBehaviour
{
    [SerializeField] private string winScene;
    [SerializeField] private Camera mainCamera;

    public Slider playerHPBar;
    public Slider enemyHPBar;

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

    [SerializeField]
    private Dictionary<string, string> falasERespostasPositivas = new Dictionary<string, string>{
    { "Você elogiou o visual do oponente.", "O oponente sorriu de volta, um pouco sem graça." },
    { "Você disse que adoraria vê-lo de novo.", "Ele pareceu surpreso com o elogio." },
    { "Você fez um elogio honesto sobre o jeito dele.", "Ele desviou o olhar, mas estava sorrindo." },
    { "Você disse que se sentia bem ao lado dele.", "Ele agradeceu, ainda que desconfiado." },
    { "Você destacou algo gentil no comportamento dele.", "Ele balançou a cabeça, rindo de leve." },
    { "Você comentou que ele parece alguém confiável.", "Ele murmurou um 'valeu' tímido." },
    { "Você sorriu e fez um elogio inesperado.", "Ele pareceu se animar um pouco." },
    { "Você demonstrou interesse genuíno pelo que ele dizia.", "Ele ficou quieto, mas você viu que gostou." },
    { "Você mencionou que ele tem uma presença acolhedora.", "Ele corou discretamente." }
    };

    [SerializeField]
    private Dictionary<string, string> falasERespostasNeutras = new Dictionary<string, string>{
    { "Você comentou sobre o tempo.", "O oponente soltou um 'aham' educado." },
    { "Você perguntou se ele gosta de pizza.", "Ele franziu a testa, confuso." },
    { "Você falou sobre o barulho na rua.", "Ele apenas assentiu, sem muito interesse." },
    { "Você perguntou quantas horas ele dormiu.", "Ele olhou para o lado, esperando algo mais." },
    { "Você comentou que esqueceu de alimentar o gato.", "Ele respondeu com um 'sei lá'." },
    { "Você perguntou se ele já viu um pato correndo.", "Ele mexeu no celular." },
    { "Você mencionou um sonho estranho sem contexto.", "Ele não entendeu muito bem a pergunta." },
    { "Você falou sobre cereal com leite ou sem.", "Ele pareceu perdido nos próprios pensamentos." },
    { "Você ficou em silêncio por alguns segundos e sorriu.", "Ele respondeu com um 'tá certo' seco." }
    };

    [SerializeField]
    private Dictionary<string, string> falasERespostasNegativas = new Dictionary<string, string>{
    { "Você criticou o estilo dele.", "O oponente cruzou os braços, visivelmente incomodado." },
    { "Você fez uma piada meio ácida.", "Ele revirou os olhos." },
    { "Você questionou as escolhas dele.", "Ele rebateu com uma piada ainda mais ácida." },
    { "Você insinuou que ele se leva a sério demais.", "Ele ficou em silêncio, com expressão dura." },
    { "Você disse que ele tenta parecer alguém que não é.", "Ele pareceu se fechar na hora." },
    { "Você revirou os olhos enquanto ele falava.", "Ele deu um sorriso falso." },
    { "Você zombou de algo que ele gosta.", "Ele respondeu com sarcasmo." },
    { "Você fez uma comparação que o colocou pra baixo.", "Ele olhou nos seus olhos, desafiador." },
    { "Você deixou claro que não está impressionado.", "Ele disse: 'É isso que você acha, então?'" }
    };


    void Start()
    {
        AtualizarHP();
        feedbackText.text = "A batalha começou!";
        RegistrarBotoes();
        AtualizarFalasNosBotoes();
        dialogueButtonsPanel.SetActive(true);

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
            colors.normalColor = (i == botaoSelecionado) ? Color.yellow : Color.white;
            dialogueButtons[i].colors = colors;
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
                cor = new Color(1f, 0f, 0f, 0.25f);
                iconsArray = rebelIcons;
                break;
            case Pretendente.Ator:
                cor = new Color(0.6f, 0f, 1f, 0.25f);
                iconsArray = actorIcons;
                break;
        }

        // Atualiza a cor do filtro (background)
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
        StartCoroutine(ExecutarTurno(tipoEscolhido));
    }

    IEnumerator ExecutarTurno(string tipo)
    {
        feedbackText.text = falaEscolhida;
        yield return EsperarAvanco();

        string resposta = ObterRespostaDoOponente(tipo);
        feedbackText.text += "\n" + resposta;
        yield return EsperarAvanco();

        feedbackText.text = "";
        yield return new WaitForSeconds(0.2f); // espera técnica, mantém

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

        while (!avancarTexto)
            yield return null;

        AudioManager.instance.PlaySound("Avancar"); 

        aguardandoAvanco = false;
    }

    string ObterRespostaDoOponente(string tipo)
    {
        if (respostasDasFalas.ContainsKey(falaEscolhida))
            return respostasDasFalas[falaEscolhida];
        return "...";
    }


    void AtualizarFalasNosBotoes()
    {
        var positiva = RemoverParDeFala(falasERespostasPositivas);
        var neutra = RemoverParDeFala(falasERespostasNeutras);
        var negativa = RemoverParDeFala(falasERespostasNegativas);

        var opcoes = new List<(string tipo, string fala, string resposta)>
    {
        ("positivo", positiva.Item1, positiva.Item2),
        ("neutro", neutra.Item1, neutra.Item2),
        ("negativo", negativa.Item1, negativa.Item2)
    };

        for (int i = 0; i < opcoes.Count; i++)
        {
            var temp = opcoes[i];
            int rand = Random.Range(i, opcoes.Count);
            opcoes[i] = opcoes[rand];
            opcoes[rand] = temp;
        }

        opcoesAtuais.Clear();
        respostasDasFalas.Clear();

        for (int i = 0; i < dialogueButtons.Length; i++)
        {
            buttonTexts[i].text = opcoes[i].fala;
            opcoesAtuais[opcoes[i].tipo] = opcoes[i].fala;
            respostasDasFalas[opcoes[i].fala] = opcoes[i].resposta;
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

        leftBarFill.fillAmount = 0f;
        rightBarFill.fillAmount = 0f;
        timerBarContainer.SetActive(false);

        StartCoroutine(ExecutarTurno(tipoEscolhido));
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

        playerHPBar.value = playerHP;
        enemyHPBar.value = enemyHP;
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
