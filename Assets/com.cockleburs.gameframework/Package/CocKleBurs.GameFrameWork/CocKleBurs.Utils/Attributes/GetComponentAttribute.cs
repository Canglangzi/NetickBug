using System;
using UnityEngine;



namespace CockleBurs.GameFramework.Utility
{
public abstract class ComponentRetrieverAttribute : PropertyAttribute { }

    [System.AttributeUsage(System.AttributeTargets.Field | AttributeTargets.Property)]
    public class GetComponentAttribute : ComponentRetrieverAttribute { }

    [System.AttributeUsage(System.AttributeTargets.Field | AttributeTargets.Property)]
    public class GetComponentsAttribute : ComponentRetrieverAttribute { }

    [System.AttributeUsage(System.AttributeTargets.Field | AttributeTargets.Property)]
    public class GetComponentInChildrenAttribute : ComponentRetrieverAttribute { }

    [System.AttributeUsage(System.AttributeTargets.Field | AttributeTargets.Property)]
    public class GetComponentsInChildrenAttribute : ComponentRetrieverAttribute { }

    [System.AttributeUsage(System.AttributeTargets.Field | AttributeTargets.Property)]
    public class GetComponentInParentAttribute : ComponentRetrieverAttribute { }

}