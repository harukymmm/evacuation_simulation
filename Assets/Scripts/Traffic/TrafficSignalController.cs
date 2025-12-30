using System;
using System.Collections.Generic;
using UnityEngine;
using RoadNetwork;

namespace Traffic
{
    /// <summary>
    /// 信号の状態
    /// </summary>
    public enum SignalState
    {
        Red,
        Yellow,
        Green,
        ArrowLeft,      // 左折矢印
        ArrowRight,     // 右折矢印
        ArrowStraight   // 直進矢印
    }

    /// <summary>
    /// 信号フェーズの定義
    /// </summary>
    [Serializable]
    public class SignalPhase
    {
        [Tooltip("青信号の継続時間（秒）")]
        public float greenDuration = 30f;

        [Tooltip("この方向で青になる道路のインデックス")]
        public List<int> greenDirections = new List<int>();

        [Tooltip("矢印信号を表示する方向")]
        public List<int> arrowDirections = new List<int>();

        [Tooltip("矢印のタイプ")]
        public TurnDirection arrowType = TurnDirection.Left;
    }

    /// <summary>
    /// 交差点の信号制御クラス
    /// </summary>
    public class TrafficSignalController : MonoBehaviour
    {
        [Header("信号設定")]
        [Tooltip("サイクル全体の時間（秒）")]
        public float cycleDuration = 90f;

        [Tooltip("黄信号の継続時間（秒）")]
        public float yellowDuration = 3f;

        [Tooltip("全赤時間（秒）- 安全クリアランス")]
        public float allRedDuration = 2f;

        [Header("フェーズ設定")]
        [Tooltip("信号フェーズのリスト")]
        public List<SignalPhase> phases = new List<SignalPhase>();

        [Tooltip("現在のフェーズインデックス")]
        public int currentPhaseIndex = 0;

        [Header("状態")]
        [Tooltip("フェーズタイマー")]
        public float phaseTimer = 0f;

        [Tooltip("現在のサブ状態")]
        public SignalSubState currentSubState = SignalSubState.Green;

        [Header("参照")]
        [Tooltip("対象の交差点")]
        public Intersection intersection;

        [Tooltip("信号表示オブジェクト")]
        public List<TrafficSignal> signalDisplays = new List<TrafficSignal>();

        // 各方向の信号状態をキャッシュ
        private Dictionary<int, SignalState> _directionStates = new Dictionary<int, SignalState>();

        /// <summary>
        /// サブ状態（フェーズ内の青→黄→全赤の遷移）
        /// </summary>
        public enum SignalSubState
        {
            Green,
            Yellow,
            AllRed
        }

        private void Awake()
        {
            // 交差点への参照を設定
            if (intersection == null)
            {
                intersection = GetComponent<Intersection>();
            }

            if (intersection != null)
            {
                intersection.signalController = this;
                intersection.hasSignal = true;
            }

            // デフォルトフェーズを設定
            if (phases.Count == 0)
            {
                InitializeDefaultPhases();
            }
        }

        private void Start()
        {
            // 初期状態を設定
            UpdateSignalStates();
        }

        private void Update()
        {
            UpdateSignal(Time.deltaTime);
        }

        /// <summary>
        /// デフォルトの信号フェーズを初期化
        /// </summary>
        private void InitializeDefaultPhases()
        {
            if (intersection == null) return;

            int roadCount = intersection.connectedRoads.Count;

            if (roadCount <= 2)
            {
                // 2方向以下の場合は常に青
                phases.Add(new SignalPhase
                {
                    greenDuration = cycleDuration,
                    greenDirections = new List<int> { 0, 1 }
                });
            }
            else if (roadCount == 3)
            {
                // T字路: 2フェーズ
                phases.Add(new SignalPhase
                {
                    greenDuration = 40f,
                    greenDirections = new List<int> { 0, 2 }
                });
                phases.Add(new SignalPhase
                {
                    greenDuration = 40f,
                    greenDirections = new List<int> { 1 }
                });
            }
            else
            {
                // 十字路以上: 対向する方向をペアに
                phases.Add(new SignalPhase
                {
                    greenDuration = 40f,
                    greenDirections = new List<int> { 0, 2 }
                });
                phases.Add(new SignalPhase
                {
                    greenDuration = 40f,
                    greenDirections = new List<int> { 1, 3 }
                });
            }
        }

