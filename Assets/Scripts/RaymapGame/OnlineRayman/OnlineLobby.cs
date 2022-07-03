using Cysharp.Threading.Tasks;
using RaymapGame;
using RaymapGame.Rayman2.Persos;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TgenNetProtocol;
using System;

public class OnlineLobby : MonoBehaviour
{
    bool inLobby = true;
    public Controller loader;
    // Start is called before the first frame update
    void Start()
    {
        loader.enabled = false;
        inLobby = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (inLobby)
            ListenToAnswer();
    }

    private void ListenToAnswer()
    {
        if (Input.GetKey(KeyCode.H))
        {
            Host();
            inLobby = false;
            loader.enabled = true;
        }
        else if (Input.GetKey(KeyCode.J))
        {
            Join();
            inLobby = false;
            loader.enabled = true;
        }
    }

    private void Join()
    {
        var con = gameObject.AddComponent<ClientScript>();
        con.SelfHosted = false;
    }

    private void Host()
    {
        gameObject.AddComponent<ServerHostScript>();
        var con = gameObject.AddComponent<ClientScript>();
        con.SelfHosted = true;
    }
}
