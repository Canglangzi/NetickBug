using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;
using Sirenix.Serialization;


namespace CockleBurs.GameFramework.Core
{
// 技能动作接口 - 负责技能执行时的具体行为
public interface ISkillAction
{
    string ActionName { get; }
    
    // 准备阶段（可选）
    UniTask Prepare(GameObject owner, SkillContext context, CancellationToken cancellationToken = default);
    
    // 执行动作
    UniTask Execute(GameObject owner, SkillContext context, CancellationToken cancellationToken = default);
    
    // 结束/取消动作（可选）
    UniTask Finish(GameObject owner, SkillContext context, bool interrupted, CancellationToken cancellationToken = default);
    
    // 更新动作（用于持续型动作）
    void UpdateAction(float deltaTime, GameObject owner, SkillContext context);
    
    // 检查动作是否完成
    bool IsCompleted();
}

// 技能上下文，用于在动作间传递数据
[Serializable]
public class SkillContext
{
    public Dictionary<string, object> Data = new Dictionary<string, object>();
    public GameObject Target;
    public Vector3 TargetPosition;
    public float StartTime;
    
    public T GetData<T>(string key, T defaultValue = default)
    {
        if (Data.TryGetValue(key, out object value) && value is T typedValue)
        {
            return typedValue;
        }
        return defaultValue;
    }
    
    public void SetData(string key, object value)
    {
        Data[key] = value;
    }
}

// 技能基础接口
public interface ISkill
{
    string SkillName { get; }
    string Description { get; }
    float Cooldown { get; }
    bool IsReady { get; }
    float CooldownTimer { get; } // 添加 CooldownTimer 属性
    
    UniTask Initialize(GameObject owner, CancellationToken cancellationToken = default);
    UniTask<bool> CanExecute(CancellationToken cancellationToken = default);
    UniTask Execute(CancellationToken cancellationToken = default);
    void UpdateCooldown(float deltaTime);
}

// 技能触发接口
public interface ISkillTrigger
{
    UniTask<bool> ShouldTrigger(GameObject owner, CancellationToken cancellationToken = default);
    UniTask OnTriggerActivated(CancellationToken cancellationToken = default);
    UniTask OnTriggerDeactivated(CancellationToken cancellationToken = default);
}

// 技能效果接口
public interface ISkillEffect
{
    UniTask ApplyEffect(GameObject target, CancellationToken cancellationToken = default);
    UniTask RemoveEffect(GameObject target, CancellationToken cancellationToken = default);
}

// 基础技能抽象类
[Serializable]
public abstract class SkillBase : ISkill
{
    [BoxGroup("基础信息")]
    public string skillName = "新技能";
    
    [BoxGroup("基础信息")]
    public string description = "技能描述";
    
    [BoxGroup("冷却设置")]
    public float cooldown = 5f;
    
    [BoxGroup("冷却设置"), ShowInInspector, ReadOnly]
    public float CooldownTimer { get; protected set; } // 修复：添加 public 访问修饰符
    
    protected GameObject owner;
    protected CancellationTokenSource skillCancellationTokenSource;
    
    public string SkillName => skillName;
    public string Description => description;
    public float Cooldown => cooldown;
    
    [ShowInInspector, ReadOnly]
    public bool IsReady => CooldownTimer <= 0;
    
    // 动作序列相关字段
    [BoxGroup("动作序列"), ListDrawerSettings(Expanded = true)]
    [OdinSerialize] public List<ISkillAction> SkillActions = new List<ISkillAction>();
    
    [BoxGroup("动作序列"), ShowInInspector, ReadOnly]
    protected int currentActionIndex = -1;
    
    [BoxGroup("动作序列"), ShowInInspector, ReadOnly]
    protected SkillContext currentContext;
    
    [ShowInInspector, ReadOnly]
    public bool IsExecuting => currentActionIndex >= 0;
    
    public virtual async UniTask Initialize(GameObject owner, CancellationToken cancellationToken = default)
    {
        this.owner = owner;
        CooldownTimer = 0f;
        
        // 初始化所有动作
        foreach (var action in SkillActions)
        {
            await action.Prepare(owner, currentContext, cancellationToken);
        }
    }
    
    public virtual async UniTask<bool> CanExecute(CancellationToken cancellationToken = default)
    {
        return IsReady;
    }
    
