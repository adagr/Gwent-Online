using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using System.Linq;

public class DeckManager : MonoBehaviour {

	public Text handCountTextPlayer;
	public Text handCountTextEnemy;
	public Transform deckPos;
	public Transform handPosPlayer;
	public Transform handPosEnemy;
	public Transform cardPos;
	public Transform leaderPosPlayer,leaderPosEnemy;
	private List<string> deck = new List<string>();
	private string selectedInfoType;
	private Card Selected;
	public Card selected {
		get{return Selected;}
		set {
			if (Selected != null && (Selected.specialAbility.CompareTo("") != 0 || Selected.hero))
				infoText[selectedInfoType].GetComponent<SpriteRenderer>().enabled = false;
			if (value != null && value.inHand) {
				if (value.specialAbility.CompareTo("Scorch") == 0 && !value.special) {
					selectedInfoType = value.specialAbility+value.type;
					infoText[selectedInfoType].GetComponent<SpriteRenderer>().enabled = true;
				} else if (value.specialAbility.CompareTo("Leader") == 0) {
					selectedInfoType = value.name;
					infoText[selectedInfoType].GetComponent<SpriteRenderer>().enabled = true;
				} else if (value.specialAbility.CompareTo("") != 0) {
					selectedInfoType = value.specialAbility;
					infoText[selectedInfoType].GetComponent<SpriteRenderer>().enabled = true;
				} else if (value.hero) {
					selectedInfoType = "Hero";
					infoText[selectedInfoType].GetComponent<SpriteRenderer>().enabled = true;
				}
			}
			if (Selected != null && Selected.specialAbility.CompareTo("Leader") == 0) {
				Selected.selected = false;
				Selected.targetPos = Selected.originalPos;
			}
			Selected = value;
		}
	}

	private List<Card> hand = new List<Card>();
	private ScoreManager sm;
	private BoardManager bm;
	private GameManager gm;
	private PhotonView photonView;
	public int handCountEnemy = 10;
	public Dictionary<string,Card> cardDict = new Dictionary<string, Card>();
	private Dictionary<string,Transform> infoText = new Dictionary<string, Transform>();
	private string infoTextSelected;
	public string leaderNamePlayer, leaderNameEnemy;
	private Card leaderPlayer,leaderEnemy;

	// Use this for initialization
	void Start () {
		gm = FindObjectOfType<GameManager>();
		sm = FindObjectOfType<ScoreManager>();
		bm = FindObjectOfType<BoardManager>();
		photonView = GetComponent<PhotonView>();
		
		foreach (Transform child in transform) {
			if (child.name.CompareTo("Cards") == 0) {
				foreach (Transform grandChild in child) {
					var card  = grandChild.GetComponent<Card>();
					cardDict.Add(card.name,card);
				}
			}
			else if (child.name.CompareTo("Info") == 0) {
				foreach (Transform grandChild in child) {
					infoText.Add(grandChild.name,grandChild);
				}
			}
		}

		leaderNamePlayer = DeckBuilder.selectedLeaderName;
		leaderPlayer = (Card)Instantiate(cardDict[leaderNamePlayer],leaderPosPlayer.position,Quaternion.identity);
		leaderPlayer.name = leaderNamePlayer;
		leaderPlayer.player = "Player";
		leaderPlayer.targetPos = leaderPosPlayer.position;
		leaderPlayer.gameObject.SetActive(true);
		photonView.RPC("sendLeader",PhotonTargets.Others,leaderNamePlayer);

		foreach(string name in DeckBuilder.cardDeck)
			deck.Add(name);
		drawCard(10);
		sortHand();
		bm.displayCardsBigList = hand;
		handCountEnemy = 10;
	}
	
	// Update is called once per frame
	void Update () {
		handCountTextPlayer.text = ""+hand.Count;
		handCountTextEnemy.text = ""+handCountEnemy;
		if (!bm.redraw)
			displayCards(hand,handPosPlayer);
		if (hand.Count == 0 && gm.turn.CompareTo("Player") == 0 && !bm.medic && !bm.medicEnemy && !leaderPlayer.inHand)
			gm.passRPC();
		if (Input.GetAxis("Fire2") == 1) //Right Click deselect
			selected = null;
	}

	public void sortHand() {
		hand = hand.OrderBy(x => x.name).ToList();
		hand = hand.OrderBy(x => x.basePower).ToList();
	}

