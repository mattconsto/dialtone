﻿using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

public class BookManager : MonoBehaviour {

    public Text txt1;
    public Text txt2;
    public Animation anim;
    bool bookOpen = false;

    private void Awake()
    {
        Debug.Log("BookManager on " + gameObject.name);
    }
    public void populate(List<Socket> socketList)
    {
        Debug.Log("POPULATING BOOK");
        int i = 0;
        txt1.text = "____________________________ \n\n";
        txt2.text = "____________________________ \n\n";
        Text outputTxt;
        foreach (Socket sckt in socketList)
        {
            if (i < (int) (socketList.Count / 2))
                outputTxt = txt1;
            else
                outputTxt = txt2;
            outputTxt.text +=  sckt.name + ":  \n";
            foreach (string name in sckt.getNames())
                outputTxt.text +="\t" + name + "\n";
            outputTxt.text += "____________________________ \n\n";
            i++;
        }
        gameObject.SetActive(false);
    }
    public void openBook()
    {
        if (bookOpen)
            gameObject.SetActive(false);
        else
        {
            anim.Stop();
            anim.Play();
            gameObject.SetActive(true);
        }
        bookOpen = !bookOpen;
        Debug.Log("OPENING BOOK");
        Debug.Log("OPENED BOOK");
    }
}
