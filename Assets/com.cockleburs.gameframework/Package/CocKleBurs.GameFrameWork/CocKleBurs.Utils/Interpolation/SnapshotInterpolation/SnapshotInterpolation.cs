using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace CockleBurs.GameFramework.Utility
{
    public static class SortedListExtensions
    {
        public static void RemoveRange<T, U>(this SortedList<T, U> list, int amount)
        {
            for (int i = 0; i < amount && i < list.Count; ++i)
                list.RemoveAt(0);
        }
    }

    public static class SnapshotInterpolation
    {
        public static double Timescale(
            double drift,
            double catchupSpeed,
            double slowdownSpeed,
            double absoluteCatchupNegativeThreshold,
            double absoluteCatchupPositiveThreshold)
        {
            if (drift > absoluteCatchupPositiveThreshold)
            {
                return 1 + catchupSpeed;
            }

            if (drift < absoluteCatchupNegativeThreshold)
            {
                return 1 - slowdownSpeed;
            }

            return 1;
        }

        public static double DynamicAdjustment(
            double sendInterval,
            double jitterStandardDeviation,
            double dynamicAdjustmentTolerance)
        {
            double intervalWithJitter = sendInterval + jitterStandardDeviation;
            double multiples = intervalWithJitter / sendInterval;
            double safezone = multiples + dynamicAdjustmentTolerance;
            return safezone;
        }

        public static bool InsertIfNotExists<T>(
            SortedList<double, T> buffer,
            int bufferLimit,
            T snapshot)
            where T : Snapshot
        {
            if (buffer.Count >= bufferLimit) return false;
            int before = buffer.Count;
            buffer[snapshot.remoteTime] = snapshot;
            return buffer.Count > before;
        }

        public static double TimelineClamp(
            double localTimeline,
            double bufferTime,
            double latestRemoteTime)
        {
            double targetTime = latestRemoteTime - bufferTime;
            double lowerBound = targetTime - bufferTime;
            double upperBound = targetTime + bufferTime;
            return Mathd.Clamp(localTimeline, lowerBound, upperBound);
        }

        public static bool InsertAndAdjust<T>(
            SortedList<double, T> buffer,
            int bufferLimit,
            T snapshot,
            ref double localTimeline,
            ref double localTimescale,
            float sendInterval,
            double bufferTime,
            double catchupSpeed,
            double slowdownSpeed,
            ref ExponentialMovingAverage driftEma,
            float catchupNegativeThreshold,
            float catchupPositiveThreshold,
            ref ExponentialMovingAverage deliveryTimeEma)
            where T : Snapshot
        {
            if (buffer.Count == 0)
                localTimeline = snapshot.remoteTime - bufferTime;

            if (buffer.Count >= bufferLimit)
                buffer.RemoveAt(0);

            if (InsertIfNotExists(buffer, bufferLimit, snapshot))
            {
                if (buffer.Count >= 2)
                {
                    double previousLocalTime = buffer.Values[buffer.Count - 2].localTime;
                    double lastestLocalTime = buffer.Values[buffer.Count - 1].localTime;
                    double localDeliveryTime = lastestLocalTime - previousLocalTime;
                    deliveryTimeEma.Add(localDeliveryTime);
                }

                double latestRemoteTime = snapshot.remoteTime;
                localTimeline = TimelineClamp(localTimeline, bufferTime, latestRemoteTime);
                double timeDiff = latestRemoteTime - localTimeline;
                driftEma.Add(timeDiff);
                double drift = driftEma.Value - bufferTime;
                double absoluteNegativeThreshold = sendInterval * catchupNegativeThreshold;
                double absolutePositiveThreshold = sendInterval * catchupPositiveThreshold;
                localTimescale = Timescale(drift, catchupSpeed, slowdownSpeed, absoluteNegativeThreshold, absolutePositiveThreshold);

                return true;
            }

            return false;
        }

        public static void Sample<T>(
            SortedList<double, T> buffer,
            double localTimeline,
            out int from,
            out int to,
            out double t)
            where T : Snapshot
        {
            from = -1;
            to = -1;
            t = 0;

            for (int i = 0; i < buffer.Count - 1; ++i)
            {
                T first = buffer.Values[i];
                T second = buffer.Values[i + 1];
                if (localTimeline >= first.remoteTime &&
                    localTimeline <= second.remoteTime)
                {
                    from = i;
                    to = i + 1;
                    t = Mathd.InverseLerp(first.remoteTime, second.remoteTime, localTimeline);
                    return;
                }
            }

            if (buffer.Values[0].remoteTime > localTimeline)
            {
                from = to = 0;
                t = 0;
            }
            else
            {
                from = to = buffer.Count - 1;
                t = 0;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void StepTime(
            double deltaTime,
            ref double localTimeline,
            double localTimescale)
        {
            localTimeline += deltaTime * localTimescale;
        }

        public static void StepInterpolation<T>(
            SortedList<double, T> buffer,
            double localTimeline,
            out T fromSnapshot,
            out T toSnapshot,
            out double t)
            where T : Snapshot
        {
            Sample(buffer, localTimeline, out int from, out int to, out t);
            fromSnapshot = buffer.Values[from];
            toSnapshot = buffer.Values[to];
            buffer.RemoveRange(from);
        }

        public static void Step<T>(
            SortedList<double, T> buffer,
            double deltaTime,
            ref double localTimeline,
            double localTimescale,
            out T fromSnapshot,
            out T toSnapshot,
            out double t)
            where T : Snapshot
        {
            StepTime(deltaTime, ref localTimeline, localTimescale);
            StepInterpolation(buffer, localTimeline, out fromSnapshot, out toSnapshot, out t);
        }
    }
}
