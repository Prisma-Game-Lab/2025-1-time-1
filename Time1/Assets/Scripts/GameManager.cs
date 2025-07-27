using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    [Header("Progresso do Jogo")]
    public float timePlayed = 0;
    public int currentChapter = 1;
    public int currentDay = 1;
    public int index = 0;
    public bool[] battlesCompleted; // índice representa cada batalha
    public bool[] itemsAcquired; // index representa cada item
    public int[] lovePoints; // índice representa cada pretendente

    [Header("Sistema de Batalha")]
    public int maxTurnsPerBattle = 4;
    public int currentTurnNumber = 0;
    public float playerHealth = 100f;
    public float enemyHealth = 100f;
    public float basePositiveDamage = 25f;
    public float baseNegativeDamage = 35f; // Dano que o jogador recebe ao escolher opção negativa
    public float bonusDamageMultiplier = 1.5f; // Multiplicador de dano após reação negativa do oponente
    
    [Header("Configurações de Diálogo em Batalha")]
    public int positiveDialoguesPerBattle = 4;
    public int neutralDialoguesPerBattle = 4;
    public int negativeDialoguesPerBattle = 4;
    public int enemyDialoguesPerType = 3; // 3 diálogos por tipo (positivo, neutro, negativo) = 9 total

    [Header("Personagens")]
    public CharacterType currentOpponent;
    public CharacterType chosenLoveInterest;

    [Header("Estado Atual do Jogo")]
    public GameState currentState = GameState.Dialogue;
    public BattleState currentBattleState = BattleState.None;

    [Header("Configurações Gerais")]
    public string sceneDialogue = "DialogueScene";
    public string sceneCombat = "CombatScene";
    public string sceneMainMenu = "MainMenuScene";

    void Awake()
    {
        // Singleton pattern
        if (instance != null && instance != this)
        {
            Destroy(this.gameObject);
            Debug.LogWarning("Instância duplicada de GameManager destruída.");
        }
        instance = this;
        DontDestroyOnLoad(this.gameObject);
    }

    public void LoadScene(string sceneName)
    {
        LevelLoader levelLoader = FindObjectOfType<LevelLoader>();
        if (levelLoader != null)
        {
            levelLoader.LoadScene(sceneName);
        }
        else
        {
            SceneManager.LoadScene(sceneName);
        }
    }

    public void StartBattle(int battleID)
    {
        Debug.Log("Iniciando batalha ID: " + battleID);
        currentState = GameState.Combat;
        // Aqui você pode passar o battleID para uma cena de combate
        LoadScene(sceneCombat);
    }

    public void EndBattle(bool won, int battleID, int rewardID = -1)
    {
        if (won)
        {
            battlesCompleted[battleID] = true;
            if (rewardID != -1)
                itemsAcquired[rewardID] = true;
        }
        currentState = GameState.Dialogue;
        LoadScene(sceneDialogue);
    }

    public void AddLovePoints(int pretendenteID, int points)
    {
        lovePoints[pretendenteID] += points;
    }

    public void GoToMainMenu()
    {
        LoadScene(sceneMainMenu);
    }
    public enum GameState
    {
        Dialogue,
        Choice,
        Combat,
        Cutscene
    }

    public enum BattleState
    {
        None,
        PlayerTurn,
        EnemyTurn,
        Victory,
        Defeat
    }

    public enum CharacterType
    {
        None,
        Tutorial,
        Nerdola,
        Rebelde,
        Ator,
        FinalBoss
    }

    public enum DialogueType
    {
        Positive,
        Neutral,
        Negative
    }

    public enum BattleRewardType
    {
        Item,
        LovePoints,
        Both
    }
}
