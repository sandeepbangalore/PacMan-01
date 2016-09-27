using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class Loading : MonoBehaviour {

	private Screen_Manager_Base MyScreenManager = null;

	void Awake (){
		MyScreenManager = Screen_Manager_Base.Instance;
	}

	// Use this for initialization
	void Start () {
		Screen.lockCursor = true;
		StartCoroutine (StartGame ());
	}
	
	private IEnumerator StartGame () {
		float screenTimer = 2.0f;
		while (screenTimer >= 0.0f) {
			screenTimer -= Time.deltaTime;
			if (MyScreenManager != null)
				MyScreenManager.SetScreenFade (screenTimer / 2.0f);
			//AudioListener.volume = 1.0f - screenTimer/2.0f;
			yield return null;
		}
		screenTimer = 2.0f;
		while (screenTimer >= 0.0f) {
			screenTimer -= Time.deltaTime;
			yield return null;
		}
		Screen.lockCursor = false;
		SceneManager.LoadScene ("Menu");
	}
}
