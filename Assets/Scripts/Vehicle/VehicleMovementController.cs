using System;
using UnityEngine;
using RoadNetwork;

namespace Vehicle
{
    /// <summary>
    /// 車両の移動制御クラス
    /// IDM（Intelligent Driver Model）とMOBIL（車線変更モデル）を実装
    /// </summary>
    public class VehicleMovementController : MonoBehaviour
    {
        [Header("IDMパラメータ")]
        [Tooltip("希望時間車頭（秒）")]
        public float desiredTimeHeadway = 1.5f;

        [Tooltip("最小車間距離（メートル）")]
        public float minGap = 2.0f;

        [Tooltip("加速度指数")]
        public float accelerationExponent = 4f;

        [Tooltip("快適減速度（m/s²）")]
        public float comfortableDeceleration = 3.0f;

        [Header("MOBILパラメータ")]
        [Tooltip("礼譲度（0=利己的, 1=利他的）")]
        [Range(0f, 1f)]
        public float politeness = 0.5f;

        [Tooltip("車線変更閾値（m/s²）")]
        public float laneChangeThreshold = 0.2f;

        [Tooltip("最大安全減速度（m/s²）")]
        public float maxSafeDeceleration = 4.0f;

        [Header("信号対応")]
        [Tooltip("信号認識距離（メートル）")]
        public float signalDetectionDistance = 50f;

        [Tooltip("黄信号での停止判断距離（メートル）")]
        public float yellowStopDistance = 30f;

        [Header("デバッグ")]
        [Tooltip("デバッグログを出力")]
        public bool debugLog = false;

        // 車両参照
        private Vehicle _vehicle;

        private void Awake()
        {
            _vehicle = GetComponent<Vehicle>();
        }

        /// <summary>
        /// IDMに基づいて加速度を計算
        /// </summary>
        /// <param name="vehicle">対象車両</param>
        /// <returns>加速度（m/s²）</returns>
        public float CalculateAcceleration(Vehicle vehicle)
        {
            if (vehicle == null)
            {
                vehicle = _vehicle;
            }

            if (vehicle == null)
            {
                return 0f;
            }

            // 基本パラメータ
            float v = vehicle.currentSpeed;                    // 現在速度 (m/s)
            float v0 = vehicle.MaxSpeedMs;                     // 希望速度 (m/s)
            float a = vehicle.acceleration;                     // 最大加速度 (m/s²)

            // 前方車両との関係
            float s = vehicle.GetDistanceToLeader();           // 前方車両との距離 (m)
            float deltaV = v - vehicle.GetLeaderSpeed();       // 速度差 (m/s)

            // 道路の制限速度を考慮
            if (vehicle.currentRoad != null)
            {
                float roadSpeedLimit = vehicle.currentRoad.speedLimit / 3.6f;
                v0 = Mathf.Min(v0, roadSpeedLimit);
            }

            // IDM加速度を計算
            float acceleration = CalculateIDMAcceleration(v, v0, a, s, deltaV);

            // 信号による減速を考慮
            acceleration = ApplySignalDeceleration(vehicle, acceleration);

            // 交差点での減速を考慮
            acceleration = ApplyIntersectionDeceleration(vehicle, acceleration);

            if (debugLog)
            {
                Debug.Log($"[VehicleMovement] {vehicle.vehicleId}: v={v:F2}, gap={s:F2}, accel={acceleration:F2}");
            }

            return acceleration;
        }

        /// <summary>
        /// IDM（Intelligent Driver Model）の加速度計算
        ///
        /// a(v, s, Δv) = a_max × [1 - (v/v0)^δ - (s*/s)²]
        ///
        /// s* = s0 + v×T + (v×Δv)/(2×√(a_max×b))
        /// </summary>
        private float CalculateIDMAcceleration(float v, float v0, float aMax, float s, float deltaV)
        {
            // 前方に車両がいない場合
            if (s >= 1000f || float.IsInfinity(s))
            {
                // 自由走行：希望速度に向かって加速
                float freeAccel = aMax * (1f - Mathf.Pow(v / Mathf.Max(v0, 0.1f), accelerationExponent));
                return freeAccel;
            }

            // 希望車間距離 s* を計算
            float sqrtTerm = 2f * Mathf.Sqrt(aMax * comfortableDeceleration);
            float interactionTerm = (v * deltaV) / Mathf.Max(sqrtTerm, 0.1f);
            float desiredGap = minGap + v * desiredTimeHeadway + Mathf.Max(0, interactionTerm);

            // IDM加速度を計算
            float velocityTerm = Mathf.Pow(v / Mathf.Max(v0, 0.1f), accelerationExponent);
            float gapTerm = Mathf.Pow(desiredGap / Mathf.Max(s, 0.1f), 2f);

            float acceleration = aMax * (1f - velocityTerm - gapTerm);

            // 減速度の制限
            acceleration = Mathf.Max(acceleration, -vehicle.deceleration);

            return acceleration;
        }

