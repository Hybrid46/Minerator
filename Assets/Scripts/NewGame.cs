using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class NewGame : MonoBehaviour
{
    public Slider sliderSld;
    public Text loadingText;
    public void NewGameScene()
    {
        sliderSld.transform.localScale = new Vector3(1, 1, 1);
        loadingText.transform.localScale = new Vector3(1, 1, 1);
        StartCoroutine(LoadAsynchonously());
    }
    IEnumerator LoadAsynchonously()
    {
        AsyncOperation operation = SceneManager.LoadSceneAsync("Game");

        while (!operation.isDone)
        {
            float progress = Mathf.Clamp01(operation.progress/.9f);
            sliderSld.value = operation.progress;
            yield return null;
        }
    }
}