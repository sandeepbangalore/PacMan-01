using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public enum GameState { GetReady, Playing, Paused, GameOver, DeathPeriod }
//public enum GhostState { Chasing,Scattering,Waiting,Retreating,Dead}

public class GameManager : MonoBehaviour {

	public GameObject Blinky = null;
	public GameObject Pinky = null;
	public GameObject Inky = null;
	public GameObject Clyde = null;
	public Transform Pacman = null;
	public int PacmanLives = 3;
	public GameState CurrentState = GameState.GetReady;
	public GhostState BlinkyInitialState = GhostState.Chasing;
	public GhostState PinkyInitialState = GhostState.Waiting;
	public GhostState InkyInitialState = GhostState.Waiting;
	public GhostState ClydeInitialState = GhostState.Waiting;
	public float chasingTime = 20.0f;
	public Text ScoreText = null;
	public Text HiScoreText = null;
	public Text GetReadyText = null;
	public Text LevelText = null;
	public Text GameOverText = null;
	public int ghostScore = 100;
	public GUISkin GUISkin_user = null;
	public float fadeTime = 2.0f;
	public AudioSource beginSound = null;
	public AudioSource backSound = null;

	private Screen_Manager_Base MyScreenManager = null;
	private int gameLevel = 1;
	private int StartPellets = 0;
	private int pelletsInGame = 0;
	private int multiplier;
	private bool isChasing = false;
	private bool isScattering = false;
	public bool isRetreat = false;
	private Dictionary<string,Timer> Timers = new Dictionary<string, Timer>();
	private Dictionary<string,GhostState> GhostStates = new Dictionary<string, GhostState>();
	Dictionary<string,GhostState> OldGhostStates;
	private int Score = 0;
	private int HiScore = 0;

	private Rect PauseBox_ScreenRect = new Rect (700,450,520,320);
	private Rect Continue_ScreenRect = new Rect (800,550,320,60);
	private Rect Quit_ScreenRect = new Rect (800,650,320,60);

	private static GameManager _Instance = null;
	public static GameManager Instance{
		get{ 
			if (_Instance == null) {
				_Instance = (GameManager)FindObjectOfType (typeof(GameManager));
			}
			return _Instance;	
		}
	}

	public void ResetTimer (string TimerName) {
		Timer timer;
		if (Timers.TryGetValue (TimerName, out timer))
			timer.Reset();
	}

	public void RegisterTimer (string TimerName) {
		if (!Timers.ContainsKey (TimerName)) {
			Timers.Add (TimerName, new Timer ());
		}
	}

	public void UpdateTimer (string TimerName, float t ){
		Timer timer;
		if (Timers.TryGetValue (TimerName, out timer))
			timer.AddTime (t);
	}

	public float GetTime (string TimerName) {
		Timer timer; 
		if (Timers.TryGetValue (TimerName, out timer)) {
			return timer.GetTime();
		}
		return -1.0f;
	}

	// Use this for initialization
	void Awake () {
		RegisterTimer ("Chasing Timer");
		RegisterTimer ("Scatter Timer");
		RegisterTimer ("Retreating Timer");
		UpdateTimer ("Chasing Timer", chasingTime);
		GhostCurrentState ("Blinky", BlinkyInitialState);
		GhostCurrentState ("Pinky", PinkyInitialState);
		GhostCurrentState ("Inky", InkyInitialState);
		GhostCurrentState ("Clyde", ClydeInitialState);
		OldGhostStates = new Dictionary<string, GhostState> (GhostStates);
		isChasing = true;
		HiScore = PlayerPrefs.GetInt ("HiScore", 0);
		if (ScoreText != null) 
			ScoreText.text = Score.ToString ();
		if (HiScoreText != null)
			HiScoreText.text = HiScore.ToString ();
		MyScreenManager = Screen_Manager_Base.Instance;
	}

	public void AddPoints(int points) {
		if (CurrentState == GameState.Playing) {
			Score += points;
			if (ScoreText != null)
				ScoreText.text = Score.ToString ();
			if (Score > HiScore)
				HiScoreText.text = Score.ToString ();
		}
	}

	void Start () {
		Screen.lockCursor = true;
		StartCoroutine (StartGame ());
	}

