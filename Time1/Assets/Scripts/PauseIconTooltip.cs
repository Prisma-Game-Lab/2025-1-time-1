using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PauseIconTooltip : MonoBehaviour
{
    [SerializeField] private GameObject tooltipObject;
    [SerializeField] private PauseMenu pauseMenu;

    [Header("Ícones")]
    [SerializeField] private Image pauseIconImage;
    [SerializeField] private Sprite pauseSprite;
    [SerializeField] private Sprite lockSprite;

    [SerializeField] private TextMeshProUGUI tooltipText;

    private BattleManager battleManager;
    private bool isBattleScene = false;

    void Start()
    {
        battleManager = FindObjectOfType<BattleManager>();
        isBattleScene = (battleManager != null);

        if (!isBattleScene)
        {
            // Cena de diálogo ou outra sem BattleManager
            pauseIconImage.sprite = pauseSprite;
            tooltipText.text = "Pressione ESC para pausar";
        }
    }

    void Update()
    {
        if (!isBattleScene)
            return;

        if (battleManager.podePausar)
        {
            pauseIconImage.sprite = pauseSprite;
            tooltipText.text = "Pressione ESC para pausar";
        }
        else
        {
            pauseIconImage.sprite = lockSprite;
            tooltipText.text = "Só é possível pausar durante o turno do oponente";
        }
    }

    public void ShowTooltip()
    {
        tooltipObject.SetActive(true);
    }

    public void HideTooltip()
    {
        tooltipObject.SetActive(false);
    }

    public void ClickPause()
    {
        if (pauseMenu == null)
            return;

        if (!isBattleScene || battleManager.podePausar)
        {
            pauseMenu.TogglePause();
        }
        else
        {
            Debug.Log("Não pode pausar durante o turno do jogador");
        }
    }
}
