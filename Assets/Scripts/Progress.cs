using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Progress : MonoBehaviour
{

    private AsyncOperation operation;
    private Transform bar;
    private Text text;
    int currentProgress;

    // Start is called before the first frame update
    void Start()
    {
        GameObject image = GameObject.Find("Progress");
        bar = image.transform;
        Debug.Log(bar.name);
        text = GetComponentInChildren<Text>();
        currentProgress = 0;

        if (SceneManager.GetActiveScene().name == "Progressing")
        {
            StartCoroutine(AsyncLoading());
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (operation == null) return;
        int progressVal;
        if (operation.progress < 0.9f)
            progressVal = (int)(operation.progress * 100);
        else progressVal = 100;
        if (currentProgress < progressVal)
        {
            currentProgress++;
        }

        Vector3 scale = bar.localScale;
        scale.x = currentProgress / 100.0f;
        bar.localScale = scale;
        text.text = currentProgress.ToString() + "%";

        if (currentProgress == 100)
        {
            operation.allowSceneActivation = true;
        }
    }

    private void OnGUI()
    {
        
    }

    IEnumerator AsyncLoading()
    {
        operation = SceneManager.LoadSceneAsync(LevelManager.selectedLevel);
        operation.allowSceneActivation = false; // ban auto switching
        yield return operation;
    }


}
