using UnityEngine;
using System.Collections;
using UIFramework;

public class GameManager : MonoBehaviour {

	// Use this for initialization
	void Start () {
        WindowManager.Instance.LoadWindow<TopWindow>();
        WindowManager.Instance.LoadWindow<MainWindow>();
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
