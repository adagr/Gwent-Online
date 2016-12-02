using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.Analytics;

public class GameManager : MonoBehaviour {

	public string turn;
	public bool playerPass;
	public bool enemyPass;
	public Button passButton;
	public Text mainText;
	public Text playerNameText;
	public Text enemyNameText;
	public Text passedTextPlayer;
	public Text passedTextEnemy;
	public Transform[] lifeTextures;
	public Text factionTextPlayer,factionTextEnemy;

	private ScoreManager sm;
	private BoardManager bm;
	private DeckManager dm;
	public int playerLife;
	public int enemyLife;
	private float newRoundTimer = 0;
	public bool nr = true;
	private PhotonView photonView;
	private string playerName;
	private string enemyName;
	private float holdTimer;
	private string factionPlayer, factionEnemy, leaderPlayer, leaderEnemy;
	private ChatGui cc;
	private bool readyPlayer = false,readyEnemy = false;

	// Use this for initialization
	void Start () {
		photonView = GetComponent<PhotonView>();
		sm = FindObjectOfType<ScoreManager>();
		bm = FindObjectOfType<BoardManager>();
		dm = FindObjectOfType<DeckManager>();
		cc = FindObjectOfType<ChatGui>();

		playerName = PhotonNetwork.playerName;
		if (playerName.CompareTo(PhotonNetwork.playerList[0].name) != 0)
			enemyName = PhotonNetwork.playerList[0].name;
		else
			enemyName = PhotonNetwork.playerList[1].name;
		if (PhotonNetwork.isMasterClient) {
			int n = Random.Range(0,2);
			setStartPlayer(n);
			photonView.RPC("setStartPlayer",PhotonTargets.Others,1-n);
			if (turn.CompareTo("Player") == 0)
				mainText.text = playerName + " begins";
			else
				mainText.text = enemyName + " begins";
		}
		playerLife = 2;
		enemyLife = 2;
		playerNameText.text = playerName;
		enemyNameText.text = enemyName;
		mainText.enabled = false;
		playerPass = true;
		enemyPass = true;
		nr = true;
		passButton.interactable = false;
		passedTextPlayer.enabled = false;
		passedTextEnemy.enabled = false;

		factionPlayer = DeckBuilder.factions[DeckBuilder.selectedFaction];
		factionTextPlayer.text = factionPlayer;
		photonView.RPC("sendFaction",PhotonTargets.Others,factionPlayer);
	}

	void endOfRound(string winner) {
		if (winner.CompareTo("Player") == 0) {
			enemyLife--;
			turn = "Player";
			mainText.text = playerName+" won the round";
		} else if (winner.CompareTo("Enemy") == 0) {
			playerLife--;
			turn = "Enemy";
			mainText.text = enemyName+" won the round";
		} else {
			enemyLife--;
			playerLife--;
			mainText.text = "Draw";
		}
		mainText.enabled = true;
	}
	
