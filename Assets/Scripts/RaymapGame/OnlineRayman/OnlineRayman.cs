using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TgenNetProtocol;
using RaymapGame.Rayman2.Persos;
using NetPackets;
using TgenNetProtocol;
using RaymapGame;

public class OnlineRayman : NetMonoBehavour
{
    private bool isLocalPlayer = false;
    public bool IsLocal => isLocalPlayer;

    int id = -1;

    public int Id => id;

    public bool Initialized => id != -1;

    private rayman Ray;
    private AnimHandler Anim;

    public void Init(int id, bool isPlayer)
    {
        if (Initialized)
            return;

        this.id = id;
        this.isLocalPlayer = isPlayer;
        Ray = GetComponent<rayman>();
        Anim = Ray.anim;
        Anim.OnAnimSet = OnAnimSet;
        Debug.LogWarning("Id: " + id + " islocal? " + isLocalPlayer + " and pos at: " + Ray.pos);
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    NetAnim currAnim;
    public void OnAnimSet(int anim, float priority, float speed, AnimFlags options, bool reset)
    {
        if (IsLocal)
        {
            //Share pose
            currAnim = new NetAnim(anim, priority, speed, options, reset);
        }
    }

    public override void Tick()
    {
        base.Tick();
        if (IsLocal)
        {
            //Share pose
            NetPose pose = new NetPose(Ray.pos, Ray.rot);
            ClientScript.Client.SendToAll(pose, LiteNetLib.DeliveryMethod.Unreliable);

            ClientScript.Client.SendToAll(currAnim, LiteNetLib.DeliveryMethod.Unreliable);
        }
    }

    //Not very efficent, This function will run for every player's pose report, per Rayman model
    [DgramReceiver]
    public void GetPlayerPos(PlayerPose playerPose)
    {
        if (playerPose.id != Id)
            return;

        Ray.pos = playerPose.pose.pos;
        Ray.rot = playerPose.pose.rot;
    }

    [DgramReceiver]
    public void GetPlayerAnim(PlayerAnim playerAnim)
    {
        if (playerAnim.id != Id)
            return;

        NetAnim animInfo = playerAnim.anim;
        Anim.Set(animInfo.anim, animInfo.priority, animInfo.speed, animInfo.options, animInfo.reset);
    }

    // Update is called once per frame
    void Update()
    {
        base.Update(); //Calls tick
    }
}
