namespace CockleBurs.GameFramework.Extension
{
    using UnityEngine;

    /// <summary>
    /// 数学和Transform相关的扩展方法
    /// 提供完整的缓动函数、插值算法、贝塞尔曲线等高级数学工具
    /// 
    /// 主要功能：
    /// - 24种标准缓动函数 (Ease In/Out/InOut)
    /// - 贝塞尔曲线和样条插值
    /// - 弹簧阻尼运动
    /// - Transform动画扩展
    /// - 实用数学计算工具
    /// 
    /// 使用示例：
    /// transform.LerpMoveToEased(targetPos, 2f, EaseType.EaseOutBounce);
    /// Vector3 point = MathExtensions.QuadraticBezier(start, control, end, 0.5f);
    /// float easedValue = MathExtensions.Ease(0.5f, EaseType.EaseInOutQuad);
    /// </summary>
    static partial class MathExtensions
    {
        /// <summary>
        /// 平滑看向目标方向（以Y轴为中心）
        /// </summary>
        /// <param name="transform">目标Transform</param>
        /// <param name="target">目标位置</param>
        /// <param name="smoothTime">平滑时间（建议大于10）</param>
        public static void LerpLookAt(this Transform transform, Vector3 target, float smoothTime = 10f)
        {
            Vector3 direction = (target - transform.position).normalized;
            direction.y = 0f; // 只在Y轴上旋转

            if (direction != Vector3.zero)
            {
                Quaternion lookRotation = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, GetFrameRateIndependentLerp(smoothTime));
            }
        }

        /// <summary>
        /// 平滑看向目标Transform（以Y轴为中心）
        /// </summary>
        /// <param name="transform">目标Transform</param>
        /// <param name="target">目标Transform</param>
        /// <param name="smoothTime">平滑时间（建议大于10）</param>
        public static void LerpLookAt(this Transform transform, Transform target, float smoothTime = 10f)
        {
            if (target != null)
            {
                transform.LerpLookAt(target.position, smoothTime);
            }
        }

        /// <summary>
        /// 获取不受帧率影响的插值系数
        /// 
        /// 这个方法使用指数衰减来生成平滑的插值系数，
        /// 无论帧率如何变化，都能保持一致的动画速度。
        /// 
        /// 数学原理：使用 1 - e^(-smoothTime * deltaTime) 公式
        /// 当deltaTime变化时，动画的视觉效果保持一致
        /// </summary>
        /// <param name="smoothTime">平滑时间（值越大越平滑，建议范围：1-20）</param>
        /// <returns>插值系数 (0-1)</returns>
        public static float GetFrameRateIndependentLerp(float smoothTime = 10f)
        {
            return 1f - Mathf.Exp(-smoothTime * Time.deltaTime);
        }

        /// <summary>
        /// 平滑移动到目标位置
        /// </summary>
        /// <param name="transform">目标Transform</param>
        /// <param name="targetPosition">目标位置</param>
        /// <param name="smoothTime">平滑时间</param>
        public static void LerpMoveTo(this Transform transform, Vector3 targetPosition, float smoothTime = 10f)
        {
            transform.position = Vector3.Lerp(transform.position, targetPosition, GetFrameRateIndependentLerp(smoothTime));
        }

        /// <summary>
        /// 检查两个向量是否近似相等
        /// </summary>
        /// <param name="vector">向量1</param>
        /// <param name="other">向量2</param>
        /// <param name="threshold">阈值</param>
        /// <returns>是否近似相等</returns>
        public static bool Approximately(this Vector3 vector, Vector3 other, float threshold = 0.01f)
        {
            return Vector3.Distance(vector, other) < threshold;
        }

        #region 动画曲线和缓动函数

        /// <summary>
        /// 缓动类型枚举
        /// 
        /// 缓动函数用于控制动画的速度变化，创造更自然和吸引人的动画效果
        /// 
        /// 命名规则：
        /// - EaseIn: 慢开始，快结束 (加速)
        /// - EaseOut: 快开始，慢结束 (减速) 
        /// - EaseInOut: 慢开始，慢结束，中间快 (先加速后减速)
        /// 
        /// 类型说明：
        /// - Quad/Cubic/Quart/Quint: 二次/三次/四次/五次方程，数字越大曲线越陡峭
        /// - Sine: 正弦曲线，平滑自然的变化
        /// - Expo: 指数曲线，变化非常剧烈
        /// - Circ: 圆形曲线，类似四分之一圆弧
        /// - Elastic: 弹性效果，类似弹簧
        /// - Back: 超调效果，会稍微超过目标值再回来
        /// - Bounce: 弹跳效果，类似球落地
        /// </summary>
        public enum EaseType
        {
            Linear,                    // 线性变化，恒定速度

