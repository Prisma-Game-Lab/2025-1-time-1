using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    [Header("Progresso do Jogo")]
    public int currentChapter = 1;
    public int currentDay = 1;
    public bool[] battlesCompleted; // índice representa cada batalha
    public bool[] itemsAcquired; // index representa cada item
    public int[] lovePoints; // índice representa cada pretendente

    [Header("Estado Atual do Jogo")]
    public GameState currentState = GameState.Dialogue;

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
        SceneManager.LoadScene(sceneName);
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
}