	public void playCard(Card card) {
		if (card.specialAbility.CompareTo("Leader") == 0) {
			card.selected = false;
			card.targetPos = card.originalPos;
		}
		if (bm.medic || bm.medicEnemy) {
			bm.removeFromDiscard(card);
			photonView.RPC("removeFromDiscardRPC",PhotonTargets.Others,card.name,"Enemy");
		} else if (card.specialAbility.CompareTo("Leader") != 0)
			hand.Remove(card);
		if (card.specialAbility.CompareTo("Spy") == 0) {
			card.player = "Enemy";
			drawCard(2);
			setEnemyHandSize(2);
		}
		card.inHand = false;
		bm.updateBoard(card);
		if ((card.specialAbility.CompareTo("Medic") != 0 || bm.discardPileMedicPlayer.Count == 0) && card.specialAbility.CompareTo("Leader") != 0)
			gm.nextTurn();
		if (card.specialAbility.CompareTo("Double") == 0 && card.special)
			photonView.RPC("sendDouble",PhotonTargets.Others,card.name,card.type);
		else
			photonView.RPC("sendCard",PhotonTargets.Others,card.name);
	}

	public Card createCard(string name) {
		Card card = (Card)Instantiate(cardDict[name],deckPos.position,Quaternion.identity);
		card.name = name;
		card.player = "Player";
		card.gameObject.SetActive(true);
		hand.Add(card);
		return card;
	}

	public void drawCard(int count) {
		while (count > 0 && deck.Count > 0) {
			string randomCard = deck[Random.Range(0,deck.Count)];
			deck.Remove(randomCard);
			Card card = createCard(randomCard);
			count--;
		}
		sortHand();
	}

	public void redrawCard(Card card) {
		string randomCard = deck[Random.Range(0,deck.Count)];
		deck.Remove(randomCard);
		Card c = createCard(randomCard);
		int index = hand.IndexOf(card);
		hand.Remove(card);
		hand.Remove(c);
		hand.Insert(index,c);
		deck.Add(card.name);
		bm.displayCardsBigList = hand;
		bm.redrawCount++;
		selected = null;
		Destroy(card.gameObject);
	}

	public void playFromDeck(string name) {
		if (deck.Contains(name)) {
			deck.Remove(name);
			Card card = createCard(name);
			setEnemyHandSize(1);
			playCard(card);
		}
	}

	public void addCardToHand(Card card) {
		card.player = "Player";
		card.inDiscard = false;
		card.inHand = true;
		hand.Add(card);
		sortHand();
		bm.discardPileMedicEnemy.Remove(card);
	}

	public void setEnemyHandSize(int i) {
		photonView.RPC("setEnemyHandSizeRPC",PhotonTargets.Others,i);
	}

	[PunRPC] void setEnemyHandSizeRPC(int value) {
		handCountEnemy += value;
	}

	public void removeFromDiscard(Card card) {
		photonView.RPC("removeFromDiscardRPC",PhotonTargets.Others,card.name,"Player");
	}

	[PunRPC] void removeFromDiscardRPC(string name, string player) {
		Card c = null;
		List<Card> list = null;
		bm.medicEnemy = false;
		if (player.CompareTo("Player") == 0)
			list = bm.discardPileMedicPlayer;
		else
			list = bm.discardPileMedicEnemy;
		foreach (Card card in list) {
			if (card.name.CompareTo(name) == 0) {
				card.player = player;
				bm.removeFromDiscard(card);
				c = card;
				break;
			}
		}
		if (c != null)
			Destroy(c.gameObject);
	}

	public void playDecoy(Card card) {
		if (card.specialAbility.CompareTo("Double") == 0)
			bm.doubleCardsPlayer[card.type] = null;
		bm.cardsPlayer[card.type].Remove(card);
		card.inHand = true;
		hand.Add(card);
		hand.Remove(selected);
		sortHand();
		selected.type = card.type;
		selected.inHand = false;
		bm.updateBoard(selected);
		gm.nextTurn();
		photonView.RPC("sendDecoy",PhotonTargets.Others,card.name);
	}

	[PunRPC] void sendCard(string name) {
		if (name.CompareTo(leaderNameEnemy) == 0) {
			leaderEnemy.inHand = false;
			return;
		}
		Card card = createEnemyCard(name);
		bm.updateBoard(card);
		if ((card.specialAbility.CompareTo("Medic") != 0 || bm.discardPileMedicEnemy.Count == 0) && card.specialAbility.CompareTo("Leader") != 0) {
			gm.nextTurn();
			handCountEnemy -= 1;
		}
	}