            // 二次方程 (温和的加速/减速)
            EaseInQuad, EaseOutQuad, EaseInOutQuad,

            // 三次方程 (中等的加速/减速)
            EaseInCubic, EaseOutCubic, EaseInOutCubic,

            // 四次方程 (较强的加速/减速)
            EaseInQuart, EaseOutQuart, EaseInOutQuart,

            // 五次方程 (很强的加速/减速)
            EaseInQuint, EaseOutQuint, EaseInOutQuint,

            // 正弦曲线 (平滑自然)
            EaseInSine, EaseOutSine, EaseInOutSine,

            // 指数曲线 (急剧变化)
            EaseInExpo, EaseOutExpo, EaseInOutExpo,

            // 圆形曲线 (类似四分之一圆)
            EaseInCirc, EaseOutCirc, EaseInOutCirc,

            // 弹性效果 (类似弹簧振荡)
            EaseInElastic, EaseOutElastic, EaseInOutElastic,

            // 超调效果 (超过目标再回来)
            EaseInBack, EaseOutBack, EaseInOutBack,

            // 弹跳效果 (类似球的弹跳)
            EaseInBounce, EaseOutBounce, EaseInOutBounce
        }

        /// <summary>
        /// 根据缓动类型计算插值
        /// 
        /// 这是核心的缓动计算函数，将线性的时间参数转换为非线性的缓动值。
        /// 所有缓动函数都基于标准的数学公式实现，确保动画效果的一致性。
        /// 
        /// 常用推荐：
        /// - UI动画: EaseOutQuad, EaseInOutQuad
        /// - 物理效果: EaseOutBounce, EaseOutElastic
        /// - 相机移动: EaseInOutSine, EaseInOutCubic
        /// - 快速反馈: EaseOutBack, EaseOutExpo
        /// </summary>
        /// <param name="t">时间参数 (0-1)，会自动钳制到有效范围</param>
        /// <param name="easeType">缓动类型，决定动画的速度曲线</param>
        /// <returns>缓动后的值 (通常在0-1范围，某些类型如Back和Elastic可能超出)</returns>
        public static float Ease(float t, EaseType easeType)
        {
            t = Mathf.Clamp01(t);

            return easeType switch
            {
                EaseType.Linear => t,

                // Quad
                EaseType.EaseInQuad => t * t,
                EaseType.EaseOutQuad => 1f - (1f - t) * (1f - t),
                EaseType.EaseInOutQuad => t < 0.5f ? 2f * t * t : 1f - Mathf.Pow(-2f * t + 2f, 2f) / 2f,

                // Cubic
                EaseType.EaseInCubic => t * t * t,
                EaseType.EaseOutCubic => 1f - Mathf.Pow(1f - t, 3f),
                EaseType.EaseInOutCubic => t < 0.5f ? 4f * t * t * t : 1f - Mathf.Pow(-2f * t + 2f, 3f) / 2f,

                // Quart
                EaseType.EaseInQuart => t * t * t * t,
                EaseType.EaseOutQuart => 1f - Mathf.Pow(1f - t, 4f),
                EaseType.EaseInOutQuart => t < 0.5f ? 8f * t * t * t * t : 1f - Mathf.Pow(-2f * t + 2f, 4f) / 2f,

                // Quint
                EaseType.EaseInQuint => t * t * t * t * t,
                EaseType.EaseOutQuint => 1f - Mathf.Pow(1f - t, 5f),
                EaseType.EaseInOutQuint => t < 0.5f ? 16f * t * t * t * t * t : 1f - Mathf.Pow(-2f * t + 2f, 5f) / 2f,

                // Sine
                EaseType.EaseInSine => 1f - Mathf.Cos(t * Mathf.PI / 2f),
                EaseType.EaseOutSine => Mathf.Sin(t * Mathf.PI / 2f),
                EaseType.EaseInOutSine => -(Mathf.Cos(Mathf.PI * t) - 1f) / 2f,

                // Expo
                EaseType.EaseInExpo => t == 0f ? 0f : Mathf.Pow(2f, 10f * (t - 1f)),
                EaseType.EaseOutExpo => t == 1f ? 1f : 1f - Mathf.Pow(2f, -10f * t),
                EaseType.EaseInOutExpo => t == 0f ? 0f : t == 1f ? 1f :
                    t < 0.5f ? Mathf.Pow(2f, 20f * t - 10f) / 2f : (2f - Mathf.Pow(2f, -20f * t + 10f)) / 2f,

                // Circ
                EaseType.EaseInCirc => 1f - Mathf.Sqrt(1f - Mathf.Pow(t, 2f)),
                EaseType.EaseOutCirc => Mathf.Sqrt(1f - Mathf.Pow(t - 1f, 2f)),
                EaseType.EaseInOutCirc => t < 0.5f ? (1f - Mathf.Sqrt(1f - Mathf.Pow(2f * t, 2f))) / 2f :
                    (Mathf.Sqrt(1f - Mathf.Pow(-2f * t + 2f, 2f)) + 1f) / 2f,

                // Elastic
                EaseType.EaseInElastic => EaseInElastic(t),
                EaseType.EaseOutElastic => EaseOutElastic(t),
                EaseType.EaseInOutElastic => EaseInOutElastic(t),

                // Back
                EaseType.EaseInBack => EaseInBack(t),
                EaseType.EaseOutBack => EaseOutBack(t),
                EaseType.EaseInOutBack => EaseInOutBack(t),

                // Bounce
                EaseType.EaseInBounce => 1f - EaseOutBounce(1f - t),
                EaseType.EaseOutBounce => EaseOutBounce(t),
                EaseType.EaseInOutBounce => t < 0.5f ? (1f - EaseOutBounce(1f - 2f * t)) / 2f : (1f + EaseOutBounce(2f * t - 1f)) / 2f,

                _ => t
            };
        }

