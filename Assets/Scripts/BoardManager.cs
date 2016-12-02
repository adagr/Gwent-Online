using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;

public class BoardManager : MonoBehaviour {

	public Transform meleePosPlayer;
	public Transform rangePosPlayer;
	public Transform siegePosPlayer;
	public Transform discardPosPlayer;

	public Transform meleePosEnemy;
	public Transform rangePosEnemy;
	public Transform siegePosEnemy;
	public Transform discardPosEnemy;

	public Transform weatherPos;
	public Transform medicPos;
	public bool medic = false;
	public bool medicEnemy = false;
	public Transform doubleMeleePosPlayer,doubleRangePosPlayer,doubleSiegePosPlayer;
	public Transform doubleMeleePosEnemy,doubleRangePosEnemy,doubleSiegePosEnemy;

	public Dictionary<string,List<Card>> cardsPlayer = new Dictionary<string,List<Card>>();
	private List<Card> discardPilePlayer = new List<Card>();
	public List<Card> discardPileMedicPlayer = new List<Card>();

	public Dictionary<string,List<Card>> cardsEnemy = new Dictionary<string,List<Card>>();
	private List<Card> discardPileEnemy = new List<Card>();
	public List<Card> discardPileMedicEnemy = new List<Card>();

	private List<Card> weatherCards = new List<Card>();
	public Dictionary<string,bool> weatherEffects = new Dictionary<string, bool>();

	//private List<Card> doubleCardsPlayer = new List<Card>();
	public Dictionary<string,Card> doubleCardsPlayer = new Dictionary<string,Card>();
	public Dictionary<string,Card> doubleCardsEnemy = new Dictionary<string,Card>();
	private Dictionary<string,Transform> doublePosEnemy = new Dictionary<string, Transform>();

	private DeckManager dm;
	private ScoreManager sm;
	private PhotonView photonView;
	public Dictionary<string,int> strongestCardsPlayer = new Dictionary<string, int>();
	public Dictionary<string,int> strongestCardsEnemy = new Dictionary<string, int>();
	private int selectedCard = 0;
	public List<Card> displayCardsBigList = new List<Card>();
	public bool redraw = true;
	public int redrawCount = 0;
	public float redrawCounter = -1;
	public Text redrawText;

	// Use this for initialization
	void Start () {
		dm = FindObjectOfType<DeckManager>();
		sm = FindObjectOfType<ScoreManager>();
		photonView = GetComponent<PhotonView>();

		cardsPlayer.Add("melee",new List<Card>());
		cardsPlayer.Add("range",new List<Card>());
		cardsPlayer.Add("siege",new List<Card>());

		cardsEnemy.Add("melee",new List<Card>());
		cardsEnemy.Add("range",new List<Card>());
		cardsEnemy.Add("siege",new List<Card>());

		weatherEffects.Add("melee",false);
		weatherEffects.Add("range",false);
		weatherEffects.Add("siege",false);

		doublePosEnemy.Add("melee",doubleMeleePosEnemy);
		doublePosEnemy.Add("range",doubleRangePosEnemy);
		doublePosEnemy.Add("siege",doubleSiegePosEnemy);
		doubleCardsPlayer.Add("melee",null);
		doubleCardsPlayer.Add("range",null);
		doubleCardsPlayer.Add("siege",null);
		doubleCardsEnemy.Add("melee",null);
		doubleCardsEnemy.Add("range",null);
		doubleCardsEnemy.Add("siege",null);

		resetStrongestCards();
	}
	
