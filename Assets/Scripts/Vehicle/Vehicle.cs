using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using RoadNetwork;
using LLM;

namespace Vehicle
{
    /// <summary>
    /// 車両タイプ
    /// </summary>
    public enum VehicleType
    {
        Sedan,      // セダン
        SUV,        // SUV
        Minivan,    // ミニバン
        Truck       // トラック
    }

    /// <summary>
    /// 車両の状態
    /// </summary>
    public enum VehicleState
    {
        Idle,       // 待機中
        Driving,    // 走行中
        Waiting,    // 信号待ち等
        Parking,    // 駐車中
        Evacuated   // 避難完了
    }

    /// <summary>
    /// 車両エージェントクラス
    /// Evacuee.csと並列構造で車両の避難行動を制御
    /// </summary>
    public class Vehicle : MonoBehaviour
    {
        [Header("車両識別")]
        [Tooltip("車両ID")]
        public string vehicleId;

        [Tooltip("車両タイプ")]
        public VehicleType type = VehicleType.Sedan;

        [Header("車両スペック")]
        [Tooltip("乗車定員")]
        public int passengerCapacity = 5;

        [Tooltip("現在の乗車人数（運転者含む）")]
        public int currentPassengers = 1;

        [Tooltip("車両の長さ（メートル）")]
        public float vehicleLength = 4.5f;

        [Tooltip("車両の幅（メートル）")]
        public float vehicleWidth = 1.8f;

        [Header("移動目標")]
        [Tooltip("目標の避難所")]
        public Shelter targetShelter;

        [Tooltip("計画された経路")]
        public List<RoadSegment> plannedPath = new List<RoadSegment>();

        [Tooltip("経路上の現在インデックス")]
        public int currentPathIndex = 0;

        [Header("移動設定")]
        [Tooltip("最高速度（km/h）")]
        public float maxSpeedKmh = 50f;

        [Tooltip("加速度（m/s²）")]
        public float acceleration = 2.5f;

        [Tooltip("減速度（m/s²）")]
        public float deceleration = 4.5f;

        [Tooltip("現在速度（m/s）")]
        public float currentSpeed = 0f;

        [Header("車線状態")]
        [Tooltip("現在走行中の道路")]
        public RoadSegment currentRoad;

        [Tooltip("現在走行中の車線")]
        public Lane currentLane;

        [Tooltip("車線上の位置（0-1）")]
        [Range(0f, 1f)]
        public float positionOnLane = 0f;

        [Header("ステータス")]
        [Tooltip("車両の状態")]
        public VehicleState state = VehicleState.Idle;

        [Tooltip("燃料レベル（0-1）")]
        [Range(0f, 1f)]
        public float fuelLevel = 1.0f;

        [Tooltip("ストレスレベル（0-1）")]
        [Range(0f, 1f)]
        public float stressLevel = 0.0f;

        [Header("LLM意思決定")]
        [Tooltip("LLMによる意思決定を使用するか")]
        public bool useLLMDecision = false;

        [Tooltip("LLM決定クライアント")]
        public LLMDecisionClient decisionClient;

        [Tooltip("最寄り避難所へのフォールバック")]
        public bool fallbackToNearest = true;

        [Tooltip("LLM再呼び出し間隔（秒）")]
        public float llmDecisionInterval = 120f;

        // 内部状態
        private VehicleMovementController _movementController;
        private VehiclePathfinder _pathfinder;
        private RoadNetworkManager _roadNetwork;
        private CancellationTokenSource _llmCts;
        private bool _isRequestingLLMDecision;
        private float _lastLLMDecisionTime;
        private List<string> _excludeShelters = new List<string>();
        private bool _isEvacuating = false;

        /// <summary>
        /// 最高速度をm/sで取得
        /// </summary>
        public float MaxSpeedMs => maxSpeedKmh / 3.6f;

        /// <summary>
        /// 現在速度をkm/hで取得
        /// </summary>
        public float CurrentSpeedKmh => currentSpeed * 3.6f;

        private void Awake()
        {
            // コンポーネントを取得または追加
            _movementController = GetComponent<VehicleMovementController>();
            if (_movementController == null)
            {
                _movementController = gameObject.AddComponent<VehicleMovementController>();
            }

            _pathfinder = GetComponent<VehiclePathfinder>();
            if (_pathfinder == null)
            {
                _pathfinder = gameObject.AddComponent<VehiclePathfinder>();
            }

            // ネットワークマネージャーを取得
            _roadNetwork = RoadNetworkManager.Instance;

            // LLMクライアントを検索
            if (useLLMDecision && decisionClient == null)
            {
                decisionClient = FindFirstObjectByType<LLMDecisionClient>();
            }
        }

