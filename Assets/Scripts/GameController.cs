﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;

public class GameController : MonoBehaviour
{

	//public dialog

	public StrikeCounter strikeCounter;
	public float betweenCalls_MIN = 2.0f;
	public float betweenCalls_MAX = 5.0f;
	public ConversationLoader loader;
	public SocketController sockControl;
	public TextWriter txtwrite;
	List<string> socketList;
	public BookManager bookMngr;
	public static string OPERATOR_NAME = "operator";
	public ConversationHandler conversationHandler;
	public GameObject DayTitle;
	public float TITLE_TIME = 2f;
	public InstructionsScript instructions;

	int day = 1;
	int callsToday = 0;
	int maxSimultaneousCalls = 2;
	int maxCallsToday = 4;
	int storycall = 2;
	List<Call> calls = new List<Call> ();
	bool pairReady = false;
	bool inconversation = false;
	bool hasInited = false;
	Conversation curconv;
	float timeElapsed = 0;
	float callDelay = 0;
	bool opConnected = false;
	string portTapTarget = "";
	bool dayStarted = false;
	int tapsIncoming = 0;

	enum GAMESTATE
	{
START,
		DRIVE,
		INTRO,
INTRO_INCALL,
DAY,
DAY_WAITINGONCONNECT

	}

	GAMESTATE gamestate = GAMESTATE.START;
	int badGuyCount = 0;
	int badGuyMax = 3;
	List<string> badGuyNames = new List<string> ();

	//Day Start & beeping
	//Story call
	//Story call end -> main loop
	//waiting for call
	//connection made

	void Awake ()
	{
		loader.init ();
	}

	/// <summary>
	/// Randomly gen names and map them to sockets.
	/// </summary>
	void assignNames (int nameCount)
	{
		shuffleSockets ();
		foreach (string socket in socketList) {
			for (int i = 0; i < nameCount; i++) {
				string name = loader.getRandomName ();
				sockControl.addName (socket, name);
				if (badGuyCount < badGuyMax) {
					badGuyCount++;
					badGuyNames.Add (name);
					Debug.Log ("New Bad Guy:" + name);
				}
			}
		}
		shuffleSockets ();
	}


	void Update ()
	{
		if (!hasInited) {
			hasInited = true;
			// Remove the operator from the socket list.
			socketList = sockControl.getAllSockets ();
			assignNames (1);
			bookMngr.populate (socketList, sockControl);
			portTapTarget = socketList [Random.Range (0, socketList.Count)];
			Debug.Log ("TAP ALL CALLS INVOLVING " + portTapTarget);
		}
		//  Debug.Log("LENGHT" + socketList.Count);
		if (!loader.finishedLoading)
			return;
		//setGameState ();
		manageDay ();
		timeElapsed += Time.deltaTime;
		if (timeElapsed >= callDelay) {
			//Debug.Log(timeElapsed);
			manageCalls ();
			callDelay = Random.Range (betweenCalls_MIN, betweenCalls_MAX);
		}
		manageConnections (Time.deltaTime);
	}

	void manageDay ()
	{
		if (!dayStarted) {
			dayStarted = true;
            
			Debug.Log ("TAP ALL CALLS INVOLVING " + portTapTarget);
			callsToday = 0;
			Invoke ("hideDayTitle", TITLE_TIME);
			DayTitle.gameObject.SetActive (true);
			DayTitle.GetComponentInChildren<Text> ().text = "Day " + day;
			//instructions.display();
			//WIPE SOCKETS AND WIRES
			//foreach(Socket skt in sockControl.getAllSockets())
			if (day == 1) {
				maxCallsToday = 4;
				maxSimultaneousCalls = 1;
				//instructions.setInstruction("None.");
				tapsIncoming = 3;
				setupTapTarget ();
			} else if (day == 2) {
				maxCallsToday = 8;
				maxSimultaneousCalls = 2;
				tapsIncoming = 1;
				setupTapTarget ();
			} else if (day == 3) {
				maxCallsToday = 12;
				maxSimultaneousCalls = 3;
				tapsIncoming = 1;
				setupTapTarget ();
				assignNames (1);
			} else if (day == 4) {
				maxCallsToday = 16;
				maxSimultaneousCalls = 4;
				tapsIncoming = 1;
				setupTapTarget ();
				assignNames (1);
			} else if (day == 5) {
				//END THE GAAAAAAAAAAME
			}

		}
		if (calls.Count == 0 && callsToday == maxCallsToday) {
			dayStarted = false;
			day++;
		}
	}

