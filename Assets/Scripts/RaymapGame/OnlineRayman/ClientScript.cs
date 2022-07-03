using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TgenNetProtocol;
using RaymapGame;
using NetPackets;
using Cysharp.Threading.Tasks;
using static RaymapGame.Rayman2.Persos.rayman;
using RaymapGame.Rayman2.Persos;

public class ClientScript : NetMonoBehavour
{
    const int PORT = 4513;
    const string LOCAL_HOST = "127.0.0.1";
    public bool SelfHosted = false;

    private static UdpManager client;
    public static UdpManager Client => client;
    public ClientScript()
    {
        client = new UdpManager();
    }

    // Start is called before the first frame update
    void Start()
    {
        if (!Main.loaded)
            Main.onLoad += (x, y) => ClienInitialize();
        else
            ClienInitialize();
    }

    void ClienInitialize()
    {
        Main.rayman.enabled = false; //Disable player until we get a response from the server
        client.Start();
        if (SelfHosted)
            SelfConnect();
        else
            Connect();
    }

    [DgramReceiver]
    public void PlayerJoined(InitializePlayer spawnInfo)
    {
        Debug.LogWarning("Spawning myself");
        Main.rayman.enabled = true; //Disable player until we get a response from the server
        var onlineComp = Main.rayman.gameObject.AddComponent<OnlineRayman>();
        onlineComp.Init(spawnInfo.spawnInfo.id, spawnInfo.isLocalPlayer /*Always true*/);
        Main.rayman.pos = spawnInfo.spawnInfo.pose.pos;
        Debug.LogWarning("Spawned myself");
    }

    [DgramReceiver]
    public void PlayerJoined(PlayerSpawn spawnInfo)
    {
        Debug.LogWarning("Spawning new player");
        CloneRayman(spawnInfo);
    }

    private async UniTask CloneRayman(PlayerSpawn spawnInfo)
    {
        var rayComp = Main.rayman;
        var newRay = Main.rayman.Clone<YLT_RaymanModel>(spawnInfo.pose.pos);

        //Init online component
        var onlineComp = newRay.gameObject.AddComponent<OnlineRayman>();
        onlineComp.Init(spawnInfo.id, false);

        Main.SetMainActor(rayComp);
        Main.rayman = rayComp;

        await UniTask.NextFrame();
        await UniTask.Delay(100);
        newRay.enabled = false; //Disable controls

        //newRay.anim.Set(Anim.HelicIdle);
        //newRay.anim.Set(Anim.Respawn, 1);
        //newRay.RespawnRay();
    }

    private void Connect()=>
        client.Connect(LOCAL_HOST, PORT); //TODO: Change from local host to a new system in the future
    private void SelfConnect() => //Client is also the host, act as a client and host at the same time
        client.Connect(LOCAL_HOST, PORT);


    // Update is called once per frame
    void Update()
    {
        if (client.IsRunning)
            client.PollEvents();
    }
}
