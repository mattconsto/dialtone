﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TextWriter : MonoBehaviour {

	private string toWrite = "";
	public int cps = 20;
	public float speakTimeout = 0.5f;
	private float timeElapsed = 0;
	public bool speaking = false;
	public Text text;
	bool ending = false;

	// Use this for initialization
	void Start () {
	}

	public bool Say (string toSay, Color colour, string alignment, bool overwrite=false) {
        //Debug.Log("Say() " + overwrite + " " + speaking);
		if (!speaking || overwrite) {
			speaking = true;
			toWrite = toSay;
			timeElapsed = 0f;
			text.text = "";
			ending = false;
			//text.color = colour;
			if (alignment == "left") {
				text.alignment = TextAnchor.UpperLeft;
			}
			else if (alignment == "right") {
				text.alignment = TextAnchor.UpperRight;
			}
			return true;
		}
		else {
			return false;
		}
	}


	// Update is called once per frame
	void Update () {
		timeElapsed += Time.deltaTime;
		text.text = toWrite.Substring (0,Mathf.Min(toWrite.Length, (int)(timeElapsed * cps)));
		if (!ending && text.text == toWrite) {
			ending = true;
			Invoke("stopSpeaking",speakTimeout);
            //Debug.Log("Stopped speaking - TextWriter");
		}
	}
	void stopSpeaking()
	{
		speaking = false;
	}
		
}
