using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 避難所に関するスクリプト（オブジェクト１台分）
/// 現在の収容人数や、受け入れ可否等のデータを用意
/// </summary>
public class Shelter : MonoBehaviour{
    public int MaxCapacity = 10; //最大収容人数
    public int NowAccCount = 0; //現在の収容人数
    // 現在の受け入れ可能人数：最大収容人数 - 現在の収容人数（常に最新の値を計算して返す）
    public int currentCapacity => MaxCapacity - NowAccCount;

    public string uuid; //タワーの識別子
    /**Events */
    public delegate void AcceptRejected(int NowAccCount) ; //収容定員が超過した時に発火する
    public AcceptRejected onRejected;

    private EnvManager _env;

    void Start() {
        _env = GetComponentInParent<EnvManager>();
        _env.OnEndEpisode += (float _) => {
            // 環境側のエピソード終了時に収容人数をリセット
            NowAccCount = 0;
            Debug.Log($"[Shelter] {gameObject.name}: エピソード終了 - 収容人数をリセットしました。MaxCapacity: {MaxCapacity}, NowAccCount: {NowAccCount}, CurrentCapacity: {currentCapacity}");
        };
        
        // 初期値をログ出力
        Debug.Log($"[Shelter] {gameObject.name}: 初期化完了 - MaxCapacity: {MaxCapacity}, NowAccCount: {NowAccCount}, CurrentCapacity: {currentCapacity}");
    }


    /// <summary>
    /// リアルタイムで収容人数をチェック
    /// </summary>
    void Update() {
        if (currentCapacity <= 0) {
            onRejected?.Invoke(NowAccCount);
        }
    }

    /// <summary>
    /// 避難者オブジェクトが建物に到達したときに呼び出される。当たり関数
    /// </summary>
    /// <param name="other"></param>
    void OnTriggerEnter(Collider other) {
        Debug.Log($"[Shelter] OnTriggerEnter called on {gameObject.name} - Colliding with: {other.name}, Tag: {other.tag}");
        Debug.Log($"[Shelter] {gameObject.name}: 現在の収容状況 - MaxCapacity: {MaxCapacity}, NowAccCount: {NowAccCount}, CurrentCapacity: {currentCapacity}");
        
        bool isEvacuee = other.CompareTag("Evacuee");
        if (isEvacuee) {
            Debug.Log($"[Shelter] Evacuee detected! Shelter: {gameObject.name}, Evacuee: {other.name}");
            Evacuee evacuee = other.GetComponent<Evacuee>();
            if (evacuee != null) {
                evacuee.Evacuation(this);
                Debug.Log($"[Shelter] {gameObject.name}: Evacuation処理後 - MaxCapacity: {MaxCapacity}, NowAccCount: {NowAccCount}, CurrentCapacity: {currentCapacity}");
            } else {
                Debug.LogError($"[Shelter] Evacuee component not found on {other.name}");
            }
        } else {
            Debug.LogWarning($"[Shelter] Collision with non-Evacuee object: {other.name}, Tag: {other.tag}");
        }
    }
}
