using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    public bool IsInvulnerable => invulnTimer > 0;
    public bool IsDead => isDead;

    [SerializeField] private int maxHp = 6;
    [SerializeField] private float invulnTime = 1f;
    [SerializeField] private Transform head;
    [SerializeField] private Transform body;
    [SerializeField] private Transform action;
    [SerializeField] private Animator deadanim;
    [SerializeField] private Animator hurtanim;
    [SerializeField] private Collider2D playerCollider;
    
    [SerializeField] private float hurtAnimLength = 0.5f;  // 调成你 Hurt clip 的实际时长
    private float hurtTimer;
    private AudioSource hurtAudio, deathAudio;
    private Rigidbody2D rb;
    private int hp;
    private float invulnTimer;
    private bool isDead;
    private bool isHurt;
    private string lastState;

    void Awake()
    {
        hp = maxHp;
        var aus = GetComponents<AudioSource>();
        hurtAudio = aus.Length > 0 ? aus[0] : null;
        deathAudio = aus.Length > 1 ? aus[1] : null;
        rb = GetComponent<Rigidbody2D>();
        playerCollider = GetComponent<Collider2D>();
    }

    void Update()
    {
        if (invulnTimer > 0) invulnTimer -= Time.deltaTime;
        if (hurtTimer > 0)
        {
            hurtTimer -= Time.deltaTime;
            if (hurtTimer <= 0) EndHurt();   // 受伤动画播完 → 切回 head/body
        }
    }

    public void TakeDamage(int amount)
    {
        if (isDead || invulnTimer > 0) return;
        hp -= amount;
        if (hp > 0)
        {
            hurtAudio?.Play();
            invulnTimer = invulnTime;
            StartHurt();
        } else { 
            hp = 0; 
            Die(); 
        }
    }
    private void StartHurt()
    {
        if (hurtanim == null && action != null) hurtanim = action.GetComponent<Animator>();
        hurtanim?.SetBool("isHurt", true);
        if (head != null) head.gameObject.SetActive(false);
        if (body != null) body.gameObject.SetActive(false);
        if (action != null) action.gameObject.SetActive(true);
        hurtTimer = hurtAnimLength;
    }


    private void EndHurt()
    {
        hurtanim?.SetBool("isHurt", false);
        if (head != null) head.gameObject.SetActive(true);     // 恢复 head/body 显示
        if (body != null) body.gameObject.SetActive(true);
        if (action != null) action.gameObject.SetActive(false);
    }

    private void Die()
    {
        if (isDead) return;
        isDead = true;
        hurtTimer = 0;

        // ① 先停逻辑/物理（最关键，动画即便出问题也得先让人停下）
        if (rb != null) rb.velocity = Vector2.zero;
        var move = GetComponent<PlayerMove>();   if (move != null) move.enabled = false;
        var atk  = GetComponent<PlayerAttack>(); if (atk  != null) atk.enabled  = false;
        var kb   = GetComponent<Knockback>();    if (kb   != null) kb.enabled  = false; // 避免死亡瞬间被弹飞

        // ② 隐藏头/身、显示整体精灵
        if (head != null) head.gameObject.SetActive(false);
        if (body != null) body.gameObject.SetActive(false);
        if (action != null) action.gameObject.SetActive(true);

        // ③ 触发死亡动画（deadanim 没拖就自动从 action 取，并用 ?. 防崩溃）
        if (deadanim == null && action != null) deadanim = action.GetComponent<Animator>();
        if (deadanim != null)
        {
            deadanim.SetBool("isHurt", false);
            deadanim.SetBool("isDead", true);
        }
        else Debug.LogError("[Die] deadanim 仍未赋值！请把 action 物体上的 Animator 拖给 deadanim/hurtanim 字段");

        deathAudio?.Play();
    }

}
