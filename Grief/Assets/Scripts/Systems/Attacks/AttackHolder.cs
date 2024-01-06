using UnityEngine;

public class AttackHolder : MonoBehaviour
{
    private IAttack attacker;

    [SerializeField] private Attack[] attackList = new Attack[0];
    private Attack[] clonedAttackList;

    private Attack activeAttack;

    private bool canAttack = true;
    private int attackCount = 0;

    private void Awake()
    {
        attacker = GetComponent<IAttack>();

        if (attackList.Length == 0 || attacker == null)
        {
            canAttack = false;
            return;
        }

        attackCount = attackList.Length;
        clonedAttackList = new Attack[attackCount];

        for (int i = 0; i < attackCount; i++)
        {
            clonedAttackList[i] = attackList[i].Clone(null, attacker, transform);
        }

        activeAttack = clonedAttackList[0];
    }

    public void DestroyClones()
    {
        for (int i = 0; i < attackCount; i++)
        {
            clonedAttackList[i].DestroyClone();
        }

        canAttack = false;
    }

    public bool CanAttack()
    {
        return canAttack;
    }

    public void SetActiveAttack(int index)
    {
        if (!canAttack || index < 0 || index >= attackCount)
        {
            return;
        }

        activeAttack = clonedAttackList[index];
    }

    public void SetRandomActiveAttack()
    {
        if (!canAttack)
        {
            return;
        }

        int index = Random.Range(0, attackCount);
        activeAttack = clonedAttackList[index];
    }

    public void SetRandomActiveAttack(int rangeStartInclusive, int rangeEndExclusive)
    {
        if (rangeStartInclusive > rangeEndExclusive)
        {
            return;
        }

        int index = Random.Range(rangeStartInclusive, rangeEndExclusive);
        SetActiveAttack(index);
    }

    public string GetAttackId()
    {
        if (!canAttack)
        {
            return "";
        }

        return activeAttack.AttackId;
    }

    public Attack GetActiveAttack()
    {
        return activeAttack;
    }

    public ParticleSystem AddParticleEffect(GameObject particleEffectPrefab, Vector3 offset, float scale)
    {
        if (particleEffectPrefab == null)
        {
            return null;
        }

        GameObject effect = Instantiate(particleEffectPrefab, attacker.ParticleEffectHolder.transform);
        effect.transform.localPosition = offset;
        //effect.transform.localScale = transform.localScale * scale;

        ParticleSystem effectParticle = effect.GetComponent<ParticleSystem>();

        return effectParticle;
    }
}