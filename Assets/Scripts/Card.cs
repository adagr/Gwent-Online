using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;

public class Card : MonoBehaviour {

	public int basePower;
	public int power;
	public string type;
	public string specialAbility;
	public string player;
	public Text powerText;
	private bool hover = false;
	private Vector3 TargetPos;
	public Vector3 targetPos {
		get{return TargetPos;}
		set {
			if (!hover && !selected)
				TargetPos = value;
		}
	}
	public float lerpTimer = 0;
	private Sprite smallSprite;
	public Sprite bigSprite;
	public bool selected = false;

	private GameManager gm;
	private DeckManager dm;
	private BoardManager bm;
	private SpriteRenderer sr;
	public bool inHand = true;
	public bool inDiscard = false;
	public bool hero = false;
	public bool special = false;
	public Vector3 originalPos;
	public Image fadeImage;

	// Use this for initialization
	void Start () {
		gm = FindObjectOfType<GameManager>();
		dm = FindObjectOfType<DeckManager>();
		bm = FindObjectOfType<BoardManager>();
		sr = GetComponent<SpriteRenderer>();
		smallSprite = sr.sprite;
		power = basePower;
		originalPos = transform.position;
	}
	
	// Update is called once per frame
	void Update () {
		if (!special && !hero) {
			powerText.text = ""+power;
			if (power > basePower)
				powerText.color = new Color(0,178f/255f,31f/255f);
			else if (power < basePower)
				powerText.color = Color.red;
			else
				powerText.color = Color.black;

			if (inDiscard)
				power = basePower;
			else if (!inHand && !inDiscard ) { 
				int count = 0;
				int morale = 0;
				List<Card> list;
				if (player.CompareTo("Player") == 0)
					list = bm.cardsPlayer[type];
				else
					list = bm.cardsEnemy[type];
				foreach(Card c in list) {
					if (c.name.CompareTo(this.name) == 0 && specialAbility.CompareTo("Bond") == 0) { //Bond
						count++;
					}
					if (c != this && c.specialAbility.CompareTo("Morale") == 0) {
						morale++;
					}
				}
				if (specialAbility.CompareTo("Bond") != 0)
					count++;
				power = basePower * count + morale;
			} else
				power = basePower;
			if (player.CompareTo("Player") == 0) {
				if (bm.doubleCardsPlayer[type] != null && bm.doubleCardsPlayer[type] != this && !inHand && !inDiscard)
					power *= 2;
			} else
				if (bm.doubleCardsEnemy[type] != null && bm.doubleCardsEnemy[type] != this && !inHand && !inDiscard)
					power *= 2;
			if (!inHand && !inDiscard && bm.weatherEffects[type] && !hero)
				power = power/basePower + power%basePower;

			if (!inHand && !inDiscard) {
				if (player.CompareTo("Player") == 0) {
					if (bm.strongestCardsPlayer[type] == 0 || power > bm.strongestCardsPlayer[type])
						bm.strongestCardsPlayer[type] = power;
					if (bm.strongestCardsPlayer["Total"] == 0 || power > bm.strongestCardsPlayer["Total"])
						bm.strongestCardsPlayer["Total"] = power;
				} else {
					if (bm.strongestCardsEnemy[type] == 0 || power > bm.strongestCardsEnemy[type])
						bm.strongestCardsEnemy[type] = power;
					if (bm.strongestCardsEnemy["Total"] == 0 || power > bm.strongestCardsEnemy["Total"])
						bm.strongestCardsEnemy["Total"] = power;
				}
			}
		}

		if ((transform.position - targetPos).magnitude > 0) {
			transform.position = Vector3.Lerp(transform.position,targetPos,lerpTimer);
			lerpTimer += Time.deltaTime;
		} else
			lerpTimer = 0;

		if (dm.selected == this && inHand && !selected && !bm.redraw) {
			lerpTimer = 0;
			hover = false;
			targetPos = dm.cardPos.position;
			selected = true;
			setBigSprite();
		} else if (dm.selected != this && !bm.displayCardsBigList.Contains(this)) {
			selected = false;
			setSmallSprite();
		} else if (bm.redraw && bm.displayCardsBigList.Contains(this))
			setBigSprite();

		if (specialAbility.CompareTo("Leader") == 0 && !inHand)
			fadeImage.enabled = true;
	}

