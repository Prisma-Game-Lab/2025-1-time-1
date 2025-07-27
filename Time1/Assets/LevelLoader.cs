using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelLoader : MonoBehaviour
{
    public Animator transition;

    public void LoadScene(string sceneName)
    {
        StartCoroutine(LoadSceneWithTransition(sceneName));
    }

    IEnumerator LoadSceneWithTransition(string sceneName)
    {
        if (transition != null)
        {
            transition.SetTrigger("Start");
            yield return new WaitForSeconds(2);
        }
        SceneManager.LoadScene(sceneName);
    }
}