        /// <summary>
        /// 信号状態を更新
        /// </summary>
        public void UpdateSignal(float deltaTime)
        {
            phaseTimer += deltaTime;

            var currentPhase = phases[currentPhaseIndex];
            float phaseDuration = currentPhase.greenDuration;

            switch (currentSubState)
            {
                case SignalSubState.Green:
                    if (phaseTimer >= phaseDuration)
                    {
                        // 黄信号に遷移
                        currentSubState = SignalSubState.Yellow;
                        phaseTimer = 0f;
                        UpdateSignalStates();
                    }
                    break;

                case SignalSubState.Yellow:
                    if (phaseTimer >= yellowDuration)
                    {
                        // 全赤に遷移
                        currentSubState = SignalSubState.AllRed;
                        phaseTimer = 0f;
                        UpdateSignalStates();
                    }
                    break;

                case SignalSubState.AllRed:
                    if (phaseTimer >= allRedDuration)
                    {
                        // 次のフェーズに遷移
                        currentPhaseIndex = (currentPhaseIndex + 1) % phases.Count;
                        currentSubState = SignalSubState.Green;
                        phaseTimer = 0f;
                        UpdateSignalStates();
                    }
                    break;
            }
        }

        /// <summary>
        /// 各方向の信号状態を更新
        /// </summary>
        private void UpdateSignalStates()
        {
            _directionStates.Clear();

            if (intersection == null) return;

            var currentPhase = phases[currentPhaseIndex];
            int roadCount = intersection.connectedRoads.Count;

            for (int i = 0; i < roadCount; i++)
            {
                SignalState state;

                if (currentSubState == SignalSubState.AllRed)
                {
                    state = SignalState.Red;
                }
                else if (currentPhase.greenDirections.Contains(i))
                {
                    state = currentSubState == SignalSubState.Green ? SignalState.Green : SignalState.Yellow;
                }
                else if (currentSubState == SignalSubState.Green && currentPhase.arrowDirections.Contains(i))
                {
                    // 矢印信号
                    state = currentPhase.arrowType switch
                    {
                        TurnDirection.Left => SignalState.ArrowLeft,
                        TurnDirection.Right => SignalState.ArrowRight,
                        _ => SignalState.ArrowStraight
                    };
                }
                else
                {
                    state = SignalState.Red;
                }

                _directionStates[i] = state;
            }

            // 信号表示を更新
            UpdateSignalDisplays();
        }

        /// <summary>
        /// 信号表示オブジェクトを更新
        /// </summary>
        private void UpdateSignalDisplays()
        {
            for (int i = 0; i < signalDisplays.Count; i++)
            {
                if (signalDisplays[i] != null && _directionStates.ContainsKey(i))
                {
                    signalDisplays[i].SetState(_directionStates[i]);
                }
            }
        }

        /// <summary>
        /// 指定道路からの進入に対する信号状態を取得
        /// </summary>
        public SignalState GetStateForRoad(RoadSegment road)
        {
            if (intersection == null || road == null)
            {
                return SignalState.Red;
            }

            int roadIndex = intersection.connectedRoads.IndexOf(road);
            if (roadIndex < 0)
            {
                return SignalState.Red;
            }

            return _directionStates.TryGetValue(roadIndex, out var state) ? state : SignalState.Red;
        }

        /// <summary>
        /// 指定方向からの進入に対する信号状態を取得
        /// </summary>
        public SignalState GetStateForDirection(Vector3 approachDirection)
        {
            if (intersection == null)
            {
                return SignalState.Red;
            }

            // 最も近い方向の道路を見つける
            int bestIndex = -1;
            float bestDot = -2f;

            for (int i = 0; i < intersection.connectedRoads.Count; i++)
            {
                var road = intersection.connectedRoads[i];
                if (road == null) continue;

                // 道路から交差点への方向
                Vector3 roadDir = (intersection.position - road.CenterPosition).normalized;
                float dot = Vector3.Dot(approachDirection.normalized, roadDir);

                if (dot > bestDot)
                {
                    bestDot = dot;
                    bestIndex = i;
                }
            }

            if (bestIndex >= 0 && _directionStates.TryGetValue(bestIndex, out var state))
            {
                return state;
            }

            return SignalState.Red;
        }

