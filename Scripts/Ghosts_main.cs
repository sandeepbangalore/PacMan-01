using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum GhostState{Chasing,Scattering,Waiting,Retreating,Dead,WaitTransition,Alive}
public class Ghosts_main : MonoBehaviour {

	public float ghostSpeed = 0.1f;
	public GameObject pacman = null;
	public GhostState currentState;
	public AudioSource ghostDeath = null;
	public AudioSource pacmanDeath = null;

	protected Vector3 dest;
	protected Rigidbody myRigidbody; 
	protected Vector3 up = new Vector3 (0.0f, 0.0f, -1.0f);
	protected Vector3 down = new Vector3 (0.0f, 0.0f, 1.0f);
	protected Vector3 right = new Vector3 (-1.0f, 0.0f, 0.0f);
	protected Vector3 left = new Vector3 (1.0f, 0.0f, 0.0f);
	protected int layerMask = 1 << 8 | 1 << 9;
	protected float dist = 10000.0f;
	protected Vector3 dir;
	protected Vector3 oldDir;
	protected bool isWaiting;
	protected Animator anim;
	protected Vector3 tempVec;
	protected GameManager MyGameManager = null;

	protected virtual void OnEnable() {
		dest = transform.position;
		transform.FindChild ("ghost_1").gameObject.SetActive (true);
		transform.FindChild ("scared").gameObject.SetActive (false);
	}

	// Use this for initialization
	protected virtual void Start () {
		//transform.position = new Vector3 (0.5f, 0.5f, -4.5f);
		MyGameManager = GameManager.Instance;
		dest = transform.position;
		myRigidbody = (Rigidbody)GetComponent<Rigidbody> ();
		layerMask = ~layerMask;
		dir = left;
		anim = GetComponent<Animator> ();
	}

	// Update is called once per frame
	protected virtual void FixedUpdate () {
		if (MyGameManager.CurrentState == GameState.Playing) {
			ghostSpeed = MyGameManager.getGhostSpeed ();
			Waiting ();
			if (currentState == GhostState.Chasing || currentState == GhostState.Alive) {
				Mover ();
			}
			if (currentState == GhostState.Scattering) {
				Scattering ();
			}
			if (currentState == GhostState.Retreating) {
				Retreating ();
			}
			if (currentState == GhostState.Dead) {
				ghostSpeed = 0.1f;
				Dead ();
			}
			if (currentState == GhostState.WaitTransition) {
				WaitTran ();
			}
		}
	}

	protected virtual void WaitTran () {
		if (transform.position == new Vector3 (0.5f, 0.5f, -2.5f))
			dest = new Vector3 (0.5f, 0.5f, -4.5f);
		else if (dest == new Vector3 (0.5f, 0.5f, -4.5f)) {
		}
		else if (dest != new Vector3 (0.5f, 0.5f, -2.5f))
			dest = new Vector3 (0.5f, 0.5f, -2.5f);
		Vector3 move = Vector3.MoveTowards (transform.position, dest, ghostSpeed);
		myRigidbody.MovePosition (move);
	}

	protected virtual void Dead () {
		Vector3 move = Vector3.MoveTowards (transform.position, dest, ghostSpeed);
		myRigidbody.MovePosition (move);
		Vector3 homeTransform = new Vector3 (0.5f, 0.5f, -4.5f);
		if (transform.position == new Vector3 (0.5f, 0.5f, -2.5f) ) {
			transform.FindChild ("ghost_1").gameObject.SetActive (true);
		}
		else if (transform.position == homeTransform) {
			dest = new Vector3 (0.5f, 0.5f, -2.5f);
		}
		else if (transform.position == dest) {			
			float tempDist;
			dist = 10000.0f;
			if(pacman != null){
				oldDir = -dir;
				if (valid (right) && !oldDir.Equals(right)) {
					tempDist = (transform.position + right - homeTransform).magnitude;
					if (tempDist <= dist) {
						dist = tempDist;
						dir = right;
					}
				}
				if (valid (down) && !oldDir.Equals(down)) {
					tempDist = (transform.position + down - homeTransform).magnitude;
					if (tempDist <= dist) {
						dist = tempDist;
						dir = down;
					}
				} 
				if (valid (left) && !oldDir.Equals(left)) {
					tempDist = (transform.position + left - homeTransform).magnitude;
					if (tempDist <= dist) {
						dist = tempDist;
						dir = left;
					}
				}
				if (valid (up) && !oldDir.Equals(up)) {
					tempDist = (transform.position + up - homeTransform).magnitude;
					if (tempDist <= dist) {
						dist = tempDist;
						dir = up;
					}
				}
				dest = transform.position + dir;
			}
			if (dest == new Vector3 (16.5f, 0.5f, -1.5f) || dest == new Vector3 (-16.5f, 0.5f, -1.5f) ) {
				dest.x = -transform.position.x;
				transform.position = dest;
			}			
			AnimUpdater ();
		}
		
	}

