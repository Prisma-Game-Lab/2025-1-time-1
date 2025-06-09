using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;

public class SaveManager : MonoBehaviour
{
    [SerializeField] private GameObject dialogueScene;
    private string[] filePath = new string[3];

    void Start()
    {
        filePath[0] = Application.streamingAssetsPath + "/save.txt";
        filePath[1] = Application.streamingAssetsPath + "/save1.txt";
        filePath[2] = Application.streamingAssetsPath + "/save2.txt";
    }

    public string[] readSave(int save)
    {
        string[] saveText = File.ReadAllLines(filePath[save]);
        return saveText;
    }

    public void writeSave(int index, string line, int save)
    {
        string[] saveText = readSave(save);
        if (saveText.Length > index)
        {
            saveText[index] = line;
        }
        File.WriteAllLines(filePath[save], saveText);
    }

    public void save(int save)
    {
        int index = dialogueScene.GetComponent<DialogueHandler>().currIndex;
        string indexLine = "scene: " + index + "\n";
        writeSave(0, indexLine, save);
    }

    public void load(int save)
    {
        string[] saveText = readSave(save);
        int index = 0;
        Int32.TryParse(saveText[0].Split(':')[1], out index);
        dialogueScene.GetComponent<DialogueHandler>().dialogueIndex = index;
        dialogueScene.GetComponent<DialogueHandler>().options.SetActive(false);
        dialogueScene.GetComponent<DialogueHandler>().NextDialogue();
    }
}