	// Update is called once per frame
	void FixedUpdate () {
		if (CurrentState == GameState.Playing) {
			if (backSound != null && !backSound.isPlaying)
				backSound.Play ();
			if (pelletsInGame <= 0){
				NextLevel ();
			}
			if (pelletsInGame == (StartPellets - 2)) {
				GhostStates ["Pinky"] = GhostState.WaitTransition;
				OldGhostStates ["Pinky"] = GhostState.WaitTransition;
			}
			if (pelletsInGame == (StartPellets - 50)) {
				GhostStates ["Inky"] = GhostState.WaitTransition;
				OldGhostStates ["Inky"] = GhostState.WaitTransition;
			}
			if (pelletsInGame == (StartPellets - 90)) {
				GhostStates ["Clyde"] = GhostState.WaitTransition;
				OldGhostStates ["Clyde"] = GhostState.WaitTransition;
			}
			//Debug.Log (StartPellets);
			//Debug.Log (pelletsInGame);
			multiplier = 1;
			if (GetTime ("Retreating Timer") > 0.0f && isRetreat) {
				backSound.pitch = 1.5f;
				List<string> keys = new List<string> (GhostStates.Keys);
				foreach (string entry in keys) {
					if (GhostStates [entry] != GhostState.Dead && GhostStates [entry] != GhostState.Waiting && GhostStates [entry] != GhostState.WaitTransition && GhostStates [entry] != GhostState.Alive)
						GhostCurrentState (entry, GhostState.Retreating);
				}
				if (isChasing)
					UpdateTimer ("Chasing Timer", Time.deltaTime);
				if (isScattering)
					UpdateTimer ("Scattering Timer", Time.deltaTime);
				//Debug.Log (GetTime ("Retreating Timer"));
				if (GetTime ("Retreating Timer") < 2.0f && GetTime ("Retreating Timer") > 1.5f) {
					if (GhostStates ["Blinky"] == GhostState.Retreating || GhostStates ["Blinky"] == GhostState.Waiting || GhostStates ["Blinky"] == GhostState.WaitTransition) {
						Blinky.transform.FindChild ("ghost_1").gameObject.SetActive (true);
						Blinky.transform.FindChild ("scared").gameObject.SetActive (false);
					}
					if (GhostStates ["Pinky"] == GhostState.Retreating || GhostStates ["Pinky"] == GhostState.Waiting || GhostStates ["Pinky"] == GhostState.WaitTransition) {
						Pinky.transform.FindChild ("ghost_1").gameObject.SetActive (true);
						Pinky.transform.FindChild ("scared").gameObject.SetActive (false);
					}
					if (GhostStates ["Inky"] == GhostState.Retreating || GhostStates ["Inky"] == GhostState.Waiting || GhostStates ["Inky"] == GhostState.WaitTransition) {
						Inky.transform.FindChild ("ghost_1").gameObject.SetActive (true);
						Inky.transform.FindChild ("scared").gameObject.SetActive (false);
					}
					if (GhostStates ["Clyde"] == GhostState.Retreating || GhostStates ["Clyde"] == GhostState.Waiting || GhostStates ["Clyde"] == GhostState.WaitTransition) {
						Clyde.transform.FindChild ("ghost_1").gameObject.SetActive (true);
						Clyde.transform.FindChild ("scared").gameObject.SetActive (false);
					}


				} else if (GetTime ("Retreating Timer") < 1.5f && GetTime ("Retreating Timer") > 1.0f) {
					if (GhostStates ["Blinky"] == GhostState.Retreating || GhostStates ["Blinky"] == GhostState.Waiting || GhostStates ["Blinky"] == GhostState.WaitTransition) {
						Blinky.transform.FindChild ("ghost_1").gameObject.SetActive (false);
						Blinky.transform.FindChild ("scared").gameObject.SetActive (true);
					}
					if (GhostStates ["Pinky"] == GhostState.Retreating || GhostStates ["Pinky"] == GhostState.Waiting || GhostStates ["Pinky"] == GhostState.WaitTransition) {
						Pinky.transform.FindChild ("ghost_1").gameObject.SetActive (false);
						Pinky.transform.FindChild ("scared").gameObject.SetActive (true);
					}
					if (GhostStates ["Inky"] == GhostState.Retreating || GhostStates ["Inky"] == GhostState.Waiting || GhostStates ["Inky"] == GhostState.WaitTransition) {
						Inky.transform.FindChild ("ghost_1").gameObject.SetActive (false);
						Inky.transform.FindChild ("scared").gameObject.SetActive (true);
					}
					if (GhostStates ["Clyde"] == GhostState.Retreating || GhostStates ["Clyde"] == GhostState.Waiting || GhostStates ["Clyde"] == GhostState.WaitTransition) {
						Clyde.transform.FindChild ("ghost_1").gameObject.SetActive (false);
						Clyde.transform.FindChild ("scared").gameObject.SetActive (true);
					}
				} else if (GetTime ("Retreating Timer") < 1.0f && GetTime ("Retreating Timer") > 0.5f) {
					if (GhostStates ["Blinky"] == GhostState.Retreating || GhostStates ["Blinky"] == GhostState.Waiting || GhostStates ["Blinky"] == GhostState.WaitTransition) {
						Blinky.transform.FindChild ("ghost_1").gameObject.SetActive (true);
						Blinky.transform.FindChild ("scared").gameObject.SetActive (false);
					}
					if (GhostStates ["Pinky"] == GhostState.Retreating || GhostStates ["Pinky"] == GhostState.Waiting || GhostStates ["Pinky"] == GhostState.WaitTransition) {
						Pinky.transform.FindChild ("ghost_1").gameObject.SetActive (true);
						Pinky.transform.FindChild ("scared").gameObject.SetActive (false);
					}
					if (GhostStates ["Inky"] == GhostState.Retreating || GhostStates ["Inky"] == GhostState.Waiting || GhostStates ["Inky"] == GhostState.WaitTransition) {
						Inky.transform.FindChild ("ghost_1").gameObject.SetActive (true);
						Inky.transform.FindChild ("scared").gameObject.SetActive (false);
					}
					if (GhostStates ["Clyde"] == GhostState.Retreating || GhostStates ["Clyde"] == GhostState.Waiting || GhostStates ["Clyde"] == GhostState.WaitTransition) {
						Clyde.transform.FindChild ("ghost_1").gameObject.SetActive (true);
						Clyde.transform.FindChild ("scared").gameObject.SetActive (false);
					}
				} else if (GetTime ("Retreating Timer") < 0.5f && GetTime ("Retreating Timer") > 0.0f) {
					if (GhostStates ["Blinky"] == GhostState.Retreating || GhostStates ["Blinky"] == GhostState.Waiting || GhostStates ["Blinky"] == GhostState.WaitTransition) {
						Blinky.transform.FindChild ("ghost_1").gameObject.SetActive (false);
						Blinky.transform.FindChild ("scared").gameObject.SetActive (true);
					}
					if (GhostStates ["Pinky"] == GhostState.Retreating || GhostStates ["Pinky"] == GhostState.Waiting || GhostStates ["Pinky"] == GhostState.WaitTransition) {
						Pinky.transform.FindChild ("ghost_1").gameObject.SetActive (false);
						Pinky.transform.FindChild ("scared").gameObject.SetActive (true);
					}
					if (GhostStates ["Inky"] == GhostState.Retreating || GhostStates ["Inky"] == GhostState.Waiting || GhostStates ["Inky"] == GhostState.WaitTransition) {
						Inky.transform.FindChild ("ghost_1").gameObject.SetActive (false);
						Inky.transform.FindChild ("scared").gameObject.SetActive (true);
					}
					if (GhostStates ["Clyde"] == GhostState.Retreating || GhostStates ["Clyde"] == GhostState.Waiting || GhostStates ["Clyde"] == GhostState.WaitTransition) {
						Clyde.transform.FindChild ("ghost_1").gameObject.SetActive (false);
						Clyde.transform.FindChild ("scared").gameObject.SetActive (true);
					}
				}
			}

			if (GetTime ("Retreating Timer") <= 0 && isRetreat) {
				backSound.pitch = 1.0f;
				List<string> keys = new List<string> (GhostStates.Keys);
				foreach (string entry in keys) {
					if (OldGhostStates [entry] == GhostState.Alive)
						GhostCurrentState (entry, GhostState.Chasing);
					else
						GhostCurrentState (entry, OldGhostStates [entry]);
				}
				isRetreat = false;
				ghostScore = 100;
				Debug.Log (isRetreat);
				if (GhostStates ["Blinky"] != GhostState.Dead) {
					Blinky.transform.FindChild ("ghost_1").gameObject.SetActive (true);
					Blinky.transform.FindChild ("scared").gameObject.SetActive (false);
				}
				if (GhostStates ["Pinky"] != GhostState.Dead) {
					Pinky.transform.FindChild ("ghost_1").gameObject.SetActive (true);
					Pinky.transform.FindChild ("scared").gameObject.SetActive (false);
				}
				if (GhostStates ["Inky"] != GhostState.Dead) {
					Inky.transform.FindChild ("ghost_1").gameObject.SetActive (true);
					Inky.transform.FindChild ("scared").gameObject.SetActive (false);
				}
				if (GhostStates ["Clyde"] != GhostState.Dead) {
					Clyde.transform.FindChild ("ghost_1").gameObject.SetActive (true);
					Clyde.transform.FindChild ("scared").gameObject.SetActive (false);
				}
			}

			if (GetTime ("Chasing Timer") <= 0 && isChasing) {
				List<string> keys = new List<string> (GhostStates.Keys);
				foreach (string entry in keys) {
					if (GhostStates [entry] == GhostState.Chasing) {
						GhostCurrentState (entry, GhostState.Scattering);
						//Debug.Log(entry.Value);
					}
				}
				UpdateTimer ("Scatter Timer", Random.Range (3.0f, 7.0f));
				isChasing = false;
				isScattering = true;
				multiplier = -1;
			}
			if (GetTime ("Scatter Timer") <= 0 && isScattering) {
				List<string> keys = new List<string> (GhostStates.Keys);
				foreach (string entry in keys) {
					if (GhostStates [entry] == GhostState.Scattering) {
						GhostCurrentState (entry, GhostState.Chasing);
						//Debug.Log(entry.Value);
					}			
				}
				UpdateTimer ("Chasing Timer", chasingTime);
				isScattering = false;
				isChasing = true;
			}
		}
	}

