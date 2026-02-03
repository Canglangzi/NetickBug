using System;
using UnityEngine;


namespace CockleBurs.GameFramework.Utility
{
public class TSubclassOf<T> where T : MonoBehaviour
{
    private Type _type;

    // Constructor that takes a type
    public TSubclassOf(Type type)
    {
        if (!typeof(T).IsAssignableFrom(type))
        {
            throw new ArgumentException($"Type {type} is not a subclass of {typeof(T)}");
        }
        _type = type;
    }

    // Method to create an instance of the subclass
    public T CreateInstance(Vector3 position, Quaternion rotation)
    {
        GameObject obj = new GameObject(_type.Name);
        T instance = obj.AddComponent(_type) as T;

        if (instance != null)
        {
            obj.transform.position = position;
            obj.transform.rotation = rotation;
            return instance;
        }
        
        return null;
    }

    // Property to get the type of the subclass
    public Type SubclassType => _type;
}


}