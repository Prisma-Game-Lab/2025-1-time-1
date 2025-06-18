using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Reaction
{
    Good = 1,
    Neutral = 0,
    Bad = -1
}

[Serializable]
public class Option
{
    public string opText;
    public int next;
    public Reaction reaction;
}
