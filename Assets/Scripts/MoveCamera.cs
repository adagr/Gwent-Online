using UnityEngine;
using System.Collections;

public class MoveCamera : MonoBehaviour {

	private Camera cam;
	private Vector3 targetPos;
	private float timer;

	// Use this for initialization
	void Start () {
		cam = GetComponent<Camera>();
	}
	
	// Update is called once per frame
	/*void Update () {
		if ((transform.position - targetPos).magnitude > 0) {
			cam.transform.position = Vector3.Lerp(cam.transform.position,targetPos,timer);
			timer += Time.deltaTime/2;
		} else
			timer = 0;
	}
	*/
	public void moveCamera(Transform pos) {
		//targetPos = pos.position;
		cam.transform.position = pos.position;
	}
}
