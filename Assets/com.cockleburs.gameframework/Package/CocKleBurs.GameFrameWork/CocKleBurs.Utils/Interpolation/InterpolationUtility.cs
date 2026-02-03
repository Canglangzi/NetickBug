using System;
using Unity.Mathematics;
using UnityEngine;


namespace CockleBurs.GameFramework.Utility
{
/// <summary>
/// A utility class for managing and simplifying interpolation tasks.
/// </summary>
public static class InterpolationUtility
{
    
    public static Clz_Interpolation<float3> CreateFloat3Interpolator(float buffering = 0.1f, int capacity = 60)
    {
        return new Clz_Interpolation<float3>(new Float3Interpolator(), capacity, buffering);
    }

    public static Clz_Interpolation<quaternion> CreateQuaternionInterpolator(float buffering = 0.1f, int capacity = 60)
    {
        return new Clz_Interpolation<quaternion>(new quaternionInterpolator(), capacity, buffering);
    }
    /// <summary>
    /// Creates and initializes an interpolator for Vector3 with optional buffering and capacity parameters.
    /// </summary>
    /// <param name="buffering">The buffering time in seconds.</param>
    /// <param name="capacity">The initial capacity of the buffer.</param>
    /// <returns>A new <see cref="Clz_Interpolation{Vector3}"/> instance.</returns>
    public static Clz_Interpolation<Vector3> CreatePositionInterpolator(float buffering = 0.1f, int capacity = 60)
    {
        return new Clz_Interpolation<Vector3>(new Vector3Interpolator(), capacity, buffering);
    }

    /// <summary>
    /// Creates and initializes an interpolator for Quaternion with optional buffering and capacity parameters.
    /// </summary>
    /// <param name="buffering">The buffering time in seconds.</param>
    /// <param name="capacity">The initial capacity of the buffer.</param>
    /// <returns>A new <see cref="Clz_Interpolation{Quaternion}"/> instance.</returns>
    public static Clz_Interpolation<Quaternion> CreateRotationInterpolator(float buffering = 0.1f, int capacity = 60)
    {
        return new Clz_Interpolation<Quaternion>(new QuaternionInterpolator(), capacity, buffering);
    }

    /// <summary>
    /// Adds a snapshot with start and end values to an interpolator.
    /// </summary>
    /// <param name="interpolator">The interpolator to add the snapshot to.</param>
    /// <param name="startTime">The time at which the start value is recorded.</param>
    /// <param name="duration">The duration over which interpolation occurs.</param>
    /// <param name="startValue">The start value for interpolation.</param>
    /// <param name="endValue">The end value for interpolation.</param>
    public static void AddSnapshotWithDuration<T>(Clz_Interpolation<T> interpolator, float startTime, float duration, T startValue, T endValue)
    {
        if (interpolator == null) throw new ArgumentNullException(nameof(interpolator));
        interpolator.Teleport(startTime, startValue);
        interpolator.Add(startTime + duration, endValue);
    }

    /// <summary>
    /// Updates the interpolator with the current time.
    /// </summary>
    /// <param name="interpolator">The interpolator to update.</param>
    /// <param name="currentTime">The current time.</param>
    public static void Update<T>(Clz_Interpolation<T> interpolator, float currentTime)
    {
        if (interpolator == null) throw new ArgumentNullException(nameof(interpolator));
        interpolator.Update(currentTime);
    }

    /// <summary>
    /// Gets the current interpolated value from the interpolator.
    /// </summary>
    /// <param name="interpolator">The interpolator to get the current value from.</param>
    /// <returns>The current interpolated value.</returns>
    public static T GetCurrent<T>(Clz_Interpolation<T> interpolator)
    {
        if (interpolator == null) throw new ArgumentNullException(nameof(interpolator));
        return interpolator.Current;
    }

    /// <summary>
    /// Performs linear interpolation between two values at specified times.
    /// </summary>
    /// <param name="interpolator">The interpolator to perform the interpolation with.</param>
    /// <param name="startTime">The start time of interpolation.</param>
    /// <param name="endTime">The end time of interpolation.</param>
    /// <param name="startValue">The start value of interpolation.</param>
    /// <param name="endValue">The end value of interpolation.</param>
    public static void LinearInterpolate<T>(Clz_Interpolation<T> interpolator, float startTime, float endTime, T startValue, T endValue)
    {
        if (interpolator == null) throw new ArgumentNullException(nameof(interpolator));
        interpolator.Teleport(startTime, startValue);
        interpolator.Add(endTime, endValue);
    }

    /// <summary>
    /// Captures the start value, performs linear interpolation to an end value, and updates the interpolator.
    /// </summary>
    /// <param name="interpolator">The interpolator to update.</param>
    /// <param name="endValue">The end value of interpolation.</param>
    /// <param name="duration">The duration of interpolation.</param>
    /// <param name="captureValue">A function to capture the current value.</param>
    public static void CaptureAndInterpolate<T>(Clz_Interpolation<T> interpolator, T endValue, float duration, Func<T> captureValue)
    {
        if (interpolator == null) throw new ArgumentNullException(nameof(interpolator));
        if (captureValue == null) throw new ArgumentNullException(nameof(captureValue));

        float currentTime = Time.time;
        T startValue = captureValue();
        LinearInterpolate(interpolator, currentTime, currentTime + duration, startValue, endValue);
    }

    /// <summary>
    /// Updates the interpolator during a fixed update loop to ensure consistency.
    /// </summary>
    /// <param name="interpolator">The interpolator to update.</param>
    /// <param name="fixedDeltaTime">The fixed delta time for the update.</param>
    public static void FixedUpdate<T>(Clz_Interpolation<T> interpolator, float fixedDeltaTime)
    {
        if (interpolator == null) throw new ArgumentNullException(nameof(interpolator));
        interpolator.Update(Time.time);
    }
}

}