	[PunRPC] void sendDouble(string name, string type) {
		Card card = createEnemyCard(name);
		card.type = type;
		bm.updateBoard(card);
		gm.nextTurn();
		handCountEnemy -= 1;
	}

	[PunRPC] void sendDecoy(string name) {
		gm.nextTurn();
		Card card = bm.removeCard(name);
		bm.cardsEnemy[card.type].Remove(card);
		Card decoy = createEnemyCard("Decoy");
		decoy.type = card.type;
		//card.targetPos = handPosEnemy.position;
		Destroy(card.gameObject);
		bm.updateBoard(decoy);
	}

	[PunRPC] void sendLeader(string name) {
		leaderNameEnemy = name;
	 	leaderEnemy = (Card)Instantiate(cardDict[name],leaderPosEnemy.position,Quaternion.identity);
		leaderEnemy.name = name;
		leaderEnemy.player = "Enemy";
		leaderEnemy.targetPos = leaderPosEnemy.position;
		leaderEnemy.gameObject.SetActive(true);
		if (leaderPlayer.name.CompareTo("Emhyr var Emreis The White Flame") == 0 || leaderEnemy.name.CompareTo("Emhyr var Emreis The White Flame") == 0) {
			leaderPlayer.inHand = false;
			leaderEnemy.inHand = false;
		}
	}

	public void nextTurn() {
		photonView.RPC("nextTurnRPC",PhotonTargets.All);
	}

	[PunRPC] void nextTurnRPC() {
		gm.nextTurn();
	}

	public void requestCards() {
		photonView.RPC("requestCardsRPC",PhotonTargets.Others);
	}

	[PunRPC] void requestCardsRPC() {
		Card card1 = hand[Random.Range(0,hand.Count)];
		hand.Remove(card1);
		Card card2 = hand[Random.Range(0,hand.Count)];
		hand.Remove(card2);
		Card card3 = hand[Random.Range(0,hand.Count)];
		hand.Add(card1);
		hand.Add(card2);
		sortHand();
		photonView.RPC("recieveCards",PhotonTargets.Others,card1.name,card2.name,card3.name);
	}

	[PunRPC] void recieveCards(string name1,string name2,string name3) {
		Card card1 = createCard(name1);
		Card card2 = createCard(name2);
		Card card3 = createCard(name3);
		card1.player = "Enemy";
		card2.player = "Enemy";
		card3.player = "Enemy";
		hand.Remove(card1);
		hand.Remove(card2);
		hand.Remove(card3);
		List<Card> list = new List<Card>();
		list.Add(card1);
		list.Add(card2);
		list.Add(card3);
		bm.displayCardsBigList = list;
	}

	Card createEnemyCard(string name) {
		Debug.Log("Creating enemy card: " + name);
		Card card = (Card)Instantiate(cardDict[name],handPosEnemy.position,Quaternion.identity);
		card.name = name;
		card.inHand = false;
		if (card.specialAbility.CompareTo("Spy") == 0)
			card.player = "Player";
		else
			card.player = "Enemy";
		card.gameObject.SetActive(true);
		return card;
	}

	void displayCards(List<Card> list, Transform pos) {
		for(int i=0;i<list.Count;i++) {
			Card card = list[i];
			if (list.Count < 10)
				//card.transform.position = new Vector3(pos.position.x+(i-list.Count/2)*0.9f,pos.position.y,pos.position.z);
				card.targetPos = new Vector3(pos.position.x+(i-list.Count/2)*0.9f,pos.position.y,pos.position.z);
			else
				//card.transform.position = new Vector3(pos.position.x+(i-list.Count/2)*(0.9f-0.1f*(list.Count-10)),pos.position.y,pos.position.z-i*0.01f);
				card.targetPos = new Vector3(pos.position.x+(i-list.Count/2)*(0.9f-0.05f*(list.Count-9)),pos.position.y,pos.position.z-i*0.01f);
		}
	}

	void aiMove() {
		if (PhotonNetwork.offlineMode) { //singleplayer
			if ("Enemy".CompareTo(gm.turn) == 0) { //AI
				if (hand.Count > 0)
					playCard(hand[Random.Range(0,hand.Count)]);
				else
					gm.pass("Enemy");
			}
		}
	}
}
