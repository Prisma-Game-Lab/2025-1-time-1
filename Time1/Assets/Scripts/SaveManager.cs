using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;
using TMPro;
using UnityEngine.SceneManagement;
using System.Linq;

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
            float savePlayed = 0;
            Single.TryParse(saveText[2].Split(':')[1], out savePlayed);
            TimeSpan totalTime = TimeSpan.FromSeconds(savePlayed);
            saveName[i].text = saveText[0].Split(": ")[1] + " - " + totalTime.Hours + ":" + totalTime.Minutes + ":" + totalTime.Seconds;
            loadName[i].text = saveText[0].Split(": ")[1] + " - " + totalTime.Hours + ":" + totalTime.Minutes + ":" + totalTime.Seconds;
        }
    }

    public string[] readSave(int save)
    {
        string[] saveText = File.ReadAllLines(filePath[save]);
        return saveText;
    }

    public void writeSave(string[] line, int save)
    {
        string[] saveText = readSave(save);
        for (int i = 0; i < 3; ++i)
        {
            saveText[i] = line[i];
        }
        File.WriteAllLines(filePath[save], saveText);
    }

    public void save(int save)
    {
        int index = dialogueScene.GetComponent<DialogueHandler>().currIndex;
        string[] saveLines = new string[3];
        saveLines[0] = "chapter: " + SceneManager.GetActiveScene().name;
        saveLines[1] = "scene: " + index;
        float savePlayed = updateSaveTime(save);
        saveLines[2] = "time: " + savePlayed;
        writeSave(saveLines, save);
        TimeSpan totalTime = TimeSpan.FromSeconds(savePlayed);
        saveName[save].text = saveLines[0].Split(": ")[1] + " - " + totalTime.Hours + ":" + totalTime.Minutes + ":" + totalTime.Seconds;
        loadName[save].text = saveLines[0].Split(": ")[1] + " - " + totalTime.Hours + ":" + totalTime.Minutes + ":" + totalTime.Seconds;
    }

    public void load(int save)
    {
        GameManager.instance.timePlayed = Time.time;
        string[] saveText = readSave(save);
        string sceneName = saveText[0].Split(": ")[1];
        int index = 0;
        Int32.TryParse(saveText[1].Split(':')[1], out index);
        if (sceneName != SceneManager.GetActiveScene().name)
        {
            GameManager.instance.index = index;
            SceneManager.LoadScene(sceneName);
        }
        dialogueScene.GetComponent<DialogueHandler>().dialogueIndex = index;
        dialogueScene.GetComponent<DialogueHandler>().options.SetActive(false);
        dialogueScene.GetComponent<DialogueHandler>().NextDialogue();
    }

    private float updateSaveTime(int save)
    {
        float currTime = Time.time;
        float saveTime = currTime - GameManager.instance.timePlayed;
        GameManager.instance.timePlayed = Time.time;
        string[] saveText = readSave(save);
        float savePlayed = 0;
        Single.TryParse(saveText[2].Split(':')[1], out savePlayed);
        savePlayed += saveTime;
        return savePlayed;
    }
}
