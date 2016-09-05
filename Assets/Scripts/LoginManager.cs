using UnityEngine;
using System.Collections;
using UIFramework;

public class LoginManager : MonoBehaviour {

    public GameObject UIRoot;
    public GameObject Manager;

	// Use this for initialization
	void Start () {

        if (WindowManager.Instance == null)
        {
            Instantiate(UIRoot);
        }
        if (GameManager.Instance == null)
        {
            Instantiate(Manager);
        }
	}	
}
