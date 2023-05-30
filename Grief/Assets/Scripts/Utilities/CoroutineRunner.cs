using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class CoroutineRunner : Singleton<CoroutineRunner>
{
    private CoroutineContainer coroutineContainer;

    protected override void Awake()
    {
        base.Awake();

        coroutineContainer = transform.AddComponent<CoroutineContainer>();
    }

    public new Coroutine StartCoroutine(IEnumerator routine)
    {
        return coroutineContainer.StartCoroutine(routine);
    }

    public new void StopCoroutine(IEnumerator routine)
    {
        coroutineContainer.StopCoroutine(routine);
    }

    private class CoroutineContainer : MonoBehaviour { }
}