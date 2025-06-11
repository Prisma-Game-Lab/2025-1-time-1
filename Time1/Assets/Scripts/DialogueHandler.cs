using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DialogueHandler : MonoBehaviour
{
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
        sprite.GetComponent<Image>().sprite = dialogues[dialogueIndex].char_sprite;
        dialogue.text = dialogues[dialogueIndex].text;
        charName.text = dialogues[dialogueIndex].char_name;
        option1.text = dialogues[dialogueIndex].op1;
        option2.text = dialogues[dialogueIndex].op2;
        option3.text = dialogues[dialogueIndex].op3;
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
            option1.text = dialogues[dialogueIndex].op1;
            option2.text = dialogues[dialogueIndex].op2;
            option3.text = dialogues[dialogueIndex].op3;
            currIndex = dialogueIndex;
            if (dialogues[dialogueIndex].options)
            {
                options.SetActive(true);
                return;
            }
            dialogueIndex = dialogues[dialogueIndex].next;
        }
    }

    public void Option(int index)
    {
        dialogueIndex = dialogues[dialogueIndex].opIds[index];

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
}
