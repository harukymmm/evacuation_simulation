using System;
using System.Collections.Generic;
using UnityEngine;

namespace RoadNetwork
{
    /// <summary>
    /// 交差点のタイプ
    /// </summary>
    public enum IntersectionType
    {
        T_Junction,     // T字路
        Cross,          // 十字路
        Roundabout,     // ラウンドアバウト
        Merge,          // 合流
        Split,          // 分岐
        DeadEnd         // 行き止まり
    }

    /// <summary>
    /// 無信号交差点の優先ルール
    /// </summary>
    public enum PriorityRule
    {
        PriorityRight,  // 右方優先（日本では左方優先）
        PriorityMain,   // 主道路優先
        YieldAll,       // 全て譲る
        FirstComeFirstServed  // 先着順
    }

    /// <summary>
    /// 進入方向から退出方向への移動を表すキー
    /// </summary>
    [Serializable]
    public struct TurnKey : IEquatable<TurnKey>
    {
        public RoadSegment fromRoad;
        public RoadSegment toRoad;

        public TurnKey(RoadSegment from, RoadSegment to)
        {
            fromRoad = from;
            toRoad = to;
        }

        public bool Equals(TurnKey other)
        {
            return fromRoad == other.fromRoad && toRoad == other.toRoad;
        }

        public override bool Equals(object obj)
        {
            return obj is TurnKey other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(fromRoad, toRoad);
        }
    }

    /// <summary>
    /// 曲がり方向
    /// </summary>
    public enum TurnDirection
    {
        Straight,   // 直進
        Left,       // 左折
        Right,      // 右折
        UTurn       // Uターン
    }

    /// <summary>
    /// 交差点クラス
    /// 道路ネットワークのノードとして機能
    /// </summary>
    public class Intersection : MonoBehaviour
    {
        [Header("識別情報")]
        [Tooltip("交差点ID")]
        public string intersectionId;

        [Header("ジオメトリ")]
        [Tooltip("交差点の中心位置")]
        public Vector3 position;

        [Tooltip("交差点の半径（バウンディング）")]
        public float radius = 10f;

        [Tooltip("交差点のタイプ")]
        public IntersectionType type = IntersectionType.Cross;

        [Header("接続情報")]
        [Tooltip("接続している道路")]
        public List<RoadSegment> connectedRoads = new List<RoadSegment>();

        [Header("信号制御")]
        [Tooltip("信号があるかどうか")]
        public bool hasSignal = false;

        [Tooltip("信号コントローラへの参照")]
        // TrafficSignalControllerはPhase 3で実装するため、暫定的にMonoBehaviourとして宣言
        public MonoBehaviour signalController;

        [Header("優先ルール")]
        [Tooltip("無信号交差点の優先ルール")]
        public PriorityRule priorityRule = PriorityRule.PriorityMain;

        [Tooltip("主道路（優先道路）")]
        public List<RoadSegment> mainRoads = new List<RoadSegment>();

        // 許可された進行方向のマッピング
        private Dictionary<TurnKey, TurnDirection> _allowedTurns = new Dictionary<TurnKey, TurnDirection>();

        // 交差点内で待機中の車両
        private List<GameObject> _waitingVehicles = new List<GameObject>();

        /// <summary>
        /// 初期化
        /// </summary>
        private void Awake()
        {
            if (position == Vector3.zero)
            {
                position = transform.position;
            }
        }

        /// <summary>
        /// 接続道路から交差点タイプを自動判定
        /// </summary>
        public void DetermineType()
        {
            int roadCount = connectedRoads.Count;

            type = roadCount switch
            {
                1 => IntersectionType.DeadEnd,
                2 => IntersectionType.Merge,  // または直線道路
                3 => IntersectionType.T_Junction,
                4 => IntersectionType.Cross,
                _ => IntersectionType.Cross
            };
        }

        /// <summary>
        /// 接続道路から許可された進行方向を構築
        /// </summary>
        public void BuildAllowedTurns()
        {
            _allowedTurns.Clear();

            foreach (var fromRoad in connectedRoads)
            {
                foreach (var toRoad in connectedRoads)
                {
                    if (fromRoad == toRoad) continue;

                    var turnDirection = CalculateTurnDirection(fromRoad, toRoad);
                    var key = new TurnKey(fromRoad, toRoad);
                    _allowedTurns[key] = turnDirection;
                }
            }
        }

