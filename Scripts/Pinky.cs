using UnityEngine;
using System.Collections;

public class Pinky : Ghosts_main {

	protected override void OnEnable() {
		transform.position = new Vector3 (0.0f,0.5f,-2.0f);
		base.OnEnable ();
	}

	protected override void Start(){
		transform.position = new Vector3 (0.0f,0.5f,-2.0f);
		base.Start ();
		currentState = GhostState.Waiting;
		tempVec = down;

	}

	protected override void FixedUpdate(){
		currentState = MyGameManager.GetGhostState ("Pinky");
		base.FixedUpdate ();
	}

	protected override void Waiting (){
		base.Waiting ();
		if (isWaiting) {
			Vector3 move = Vector3.MoveTowards (transform.position, dest, ghostSpeed);
			myRigidbody.MovePosition (move);
			//Debug.Log (transform.position);
			if (transform.position == dest) {			
				dest = transform.position + tempVec;
				tempVec = -tempVec;
			}

		}
	}

	protected override void WaitTran () {
		base.WaitTran ();
		if (transform.position == new Vector3 (0.5f, 0.5f, -4.5f))
			MyGameManager.isActive ("Pinky");
	}

	protected override void Scattering () {
		base.Scattering ();
		Vector3 move = Vector3.MoveTowards (transform.position, dest, ghostSpeed);
		myRigidbody.MovePosition (move);
		//Debug.Log ("HELLO");

		if (transform.position == dest) {			
			float tempDist;
			dist = 10000.0f;

			if(pacman != null){
				Vector3 scatterTransform = new Vector3 (12.5f, 0.5f, -20.5f);
				oldDir = -dir;

				if (valid (right) && !oldDir.Equals(right)) {
					tempDist = (transform.position + right - scatterTransform).magnitude;
					if (tempDist <= dist) {
						dist = tempDist;
						dir = right;
					}
				}
				if (valid (down) && !oldDir.Equals(down)) {
					tempDist = (transform.position + down - scatterTransform).magnitude;
					if (tempDist <= dist) {
						dist = tempDist;
						dir = down;
					}
				} 
				if (valid (left) && !oldDir.Equals(left)) {
					tempDist = (transform.position + left - scatterTransform).magnitude;
					if (tempDist <= dist) {
						dist = tempDist;
						dir = left;
					}
				}
				if (valid (up) && !oldDir.Equals(up)) {
					tempDist = (transform.position + up - scatterTransform).magnitude;
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


	protected override void Mover () {
		base.Mover ();
		Vector3 move = Vector3.MoveTowards (transform.position, dest, ghostSpeed);
		myRigidbody.MovePosition (move);
		if (transform.position == dest) {			
			float tempDist;
			dist = 10000.0f;

			if(pacman != null){
				Transform pacmanTransform = pacman.GetComponent<Transform>();
				//Debug.Log (pacmanTransform.right);
				oldDir = -dir;
				if (valid (right) && !oldDir.Equals(right)) {
					tempDist = (transform.position + right - (pacmanTransform.position + 4*pacmanTransform.right)).magnitude;
					if (tempDist <= dist) {
						dist = tempDist;
						dir = right;
					}
				}
				if (valid (down) && !oldDir.Equals(down)) {
					tempDist = (transform.position + down - (pacmanTransform.position + 4*pacmanTransform.right)).magnitude;
					if (tempDist <= dist) {
						dist = tempDist;
						dir = down;
					}
				} 
				if (valid (left) && !oldDir.Equals(left)) {
					tempDist = (transform.position + left - (pacmanTransform.position + 4*pacmanTransform.right)).magnitude;
					if (tempDist <= dist) {
						dist = tempDist;
						dir = left;
					}
				}
				if (valid (up) && !oldDir.Equals(up)) {
					tempDist = (transform.position + up - (pacmanTransform.position + 4*pacmanTransform.right)).magnitude;
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

	protected override void OnTriggerEnter(Collider collision) {
		base.OnTriggerEnter (collision);
		if (collision.gameObject.tag == "Player") {
			if (!MyGameManager.isRetreat) {
				if (currentState != GhostState.Dead) {
					collision.gameObject.SetActive (false);
					MyGameManager.ResetLevel ();
				}
			}
			else {
				MyGameManager.isDead ("Pinky");
			}
		}
	}

	protected override void Dead () {
		base.Dead ();
		if (transform.position == new Vector3 (0.5f, 0.5f, -2.5f)) {
			dest = new Vector3 (0.5f, 0.5f, -4.5f);
			MyGameManager.isAlive ("Pinky");
		}
	}
}
