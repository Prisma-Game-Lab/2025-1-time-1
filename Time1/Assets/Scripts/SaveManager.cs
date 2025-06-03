using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;

public class SaveManager : MonoBehaviour
{
    [SerializeField] GameObject dialogueScene;
    private string filePath;

    void Start()
    {
        filePath = Application.streamingAssetsPath + "/save.txt";
    }

    public string[] readSave()
    {
        string[] saveText = File.ReadAllLines(filePath);
        return saveText;
    }

    public void writeSave(int index, string line)
    {
        string[] saveText = readSave();
        if (saveText.Length > index)
        {
            saveText[index] = line;
        }
        File.WriteAllLines(filePath, saveText);
    }

    public void save()
    {
        int index = dialogueScene.GetComponent<DialogueHandler>().currIndex;
        string indexLine = "scene: " + index + "\n";
        writeSave(0, indexLine);
    }

    public void load()
    {
        string[] saveText = readSave();
        int index = 0;
        Int32.TryParse(saveText[0].Split(':')[1], out index);
        dialogueScene.GetComponent<DialogueHandler>().dialogueIndex = index;
        dialogueScene.GetComponent<DialogueHandler>().options.SetActive(false);
        dialogueScene.GetComponent<DialogueHandler>().NextDialogue();
    }
}
