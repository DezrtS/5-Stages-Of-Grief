using UnityEngine;

public class DestroyAfter : MonoBehaviour
{
    [SerializeField] private float destroyAfter = 1;

    private void Start()
    {
        Destroy(gameObject, destroyAfter);
    }
}