	void setupTapTarget ()
	{
		//portTapTarget = socketList[Random.Range(0, socketList.Count)];
		//string name = sockControl.getSocket(portTapTarget).getNames()[0];
		//instructions.setInstruction("Tap calls by connecting to the recording box and connect the recording box to the requested socket.\n\nTap all calls from " + name);
		//socketList = sockControl.getAllSockets().Where(x => x != OPERATOR_NAME && x!=portTapTarget).ToList();
	}

	void manageCalls ()
	{
		timeElapsed = 0;
        
		if (calls.Count < maxSimultaneousCalls && callsToday < maxCallsToday + tapsIncoming) {
			startNewPair ();
		}
	}

	void hideDayTitle ()
	{
		DayTitle.gameObject.SetActive (false);
	}

	void manageConnections (float deltaTime)
	{
		List<Call> callsToDelete = new List<Call> ();
		foreach (Call call in calls) {
			string connected = sockControl.getConnectedTo (call.incomingPort);
			string tapConnection = sockControl.getConnectedTo ("tappingSocket");
			bool keepAlive = call.handleState (connected, sockControl, OPERATOR_NAME, conversationHandler, strikeCounter);

			if (!keepAlive) {
				callsToDelete.Add (call);
			}

		}

		foreach (Call call in callsToDelete) {
			Debug.Log ("Killing " + call.incomingPort);
			calls.Remove (call);
		}
		// !!!!!!!!!WARNING: HORRIBLE CODE BELOW!!!!!!!!!!!!

		//Ask andy code for connection complete for the pair
		//Once connection made

		//if tooperator
		//display connection pair request text
		//if this is the story, display the story text instead
		//else
//			//don't display call unless tapped
//		//Debug.Log("MANAGING()");
//		for (int i=0; i<calls.Count; i++) {
//			// Polling loop
//
//			//if(sockControl.getConnectedTo(calls[i].incomingPort)!=null)
//				//Debug.Log("ID:"+i+"PORT:"+calls[i].incomingPort+" connected to "+sockControl.getConnectedTo(calls[i].incomingPort).name+" Target:"+calls[i].targetPort);
//
//
//			if(!calls[i].spokenToOperator && sockControl.getConnectedTo(calls[i].incomingPort)!=null && sockControl.getConnectedTo(calls[i].incomingPort).name=="operator" && !inconversation)
//			{
//				//Debug.Log("CONNTECED CORRECTLY");
//				calls[i].spokenToOperator = true;
//				//get operator[story] conversation next & display
//				curconv = loader.getNextConversation();
//				curconv.setFormatter(sockControl.getSocket(calls[i].targetPort).getRandomName());
//				Debug.Log("targetPort::"+calls[i].targetPort);
//				StartCoroutine(sendConversation());
//				inconversation = true;
//				sockControl.setLED (calls[i].incomingPort, "GREEN");
//				sockControl.setLED ("operator", "GREEN");
//                opConnected = true;
//			}
//			else if(calls[i].spokenToOperator && sockControl.getConnectedTo(calls[i].incomingPort) != null && sockControl.getConnectedTo(calls[i].incomingPort).name==calls[i].targetPort)
//			{
//                //Debug.Log("CONNTECED CORRECTLY");
//                score++;
//				calls[i].connected = true;
//				sockControl.setLED (calls[i].incomingPort, "GREEN");
//				sockControl.setLED (calls[i].targetPort, "GREEN");
//			}
//			else if(calls[i].spokenToOperator && sockControl.getConnectedTo(calls[i].incomingPort) != null && (sockControl.getConnectedTo(calls[i].incomingPort).name!=calls[i].targetPort && sockControl.getConnectedTo(calls[i].incomingPort).name != "operator"))
//			{
//				//DROP CALL
//				sockControl.setLED (calls[i].incomingPort, "OFF");
//                sockControl.getSocket(calls[i].incomingPort).markedForUse = false;
//                sockControl.getSocket(calls[i].targetPort).markedForUse = false;
//                calls.RemoveAt(i);
//                Debug.Log("Dropped Call");
//			}
//			if(calls[i].connected)
//			{
//                Debug.Log("ID:" + i + " t:" + calls[i].timeLeft);
//				calls[i].timeLeft -= deltaTime;
//				if(calls[i].timeLeft < 0)
//				{
//					sockControl.setLED (calls[i].incomingPort, "OFF");
//					sockControl.setLED (calls[i].targetPort, "OFF");
//					calls.RemoveAt(i);
//				}
//                else
//                {
//                    if(sockControl.getConnectedTo(calls[i].incomingPort) == null)
//                    {
//                        sockControl.setLED(calls[i].incomingPort, "OFF");
//                        sockControl.setLED(calls[i].targetPort, "OFF");
//                        sockControl.getSocket(calls[i].incomingPort).markedForUse = false;
//                        sockControl.getSocket(calls[i].targetPort).markedForUse = false;
//                        calls.RemoveAt(i);
//                    }
//                }
//			}
//            if(opConnected && sockControl.getConnectedTo("operator") == null)
//            {
//                opConnected = false;
//                txtwrite.Say("[Disconnected]",new Color(),"left",true);
//            }
//		}

		//Display the LED for the incoming call

	}