	void Update () {

		if (Input.GetKeyDown (KeyCode.Escape)) {
			if (CurrentState == GameState.Playing) {
				CurrentState = GameState.Paused;
				Time.timeScale = 0.0f;
				Screen.lockCursor = false;
				if (MyScreenManager)
					MyScreenManager.SetScreenFade (0.7f);
				
			} else if (CurrentState == GameState.Paused) {
				CurrentState = GameState.Playing;
				//Time.timeScale = 1.0f;
				Screen.lockCursor = true;

			}
		}
		if (Input.GetKeyDown (KeyCode.F)) {
			Screen.fullScreen = true;
		}
		if (PacmanLives <= 0) {
			EndGame ();
			return;
		}
		foreach ( KeyValuePair<string, Timer> entry in Timers ) {
			entry.Value.Tick (Time.deltaTime);
		}
	}

	public void UnregisterLife() {
		PacmanLives--;
	}

	public void RegisterLife() {
		PacmanLives++;
	}

	public void GhostCurrentState(string ghostName, GhostState curr) {
		//GhostState gs;
		if (!GhostStates.ContainsKey (ghostName)) {
			GhostStates.Add (ghostName, curr);
		}
//		if (GhostStates.TryGetValue (ghostName, out gs)) {
//			gs = curr;
//		}
		GhostStates [ghostName] = curr;
		//Debug.Log (GhostStates [ghostName]);
	}