        /// <summary>
        /// 信号が青または矢印かどうかを判定
        /// </summary>
        public bool IsGreenOrArrow(RoadSegment road)
        {
            var state = GetStateForRoad(road);
            return state == SignalState.Green ||
                   state == SignalState.ArrowLeft ||
                   state == SignalState.ArrowRight ||
                   state == SignalState.ArrowStraight;
        }

        /// <summary>
        /// 青信号までの推定待ち時間を取得
        /// </summary>
        public float EstimateWaitTime(RoadSegment road)
        {
            if (IsGreenOrArrow(road))
            {
                return 0f;
            }

            int roadIndex = intersection?.connectedRoads.IndexOf(road) ?? -1;
            if (roadIndex < 0)
            {
                return cycleDuration / 2f; // 不明な場合は平均値
            }

            // 現在のフェーズからこの方向が青になるフェーズまでの時間を計算
            float waitTime = 0f;
            int checkIndex = currentPhaseIndex;

            // 現在のフェーズの残り時間
            switch (currentSubState)
            {
                case SignalSubState.Green:
                    waitTime += phases[checkIndex].greenDuration - phaseTimer;
                    waitTime += yellowDuration + allRedDuration;
                    break;
                case SignalSubState.Yellow:
                    waitTime += yellowDuration - phaseTimer;
                    waitTime += allRedDuration;
                    break;
                case SignalSubState.AllRed:
                    waitTime += allRedDuration - phaseTimer;
                    break;
            }

            // 次のフェーズ以降を確認
            for (int i = 0; i < phases.Count; i++)
            {
                checkIndex = (currentPhaseIndex + 1 + i) % phases.Count;

                if (phases[checkIndex].greenDirections.Contains(roadIndex))
                {
                    return waitTime;
                }

                waitTime += phases[checkIndex].greenDuration + yellowDuration + allRedDuration;
            }

            return waitTime;
        }

        /// <summary>
        /// サイクル内の現在の進行度（0-1）
        /// </summary>
        public float GetCycleProgress()
        {
            float elapsed = 0f;

            // 現在のフェーズまでの経過時間
            for (int i = 0; i < currentPhaseIndex; i++)
            {
                elapsed += phases[i].greenDuration + yellowDuration + allRedDuration;
            }

            // 現在のフェーズ内の経過時間
            elapsed += phaseTimer;
            switch (currentSubState)
            {
                case SignalSubState.Yellow:
                    elapsed += phases[currentPhaseIndex].greenDuration;
                    break;
                case SignalSubState.AllRed:
                    elapsed += phases[currentPhaseIndex].greenDuration + yellowDuration;
                    break;
            }

            return elapsed / cycleDuration;
        }

        /// <summary>
        /// 信号状態のデバッグ情報を取得
        /// </summary>
        public string GetDebugInfo()
        {
            string info = $"Phase {currentPhaseIndex + 1}/{phases.Count} ({currentSubState})";
            info += $"\nTimer: {phaseTimer:F1}s";
            info += $"\nCycle: {GetCycleProgress() * 100:F0}%";

            foreach (var kvp in _directionStates)
            {
                info += $"\nDir{kvp.Key}: {kvp.Value}";
            }

            return info;
        }

        /// <summary>
        /// デバッグ用Gizmo描画
        /// </summary>
        private void OnDrawGizmosSelected()
        {
            if (intersection == null) return;

            for (int i = 0; i < intersection.connectedRoads.Count; i++)
            {
                var road = intersection.connectedRoads[i];
                if (road == null) continue;

                // 信号状態に応じた色
                if (_directionStates.TryGetValue(i, out var state))
                {
                    Gizmos.color = state switch
                    {
                        SignalState.Green => Color.green,
                        SignalState.Yellow => Color.yellow,
                        SignalState.Red => Color.red,
                        SignalState.ArrowLeft => new Color(0, 1, 0.5f),
                        SignalState.ArrowRight => new Color(0, 1, 0.5f),
                        _ => Color.white
                    };
                }
                else
                {
                    Gizmos.color = Color.gray;
                }

                Vector3 dir = (road.CenterPosition - intersection.position).normalized;
                Vector3 signalPos = intersection.position + dir * (intersection.radius + 2f);
                Gizmos.DrawSphere(signalPos, 1f);
            }
        }
    }
}