	// Update is called once per frame
	void Update () {
		displayCards(cardsPlayer["melee"],meleePosPlayer);
		displayCards(cardsPlayer["range"],rangePosPlayer);
		displayCards(cardsPlayer["siege"],siegePosPlayer);

		displayCards(cardsEnemy["melee"],meleePosEnemy);
		displayCards(cardsEnemy["range"],rangePosEnemy);
		displayCards(cardsEnemy["siege"],siegePosEnemy);

		displayCards(weatherCards,weatherPos);
		if (medic)
			displayCards(discardPileMedicPlayer,medicPos);
		else {
			moveListToDiscard(discardPileMedicPlayer);
		}
		if (medicEnemy) {
			if (discardPileMedicEnemy.Count == 0)
				medicEnemy = false;
			else
				displayCards(discardPileMedicEnemy,medicPos);
		} else {
			moveListToDiscard(discardPileMedicEnemy);
		}

		displayCardsBig(displayCardsBigList);

		if (displayCardsBigList != null && displayCardsBigList.Count>0) {
			float x = Input.GetAxis("Mouse ScrollWheel");
			if (x < 0 || Input.GetKeyDown("right") || Input.GetKeyDown("d")) {
				selectedCard = Mathf.Min(++selectedCard,displayCardsBigList.Count-1);
			} else if (x > 0 || Input.GetKeyDown("left") || Input.GetKeyDown("a")) {
				selectedCard = Mathf.Max(--selectedCard,0);
			}
			if (Input.GetAxis("Fire2") == 1) {
				if (redraw && redrawCounter < 0) {
					redrawCounter = 2;
				} else if (!redraw) {
					foreach (Card card in displayCardsBigList) {
						card.GetComponent<SpriteRenderer>().enabled = false;
						card.targetPos = new Vector3(1000,1000,0);
					}
					dm.nextTurn();
					displayCardsBigList = new List<Card>();
				}
			}
		}

		if (redrawCount == 2 && redrawCounter < 0) {
			redrawCounter = 0;
		}
		if (redrawCounter >= 0) {
			redrawCounter += Time.deltaTime;
			if (redrawCounter > 2) {
				redraw = false;
				displayCardsBigList = new List<Card>();
				dm.sortHand();
				redrawCount = 0;
				redrawCounter = -1;
				redrawText.enabled = false;
			}
		}
		redrawText.text = "Redraw up to two cards ("+redrawCount+"/2)";

		Debug.Log("Player Total: "+strongestCardsPlayer["Total"]+"\nMelee: "+strongestCardsPlayer["melee"]+"\nRange: "+strongestCardsPlayer["range"]+"\nSiege: "+strongestCardsPlayer["siege"]);
		Debug.Log("Enemy Total: "+strongestCardsEnemy["Total"]+"\nMelee: "+strongestCardsEnemy["melee"]+"\nRange: "+strongestCardsEnemy["range"]+"\nSiege: "+strongestCardsEnemy["siege"]);
	}

	public void updateBoard(Card card) {
		if (card.type.CompareTo("Weather") == 0) {
			if (card.specialAbility.CompareTo("Clear") == 0) {
				clearWeatherEffect();
				moveCardToDiscard(card);
			} else
				addWeatherEffect(card);
		} else if (card.specialAbility.CompareTo("Scorch") == 0 && card.special) {
			if (strongestCardsPlayer["Total"] >= strongestCardsEnemy["Total"]) {
				scorchCards(strongestCardsPlayer,cardsPlayer);
			}
			if (strongestCardsEnemy["Total"] >= strongestCardsPlayer["Total"]) {
				scorchCards(strongestCardsEnemy,cardsEnemy);
			}
			//resetStrongestCards();
			moveCardToDiscard(card);
		} else if (card.specialAbility.CompareTo("Leader") == 0) {
			playLeaderCard(card.name);
		} else if (card.player.CompareTo("Player") == 0) {
			if (card.type.CompareTo("Decoy") == 0)
				addCard(cardsPlayer,card,card.type); 
			else if (card.specialAbility.CompareTo("Medic") == 0 && discardPileMedicPlayer.Count > 0) {
				medic = true;
				addCard(cardsPlayer,card,card.type);
			} else if (card.specialAbility.CompareTo("Double") == 0) {
				if (!card.special) {
					addCard(cardsPlayer,card,card.type);
					if (doubleCardsPlayer[card.type] == null)
						doubleCardsPlayer[card.type] = card;
				} else
					doubleCardsPlayer[card.type] = card;
			} else if (card.specialAbility.CompareTo("Scorch") == 0) {
				scorchCardsByType(sm.powerEnemy[card.type],card.type,strongestCardsEnemy,cardsEnemy);
				//scorchCardsByType(sm.powerEnemy[card.type],card.type,"Enemy", strongestCardsEnemy,cardsEnemy);
				addCard(cardsPlayer,card,card.type);
			} else
				addCard(cardsPlayer,card,card.type);
		} else {
			if (card.type.CompareTo("Decoy") == 0)
				addCard(cardsEnemy,card,card.type); 
			else if (card.specialAbility.CompareTo("Double") == 0) {
				if (card.special) {
					card.targetPos = doublePosEnemy[card.type].position;
					doubleCardsEnemy[card.type] = card;
				} else {
					addCard(cardsEnemy,card,card.type);
					if (doubleCardsEnemy[card.type] == null)
						doubleCardsEnemy[card.type] = card;
				}
			} else if (card.specialAbility.CompareTo("Scorch") == 0) {
				scorchCardsByType(sm.powerPlayer[card.type],card.type,strongestCardsPlayer,cardsPlayer);
				//scorchCardsByType(sm.powerPlayer[card.type],card.type,"Player", strongestCardsPlayer,cardsPlayer);
				addCard(cardsEnemy,card,card.type);
			} else
				addCard(cardsEnemy,card,card.type);
		}
		resetStrongestCards();
		dm.selected = null;
	}

