using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TgenNetProtocol;

public class NetMonoBehavour : MonoBehaviour, INetworkObject
{
    public List<MethodData> ServerMethods { get; private set; }
    public List<MethodData> ClientMethods { get; private set; }
    public List<MethodData> DgramMethods { get; private set; }

    public const int TICK_RATE = 30; //ticks per second
    private float tickTimer = 0;

    public NetMonoBehavour()
    {
        SetUpMethods();

        //`System.Threading.Monitor` check later, responsible for thread work
        //Task.Run(AddToAttributes);
        Add2Attributes();
    }

    public virtual void Tick()
    {
        
    }

    protected void Update()
    {
        tickTimer += Time.deltaTime;
        if (tickTimer > (1 / TICK_RATE))
        {
            Tick();
            tickTimer = 0;
        }
    }

    public void SetUpMethods()
    {
        //Gets public/private/(public inherited) methods
        System.Reflection.MethodInfo[] methods = GetType().GetMethods(System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.FlattenHierarchy | System.Reflection.BindingFlags.InvokeMethod);
        IEnumerable<System.Reflection.MethodInfo> serverActions = methods.Where(x => x.GetCustomAttributes(typeof(ServerReceiverAttribute), false).FirstOrDefault() != null);
        IEnumerable<System.Reflection.MethodInfo> clientActions = methods.Where(x => x.GetCustomAttributes(typeof(ClientReceiverAttribute), false).FirstOrDefault() != null);
        IEnumerable<System.Reflection.MethodInfo> dgramAction = methods.Where(x => x.GetCustomAttributes(typeof(DgramReceiverAttribute), false).FirstOrDefault() != null);

        ServerMethods = GetMethodsData(serverActions);
        ClientMethods = GetMethodsData(clientActions);
        DgramMethods = GetMethodsData(dgramAction);
    }


    //Might be slow, don't use yet
    /// <summary>Recursive search for type's method, only way to get all methods in object (including private inherited methods)</summary>
    public static IEnumerable<System.Reflection.MethodInfo> GetMethods(System.Type type)
    {
        IEnumerable<System.Reflection.MethodInfo> methods = type.GetMethods(System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);

        if (type.BaseType != null)
            methods = methods.Concat(GetMethods(type.BaseType));

        return methods;
    }

    private List<MethodData> GetMethodsData(IEnumerable<System.Reflection.MethodInfo> methods)
    {
        List<MethodData> methodsData = new List<MethodData>();
        foreach (System.Reflection.MethodInfo item in methods)
            methodsData.Add(new MethodData(item, this));

        return methodsData;
    }

    /// <summary>
    /// This method makes sure the other threads that sends message isn't getting effected while it's active
    /// Things can break if two thread work on the same variable/method
    /// </summary>
    private void AddToAttributes()
    {
        bool isDone = false;
        while (!isDone)
        {
            if (!TypeSetter.isWorking)
            {
                TypeSetter.networkObjects.Add(this);
                isDone = true;
            }
        }
    }
    private void Add2Attributes()
    {
        TypeSetter.networkObjects.Add(this);
    }

    private void RemoveFromAttributes()
    {
        bool isDone = false;
        while (!isDone)
        {
            if (!TypeSetter.isWorking)
            {
                TypeSetter.networkObjects.Remove(this);
                isDone = true;
            }
            ServerMethods.Clear();
            ClientMethods.Clear();
            DgramMethods.Clear();
        }
    }

    private void Remove2Attributes()
    {
        int index = TypeSetter.networkObjects.IndexOf(this);
        if (index != -1) //Found
            TypeSetter.networkObjects[index] = null;

        ServerMethods.Clear();
        ClientMethods.Clear();
        DgramMethods.Clear();
    }

    public void Dispose() =>
        Remove2Attributes();//Task.Run(RemoveFromAttributes);

    public void InvokeNetworkMethods(MethodData method, object[] objetsToSend) =>
        method.Invoke(objetsToSend);
}