	public GhostState GetGhostState(string ghostName){
		return GhostStates [ghostName];
	}

	public void EndGame() {
		if (beginSound != null && beginSound.isPlaying)
			beginSound.Stop ();
		if (backSound != null && backSound.isPlaying)
			backSound.Stop ();
		if (CurrentState != GameState.GameOver) {
			CurrentState = GameState.GameOver;
			if (Score > HiScore)
				PlayerPrefs.SetInt ("HiScore", Score);
			StartCoroutine (GoToMenu (fadeTime));
		}	
	}

	public int dirUpdate(){
		return multiplier;
	}

	public void isRetreating () {
		UpdateTimer ("Retreating Timer", Random.Range(4.0f,7.0f));
		if(!isRetreat)
			OldGhostStates = new Dictionary<string, GhostState> (GhostStates);
		isRetreat = true;
		if (GhostStates ["Blinky"] != GhostState.Dead ) {
			Blinky.transform.FindChild ("ghost_1").gameObject.SetActive (false);
			Blinky.transform.FindChild ("scared").gameObject.SetActive (true);
		}
		if (GhostStates ["Pinky"] != GhostState.Dead ) {
			Pinky.transform.FindChild ("ghost_1").gameObject.SetActive (false);
			Pinky.transform.FindChild ("scared").gameObject.SetActive (true);
		}
		if (GhostStates ["Inky"] != GhostState.Dead ) {
			Inky.transform.FindChild ("ghost_1").gameObject.SetActive (false);
			Inky.transform.FindChild ("scared").gameObject.SetActive (true);
		}
		if (GhostStates ["Clyde"] != GhostState.Dead ) {
			Clyde.transform.FindChild ("ghost_1").gameObject.SetActive (false);
			Clyde.transform.FindChild ("scared").gameObject.SetActive (true);
		}
		multiplier = -1;
	}