        private void Start()
        {
            // IDが未設定の場合は生成
            if (string.IsNullOrEmpty(vehicleId))
            {
                vehicleId = Guid.NewGuid().ToString().Substring(0, 8);
            }
        }

        private void OnDestroy()
        {
            _llmCts?.Cancel();
            _llmCts?.Dispose();
            _llmCts = null;
        }

        private void Update()
        {
            if (state != VehicleState.Driving && state != VehicleState.Waiting)
            {
                return;
            }

            // 経路に沿って移動
            UpdateMovement();

            // LLM再呼び出しのチェック
            CheckLLMReevaluation();
        }

        /// <summary>
        /// 車両IDを設定
        /// </summary>
        public void SetVehicleId(int id)
        {
            vehicleId = id.ToString();
        }

        /// <summary>
        /// 目的地を設定して移動開始
        /// </summary>
        public void SetDestination(Shelter shelter)
        {
            targetShelter = shelter;

            if (shelter == null)
            {
                Debug.LogWarning($"[Vehicle] {vehicleId}: 目的地がnullです");
                return;
            }

            // 現在位置から目的地への経路を計算
            Vector3 destination = shelter.transform.position;
            plannedPath = _pathfinder.FindPath(transform.position, destination);

            if (plannedPath.Count > 0)
            {
                currentPathIndex = 0;
                currentRoad = plannedPath[0];
                currentLane = currentRoad.GetNearestLane(transform.position, LaneDirection.Forward);
                positionOnLane = currentLane?.GetTFromWorldPosition(transform.position) ?? 0f;
                state = VehicleState.Driving;

                // 車線に車両を登録
                currentLane?.AddVehicle(gameObject);

                Debug.Log($"[Vehicle] {vehicleId}: 経路設定完了 - {plannedPath.Count}セグメント, 目的地: {shelter.displayName}");
            }
            else
            {
                Debug.LogWarning($"[Vehicle] {vehicleId}: 経路が見つかりません");
            }
        }

        /// <summary>
        /// 移動を更新
        /// </summary>
        private void UpdateMovement()
        {
            if (currentRoad == null || currentLane == null || state != VehicleState.Driving)
            {
                return;
            }

            // MovementControllerから加速度を取得
            float accel = _movementController.CalculateAcceleration(this);

            // 速度を更新
            currentSpeed += accel * Time.deltaTime;
            currentSpeed = Mathf.Clamp(currentSpeed, 0f, MaxSpeedMs);

            // 車線上の位置を更新
            float moveDistance = currentSpeed * Time.deltaTime;
            float tDelta = currentLane.length > 0 ? moveDistance / currentLane.length : 0f;
            positionOnLane += tDelta;

            // 車線の終端に達した場合
            if (positionOnLane >= 1f)
            {
                AdvanceToNextSegment();
            }
            else
            {
                // 位置と回転を更新
                UpdateTransform();
            }
        }

        /// <summary>
        /// 次の道路セグメントに進む
        /// </summary>
        private void AdvanceToNextSegment()
        {
            // 現在の車線から離脱
            currentLane?.RemoveVehicle(gameObject);

            currentPathIndex++;

            if (currentPathIndex >= plannedPath.Count)
            {
                // 目的地に到着
                OnReachedDestination();
                return;
            }

            // 次のセグメントに移動
            currentRoad = plannedPath[currentPathIndex];
            currentLane = currentRoad.GetNearestLane(transform.position, LaneDirection.Forward);
            positionOnLane = 0f;

            // 新しい車線に登録
            currentLane?.AddVehicle(gameObject);

            UpdateTransform();
        }

        /// <summary>
        /// Transformを更新
        /// </summary>
        private void UpdateTransform()
        {
            if (currentLane == null) return;

            Vector3 position = currentLane.GetPositionAtT(positionOnLane);
            Vector3 direction = currentLane.GetDirectionAtT(positionOnLane);

            transform.position = position;
            if (direction != Vector3.zero)
            {
                transform.rotation = Quaternion.LookRotation(direction);
            }
        }

        /// <summary>
        /// 目的地に到達した時の処理
        /// </summary>
        private void OnReachedDestination()
        {
            if (targetShelter == null)
            {
                state = VehicleState.Idle;
                return;
            }

            // 避難処理を試行
            Evacuation(targetShelter);
        }