        // Elastic辅助方法 - 弹性效果实现
        // 模拟弹簧或橡皮筋的振荡效果，适用于有弹性感的UI动画
        private static float EaseInElastic(float t)
        {
            const float c4 = (2f * Mathf.PI) / 3f; // 控制振荡频率的常数
            return t == 0f ? 0f : t == 1f ? 1f : -Mathf.Pow(2f, 10f * t - 10f) * Mathf.Sin((t * 10f - 10.75f) * c4);
        }

        private static float EaseOutElastic(float t)
        {
            const float c4 = (2f * Mathf.PI) / 3f;
            return t == 0f ? 0f : t == 1f ? 1f : Mathf.Pow(2f, -10f * t) * Mathf.Sin((t * 10f - 0.75f) * c4) + 1f;
        }

        private static float EaseInOutElastic(float t)
        {
            const float c5 = (2f * Mathf.PI) / 4.5f;
            return t == 0f ? 0f : t == 1f ? 1f : t < 0.5f ?
                -(Mathf.Pow(2f, 20f * t - 10f) * Mathf.Sin((20f * t - 11.125f) * c5)) / 2f :
                (Mathf.Pow(2f, -20f * t + 10f) * Mathf.Sin((20f * t - 11.125f) * c5)) / 2f + 1f;
        }

        // Back辅助方法 - 超调效果实现
        // 创建超过目标值再回来的效果，常用于吸引注意力的动画
        private static float EaseInBack(float t)
        {
            const float c1 = 1.70158f; // 控制超调幅度的魔法数字
            const float c3 = c1 + 1f;
            return c3 * t * t * t - c1 * t * t;
        }

        private static float EaseOutBack(float t)
        {
            const float c1 = 1.70158f;
            const float c3 = c1 + 1f;
            return 1f + c3 * Mathf.Pow(t - 1f, 3f) + c1 * Mathf.Pow(t - 1f, 2f);
        }

        private static float EaseInOutBack(float t)
        {
            const float c1 = 1.70158f;
            const float c2 = c1 * 1.525f;
            return t < 0.5f ? (Mathf.Pow(2f * t, 2f) * ((c2 + 1f) * 2f * t - c2)) / 2f :
                (Mathf.Pow(2f * t - 2f, 2f) * ((c2 + 1f) * (t * 2f - 2f) + c2) + 2f) / 2f;
        }

        // Bounce辅助方法 - 弹跳效果实现
        // 模拟球弹跳的物理效果，每次弹跳的高度逐渐减小
        private static float EaseOutBounce(float t)
        {
            const float n1 = 7.5625f; // 弹跳强度系数
            const float d1 = 2.75f;   // 弹跳时间分割点

            // 第一次弹跳 (最高)
            if (t < 1f / d1)
            {
                return n1 * t * t;
            }
            // 第二次弹跳
            else if (t < 2f / d1)
            {
                return n1 * (t -= 1.5f / d1) * t + 0.75f;
            }
            // 第三次弹跳
            else if (t < 2.5f / d1)
            {
                return n1 * (t -= 2.25f / d1) * t + 0.9375f;
            }
            // 最后一次小弹跳
            else
            {
                return n1 * (t -= 2.625f / d1) * t + 0.984375f;
            }
        }

