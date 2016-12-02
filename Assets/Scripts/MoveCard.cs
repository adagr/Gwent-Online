using UnityEngine;
using System.Collections;

public class MoveCard : MonoBehaviour {
	
	private DeckManager dm;
	private BoardManager bm;
	public string type;
	public bool player;

	// Use this for initialization
	void Start () {
		dm = FindObjectOfType<DeckManager>();
		bm = FindObjectOfType<BoardManager>();
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	void OnMouseDown() {
		if (dm.selected == null || bm.redraw)
			return;
		else if (dm.selected.specialAbility.CompareTo("Double") == 0 && (bm.doubleCardsPlayer[type] == null || !bm.doubleCardsPlayer[type].special) && dm.selected.special) { //Double
			dm.selected.selected = false;
			dm.selected.type = type;
			dm.selected.targetPos = transform.position;
			dm.playCard(dm.selected);
		} else if (type.CompareTo(dm.selected.type) == 0) { //melee,range,siege
			if (!player && dm.selected.specialAbility.CompareTo("Spy") == 0)
				dm.playCard(dm.selected);
			else if (player && dm.selected.specialAbility.CompareTo("Spy") != 0)
				dm.playCard(dm.selected);
		} else if (dm.selected.specialAbility.CompareTo("Scorch") == 0 && dm.selected.special) //Scorch
			dm.playCard(dm.selected);
		else if (dm.selected.specialAbility.CompareTo("Leader") == 0)
			dm.playCard(dm.selected);
	}
}