    public virtual async UniTask Execute(CancellationToken cancellationToken = default)
    {
        // 取消之前的技能执行（如果有）
        skillCancellationTokenSource?.Cancel();
        skillCancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        
        // 创建技能上下文
        currentContext = new SkillContext
        {
            StartTime = Time.time,
            Target = owner, // 默认目标是自身
            TargetPosition = owner.transform.position
        };
        
        ExecuteSkillActions(skillCancellationTokenSource.Token).Forget();
        
        // 开始冷却
        StartCooldown();
        
        Debug.Log($"{skillName} 开始执行!");
    }
    
    // 使用UniTask执行技能动作序列
    private async UniTaskVoid ExecuteSkillActions(CancellationToken cancellationToken) // 修复：使用 UniTaskVoid 而不是 void
    {
        currentActionIndex = 0;
        
        try
        {
            while (currentActionIndex < SkillActions.Count && !cancellationToken.IsCancellationRequested)
            {
                var currentAction = SkillActions[currentActionIndex];
                
                // 执行当前动作
                await currentAction.Execute(owner, currentContext, cancellationToken);
                
                // 等待动作完成
                await WaitForActionCompletion(currentAction, cancellationToken);
                
                // 完成当前动作
                await currentAction.Finish(owner, currentContext, false, cancellationToken);
                
                // 移动到下一个动作
                currentActionIndex++;
            }
            
            if (!cancellationToken.IsCancellationRequested)
            {
                Debug.Log($"{skillName} 执行完成!");
            }
        }
        catch (OperationCanceledException)
        {
            // 技能被取消
            if (currentActionIndex >= 0 && currentActionIndex < SkillActions.Count)
            {
                await SkillActions[currentActionIndex].Finish(owner, currentContext, true, cancellationToken);
            }
            Debug.Log($"{skillName} 被取消!");
        }
        finally
        {
            currentActionIndex = -1;
            skillCancellationTokenSource?.Dispose();
            skillCancellationTokenSource = null;
        }
    }
    
    // 等待动作完成
    private async UniTask WaitForActionCompletion(ISkillAction action, CancellationToken cancellationToken)
    {
        while (!action.IsCompleted() && !cancellationToken.IsCancellationRequested)
        {
            await UniTask.Yield(PlayerLoopTiming.Update, cancellationToken);
        }
    }
    
    // 中断技能
    public void InterruptSkill()
    {
        skillCancellationTokenSource?.Cancel();
    }
    
    public virtual void UpdateCooldown(float deltaTime)
    {
        if (CooldownTimer > 0)
        {
            CooldownTimer -= deltaTime;
        }
    }
    
    protected void StartCooldown()
    {
        CooldownTimer = cooldown;
    }
}

// 基础动作类
[Serializable]
public abstract class SkillActionBase : ISkillAction
{
    [BoxGroup("基础设置")]
    public string actionName = "新动作";
    
    [BoxGroup("基础设置"), TextArea]
    public string description;
    
    [BoxGroup("时间设置")]
    public float duration = 1f;
    
    protected float elapsedTime;
    protected bool isCompleted;
    
    public string ActionName => actionName;
    
    public virtual UniTask Prepare(GameObject owner, SkillContext context, CancellationToken cancellationToken = default)
    {
        elapsedTime = 0f;
        isCompleted = false;
        return UniTask.CompletedTask;
    }
    
    public virtual UniTask Execute(GameObject owner, SkillContext context, CancellationToken cancellationToken = default)
    {
        return UniTask.CompletedTask;
    }
    
    public virtual UniTask Finish(GameObject owner, SkillContext context, bool interrupted, CancellationToken cancellationToken = default)
    {
        return UniTask.CompletedTask;
    }
    
    public virtual void UpdateAction(float deltaTime, GameObject owner, SkillContext context)
    {
        elapsedTime += deltaTime;
        if (elapsedTime >= duration)
        {
            isCompleted = true;
        }
    }
    
    public virtual bool IsCompleted()
    {
        return isCompleted;
    }
}

// 播放动画动作
[Serializable]
public class PlayAnimationAction : SkillActionBase
{
    [BoxGroup("动画设置")]
    public string animationName = "Attack";
    
    [BoxGroup("动画设置")]
    public float crossFadeTime = 0.1f;
    
    [BoxGroup("动画设置")]
    public bool waitForCompletion = true;
    
    private Animator animator;
    
    public override async UniTask Prepare(GameObject owner, SkillContext context, CancellationToken cancellationToken = default)
    {
        await base.Prepare(owner, context, cancellationToken);
        animator = owner.GetComponent<Animator>();
    }
    