	public void setBigSprite() {
		sr = GetComponent<SpriteRenderer>(); //Need to fecth it here otherwise it's sometimes null
		sr.sprite = bigSprite;
		if (!special && !hero)
			powerText.enabled = false;
		transform.localScale = new Vector3(1.6f,1.6f,1);
		BoxCollider2D collider = GetComponent<BoxCollider2D>();
		collider.size = bigSprite.bounds.size;
	}

	public void setSmallSprite() {
		sr = GetComponent<SpriteRenderer>();
		sr.sprite = smallSprite;
		if (!special && !hero)
			powerText.enabled = true;
		transform.localScale = new Vector3(1,1,1);
		BoxCollider2D collider = GetComponent<BoxCollider2D>();
		collider.size = smallSprite.bounds.size;
	}

	void OnMouseDown() {
		if (player.CompareTo(gm.turn) == 0 && player.CompareTo("Player") == 0 && !gm.nr) {
			if (dm.selected != null && !inHand && !inDiscard && dm.selected.specialAbility.CompareTo("Decoy") == 0 && dm.selected.inHand && !special && !hero) { //Decoy
				dm.playDecoy(this);
			} else if (dm.selected == this && !inHand && inDiscard && bm.medic && !special && !hero) { //Medic
				dm.playCard(this);
			} else if (dm.selected != null && dm.selected != this && dm.selected.specialAbility.CompareTo("Scorch") == 0 && dm.selected.special && !inHand && !inDiscard && !bm.medic && specialAbility.CompareTo("Double") != 0 && type.CompareTo("Weather") != 0) { //Scorch
				dm.playCard(dm.selected);
			} else if (dm.selected != null && dm.selected != this && type.CompareTo(dm.selected.type) == 0 && !inHand && !inDiscard && dm.selected.specialAbility.CompareTo("Spy") != 0) { //Play selected card
				dm.playCard(dm.selected);
			} else if (dm.selected != null && dm.selected != this && !inHand && !inDiscard && dm.selected.specialAbility.CompareTo("Leader") == 0) //Leader
				dm.playCard(dm.selected);
			else if (dm.selected != this && (inHand || inDiscard))
				dm.selected = this;
		} else if (player.CompareTo("Enemy") == 0 && !gm.nr) {
			if (dm.selected != null && dm.selected != this && type.CompareTo(dm.selected.type) == 0 && !inHand && !inDiscard) {
				if (dm.selected.specialAbility.CompareTo("Spy") == 0 || type.CompareTo("Weather") == 0 || dm.selected.specialAbility.CompareTo("Leader") == 0)
					dm.playCard(dm.selected);
			} else if (dm.selected != null && dm.selected == this && bm.medicEnemy && inDiscard && !special && !hero && !inHand) { //Medic Enemy
				dm.addCardToHand(this);
				bm.medicEnemy = false;
				dm.nextTurn();
				dm.setEnemyHandSize(1);
				dm.removeFromDiscard(this);
				dm.selected = null;
			} else if (bm.medicEnemy && inDiscard)
				dm.selected = this;
		} else if (player.CompareTo("Player") == 0 && bm.redraw && bm.redrawCounter < 0) {
			if (dm.selected != null && dm.selected == this)
				dm.redrawCard(this);
			else if (bm.redrawCounter < 0 && specialAbility.CompareTo("Leader") != 0)
				dm.selected = this;
		}
	}

	void OnMouseOver() {
		if (inHand && (transform.position - targetPos).magnitude == 0 && transform.position.y == dm.handPosPlayer.position.y) {
			targetPos = new Vector3(targetPos.x,targetPos.y+0.3f,targetPos.z-1);
			lerpTimer = 0;
			hover = true;
		} else if (!inHand)
			hover = false;
	}


	void OnMouseExit() {
		hover = false;
	}

}