	public float getGhostSpeed () {
		if (isRetreat)
			return 0.06f  + (0.01f * (gameLevel-1));
		else
			return 0.1f + (0.01f * (gameLevel-1));
	}

	public void isDead (string ghostName){
		if(GhostStates[ghostName] != GhostState.Dead)
			ghostScore += 100;
		Debug.Log (ghostScore);
		GhostStates [ghostName] = GhostState.Dead;
		OldGhostStates [ghostName] = GhostState.Dead;
	}

	public void isAlive (string ghostName){
		GhostStates [ghostName] = GhostState.Alive;
		OldGhostStates [ghostName] = GhostState.Chasing;
	}

	public void isActive (string ghostName){
		GhostStates [ghostName] = GhostState.Chasing;
		OldGhostStates [ghostName] = GhostState.Chasing;
	}

	public void AddPellets (int adder){
		pelletsInGame += adder;
	}

	public void MinusPellets (){
		pelletsInGame--;
	}

	public void TotalPellets () {
		StartPellets = pelletsInGame;
	}

	private IEnumerator DeathWait () {
		float Deathtime = 2.0f;
		CurrentState = GameState.DeathPeriod;
		while (Deathtime > 0f) {
			Deathtime -= Time.deltaTime;
			yield return null;
		}
		CurrentState = GameState.Playing;
		if (PacmanLives > 0 ) {
			Debug.Log (CurrentState);
			Blinky.SetActive (false);
			Blinky.SetActive (true);
			Pinky.SetActive (false);
			Pinky.SetActive (true);
			Inky.SetActive (false);
			Inky.SetActive (true);
			Clyde.SetActive (false);
			Clyde.SetActive (true);
			StartPellets = pelletsInGame;
			GhostStates ["Blinky"] = BlinkyInitialState;
			GhostStates ["Pinky"] = PinkyInitialState;
			GhostStates ["Inky"] = InkyInitialState;
			GhostStates ["Clyde"] = ClydeInitialState;
			ResetTimer ("Chasing Timer");
			ResetTimer ("Scatter Timer");
			ResetTimer ("Retreating Timer");
			UpdateTimer ("Chasing Timer", chasingTime);
			isChasing = true;
			isScattering = false;
			isRetreat = false;
			CurrentState = GameState.GetReady;
			StartCoroutine (StartGame ());
			Pacman.gameObject.SetActive (true);
		}
	}

	public void ResetLevel () {
		UnregisterLife ();
		StartCoroutine (DeathWait ());
	}

	public void NextLevel () {
		Blinky.SetActive (false);
		Blinky.SetActive (true);
		Pinky.SetActive (false);
		Pinky.SetActive (true);
		Inky.SetActive (false);
		Inky.SetActive (true);
		Clyde.SetActive (false);
		Clyde.SetActive (true);
		GhostStates ["Blinky"] = BlinkyInitialState;
		GhostStates ["Pinky"] = PinkyInitialState;
		GhostStates ["Inky"] = InkyInitialState;
		GhostStates ["Clyde"] = ClydeInitialState;
		GetComponent<Pellets_creator>().enabled = false;
		GetComponent<Pellets_creator>().enabled = true;
		Pacman.gameObject.SetActive (false);
		Pacman.gameObject.SetActive (true);
		ResetTimer ("Chasing Timer");
		ResetTimer ("Scatter Timer");
		ResetTimer ("Retreating Timer");
		UpdateTimer ("Chasing Timer", chasingTime);
		isChasing = true;
		isScattering = false;
		isRetreat = false;
		gameLevel++;
		CurrentState = GameState.GetReady;
		StartCoroutine (StartGame ());

	}

