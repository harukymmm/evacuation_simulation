using UnityEngine;

namespace Traffic
{
    /// <summary>
    /// 信号の視覚的表現を管理するクラス
    /// </summary>
    public class TrafficSignal : MonoBehaviour
    {
        [Header("信号状態")]
        [Tooltip("現在の信号状態")]
        public SignalState currentState = SignalState.Red;

        [Header("表示設定")]
        [Tooltip("信号灯のRenderer")]
        public Renderer signalRenderer;

        [Tooltip("赤信号のマテリアル")]
        public Material redMaterial;

        [Tooltip("黄信号のマテリアル")]
        public Material yellowMaterial;

        [Tooltip("青信号のマテリアル")]
        public Material greenMaterial;

        [Tooltip("矢印信号のマテリアル")]
        public Material arrowMaterial;

        [Header("ライト設定")]
        [Tooltip("赤信号のライト")]
        public Light redLight;

        [Tooltip("黄信号のライト")]
        public Light yellowLight;

        [Tooltip("青信号のライト")]
        public Light greenLight;

        [Tooltip("ライトの強度")]
        public float lightIntensity = 2f;

        [Header("矢印オブジェクト")]
        [Tooltip("左折矢印オブジェクト")]
        public GameObject leftArrowObject;

        [Tooltip("右折矢印オブジェクト")]
        public GameObject rightArrowObject;

        [Tooltip("直進矢印オブジェクト")]
        public GameObject straightArrowObject;

        [Header("オプション")]
        [Tooltip("発光エフェクトを使用")]
        public bool useEmission = true;

        [Tooltip("発光色の強度")]
        public float emissionIntensity = 2f;

        // マテリアルプロパティ
        private MaterialPropertyBlock _propertyBlock;
        private static readonly int EmissionColorProperty = Shader.PropertyToID("_EmissionColor");

        private void Awake()
        {
            _propertyBlock = new MaterialPropertyBlock();

            // Rendererが未設定の場合は自動取得
            if (signalRenderer == null)
            {
                signalRenderer = GetComponent<Renderer>();
            }

            // デフォルトマテリアルを生成（未設定の場合）
            CreateDefaultMaterials();
        }

        private void Start()
        {
            // 初期状態を設定
            SetState(currentState);
        }

        /// <summary>
        /// デフォルトのマテリアルを生成
        /// </summary>
        private void CreateDefaultMaterials()
        {
            if (redMaterial == null)
            {
                redMaterial = new Material(Shader.Find("Standard"));
                redMaterial.color = new Color(1f, 0.2f, 0.2f);
                redMaterial.EnableKeyword("_EMISSION");
                redMaterial.SetColor(EmissionColorProperty, new Color(1f, 0f, 0f) * emissionIntensity);
            }

            if (yellowMaterial == null)
            {
                yellowMaterial = new Material(Shader.Find("Standard"));
                yellowMaterial.color = new Color(1f, 0.9f, 0.2f);
                yellowMaterial.EnableKeyword("_EMISSION");
                yellowMaterial.SetColor(EmissionColorProperty, new Color(1f, 0.8f, 0f) * emissionIntensity);
            }

            if (greenMaterial == null)
            {
                greenMaterial = new Material(Shader.Find("Standard"));
                greenMaterial.color = new Color(0.2f, 1f, 0.4f);
                greenMaterial.EnableKeyword("_EMISSION");
                greenMaterial.SetColor(EmissionColorProperty, new Color(0f, 1f, 0.3f) * emissionIntensity);
            }

            if (arrowMaterial == null)
            {
                arrowMaterial = new Material(Shader.Find("Standard"));
                arrowMaterial.color = new Color(0.2f, 1f, 0.6f);
                arrowMaterial.EnableKeyword("_EMISSION");
                arrowMaterial.SetColor(EmissionColorProperty, new Color(0f, 1f, 0.5f) * emissionIntensity);
            }
        }

        /// <summary>
        /// 信号状態を設定
        /// </summary>
        public void SetState(SignalState state)
        {
            currentState = state;
            UpdateVisuals();
        }

        /// <summary>
        /// 視覚表現を更新
        /// </summary>
        private void UpdateVisuals()
        {
            // マテリアルを更新
            UpdateMaterial();

            // ライトを更新
            UpdateLights();

            // 矢印オブジェクトを更新
            UpdateArrows();
        }