        /// <summary>
        /// 避難処理
        /// </summary>
        public void Evacuation(Shelter shelter)
        {
            if (_isEvacuating)
            {
                return;
            }
            _isEvacuating = true;

            // 容量チェック
            if (shelter.currentCapacity >= currentPassengers)
            {
                // 避難成功
                shelter.NowAccCount += currentPassengers;
                state = VehicleState.Evacuated;

                // 車線から離脱
                currentLane?.RemoveVehicle(gameObject);

                gameObject.SetActive(false);
                Debug.Log($"[Vehicle] {vehicleId}: 避難完了 - {currentPassengers}人が{shelter.displayName}に避難");
            }
            else
            {
                // 容量不足 - 別の避難所を探す
                _excludeShelters.Add(shelter.uuid);

                if (useLLMDecision && decisionClient != null)
                {
                    RequestLLMDecision();
                }
                else
                {
                    MoveToNearestShelter();
                }
            }

            _isEvacuating = false;
        }

        /// <summary>
        /// LLM再評価をチェック
        /// </summary>
        private void CheckLLMReevaluation()
        {
            if (!useLLMDecision || decisionClient == null || _lastLLMDecisionTime <= 0f)
            {
                return;
            }

            float elapsed = Time.time - _lastLLMDecisionTime;
            if (elapsed >= llmDecisionInterval && !_isRequestingLLMDecision)
            {
                Debug.Log($"[Vehicle] {vehicleId}: 定期LLM再呼び出し（経過時間: {elapsed:F2}秒）");
                RequestLLMDecision();
            }
        }

        /// <summary>
        /// LLM意思決定を要求
        /// </summary>
        public void RequestLLMDecision()
        {
            if (_isRequestingLLMDecision)
            {
                return;
            }

            _ = DecideAndMoveAsync();
        }

        private async Task DecideAndMoveAsync()
        {
            _isRequestingLLMDecision = true;
            _llmCts?.Cancel();
            _llmCts = new CancellationTokenSource();

            try
            {
                // 暫定: Evacueeと同じリクエスト形式を使用
                // Phase 6でVehicle専用のリクエストに置き換え
                var response = await RequestEvacueeDecisionAsync(_llmCts.Token);

                if (!ApplyLLMDecision(response))
                {
                    if (fallbackToNearest)
                    {
                        Debug.Log($"[Vehicle] {vehicleId}: LLM応答無効、最寄り避難所へフォールバック");
                        MoveToNearestShelter();
                    }
                }
                _lastLLMDecisionTime = Time.time;
            }
            catch (OperationCanceledException)
            {
                Debug.LogWarning($"[Vehicle] {vehicleId}: LLM決定がキャンセルされました");
                if (fallbackToNearest)
                {
                    MoveToNearestShelter();
                }
                _lastLLMDecisionTime = Time.time;
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[Vehicle] {vehicleId}: LLM決定失敗 - {ex.Message}");
                if (fallbackToNearest)
                {
                    MoveToNearestShelter();
                }
                _lastLLMDecisionTime = Time.time;
            }
            finally
            {
                _isRequestingLLMDecision = false;
            }
        }

        /// <summary>
        /// LLM決定を適用
        /// </summary>
        private bool ApplyLLMDecision(LLMEvacDecisionResponse response)
        {
            if (response == null || string.IsNullOrEmpty(response.selected_shelter_id))
            {
                return false;
            }

            // 避難所を検索
            var shelters = GameObject.FindGameObjectsWithTag("Shelter");
            Shelter selectedShelter = null;

            foreach (var shelterObj in shelters)
            {
                var shelter = shelterObj.GetComponent<Shelter>();
                if (shelter != null && shelter.displayName == response.selected_shelter_id)
                {
                    selectedShelter = shelter;
                    break;
                }
            }

            if (selectedShelter == null)
            {
                Debug.LogWarning($"[Vehicle] {vehicleId}: 選択された避難所が見つかりません: {response.selected_shelter_id}");
                return false;
            }

            SetDestination(selectedShelter);
            Debug.Log($"[Vehicle] {vehicleId}: LLMにより{selectedShelter.displayName}を選択");
            return true;
        }

        /// <summary>
        /// 暫定的なLLMリクエスト（Evacueeと同じ形式）
        /// </summary>
        private async Task<LLMEvacDecisionResponse> RequestEvacueeDecisionAsync(CancellationToken ct)
        {
            // 暫定実装：Phase 6で車両専用リクエストに置き換え
            var request = new LLMEvacDecisionRequest
            {
                request_id = $"vehicle-{vehicleId}-{Guid.NewGuid()}",
                timestamp = Time.time,
                evacuee = new EvacueePayload
                {
                    id = vehicleId,
                    position = new Vector3Payload(transform.position)
                },
                shelter_candidates = BuildShelterCandidates(),
                self_state = new SelfStatePayload
                {
                    position = new Vector3Payload(transform.position),
                    velocity = new Vector3Payload(transform.forward * currentSpeed),
                    energy_level = fuelLevel,
                    stress_level = stressLevel,
                    stamina = 1.0f,
                    current_goal = targetShelter?.displayName
                }
            };

            return await decisionClient.RequestEvacueeDecisionAsync(request, ct);
        }

