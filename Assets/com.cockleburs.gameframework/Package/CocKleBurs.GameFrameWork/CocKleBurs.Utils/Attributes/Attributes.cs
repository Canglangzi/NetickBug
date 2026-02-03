using System;
using System.Collections.Generic;
using CockleBurs.GameFramework.Core;
using UnityEngine;

namespace CockleBurs.GameFramework.Utility
{
[System.AttributeUsage(System.AttributeTargets.Class, AllowMultiple = false)]
public class ComponentTabAttribute : PropertyAttribute
{
    public ComponentCategory[] Categories { get; private set; }
    public ComponentTabAttribute(params ComponentCategory[] categories)
    {
        Categories = categories;
    }
}
public enum ComponentCategory
{
    核心功能,
    视觉效果,
    音频系统,
    物理交互,
    游戏逻辑,
    调试工具
}

[AttributeUsage(AttributeTargets.Class)]
internal class CreateOnInit : Attribute
{
    public string CallOnInit { get; set; }
    public string ObjectNameOverride { get; set; }
    public RuntimeInitializeLoadType LoadType { get; set; }
}
public class TagTreeAttribute : PropertyAttribute { }
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
public class FieldHelpAttribute : PropertyAttribute
{
    public string HelpText { get; }

    public FieldHelpAttribute(string helpText)
    {
        HelpText = helpText;
    }
}

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
public class ThreadSafeAttribute : Attribute
{
    public bool ThrowsException { get; set; }

    public ThreadSafeAttribute(bool throwsException)
    {
        ThrowsException = throwsException;
    }
}

public enum HashAlgorithmType
{
    SHA256,
    MD5
}
public enum GameObjectType
{
    None,
    Player,
    Enemy,
    Item,
    NPC,
    Environment
}
public class PrefabPreviewAttribute : PropertyAttribute
{
    public string Label { get; private set; }

    public PrefabPreviewAttribute(string label = "Prefab Preview")
    {
        Label = label;
    }
}
[Serializable]
public class WeakReferenceObject
{
    [SerializeField]
    private string resourcePath; // 资源路径

    public WeakReferenceObject(string path)
    {
        resourcePath = path;
    }

    public string ResourcePath => resourcePath;

    // 使用此方法来更新路径
    public void SetResourcePath(string path)
    {
        resourcePath = path;
    }
}
public class CkbBoxGroup : PropertyAttribute
{
    public string name;

    public CkbBoxGroup(string name)
    {
        this.name = name;
    }
}
public class ResetButtonAttribute : PropertyAttribute
{
}
public class UserHintAttribute : PropertyAttribute
{
    public string Hint;

    public UserHintAttribute(string hint)
    {
        Hint = hint;
    }
}
public class SeparatorAttribute : PropertyAttribute
{
}
public class FilePathSelectorAttribute : PropertyAttribute
{
}
public class ConditionalShowAttribute : PropertyAttribute
{
    public string ConditionField;

    public ConditionalShowAttribute(string conditionField)
    {
        ConditionField = conditionField;
    }
}
public class MinMaxSliderAttribute : PropertyAttribute
{
    public float MinLimit, MaxLimit;

    public MinMaxSliderAttribute(float min, float max)
    {
        MinLimit = min;
        MaxLimit = max;
    }
}
[AttributeUsage(AttributeTargets.Class, Inherited = true)]
public class DisableNetworkEntityCheckAttribute : Attribute
{
}
[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public class ListenerTagAttribute : Attribute
{
    public string Tag { get; }

    public ListenerTagAttribute(string tag)
    {
        Tag = tag;
    }
}
public enum CalculationType
{
    Additive,
    Multiplicative,
    Percentage
}
[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public class EarlyAttribute : Attribute { }

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public class LateAttribute : Attribute { }

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public class BeforeAttribute : Attribute
{
    public Type TargetType { get; }

    public BeforeAttribute(Type targetType)
    {
        TargetType = targetType;
    }
}

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public class AfterAttribute : Attribute
{
    public Type TargetType { get; }

    public AfterAttribute(Type targetType)
    {
        TargetType = targetType;
    }
}
public class SerializableDictionaryAttribute : PropertyAttribute { }

public class SliderAttribute : PropertyAttribute
{
    public float min;
    public float max;

    public SliderAttribute(float min, float max)
    {
        this.min = min;
        this.max = max;
    }
}

public class ImagePreviewAttribute : PropertyAttribute { }
public class ColorAttribute : PropertyAttribute { }
public class FoldoutAttribute : PropertyAttribute
{
    public string label;

    public FoldoutAttribute(string label)
    {
        this.label = label;
    }
}

public class RequireComponentExAttribute : PropertyAttribute{}


public class CKBPropertyAttribute : PropertyAttribute { }

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method | AttributeTargets.Property)]
public class CodeGeneratorAttribute : Attribute
{
    public CodeGeneratorFlags Flags { get; }
    public string Callback { get; }

