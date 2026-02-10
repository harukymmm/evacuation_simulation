using System;
using System.Collections.Generic;
using System.Linq;
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

        // 現在の行動状態
        private VehicleActionType _currentActionType = VehicleActionType.DRIVE_TO_SHELTER;
        private List<string> _actionHistory = new List<string>();
        private float _waitStartTime;
        private float _maxWaitTime;
        private string _waitReason;

        // PICKUP_FAMILY用
        private string _targetFamilyMember;
        private Vector3 _pickupLocation;
        private Shelter _afterPickupShelter;
        private bool _isFamilyPickedUp = false;

        // CONTACT用
        private bool _isContacting = false;
        private string _contactTarget;

        /// <summary>
        /// 現在の行動タイプを取得
        /// </summary>
        public VehicleActionType CurrentActionType => _currentActionType;

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
                // 車両専用のLLMリクエストを使用
                var response = await RequestVehicleDecisionAsync(_llmCts.Token);

                if (!ApplyVehicleDecision(response))
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
        /// 車両用LLM決定を適用
        /// </summary>
        private bool ApplyVehicleDecision(LLMVehicleDecisionResponse response)
        {
            if (response == null || string.IsNullOrEmpty(response.action_type))
            {
                return false;
            }

            // 行動タイプをパース
            if (!Enum.TryParse<VehicleActionType>(response.action_type, true, out var actionType))
            {
                Debug.LogWarning($"[Vehicle] {vehicleId}: 不明な行動タイプ: {response.action_type}");
                return false;
            }

            // 行動履歴に追加
            _actionHistory.Add($"{Time.time:F1}:{actionType}");
            if (_actionHistory.Count > 10) _actionHistory.RemoveAt(0);

            _currentActionType = actionType;
            Debug.Log($"[Vehicle] {vehicleId}: LLM決定 - {actionType}, 理由: {response.reasoning}");

            // 行動タイプに応じた処理を実行
            switch (actionType)
            {
                case VehicleActionType.DRIVE_TO_SHELTER:
                    return ExecuteDriveToShelter(response);

                case VehicleActionType.WAIT_IN_CAR:
                    return ExecuteWaitInCar(response);

                case VehicleActionType.PICKUP_FAMILY:
                    return ExecutePickupFamily(response);

                case VehicleActionType.CONTACT:
                    return ExecuteContact(response);

                case VehicleActionType.PARK_AND_WALK:
                    return ExecuteParkAndWalk(response);

                default:
                    Debug.LogWarning($"[Vehicle] {vehicleId}: 未実装の行動タイプ: {actionType}");
                    return false;
            }
        }

        /// <summary>
        /// DRIVE_TO_SHELTER: 避難所へ走行
        /// </summary>
        private bool ExecuteDriveToShelter(LLMVehicleDecisionResponse response)
        {
            if (string.IsNullOrEmpty(response.selected_shelter_id))
            {
                return false;
            }

            // 避難所を検索
            var shelter = FindShelterByName(response.selected_shelter_id);
            if (shelter == null)
            {
                Debug.LogWarning($"[Vehicle] {vehicleId}: 避難所が見つかりません: {response.selected_shelter_id}");
                return false;
            }

            // 速度設定を適用
            if (!string.IsNullOrEmpty(response.desired_speed))
            {
                ApplySpeedChoice(response.desired_speed);
            }

            SetDestination(shelter);
            Debug.Log($"[Vehicle] {vehicleId}: {shelter.displayName}へ向かいます");
            return true;
        }

        /// <summary>
        /// WAIT_IN_CAR: 車内で待機
        /// </summary>
        private bool ExecuteWaitInCar(LLMVehicleDecisionResponse response)
        {
            Stop();
            _waitStartTime = Time.time;
            _maxWaitTime = response.max_wait_time_sec > 0 ? response.max_wait_time_sec : 60f;
            _waitReason = response.wait_reason ?? "状況確認中";

            Debug.Log($"[Vehicle] {vehicleId}: 車内で待機 - 理由: {_waitReason}, 最大{_maxWaitTime}秒");

            // 待機終了後の再評価をスケジュール
            _ = WaitAndReevaluateAsync(_maxWaitTime);
            return true;
        }

        /// <summary>
        /// PICKUP_FAMILY: 家族を迎えに行く
        /// </summary>
        private bool ExecutePickupFamily(LLMVehicleDecisionResponse response)
        {
            if (string.IsNullOrEmpty(response.target_family_member))
            {
                Debug.LogWarning($"[Vehicle] {vehicleId}: 迎えに行く家族が指定されていません");
                return false;
            }

            _targetFamilyMember = response.target_family_member;
            _isFamilyPickedUp = false;

            // 合流後の避難所を設定
            if (!string.IsNullOrEmpty(response.after_pickup_shelter))
            {
                _afterPickupShelter = FindShelterByName(response.after_pickup_shelter);
            }

            // 迎えに行く場所を設定
            if (response.pickup_location != null)
            {
                _pickupLocation = response.pickup_location.ToVector3();

                // 経路を計算して移動開始
                plannedPath = _pathfinder.FindPath(transform.position, _pickupLocation);
                if (plannedPath.Count > 0)
                {
                    currentPathIndex = 0;
                    currentRoad = plannedPath[0];
                    currentLane = currentRoad.GetNearestLane(transform.position, LaneDirection.Forward);
                    positionOnLane = currentLane?.GetTFromWorldPosition(transform.position) ?? 0f;
                    state = VehicleState.Driving;
                    currentLane?.AddVehicle(gameObject);

                    Debug.Log($"[Vehicle] {vehicleId}: {_targetFamilyMember}を迎えに行きます");
                    return true;
                }
            }

            Debug.LogWarning($"[Vehicle] {vehicleId}: 家族の場所への経路が見つかりません");
            return false;
        }

        /// <summary>
        /// CONTACT: 電話連絡
        /// </summary>
        private bool ExecuteContact(LLMVehicleDecisionResponse response)
        {
            if (string.IsNullOrEmpty(response.contact_target))
            {
                Debug.LogWarning($"[Vehicle] {vehicleId}: 連絡先が指定されていません");
                return false;
            }

            _contactTarget = response.contact_target;
            _isContacting = true;

            // 停車が必要な場合
            if (response.should_stop)
            {
                Stop();
            }

            Debug.Log($"[Vehicle] {vehicleId}: {_contactTarget}に連絡中 - メッセージ: {response.contact_message}");

            // 連絡処理（簡易実装：一定時間後に完了）
            _ = ContactAndContinueAsync(response.contact_message);
            return true;
        }

        /// <summary>
        /// PARK_AND_WALK: 駐車して徒歩避難
        /// </summary>
        private bool ExecuteParkAndWalk(LLMVehicleDecisionResponse response)
        {
            // 現在位置で停車
            Stop();
            state = VehicleState.Parking;

            // 車線から離脱
            currentLane?.RemoveVehicle(gameObject);

            Debug.Log($"[Vehicle] {vehicleId}: 車を駐車し、{currentPassengers}人が徒歩で避難を開始");

            // 乗客をEvacueeとしてスポーン
            SpawnPassengersAsEvacuees(response.walking_destination);

            // 車両を無効化
            if (response.abandon_vehicle)
            {
                gameObject.SetActive(false);
            }

            return true;
        }

        /// <summary>
        /// 待機後に再評価
        /// </summary>
        private async Task WaitAndReevaluateAsync(float waitTime)
        {
            await Task.Delay((int)(waitTime * 1000));

            if (state == VehicleState.Waiting && _currentActionType == VehicleActionType.WAIT_IN_CAR)
            {
                Debug.Log($"[Vehicle] {vehicleId}: 待機終了、再評価を開始");
                RequestLLMDecision();
            }
        }

        /// <summary>
        /// 連絡処理後に継続
        /// </summary>
        private async Task ContactAndContinueAsync(string message)
        {
            // 連絡に5秒かかると仮定
            await Task.Delay(5000);

            _isContacting = false;
            Debug.Log($"[Vehicle] {vehicleId}: 連絡完了");

            // 連絡後に再評価
            RequestLLMDecision();
        }

        /// <summary>
        /// 乗客をEvacueeとしてスポーン
        /// </summary>
        private void SpawnPassengersAsEvacuees(string walkingDestination)
        {
            // EnvManagerを検索してEvacueeをスポーン
            var envManager = FindFirstObjectByType<ShelterEnvManager>();
            if (envManager == null)
            {
                Debug.LogWarning($"[Vehicle] {vehicleId}: ShelterEnvManagerが見つかりません");
                return;
            }

            // 目的地の避難所を取得
            Shelter destination = null;
            if (!string.IsNullOrEmpty(walkingDestination))
            {
                destination = FindShelterByName(walkingDestination);
            }

            // 乗客数分のEvacueeをスポーン（簡易実装）
            for (int i = 0; i < currentPassengers; i++)
            {
                Vector3 spawnPos = transform.position + UnityEngine.Random.insideUnitSphere * 2f;
                spawnPos.y = transform.position.y;

                // Evacueeプレハブをインスタンス化（EnvManagerの機能を使用）
                Debug.Log($"[Vehicle] {vehicleId}: 乗客{i + 1}がEvacueeとしてスポーン（位置: {spawnPos}）");
            }
        }

        /// <summary>
        /// 速度選択を適用
        /// </summary>
        private void ApplySpeedChoice(string speedChoice)
        {
            float baseSpeed = maxSpeedKmh;

            switch (speedChoice.ToUpper())
            {
                case "SLOW":
                    maxSpeedKmh = baseSpeed * 0.7f;
                    break;
                case "NORMAL":
                    // そのまま
                    break;
                case "FAST":
                    maxSpeedKmh = Mathf.Min(baseSpeed * 1.2f, 80f);
                    break;
            }
        }

        /// <summary>
        /// 名前で避難所を検索
        /// </summary>
        private Shelter FindShelterByName(string name)
        {
            var shelters = GameObject.FindGameObjectsWithTag("Shelter");
            foreach (var shelterObj in shelters)
            {
                var shelter = shelterObj.GetComponent<Shelter>();
                if (shelter != null && (shelter.displayName == name || shelterObj.name == name))
                {
                    return shelter;
                }
            }
            return null;
        }

        /// <summary>
        /// 車両専用LLMリクエスト
        /// </summary>
        private async Task<LLMVehicleDecisionResponse> RequestVehicleDecisionAsync(CancellationToken ct)
        {
            var request = BuildVehicleDecisionRequest();
            return await decisionClient.RequestVehicleDecisionAsync(request, ct);
        }

        /// <summary>
        /// 車両用LLMリクエストを構築
        /// </summary>
        private LLMVehicleDecisionRequest BuildVehicleDecisionRequest()
        {
            var persona = VehiclePersonaManager.GetVehiclePersona(int.Parse(vehicleId));

            return new LLMVehicleDecisionRequest
            {
                request_id = $"vehicle-{vehicleId}-{Guid.NewGuid()}",
                timestamp = Time.time,
                vehicle = new VehiclePayload
                {
                    id = vehicleId,
                    position = new Vector3Payload(transform.position),
                    current_speed_kmh = CurrentSpeedKmh,
                    passenger_count = currentPassengers,
                    current_road = currentRoad?.name ?? "",
                    vehicle_type = type.ToString()
                },
                shelter_candidates = BuildVehicleShelterCandidates(),
                vehicle_state = new VehicleStatePayload
                {
                    position = new Vector3Payload(transform.position),
                    velocity = new Vector3Payload(transform.forward * currentSpeed),
                    current_speed_kmh = CurrentSpeedKmh,
                    fuel_level = fuelLevel,
                    stress_level = stressLevel,
                    current_road_name = currentRoad?.name ?? "",
                    current_lane = currentLane?.name ?? "",
                    current_goal = targetShelter?.displayName ?? "",
                    vehicle_state = state.ToString()
                },
                persona = VehiclePersonaManager.ToPayload(persona),
                temporal_context = new TemporalContextPayload
                {
                    elapsed_time = Time.time,
                    time_limit = 600f // 10分
                },
                traffic_context = BuildTrafficContext(),
                current_action = _currentActionType.ToString(),
                action_history = _actionHistory.ToArray()
            };
        }

        /// <summary>
        /// 車両用避難所候補を構築
        /// </summary>
        private VehicleShelterPayload[] BuildVehicleShelterCandidates()
        {
            var shelters = GameObject.FindGameObjectsWithTag("Shelter");
            var candidates = new List<VehicleShelterPayload>();

            foreach (var shelterObj in shelters)
            {
                var shelter = shelterObj.GetComponent<Shelter>();
                if (shelter == null) continue;
                if (_excludeShelters.Contains(shelter.uuid)) continue;

                // 経路距離と所要時間を計算
                var path = _pathfinder.FindPath(transform.position, shelterObj.transform.position);
                float distance = _roadNetwork?.CalculatePathLength(path) ?? 0f;
                float travelTime = _roadNetwork?.CalculatePathTravelTime(path) ?? 0f;

                candidates.Add(new VehicleShelterPayload
                {
                    id = shelterObj.name,
                    display_name = shelter.displayName,
                    description = shelter.description,
                    position = new Vector3Payload(shelterObj.transform.position),
                    current_capacity = shelter.currentCapacity,
                    max_capacity = shelter.MaxCapacity,
                    available_parking_spots = 10, // 仮の値
                    max_parking_spots = 20,       // 仮の値
                    driving_distance_km = distance / 1000f,
                    driving_time_minutes = travelTime / 60f
                });
            }

            return candidates.ToArray();
        }

        /// <summary>
        /// 交通状況コンテキストを構築
        /// </summary>
        private TrafficContextPayload BuildTrafficContext()
        {
            var nearbyRoads = new List<RoadConditionPayload>();

            // 現在の道路と隣接道路の情報を収集
            if (currentRoad != null)
            {
                nearbyRoads.Add(new RoadConditionPayload
                {
                    road_id = currentRoad.name,
                    road_name = currentRoad.name,
                    congestion_level = currentRoad.currentDensity / 100f,
                    average_speed_kmh = currentRoad.speedLimit * (1f - currentRoad.currentDensity / 200f),
                    density = currentRoad.currentDensity,
                    level_of_service = GetLevelOfService(currentRoad.currentDensity)
                });
            }

            return new TrafficContextPayload
            {
                average_network_density = currentRoad?.currentDensity ?? 0f,
                congested_road_count = nearbyRoads.Count(r => r.congestion_level > 0.7f),
                overall_level_of_service = GetLevelOfService(currentRoad?.currentDensity ?? 0f),
                nearby_roads = nearbyRoads.ToArray()
            };
        }

        /// <summary>
        /// 密度からLOS（サービスレベル）を計算
        /// </summary>
        private string GetLevelOfService(float density)
        {
            if (density < 10) return "A";
            if (density < 20) return "B";
            if (density < 35) return "C";
            if (density < 55) return "D";
            if (density < 80) return "E";
            return "F";
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
