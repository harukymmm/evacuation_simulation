/// <summary>
/// シミュレーション全体で使用する定数を集約するクラス
/// マジックナンバーを排除し、設定変更を容易にする
/// </summary>
public static class SimulationConstants
{
    /// <summary>
    /// スタミナ（体力）管理に関する定数
    /// </summary>
    public static class Stamina
    {
        /// <summary>急ぎ足に必要な最低体力</summary>
        public const float THRESHOLD_FAST = 0.3f;

        /// <summary>走るに必要な最低体力</summary>
        public const float THRESHOLD_RUN = 0.5f;

        /// <summary>急ぎ足時の消費率（%/秒）</summary>
        public const float DRAIN_FAST = 0.02f;

        /// <summary>走り時の消費率（%/秒）</summary>
        public const float DRAIN_RUN = 0.05f;

        /// <summary>ゆっくり歩行時の回復率（%/秒）</summary>
        public const float RECOVERY_SLOW = 0.01f;

        /// <summary>停止中の回復率（%/秒）</summary>
        public const float RECOVERY_STOP = 0.03f;

        /// <summary>高齢者の消費倍率</summary>
        public const float ELDERLY_MULTIPLIER = 1.5f;
    }

    /// <summary>
    /// 会話・連絡に関する定数
    /// </summary>
    public static class Communication
    {
        /// <summary>会話メッセージの遅延時間（シミュレーション秒）</summary>
        public const float CONVERSATION_MESSAGE_LAG_SEC = 5.0f;

        /// <summary>連絡メッセージの遅延時間（シミュレーション秒）</summary>
        public const float CONTACT_MESSAGE_LAG_SEC = 3.0f;

        /// <summary>会話応答タイムアウト（シミュレーション秒）</summary>
        public const float CONVERSATION_TIMEOUT_SEC = 70.0f;

        /// <summary>連絡応答タイムアウト（シミュレーション秒）</summary>
        public const float CONTACT_TIMEOUT_SEC = 105.0f;

        /// <summary>連続TALK回数の上限</summary>
        public const int MAX_CONSECUTIVE_TALK = 5;

        /// <summary>連続CONTACT回数の上限</summary>
        public const int MAX_CONSECUTIVE_CONTACT = 5;

        /// <summary>1セッション内のターン上限</summary>
        public const int MAX_CONVERSATION_TURNS = 10;

        /// <summary>会話履歴の保持数</summary>
        public const int MAX_CONVERSATION_HISTORY = 5;
    }

    /// <summary>
    /// 家族・追従行動に関する定数
    /// </summary>
    public static class Social
    {
        /// <summary>家族合流判定距離（メートル）</summary>
        public const float FAMILY_REUNION_DISTANCE = 10f;

        /// <summary>周辺避難者検索半径（メートル）</summary>
        public const float FOLLOW_SEARCH_RADIUS = 30f;

        /// <summary>追従距離（メートル）</summary>
        public const float FOLLOW_DISTANCE = 5f;

        /// <summary>家族検索座標更新間隔（秒）</summary>
        public const float SEARCH_FAMILY_CHECK_INTERVAL = 10f;

        /// <summary>追従チェック間隔（秒）</summary>
        public const float FOLLOW_CHECK_INTERVAL = 1f;

        /// <summary>情報伝播チェック間隔（秒）</summary>
        public const float INFORMATION_DIFFUSION_CHECK_INTERVAL = 1f;
    }

    /// <summary>
    /// 行動履歴に関する定数
    /// </summary>
    public static class History
    {
        /// <summary>保持する行動履歴の最大件数</summary>
        public const int MAX_ACTION_HISTORY = 5;
    }

    /// <summary>
    /// LLM意思決定に関する定数
    /// </summary>
    public static class LLMDecision
    {
        /// <summary>デフォルトのLLM再呼び出し間隔（秒）</summary>
        public const float DEFAULT_DECISION_INTERVAL = 120f;

        /// <summary>最小リクエスト間隔（秒）</summary>
        public const float MIN_REQUEST_INTERVAL = 1f;
    }
}
