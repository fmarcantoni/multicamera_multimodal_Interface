using System;
using System.Collections.Generic;
using UnityEngine;

public class UnityMainThreadDispatcher : MonoBehaviour
{
    private static readonly Queue<Action> _executionQueue = new();

    public void Update()
    {
        while (_executionQueue.Count > 0)
        {
            _executionQueue.Dequeue().Invoke();
        }
    }

    public void Enqueue(Action action)
    {
        lock (_executionQueue)
        {
            _executionQueue.Enqueue(action);
        }
    }

    private static UnityMainThreadDispatcher _instance;

    public static UnityMainThreadDispatcher Instance()
    {
        if (_instance == null)
        {
            _instance = new GameObject("MainThreadDispatcher").AddComponent<UnityMainThreadDispatcher>();
            DontDestroyOnLoad(_instance.gameObject);
        }
        return _instance;
    }
}