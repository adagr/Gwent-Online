using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;

public class DeckBuilder : MonoBehaviour {

	public Transform[] cardPos;
	public Transform[] deckPos;
	public Transform offscreenPos;
	public Transform leaderPos;
	public List<CardDeckBuilder> cards = new List<CardDeckBuilder>();
	public List<CardDeckBuilder> deck = new List<CardDeckBuilder>();
	public CardDeckBuilder selected;
	public Text totalCardsText,unitCardsText,specialCardsText,totalPowerText,heroCardsText;
	private int specialCards, heroCards, totalPower, totalCards;
	public static List<string> cardDeck = new List<string>();
	private bool startUp = true;
	public Button menuButton;
	public Color normalColor;
	public static int selectedFaction = 0;
	public static int selectedLeader = 0;
	public static string selectedLeaderName = "";
	public static string[] factions = {"Northern Realms","Nilfgaardian Empire"};
	public SpriteRenderer[] factionSprites;
	private bool ignoreFaction = true;
	public bool selectLeader = false;

	public List<CardDeckBuilder> northernDeck, nilfgaardDeck;
	private List<CardDeckBuilder> neutralCards = new List<CardDeckBuilder>();
	private List<CardDeckBuilder> northernCards = new List<CardDeckBuilder>();
	private List<CardDeckBuilder> nilfgaardCards = new List<CardDeckBuilder>();
	private Dictionary<string, List<CardDeckBuilder>> factionCards = new Dictionary<string, List<CardDeckBuilder>>();
	private Dictionary<string, List<CardDeckBuilder>> factionDecks = new Dictionary<string, List<CardDeckBuilder>>();
	public Dictionary<string, List<CardLeader>> leaderCards = new Dictionary<string, List<CardLeader>>();

	private int startIndexCards;
	public int StartIndexCards {
		get {return startIndexCards;}
		set {
			startIndexCards = setIndex(startIndexCards,value,cards);
		}
	}
	private int startIndexDeck = 0;
	public int StartIndexDeck {
		get {return startIndexDeck;}
		set {
			startIndexDeck = setIndex(startIndexDeck,value,deck);
		}
	}

	int setIndex(int index, int value, List<CardDeckBuilder> list) {
		if (value < 0)
			index = 0;
		else if (value >= list.Count-3 && value >= 3) {
			moveCardsOffscreen(list,index);
			int offset = 6-(value-(list.Count-3));
			index = list.Count - offset;
		} else {
			moveCardsOffscreen(list,index);
			index = value;
		}
		return index;
	}

	void initCards(Transform child,List<CardDeckBuilder> list) {
		foreach (Transform grandChild in child) {
			if (grandChild.name.CompareTo("Leaders") != 0) {
				var card  = grandChild.GetComponent<CardDeckBuilder>();
				list.Add(card);
			} else {
				List<CardLeader> leaders = new List<CardLeader>();
				foreach (Transform ggc in grandChild) {
					var card  = ggc.GetComponent<CardLeader>();
					leaders.Add(card);
				}
				leaderCards.Add(child.name,leaders);
			}
		}
	}

	public void changeFaction(int i) {
		unDisplayLeaders();
		leaderCards[factions[selectedFaction]][selectedLeader].transform.position = offscreenPos.position;
		specialCards = 0;
		heroCards = 0;
		totalPower = 0;
		totalCards = 0;
		StartIndexCards = 0;
		StartIndexDeck = 0;
		factionSprites[selectedFaction].enabled = false;
		selectedFaction = (selectedFaction+i) % factions.Length;
		selectedFaction = (selectedFaction < 0)?factions.Length-1:selectedFaction;
		leaderCards[factions[selectedFaction]][selectedLeader].transform.position = leaderPos.position;
		selectedLeaderName = leaderCards[factions[selectedFaction]][selectedLeader].name;
		factionSprites[selectedFaction].enabled = true;
		foreach (CardDeckBuilder card in deck) {
			card.inDeck = false;
			card.transform.position = offscreenPos.position;
			card.count = 1;
		}
		foreach (CardDeckBuilder card in cards) {
			card.transform.position = offscreenPos.position;
			card.count = card.maxCount;
			card.inDeck = false;
		}
		deck.Clear();
		cards.Clear();
		cardDeck.Clear();
		foreach(CardDeckBuilder card in neutralCards) {
			card.count = card.maxCount;
			card.inDeck = false;
			cards.Add(card);
		}
		foreach(CardDeckBuilder card in factionCards[factions[selectedFaction]]) {
			card.count = card.maxCount;
			card.inDeck = false;
			cards.Add(card);
		}
		ignoreFaction = true;
		foreach(CardDeckBuilder card in factionDecks[factions[selectedFaction]])
			addToDeck(card);
		ignoreFaction = false;
        sortCards();
    }

