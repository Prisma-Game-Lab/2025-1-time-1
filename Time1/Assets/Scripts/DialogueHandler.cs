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

    void Start()
    {
        sprite.GetComponent<Image>().sprite = dialogues[0].char_sprite;
        dialogue.text = dialogues[0].text;
        charName.text = dialogues[0].char_name;
    }
}
