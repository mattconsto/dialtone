﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GameController : MonoBehaviour {

	//public diagloge
	public float betweenCalls_MIN = 2f;
	public float betweenCalls_MAX = 5f;
	ConversationLoader loader = new ConversationLoader ();
	public SocketController sockControl;


	int day = 1;
	int callsToday = 0;
	int maxCallsToday = 5;
	int storycall = 2;
	List<PendingConnection> calls = new List<PendingConnection>();
	bool pairReady = false;
	bool toOperator = false;
	bool inconversation = false;
	Conversation curconv;

	enum GAMESTATE {START, DRIVE, INTRO,INTRO_INCALL,DAY,DAY_WAITINGONCONNECT}
	GAMESTATE gamestate = GAMESTATE.START;


	//Day Start & beeping
	//Story call
	//Story call end -> main loop
	//waiting for call
	//connection made

	void Awake()
	{
		loader.init ();
	}
	void Update()
	{
		if (!loader.finishedLoading)
			return;
		//setGameState ();
		manageCalls ();
		manageConnections ();
	}
	void manageCalls()
	{
		if (day == 1) {
			//at 3 semi-random times start a new pending (Flashing LED > OP > CONN)
			float t = Random.Range(betweenCalls_MIN,betweenCalls_MAX);
			for(int i=0;i<3;i++)
			{
				Invoke("startNewPair",t);
				t+=Random.Range(betweenCalls_MIN,betweenCalls_MAX);
			}

		}
	}
	void manageConnections()
	{
		//Ask andy code for connection complete for the pair
		//Once connection made

		//if tooperator
			//display connection pair request text
				//if this is the story, display the story text instead
		//else
			//don't display call unless tapped

		for (int i=0; i<calls.Count; i++) {
			if(!calls[i].spokenToOperator && sockControl.getConnectedTo(calls[i].incomingPort).name=="operator" && !inconversation)
			{
				calls[i].spokenToOperator = true;
				//get operator[story] conversation next & display
				curconv = loader.getNextConversation();
				StartCoroutine(sendConversation());
				inconversation = true;
			}
			else
			{
				calls[i].connected = true;
			}
		}

		//Display the LED for the incoming call

		//Mark the target port as unavailable

	}
	IEnumerator sendConversation()
	{
		while(curconv.hasNextSentance())
		{
			Debug.Log("[Story]"+curconv.getNextSentance().content);

			return new WaitforSeconds(0.1);
		}
		inconversation = false;
	}
	void startNewPair()
	{	
		if (callsToday < maxCallsToday) {
			chosePorts();
			pairReady = true;
		}
	}
		
	void chosePorts()
	{
		toOperator = !toOperator;//calls go to operator, then to target ect
		//if toOperator, one port needs to be operator port
		PendingConnection pending = new PendingConnection ();
		Socket socketobj = getAvailablePort ();
		if (socketobj == null)
			return;
		pending.targetPort = socketobj.name;
		socketobj = getAvailablePort ();
		if (socketobj == null)
			return;
		pending.incomingPort = socketobj.name;
		pending.conv = loader.getRandomConversation ();
		
		//Display the input socket as lit up
		
		//tell andy code to listen for connection
		
	}
	Socket getAvailablePort()
	{
		List<Socket> list = sockControl.getAllSockets ();
		foreach (Socket sock in list) {
			if(!sock.markedForUse)
			{
				sock.markedForUse = true;
				return sock;
			}
		}
		return null;
	}
	void setGameState()
	{
		if (gamestate == GAMESTATE.START)
			gamestate = GAMESTATE.DRIVE;
		if (gamestate == GAMESTATE.DRIVE) {
			//if driving scene complete
			gamestate = GAMESTATE.INTRO;
		}
		if (gamestate == GAMESTATE.INTRO) {
			loader.getStory(day);
			Debug.Log("story conversation displayed here");
			//display the story conversation
			gamestate = GAMESTATE.INTRO_INCALL;
		}
		if (gamestate == GAMESTATE.INTRO_INCALL) {
			//if call ended
			gamestate = GAMESTATE.DAY;
		}
		if (gamestate == GAMESTATE.DAY) {
			//if toOperator & call complete or !toOperator
			if(!pairReady)
			{
				Invoke("startNewPair",Random.Range(betweenCalls_MIN,betweenCalls_MAX));
			}
		}
	}
} 
