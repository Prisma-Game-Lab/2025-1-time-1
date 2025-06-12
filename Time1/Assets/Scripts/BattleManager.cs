using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class BattleManager : MonoBehaviour
{
    [SerializeField] private string winScene;

    public Slider playerHPBar;
    public Slider enemyHPBar;

    public TextMeshProUGUI feedbackText;
    public GameObject dialogueButtonsPanel;
    public Button[] dialogueButtons;

    [Header("Reward System")]
    [SerializeField] private Item3DViewer itemViewer;
    [SerializeField] private GameObject itemViewerPanel;
    [SerializeField] private Transform itemPrefab;

    private TextMeshProUGUI[] buttonTexts;

    private int playerHP = 100;
    private int enemyHP = 100;

    private int turn = 1;
    private const int maxTurns = 4;

    private bool opponentVulnerable = false;

    private string tipoEscolhido;
    private string falaEscolhida;

    private Dictionary<string, string> opcoesAtuais = new Dictionary<string, string>();

    [SerializeField] private List<string> falasPositivas = new List<string>
    {
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

    [SerializeField] private List<string> falasNeutras = new List<string>
    {
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


    [SerializeField] private List<string> falasNegativas = new List<string>
    {
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


    void Start()
    {
        AtualizarHP();
        feedbackText.text = "A batalha começou!";
        RegistrarBotoes();
        AtualizarFalasNosBotoes();
        dialogueButtonsPanel.SetActive(true);

        // Garantir que o painel de item começa desativado
        if (itemViewerPanel != null)
        {
            itemViewerPanel.SetActive(false);
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
        feedbackText.text = $"{falaEscolhida}\n";
        yield return new WaitForSeconds(1.5f);

        // Player age
        if (tipo == "positivo")
        {
            int dano = opponentVulnerable ? 50 : 35;
            enemyHP -= dano;
            feedbackText.text += $"Foi uma fala positiva! Causou {dano} de dano ao oponente.\n";
            opponentVulnerable = false;
        }
        else if (tipo == "neutro")
        {
            int autoDano = 10;
            playerHP -= autoDano;
            feedbackText.text += $"Foi uma fala neutra. Você sofreu {autoDano} de dano por hesitação.\n";
        }
        else if (tipo == "negativo")
        {
            int danoRecebido = 30;
            playerHP -= danoRecebido;
            feedbackText.text += $"Foi uma fala negativa! Você levou {danoRecebido} de dano no mini combo do oponente.\n";
        }

        AtualizarHP();
        
        // Verifica se alguém perdeu após o ataque do jogador
        if (enemyHP <= 0 || playerHP <= 0)
        {
            yield return new WaitForSeconds(1.5f);
            FinalizarBatalhaPorTurno();
            yield break;
        }

        yield return new WaitForSeconds(1.5f);

        // Reação do oponente
        string[] reacoes = { "atacar", "neutro", "esquisita" };
        string reacao = reacoes[Random.Range(0, reacoes.Length)];

        if (reacao == "atacar")
        {
            int dano = 25;
            playerHP -= dano;
            feedbackText.text += $"O oponente contra-atacou! Você perdeu {dano} de vida.\n";
            opponentVulnerable = false;
        }
        else if (reacao == "neutro")
        {
            feedbackText.text += "O oponente ficou em silêncio...\n";
            opponentVulnerable = false;
        }
        else if (reacao == "esquisita")
        {
            feedbackText.text += "O oponente teve uma reação esquisita... parece vulnerável!\n";
            opponentVulnerable = true;
        }

        AtualizarHP();

        // Verifica se alguém perdeu após a reação do oponente
        if (enemyHP <= 0 || playerHP <= 0)
        {
            yield return new WaitForSeconds(1.5f);
            FinalizarBatalhaPorTurno();
            yield break;
        }

        yield return new WaitForSeconds(1.5f);

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

        // Embaralha
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
        // Garante que o HP não fique negativo
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
            StartCoroutine(ShowRewardAfterDelay());
        }
        else if (playerHP > enemyHP)
        {
            enemyHP = 0;
            AtualizarHP();
            feedbackText.text = "Você venceu a batalha!";
            StartCoroutine(ShowRewardAfterDelay());
        }
        else
        {
            playerHP = 0;
            AtualizarHP();
            feedbackText.text = "Você perdeu a batalha!";
            StartCoroutine(ReturnToMenuAfterDelay());
        }
    }

    IEnumerator ShowRewardAfterDelay()
    {
        yield return new WaitForSeconds(3f); // Aumentado para dar tempo de ler o feedback
        
        if (itemViewer != null && itemPrefab != null && itemViewerPanel != null)
        {
            itemViewerPanel.SetActive(true);
            itemViewer.ShowItem();
            Debug.Log("Reward shown");
            yield return new WaitForSeconds(2f);
            StartCoroutine(ReturnToMenuAfterDelay());
        }
        else
        {
            Debug.LogWarning("Item3DViewer, itemPrefab or itemViewerPanel not set in BattleManager!");
            StartCoroutine(ReturnToMenuAfterDelay());
        }
    }

    IEnumerator ReturnToMenuAfterDelay()
    {
        yield return new WaitForSeconds(3f); // Aumentado para dar tempo de ler o feedback
        if (string.IsNullOrEmpty(winScene))
        {
            Debug.LogWarning("winScene not set in BattleManager! Defaulting to 'Menu'");
            winScene = "Menu";
        }
        SceneManager.LoadScene(winScene);
    }
}
