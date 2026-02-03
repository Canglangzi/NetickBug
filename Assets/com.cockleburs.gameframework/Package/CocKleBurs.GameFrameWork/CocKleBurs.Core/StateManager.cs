using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;


namespace CockleBurs.GameFramework.Core
{
public class StateManager : MonoBehaviour
{
    [Header("Configuration")]
    [SerializeField] private StateConfiguration config;
    
    [Header("Debug")]
    [SerializeField] private bool visualizeStates = true;

    // 状态存储
    private Dictionary<string, bool> baseStates = new Dictionary<string, bool>();
    private Dictionary<string, bool> finalStates = new Dictionary<string, bool>();
    private Dictionary<string, bool> previousStates = new Dictionary<string, bool>();
    
    // 状态值存储（用于数值型状态）
    private Dictionary<string, float> stateValues = new Dictionary<string, float>();
    
    // 状态历史（用于恢复）
    private Dictionary<string, bool> preModificationStates = new Dictionary<string, bool>();
    
    // 事件系统
    public event System.Action<string, bool, bool> OnStateChanged; // stateName, newState, isBaseState

    private void Awake()
    {
        InitializeStates();
    }

    private void InitializeStates()
    {
        foreach (var stateDef in config.stateDefinitions)
        {
            baseStates[stateDef.stateName] = false;
            finalStates[stateDef.stateName] = false;
            previousStates[stateDef.stateName] = false;
        }
    }

    private void Update()
    {
        // 1. 更新基础状态
        UpdateBaseStates();
        
        // 2. 复制基础状态作为修改起点
        foreach (var state in baseStates)
        {
            finalStates[state.Key] = state.Value;
        }
        
        // 3. 应用状态修改器
        ApplyStateModifiers();
        
        // 4. 检测状态变化并触发事件
        CheckStateTransitions();
    }

    private void UpdateBaseStates()
    {
        foreach (var stateDef in config.stateDefinitions)
        {
            if (stateDef.inputAction == null) continue;
            
            bool newState = false;
            InputAction action = stateDef.inputAction.action;
            
            switch (stateDef.activationType)
            {
                case StateConfiguration.ActivationType.Hold:
                    newState = action.IsPressed();
                    break;
                    
                case StateConfiguration.ActivationType.Toggle:
                    if (action.WasPressedThisFrame())
                    {
                        newState = !baseStates[stateDef.stateName];
                    }
                    else
                    {
                        newState = baseStates[stateDef.stateName];
                    }
                    break;
                    
                case StateConfiguration.ActivationType.Trigger:
                    newState = action.WasPressedThisFrame();
                    break;
            }
            
            // 更新基础状态
            if (baseStates[stateDef.stateName] != newState)
            {
                baseStates[stateDef.stateName] = newState;
                OnStateChanged?.Invoke(stateDef.stateName, newState, true);
            }
        }
    }

    private void ApplyStateModifiers()
    {
        foreach (var modifier in config.stateModifiers)
        {
            // 检查修改器是否应该激活
            bool shouldApply = true;
            foreach (var requiredState in modifier.requiredStates)
            {
                if (!finalStates.ContainsKey(requiredState) || !finalStates[requiredState])
                {
                    shouldApply = false;
                    break;
                }
            }
            
            if (!shouldApply)
            {
                // 如果修改器不再生效且有恢复标记，恢复之前状态
                if (modifier.restoreOnExit && 
                    preModificationStates.ContainsKey(modifier.targetState))
                {
                    finalStates[modifier.targetState] = preModificationStates[modifier.targetState];
                    preModificationStates.Remove(modifier.targetState);
                }
                continue;
            }
            
            // 保存修改前的状态（如果尚未保存）
            if (!preModificationStates.ContainsKey(modifier.targetState))
            {
                preModificationStates[modifier.targetState] = finalStates[modifier.targetState];
            }
            
            // 应用修改
            switch (modifier.modificationType)
            {
                case StateConfiguration.ModificationType.ForceEnable:
                    finalStates[modifier.targetState] = true;
                    break;
                    
                case StateConfiguration.ModificationType.ForceDisable:
                    finalStates[modifier.targetState] = false;
                    break;
                    
                case StateConfiguration.ModificationType.OverrideValue:
                    // 对于数值状态的特殊处理
                    // 这里可以根据需要扩展
                    break;
            }
        }
    }

    private void CheckStateTransitions()
    {
        foreach (var state in finalStates)
        {
            if (state.Value != previousStates[state.Key])
            {
                // 触发状态变化事件
                OnStateChanged?.Invoke(state.Key, state.Value, false);
                previousStates[state.Key] = state.Value;
                
                // 调试输出
                if (visualizeStates)
                {
                    Debug.Log($"[State] {state.Key} changed to {state.Value}");
                }
            }
        }
    }
    
    // 公共API
    public bool IsStateActive(string stateName)
    {
        return finalStates.ContainsKey(stateName) && finalStates[stateName];
    }
    
    public bool IsBaseStateActive(string stateName)
    {
        return baseStates.ContainsKey(stateName) && baseStates[stateName];
    }
    
    public void SetBaseState(string stateName, bool value)
    {
        if (baseStates.ContainsKey(stateName))
        {
            baseStates[stateName] = value;
        }
    }
    
    public void ForceFinalState(string stateName, bool value)
    {
        if (finalStates.ContainsKey(stateName))
        {
            finalStates[stateName] = value;
            previousStates[stateName] = !value; // 确保下次更新触发事件
        }
    }
}
}