	public int Level () {
		return gameLevel;
	}

	private IEnumerator StartGame () {
		if (backSound != null && backSound.isPlaying)
			backSound.Stop ();
		if (beginSound != null && !beginSound.isPlaying)
			beginSound.Play ();
		float screenTimer = 2.0f;
		while (screenTimer >= 0.0f) {
			screenTimer -= Time.deltaTime;
			if (MyScreenManager != null)
				MyScreenManager.SetScreenFade (screenTimer / 2.0f);
			//AudioListener.volume = 1.0f - screenTimer/2.0f;
			yield return null;

		}
		Color FadeColor = Color.white;
		Color FadeMaterial = Color.clear;
		Color FadeMaterial1 = Color.clear;
		screenTimer = 3.0f;
		if (GetReadyText != null && GetReadyText.GetComponent<Text> () != null && LevelText != null && LevelText.GetComponent<Text> () != null) {
			GetReadyText.gameObject.SetActive(true);
			LevelText.text = "Level " + gameLevel.ToString();
			LevelText.gameObject.SetActive(true);
			FadeMaterial = GetReadyText.GetComponent<Text> ().color;
			FadeMaterial1 = LevelText.GetComponent<Text> ().color;
		}
		while (screenTimer >= 0.0f) {
			screenTimer -= Time.deltaTime;
			if (GetReadyText != null && FadeMaterial != null && LevelText != null && FadeMaterial1 != null) {
				FadeColor.a = 2.0f - screenTimer;
				FadeMaterial = FadeColor;
				FadeMaterial1 = FadeColor;
			}
			yield return null;
		}
		GetReadyText.gameObject.SetActive(false);
		LevelText.gameObject.SetActive(false);
		CurrentState = GameState.Playing;
	}

	private IEnumerator GoToMenu(float duration = 2.0f){
		float timer = duration;
		Color FadeColor = Color.white;
		Color FadeMaterial = Color.clear;

		if (GameOverText != null && GameOverText.GetComponent<Text> () != null) {
			GameOverText.gameObject.SetActive (true);
			FadeMaterial = GameOverText.GetComponent<Text> ().color;
		}

		while (timer >= 0.0f) {
			timer -= Time.deltaTime;
			if (GameOverText != null && FadeMaterial != null) {
				FadeColor.a = duration - timer;
				FadeMaterial = FadeColor;
			}
			AudioListener.volume = timer/duration;
			yield return null;
		}
		GameOverText.gameObject.SetActive(false);

		timer = duration;
		while (timer >= 0.0f) {
			timer -= Time.deltaTime;
			if (MyScreenManager != null ) {
				MyScreenManager.SetScreenFade (1.0f - (timer / duration));
			}
			yield return null;
		}
		Screen.lockCursor = false;
		Application.LoadLevel ("MainMenu");
	}


	void OnGUI(){
		if (CurrentState != GameState.Paused)
			return;
		if (GUISkin_user)
			GUI.skin = GUISkin_user;
		float x = Screen.width / 1920.0f;
		float y = Screen.height / 1280.0f;

		Matrix4x4 oldMat = GUI.matrix;
		GUI.matrix = Matrix4x4.TRS (new Vector3 (0,0,0), Quaternion.identity, new Vector3 (x, y, 1.0f));
		GUI.Box (PauseBox_ScreenRect, "Game Paused");
		if(GUI.Button(Continue_ScreenRect, "Continue")){
			CurrentState = GameState.Playing;
			Screen.lockCursor = true;
			Time.timeScale = 1.0f;
			if(MyScreenManager)
				MyScreenManager.SetScreenFade(0.0f);
			//if (BrickLoweringAudio)
			//	BrickLoweringAudio.volume = WallLoweringVolume;
		}
		if (GUI.Button (Quit_ScreenRect, "Quit")) {
			Time.timeScale = 1.0f;
			EndGame ();
		}
		GUI.matrix = oldMat;

	}


}