using System.Reflection;


namespace CockleBurs.GameFramework.Extension
{
public static partial class ReflectionExtensions
{
    public static object InvokeInternalMethod(this object obj, string methodName, params object[] args)
    {
        var method = obj.GetType().GetMethod(methodName, 
            BindingFlags.NonPublic | BindingFlags.Instance);
        return method?.Invoke(obj, args);
    }

    public static object GetInternalMember(this object obj, string memberName)
    {
        var field = obj.GetType().GetField(memberName, 
            BindingFlags.NonPublic | BindingFlags.Instance);
        return field?.GetValue(obj);
    }
}
}