using UnityEngine;
using System.Collections.Generic;
using System;

public class UnityMainThreadDispatcher : MonoBehaviour
{
    private static readonly Queue<Action> _executionQueue = new Queue<Action>();
    private static UnityMainThreadDispatcher _instance;

    public static UnityMainThreadDispatcher Instance() {
        if (!_instance) throw new Exception("UnityMainThreadDispatcher not initialized. Add the AppManager prefab to the scene.");
        return _instance;
    }

    private void Awake() {
        if (_instance == null) {
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }

    private void Update() {
        lock (_executionQueue) while (_executionQueue.Count > 0) _executionQueue.Dequeue().Invoke();
    }

    public void Enqueue(Action action) {
        lock (_executionQueue) _executionQueue.Enqueue(action);
    }
}