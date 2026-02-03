using UnityEngine;

namespace CockleBurs.GameFramework.Utility
{
    public abstract class ConditionalAttribute : PropertyAttribute
    {
        public string Selector { get; protected set; }

        public ConditionalAttribute(string selector)
        {
            order = -1;
            Selector = selector;
        }
    }
}