    public void sortCards() {
        moveCardsOffscreen(cards, startIndexCards);
        cards = cards.OrderBy(x => x.name).ToList();
        cards = cards.OrderByDescending(x => x.power).ToList();
        cards = cards.OrderByDescending(x => x.hero).ToList();
        cards = cards.OrderByDescending(x => x.special).ToList();
    }

	// Use this for initialization
	void Start () {
		foreach (Transform child in transform) {
			if (child.name.CompareTo("Neutral") == 0) {
				initCards(child,neutralCards);
			} else if (child.name.CompareTo("Northern Realms") == 0) {
				initCards(child,northernCards);
			} else if (child.name.CompareTo("Nilfgaardian Empire") == 0) {
				initCards(child,nilfgaardCards);
			}
		}
		leaderCards[factions[selectedFaction]][selectedLeader].transform.position = leaderPos.position;
		selectedLeaderName = leaderCards[factions[selectedFaction]][selectedLeader].name;
		neutralCards = neutralCards.OrderBy(x => x.name).ToList();
		neutralCards = neutralCards.OrderByDescending(x => x.power).ToList();
		neutralCards = neutralCards.OrderByDescending(x => x.special).ToList();
		northernCards = northernCards.OrderBy(x => x.name).ToList();
		northernCards = northernCards.OrderByDescending(x => x.power).ToList();
		nilfgaardCards = nilfgaardCards.OrderBy(x => x.name).ToList();
		nilfgaardCards = nilfgaardCards.OrderByDescending(x => x.power).ToList();
		factionCards.Add("Northern Realms",northernCards);
		factionCards.Add("Nilfgaardian Empire",nilfgaardCards);
		factionDecks.Add("Northern Realms",northernDeck);
		factionDecks.Add("Nilfgaardian Empire",nilfgaardDeck);
		if (cardDeck.Count > 0) {
			foreach (CardDeckBuilder card in neutralCards)
				cards.Add(card);
			foreach(CardDeckBuilder card in factionCards[factions[selectedFaction]]) {
				card.count = card.maxCount;
				cards.Add(card);
			}
			foreach (string name in cardDeck) {
				foreach (CardDeckBuilder card in cards) {
					if (card.name.CompareTo(name) == 0) {
						addToDeck(card);
						break;
					}
				}
			}
			startUp = false;
		} else {
			startUp = false;
			foreach(CardDeckBuilder card in neutralCards)
				cards.Add(card);
			foreach(CardDeckBuilder card in northernCards)
				cards.Add(card);
			foreach (CardDeckBuilder card in northernDeck)
				addToDeck(card);
			ignoreFaction = false;
		}
		factionSprites[selectedFaction].enabled = true;
        sortCards();
	}
	
