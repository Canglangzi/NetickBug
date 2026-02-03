namespace CockleBurs.GameFramework.Extension
{
    using UnityEngine;

    /// <summary>
    /// 动画相关的扩展方法
    /// </summary>
    public static class AnimationExtensions
    {
        /// <summary>
        /// 检查当前动画状态是否为指定状态，如果是，则返回true，并返回动画的normalizedTime
        /// </summary>
        /// <param name="animator">动画控制器</param>
        /// <param name="stateName">状态名称</param>
        /// <param name="normalizedTime">归一化时间</param>
        /// <param name="layerIndex">层索引</param>
        /// <returns>是否为指定状态</returns>
        public static bool CheckAnimatorState(this Animator animator, string stateName, out float normalizedTime, int layerIndex = 0)
        {
            int targetStateHash = Animator.StringToHash(stateName);
            return CheckAnimatorState(animator, targetStateHash, out normalizedTime, layerIndex);
        }

        /// <summary>
        /// 检查当前动画状态是否为指定状态哈希值
        /// </summary>
        /// <param name="animator">动画控制器</param>
        /// <param name="stateHash">状态哈希值</param>
        /// <param name="normalizedTime">归一化时间</param>
        /// <param name="layerIndex">层索引</param>
        /// <returns>是否为指定状态</returns>
        public static bool CheckAnimatorState(this Animator animator, int stateHash, out float normalizedTime, int layerIndex = 0)
        {
            normalizedTime = 0;

            // 验证参数
            if (animator == null)
            {
                Debug.LogWarning("[AnimationExtensions] Animator is null");
                return false;
            }

            // 验证层索引是否有效
            if (layerIndex < 0 || layerIndex >= animator.layerCount)
            {
                Debug.LogError($"[AnimationExtensions] Invalid layer index: {layerIndex}. Total layers: {animator.layerCount}");
                return false;
            }

            AnimatorStateInfo currentInfo = animator.GetCurrentAnimatorStateInfo(layerIndex);
            AnimatorStateInfo nextInfo = animator.GetNextAnimatorStateInfo(layerIndex);

            // 优先检查下一个状态（过渡期间）
            if (nextInfo.shortNameHash == stateHash)
            {
                normalizedTime = nextInfo.normalizedTime;
                return true;
            }

            // 检查当前状态
            if (currentInfo.shortNameHash == stateHash)
            {
                normalizedTime = currentInfo.normalizedTime;
                return true;
            }

            return false;
        }

        /// <summary>
        /// 检查当前动画是否处于指定Tag状态
        /// </summary>
        /// <param name="animator">动画控制器</param>
        /// <param name="tagName">标签名称</param>
        /// <param name="layerIndex">层索引</param>
        /// <returns>是否为指定标签状态</returns>
        public static bool IsAnimationAtTag(this Animator animator, string tagName, int layerIndex = 0)
        {
            if (animator == null)
            {
                Debug.LogWarning("[AnimationExtensions] Animator is null");
                return false;
            }

            if (layerIndex < 0 || layerIndex >= animator.layerCount)
            {
                Debug.LogError($"[AnimationExtensions] Invalid layer index: {layerIndex}. Total layers: {animator.layerCount}");
                return false;
            }

            return animator.GetCurrentAnimatorStateInfo(layerIndex).IsTag(tagName);
        }
    }
}