        #endregion

        #region 高级插值方法

        /// <summary>
        /// 高级插值方法区域
        /// 
        /// 这个区域包含了各种高级的插值算法：
        /// - 带缓动的插值 (LerpEased系列)
        /// - 贝塞尔曲线插值 (QuadraticBezier, CubicBezier)
        /// - 样条插值 (CatmullRom)
        /// 
        /// 这些方法可以创建比简单线性插值更复杂和自然的动画效果
        /// </summary>

        /// <summary>
        /// 带缓动的向量插值
        /// </summary>
        /// <param name="from">起始向量</param>
        /// <param name="to">目标向量</param>
        /// <param name="t">时间参数 (0-1)</param>
        /// <param name="easeType">缓动类型</param>
        /// <returns>插值结果</returns>
        public static Vector3 LerpEased(Vector3 from, Vector3 to, float t, EaseType easeType = EaseType.Linear)
        {
            float easedT = Ease(t, easeType);
            return Vector3.Lerp(from, to, easedT);
        }

        /// <summary>
        /// 带缓动的颜色插值
        /// </summary>
        /// <param name="from">起始颜色</param>
        /// <param name="to">目标颜色</param>
        /// <param name="t">时间参数 (0-1)</param>
        /// <param name="easeType">缓动类型</param>
        /// <returns>插值结果</returns>
        public static Color LerpEased(Color from, Color to, float t, EaseType easeType = EaseType.Linear)
        {
            float easedT = Ease(t, easeType);
            return Color.Lerp(from, to, easedT);
        }

        /// <summary>
        /// 带缓动的四元数插值
        /// </summary>
        /// <param name="from">起始旋转</param>
        /// <param name="to">目标旋转</param>
        /// <param name="t">时间参数 (0-1)</param>
        /// <param name="easeType">缓动类型</param>
        /// <returns>插值结果</returns>
        public static Quaternion SlerpEased(Quaternion from, Quaternion to, float t, EaseType easeType = EaseType.Linear)
        {
            float easedT = Ease(t, easeType);
            return Quaternion.Slerp(from, to, easedT);
        }

        /// <summary>
        /// 贝塞尔曲线插值（二次）
        /// 
        /// 二次贝塞尔曲线由三个控制点定义，生成平滑的曲线路径。
        /// 非常适合创建抛物线运动，如投掷物轨迹、UI元素的弧形移动等。
        /// 
        /// 数学公式: B(t) = (1-t)²P₀ + 2(1-t)tP₁ + t²P₂
        /// 
        /// 使用场景：
        /// - 投掷物轨迹
        /// - UI元素弧形移动
        /// - 相机路径规划
        /// </summary>
        /// <param name="p0">起点 - 曲线的开始位置</param>
        /// <param name="p1">控制点 - 决定曲线的弯曲方向和程度</param>
        /// <param name="p2">终点 - 曲线的结束位置</param>
        /// <param name="t">时间参数 (0-1)，0返回起点，1返回终点</param>
        /// <returns>曲线上对应时间点的位置</returns>
        public static Vector3 QuadraticBezier(Vector3 p0, Vector3 p1, Vector3 p2, float t)
        {
            t = Mathf.Clamp01(t);
            float oneMinusT = 1f - t;
            return oneMinusT * oneMinusT * p0 + 2f * oneMinusT * t * p1 + t * t * p2;
        }

