using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class InitializeSettings : MonoBehaviour {

	void Start()
	{
		PlayerPrefs.SetFloat("size", 35f);
		PlayerPrefs.SetInt("value", 1);
		
		SceneManager.LoadScene(1);
	}
}
