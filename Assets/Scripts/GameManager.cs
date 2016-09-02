using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using UIFramework;

public class GameManager : MonoBehaviour {

    public GameManager Instance
    {
        get { return instance; }
    }
    private GameManager instance;

    void Awake()
    {
        instance = this;
        DontDestroyOnLoad(gameObject);
    }

	void Start () {



        //WindowManager.Instance.LoadWindow<TopWindow>();
        //WindowManager.Instance.LoadWindow<MainWindow>();       
	}


    public static void Login2Home()
    {

    }

    public static void Home2Battle()
    {

    }

    public static void Home2Login()
    {

    }

    public static void Battle2
}
