using RaymapGame;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NetPackets
{
    [Serializable]
    public class PlayerSpawn
    {
        public NetPose pose;
        public int id;
    }

    [Serializable]
    public class InitializePlayer
    {
        public PlayerSpawn spawnInfo;
        public bool isLocalPlayer;
    }

    [Serializable]
    public struct NetPose
    {
        public Vector3Net pos;
        public Vector3Net rot;
        public NetPose(Vector3 pos, Vector3 rot) 
        {
            this.pos = pos;
            this.rot = rot;
        }
    }

    [Serializable]
    public struct NetAnim
    {
        public int anim;
        public float priority;
        public float speed;
        public AnimFlags options;
        public bool reset;

        public NetAnim(int anim, float priority, float speed, AnimFlags options, bool reset)
        {
            this.anim = anim;
            this.priority = priority;
            this.speed = speed;
            this.options = options;
            this.reset = reset;
        }
    }

    [Serializable]
    public struct PlayerAnim
    {
        public NetAnim anim;
        public int id;
        public PlayerAnim(NetAnim anim, int id)
        {
            this.anim = anim;
            this.id = id;
        }
    }

    [Serializable]
    public struct PlayerPose
    {
        public NetPose pose;
        public int id;
        public PlayerPose(NetPose pose, int id)
        {
            this.pose = pose;
            this.id = id;
        }
    }

    [Serializable]
    public struct Vector3Net
    {
        public float x;
        public float y;
        public float z;
        public Vector3Net(float x, float y, float z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }
        public Vector3Net(Vector3 vec)
        {
            x = vec.x;
            y = vec.y;
            z = vec.z;
        }

        public static implicit operator Vector3Net(Vector3 vec) => new Vector3Net(vec);
        public static implicit operator Vector3(Vector3Net vec) => new Vector3(vec.x, vec.y, vec.z);

    }


}
