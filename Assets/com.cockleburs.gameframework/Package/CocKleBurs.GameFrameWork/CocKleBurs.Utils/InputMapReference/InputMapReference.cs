using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.InputSystem;


namespace CockleBurs.GameFramework.Utility
{
[System.Serializable]
public struct InputActionMapReference
{
    public string ActionMapName;
    
    [SerializeField, HideInInspector]
    private string _actionMapGuid; // 序列化为字符串
    
    [SerializeField, HideInInspector]
    private string _assetGuid;     // 序列化为字符串
    
    // 使用属性来访问 GUID，确保类型安全
    public Guid ActionMapGuid
    {
        get => string.IsNullOrEmpty(_actionMapGuid) ? Guid.Empty : new Guid(_actionMapGuid);
        set => _actionMapGuid = value.ToString();
    }
    
    public Guid AssetGuid
    {
        get => string.IsNullOrEmpty(_assetGuid) ? Guid.Empty : new Guid(_assetGuid);
        set => _assetGuid = value.ToString();
    }
}
}