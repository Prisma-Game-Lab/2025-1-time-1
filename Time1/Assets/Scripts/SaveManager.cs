using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;
using TMPro;
using UnityEngine.SceneManagement;

public class SaveManager : MonoBehaviour
{
    [SerializeField] private GameObject dialogueScene;
    [SerializeField] private TextMeshProUGUI[] saveName;
    [SerializeField] private TextMeshProUGUI[] loadName;
    private string[] filePath = new string[3];

    void Start()
    {
        filePath[0] = Application.streamingAssetsPath + "/save.txt";
        filePath[1] = Application.streamingAssetsPath + "/save1.txt";
        filePath[2] = Application.streamingAssetsPath + "/save2.txt";
        for (int i = 0; i < 3; i++)
        {
            string[] saveText = readSave(i);
            saveName[i].text = saveText[0].Split(": ")[1];
            loadName[i].text = saveText[0].Split(": ")[1];
        }

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
        string chapterLine = "chapter: " + SceneManager.GetActiveScene().name + "\n";
        writeSave(0, chapterLine, save);
        string indexLine = "scene: " + index + "\n";
        writeSave(1, indexLine, save);
        saveName[save].text = chapterLine.Split(": ")[1];
        loadName[save].text = chapterLine.Split(": ")[1];
    }

    public void load(int save)
    {
        string[] saveText = readSave(save);
        string sceneName = saveText[0].Split(": ")[1];
        if (sceneName != SceneManager.GetActiveScene().name)
        {
            SceneManager.LoadScene(sceneName);
        }
        int index = 0;
        Int32.TryParse(saveText[1].Split(':')[1], out index);
        dialogueScene.GetComponent<DialogueHandler>().dialogueIndex = index;
        dialogueScene.GetComponent<DialogueHandler>().options.SetActive(false);
        dialogueScene.GetComponent<DialogueHandler>().NextDialogue();
    }
}
