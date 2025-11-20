# ML-Agents 避難所選択シミュレーション

## 環境構築

### 1. Python環境のセットアップ

Python 3.10.12 と ML-Agents 1.0.0 を使用します。

```bash
# conda環境の作成
eval "$(/opt/homebrew/anaconda3/bin/conda shell.zsh hook)"
conda create -n mlagents python=3.10.12 -y

# 環境の有効化
conda activate mlagents

# 依存パッケージのインストール（ビルドエラー回避のためconda-forgeから先にインストール）
conda install -n mlagents -c conda-forge grpcio==1.48.2 onnx==1.12.0 -y

# ML-Agents 1.0.0のインストール
pip install mlagents==1.0.0
```

### 2. Unity側の設定

- Unity Editorでプロジェクトを開く
- ML-Agentsパッケージ（com.unity.ml-agents）がインストールされていることを確認
- シーン `Assets/namie.unity` を開く
- `EnvManager` コンポーネントの設定を確認：
  - `SpawnEvacueeSize`: 避難者の生成数
  - `SpawnEvacueePref`: 避難者のプレハブ
  - `AgentObj`: `ShelterManagementAgent` がアタッチされたGameObject

## 実行方法

```bash
# 訓練の開始
mlagents-learn Assets/Config/Tutorial-1.yaml --run-id=shelter_training --force
```

訓練結果は `results/shelter_training/` に保存されます。

## 実装概要

### システム構成

本シミュレーションは、災害時の避難所選択を強化学習で最適化するシステムです。

#### 主要コンポーネント

1. **ShelterManagementAgent** (`Assets/ShelterManagementAgent.cs`)
   - ML-Agentsのエージェント
   - 複数の建物候補から避難所を選択する
   - 観測情報に基づいて各建物を「避難所にする(1) / しない(0)」を決定

2. **EnvManager** (`Assets/ShelterEnvManager.cs`)
   - シミュレーション環境全体の管理
   - 避難者の生成・削除
   - エピソードの開始・終了制御
   - 報酬の計算と付与

3. **Evacuee** (`Assets/Evacuee.cs`)
   - 個々の避難者の制御
   - NavMeshAgentを使用した避難所への移動
   - エージェントの行動に応じて最寄りの避難所を探索

4. **Shelter** (`Assets/Shelter.cs`)
   - 避難所の状態管理（収容人数、現在の避難者数など）

### 観測（入力）

エージェントは以下の情報を観測します：

- **各避難所候補について**（`ShelterCandidates` の順序で）:
  - 建物の位置座標（X, Y, Z）
  - 現在の収容可能人数（`currentCapacity`）
- **避難者情報**:
  - 現在環境内にいる避難者の総数
  - 各避難者の位置座標（X, Y, Z）

観測サイズは `(候補数 × 4) + 1 + (避難者数 × 3)` となります。

### 行動（出力）

エージェントは各建物候補に対して **0（非選択）または 1（選択）** を出力します。

- **0**: 避難所として選択しない → タグを `Untagged` に変更、非選択マテリアルを適用
- **1**: 避難所として選択 → `CurrentShelters` に追加、タグを `Shelter` に変更、選択マテリアルを適用

エージェントが行動を決定すると、`OnDidActioned` イベントが発火し、全避難者が最寄りの選択された避難所を目指して移動を開始します。

### 報酬設計

エピソード終了時に以下の報酬が与えられます：

1. **避難率による報酬**: `GetCurrentEvacueeRate()` （0.0 ～ 1.0）
   - 避難完了した避難者の割合
2. **時間ボーナス**: `(MaxSeconds - currentTimeSec) / MaxSeconds`
   - 制限時間内に完了するほど高くなる

**総合報酬** = 避難率 + 時間ボーナス

### エピソードの流れ

1. `OnEpisodeBegin()`: 環境をリセットし、避難者をスポーン
2. エージェントが行動を決定（避難所の選択）
3. 避難者が選択された避難所へ移動
4. 避難所に到着した避難者は避難処理を実行
5. 制限時間到達または全員避難完了でエピソード終了
6. 報酬を計算してエージェントに付与
7. 1に戻る

### データ記録

`IsRecordData` が有効な場合、以下のCSVファイルが `Logs/<recordID>/` に保存されます：

- `ActionLog_Episode_<N>.csv`: 各エピソード・ステップでの避難所選択履歴
- `EvaRatesPerSec_Episode_<N>.csv`: 時間経過ごとの避難率の変化

### 設定ファイル

訓練設定は `Assets/Config/Tutorial-1.yaml` で定義されています：

- **Trainer**: PPO (Proximal Policy Optimization)
- **Max Steps**: 500,000
- **Learning Rate**: 1e-3
- **Network**: 3層、各層512ユニット
- **Reward Signal**: Gamma=0.80