        /// <summary>
        /// 信号による減速を適用
        /// </summary>
        private float ApplySignalDeceleration(Vehicle vehicle, float currentAccel)
        {
            if (vehicle.currentRoad == null) return currentAccel;

            // 前方の交差点を取得
            Intersection nextIntersection = vehicle.currentRoad.endIntersection;
            if (nextIntersection == null || !nextIntersection.hasSignal)
            {
                return currentAccel;
            }

            // 交差点までの距離を計算
            float remainingT = 1f - vehicle.positionOnLane;
            float distanceToIntersection = remainingT * (vehicle.currentLane?.length ?? 0);

            if (distanceToIntersection > signalDetectionDistance)
            {
                return currentAccel;
            }

            // 信号状態を取得（Phase 3で実装予定、暫定的にtrueを返す）
            bool isGreen = true; // 暫定
            bool isYellow = false; // 暫定

            if (isGreen)
            {
                return currentAccel;
            }

            if (isYellow)
            {
                // ジレンマゾーン判定
                float stoppingDistance = (vehicle.currentSpeed * vehicle.currentSpeed) / (2f * comfortableDeceleration);

                if (distanceToIntersection > stoppingDistance)
                {
                    // 安全に停止可能
                    return CalculateDecelerationToStop(vehicle.currentSpeed, distanceToIntersection);
                }
                else
                {
                    // 停止困難 - そのまま通過
                    return currentAccel;
                }
            }

            // 赤信号
            return CalculateDecelerationToStop(vehicle.currentSpeed, distanceToIntersection);
        }

        /// <summary>
        /// 交差点での減速を適用
        /// </summary>
        private float ApplyIntersectionDeceleration(Vehicle vehicle, float currentAccel)
        {
            if (vehicle.currentRoad == null) return currentAccel;

            Intersection nextIntersection = vehicle.currentRoad.endIntersection;
            if (nextIntersection == null)
            {
                return currentAccel;
            }

            // 無信号交差点での優先判断
            if (!nextIntersection.hasSignal)
            {
                float remainingT = 1f - vehicle.positionOnLane;
                float distanceToIntersection = remainingT * (vehicle.currentLane?.length ?? 0);

                // 交差点に近づいたら減速
                if (distanceToIntersection < 20f)
                {
                    // 進入可能かチェック
                    RoadSegment nextRoad = null;
                    if (vehicle.plannedPath.Count > vehicle.currentPathIndex + 1)
                    {
                        nextRoad = vehicle.plannedPath[vehicle.currentPathIndex + 1];
                    }

                    if (nextRoad != null && !nextIntersection.CanEnter(vehicle.gameObject, vehicle.currentRoad, nextRoad))
                    {
                        // 進入不可 - 停止
                        return CalculateDecelerationToStop(vehicle.currentSpeed, distanceToIntersection - 5f);
                    }
                    else
                    {
                        // 進入可能だが減速
                        float targetSpeed = Mathf.Min(vehicle.currentSpeed, 5f); // 20 km/h
                        return CalculateAccelerationToSpeed(vehicle.currentSpeed, targetSpeed, distanceToIntersection);
                    }
                }
            }

            return currentAccel;
        }

        /// <summary>
        /// 指定距離で停止するための減速度を計算
        /// </summary>
        private float CalculateDecelerationToStop(float currentSpeed, float distance)
        {
            if (distance <= 0.1f)
            {
                return -maxSafeDeceleration;
            }

            // v² = 2as → a = -v²/(2s)
            float requiredDecel = -(currentSpeed * currentSpeed) / (2f * distance);
            return Mathf.Max(requiredDecel, -maxSafeDeceleration);
        }

        /// <summary>
        /// 指定距離で目標速度に到達するための加速度を計算
        /// </summary>
        private float CalculateAccelerationToSpeed(float currentSpeed, float targetSpeed, float distance)
        {
            if (distance <= 0.1f)
            {
                return targetSpeed > currentSpeed ? _vehicle.acceleration : -comfortableDeceleration;
            }

            // v² - v0² = 2as → a = (v² - v0²)/(2s)
            float requiredAccel = (targetSpeed * targetSpeed - currentSpeed * currentSpeed) / (2f * distance);
            return Mathf.Clamp(requiredAccel, -comfortableDeceleration, _vehicle.acceleration);
        }

