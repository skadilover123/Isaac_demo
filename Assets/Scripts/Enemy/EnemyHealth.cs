using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    [SerializeField] private int maxHp = 10;
    private int hp;
    void Start() => hp = maxHp;

    public void TakeDamage(int amount)
    {
        hp -= amount;
        if (hp <= 0) { hp = 0; Destroy(gameObject); } // 死亡
    }
}

