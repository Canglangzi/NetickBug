using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

    static class GameObjectExtensionMethods
    {
        #region Component
        /// <summary>
        /// Tries to find T on game object then child then parent
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="component"></param>
        /// <returns></returns>
        public static T SearchFor<T>(this Component component)
        {
            if (component.GetComponent<T>() != null) return component.GetComponent<T>();
            if (component.GetComponentInChildren<T>() != null) return component.GetComponentInChildren<T>();

            return component.GetComponentInParent<T>();
        }

        /// <summary>
        /// Tries to find Component on game object then child then parent
        /// </summary>
        /// <param name="component"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public static Component SearchFor(this Component component, Type type)
        {
            if (component.GetComponent(type) != null) return component.GetComponent(type);
            if (component.GetComponentInChildren(type) != null) return component.GetComponentInChildren(type);

            return component.GetComponentInParent(type);
        }
        #endregion

        #region Transform
        /// <summary>
        /// Sets transform position to given position
        /// </summary>
        /// <param name="transform"></param>
        /// <param name="position">Target position</param>
        /// <param name="local">If true position is going to chnage in local space insted of global space</param>
        public static void SetPosition(this Transform transform, Vector3 position, bool local = false)
        {
            if (local) transform.localPosition = position;
            else transform.position = position;
        }

        /// <summary>
        /// Sets transform rotation to given rotation
        /// </summary>
        /// <param name="transform"></param>
        /// <param name="rotation">Target rotation</param>
        /// <param name="local">If true position is going to chnage in local space insted of global space</param>
        public static void SetRotation(this Transform transform, Quaternion rotation, bool local = false)
        {
            if (local) transform.localRotation = rotation;
            else transform.rotation = rotation;
        }

        /// <summary>
        /// Resets transform position, roation & scale
        /// </summary>
        /// <param name="transform"></param>
        public static void Reset(this Transform transform)
        {
            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.identity;
            transform.localScale = Vector3.one;
        }

        /// <summary>
        /// Adds a new game object as a child of the transform
        /// </summary>
        /// <param name="transform"></param>
        /// <returns></returns>
        public static Transform CreateChild(this Transform transform)
        {
            Transform children = new GameObject("GameObject").transform;

            children.parent = transform;
            children.Reset();

            return children;
        }

        /// <summary>
        /// Adds a new game object as a child of the transform
        /// </summary>
        /// <param name="transform"></param>
        /// <param name="name">Name of the child</param>
        /// <returns></returns>
        public static Transform CreateChild(this Transform transform, string name)
        {
            Transform children = new GameObject(name).transform;

            children.parent = transform;
            children.Reset();

            return children;
        }


        /// <summary>
        /// Adds a new game object as a child of the transform
        /// </summary>
        /// <param name="transform"></param>
        /// <param name="names">Target names the more names you have the more childern you get</param>
        /// <param name="parentAll">If true all childern will be child if each other</param>
        /// <returns></returns>
        public static Transform[] CreateChildren(this Transform transform, string[] names, bool parentAll = false)
        {
            List<Transform> transforms = new List<Transform>();

            for (int i = 0; i < names.Length; i++)
            {
                Transform child = CreateChild(transform, names[i]);
                transforms.Add(child);

                if (parentAll)
                {
                    if (i > 1)
                    {
                        transforms[1].SetParent(transforms[0]);

                        child.SetParent(transforms[transforms.Count - 2]);
                    }
                }
            }

            return transforms.ToArray();
        }

        /// <summary>
        /// destroies all childern in transform
        /// </summary>
        /// <param name="transform"></param>
        public static void ClearChildren(this Transform transform)
        {
            for (int i = 0; i < transform.childCount; i++)
            {
                GameObject.Destroy(transform.GetChild(i).gameObject);
            }
        }

        public static void SetPositionAndRotation(this Transform transform, Vector3 position, Quaternion rotation, bool local = false)
        {
            if (!local)
            {
                transform.position = position;
                transform.rotation = rotation;
            }
            else
            {
                transform.localPosition = position;
                transform.localRotation = rotation;
            }
        }

        public static void SetPositionAndRotation(this Transform transform, Vector3 position, Vector3 eulerAngles, bool local = false)
        {
            if (!local)
            {
                transform.position = position;
                transform.eulerAngles = eulerAngles;
            }
            else
            {
                transform.localPosition = position;
                transform.localEulerAngles = eulerAngles;
            }
        }
        #endregion
        

        #region Rigidbody
        public static bool IsVelocityZero(this Rigidbody rigidbody)
        {
            //check if rigidbody is not moving if yes set to true else set to false
            return rigidbody.linearVelocity.magnitude <= 0;
        }
        #endregion
    }