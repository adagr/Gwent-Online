using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class CardDeckBuilder : MonoBehaviour {

	public int power;
	public string type;
	public int count;
	public int maxCount;
	public bool special, hero;
	public CardDeckBuilder clone;
	public Text countText;

	private DeckBuilder db;
	public bool inDeck = false;

	// Use this for initialization
	void Start () {
		db = FindObjectOfType<DeckBuilder>();
	}
	
	// Update is called once per frame
	void Update () {
		countText.text = "x"+count;
	}

	void OnMouseDown() {
		if (db.selected != this)
			db.selected = this;
		else if (!inDeck && db.selected == this) {
			db.addToDeck(this);
		} else if (inDeck && db.selected == this) {
			db.removeFromDeck(this);
		}
	}

	void OnMouseOver() {
		float x = Input.GetAxis("Mouse ScrollWheel");
		if (x < 0 || Input.GetKeyDown("down") || Input.GetKeyDown("s")) {
			if (inDeck)
				db.StartIndexDeck += 3;
			else
				db.StartIndexCards += 3;
		} else if (x > 0 || Input.GetKeyDown("up") || Input.GetKeyDown("w")) {
			if (inDeck)
				db.StartIndexDeck -= 3;
			else
				db.StartIndexCards -= 3;
		}
	}
}
