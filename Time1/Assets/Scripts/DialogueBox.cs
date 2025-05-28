using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class DialogueBox
{
    public string text;
    public Sprite char_sprite;
    public string char_name;
    public int next;
    public string op1;
    public string op2;
    public string op3;
    public int[] opIds;
    public bool options;
}
