import csv
import random

# ==========================================
# 1. 設定データとペルソナテンプレート
# ==========================================

# 日本人の名前リスト（ランダム生成用）
SURNAMES = ["佐藤", "鈴木", "高橋", "田中", "渡辺", "伊藤", "山本", "中村", "小林", "加藤", "吉田", "山田", "佐々木", "斎藤", "松本", "井上", "木村", "林", "清水", "山崎", "阿部", "森", "池田", "橋本", "山下", "石川", "中島", "前田", "藤田", "小川", "遠藤", "根本", "三浦", "大野", "菅野"]
NAMES_MALE = ["健太", "誠", "剛", "翔太", "大輔", "拓也", "直樹", "達也", "浩", "一郎", "健", "修", "勇気", "大樹", "翼", "陽翔", "蓮", "湊"]
NAMES_FEMALE = ["裕子", "愛", "美咲", "花子", "恵", "香織", "直美", "由美", "結衣", "陽菜", "葵", "凛", "美優", "七海", "さくら", "美月"]

# ペルソナの定義（役割ごとの特性）
# speed: (min, max) の範囲でランダム設定
ARCHETYPES = {
    "MANAGER": {
        "role": "施設管理者",
        "speed_range": (1.1, 1.3),
        "stairs": "allowed",
        "mental": "冷静・分析的",
        "priority": "他者の誘導と安全確認",
        "prompt": "あなたは責任感の強い施設管理者です。自身の避難よりも、逃げ遅れた人がいないかの確認や、他者の誘導を最優先します。パニックに陥らず、常に情報を分析し、全体にとって最も合理的なルートを選択してください。"
    },
    "NORMAL_BIAS": {
        "role": "一般社員/住民",
        "speed_range": (0.9, 1.1),
        "stairs": "allowed",
        "mental": "正常性バイアス・楽観的",
        "priority": "荷物・現状維持",
        "prompt": "あなたは状況を過小評価する傾向があります。「ただの訓練か誤報だろう」と考え、避難開始を遅らせます。周囲が動いていても、まずは自分の荷物をまとめたり、SNSを確認することを優先してください。"
    },
    "PARENT": {
        "role": "保護者",
        "speed_range": (0.8, 1.0), # 子供を探すため足が止まりがち
        "stairs": "allowed",
        "mental": "焦燥・目的外行動",
        "priority": "家族の捜索と保護",
        "prompt": "あなたは家族（子供など）とはぐれてしまった保護者です。あなたの最優先事項は「避難」ではなく「家族を見つけること」です。家族がいる可能性のある場所へ向かうためなら、避難経路を逆走することも厭いません。合流するまでは非常に焦っています。"
    },
    "ELDERLY": {
        "role": "高齢者",
        "speed_range": (0.4, 0.6),
        "stairs": "forbidden",
        "mental": "慎重・人混み恐怖",
        "priority": "平坦なルート・転倒防止",
        "prompt": "あなたは足腰に不安のある高齢者です（杖使用など）。移動速度は遅く、階段の上り下りは不可能です。転倒を極端に恐れているため、最短ルートであっても人が密集している場所や、段差のある場所は避けてください。"
    },
    "FOLLOWER": {
        "role": "学生/若者",
        "speed_range": (0.9, 1.2),
        "stairs": "allowed",
        "mental": "同調性・パニック気味",
        "priority": "集団への追従",
        "prompt": "あなたはパニックになりやすく、自分の判断に自信がありません。非常口の表示よりも、「多くの人が走っていく方向」を信じてついていきます。周囲が走れば走り、周囲が叫べば不安になります。"
    }
}

# ==========================================
# 2. 生成ロジック
# ==========================================

def generate_personas(count=500):
    data = []
    
    # 福島県の人口構成比率（概算）に基づいた配分ターゲット
    # 合計 1.0 (100%)
    # 年少(11%) + 生産(57%) + 老年(32%)
    
    # 役割の割り当て確率
    # 0-14歳 (11%): 学生(FOLLOWER)
    # 15-64歳 (57%): 管理者(3%), 親(10%), 正常性バイアス(22%), 同調者(22%)
    # 65歳以上 (32%): 高齢者(ELDERLY)
    
    weights = {
        "ELDERLY": 0.32,      # 高齢者
        "FOLLOWER": 0.33,     # 学生 + 若者同調者
        "NORMAL_BIAS": 0.22,  # 一般
        "PARENT": 0.10,       # 保護者
        "MANAGER": 0.03       # 管理者
    }
    
    # 役割リストを確率に基づいて生成
    roles_distribution = random.choices(
        list(weights.keys()),
        weights=list(weights.values()),
        k=count
    )

    for i, archetype_key in enumerate(roles_distribution):
        agent_id = i + 1
        archetype = ARCHETYPES[archetype_key]
        
        # 性別決定 (概ね半々)
        gender = random.choice(["Male", "Female"])
        
        # 名前生成
        surname = random.choice(SURNAMES)
        firstname = random.choice(NAMES_MALE if gender == "Male" else NAMES_FEMALE)
        full_name = f"{surname} {firstname}"
        
        # 年代決定 (役割に応じた年齢層を割り当て)
        if archetype_key == "ELDERLY":
            age = random.randint(65, 90)
            age_group = f"{age//10 * 10}s_{gender}" # 70s_Male 等
        elif archetype_key == "FOLLOWER":
            # 学生または若手社員
            age = random.choice([random.randint(10, 19), random.randint(20, 29)])
            age_group = f"{age}s_{gender}" if age < 20 else f"{age//10 * 10}s_{gender}"
            if age < 20: archetype["role"] = "学生" # 若い場合は役割名を学生に
        elif archetype_key == "MANAGER":
            age = random.randint(30, 60)
            age_group = f"{age//10 * 10}s_{gender}"
        else: # PARENT, NORMAL_BIAS
            age = random.randint(25, 64)
            age_group = f"{age//10 * 10}s_{gender}"

        # 速度決定 (範囲内でランダム)
        speed = round(random.uniform(*archetype["speed_range"]), 2)
        
        # レコード作成
        row = [
            agent_id,
            full_name,
            archetype["role"],
            age_group,
            speed,
            archetype["stairs"],
            archetype["mental"],
            archetype["priority"],
            archetype["prompt"]
        ]
        data.append(row)

    return data

# ==========================================
# 3. CSV出力
# ==========================================

output_file = "fukushima_evacuation_personas_500.csv"
header = ["agent_id", "name", "role", "age_group", "speed_multiplier", "stairs_usage", "mental_state", "priority", "system_prompt_context"]
personas = generate_personas(500)

with open(output_file, mode="w", encoding="utf-8", newline="") as f:
    writer = csv.writer(f)
    writer.writerow(header)
    writer.writerows(personas)

print(f"生成完了: {output_file}")
print("内訳確認:")
from collections import Counter
roles = [p[2] for p in personas] # role列
print(Counter(roles))