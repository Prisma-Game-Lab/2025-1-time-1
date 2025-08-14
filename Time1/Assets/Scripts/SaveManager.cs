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
    [SerializeField] private string currChapter;
    [SerializeField] private string nextChapter;
    private string[] filePath = new string[3];
    public bool chapterEnd = false;

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
            string time = totalTime.ToString(@"hh\:mm\:ss");
            saveName[i].text = saveText[0].Split(": ")[1] + " " + time;
            loadName[i].text = saveText[0].Split(": ")[1] + " " + time;
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
        for (int i = 0; i < 5; ++i)
        {
            saveText[i] = line[i];
        }
        File.WriteAllLines(filePath[save], saveText);
        if (chapterEnd)
        {
            dialogueScene.GetComponent<DialogueHandler>().NextDialogue();
        }
    }

    public void save(int save)
    {
        int index = dialogueScene.GetComponent<DialogueHandler>().currIndex;
        string[] saveLines = new string[5];
        if (chapterEnd)
        {
            saveLines[0] = "chapter: " + nextChapter;
            saveLines[1] = "scene: " + 0;
        }
        else
        {
            saveLines[0] = "chapter: " + SceneManager.GetActiveScene().name;
            saveLines[1] = "scene: " + index;
        }
        float savePlayed = updateSaveTime(save);
        saveLines[2] = "time: " + savePlayed;
        saveLines[3] = "lovePts: " + GameManager.instance.lovePoints[0] + ": " + GameManager.instance.lovePoints[1] + ": " + GameManager.instance.lovePoints[2];
        saveLines[4] = "items: " + (GameManager.instance.itemsAcquired[0]? 1 : 0) + ": " + (GameManager.instance.itemsAcquired[1] ? 1 : 0) + ": " + (GameManager.instance.itemsAcquired[2] ? 1 : 0) + ": " + (GameManager.instance.itemsAcquired[3] ? 1 : 0) + ": " + (GameManager.instance.itemsAcquired[4] ? 1 : 0);
        writeSave(saveLines, save);
        TimeSpan totalTime = TimeSpan.FromSeconds(savePlayed);
        string time = totalTime.ToString(@"hh\:mm\:ss");
        saveName[save].text = saveLines[0].Split(": ")[1] + " " + time;
        loadName[save].text = saveLines[0].Split(": ")[1] + " " + time;
    }

    public void load(int save)
    {
        GameManager.instance.timePlayed = Time.time;
        string[] saveText = readSave(save);
        string sceneName = saveText[0].Split(": ")[1];
        if (sceneName == "")
        {
            return;
        }
        for (int i = 0; i < 3; i++)
        {
            Int32.TryParse(saveText[3].Split(':')[i + 1], out GameManager.instance.lovePoints[i]);
        }
        for (int i = 0; i < 5; i++)
        {
            int hasItem;
            Int32.TryParse(saveText[4].Split(':')[i + 1], out hasItem);
            GameManager.instance.itemsAcquired[i] = Convert.ToBoolean(hasItem);
        }
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