    public override async UniTask Execute(GameObject owner, SkillContext context, CancellationToken cancellationToken = default)
    {
        if (animator != null)
        {
            animator.CrossFade(animationName, crossFadeTime);
            
            if (waitForCompletion)
            {
                // 等待动画完成
                await UniTask.WaitUntil(() => 
                    IsAnimationDone(animator, animationName) || cancellationToken.IsCancellationRequested, 
                    cancellationToken: cancellationToken
                );
            }
        }
    }
    
    private bool IsAnimationDone(Animator animator, string animationName)
    {
        var state = animator.GetCurrentAnimatorStateInfo(0);
        return state.IsName(animationName) && state.normalizedTime >= 0.95f;
    }
}

// 移动动作
[Serializable]
public class MoveToAction : SkillActionBase
{
    [BoxGroup("移动设置")]
    public float moveSpeed = 5f;
    
    [BoxGroup("移动设置")]
    public string targetPositionKey = "TargetPosition";
    
    [BoxGroup("移动设置")]
    public AnimationCurve speedCurve = AnimationCurve.Linear(0, 0, 1, 1);
    
    private Vector3 startPosition;
    private Vector3 targetPosition;
    
    public override async UniTask Prepare(GameObject owner, SkillContext context, CancellationToken cancellationToken = default)
    {
        await base.Prepare(owner, context, cancellationToken);
        startPosition = owner.transform.position;
        
        // 从上下文中获取目标位置
        targetPosition = context.GetData<Vector3>(targetPositionKey, owner.transform.position + owner.transform.forward * 5f);
    }
    
    public override async UniTask Execute(GameObject owner, SkillContext context, CancellationToken cancellationToken = default)
    {
        // 移动逻辑在Update中处理
        await UniTask.WaitUntil(() => isCompleted || cancellationToken.IsCancellationRequested, 
            cancellationToken: cancellationToken);
    }
    
    public override void UpdateAction(float deltaTime, GameObject owner, SkillContext context)
    {
        base.UpdateAction(deltaTime, owner, context);
        
        // 计算移动进度
        float progress = Mathf.Clamp01(elapsedTime / duration);
        float curvedProgress = speedCurve.Evaluate(progress);
        
        // 移动角色
        owner.transform.position = Vector3.Lerp(startPosition, targetPosition, curvedProgress);
    }
}

// 发射投射物动作
[Serializable]
public class ProjectileAction : SkillActionBase
{
    [BoxGroup("投射物设置")]
    public GameObject projectilePrefab;
    
    [BoxGroup("投射物设置")]
    public float projectileSpeed = 10f;
    
    [BoxGroup("投射物设置")]
    public string targetKey = "Target";
    
    [BoxGroup("伤害设置")]
    public float damage = 30f;
    
    [BoxGroup("投射物设置")]
    public bool waitForHit = false;
    
    public override async UniTask Execute(GameObject owner, SkillContext context, CancellationToken cancellationToken = default)
    {
        if (projectilePrefab == null) return;
        
        // 获取目标
        GameObject target = context.GetData<GameObject>(targetKey, null);
        Vector3 targetPosition = target != null ? target.transform.position : context.TargetPosition;
        
        // 计算方向
        Vector3 direction = (targetPosition - owner.transform.position).normalized;
        
        // 创建投射物
        GameObject projectile = GameObject.Instantiate(
            projectilePrefab, 
            owner.transform.position + direction, 
            Quaternion.LookRotation(direction)
        );
        
        // 设置投射物属性
        Projectile projComponent = projectile.GetComponent<Projectile>();
        if (projComponent == null)
        {
            projComponent = projectile.AddComponent<Projectile>();
        }
        
        projComponent.Damage = damage;
        projComponent.Owner = owner;
        projComponent.Speed = projectileSpeed;
        projComponent.Direction = direction;
        
        if (waitForHit)
        {
            // 等待投射物命中或超时
            var timeoutTask = UniTask.Delay(TimeSpan.FromSeconds(10), cancellationToken: cancellationToken);
            var hitTask = projComponent.WaitForHit(cancellationToken);
            
            await UniTask.WhenAny(hitTask, timeoutTask);
        }
        
        // 立即完成
        isCompleted = true;
    }
}

// 投射物组件（增强版）
public class Projectile : MonoBehaviour
{
    public float Damage;
    public GameObject Owner;
    public float Speed;
    public Vector3 Direction;
    
    private UniTaskCompletionSource<bool> hitCompletionSource;
    