        /// <summary>
        /// 2つの道路間の曲がり方向を計算
        /// </summary>
        private TurnDirection CalculateTurnDirection(RoadSegment fromRoad, RoadSegment toRoad)
        {
            // 進入方向のベクトルを計算
            Vector3 entryDirection = GetEntryDirection(fromRoad);
            // 退出方向のベクトルを計算
            Vector3 exitDirection = GetExitDirection(toRoad);

            // 外積で左右を判定
            float cross = Vector3.Cross(entryDirection, exitDirection).y;
            // 内積で直進・Uターンを判定
            float dot = Vector3.Dot(entryDirection, exitDirection);

            if (dot < -0.7f) // ほぼ逆方向
            {
                return TurnDirection.UTurn;
            }
            else if (dot > 0.7f) // ほぼ同方向
            {
                return TurnDirection.Straight;
            }
            else if (cross > 0) // 左折（日本の場合）
            {
                return TurnDirection.Left;
            }
            else // 右折
            {
                return TurnDirection.Right;
            }
        }

        /// <summary>
        /// 道路から交差点への進入方向を取得
        /// </summary>
        private Vector3 GetEntryDirection(RoadSegment road)
        {
            // 道路の終点がこの交差点に近いか開始点が近いかで判定
            float distToEnd = Vector3.Distance(road.EndPosition, position);
            float distToStart = Vector3.Distance(road.StartPosition, position);

            if (distToEnd < distToStart)
            {
                // 道路の終点から進入 = 道路の進行方向
                if (road.centerLine.Count >= 2)
                {
                    int lastIdx = road.centerLine.Count - 1;
                    return (road.centerLine[lastIdx] - road.centerLine[lastIdx - 1]).normalized;
                }
            }
            else
            {
                // 道路の開始点から進入 = 道路の逆方向
                if (road.centerLine.Count >= 2)
                {
                    return (road.centerLine[0] - road.centerLine[1]).normalized;
                }
            }

            return (position - road.CenterPosition).normalized;
        }

        /// <summary>
        /// 交差点から道路への退出方向を取得
        /// </summary>
        private Vector3 GetExitDirection(RoadSegment road)
        {
            float distToEnd = Vector3.Distance(road.EndPosition, position);
            float distToStart = Vector3.Distance(road.StartPosition, position);

            if (distToStart < distToEnd)
            {
                // 道路の開始点から退出 = 道路の進行方向
                if (road.centerLine.Count >= 2)
                {
                    return (road.centerLine[1] - road.centerLine[0]).normalized;
                }
            }
            else
            {
                // 道路の終点から退出 = 道路の逆方向
                if (road.centerLine.Count >= 2)
                {
                    int lastIdx = road.centerLine.Count - 1;
                    return (road.centerLine[lastIdx - 1] - road.centerLine[lastIdx]).normalized;
                }
            }

            return (road.CenterPosition - position).normalized;
        }

        /// <summary>
        /// 指定の進行が許可されているか確認
        /// </summary>
        public bool IsTurnAllowed(RoadSegment fromRoad, RoadSegment toRoad)
        {
            var key = new TurnKey(fromRoad, toRoad);
            return _allowedTurns.ContainsKey(key);
        }

        /// <summary>
        /// 指定の進行方向を取得
        /// </summary>
        public TurnDirection GetTurnDirection(RoadSegment fromRoad, RoadSegment toRoad)
        {
            var key = new TurnKey(fromRoad, toRoad);
            return _allowedTurns.TryGetValue(key, out var direction) ? direction : TurnDirection.Straight;
        }

        /// <summary>
        /// 指定道路から進入可能な道路リストを取得
        /// </summary>
        public List<RoadSegment> GetPossibleExits(RoadSegment fromRoad)
        {
            var exits = new List<RoadSegment>();
            foreach (var road in connectedRoads)
            {
                if (road != fromRoad)
                {
                    exits.Add(road);
                }
            }
            return exits;
        }

        /// <summary>
        /// 道路が主道路かどうかを判定
        /// </summary>
        public bool IsMainRoad(RoadSegment road)
        {
            return mainRoads.Contains(road);
        }

        /// <summary>
        /// 車両が交差点に進入できるかを判定
        /// </summary>
        public bool CanEnter(GameObject vehicle, RoadSegment fromRoad, RoadSegment toRoad)
        {
            // 信号がある場合
            if (hasSignal && signalController != null)
            {
                // TrafficSignalControllerの実装後に詳細化
                // 暫定的にtrueを返す
                return true;
            }

            // 無信号の場合、優先ルールに従う
            return CheckPriorityRule(vehicle, fromRoad, toRoad);
        }

