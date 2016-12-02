using UnityEngine;
using System.Collections;

public class ScrollDeck : MonoBehaviour {

	public bool deck;
	private DeckBuilder db;

	// Use this for initialization
	void Start () {
		db = FindObjectOfType<DeckBuilder>();
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	void OnMouseOver() {
		float x = Input.GetAxis("Mouse ScrollWheel");
		if (x < 0 || Input.GetKeyDown("down") || Input.GetKeyDown("s")) {
			if (deck)
				db.StartIndexDeck += 3;
			else
				db.StartIndexCards += 3;
		} else if (x > 0 || Input.GetKeyDown("up") || Input.GetKeyDown("w")) {
			if (deck)
				db.StartIndexDeck -= 3;
			else
				db.StartIndexCards -= 3;
		}
	}
}
