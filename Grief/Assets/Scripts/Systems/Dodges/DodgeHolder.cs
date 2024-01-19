using UnityEngine;

public class DodgeHolder : MonoBehaviour
{
    private IDodge dodger;

    [SerializeField] private Dodge[] dodgeList = new Dodge[0];
    private Dodge[] clonedDodgeList = new Dodge[0];

    private Dodge activeDodge;

    private bool canDodge = true;
    private int dodgeCount = 0;

    private void Awake()
    {
        dodger = GetComponent<IDodge>();

        if (dodgeList.Length == 0 || dodger == null)
        {
            canDodge = false;
            return;
        }

        dodgeCount = dodgeList.Length;
        clonedDodgeList = new Dodge[dodgeCount];

        for (int i = 0; i < dodgeCount; i++)
        {
            clonedDodgeList[i] = dodgeList[i].Clone(null, dodger, transform);
        }

        activeDodge = clonedDodgeList[0];
    }

    private void FixedUpdate()
    {
        foreach (Dodge dodge in clonedDodgeList)
        {
            dodge.OnDodgeState();
        }
    }

    public void DestroyClones()
    {
        for (int i = 0; i < dodgeCount; i++)
        {
            clonedDodgeList[i].DestroyClone();
        }

        canDodge = false;
    }

    public bool CanDodge()
    {
        return canDodge;
    }

    public void SetActiveDodge(int index)
    {
        if (!canDodge || index < 0 || index >= dodgeCount)
        {
            return;
        }

        activeDodge = clonedDodgeList[index];
    }

    public void SetRandomActiveDodge()
    {
        if (!canDodge)
        {
            return;
        }

        int index = Random.Range(0, dodgeCount);
        activeDodge = clonedDodgeList[index];
    }

    public void SetRandomActiveDodge(int rangeStartInclusive, int rangeEndExclusive)
    {
        if (rangeStartInclusive > rangeEndExclusive)
        {
            return;
        }

        int index = Random.Range(rangeStartInclusive, rangeEndExclusive);
        SetActiveDodge(index);
    }

    public string GetDodgeId()
    {
        if (!canDodge)
        {
            return "";
        }

        return activeDodge.DodgeId;
    }

    public Dodge GetActiveDodge()
    {
        return activeDodge;
    }
}