	void scorchCards(Dictionary<string,int> strongestCards, Dictionary<string,List<Card>> cards) {
		Dictionary<string, List<Card>> temp = new Dictionary<string, List<Card>>();
		temp.Add("melee",new List<Card>());
		temp.Add("range",new List<Card>());
		temp.Add("siege",new List<Card>());
		foreach (var list in cards.Values) {
			foreach (Card c in list) {
				if (c.power == strongestCards["Total"] && !c.special && !c.hero)
					temp[c.type].Add(c);
			}
		}
		foreach (KeyValuePair<string,List<Card>> list in temp) {
			foreach (Card c in list.Value) {
				cards[list.Key].Remove(c);
				moveCardToDiscard(c);
			}
		}
	}

	void scorchCardsByType(int power, string type, Dictionary<string,int> strongestCards, Dictionary<string,List<Card>> cards) {
		if (power >= 10) {
			List<Card> temp = new List<Card>();
			foreach (Card c in cards[type])
				if (c.power == strongestCards[type] && !c.special && !c.hero)
					temp.Add(c);
			foreach (Card c in temp) {
				cards[type].Remove(c);
				moveCardToDiscard(c);
			}
		}
		strongestCards[type] = 0;
		strongestCards["Total"] = 0;
	}
	
	[PunRPC] void scorchCardsByTypeRPC(string type) {
		scorchCardsByType(sm.powerPlayer[type],type,strongestCardsPlayer,cardsPlayer);
	}

	void resetStrongestCards() {
		strongestCardsPlayer["Total"] = 0;
		strongestCardsPlayer["melee"] = 0;
		strongestCardsPlayer["range"] = 0;
		strongestCardsPlayer["siege"] = 0;
		strongestCardsEnemy["Total"] = 0;
		strongestCardsEnemy["melee"] = 0;
		strongestCardsEnemy["range"] = 0;
		strongestCardsEnemy["siege"] = 0;
	}
	
	void addCard(Dictionary<string,List<Card>> cards, Card card, string type) {
		cards[type].Add(card);
		cards[type] = cards[type].OrderBy(x => x.name).ToList();
		cards[type] = cards[type].OrderBy(x => x.basePower).ToList();
	}

	public Card removeCard(string name) {
		string type = "";
		Card card = null;
		foreach(KeyValuePair<string,List<Card>> list in cardsEnemy) {
			foreach (Card c in list.Value) {
				if (c.name.CompareTo(name) == 0) {
					card = c;
					type = c.type;
					break;
				}
			}
		}
		return card;
	}

	void displayCards(List<Card> list, Transform pos) {
		for(int i=0;i<list.Count;i++) {
			Card card = list[i];
			if (list.Count < 10)
				//card.transform.position = new Vector3(pos.position.x+(i-list.Count/2)*0.9f,pos.position.y,pos.position.z);
				card.targetPos = new Vector3(pos.position.x+(i-list.Count/2)*0.9f,pos.position.y,pos.position.z);
			else
				//card.transform.position = new Vector3(pos.position.x+(i-list.Count/2)*0.45f,pos.position.y,pos.position.z-i*0.01f);
				card.targetPos = new Vector3(pos.position.x+(i-list.Count/2)*0.45f,pos.position.y,pos.position.z-i*0.01f);
		}
	}

	void displayCardsBig(List<Card> list) {
		if (list == null || list.Count == 0)
			return;
		for(int i=0;i<list.Count;i++) {
			Card card = list[i];
			if (!redraw)
				card.setBigSprite();
			card.targetPos = medicPos.position + Vector3.left*3.5f*(selectedCard-i) + Vector3.down;
		}
	}