        /// <summary>
        /// 贝塞尔曲线插值（三次）
        /// 
        /// 三次贝塞尔曲线是最常用的曲线类型，由四个控制点定义。
        /// 提供了更高的灵活性，可以创建S形曲线和更复杂的路径。
        /// 
        /// 数学公式: B(t) = (1-t)³P₀ + 3(1-t)²tP₁ + 3(1-t)t²P₂ + t³P₃
        /// 
        /// 使用场景：
        /// - 复杂的运动路径
        /// - 平滑的相机轨道
        /// - 高级UI动画
        /// - 路径编辑器
        /// </summary>
        /// <param name="p0">起点 - 曲线的开始位置</param>
        /// <param name="p1">控制点1 - 影响起点附近的曲线形状</param>
        /// <param name="p2">控制点2 - 影响终点附近的曲线形状</param>
        /// <param name="p3">终点 - 曲线的结束位置</param>
        /// <param name="t">时间参数 (0-1)，0返回起点，1返回终点</param>
        /// <returns>曲线上对应时间点的位置</returns>
        public static Vector3 CubicBezier(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
        {
            t = Mathf.Clamp01(t);
            float oneMinusT = 1f - t;
            return oneMinusT * oneMinusT * oneMinusT * p0 +
                   3f * oneMinusT * oneMinusT * t * p1 +
                   3f * oneMinusT * t * t * p2 +
                   t * t * t * p3;
        }

        /// <summary>
        /// Catmull-Rom样条插值
        /// 
        /// Catmull-Rom样条是一种特殊的三次样条，保证曲线经过所有控制点。
        /// 它自动计算切线方向，生成非常平滑和自然的曲线。
        /// 
        /// 特点：
        /// - 曲线经过中间两个控制点 (p1 和 p2)
        /// - 前后两个点 (p0 和 p3) 用于计算切线方向
        /// - 生成的曲线C1连续 (位置和切线连续)
        /// 
        /// 使用场景：
        /// - 路径系统 (角色/相机移动)
        /// - 关键帧动画插值
        /// - 地形高度插值
        /// - 音频可视化曲线
        /// </summary>
        /// <param name="p0">前一个点 - 用于计算p1处的切线方向</param>
        /// <param name="p1">起点 - 曲线经过此点，t=0时的位置</param>
        /// <param name="p2">终点 - 曲线经过此点，t=1时的位置</param>
        /// <param name="p3">后一个点 - 用于计算p2处的切线方向</param>
        /// <param name="t">时间参数 (0-1)，0返回p1，1返回p2</param>
        /// <returns>样条曲线上对应时间点的位置</returns>
        public static Vector3 CatmullRom(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
        {
            t = Mathf.Clamp01(t);
            float t2 = t * t;
            float t3 = t2 * t;

            return 0.5f * (
                (2f * p1) +
                (-p0 + p2) * t +
                (2f * p0 - 5f * p1 + 4f * p2 - p3) * t2 +
                (-p0 + 3f * p1 - 3f * p2 + p3) * t3
            );
        }

        #endregion

        #region 扩展的Transform动画方法

        /// <summary>
        /// 带缓动的平滑移动
        /// </summary>
        /// <param name="transform">目标Transform</param>
        /// <param name="targetPosition">目标位置</param>
        /// <param name="smoothTime">平滑时间</param>
        /// <param name="easeType">缓动类型</param>
        public static void LerpMoveToEased(this Transform transform, Vector3 targetPosition, float smoothTime = 10f, EaseType easeType = EaseType.EaseOutQuad)
        {
            float t = GetFrameRateIndependentLerp(smoothTime);
            float easedT = Ease(t, easeType);
            transform.position = Vector3.Lerp(transform.position, targetPosition, easedT);
        }

        /// <summary>
        /// 带缓动的平滑旋转
        /// </summary>
        /// <param name="transform">目标Transform</param>
        /// <param name="targetRotation">目标旋转</param>
        /// <param name="smoothTime">平滑时间</param>
        /// <param name="easeType">缓动类型</param>
        public static void LerpRotateToEased(this Transform transform, Quaternion targetRotation, float smoothTime = 10f, EaseType easeType = EaseType.EaseOutQuad)
        {
            float t = GetFrameRateIndependentLerp(smoothTime);
            float easedT = Ease(t, easeType);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, easedT);
        }

        /// <summary>
        /// 带缓动的平滑缩放
        /// </summary>
        /// <param name="transform">目标Transform</param>
        /// <param name="targetScale">目标缩放</param>
        /// <param name="smoothTime">平滑时间</param>
        /// <param name="easeType">缓动类型</param>
        public static void LerpScaleToEased(this Transform transform, Vector3 targetScale, float smoothTime = 10f, EaseType easeType = EaseType.EaseOutQuad)
        {
            float t = GetFrameRateIndependentLerp(smoothTime);
            float easedT = Ease(t, easeType);
            transform.localScale = Vector3.Lerp(transform.localScale, targetScale, easedT);
        }

        /// <summary>
        /// 沿贝塞尔曲线移动
        /// </summary>
        /// <param name="transform">目标Transform</param>
        /// <param name="startPos">起点</param>
        /// <param name="controlPos">控制点</param>
        /// <param name="endPos">终点</param>
        /// <param name="t">时间参数 (0-1)</param>
        public static void MoveBezier(this Transform transform, Vector3 startPos, Vector3 controlPos, Vector3 endPos, float t)
        {
            transform.position = QuadraticBezier(startPos, controlPos, endPos, t);
        }

        #endregion

        #region 实用数学函数

        /// <summary>
        /// 实用数学函数区域
        /// 
        /// 包含各种实用的数学计算工具：
        /// - 数值重映射 (Remap)
        /// - 弹簧阻尼系统 (SmoothDamp系列)
        /// - 角度处理函数
        /// 
        /// 这些函数是游戏开发中经常使用的数学工具
        /// </summary>

        /// <summary>
        /// 将值重新映射到新的范围
        /// 
        /// 线性映射函数，将一个范围内的值映射到另一个范围。
        /// 保持原始值在原范围内的相对位置。
        /// 
        /// 公式: newValue = toMin + (value - fromMin) * (toMax - toMin) / (fromMax - fromMin)
        /// 
        /// 使用场景：
        /// - 传感器数据转换
        /// - UI滑块值转换
        /// - 音量控制映射
        /// - 颜色渐变计算
        /// 
        /// 示例: Remap(0.5f, 0f, 1f, 0f, 100f) 返回 50f
        /// </summary>
        /// <param name="value">要映射的原始值</param>
        /// <param name="fromMin">原始范围的最小值</param>
        /// <param name="fromMax">原始范围的最大值</param>
        /// <param name="toMin">目标范围的最小值</param>
        /// <param name="toMax">目标范围的最大值</param>
        /// <returns>映射到新范围的值</returns>
        public static float Remap(float value, float fromMin, float fromMax, float toMin, float toMax)
        {
            return toMin + (value - fromMin) * (toMax - toMin) / (fromMax - fromMin);
        }

        /// <summary>
        /// 弹簧阻尼运动（单个浮点值）
        /// 
        /// 实现基于物理的平滑运动，模拟弹簧-阻尼系统。
        /// 与简单的Lerp不同，这个函数考虑了速度和惯性，提供更自然的运动效果。
        /// 
        /// 特点：
        /// - 基于物理的运动模型
        /// - 不会超调目标值
        /// - 速度会逐渐减小到零
        /// - 适合需要平滑停止的动画
        /// 
        /// 使用场景：
        /// - 相机跟随
        /// - UI元素平滑移动
        /// - 音量/亮度调节
        /// - 数值平滑过渡
        /// </summary>
        /// <param name="current">当前值</param>
        /// <param name="target">目标值</param>
        /// <param name="velocity">当前速度（引用传递，函数会修改这个值）</param>
        /// <param name="smoothTime">到达目标值约63%所需的时间</param>
        /// <param name="maxSpeed">最大速度限制</param>
        /// <returns>新的位置值</returns>
        public static float SmoothDamp(float current, float target, ref float velocity, float smoothTime, float maxSpeed = Mathf.Infinity)
        {
            return Mathf.SmoothDamp(current, target, ref velocity, smoothTime, maxSpeed, Time.deltaTime);
        }

        /// <summary>
        /// 向量弹簧阻尼运动
        /// </summary>
        /// <param name="current">当前向量</param>
        /// <param name="target">目标向量</param>
        /// <param name="velocity">当前速度向量（引用传递）</param>
        /// <param name="smoothTime">平滑时间</param>
        /// <param name="maxSpeed">最大速度</param>
        /// <returns>新的向量</returns>
        public static Vector3 SmoothDamp(Vector3 current, Vector3 target, ref Vector3 velocity, float smoothTime, float maxSpeed = Mathf.Infinity)
        {
            return Vector3.SmoothDamp(current, target, ref velocity, smoothTime, maxSpeed, Time.deltaTime);
        }

        /// <summary>
        /// 角度弹簧阻尼运动
        /// </summary>
        /// <param name="current">当前角度</param>
        /// <param name="target">目标角度</param>
        /// <param name="velocity">当前角速度（引用传递）</param>
        /// <param name="smoothTime">平滑时间</param>
        /// <param name="maxSpeed">最大角速度</param>
        /// <returns>新的角度</returns>
        public static float SmoothDampAngle(float current, float target, ref float velocity, float smoothTime, float maxSpeed = Mathf.Infinity)
        {
            return Mathf.SmoothDampAngle(current, target, ref velocity, smoothTime, maxSpeed, Time.deltaTime);
        }

        #endregion
    }
}
