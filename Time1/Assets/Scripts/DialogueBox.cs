using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class DialogueBox: ScriptableObject
{
    public string text;
    public Sprite char_sprite;
    public string char_name;
    public int next;
    public bool options;
    public Option[] option;
    public bool hasReward; // Indica se este diálogo tem uma recompensa
}
