using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu]
public class DialogueBox: ScriptableObject
{
    public Sprite background;
    public bool fadeInBackground = false;
    public string text;
    public Sprite char_sprite;
    public bool fadeInCharacter = false;
    public string char_name;
    public int next;
    public bool chapter_end;
    public bool final_option;
    public bool options;
    public Option[] option;
    public bool hasReward; // Indica se este diálogo tem uma recompensa

}
