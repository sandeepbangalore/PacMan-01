using UnityEngine;
using System.Collections;

public class Controller : MonoBehaviour {
	public float Speed = 0.1f;

	private GameManager MyGameManager; 
	private Vector3 dest;
	private Rigidbody myRigidbody; 
	private float pacmanSpeed = 0.0f;
	private Vector3 up = new Vector3 (0.0f, 0.0f, -1.0f);
	private Vector3 down = new Vector3 (0.0f, 0.0f, 1.0f);
	private Vector3 right = new Vector3 (-1.0f, 0.0f, 0.0f);
	private Vector3 left = new Vector3 (1.0f, 0.0f, 0.0f);
	private Vector3 lastKey;
	private KeyCode currentKey = KeyCode.RightArrow;
	private int layerMask = 1 << 8 | 1 << 10;
	// Use this for initialization
	void Start () {
		transform.position = new Vector3 (0.5f, 0.5f, 1.5f);
		MyGameManager = GameManager.Instance;
		pacmanSpeed = Speed + (0.01f * (MyGameManager.Level()-1));
		//Debug.Log (transform.position);
		dest = transform.position;
		myRigidbody = (Rigidbody)GetComponent<Rigidbody> ();
		transform.rotation = Quaternion.Euler (0.0f, 180.0f, 0.0f);
		layerMask = ~layerMask;
	}

	void OnEnable () {
		MyGameManager = GameManager.Instance;
		transform.position = new Vector3 (0.5f, 0.5f, 1.5f);
		dest = transform.position;
		transform.rotation = Quaternion.Euler (0.0f, 180.0f, 0.0f);
		pacmanSpeed = Speed + (0.01f * (MyGameManager.Level()-1));
	}

	void Update () {
		if (MyGameManager.CurrentState == GameState.Playing) {
			if (Input.GetKeyDown (KeyCode.RightArrow)) {
				currentKey = KeyCode.RightArrow;
			} else if (Input.GetKeyDown (KeyCode.LeftArrow)) {
				currentKey = KeyCode.LeftArrow;
			} else if (Input.GetKeyDown (KeyCode.UpArrow)) {
				currentKey = KeyCode.UpArrow;
			} else if (Input.GetKeyDown (KeyCode.DownArrow)) {
				currentKey = KeyCode.DownArrow;
			}
		}
	}

	// Update is called once per frame
	void FixedUpdate () {
		if (MyGameManager.CurrentState == GameState.Playing) {
			
			Vector3 move = Vector3.MoveTowards (transform.position, dest, pacmanSpeed);
			myRigidbody.MovePosition (move);
			//Debug.Log (transform.position);
			if (transform.position == dest) {
				//Debug.Log (lastKey);
				//Debug.Log (currentKey);
				if ((currentKey == KeyCode.RightArrow) && valid (right)) {
					dest = transform.position + right;
					transform.rotation = Quaternion.Euler (0.0f, 180.0f, 0.0f);
					lastKey = right;
				} else if ((currentKey == KeyCode.LeftArrow) && valid (left)) {
					dest = transform.position + left;
					transform.rotation = Quaternion.Euler (0.0f, 0.0f, 0.0f);
					lastKey = left;
				} else if ((currentKey == KeyCode.UpArrow) && valid (up)) {
					dest = transform.position + up;
					transform.rotation = Quaternion.Euler (0.0f, 90.0f, 0.0f);
					lastKey = up;
				} else if ((currentKey == KeyCode.DownArrow) && valid (down)) {
					dest = transform.position + down;
					transform.rotation = Quaternion.Euler (0.0f, -90.0f, 0.0f);
					lastKey = down;
				} else if (valid (lastKey)) {
					dest = transform.position + lastKey;
				}
				if (dest == new Vector3 (15.5f, 0.5f, -1.5f) || dest == new Vector3 (-15.5f, 0.5f, -1.5f)) {
					dest.x = -transform.position.x;
					transform.position = dest;
				}
			}
		}
	}

	void OnTriggerEnter(Collider collision) {
		if (MyGameManager.CurrentState == GameState.Playing) {
			
			if (collision.gameObject.tag == "Pellets") {
				Destroy (collision.gameObject);
				MyGameManager.MinusPellets ();
				MyGameManager.AddPoints (10);
			}
			if (collision.tag == "Power Pellets") {
				Destroy (collision.gameObject);
				MyGameManager.isRetreating ();
				MyGameManager.MinusPellets ();
				MyGameManager.AddPoints (50);
			}
		}
	}

	bool valid (Vector3 direction){
		Vector3 position = transform.position;
		RaycastHit hit;
		Physics.Raycast (position, direction, out hit, 1.1f, layerMask);
		return (hit.collider == null);
	}
}
