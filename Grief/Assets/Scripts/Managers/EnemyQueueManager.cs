using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyQueueManager : Singleton<EnemyQueueManager>
{
    [SerializeField] private float maxQueueTime = 99;
    private float queueTimer = 0;

    private Enemy[] queuedEnemies = new Enemy[1];
    private List<Enemy> queueRequests = new();

    private void Start()
    {
        queueTimer = maxQueueTime;
    }

    private void FixedUpdate()
    {
        queueTimer -= Time.fixedDeltaTime;

        if (queueTimer <= 0)
        {
            queueTimer = maxQueueTime;
            CycleQueue();
        }
    }

    public void RequestToAttack(Enemy enemy)
    {
        if (queueRequests.Contains(enemy))
        {
            return;
        }

        if (queueRequests.Count == 0)
        {
            for (int i = 0; i < queuedEnemies.Length; i++)
            {
                if (queuedEnemies[i] == null)
                {
                    queuedEnemies[i] = enemy;
                    queuedEnemies[i].IsQueued = true;
                    queueTimer = maxQueueTime;
                    return;
                }
            }
        }

        queueRequests.Add(enemy);
    }

    public void RemoveFromQueue(Enemy enemy)
    {
        if (!queueRequests.Contains(enemy))
        {
            return;
        }

        queueRequests.Remove(enemy);
    }

    public void CycleOut(Enemy enemy)
    {
        int enemyIndex = -1;

        for (int i = 0; i < queuedEnemies.Length; i++)
        {
            if (enemy == queuedEnemies[i])
            {
                enemyIndex = i;
            }
        }

        if (enemyIndex == -1)
        {
            return;
        }

        queuedEnemies[enemyIndex].IsQueued = false;

        if (queueRequests.Count > 0)
        {
            queuedEnemies[enemyIndex] = queueRequests[0];
            queueRequests.RemoveAt(0);
            queuedEnemies[enemyIndex].IsQueued = true;
            queueTimer = maxQueueTime;
        }
        else
        {
            queuedEnemies[enemyIndex] = null;
        }

    }

    public void CycleQueue()
    {
        if (queueRequests.Count == 0)
        {
            return;
        }

        for (int i = 0; i < queuedEnemies.Length; i++)
        {
            if (queuedEnemies[i] != null)
            {
                queuedEnemies[i].IsQueued = false;
            }

            if (i < queueRequests.Count)
            {
                queuedEnemies[i] = queueRequests[0];
                queueRequests.RemoveAt(0);
                queuedEnemies[i].IsQueued = true;
            }
            else
            {
                queuedEnemies[i] = null;
            }
        }

        queueTimer = maxQueueTime;
    }
}
