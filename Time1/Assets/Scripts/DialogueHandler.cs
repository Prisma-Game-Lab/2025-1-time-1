using System.Collections;
using System.Collections.Generic;
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
    public GameObject options;
    [SerializeField] private DialogueBox[] dialogues;
    public int dialogueIndex = 0;
    public int currIndex = 0;

    [Header("Item Viewer")]
    [SerializeField] private Item3DViewer itemViewer;
    [SerializeField] public GameObject itemViewerPanel;

    void Start()
    {
        dialogueIndex = GameManager.instance.index;
        sprite.GetComponent<Image>().sprite = dialogues[dialogueIndex].char_sprite;
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
    }

    public void NextDialogue()
    {
        if (dialogueIndex < dialogues.Length)
        {
            sprite.GetComponent<Image>().sprite = dialogues[dialogueIndex].char_sprite;
            dialogue.text = dialogues[dialogueIndex].text;
            charName.text = dialogues[dialogueIndex].char_name;
            currIndex = dialogueIndex;
            if (dialogues[dialogueIndex].options)
            {
                option1.text = dialogues[dialogueIndex].option[0].opText;
                option2.text = dialogues[dialogueIndex].option[1].opText;
                option3.text = dialogues[dialogueIndex].option[2].opText;
                options.SetActive(true);
                return;
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
        dialogueIndex = dialogues[dialogueIndex].option[index].next;
        addPoints(index);

        // Se a opção selecionada tiver um item associado, mostra o visualizador
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
        switch(dialogues[currIndex].char_name)
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
        GameManager.instance.AddLovePoints(idPretendente, (int) dialogues[currIndex].option[index].reaction);
    }
}
