using System;
using System.Collections.Generic;


namespace CockleBurs.GameFramework.Utility
{
public class InfoStorage
{
    // Using a dictionary to store values based on keys with dedicated storage for each type
    private readonly Dictionary<Type, IDictionary<Type, object>> storage
        = new Dictionary<Type, IDictionary<Type, object>>();

    // Method to set information
    public void SetInfo<TKey, T>(T value) where T : unmanaged where TKey : unmanaged
    {
        var keyType = typeof(TKey);
        var valueType = typeof(T);

        // Initialize inner dictionary if it doesn't exist
        if (!storage.TryGetValue(keyType, out var innerDict))
        {
            innerDict = new Dictionary<Type, object>();
            storage[keyType] = innerDict;
        }

        // Set the value in the dictionary
        innerDict[valueType] = value;
    }

    // Method to get information
    public T GetInfo<TKey, T>(T defaultValue = default) where T : unmanaged where TKey : unmanaged
    {
        var keyType = typeof(TKey);
        var valueType = typeof(T);

        // Check if the key exists and return the value if found
        if (storage.TryGetValue(keyType, out var innerDict) && innerDict.TryGetValue(valueType, out var value))
        {
            return (T)value; // Cast directly to T without boxing
        }

        // Return the default value if not found
        return defaultValue;
    }
}
}