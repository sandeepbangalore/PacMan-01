using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

// ---------------------------------------------------------------------------------------
// Class : SceneManager_TitleScreen
// Desc	 : Manages the sequence of events on the title screen and reacts to button 
//		   button selections
// ---------------------------------------------------------------------------------------
public class Screen_Manager_TitleScreen : Screen_Manager_Base
{
	public GameObject MenuObject	=	null;
	private Screen_Manager_TitleScreen MyScreenManager = null;
	protected static Screen_Manager_TitleScreen _Instance = null;

	void Awake()
	{
		MyScreenManager = Screen_Manager_TitleScreen.Instance;
	}

	// ------------------------------------------------------------------------------------
	// Name	:	Start
	// Desc	:	Called once prior to the first update
	// ------------------------------------------------------------------------------------
	void Start()
	{
		// Enabel the showing of the mouse cursor so we can select menu items
		Screen.lockCursor = false;
		StartCoroutine (StartGame ());
	}

	public static Screen_Manager_TitleScreen Instance {
		get{ 
			if (_Instance == null) 
				_Instance = (Screen_Manager_TitleScreen)FindObjectOfType(typeof(Screen_Manager_TitleScreen));
			return _Instance;
		}
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
	}

	public override void OnButtonSelect( string buttonName ){
			if(ActionSelected) return;

			if (buttonName == "Play") {
				ActionSelected = true;
				StartCoroutine( LoadGameScene() );
			}else{
				if(buttonName == "Quit" ){
					ActionSelected = true;
					StartCoroutine( QuitGame() );
				}
			}
	}

	// ------------------------------------------------------------------------------------
	// Name	:	LoadGameScene
	// Desc	:	Fades out the menu, animates the camera through the hole in the wall, and
	//			then load in the main game.
	// ------------------------------------------------------------------------------------
	private IEnumerator LoadGameScene()
	{
		// Get all the renderers of the menu object
		Renderer[] renderers = MenuObject.GetComponentsInChildren<Renderer>();	

		// Perform a 1 second fade-out of the menu
		float timer = 1.0f;

		// While the 1 second has still not expired
		while (timer>0.0f)
		{
			// Update the timer
			timer-=Time.deltaTime;

			// Loop through each renderer in the menu object
			foreach( Renderer r in renderers )
			{
				if (r && r.material)
				{
					// Fetch the material of the renderer
					Color col = r.material.color;

					// set the alpha of the material to the timer value
					col.a = timer;
					r.material.color = col;	
				}
			}

			// Yield control
			yield return null;
		}
		// Now do a 1.5 second animation of the camera
		timer = 1.5f;

		// While the timer has not expired
		while (timer>0.0f)
		{
			// Decrement timer
			timer-=Time.deltaTime;

			// Set the screen fade
			if (timer>=0.0f) ScreenFade = 1.0f-(timer/1.5f);

     		// Yield control
			yield return null;
		}

		// We have now faded out the menu and moved the camera between the hole in the wall
		// so we are now in a black-out and can load the game scene.
		SceneManager.LoadScene ("Game");
	}

	// ------------------------------------------------------------------------------------
	// Name	:	QuitGame (IEnumerator)
	// Desc	:	Performs a 2.5 second fade-out of the scene and then loads the closing
	//			credits scene.
	// ------------------------------------------------------------------------------------
	private IEnumerator QuitGame()
	{
		// Set the initial timer to 2.5 seconds
		float timer = 2.5f;

		// While the timer has not reached zero
		while (timer>0.0f)
		{
			// Deduct elapsed time from timer
			timer-=Time.deltaTime;

			// Map current timer into 0.1 range and invert to
			// set the current screen fade strength.
			if (timer>=0.0f) ScreenFade = 1.0f-(timer/2.5f);

			// Yield control
			yield return null;
		}	

		// Fade out complete so load in the closing credits.
		Application.Quit();
	}
}