	// Update is called once per frame
	void Update () {
		if (playerPass && enemyPass && !nr) {
			if (sm.powerPlayer["total"] > sm.powerEnemy["total"]) { //player won round
				endOfRound("Player");
				if (factionPlayer.CompareTo("Northern Realms") == 0 && enemyLife == 1)
					dm.drawCard(1);
			} else if (sm.powerPlayer["total"] < sm.powerEnemy["total"]) { //enemy won round
				endOfRound("Enemy");
				if (factionEnemy.CompareTo("Northern Realms") == 0 && playerLife == 1)
					dm.handCountEnemy++;
			} else { //draw
				if (factionPlayer.CompareTo("Nilfgaardian Empire") == 0 && factionEnemy.CompareTo("Nilfgaardian Empire") == 0) {
					endOfRound("Draw");
				} else if (factionPlayer.CompareTo("Nilfgaardian Empire") == 0) {
					endOfRound("Player");
				} else if (factionEnemy.CompareTo("Nilfgaardian Empire") == 0) {
					endOfRound("Enemy");
				} else {
					endOfRound("Draw");
				}
			}

			if (playerLife == 1)
				lifeTextures[0].GetComponent<SpriteRenderer>().enabled = true;
			if (enemyLife == 1)
				lifeTextures[2].GetComponent<SpriteRenderer>().enabled = true;

			if (playerLife == 0 && enemyLife == 0) { //draw
				mainText.text = "Draw";
				mainText.enabled = true;
				lifeTextures[1].GetComponent<SpriteRenderer>().enabled = true;
				lifeTextures[3].GetComponent<SpriteRenderer>().enabled = true;
                Analytics.CustomEvent("gameOver", new Dictionary<string, object>{{ "draw", factionPlayer }});
            } else if (playerLife == 0) { //enemy win
				lifeTextures[1].GetComponent<SpriteRenderer>().enabled = true;
				mainText.text = enemyName+" won the game";
				mainText.enabled = true;
                Analytics.CustomEvent("gameOver", new Dictionary<string, object> { { "loss", factionPlayer } });
            } else if (enemyLife == 0) { //player win
				lifeTextures[3].GetComponent<SpriteRenderer>().enabled = true;
				mainText.text = playerName+" won the game";
				mainText.enabled = true;
                Analytics.CustomEvent("gameOver", new Dictionary<string, object> { { "win", factionPlayer } });
            }
			nr = true;
		}
		if (nr && readyPlayer && readyEnemy && PhotonNetwork.isMasterClient) {
			if (newRoundTimer > 3)
				photonView.RPC("newRound",PhotonTargets.All,null);
			newRoundTimer += Time.deltaTime;
		}
		if (Input.GetKey("space") && turn.CompareTo("Player") == 0 && !playerPass) { // Pass
			if (holdTimer > 1)
				passRPC();
			holdTimer += Time.deltaTime;
		} else if (Input.GetKey(KeyCode.Escape) || Input.GetKey("q")) { // Forfeit game
			if (holdTimer > 1) {
				PhotonNetwork.LeaveRoom();
				string[] channel = new string[1];
				channel[0] = PhotonNetwork.room.name;
				cc.chatClient.Unsubscribe(channel);
				PhotonNetwork.LoadLevel("menu");
			}
			holdTimer += Time.deltaTime;
		} else
			holdTimer = 0;
		if (PhotonNetwork.playerList.Length < 2) {
			enemyLife = 0;
			playerPass = true;
			enemyPass = true;
			mainText.text = enemyName+" has left the game";
			mainText.enabled = true;
		}
		if (!bm.redraw && !readyPlayer) {
			readyPlayer = true;
			photonView.RPC("ready",PhotonTargets.Others);
		}
		if (nr && readyPlayer && readyEnemy)
			mainText.enabled = true;
	}

	[PunRPC] void ready() {
		readyEnemy = true;
	}

	public void nextTurn() {
		if (playerPass || enemyPass)
			return;
		else if (turn.CompareTo("Player") == 0) {
			turn = "Enemy";
			passButton.interactable = false;
			//mainText.text = enemyName + " turn";
		} else {
			turn = "Player";
			passButton.interactable = true;
			//mainText.text = playerName + " turn";
		}
	}

	[PunRPC] public void pass(string type) {
		nextTurn();
		if (type.CompareTo("Player") == 0) {
			playerPass = true;
			passedTextPlayer.enabled = true;
		}
		else {
			enemyPass = true;
			passedTextEnemy.enabled = true;
		}
	}

	public void passRPC() {
		passButton.interactable = false;
		pass("Player");
		photonView.RPC("pass",PhotonTargets.Others,"Enemy");
	}

	[PunRPC] void newRound() {
		if (playerLife == 0 || enemyLife == 0)
			LoadLevel();
		bm.newRound();
		sm.newRound();
		playerPass = false;
		enemyPass = false;
		nr = false;
		newRoundTimer = 0;
		mainText.enabled = false;
		passedTextPlayer.enabled = false;
		passedTextEnemy.enabled = false;
		if (turn.CompareTo("Player") == 0)
			passButton.interactable = true;
	}

	/*void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info) {
		if (stream.isWriting) {

		}
		else {	

		}
	}*/

	[PunRPC] void LoadLevel() {
		if (PhotonNetwork.isMasterClient)
			photonView.RPC("LoadLevel",PhotonTargets.Others,null);
		PhotonNetwork.LeaveRoom();
		string[] channel = new string[1];
		channel[0] = PhotonNetwork.room.name;
		cc.chatClient.Unsubscribe(channel);
		PhotonNetwork.LoadLevel("menu");
	}

	[PunRPC] void setStartPlayer(int startingPlayer) {
		if (startingPlayer == 0) {
			turn = "Enemy";
			mainText.text = enemyName + " begins";
		} else {
			turn = "Player";
			mainText.text = playerName + " begins";
		}
	}

	[PunRPC] void sendFaction(string name) {
		factionEnemy = name;
		factionTextEnemy.text = factionEnemy;
	}
}