	void startNewPair ()
	{	
		choosePorts ();
		pairReady = true;
	}

	void choosePorts ()
	{
		//if toOperator, one port needs to be operator port
		Call call = new Call ();
		string socketA = getAvailablePort ();
		if (socketA == null) { // nothing available
			return;
		}
		sockControl.reserveForCall (socketA);

		string socketB;
		socketB = getAvailablePort ();
		if (socketB == null) {// nothing available, clear A.
			sockControl.unreserveForCall (socketA);
			return;
		}
		sockControl.reserveForCall (socketB);

		call.targetPort = socketA;
		call.incomingPort = socketB;
		call.fromName = sockControl.getSocket (call.incomingPort).getRandomName ();
		call.toName = sockControl.getSocket (call.targetPort).getRandomName ();
		call.operatorConv = loader.getRandomOperatorConversation ();
		call.operatorConv.setFormatter (call.toName);
        

		bool to = false;
		bool from = false;
		if (badGuyNames.Contains (call.toName))
			to = true;
		if (badGuyNames.Contains (call.fromName))
			from = true;
		call.tappedConv = loader.getRandomBadConvo (from, to);
		call.tappedConv.toReplace = call.toName;
		call.tappedConv.fromReplace = call.fromName;



		//Display the input socket as lit up
		//tell andy code to listen for connection
		callsToday++;
		calls.Add (call);
	}


	void shuffleSockets ()
	{
		// Shuffle sockets
		for (int i = 0; i < socketList.Count; i++) {
			string temp = socketList [i];
			int random = Random.Range (i, socketList.Count);
			socketList [i] = socketList [random];
			socketList [random] = temp;
		}
	}

	string getAvailablePort ()
	{
		shuffleSockets ();
		foreach (string sock in socketList) {
			Debug.Log (sock);
			if (!sockControl.isReservedForCall (sock)) {
				return sock;
			}
		}
		return null;
	}

	void setGameState ()
	{
		if (gamestate == GAMESTATE.START)
			gamestate = GAMESTATE.DRIVE;
		if (gamestate == GAMESTATE.DRIVE) {
			//if driving scene complete
			gamestate = GAMESTATE.INTRO;
		}
		if (gamestate == GAMESTATE.INTRO) {
			loader.getStory (day);
			Debug.Log ("story conversation displayed here");
			//display the story conversation
			gamestate = GAMESTATE.INTRO_INCALL;
		}
		if (gamestate == GAMESTATE.INTRO_INCALL) {
			//if call ended
			gamestate = GAMESTATE.DAY;
		}
		if (gamestate == GAMESTATE.DAY) {
			//if toOperator & call complete or !toOperator
			if (!pairReady) {
				Invoke ("startNewPair", Random.Range (betweenCalls_MIN, betweenCalls_MAX));
			}
		}
	}
}
