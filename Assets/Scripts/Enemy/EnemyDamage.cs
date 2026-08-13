using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyDamage : MonoBehaviour
{
    [SerializeField] private int damage = 10; // 不同敌人设不同值即可
    public int Damage => damage;
}

