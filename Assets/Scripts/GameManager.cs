using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UIFramework;

public class GameManager : MonoBehaviour {

    public static GameManager Instance
    {
        get { return instance; }
    }
    private static GameManager instance;

    private Text progressText;

    public AsyncOperation loadingSceneOpt;

    void Awake()
    {
        instance = this;
        DontDestroyOnLoad(gameObject);

        progressText = WindowManager.Instance.loadingUI.transform.Find("Text").GetComponent<Text>();
    }

	void Start () {
        SceneManager.sceneLoaded += OnSceneLoaded;
        SceneManager.sceneUnloaded += OnSceneUnloaded;


        WindowManager.Instance.LoadWindow<LoginWindow>();
    }

    void Update()
    {
        if (loadingSceneOpt != null)
        {
            progressText.text = string.Format("{0}%", loadingSceneOpt.progress);
        }
    }

    private void OnSceneUnloaded(Scene scene)
    {
        Debug.LogFormat("Unload scene : {0}", scene.name);

        GC();

        if (scene.name == "Login" || scene.name == "Battle")
        {
            WindowManager.Instance.Clear(false);
        }
        else if (scene.name == "Home")
        {
            WindowManager.Instance.Clear(true);
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.LogFormat("Load scene : {0}", scene.name);
        if (scene.name == "Login")
        {
            WindowManager.Instance.savedWindows.Clear();
            WindowManager.Instance.LoadWindow<LoginWindow>();
        }
        else if (scene.name == "Home")
        {
            if (WindowManager.Instance.savedWindows.Count == 0)
            {
                WindowManager.Instance.LoadWindow<TopWindow>();
                WindowManager.Instance.LoadWindow<MainWindow>();
            }
            else
            {
                WindowManager.Instance.RestoreWindows();
            }
        }
        else if (scene.name == "Battle")
        {
            WindowManager.Instance.LoadWindow<BattleWindow>();
        }
    }

    public void GotoHome()
    {
        StartCoroutine(LoadScene("Home"));
    }

    public void GotoBattle()
    {
        StartCoroutine(LoadScene("Battle"));
    }

    public void GotoLogin()
    {
        StartCoroutine(LoadScene("Login"));
    }

    private IEnumerator LoadScene(string name)
    {
        WindowManager.Instance.loadingUI.SetActive(true);
        loadingSceneOpt = SceneManager.LoadSceneAsync(name);
        yield return loadingSceneOpt;
        WindowManager.Instance.loadingUI.SetActive(false);
    }

    private void GC()
    {
        System.GC.Collect();
        Resources.UnloadUnusedAssets();
    }
}
