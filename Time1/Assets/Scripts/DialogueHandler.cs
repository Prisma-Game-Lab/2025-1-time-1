using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class DialogueHandler : MonoBehaviour
{
    [SerializeField] private string nextScene;
    [SerializeField] private GameObject sprite;
    [SerializeField] private TextMeshProUGUI dialogue;
    [SerializeField] private TextMeshProUGUI charName;
    [SerializeField] private TextMeshProUGUI option1;
    [SerializeField] private TextMeshProUGUI option2;
    [SerializeField] private TextMeshProUGUI option3;
    [SerializeField] private GameObject endScreen;
    [SerializeField] private TextMeshProUGUI[] LPtext;
    public GameObject options;
    [SerializeField] private DialogueBox[] dialogues;
    public int dialogueIndex = 0;
    public int currIndex = 0;

    [Header("Item Viewer")]
    [SerializeField] private Item3DViewer itemViewer;
    [SerializeField] public GameObject itemViewerPanel;

    private int selectedOptionIndex = 0;
    private Sprite lastSprite;

    void Start()
    {
        dialogueIndex = GameManager.instance.index;
        Sprite newSprite = dialogues[dialogueIndex].char_sprite;
        sprite.GetComponent<Image>().sprite = newSprite;
        lastSprite = newSprite;
        StartCoroutine(FadeInSprite());

        dialogue.text = dialogues[dialogueIndex].text;
        charName.text = dialogues[dialogueIndex].char_name;

        if (dialogues[dialogueIndex].options)
        {
            option1.text = dialogues[dialogueIndex].option[0].opText;
            option2.text = dialogues[dialogueIndex].option[1].opText;
            option3.text = dialogues[dialogueIndex].option[2].opText;
        }

        currIndex = dialogueIndex;
        dialogueIndex = dialogues[dialogueIndex].next;

        if (itemViewerPanel != null)
        {
            itemViewerPanel.SetActive(false);
        }

        AtualizarSelecao();
    }

    void Update()
    {
        if (options.activeSelf)
        {
            if (Input.GetKeyDown(KeyCode.UpArrow))
            {
                selectedOptionIndex = (selectedOptionIndex - 1 + 3) % 3;
                AtualizarSelecao();
            }
            else if (Input.GetKeyDown(KeyCode.DownArrow))
            {
                selectedOptionIndex = (selectedOptionIndex + 1) % 3;
                AtualizarSelecao();
            }
            else if (Input.GetKeyDown(KeyCode.Space))
            {
                options.SetActive(false);
                Option(selectedOptionIndex);
            }
        }
        else if (Input.GetKeyDown(KeyCode.Space))
        {
            NextDialogue();
        }
    }

    void AtualizarSelecao()
    {
        Color amarelo = new Color(1f, 0.85f, 0.3f);
        Color branco = Color.white;

        option1.color = (selectedOptionIndex == 0) ? amarelo : branco;
        option2.color = (selectedOptionIndex == 1) ? amarelo : branco;
        option3.color = (selectedOptionIndex == 2) ? amarelo : branco;
    }

    public void NextDialogue()
    {
        if (dialogueIndex < dialogues.Length)
        {
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
                option1.text = dialogues[dialogueIndex].option[0].opText;
                option2.text = dialogues[dialogueIndex].option[1].opText;
                option3.text = dialogues[dialogueIndex].option[2].opText;
                options.SetActive(true);
                selectedOptionIndex = 0;
                AtualizarSelecao();
                return;
            }

            if (dialogues[dialogueIndex].chapter_end)
            {
                endScreen.SetActive(true);
                for (int i = 0; i < 3; ++i)
                {
                    LPtext[i].text = GameManager.instance.lovePoints[i].ToString();
                }
            }

            dialogueIndex = dialogues[dialogueIndex].next;
        }
        else
        {
            SceneManager.LoadScene(nextScene);
        }
    }

    public void Option(int index)
    {
        dialogueIndex = dialogues[currIndex].option[index].next;
        addPoints(index);

        if (dialogues[currIndex].hasReward && itemViewer != null && itemViewerPanel != null)
        {
            itemViewerPanel.SetActive(true);
            itemViewer.ShowItem();
        }
    }

    public void CloseItemViewer()
    {
        if (itemViewer != null)
        {
            itemViewer.HideItem();
        }

        if (itemViewerPanel != null)
        {
            itemViewerPanel.SetActive(false);
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
