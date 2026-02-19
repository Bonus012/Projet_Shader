using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[System.Serializable]
public struct Attack
{
    public AnimationClip Clip;
    public float Duration;
    public float Damage;
    public float BlendTime;
}

[System.Serializable]
public struct Combo
{
    public List<Attack> Attack_List;
    public float Time_BeforeReset;
}

[System.Serializable]
public struct Shield
{
    public List<Attack> Attack_List;
    public AnimationClip Block;
    public float BlockBlendTime;
}

[RequireComponent(typeof(PlayerInput))]
public class Attack_System : MonoBehaviour
{
    public Combo Combo;
    public Shield Shield;

    Attack currentAttackData;
    bool currentIsShieldAttack;

    public Material ShieldMaterial;
    [SerializeField] Animator animator;
    [SerializeField] GameObject Left_Weapon;
    [SerializeField] GameObject Right_Weapon;
    [SerializeField] string idleAnimationName = "EMPTY";
    [SerializeField] float idleBlendTime = 0.2f;
    [SerializeField] GameObject AttackTrail;

    PlayerInput playerInput;
    InputAction attackAction;
    InputAction blockAction;

    [SerializeField] GameObject AttackHitbox;

    int currentAttack = 0;
    bool isAttacking = false;
    bool isBlocking = false;
    bool nextAttackQueued = false;
    float attackTimer = 0f;
    float comboResetTimer = 0f;

    void Awake()
    {
        playerInput = GetComponent<PlayerInput>();
    }
    
    void OnEnable()
    {
        attackAction = playerInput.actions["Attack"];
        blockAction = playerInput.actions["Block"];
        attackAction?.Enable();
        blockAction?.Enable();
    }
    void OnDisable()
    {
        attackAction?.Disable();
        blockAction?.Disable();
    }

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        // Gestion du blocage
        bool blockPressed = blockAction != null && blockAction.IsPressed();

        if (blockPressed && !isBlocking && !isAttacking)
        {
            StartBlock();
        }
        else if (!blockPressed && isBlocking && !isAttacking)
        {
            EndBlock();
        }

        // Gestion des attaques
        if (attackAction != null && attackAction.triggered)
        {
            if (blockPressed)
            {
                if (!isAttacking)
                    StartShieldAttack();
                else if (Shield.Attack_List.Count > 1)
                    nextAttackQueued = true;
            }
            else if (!isAttacking)
            {
                StartAttack();
            }
            else
            {
                QueueNextAttack();
            }
        }

        if (isAttacking)
            UpdateAttack();
        else if (!isBlocking)
            UpdateComboReset();
    }

    void StartBlock()
    {
        isBlocking = true;
        comboResetTimer = 0f;
        currentAttack = 0;
        ShieldMaterial.SetFloat("_Strength", 4f);
        if (Shield.Block != null)
        {
            animator.CrossFade(Shield.Block.name, Shield.BlockBlendTime);
        }
    }

    void EndBlock()
    {
        ShieldMaterial.SetFloat("_Strength", 0f);
        isBlocking = false;
        animator.CrossFade(idleAnimationName, idleBlendTime);
    }

    void StartShieldAttack()
    {
        if (Shield.Attack_List == null || Shield.Attack_List.Count == 0)
            return;

        int randomIndex = Random.Range(0, Shield.Attack_List.Count);
        currentAttackData = Shield.Attack_List[randomIndex];
        currentIsShieldAttack = true;

        isAttacking = true;
        attackTimer = 0f;

        animator.CrossFade(currentAttackData.Clip.name, currentAttackData.BlendTime);
    }

    void StartAttack()
    {
        isAttacking = true;
        attackTimer = 0f;
        comboResetTimer = 0f;

        currentAttackData = Combo.Attack_List[currentAttack];
        currentIsShieldAttack = false;
        Hascallcoroutine = false;

        AttackTrail?.SetActive(true);
        animator.CrossFade(currentAttackData.Clip.name, currentAttackData.BlendTime);
    }

    void QueueNextAttack()
    {
        nextAttackQueued = true;
    }

    bool Hascallcoroutine;
    void UpdateAttack()
    {
        attackTimer += Time.deltaTime;

        if (nextAttackQueued &&
            attackTimer >= currentAttackData.Duration * 0.6f &&
            attackTimer < currentAttackData.Duration * 0.9f)
        {
            nextAttackQueued = false;

            if (currentIsShieldAttack)
                NextShieldAttack();
            else
                NextAttack();

            return;
        }

        if (attackTimer >= currentAttackData.Duration * 0.5f && !Hascallcoroutine)
        {
            Hascallcoroutine = true;
            StartCoroutine(ActivateHitboxBriefly());
        }

        if (attackTimer >= currentAttackData.Duration * 1.1f)
        {
            if (currentIsShieldAttack)
                EndShieldAttack();
            else
                EndAttack();
            return;
        }

        if (attackTimer >= currentAttackData.Duration)
        {
            if (currentIsShieldAttack)
                EndShieldAttack();
            else
                EndAttack();
        }
    }

    void NextShieldAttack()
    {
        if (Shield.Attack_List.Count == 1)
        {
            return;
        }
        attackTimer = 0f;
        int randomIndex = Random.Range(0, Shield.Attack_List.Count);
        currentAttackData = Shield.Attack_List[randomIndex];
        currentIsShieldAttack = true;

        animator.CrossFade(currentAttackData.Clip.name, currentAttackData.BlendTime);
    }

    void NextAttack()
    {
        attackTimer = 0f;
        currentAttack++;
        if (currentAttack >= Combo.Attack_List.Count)
            currentAttack = 0;

        currentAttackData = Combo.Attack_List[currentAttack];
        currentIsShieldAttack = false;
        Hascallcoroutine = false;
        animator.CrossFade(currentAttackData.Clip.name, currentAttackData.BlendTime);
    }

    void EndShieldAttack()
    {
        isAttacking = false;
        nextAttackQueued = false;

        if (blockAction != null && blockAction.IsPressed())
        {
            StartBlock();
        }
        else
        {
            animator.CrossFade(idleAnimationName, idleBlendTime);
        }

        StartCoroutine(ActivateHitboxBriefly());
    }

    void EndAttack()
    {
        isAttacking = false;
        nextAttackQueued = false;
        comboResetTimer = 0f;
        currentAttack = 0;

        AttackTrail?.SetActive(false);
        animator.CrossFade(idleAnimationName, idleBlendTime);
    }

    void UpdateComboReset()
    {
        if (currentAttack > 0)
        {
            comboResetTimer += Time.deltaTime;
            float resetThreshold = Combo.Time_BeforeReset * 0.9f;
            if (comboResetTimer >= resetThreshold)
            {
                currentAttack = 0;
                comboResetTimer = 0f;
            }
        }
    }
    IEnumerator ActivateHitboxBriefly()
    {
        AttackZone atk = AttackHitbox.GetComponent<AttackZone>();
        atk.IsInShieldMode = isBlocking;
        AttackHitbox.SetActive(true);
        yield return new WaitForSeconds(0.1f);
        AttackHitbox.SetActive(false);
    }

    void EnableWeapons(bool state)
    {
        Left_Weapon?.SetActive(state);
        Right_Weapon?.SetActive(state);
    }
}