        /// <summary>
        /// マテリアルを更新
        /// </summary>
        private void UpdateMaterial()
        {
            if (signalRenderer == null) return;

            Material targetMaterial = currentState switch
            {
                SignalState.Red => redMaterial,
                SignalState.Yellow => yellowMaterial,
                SignalState.Green => greenMaterial,
                SignalState.ArrowLeft => arrowMaterial,
                SignalState.ArrowRight => arrowMaterial,
                SignalState.ArrowStraight => arrowMaterial,
                _ => redMaterial
            };

            if (targetMaterial != null)
            {
                signalRenderer.material = targetMaterial;
            }
        }

        /// <summary>
        /// ライトを更新
        /// </summary>
        private void UpdateLights()
        {
            // すべてのライトをオフ
            if (redLight != null) redLight.enabled = false;
            if (yellowLight != null) yellowLight.enabled = false;
            if (greenLight != null) greenLight.enabled = false;

            // 現在の状態に応じたライトをオン
            switch (currentState)
            {
                case SignalState.Red:
                    if (redLight != null)
                    {
                        redLight.enabled = true;
                        redLight.intensity = lightIntensity;
                        redLight.color = Color.red;
                    }
                    break;

                case SignalState.Yellow:
                    if (yellowLight != null)
                    {
                        yellowLight.enabled = true;
                        yellowLight.intensity = lightIntensity;
                        yellowLight.color = Color.yellow;
                    }
                    break;

                case SignalState.Green:
                case SignalState.ArrowLeft:
                case SignalState.ArrowRight:
                case SignalState.ArrowStraight:
                    if (greenLight != null)
                    {
                        greenLight.enabled = true;
                        greenLight.intensity = lightIntensity;
                        greenLight.color = Color.green;
                    }
                    break;
            }
        }

        /// <summary>
        /// 矢印表示を更新
        /// </summary>
        private void UpdateArrows()
        {
            // すべての矢印を非表示
            if (leftArrowObject != null) leftArrowObject.SetActive(false);
            if (rightArrowObject != null) rightArrowObject.SetActive(false);
            if (straightArrowObject != null) straightArrowObject.SetActive(false);

            // 該当する矢印を表示
            switch (currentState)
            {
                case SignalState.ArrowLeft:
                    if (leftArrowObject != null) leftArrowObject.SetActive(true);
                    break;

                case SignalState.ArrowRight:
                    if (rightArrowObject != null) rightArrowObject.SetActive(true);
                    break;

                case SignalState.ArrowStraight:
                    if (straightArrowObject != null) straightArrowObject.SetActive(true);
                    break;
            }
        }

        /// <summary>
        /// 信号が進入可能な状態かどうか
        /// </summary>
        public bool CanProceed()
        {
            return currentState == SignalState.Green ||
                   currentState == SignalState.ArrowLeft ||
                   currentState == SignalState.ArrowRight ||
                   currentState == SignalState.ArrowStraight;
        }

        /// <summary>
        /// 信号が停止を要求しているかどうか
        /// </summary>
        public bool RequiresStop()
        {
            return currentState == SignalState.Red || currentState == SignalState.Yellow;
        }

        /// <summary>
        /// 信号が黄色かどうか
        /// </summary>
        public bool IsYellow()
        {
            return currentState == SignalState.Yellow;
        }

        /// <summary>
        /// 発光エフェクトを設定
        /// </summary>
        public void SetEmission(bool enabled, Color? color = null)
        {
            if (signalRenderer == null || !useEmission) return;

            signalRenderer.GetPropertyBlock(_propertyBlock);

            if (enabled && color.HasValue)
            {
                _propertyBlock.SetColor(EmissionColorProperty, color.Value * emissionIntensity);
            }
            else
            {
                _propertyBlock.SetColor(EmissionColorProperty, Color.black);
            }

            signalRenderer.SetPropertyBlock(_propertyBlock);
        }

        /// <summary>
        /// デバッグ情報を取得
        /// </summary>
        public string GetDebugInfo()
        {
            return $"Signal: {currentState}";
        }

        /// <summary>
        /// Gizmo描画
        /// </summary>
        private void OnDrawGizmos()
        {
            Gizmos.color = currentState switch
            {
                SignalState.Green => Color.green,
                SignalState.Yellow => Color.yellow,
                SignalState.Red => Color.red,
                SignalState.ArrowLeft => new Color(0, 1, 0.5f),
                SignalState.ArrowRight => new Color(0, 1, 0.5f),
                SignalState.ArrowStraight => new Color(0, 1, 0.5f),
                _ => Color.gray
            };

            Gizmos.DrawSphere(transform.position, 0.5f);
        }
    }
}
