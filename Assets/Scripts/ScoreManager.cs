using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class ScoreManager : MonoBehaviour {

	public Text meleePowerTextPlayer;
	public Text rangePowerTextPlayer;
	public Text siegePowerTextPlayer;
	public Text totalPowerTextPlayer;
	public Dictionary<string,int> powerPlayer = new Dictionary<string,int>();
	
	public Text meleePowerTextEnemy;
	public Text rangePowerTextEnemy;
	public Text siegePowerTextEnemy;
	public Text totalPowerTextEnemy;
	public Dictionary<string,int> powerEnemy = new Dictionary<string,int>();

	private BoardManager bm;

	// Use this for initialization
	void Start () {
		bm = FindObjectOfType<BoardManager>();
		powerPlayer.Add("melee",0);
		powerPlayer.Add("range",0);
		powerPlayer.Add("siege",0);
		powerPlayer.Add("total",0);
		powerEnemy.Add("melee",0);
		powerEnemy.Add("range",0);
		powerEnemy.Add("siege",0);
		powerEnemy.Add("total",0);
	}
	
	// Update is called once per frame
	void Update () {
		calculateScore(bm.cardsPlayer,powerPlayer);
		calculateScore(bm.cardsEnemy,powerEnemy);

		meleePowerTextPlayer.text = ""+powerPlayer["melee"];
		rangePowerTextPlayer.text = ""+powerPlayer["range"];
		siegePowerTextPlayer.text = ""+powerPlayer["siege"];
		totalPowerTextPlayer.text = ""+powerPlayer["total"];

		meleePowerTextEnemy.text = ""+powerEnemy["melee"];
		rangePowerTextEnemy.text = ""+powerEnemy["range"];
		siegePowerTextEnemy.text = ""+powerEnemy["siege"];
		totalPowerTextEnemy.text = ""+powerEnemy["total"];
	}

	void calculateScore(Dictionary<string,List<Card>> cards, Dictionary<string,int> power) {
		foreach(KeyValuePair<string,List<Card>> list in cards) {
			var p = 0;
			foreach(Card c in list.Value)
				p += c.power;
			power[list.Key] = p;
		}
		power["total"] = power["melee"] + power["range"] + power["siege"];
	}

	/*public void updateScore(string type, int value, string player) {
		if (player.CompareTo("Player") == 0) {
			powerPlayer[type] += value;
			powerPlayer["total"] += value;
		} else {
			powerEnemy[type] += value;
			powerEnemy["total"] += value;
		}
	}*/

	public void newRound() {
		powerPlayer["melee"] = 0;
		powerPlayer["range"] = 0;
		powerPlayer["siege"] = 0;
		powerPlayer["total"] = 0;

		powerEnemy["melee"] = 0;
		powerEnemy["range"] = 0;
		powerEnemy["siege"] = 0;
		powerEnemy["total"] = 0;
	}
}