        /// <summary>
        /// MOBIL車線変更判定
        /// </summary>
        /// <param name="targetLane">目標車線</param>
        /// <returns>車線変更すべきか</returns>
        public bool ShouldChangeLane(Lane targetLane)
        {
            if (_vehicle == null || _vehicle.currentLane == null || targetLane == null)
            {
                return false;
            }

            // 現在車線での加速度
            float accelCurrent = CalculateAcceleration(_vehicle);

            // 目標車線での加速度を推定
            float accelTarget = EstimateAccelerationInLane(targetLane);

            // 現在車線の後続車への影響
            float accelFollowerOld = GetFollowerAcceleration(_vehicle.currentLane);

            // 目標車線の後続車への影響（車線変更後）
            float accelFollowerNew = EstimateFollowerAccelerationAfterChange(targetLane);

            // MOBIL判定式
            // ã_c - a_c > Δa_th + p × (a_n + ã_n - ã_o - a_o)
            float advantage = accelTarget - accelCurrent;
            float politenessImpact = politeness * (accelFollowerNew + accelFollowerNew - accelFollowerOld - accelFollowerOld);
            float threshold = laneChangeThreshold + politenessImpact;

            // 安全基準のチェック
            if (accelFollowerNew < -maxSafeDeceleration)
            {
                return false;
            }

            return advantage > threshold;
        }

        /// <summary>
        /// 車線変更を実行
        /// </summary>
        public void ExecuteLaneChange(Lane targetLane)
        {
            if (_vehicle == null || targetLane == null) return;

            // 現在車線から離脱
            _vehicle.currentLane?.RemoveVehicle(_vehicle.gameObject);

            // 新しい車線に登録
            _vehicle.currentLane = targetLane;
            _vehicle.positionOnLane = targetLane.GetTFromWorldPosition(_vehicle.transform.position);
            targetLane.AddVehicle(_vehicle.gameObject);

            if (debugLog)
            {
                Debug.Log($"[VehicleMovement] {_vehicle.vehicleId}: 車線変更実行");
            }
        }

        /// <summary>
        /// 指定車線での加速度を推定
        /// </summary>
        private float EstimateAccelerationInLane(Lane lane)
        {
            if (_vehicle == null || lane == null) return 0f;

            // 暫定的な実装：車線の先行車両との距離から推定
            float t = lane.GetTFromWorldPosition(_vehicle.transform.position);
            var leader = lane.GetLeadVehicle(t, null);

            if (leader == null)
            {
                return _vehicle.acceleration;
            }

            float leaderT = lane.GetTFromWorldPosition(leader.transform.position);
            float gap = (leaderT - t) * lane.length;

            var leaderVehicle = leader.GetComponent<Vehicle>();
            float leaderSpeed = leaderVehicle != null ? leaderVehicle.currentSpeed : 0f;
            float deltaV = _vehicle.currentSpeed - leaderSpeed;

            return CalculateIDMAcceleration(
                _vehicle.currentSpeed,
                _vehicle.MaxSpeedMs,
                _vehicle.acceleration,
                gap,
                deltaV
            );
        }

        /// <summary>
        /// 後続車の加速度を取得
        /// </summary>
        private float GetFollowerAcceleration(Lane lane)
        {
            if (_vehicle == null || lane == null) return 0f;

            var follower = lane.GetFollowingVehicle(_vehicle.positionOnLane, _vehicle.gameObject);
            if (follower == null) return 0f;

            var followerVehicle = follower.GetComponent<Vehicle>();
            if (followerVehicle == null) return 0f;

            var followerController = follower.GetComponent<VehicleMovementController>();
            return followerController != null ? followerController.CalculateAcceleration(followerVehicle) : 0f;
        }

        /// <summary>
        /// 車線変更後の目標車線の後続車加速度を推定
        /// </summary>
        private float EstimateFollowerAccelerationAfterChange(Lane targetLane)
        {
            if (_vehicle == null || targetLane == null) return 0f;

            float t = targetLane.GetTFromWorldPosition(_vehicle.transform.position);
            var follower = targetLane.GetFollowingVehicle(t, null);

            if (follower == null) return 0f;

            var followerVehicle = follower.GetComponent<Vehicle>();
            if (followerVehicle == null) return 0f;

            // 車線変更後の距離を計算
            float followerT = targetLane.GetTFromWorldPosition(follower.transform.position);
            float gap = (t - followerT) * targetLane.length;
            float deltaV = followerVehicle.currentSpeed - _vehicle.currentSpeed;

            return CalculateIDMAcceleration(
                followerVehicle.currentSpeed,
                followerVehicle.MaxSpeedMs,
                followerVehicle.acceleration,
                gap,
                deltaV
            );
        }

        /// <summary>
        /// 追い越しの必要性を判定
        /// </summary>
        public bool ShouldOvertake()
        {
            if (_vehicle == null || _vehicle.currentRoad == null) return false;

            // 前方車両が遅い場合
            float leaderSpeed = _vehicle.GetLeaderSpeed();
            if (leaderSpeed > 0 && leaderSpeed < _vehicle.currentSpeed * 0.7f)
            {
                // 隣の車線があるか確認
                var lanes = _vehicle.currentRoad.GetLanesByDirection(_vehicle.currentLane.direction);
                foreach (var lane in lanes)
                {
                    if (lane != _vehicle.currentLane && ShouldChangeLane(lane))
                    {
                        return true;
                    }
                }
            }

            return false;
        }
    }
}