        /// <summary>
        /// 優先ルールをチェック
        /// </summary>
        private bool CheckPriorityRule(GameObject vehicle, RoadSegment fromRoad, RoadSegment toRoad)
        {
            switch (priorityRule)
            {
                case PriorityRule.PriorityMain:
                    // 主道路からの進入は常に可能
                    if (IsMainRoad(fromRoad)) return true;
                    // 主道路に車両がいない場合のみ進入可能
                    return !HasVehiclesOnMainRoads();

                case PriorityRule.PriorityRight:
                    // 右方（日本では左方）からの車両がいない場合のみ進入可能
                    return !HasVehicleFromRight(vehicle, fromRoad);

                case PriorityRule.YieldAll:
                    // 他に待機車両がいない場合のみ進入可能
                    return _waitingVehicles.Count == 0 || _waitingVehicles[0] == vehicle;

                case PriorityRule.FirstComeFirstServed:
                    // 先着順
                    return _waitingVehicles.Count == 0 || _waitingVehicles[0] == vehicle;

                default:
                    return true;
            }
        }

        /// <summary>
        /// 主道路に車両がいるかチェック
        /// </summary>
        private bool HasVehiclesOnMainRoads()
        {
            foreach (var road in mainRoads)
            {
                if (road.GetTotalVehicleCount() > 0)
                {
                    // 交差点近くの車両のみをカウント
                    foreach (var lane in road.lanes)
                    {
                        foreach (var vehicle in lane.vehiclesInLane)
                        {
                            if (vehicle == null) continue;
                            float dist = Vector3.Distance(vehicle.transform.position, position);
                            if (dist < radius * 3) return true;
                        }
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// 右方から車両が来ているかチェック（日本ルール：左方優先）
        /// </summary>
        private bool HasVehicleFromRight(GameObject currentVehicle, RoadSegment currentRoad)
        {
            // 進入方向のベクトル
            Vector3 myDirection = GetEntryDirection(currentRoad);
            Vector3 rightDirection = Quaternion.Euler(0, -90, 0) * myDirection;

            foreach (var road in connectedRoads)
            {
                if (road == currentRoad) continue;

                Vector3 otherDirection = GetEntryDirection(road);
                float dot = Vector3.Dot(rightDirection, otherDirection);

                // 右方からの道路
                if (dot > 0.5f)
                {
                    // その道路上に交差点に近づいている車両がいるか
                    foreach (var lane in road.lanes)
                    {
                        foreach (var vehicle in lane.vehiclesInLane)
                        {
                            if (vehicle == null || vehicle == currentVehicle) continue;
                            float dist = Vector3.Distance(vehicle.transform.position, position);
                            if (dist < radius * 3) return true;
                        }
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// 車両を待機リストに追加
        /// </summary>
        public void AddWaitingVehicle(GameObject vehicle)
        {
            if (!_waitingVehicles.Contains(vehicle))
            {
                _waitingVehicles.Add(vehicle);
            }
        }

        /// <summary>
        /// 車両を待機リストから削除
        /// </summary>
        public void RemoveWaitingVehicle(GameObject vehicle)
        {
            _waitingVehicles.Remove(vehicle);
        }

        /// <summary>
        /// 道路を接続
        /// </summary>
        public void ConnectRoad(RoadSegment road)
        {
            if (!connectedRoads.Contains(road))
            {
                connectedRoads.Add(road);
                DetermineType();
                BuildAllowedTurns();
            }
        }

        /// <summary>
        /// 点が交差点内にあるかどうかを判定
        /// </summary>
        public bool ContainsPoint(Vector3 point)
        {
            return Vector3.Distance(point, position) <= radius;
        }

        /// <summary>
        /// デバッグ用のGizmo描画
        /// </summary>
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = hasSignal ? Color.red : Color.yellow;
            Gizmos.DrawWireSphere(position != Vector3.zero ? position : transform.position, radius);

            // 接続道路への線を描画
            Gizmos.color = Color.cyan;
            foreach (var road in connectedRoads)
            {
                if (road != null)
                {
                    Gizmos.DrawLine(position, road.CenterPosition);
                }
            }

            // 主道路を強調表示
            Gizmos.color = Color.green;
            foreach (var road in mainRoads)
            {
                if (road != null)
                {
                    Gizmos.DrawLine(position, road.CenterPosition);
                }
            }
        }
    }
}