    void Update()
    {
        transform.position += Direction * Speed * Time.deltaTime;
    }
    
    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject != Owner)
        {
            Health health = other.GetComponent<Health>();
            if (health != null)
            {
                health.TakeDamage(Damage, Owner);
            }
            
            hitCompletionSource?.TrySetResult(true);
            Destroy(gameObject);
        }
    }
    
    public UniTask WaitForHit(CancellationToken cancellationToken = default)
    {
        hitCompletionSource = new UniTaskCompletionSource<bool>();
        cancellationToken.Register(() => hitCompletionSource?.TrySetCanceled());
        return hitCompletionSource.Task;
    }
}

// 应用效果动作
[Serializable]
public class ApplyEffectAction : SkillActionBase
{
    [BoxGroup("效果设置")]
    public GameObject effectPrefab;
    
    [BoxGroup("效果设置")]
    public string targetKey = "Target";
    
    [BoxGroup("效果设置")]
    public bool attachToTarget = true;
    
    [BoxGroup("效果设置")]
    public float effectDuration = 3f;
    
    [BoxGroup("效果设置")]
    public bool waitForEffectCompletion = false;
    
    public override async UniTask Execute(GameObject owner, SkillContext context, CancellationToken cancellationToken = default)
    {
        if (effectPrefab == null) return;
        
        // 获取目标
        GameObject target = context.GetData<GameObject>(targetKey, owner);
        
        // 创建效果
        GameObject effect = GameObject.Instantiate(effectPrefab);
        
        if (attachToTarget && target != null)
        {
            effect.transform.SetParent(target.transform);
            effect.transform.localPosition = Vector3.zero;
        }
        else
        {
            effect.transform.position = target != null ? target.transform.position : owner.transform.position;
        }
        
        if (waitForEffectCompletion && effectDuration > 0)
        {
            // 等待效果结束
            await UniTask.Delay(TimeSpan.FromSeconds(effectDuration), cancellationToken: cancellationToken);
        }
        else
        {
            // 设置自动销毁
            if (effectDuration > 0)
            {
                GameObject.Destroy(effect, effectDuration);
            }
        }
        
        // 立即完成
        isCompleted = true;
    }
}

// 等待动作
[Serializable]
public class WaitAction : SkillActionBase
{
    // 只需要基础的Update逻辑，等待指定时间
}

// 按键触发
[Serializable]
public class KeyPressTrigger : ISkillTrigger
{
    [BoxGroup("按键设置")]
    public KeyCode triggerKey = KeyCode.Space;
    
    [BoxGroup("触发模式")]
    public TriggerMode mode = TriggerMode.OnPress;
    
    private bool wasPressed = false;
    
    public enum TriggerMode
    {
        OnPress,
        OnRelease,
        WhilePressed
    }
    
    public async UniTask<bool> ShouldTrigger(GameObject owner, CancellationToken cancellationToken = default)
    {
        bool isPressed = Input.GetKey(triggerKey);
        
        switch (mode)
        {
            case TriggerMode.OnPress:
                if (isPressed && !wasPressed)
                {
                    wasPressed = true;
                    return true;
                }
                break;
                
            case TriggerMode.OnRelease:
                if (!isPressed && wasPressed)
                {
                    wasPressed = false;
                    return true;
                }
                break;
                
            case TriggerMode.WhilePressed:
                wasPressed = isPressed;
                return isPressed;
        }
        
        wasPressed = isPressed;
        
        // 等待一帧以避免过度检查
        await UniTask.Yield(PlayerLoopTiming.Update, cancellationToken);
        return false;
    }
    
    public UniTask OnTriggerActivated(CancellationToken cancellationToken = default)
    {
        // 触发器激活时的逻辑
        Debug.Log($"技能触发器已激活，使用按键: {triggerKey}");
        return UniTask.CompletedTask;
    }
    
    public UniTask OnTriggerDeactivated(CancellationToken cancellationToken = default)
    {
        // 触发器禁用时的逻辑
        Debug.Log($"技能触发器已禁用，使用按键: {triggerKey}");
        return UniTask.CompletedTask;
    }
}

// 角色脚本与技能管理
public class Role : MonoBehaviour
{
    [Title("技能配置")]
    [SerializeField, TableList(ShowIndexLabels = true)]
    private List<SkillSlot> skillSlots = new List<SkillSlot>();
    
    [Title("当前执行的技能")]
    [ShowInInspector, ReadOnly]
    private SkillBase currentlyExecutingSkill;
    
    [Title("技能冷却状态")]
    [ShowInInspector, ProgressBar(0, 1)]
    private Dictionary<string, float> skillCooldowns = new Dictionary<string, float>();
    
    private CancellationTokenSource roleCancellationTokenSource;
    
    void Start()
    {
        roleCancellationTokenSource = new CancellationTokenSource();
        InitializeSkills().Forget();
    }
    