	// Update is called once per frame
	void Update () {
		displayCards(cards,cardPos,startIndexCards);
		displayCards(deck,deckPos,startIndexDeck);
		int unitCards = totalCards-specialCards;
		totalCardsText.text = ""+totalCards;
		unitCardsText.text = ""+unitCards;
		specialCardsText.text = specialCards+"/10";
		heroCardsText.text = ""+heroCards;
		totalPowerText.text = ""+totalPower;

		if (unitCards < 22) {
			menuButton.interactable = false;
			unitCardsText.color = Color.red;
		} else {
			menuButton.interactable = true;
			unitCardsText.color = normalColor;
		}

		if (selectLeader) {
			float x = Input.GetAxis("Mouse ScrollWheel");
			if (x < 0 || Input.GetKeyDown("right") || Input.GetKeyDown("d")) {
				selectedLeader = Mathf.Min(++selectedLeader,leaderCards[factions[selectedFaction]].Count-1);
				displayLeaders();
			} else if (x > 0 || Input.GetKeyDown("left") || Input.GetKeyDown("a")) {
				selectedLeader = Mathf.Max(--selectedLeader,0);
				displayLeaders();
			}
		}
	}

    public void clearDeck() {
        int count = totalCards;
        for (int i=0;i<count;i++) {
            removeFromDeck(deck[0]);
        }
        sortCards();
        startIndexDeck = 0;
    }

	public void addToDeck(CardDeckBuilder card) {
		if (specialCards == 10 && card.special)
			return;
		if (card.special)
			specialCards++;
		if (card.hero)
			heroCards++;
		totalPower += card.power;
		totalCards++;
		if (!startUp)
			cardDeck.Add(card.name);
		if (!ignoreFaction)
			factionDecks[factions[selectedFaction]].Add(card);
		addCard(cards,deck,card);
	}

	public void removeFromDeck(CardDeckBuilder card) {
		if (card.special)
			specialCards--;
		if (card.hero)
			heroCards--;
		totalPower -= card.power;
		totalCards--;
		if (!startUp)
			cardDeck.Remove(card.name);
		if (card.maxCount > 1)
			factionDecks[factions[selectedFaction]].Remove(card.clone);
		else
			factionDecks[factions[selectedFaction]].Remove(card);
		addCard(deck,cards,card);
	}

	void addCard(List<CardDeckBuilder> from, List<CardDeckBuilder> to, CardDeckBuilder card) {
		selected = null;
		if (card.count > 1 && card.count == card.maxCount) {
			CardDeckBuilder c;
			if (card.clone == null) {
				c = (CardDeckBuilder) Instantiate(card,offscreenPos.position,Quaternion.identity);
				c.name = card.name;
				card.clone = c;
				c.clone = card;
			} else
				c = card.clone;
			c.count = 1;
			card.count -= 1;
			to.Add(c);
			c.inDeck = !c.inDeck;
		} else if (card.count > 1 && card.maxCount > 1) {
			card.count -= 1;
			card.clone.count += 1;
		} else if (card.maxCount > 1) {
			card.clone.count += 1;
			from.Remove(card);
			card.inDeck = !card.inDeck;
			card.transform.position = offscreenPos.position;
		} else {
			from.Remove(card);
			to.Add(card);
			card.inDeck = !card.inDeck;
			card.transform.position = offscreenPos.position;
		}
	}

	void displayCards(List<CardDeckBuilder> list, Transform[] pos, int index) {
		int i = index;
		while (i < index+6 && i < list.Count) {
			list[i].transform.position = pos[i-index].position;
			i++;
		}
	}

	void moveCardsOffscreen(List<CardDeckBuilder> list, int index) {
		for (int i=index;i<Mathf.Min(index+6,list.Count);i++)
			list[i].transform.position = offscreenPos.position;
	}

	public void displayLeaders() {
		selectLeader = true;
		int i = 0;
		foreach (CardLeader card in leaderCards[factions[selectedFaction]]) {
			card.selectBigSprite();
			card.transform.position = leaderPos.position + Vector3.left*3.5f*(selectedLeader-i) + Vector3.down*2;
			i++;
		}
	}

	public void unDisplayLeaders() {
		selectLeader = false;
		selectedLeaderName = leaderCards[factions[selectedFaction]][selectedLeader].name;
		int i = 0;
		foreach (CardLeader card in leaderCards[factions[selectedFaction]]) {
			card.selectSmallSprite();
			if (i == selectedLeader)
				card.transform.position = leaderPos.position;
			else
				card.transform.position = offscreenPos.position;
			i++;
		}
	}
}
