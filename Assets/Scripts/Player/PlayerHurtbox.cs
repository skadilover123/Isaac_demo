using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHurtbox : MonoBehaviour
{
    private PlayerHealth health;

    void Awake() => health = GetComponent<PlayerHealth>();

    // 任意敌人进入触发圈就掉血；用 GetComponentInParent 兼容敌人碰撞体在子物体上的情况
    void OnTriggerStay2D(Collider2D other)
    {
        var enemy = other.GetComponentInParent<EnemyDamage>();
        if (enemy == null) return;

        if (health == null || health.IsInvulnerable) return;
        health.TakeDamage(enemy.Damage);

        var pKb = GetComponentInParent<Knockback>();
        var eKb = other.GetComponentInParent<Knockback>();

        Vector2 dir = ((Vector2)health.transform.position - (Vector2)other.transform.position).normalized;
        pKb?.Apply(dir);
        eKb?.Apply(-dir);
    }

}