        /// <summary>
        /// 避難所候補を構築
        /// </summary>
        private ShelterCandidatePayload[] BuildShelterCandidates()
        {
            var shelters = GameObject.FindGameObjectsWithTag("Shelter");
            var candidates = new List<ShelterCandidatePayload>();

            foreach (var shelterObj in shelters)
            {
                var shelter = shelterObj.GetComponent<Shelter>();
                if (shelter == null) continue;
                if (_excludeShelters.Contains(shelter.uuid)) continue;

                // 経路距離と所要時間を計算
                var path = _pathfinder.FindPath(transform.position, shelterObj.transform.position);
                float distance = _roadNetwork.CalculatePathLength(path);
                float travelTime = _roadNetwork.CalculatePathTravelTime(path);

                candidates.Add(new ShelterCandidatePayload
                {
                    id = shelterObj.name,
                    display_name = shelter.displayName,
                    description = shelter.description,
                    position = new Vector3Payload(shelterObj.transform.position),
                    current_capacity = shelter.currentCapacity,
                    max_capacity = shelter.MaxCapacity,
                    distance_meters = distance,
                    walking_time_minutes = travelTime / 60f
                });
            }

            return candidates.ToArray();
        }

        /// <summary>
        /// 最寄りの避難所に移動
        /// </summary>
        public void MoveToNearestShelter()
        {
            var shelters = GameObject.FindGameObjectsWithTag("Shelter");
            Shelter nearest = null;
            float minDistance = float.MaxValue;

            foreach (var shelterObj in shelters)
            {
                var shelter = shelterObj.GetComponent<Shelter>();
                if (shelter == null) continue;
                if (_excludeShelters.Contains(shelter.uuid)) continue;

                float dist = Vector3.Distance(transform.position, shelterObj.transform.position);
                if (dist < minDistance)
                {
                    minDistance = dist;
                    nearest = shelter;
                }
            }

            if (nearest != null)
            {
                SetDestination(nearest);
                Debug.Log($"[Vehicle] {vehicleId}: 最寄り避難所{nearest.displayName}に向かいます（距離: {minDistance:F0}m）");
            }
            else
            {
                Debug.LogWarning($"[Vehicle] {vehicleId}: 利用可能な避難所が見つかりません");
            }
        }

        /// <summary>
        /// 速度を設定
        /// </summary>
        public void SetSpeed(float speedKmh)
        {
            maxSpeedKmh = Mathf.Clamp(speedKmh, 0f, 120f);
        }

        /// <summary>
        /// 停止
        /// </summary>
        public void Stop()
        {
            currentSpeed = 0f;
            state = VehicleState.Waiting;
        }

        /// <summary>
        /// 走行再開
        /// </summary>
        public void Resume()
        {
            if (state == VehicleState.Waiting)
            {
                state = VehicleState.Driving;
            }
        }

        /// <summary>
        /// 前方車両との距離を取得
        /// </summary>
        public float GetDistanceToLeader()
        {
            if (currentLane == null) return float.MaxValue;

            var leader = currentLane.GetLeadVehicle(positionOnLane, gameObject);
            if (leader == null) return float.MaxValue;

            float leaderT = currentLane.GetTFromWorldPosition(leader.transform.position);
            float gapT = leaderT - positionOnLane;
            return gapT * currentLane.length;
        }

        /// <summary>
        /// 前方車両の速度を取得
        /// </summary>
        public float GetLeaderSpeed()
        {
            if (currentLane == null) return 0f;

            var leader = currentLane.GetLeadVehicle(positionOnLane, gameObject);
            if (leader == null) return 0f;

            var leaderVehicle = leader.GetComponent<Vehicle>();
            return leaderVehicle != null ? leaderVehicle.currentSpeed : 0f;
        }

        /// <summary>
        /// デバッグ情報を表示
        /// </summary>
        private void OnDrawGizmosSelected()
        {
            // 現在位置
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(transform.position, 1f);

            // 目的地
            if (targetShelter != null)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawLine(transform.position, targetShelter.transform.position);
            }

            // 経路
            if (plannedPath != null && plannedPath.Count > 0)
            {
                Gizmos.color = Color.cyan;
                for (int i = currentPathIndex; i < plannedPath.Count - 1; i++)
                {
                    if (plannedPath[i] != null && plannedPath[i + 1] != null)
                    {
                        Gizmos.DrawLine(plannedPath[i].CenterPosition, plannedPath[i + 1].CenterPosition);
                    }
                }
            }
        }
    }
}