    public CodeGeneratorAttribute(CodeGeneratorFlags flags, string callback)
    {
        Flags = flags;
        Callback = callback;
    }
}

[Flags]
public enum CodeGeneratorFlags
{
    WrapMethod = 1,
    WrapPropertySet = 2,
    WrapPropertyGet = 4,
    Instance = 8
}


[AttributeUsage(AttributeTargets.Method)]
public class ServerAttribute : Attribute {}

[AttributeUsage(AttributeTargets.Method)]
public class ClientAttribute : Attribute {}

[AttributeUsage(AttributeTargets.Method)]
public class HostAttribute : Attribute {}
[AttributeUsage(AttributeTargets.Method)]
public class DebugAttribute : Attribute{
}
[AttributeUsage(AttributeTargets.Method)]
public class GameDebugAttribute : Attribute
{
}
public enum NetworkSpawnMethod
{
    Default,
}
public enum TickType
{
    Predicted,      // 预测的 Tick（客户端）
    Authoritative,  // 权威的 Tick（服务器）
    Current         // 当前的 Tick，无论是在客户端还是服务器
}
public enum Stage
{
    PhysicsStep,
    Update,
    LateUpdate,
    Render,
    // 其他阶段可以根据需求添加
}
public enum ScriptHeaderBackColor
{
    None,
    Gray,
    Blue,
    Red,
    Green,
    Orange,
    Black,
    Steel,
    Sand,
    Olive,
    Cyan,
    Violet
}

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public class CKBScriptInfo : Attribute
{
    public string Title { get; }
    public string Content { get; }
    public ScriptHeaderBackColor BackgroundColorOption { get; }

    // Mapping from enum to actual color
    private static readonly Dictionary<ScriptHeaderBackColor, Color> ColorMap = new Dictionary<ScriptHeaderBackColor, Color>
    {
        { ScriptHeaderBackColor.None, Color.clear },
        { ScriptHeaderBackColor.Gray, new Color(0.35f, 0.35f, 0.35f) },
        { ScriptHeaderBackColor.Blue, new Color(0.15f, 0.25f, 0.6f) },
        { ScriptHeaderBackColor.Red, new Color(0.5f, 0.1f, 0.1f) },
        { ScriptHeaderBackColor.Green, new Color(0.1f, 0.4f, 0.1f) },
        { ScriptHeaderBackColor.Orange, new Color(0.6f, 0.35f, 0.1f) },
        { ScriptHeaderBackColor.Black, new Color(0.1f, 0.1f, 0.1f) },
        { ScriptHeaderBackColor.Steel, new Color(0.32f, 0.35f, 0.38f) },
        { ScriptHeaderBackColor.Sand, new Color(0.38f, 0.35f, 0.32f) },
        { ScriptHeaderBackColor.Olive, new Color(0.25f, 0.33f, 0.15f) },
        { ScriptHeaderBackColor.Cyan, new Color(0.25f, 0.5f, 0.5f) },
        { ScriptHeaderBackColor.Violet, new Color(0.35f, 0.2f, 0.4f) }
    };

    public CKBScriptInfo(string title = "", string content = "", ScriptHeaderBackColor backgroundColor = ScriptHeaderBackColor.Blue)
    {
        Title = title;
        Content = content;
        BackgroundColorOption = backgroundColor;
    }

    public Color GetBackgroundColor()
    {
        return ColorMap.TryGetValue(BackgroundColorOption, out Color color) ? color : Color.blue;
    }
}


[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
public class CKBFieldHelpAttribute : PropertyAttribute
{
    public string HelpText { get; }

    public CKBFieldHelpAttribute(string helpText)
    {
        HelpText = helpText;
    }
}

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public class StartAfterAttribute : Attribute
{
    public Type TargetType { get; }

    public StartAfterAttribute(Type targetType)
    {
        TargetType = targetType;
    }
}

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public class StartBeforeAttribute : Attribute
{
    public Type TargetType { get; }

    public StartBeforeAttribute(Type targetType)
    {
        TargetType = targetType;
    }
}


public class AssetReferenceAttribute : PropertyAttribute
{
    public System.Type AssetType { get; private set; }

    public AssetReferenceAttribute(System.Type assetType)
    {
        AssetType = assetType;
    }
}
public enum NetworkManagerMode
{
    Offline,
    ServerOnly,
    ClientOnly,
    Host,
}  public interface ILocalPlayer
{
    int LocalPlayerId { get;set; }
    void LocalPlayerInitialize();
    void LocalPlayerCleanup();
}
// public interface IManagedSubsystem
// {
//    
//     void OnStart(World world);
//     void Destroy();
// }
[AttributeUsage(AttributeTargets.Class, Inherited = true)]
public class NotionAttribute : Attribute
{
    public string Url { get; private set; }

    public NotionAttribute(string url)
    {
        Url = url;
    }
}
/// <summary>
/// 标记需要自定义序列化的字段
/// </summary>
[AttributeUsage(AttributeTargets.Field)]
public class SerializeCustomAttribute : Attribute { }
/// <summary>
/// Draw the properties with a darker background and
/// borders, optionally.
/// </summary>
public class DarkBoxAttribute : Attribute
{
    /// <summary>
    /// Dark
    /// </summary>
    public readonly bool withBorders;

    public DarkBoxAttribute()
    { }

    public DarkBoxAttribute(bool withBorders)
    {
        this.withBorders = withBorders;
    }
}

public class ColorBox : Attribute
{
};

/// <summary>
/// 属性组特性 - 用于标记需要分组显示的属性
/// </summary>
public class PropertyGroupAttribute : PropertyAttribute
{
    public string GroupTitle { get; private set; }
    public BoxStyle StyleType { get; private set; }

    public enum BoxStyle
    {
        Default,
        Info,
        Warning,
        Error,
        Success
    }

    public PropertyGroupAttribute(string title = "", BoxStyle style = BoxStyle.Default)
    {
        GroupTitle = title;
        StyleType = style;
    }
}
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public class ToggleButtonsAttribute : Attribute
{
	public string m_trueText;
	public string m_falseText;
	
	public string m_trueTooltip;
	public string m_falseTooltip;
	
	public string m_trueIcon;
	public string m_falseIcon;

	public string m_trueColor;
	public string m_falseColor;
	
	public float m_sizeCompensationCompensation;
	public bool m_singleButton;

	/// <summary>
	/// Attribute to draw boolean as buttons
	/// </summary>
	/// <param name="trueText">Text for true button. Can be resolved string</param>
	/// <param name="falseText">Text for false button. Can be resolved string</param>
	/// <param name="singleButton">If set to true, only one button matching bool value will be shown</param>
	/// <param name="sizeCompensation">Amount by which smaller button size is lerped to match bigger button.
	/// 0 - original size of smaller button (takes the least space).
	/// 1 - matches size of bigger button.</param>
	/// <param name="trueTooltip">Tooltip for true button. Can be resolved string</param>
	/// <param name="falseTooltip">Tooltip for false button. Can be resolved string</param>
	/// <param name="trueColor">Color of true button</param>
	/// <param name="falseColor">Color of false button</param>
	/// <param name="trueIcon">Icon for true button</param>
	/// <param name="falseIcon">Icon for false button</param>
	public ToggleButtonsAttribute(string trueText = "Yes", string falseText = "No", bool singleButton = false, 
		float sizeCompensation = 1f, string trueTooltip = "", string falseTooltip = "",
		string trueColor = "", string falseColor = "", string trueIcon = "", string falseIcon = "")
	{
		m_trueText = trueText;
		m_falseText = falseText;

		m_singleButton = singleButton;
		m_sizeCompensationCompensation = sizeCompensation;

		m_trueTooltip = trueTooltip;
		m_falseTooltip = falseTooltip;
		
		m_trueIcon = trueIcon;
		m_falseIcon = falseIcon;

		m_trueColor = trueColor;
		m_falseColor = falseColor;
	}
    [AttributeUsage(AttributeTargets.Field, Inherited = true)]
    public class CKBTitleAttribute : PropertyAttribute
    {
        #region Constants
        public const float DefaultLineHeight = 1f;
        public const CustomColor DefaultLineColor = CustomColor.LightGray;
        public const CustomColor DefaultTitleColor = CustomColor.Bright;
        #endregion

        #region Properties
        public string Title { get; private set; }
        public float LineHeight { get; private set; }
        public CustomColor LineColor { get; private set; }
        public CustomColor TitleColor { get; private set; }
        public string LineColorString { get; private set; }
        public string TitleColorString { get; private set; }
        public float Spacing { get; private set; }
        public bool AlignTitleLeft { get; private set; }
        #endregion

        public CKBTitleAttribute(string title = "", CustomColor titleColor = DefaultTitleColor,
            CustomColor lineColor = DefaultLineColor, float lineHeight = DefaultLineHeight, float spacing = 14f,
            bool alignTitleLeft = false)
        {
            Title = title;
            TitleColor = titleColor;
            LineColor = lineColor;
            TitleColorString = ColorUtility.ToHtmlStringRGB(TitleColor.ToColor());
            LineColorString = ColorUtility.ToHtmlStringRGB(LineColor.ToColor());
            LineHeight = Mathf.Max(1f, lineHeight);
            Spacing = spacing;
            AlignTitleLeft = alignTitleLeft;
        }
    }
}
public enum CustomColor
{
    Aqua,
    Beige,
    Black,
    Blue,
    BlueVariant,
    DarkBlue,
    Bright,
    Brown,
    Cyan,
    DarkGray,
    Fuchsia,
    Gray,
    Green,
    Indigo,
    LightGray,
    Lime,
    Navy,
    Olive,
    DarkOlive,
    Orange,
    OrangeVariant,
    Pink,
    Red,
    LightRed,
    RedVariant,
    DarkRed,
    Tan,
    Teal,
    Violet,
    White,
    Yellow
}
}