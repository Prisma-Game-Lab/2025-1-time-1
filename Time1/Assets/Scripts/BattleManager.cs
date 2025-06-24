using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class BattleManager : MonoBehaviour
{
    [SerializeField] private string winScene;
    [SerializeField] private Camera mainCamera;

    public Slider playerHPBar;
    public Slider enemyHPBar;

    public TextMeshProUGUI feedbackText;
    public GameObject dialogueButtonsPanel;
    public Button[] dialogueButtons;

    [Header("Reward System")]
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

    [Header("Battle Filter")]
    public Image filterBackgroundImage;
    public GameObject iconPrefab;
    public Transform iconsContainer;

    public Sprite[] nerdIcons;
    public Sprite[] rebelIcons;
    public Sprite[] actorIcons;

    private List<GameObject> activeIcons = new List<GameObject>();

    private enum Pretendente { Nerd, Rebelde, Ator }
    private Pretendente pretendenteAtual;

    [SerializeField]
    private List<string> falasPositivas = new List<string> {
        "Você elogiou o visual do oponente.",
        "Você disse que adoraria vê-lo de novo.",
        "Você fez um elogio honesto sobre o jeito dele.",
        "Você disse que se sentia bem ao lado dele.",
        "Você destacou algo gentil no comportamento dele.",
        "Você comentou que ele parece alguém confiável.",
        "Você sorriu e fez um elogio inesperado.",
        "Você demonstrou interesse genuíno pelo que ele dizia.",
        "Você mencionou que ele tem uma presença acolhedora."
    };

    [SerializeField]
    private List<string> falasNeutras = new List<string> {
        "Você comentou sobre o tempo.",
        "Você perguntou se ele gosta de pizza.",
        "Você falou sobre o barulho na rua.",
        "Você perguntou quantas horas ele dormiu.",
        "Você comentou que esqueceu de alimentar o gato.",
        "Você perguntou se ele já viu um pato correndo.",
        "Você mencionou um sonho estranho sem contexto.",
        "Você falou sobre cereal com leite ou sem.",
        "Você ficou em silêncio por alguns segundos e sorriu."
    };

    [SerializeField]
    private List<string> falasNegativas = new List<string> {
        "Você criticou o estilo dele.",
        "Você fez uma piada meio ácida.",
        "Você questionou as escolhas dele.",
        "Você insinuou que ele se leva a sério demais.",
        "Você disse que ele tenta parecer alguém que não é.",
        "Você revirou os olhos enquanto ele falava.",
        "Você zombou de algo que ele gosta.",
        "Você fez uma comparação que o colocou pra baixo.",
        "Você deixou claro que não está impressionado."
    };

    [SerializeField]
    private List<string> respostasPositivas = new List<string> {
        "O oponente sorriu de volta, um pouco sem graça.",
        "Ele pareceu surpreso com o elogio.",
        "Ele desviou o olhar, mas estava sorrindo.",
        "Ele agradeceu, ainda que desconfiado.",
        "Ele balançou a cabeça, rindo de leve.",
        "Ele murmurou um 'valeu' tímido.",
        "Ele pareceu se animar um pouco.",
        "Ele ficou quieto, mas você viu que gostou.",
        "Ele corou discretamente."
    };

    [SerializeField]
    private List<string> respostasNeutras = new List<string> {
        "O oponente soltou um 'aham' educado.",
        "Ele franziu a testa, confuso.",
        "Ele apenas assentiu, sem muito interesse.",
        "Ele olhou para o lado, esperando algo mais.",
        "Ele respondeu com um 'sei lá'.",
        "Ele mexeu no celular.",
        "Ele não entendeu muito bem a pergunta.",
        "Ele pareceu perdido nos próprios pensamentos.",
        "Ele respondeu com um 'tá certo' seco."
    };

    [SerializeField]
    private List<string> respostasNegativas = new List<string> {
        "O oponente cruzou os braços, visivelmente incomodado.",
        "Ele revirou os olhos.",
        "Ele rebateu com uma piada ainda mais ácida.",
        "Ele ficou em silêncio, com expressão dura.",
        "Ele pareceu se fechar na hora.",
        "Ele deu um sorriso falso.",
        "Ele respondeu com sarcasmo.",
        "Ele olhou nos seus olhos, desafiador.",
        "Ele disse: 'É isso que você acha, então?'"
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
        SetBattleFilter(pretendenteAtual);
    }

    void Update()
    {
        AtualizarIcones();
    }

    private void SetBattleFilter(Pretendente tipo)
    {
        Color cor = Color.clear;
        Sprite[] iconsArray = null;

        switch (tipo)
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

        filterBackgroundImage.color = cor;

        foreach (var icon in activeIcons)
        {
            Destroy(icon);
        }
        activeIcons.Clear();

        int quantidade = 10;
        for (int i = 0; i < quantidade; i++)
        {
            GameObject iconGO = Instantiate(iconPrefab, iconsContainer);
            Image iconImage = iconGO.GetComponent<Image>();
            iconImage.sprite = iconsArray[Random.Range(0, iconsArray.Length)];

            iconGO.transform.localPosition = new Vector3(
                Random.Range(-800f, 800f),
                Random.Range(-400f, 400f),
                0f
            );

            activeIcons.Add(iconGO);
        }
    }

    private void AtualizarIcones()
    {
        float velocidadeMin = 5f;
        float velocidadeMax = 20f;

        foreach (var icon in activeIcons)
        {
            Vector3 dir = new Vector3(
                Mathf.Sin(Time.time * 0.5f + icon.GetInstanceID()),
                Mathf.Cos(Time.time * 0.5f + icon.GetInstanceID()),
                0f
            );

            float speed = Mathf.PingPong(Time.time, velocidadeMax - velocidadeMin) + velocidadeMin;

            icon.transform.localPosition += dir.normalized * speed * Time.deltaTime;

            Vector3 pos = icon.transform.localPosition;
            if (pos.x > 900) pos.x = -900;
            else if (pos.x < -900) pos.x = 900;

            if (pos.y > 500) pos.y = -500;
            else if (pos.y < -500) pos.y = 500;

            icon.transform.localPosition = pos;
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
        yield return new WaitForSeconds(1.5f);

       
        string resposta = ObterRespostaDoOponente(tipo);
        feedbackText.text += "\n" + resposta;
        yield return new WaitForSeconds(2.5f);

    
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

        feedbackText.text = efeitoTexto;
        yield return new WaitForSeconds(2.5f);

        
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
        yield return new WaitForSeconds(2.5f);

        // Verifica vitória/derrota
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

    string ObterRespostaDoOponente(string tipo)
    {
        switch (tipo)
        {
            case "positivo":
                return RemoverUmaAleatoria(respostasPositivas);
            case "neutro":
                return RemoverUmaAleatoria(respostasNeutras);
            case "negativo":
                return RemoverUmaAleatoria(respostasNegativas);
            default:
                return "...";
        }
    }

    void AtualizarFalasNosBotoes()
    {
        string positiva = RemoverUmaAleatoria(falasPositivas);
        string neutra = RemoverUmaAleatoria(falasNeutras);
        string negativa = RemoverUmaAleatoria(falasNegativas);

        var opcoes = new List<(string tipo, string fala)>
        {
            ("positivo", positiva),
            ("neutro", neutra),
            ("negativo", negativa)
        };

        for (int i = 0; i < opcoes.Count; i++)
        {
            var temp = opcoes[i];
            int rand = Random.Range(i, opcoes.Count);
            opcoes[i] = opcoes[rand];
            opcoes[rand] = temp;
        }

        opcoesAtuais.Clear();
        for (int i = 0; i < dialogueButtons.Length; i++)
        {
            buttonTexts[i].text = opcoes[i].fala;
            opcoesAtuais[opcoes[i].tipo] = opcoes[i].fala;
        }
    }

    string RemoverUmaAleatoria(List<string> lista)
    {
        if (lista.Count == 0) return "???";
        int index = Random.Range(0, lista.Count);
        string item = lista[index];
        lista.RemoveAt(index);
        return item;
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
