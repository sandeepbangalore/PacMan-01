using UnityEngine;
using System.Collections;

public class Screen_Manager_Base1 : MonoBehaviour {
	public bool ActionSelected = false;
	protected float ScreenFade = 1.0f;
	//protected Screen_Manager_TitleScreen MyTitleManager = GetComponent<Screen_Manager_TitleScreen>();

	protected static Screen_Manager_Base1 _Instance = null;
	public static Screen_Manager_Base1 Instance {
		get{ 
			if (_Instance == null) 
				_Instance = (Screen_Manager_Base1)FindObjectOfType(typeof(Screen_Manager_Base1));
			return _Instance;
		}
	}
	public float GetScreenFade(){
		return ScreenFade;
	}
	public void SetScreenFade(float fade) {
		ScreenFade = fade;
	}	

	public virtual void OnButtonHover ( string buttonName ){}
	public virtual void OnButtonSelect( string buttonName ){}
}
