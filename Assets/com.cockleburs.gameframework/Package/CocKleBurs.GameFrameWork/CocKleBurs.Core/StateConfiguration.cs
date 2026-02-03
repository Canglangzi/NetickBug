using System;
using UnityEngine;
using UnityEngine.InputSystem;


namespace CockleBurs.GameFramework.Core
{
[CreateAssetMenu(fileName = "StateConfig", menuName = "CocKleBurs.GameFrameWork/State System/State Configuration")]
public class StateConfiguration : ScriptableObject
{
    [System.Serializable]
    public class StateDefinition
    {
        public string stateName;
        public InputActionReference inputAction;
        public ActivationType activationType;
        public Color debugColor = Color.white;
    }

    [System.Serializable]
    public class StateModifier
    {
        public string modifierName;
        
        [Tooltip("当这些状态激活时，此修改器生效")]
        public string[] requiredStates;
        
        [Tooltip("要修改的目标状态")]
        public string targetState;
        
        [Tooltip("修改方式")]
        public ModificationType modificationType;
        
        [Tooltip("当修改器不再生效时，是否恢复之前状态")]
        public bool restoreOnExit = true;
    }

    public enum ActivationType
    {
        Hold,       // 按住激活
        Toggle,     // 切换激活
        Trigger     // 触发激活（自动退出）
    }
    
    public enum ModificationType
    {
        ForceEnable,    // 强制激活
        ForceDisable,   // 强制禁用
        OverrideValue   // 覆盖值
    }

    public StateDefinition[] stateDefinitions;
    public StateModifier[] stateModifiers;
}
}