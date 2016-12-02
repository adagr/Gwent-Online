using UnityEngine;
using System.Collections;

public class CardLeader : MonoBehaviour {

	public Sprite bigSprite;
	public Transform info;

	private Sprite smallSprite;
	private bool selected = false;
	private DeckBuilder db;
	private SpriteRenderer sr;

	// Use this for initialization
	void Start () {
		db = FindObjectOfType<DeckBuilder>();
		sr = GetComponent<SpriteRenderer>();
		smallSprite = sr.sprite;
	}
	
	// Update is called once per frame
	void Update () {
		if (db.selectLeader && DeckBuilder.selectedLeader == db.leaderCards[DeckBuilder.factions[DeckBuilder.selectedFaction]].IndexOf(this))
			info.gameObject.SetActive(true);
		else
			info.gameObject.SetActive(false);
	}

	void OnMouseDown() {
		if (db.selectLeader) {
			int leaderNumber = db.leaderCards[DeckBuilder.factions[DeckBuilder.selectedFaction]].IndexOf(this);
			if (DeckBuilder.selectedLeader != leaderNumber) {
				DeckBuilder.selectedLeader = leaderNumber;
				db.displayLeaders();
			} else {
				db.unDisplayLeaders();
			}
		}
		if (!selected)
			selected = true;
		else {
			selected = false;
			db.displayLeaders();
		}
	}

	public void selectBigSprite() {
		transform.localScale = new Vector3(1.6f,1.6f,1);
		sr.sprite = bigSprite;
		BoxCollider2D collider = GetComponent<BoxCollider2D>();
		collider.size = bigSprite.bounds.size;
	}

	public void selectSmallSprite() {
		transform.localScale = new Vector3(1,1,1);
		sr.sprite = smallSprite;
		BoxCollider2D collider = GetComponent<BoxCollider2D>();
		collider.size = smallSprite.bounds.size;
	}
}
