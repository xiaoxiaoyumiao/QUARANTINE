using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LevelBlock : MonoBehaviour
{
    private LevelManager manager;
    private int index;

    public string levelName;
    public string sceneName;
    public int score;
    // Start is called before the first frame update
    void Start()
    {
        manager = GameObject.Find("LevelManager").GetComponent<LevelManager>();
        index = manager.RegisterLevel(this);
    }

    void OnClick()
    {
        manager.OnLevelClicked(this.index);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
