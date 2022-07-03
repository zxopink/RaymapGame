using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TgenNetProtocol;
using RaymapGame;
using NetPackets;
using RaymapGame.Rayman2.Persos;
using LiteNetLib;

public class ServerHostScript : NetMonoBehavour
{
    const int PORT = 4513;

    UdpManager host;
    int idCount = 1;
    public ServerHostScript()
    {
        host = new UdpManager(PORT);
    }

    // Start is called before the first frame update
    void Start()
    {
        if (!Main.loaded)
            Main.onLoad += (x, y) => ServerInitialize();
        else
            ServerInitialize();
    }

    public void ServerInitialize()
    {
        host.PeerConnectedEvent += Host_PeerConnectedEvent;
        host.Start();
    }

    Dictionary<NetPeer, int> players = new Dictionary<NetPeer, int>();
    private void Host_PeerConnectedEvent(NetPeer peer)
    {
        Debug.LogWarning("Sending out");
        var ray = Main.rayman; //Spawn on host

        //Send the new player all connected players
        SharePlayersWithNewPeer(peer);

        PlayerSpawn info = new();
        //Share with everyone the new joining player
        info.pose.pos = ray.gameObject.transform.position;
        info.pose.rot = ray.gameObject.transform.rotation.eulerAngles;
        int pId = idCount++;
        players.Add(peer,pId);
        info.id = pId;
        host.SendToAllExcept(info, peer);

        //Initialize the new player's controller (Rayman)
        InitializePlayer setPlayer = new InitializePlayer();
        setPlayer.spawnInfo = info;
        setPlayer.isLocalPlayer = true; //Kinda useless since we use a different type (event)
        peer.Send(setPlayer);
        Debug.LogWarning("Done sending");
    }
    private void SharePlayersWithNewPeer(NetPeer peer)
    {
        foreach (var ray in FindObjectsOfType<OnlineRayman>())
        {
            PlayerSpawn info = new();
            info.pose.pos = ray.gameObject.transform.position;
            info.pose.rot = ray.gameObject.transform.rotation.eulerAngles;
            info.id = ray.Id;
            peer.Send(info);
        }
    }

    //Not very efficent, This function will run for every player's pose report, per Rayman model
    [DgramReceiver]
    public void GetPlayerPos(NetPose playerPose, UdpInfo info)
    {
        int id = players.GetValueOrDefault(info.Peer);
        PlayerPose pPose = new PlayerPose(playerPose, id);
        host.SendToAllExcept(pPose, info.Peer, DeliveryMethod.Unreliable);
    }

    [DgramReceiver]
    public void GetPlayerPos(NetAnim playerAnim, UdpInfo info)
    {
        int id = players.GetValueOrDefault(info.Peer);
        PlayerAnim pPose = new PlayerAnim(playerAnim, id);
        host.SendToAllExcept(pPose, info.Peer, DeliveryMethod.Unreliable);
    }


    // Update is called once per frame
    new void Update()
    {
        if (host.IsRunning)
            host.PollEvents();
    }
}