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
    [SerializeField] private DialogueBox[] dialogues;
    private int dialogueIndex = 0;

    void Start()
    {
        sprite.GetComponent<Image>().sprite = dialogues[dialogueIndex].char_sprite;
        dialogue.text = dialogues[dialogueIndex].text;
        charName.text = dialogues[dialogueIndex].char_name;
        dialogueIndex = dialogues[dialogueIndex].next;
    }

    public void NextDialogue()
    {
        if (dialogueIndex < dialogues.Length)
        {
            sprite.GetComponent<Image>().sprite = dialogues[dialogueIndex].char_sprite;
            dialogue.text = dialogues[dialogueIndex].text;
            charName.text = dialogues[dialogueIndex].char_name;
            dialogueIndex = dialogues[dialogueIndex].next;
        }
        else
        {
            Debug.Log("acabou");
        }
    }
}
