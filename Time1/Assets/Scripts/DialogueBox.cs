using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu]
public class DialogueBox: ScriptableObject
{
    public Sprite background;
    public string text;
    public Sprite char_sprite;
    public string char_name;
    public int next;
    public bool chapter_end;
    public bool options;
    public Option[] option;
    public bool hasReward; // Indica se este diálogo tem uma recompensa
}