	public void newRound() {
		discard(cardsPlayer,discardPilePlayer,discardPileMedicPlayer,discardPosPlayer);
		discard(cardsEnemy,discardPileEnemy,discardPileMedicEnemy,discardPosEnemy);
		clearWeatherEffect();
		foreach(Card c in doubleCardsPlayer.Values) {
			if (c != null)
				moveCardToDiscard(c);
		}
		doubleCardsPlayer["melee"] = null;
		doubleCardsPlayer["range"] = null;
		doubleCardsPlayer["siege"] = null;
		foreach(Card c in doubleCardsEnemy.Values) {
			if (c != null)
				moveCardToDiscard(c);
		}
		doubleCardsEnemy["melee"] = null;
		doubleCardsEnemy["range"] = null;
		doubleCardsEnemy["siege"] = null;
		resetStrongestCards();
	}

	void discard(Dictionary<string,List<Card>> cards, List<Card> discardPile, List<Card> discardPileMedic, Transform discardPos) {
		foreach (var list in cards.Values) {
			foreach (Card c in list) {
				c.targetPos = discardPos.position;
				c.inDiscard = true;
				if (!c.special && !c.hero)
					discardPileMedic.Add(c);
				else
					discardPile.Add(c);
			}
			list.Clear();
		}
	}

	void moveCardToDiscard(Card c) {
		c.inDiscard = true;
		if (c.player.CompareTo("Player") == 0) {
			c.selected = false;
			c.targetPos = discardPosPlayer.position;
			if (!c.special && !c.hero)
				discardPileMedicPlayer.Add(c);
			else
				discardPilePlayer.Add(c);
		} else {
			c.selected = false;
			c.targetPos = discardPosEnemy.position;
			if (!c.special && !c.hero)
				discardPileMedicEnemy.Add(c);
			else
				discardPileEnemy.Add(c);
		}
	}

	void moveListToDiscard(List<Card> cards) {
		foreach(Card c in cards) {
			if (c.player.CompareTo("Player") == 0)
				c.targetPos = discardPosPlayer.position;
			else
				c.targetPos = discardPosEnemy.position;
		}
	}

	public void removeFromDiscard(Card card) {
		card.inDiscard = false;
		if (card.player.CompareTo("Player") == 0) {
			discardPileMedicPlayer.Remove(card);
			medic = false;
		} else {
			discardPileMedicEnemy.Remove(card);
			medicEnemy = false;
		}
	}
	
	void addWeatherEffect(Card card) {
		if (!weatherEffects[card.specialAbility]) {
			weatherCards.Add(card);
			strongestCardsPlayer[card.specialAbility] = 0;
			strongestCardsPlayer["Total"] = 0;
			strongestCardsEnemy[card.specialAbility] = 0;
			strongestCardsEnemy["Total"] = 0;
		} else
			moveCardToDiscard(card);
		weatherEffects[card.specialAbility] = true;
	}

	void clearWeatherEffect() {
		weatherEffects["melee"] = false;
		weatherEffects["range"] = false;
		weatherEffects["siege"] = false;
		foreach(Card c in weatherCards)
			moveCardToDiscard(c);
		weatherCards.Clear();
	}

	void playLeaderCard(string name) {
		if (name.CompareTo("Foltest King of Temeria") == 0) {
			dm.playFromDeck("Impenetrable Fog");
		} else if (name.CompareTo("Foltest Lord Commander of the North") == 0) {
			Card card = dm.createCard("Clear Weather");
			dm.playCard(card);
			dm.setEnemyHandSize(1);
		} else if (name.CompareTo("Foltest The Siegemaster") == 0) {
			Card card = dm.createCard("Commander's Horn");
			card.type = "siege";
			card.targetPos = doubleSiegePosPlayer.position;
			dm.playCard(card);
			dm.setEnemyHandSize(1);
		} else if (name.CompareTo("Foltest The Steel-Forged") == 0) {
			scorchCardsByType(sm.powerEnemy["siege"],"siege",strongestCardsEnemy,cardsEnemy);
			photonView.RPC("scorchCardsByTypeRPC",PhotonTargets.Others,"siege");
			dm.nextTurn();
		} else if (name.CompareTo("Emhyr var Emreis His Imperial Majesty") == 0) {
			dm.playFromDeck("Torrential Rain");
		} else if (name.CompareTo("Emhyr var Emreis Emperor of Nilfgaard") == 0) {
			dm.requestCards();
		} else if (name.CompareTo("Emhyr var Emreis The Relentless") == 0) {
			medicEnemy = true;
		}
	}
}
