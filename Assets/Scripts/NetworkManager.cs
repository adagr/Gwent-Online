using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class NetworkManager : MonoBehaviour {

	private PhotonView photonView;
	public int playerCount;
	private ChatGui cg;
	public static string version = "1.62";
	public Text versionText;
	
	// Use this for initialization
	void Start () {
		//PhotonNetwork.ConnectUsingSettings("1.2");
		photonView = GetComponent<PhotonView>();
		PhotonNetwork.playerName = playerName;
		cg = FindObjectOfType<ChatGui>();
		versionText.text = "v "+version;
	}
	
	// Update is called once per frame
	void Update () {
		playerCount = PhotonNetwork.countOfPlayers;
	}
	
	private static string roomName = "RoomName";
	private static string playerName = "PlayerName";
	private string playersInLobby = "";
	private RoomInfo[] roomsList;
	private bool inRoom = false;
	public Vector2 scrollPosition = Vector2.zero;
	public bool debugMode = false;
	private bool hideFullRooms = false;
	
	void OnGUI()
	{
		if (!PhotonNetwork.connected)
		{
			GUILayout.Label(PhotonNetwork.connectionStateDetailed.ToString());
			if (GUI.Button(new Rect(Screen.width/2-50, Screen.height/2-15, 100, 30), "Connect")) {
				PhotonNetwork.ConnectUsingSettings(version);
			}
			playerName = GUI.TextField(new Rect(Screen.width/2-75,Screen.height/2-50,150,30),playerName);
			PhotonNetwork.playerName = playerName;
		}
		else if (PhotonNetwork.room == null)
		{

			roomName = GUI.TextField(new Rect(Screen.width/4+170,Screen.height*3/16,150,30),roomName);
            // Create Room
            if (GUI.Button(new Rect(Screen.width / 4 + 340, Screen.height * 3 / 16, 100, 30), "Create Room"))
            {
                RoomOptions op = new RoomOptions();
                op.maxPlayers = 2;
                PhotonNetwork.CreateRoom(roomName, op, null);
            }
			
			// Join Room
			if (roomsList != null)
			{
				scrollPosition = GUI.BeginScrollView(new Rect(Screen.width/4,Screen.height/4,Screen.width/2,Screen.height/2),scrollPosition,new Rect(0,0,Screen.width/2,20+40*(roomsList.Length)));
				int offset = 0;
				for (int i = 0; i < roomsList.Length; i++) {
					if (roomsList[i].playerCount == 2 && hideFullRooms)
						offset++;
					else {
						GUI.Label(new Rect(0,20+40*(i-offset),Screen.width,30), roomsList[i].name);
						GUI.Label(new Rect(170,20+40*(i-offset),Screen.width,30), roomsList[i].playerCount + "/" + roomsList[i].maxPlayers);
						if (GUI.Button(new Rect(340,20+40*(i-offset), 100, 30), "Connect"))
							PhotonNetwork.JoinRoom(roomsList[i].name);
					}
				}
				GUI.EndScrollView();
			}
			hideFullRooms = GUI.Toggle(new Rect(Screen.width/4+170,Screen.height*3/16+30,150,30),hideFullRooms,"Filter full rooms");
		}
		else if (PhotonNetwork.room != null) {
			GUI.Label(new Rect(Screen.width/2-100,Screen.height/5,300,50),"Connected to " + PhotonNetwork.room.name);
			playersInLobby = "";
			foreach(PhotonPlayer player in PhotonNetwork.playerList)
				playersInLobby += player.name + "\n";
			GUI.Label(new Rect(Screen.width/2-100,Screen.height/4,200,100),"Players in lobby:\n" + playersInLobby);
			if (PhotonNetwork.isMasterClient) {
				if (PhotonNetwork.room.playerCount != PhotonNetwork.room.maxPlayers && !debugMode)
					GUI.Label(new Rect(Screen.width/2-100, Screen.height/2, 150, 30), "Waiting for more players");
				else
					if 	(GUI.Button(new Rect(Screen.width/2-100, Screen.height/2, 150, 30), "Start Game"))
						LoadLevel();
			} else
				GUI.Label(new Rect(Screen.width/2-100,Screen.height*3/16+20,150,30), "Waiting for host to start...");
			if (GUI.Button(new Rect(Screen.width/2-100,Screen.height*5/8,150,30), "Leave room"))
				PhotonNetwork.LeaveRoom();
		}
		if (PhotonNetwork.connected) {
			GUILayout.Label("Players Online: "+playerCount);
			cg.enabled = true;
		}
		
	}
	
	void OnReceivedRoomListUpdate()
	{
		roomsList = PhotonNetwork.GetRoomList();
	}
	void OnJoinedRoom()
	{
		Debug.Log("Connected to Room");
	}
	
	[PunRPC] void LoadLevel() {
		if (PhotonNetwork.isMasterClient)
			photonView.RPC("LoadLevel",PhotonTargets.Others,null);
		PhotonNetwork.LoadLevel("main");
	}
}
