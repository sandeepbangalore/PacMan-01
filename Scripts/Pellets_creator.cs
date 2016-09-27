using UnityEngine;
using System.Collections;

public class Pellets_creator : MonoBehaviour {

	public GameObject pellets = null;
	public GameObject powerPellets = null;
	public float powerPelletsFreq = 0.015f;
	// Use this for initialization
	private GameManager MyGameManager = null;
	//private int layerMask = 1 << 9 | 1 << 10;
	private int layerMask = 1 << 11;


	void OnEnable () {
		MyGameManager = GameManager.Instance;
		//layerMask = ~layerMask;
		if (pellets != null && powerPellets != null) {
			float x = -13.5f;
			float z = -15.5f;
			Random.seed = 13;
			for (int i = 0; i < 28; i++) {
				for (int j = 0; j < 31; j++) {
					Collider[] hitColliders = Physics.OverlapSphere (new Vector3 (x + i, 0.5f, z + j), 0.5f, layerMask);
					//Debug.Log (hitColliders.Length);
					if (hitColliders.Length == 0 && !((x + i < 3.0f) && (x + i > -3.0f) && (z + j > -3.0f) && (z + j < 0.0f) )) {
						if (Random.value > powerPelletsFreq) {
							MyGameManager.AddPellets (1);
							GameObject childObject = Instantiate (pellets, new Vector3 (x + i, 0.5f, z + j), Quaternion.identity) as GameObject;
							childObject.transform.parent = GameObject.Find ("GameManager").transform;
						} else {
							GameObject childObject = Instantiate (powerPellets, new Vector3 (x + i, 0.5f, z + j), Quaternion.identity) as GameObject;
							childObject.transform.parent = GameObject.Find ("GameManager").transform;
							childObject = Instantiate (powerPellets, new Vector3 (-(x + i), 0.5f, z + j), Quaternion.identity) as GameObject;
							childObject.transform.parent = GameObject.Find ("GameManager").transform;
							MyGameManager.AddPellets (2);

						}
					}
				}
			}
		}
		MyGameManager.TotalPellets();
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
