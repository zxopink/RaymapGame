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

        Vector3Net pos = ray.gameObject.transform.position;
        Vector3Net rot = ray.gameObject.transform.rotation.eulerAngles;
        int pId = idCount++;

        PlayerSpawn info = new(new(pos, rot), pId);
        //Share with everyone the new joining player
        players.Add(peer,pId);
        host.SendToAllExcept(info, peer);

        //Initialize the new player's controller (Rayman)
        InitializePlayer setPlayer = new InitializePlayer(info, true);
        //setPlayer.spawnInfo = info;
        //setPlayer.isLocalPlayer = true; //Kinda useless since we use a different type (event)
        peer.Send(setPlayer);
        Debug.LogWarning("Done sending");
    }
    private void SharePlayersWithNewPeer(NetPeer peer)
    {
        foreach (var ray in FindObjectsOfType<OnlineRayman>())
        {
            Vector3Net pos = ray.gameObject.transform.position;
            Vector3Net rot = ray.gameObject.transform.rotation.eulerAngles;
            int id = ray.Id;
            PlayerSpawn info = new(new(pos,rot), id);
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