    void OnDestroy()
    {
        roleCancellationTokenSource?.Cancel();
        roleCancellationTokenSource?.Dispose();
    }
    
    void Update()
    {
        UpdateSkillCooldowns(Time.deltaTime);
        CheckSkillTriggers().Forget();
    }
    
    private async UniTaskVoid InitializeSkills() // 修复：使用 UniTaskVoid 而不是 void
    {
        foreach (var skillSlot in skillSlots)
        {
            if (skillSlot.Skill != null)
            {
                await skillSlot.Skill.Initialize(gameObject, roleCancellationTokenSource.Token);
            }
        }
    }
    
    private void UpdateSkillCooldowns(float deltaTime)
    {
        foreach (var skillSlot in skillSlots)
        {
            if (skillSlot.Skill != null)
            {
                skillSlot.Skill.UpdateCooldown(deltaTime);
                
                // 更新冷却UI显示
                if (skillCooldowns.ContainsKey(skillSlot.Skill.SkillName))
                {
                    // 修复：使用 CooldownTimer 属性而不是字段
                    skillCooldowns[skillSlot.Skill.SkillName] = 
                        1 - (skillSlot.Skill.CooldownTimer / skillSlot.Skill.Cooldown);
                }
            }
        }
    }
    
    private async UniTaskVoid CheckSkillTriggers() // 修复：使用 UniTaskVoid 而不是 void
    {
        foreach (var skillSlot in skillSlots)
        {
            if (skillSlot.Skill != null && skillSlot.Trigger != null)
            {
                if (await skillSlot.Trigger.ShouldTrigger(this.gameObject, roleCancellationTokenSource.Token) && 
                    await skillSlot.Skill.CanExecute(roleCancellationTokenSource.Token))
                {
                    await skillSlot.Skill.Execute(roleCancellationTokenSource.Token);
                    currentlyExecutingSkill = skillSlot.Skill as SkillBase;
                }
            }
        }
    }
    
    // 添加技能到指定槽位
    public async UniTask AssignSkillToSlot(int slotIndex, ISkill skill, ISkillTrigger trigger)
    {
        if (slotIndex >= 0 && slotIndex < skillSlots.Count)
        {
            skillSlots[slotIndex].Skill = skill;
            skillSlots[slotIndex].Trigger = trigger;
            
            await skill.Initialize(this.gameObject, roleCancellationTokenSource.Token);
            await trigger.OnTriggerActivated(roleCancellationTokenSource.Token);
            
            // 初始化冷却显示
            if (!skillCooldowns.ContainsKey(skill.SkillName))
            {
                skillCooldowns.Add(skill.SkillName, 1f);
            }
        }
    }
    
    // 中断当前技能
    public void InterruptCurrentSkill()
    {
        if (currentlyExecutingSkill != null)
        {
            currentlyExecutingSkill.InterruptSkill();
            currentlyExecutingSkill = null;
        }
    }
    
    // Odin序列化的技能槽位类
    [System.Serializable]
    public class SkillSlot
    {
        [TableColumnWidth(50)]
        public int SlotIndex;
        
        [OdinSerialize, TableColumnWidth(200)]
        public ISkill Skill;
        
        [OdinSerialize, TableColumnWidth(200)]
        public ISkillTrigger Trigger;
        
        [TextArea, TableColumnWidth(300)]
        public string Notes;
    }
}

// 健康组件
public class Health : MonoBehaviour
{
    public float currentHealth = 100f;
    public float maxHealth = 100f;
    
    public event Action<float, GameObject> OnDamageTaken;
    public event Action<float> OnHealed;
    public event Action OnDeath;
    
    public void TakeDamage(float damage, GameObject damageSource)
    {
        currentHealth -= damage;
        OnDamageTaken?.Invoke(damage, damageSource);
        
        if (currentHealth <= 0)
        {
            Die();
        }
    }
    
    public void Heal(float amount)
    {
        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
        OnHealed?.Invoke(amount);
    }
    
    public bool IsAlly(GameObject other)
    {
        // 简单的盟友判断逻辑
        return gameObject.CompareTag(other.tag);
    }
    
    private void Die()
    {
        Debug.Log($"{gameObject.name} 死亡!");
        OnDeath?.Invoke();
        Destroy(gameObject);
    }
    
    public UniTask WaitForDeath(CancellationToken cancellationToken = default)
    {
        var completionSource = new UniTaskCompletionSource<bool>();
        cancellationToken.Register(() => completionSource.TrySetCanceled());
        
        OnDeath += () => completionSource.TrySetResult(true);
        
        return completionSource.Task;
    }
}
}