	protected virtual void Mover () {
	}

	protected virtual void Waiting () {
		if (currentState == GhostState.Waiting)
			isWaiting = true;
		else
			isWaiting = false;
	}

	protected virtual void Scattering () {
		dir *= MyGameManager.dirUpdate ();
	}

	protected virtual void Retreating () {
		dir *= MyGameManager.dirUpdate ();
		Vector3 move = Vector3.MoveTowards (transform.position, dest, ghostSpeed);
		myRigidbody.MovePosition (move);
		if (transform.position == dest) {			
			float tempDist;
			dist = 10000.0f;

			if(pacman != null){
				Vector3 retreatTransform = new Vector3 (Random.Range(-16.0f,16.0f), 0.5f, Random.Range(-18.0f,18.0f));
				oldDir = -dir;

				if (valid (right) && !oldDir.Equals(right)) {
					tempDist = (transform.position + right - retreatTransform).magnitude;
					if (tempDist <= dist) {

						dist = tempDist;
						dir = right;
					}
				}
				if (valid (down) && !oldDir.Equals(down)) {
					tempDist = (transform.position + down - retreatTransform).magnitude;
					if (tempDist <= dist) {
						dist = tempDist;
						dir = down;
					}
				} 
				if (valid (left) && !oldDir.Equals(left)) {
					tempDist = (transform.position + left - retreatTransform).magnitude;
					if (tempDist <= dist) {
						dist = tempDist;
						dir = left;
					}
				}
				if (valid (up) && !oldDir.Equals(up)) {
					tempDist = (transform.position + up - retreatTransform).magnitude;
					if (tempDist <= dist) {
						dist = tempDist;
						dir = up;
					}
				}
				dest = transform.position + dir;

				//Debug.Log (dir);

			}
			if (dest == new Vector3 (16.5f, 0.5f, -1.5f) || dest == new Vector3 (-16.5f, 0.5f, -1.5f) ) {
				dest.x = -transform.position.x;
				transform.position = dest;
			}			
			AnimUpdater ();
		}
	}

	protected virtual void OnTriggerEnter(Collider collision) {
		//Debug.Log ("Collision");
		if (MyGameManager.CurrentState == GameState.Playing) {
			if (collision.gameObject.tag == "Player") {
				if (!MyGameManager.isRetreat) {
					if (pacmanDeath != null && !pacmanDeath.isPlaying) {
						pacmanDeath.Play ();
					}
				}
				else {
					if(currentState != GhostState.Dead && !ghostDeath.isPlaying && ghostDeath != null )
						ghostDeath.Play ();
					transform.FindChild ("scared").gameObject.SetActive (false);
					transform.FindChild ("ghost_1").gameObject.SetActive (false);
					MyGameManager.AddPoints (MyGameManager.ghostScore);
				}
			}
		}
	}

	protected virtual bool valid (Vector3 direction){
		Vector3 position = transform.position;
		RaycastHit hit;
		Physics.Raycast (position, direction, out hit, 1.1f, layerMask);
		return (hit.collider == null);
	}

	protected virtual void AnimUpdater () {
		if (dir.Equals (right)) {
			transform.FindChild("Eyes").transform.localPosition = new Vector3 (-0.1f, 0f ,0f);
		} else if (dir.Equals (down)) {
			transform.FindChild("Eyes").transform.localPosition = new Vector3 (0.0f, 0f ,0.2f);
		} else if (dir.Equals (left)) {
			transform.FindChild("Eyes").transform.localPosition = new Vector3 (0.1f, 0f ,0f);
		} else if (dir.Equals (up)) {
			transform.FindChild("Eyes").transform.localPosition = new Vector3 (0.0f, 0f ,-0.2f);
		}

	}
}
