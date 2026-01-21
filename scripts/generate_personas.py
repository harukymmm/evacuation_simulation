#!/usr/bin/env python3
"""
personas.csv を30人から500人に拡張するスクリプト

分布:
- 年齢: いわき市統計準拠
  - 10歳未満: 10% (50人)
  - 10代: 10% (50人)
  - 20-30代: 20% (100人)
  - 40-50代: 25% (125人)
  - 60代以上: 35% (175人)

- mental_state:
  - 冷静・合理的/分析的: 30% (150人)
  - 正常性バイアス・楽観的: 25% (125人)
  - 同調性・集団追従/パニック気味: 20% (100人)
  - 焦燥・目的外行動: 10% (50人)
  - 慎重・人混み恐怖: 15% (75人)
"""

import csv
import random
import os

# 日本人名のパターン
LAST_NAMES = [
    "佐藤", "鈴木", "高橋", "田中", "渡辺", "伊藤", "山本", "中村", "小林", "加藤",
    "吉田", "山田", "佐々木", "山口", "松本", "井上", "木村", "林", "斎藤", "清水",
    "山崎", "池田", "阿部", "橋本", "石川", "前田", "藤田", "岡田", "後藤", "長谷川",
    "村上", "近藤", "石井", "坂本", "遠藤", "青木", "藤井", "西村", "福田", "太田",
    "三浦", "岡本", "松田", "中川", "中野", "原田", "小川", "竹内", "金子", "和田"
]

MALE_FIRST_NAMES = [
    "健太", "翔太", "大輔", "拓也", "直樹", "雄一", "誠", "剛", "浩二", "正男",
    "太郎", "一郎", "勝男", "修", "隆", "和夫", "博", "進", "茂", "清",
    "明", "実", "勇", "武", "豊", "昭", "弘", "正", "達也", "康介",
    "裕太", "健一", "光", "徹", "学", "悠太", "蓮", "陽太", "颯太", "樹"
]

FEMALE_FIRST_NAMES = [
    "裕子", "愛", "美香", "恵子", "幸子", "由美", "久美", "真理", "さくら", "明子",
    "和子", "トメ", "花子", "京子", "節子", "文子", "良子", "洋子", "美智子", "道子",
    "千代", "ツル", "ミツ", "ハナ", "マサ", "美咲", "結衣", "陽菜", "葵", "楓",
    "彩", "優花", "遥", "美月", "凜", "杏", "芽衣", "心春", "詩織", "菜々子"
]

# 年齢グループ
AGE_GROUPS = [
    ("child", "10s_", 0.10),      # 10歳未満+10代 合計20%
    ("young", "20s_", 0.10),      # 20代
    ("adult30", "30s_", 0.10),    # 30代
    ("adult40", "40s_", 0.125),   # 40代
    ("adult50", "50s_", 0.125),   # 50代
    ("senior60", "60s_", 0.15),   # 60代
    ("senior70", "70s_", 0.15),   # 70代
    ("senior80", "80s_", 0.05),   # 80代以上
]

# mental_state と対応するテンプレート
MENTAL_STATE_TEMPLATES = {
    "冷静・合理的": {
        "priority": "安全確保",
        "context_templates": [
            "あなたは冷静に状況を判断できます。感情に流されず、客観的な情報に基づいて適切な避難行動を取ります。",
            "あなたは冷静で判断力があります。災害時には素早く状況を把握し、最も安全な行動を選択します。",
            "あなたは落ち着いて判断できます。周囲の混乱に惑わされず、合理的に避難経路を選びます。",
        ]
    },
    "冷静・分析的": {
        "priority": "他者の誘導と安全確認",
        "context_templates": [
            "あなたは責任感が強く、状況を分析して行動します。自分の避難だけでなく、周囲の人の安全も気にかけます。",
            "あなたは全体状況を把握して行動します。最も効率的な避難方法を考え、必要に応じて周囲に声をかけます。",
            "あなたは冷静に情報を分析できます。災害時には状況を見極め、適切な判断を下します。",
        ]
    },
    "正常性バイアス・楽観的": {
        "priority": "状況判断・自己決定",
        "context_templates": [
            "あなたは楽観的な性格ですが、危険が明確になれば行動を起こせます。最初は様子を見ることが多いですが、状況に応じて判断を変えられます。",
            "あなたは落ち着いて状況を見極めようとします。ただし、周囲の動きや追加情報によって、避難を決断することもあります。",
            "あなたは自分の判断に自信を持っていますが、状況が深刻だと分かれば柔軟に対応できます。",
            "あなたは慌てて行動することは少ないですが、危険を認識すれば適切に避難を選択できます。",
            "あなたは前向きに状況を捉えますが、安全を最優先にすることも忘れません。",
        ]
    },
    "同調性・集団追従": {
        "priority": "周囲に合わせる",
        "context_templates": [
            "あなたは周囲の人の行動を見て判断します。みんなが避難していれば自分も避難し、待機していれば自分も待機します。",
            "あなたは一人で判断するより、みんなと同じ行動をとる方が安心です。集団の流れについていきます。",
            "あなたは周囲の雰囲気を読んで行動します。みんなが落ち着いていれば落ち着き、慌てていれば慌てます。",
            "あなたは多数派の意見に従います。みんなが同じ方向に向かっているなら、それが正しいと信じます。",
        ]
    },
    "同調性・パニック気味": {
        "priority": "集団への追従",
        "context_templates": [
            "あなたはパニックになりやすく、自分の判断に自信がありません。周囲が走れば走り、叫べば不安になります。",
            "あなたは不安を感じやすく、周囲の人についていきます。自分で判断するのは苦手です。",
            "あなたはストレスに弱く、混乱した状況では周囲の行動に流されやすいです。",
        ]
    },
    "焦燥・目的外行動": {
        "priority": "子供の捜索と保護",
        "context_templates": [
            "あなたは家族のことが心配で、避難より先に家族を探そうとします。合流するまで非常に焦っています。",
            "あなたは大切な人の安全が気がかりで、その人を探すことを最優先します。",
            "あなたは家族と離れてしまい、何としても合流したいと考えています。避難経路を逆走することも厭いません。",
        ]
    },
    "慎重・人混み恐怖": {
        "priority": "平坦なルート・転倒防止",
        "context_templates": [
            "あなたは身体的な制約があり、移動に時間がかかります。転倒を恐れ、人混みや段差のある場所は避けようとします。",
            "あなたは慎重に行動します。急いで転倒するより、安全なルートをゆっくり進むことを優先します。",
            "あなたは体力に自信がなく、無理のないペースで移動します。混雑した場所は避けたいと考えています。",
        ]
    },
}

# 職業リスト
ROLES_BY_AGE = {
    "child": ["小学生", "中学生", "高校生"],
    "young": ["大学生", "会社員", "アルバイト", "専門学校生"],
    "adult30": ["会社員", "主婦", "自営業", "看護師", "教師", "公務員"],
    "adult40": ["会社員", "主婦", "自営業", "パート従業員", "管理職", "施設管理者"],
    "adult50": ["会社員", "自営業", "農業", "漁師", "パート従業員", "管理職"],
    "senior60": ["自営業", "農業", "漁師", "パート従業員", "無職", "消防団員"],
    "senior70": ["高齢者", "無職", "農業"],
    "senior80": ["高齢者", "無職"],
}

# 現在地の理由
CURRENT_LOCATION_REASONS = {
    "child": ["学校で授業を受けていた", "放課後に友達と遊んでいた", "塾から帰る途中だった"],
    "young": ["大学で授業中だった", "カフェでアルバイト中だった", "自宅でオンライン授業中だった", "職場で仕事中だった"],
    "adult30": ["職場でデスクワーク中", "自宅で家事をしていた", "在宅勤務中だった", "買い物中だった"],
    "adult40": ["職場で仕事中だった", "自宅で家事をしていた", "スーパーでパート中だった", "職場で会議中だった"],
    "adult50": ["自分の店で仕事中だった", "職場で仕事中だった", "畑で作業中だった", "漁港で網の手入れをしていた"],
    "senior60": ["自宅で休憩していた", "畑で作業中だった", "漁港にいた", "近所の人と立ち話をしていた"],
    "senior70": ["自宅で休憩していた", "自宅で編み物をしていた", "近所の集会所の前にいた", "自宅でお茶を飲んでいた"],
    "senior80": ["自宅で休憩していた", "自宅の玄関先で近所の人と話していた", "デイサービスから帰宅したところだった"],
}

# 過去の災害経験
PAST_DISASTER_EXPERIENCES = [
    "2011年の津波で避難誘導を経験した",
    "2011年の津波で自宅が浸水した",
    "2011年の津波で家族と離れ離れになった経験がある",
    "2011年の津波を経験し近所の人に助けられた",
    "2011年は周りの人と一緒に避難した",
    "過去の地震警報で避難したが何もなかった",
    "過去に何度も津波警報を聞いたが来なかった",
    "何度も地震を経験したが大事には至らなかった",
    "他地域で震災を経験した",
    "この地域での被災経験なし",
    "大きな災害経験なし",
]

# 身体状態
PHYSICAL_CONDITIONS_BY_AGE = {
    "child": ["健康", "健康だがストレスに弱い"],
    "young": ["健康", "健康で体力がある", "健康だがストレスに弱い"],
    "adult30": ["健康", "健康で体力がある"],
    "adult40": ["健康", "健康で体力がある", "軽い腰痛がある", "軽い膝痛"],
    "adult50": ["健康", "軽い腰痛がある", "軽い高血圧", "腰が悪い"],
    "senior60": ["健康", "足腰が弱い", "軽い高血圧", "腰が悪い"],
    "senior70": ["足が悪く歩行が遅い", "杖を使用・足腰が弱い", "杖を使って歩く", "歩行困難"],
    "senior80": ["歩行困難・介護が必要", "足が悪く一人では歩けない", "杖を使用・足腰が弱い"],
}


def get_speed_multiplier(age_category: str, physical_condition: str) -> float:
    """年齢と身体状態に基づいて速度倍率を決定"""
    base_speeds = {
        "child": 1.0,
        "young": 1.1,
        "adult30": 1.0,
        "adult40": 1.0,
        "adult50": 0.9,
        "senior60": 0.7,
        "senior70": 0.5,
        "senior80": 0.4,
    }
    base = base_speeds.get(age_category, 1.0)

    # 身体状態による調整
    if "体力がある" in physical_condition:
        base += 0.1
    if "歩行困難" in physical_condition or "一人では歩けない" in physical_condition:
        base -= 0.1
    if "杖を使用" in physical_condition or "杖を使って" in physical_condition:
        base -= 0.05

    # ランダムな揺らぎを追加
    base += random.uniform(-0.1, 0.1)

    return round(max(0.3, min(1.3, base)), 1)


def get_local_knowledge(residence_years: int) -> str:
    """居住年数に基づいて土地勘レベルを決定"""
    if residence_years <= 3:
        return "newcomer"
    elif residence_years <= 15:
        return "resident"
    else:
        return "native"


def generate_persona(agent_id: int, age_category: str, mental_state: str, gender: str) -> dict:
    """1人分のペルソナを生成"""
    # 名前生成
    last_name = random.choice(LAST_NAMES)
    if gender == "Male":
        first_name = random.choice(MALE_FIRST_NAMES)
    else:
        first_name = random.choice(FEMALE_FIRST_NAMES)
    name = f"{last_name} {first_name}"

    # 年齢グループ
    age_decade = {
        "child": random.choice(["10s"]),
        "young": "20s",
        "adult30": "30s",
        "adult40": "40s",
        "adult50": "50s",
        "senior60": "60s",
        "senior70": "70s",
        "senior80": "80s",
    }[age_category]
    age_group = f"{age_decade}_{gender}"

    # 職業
    role = random.choice(ROLES_BY_AGE[age_category])

    # mental_state情報
    ms_info = MENTAL_STATE_TEMPLATES[mental_state]
    priority = ms_info["priority"]
    system_prompt_context = random.choice(ms_info["context_templates"])

    # 身体状態
    physical_condition = random.choice(PHYSICAL_CONDITIONS_BY_AGE[age_category])

    # 速度
    speed_multiplier = get_speed_multiplier(age_category, physical_condition)

    # 居住年数
    if age_category in ["child", "young"]:
        residence_years = random.randint(1, 10)
    elif age_category in ["adult30", "adult40"]:
        residence_years = random.randint(3, 25)
    elif age_category in ["adult50", "senior60"]:
        residence_years = random.randint(10, 45)
    else:
        residence_years = random.randint(40, 75)

    # 土地勘
    local_knowledge = get_local_knowledge(residence_years)

    # 自宅
    home_location_category = random.choice(["coastal", "hill", "center"])
    home_elevation = {
        "coastal": random.randint(3, 10),
        "hill": random.randint(15, 25),
        "center": random.randint(10, 18),
    }[home_location_category]
    home_structure = random.choice(["wooden_1story", "wooden_2story", "rc_2story", "rc_3story"])

    # 現在地の理由
    current_location_reason = random.choice(CURRENT_LOCATION_REASONS[age_category])

    # 過去の災害経験
    past_disaster_experience = random.choice(PAST_DISASTER_EXPERIENCES)

    return {
        "agent_id": agent_id,
        "name": name,
        "role": role,
        "age_group": age_group,
        "speed_multiplier": speed_multiplier,
        "mental_state": mental_state,
        "priority": priority,
        "system_prompt_context": system_prompt_context,
        "home_location_category": home_location_category,
        "home_elevation": home_elevation,
        "home_structure": home_structure,
        "residence_years": residence_years,
        "local_knowledge_level": local_knowledge,
        "current_location_reason": current_location_reason,
        "past_disaster_experience": past_disaster_experience,
        "physical_condition": physical_condition,
    }


def generate_500_personas() -> list:
    """500人分のペルソナを生成"""
    personas = []
    agent_id = 1

    # mental_state の分布（合計500人）
    mental_state_distribution = [
        ("冷静・合理的", 75),
        ("冷静・分析的", 75),
        ("正常性バイアス・楽観的", 125),
        ("同調性・集団追従", 80),
        ("同調性・パニック気味", 20),
        ("焦燥・目的外行動", 50),
        ("慎重・人混み恐怖", 75),
    ]

    # 年齢カテゴリの分布
    age_distribution = [
        ("child", 100),      # 10歳未満+10代: 20%
        ("young", 50),       # 20代: 10%
        ("adult30", 50),     # 30代: 10%
        ("adult40", 63),     # 40代: 12.5%
        ("adult50", 62),     # 50代: 12.5%
        ("senior60", 75),    # 60代: 15%
        ("senior70", 75),    # 70代: 15%
        ("senior80", 25),    # 80代以上: 5%
    ]

    # mental_stateごとにループ
    for mental_state, ms_count in mental_state_distribution:
        # このmental_stateの人数を年齢分布に従って割り当て
        remaining = ms_count
        for i, (age_cat, age_count) in enumerate(age_distribution):
            # 各年齢カテゴリに割り当てる人数（比率に基づく）
            if i == len(age_distribution) - 1:
                # 最後のカテゴリは残り全部
                count = remaining
            else:
                # 比率に基づいて計算
                ratio = age_count / 500
                count = max(1, int(ms_count * ratio))
                remaining -= count

            for _ in range(count):
                gender = random.choice(["Male", "Female"])
                persona = generate_persona(agent_id, age_cat, mental_state, gender)
                personas.append(persona)
                agent_id += 1

    # 500人に調整（多い場合は切り捨て、少ない場合は追加）
    while len(personas) > 500:
        personas.pop()
    while len(personas) < 500:
        age_cat = random.choice(["adult30", "adult40", "adult50"])
        mental_state = random.choice(["冷静・合理的", "正常性バイアス・楽観的", "同調性・集団追従"])
        gender = random.choice(["Male", "Female"])
        persona = generate_persona(len(personas) + 1, age_cat, mental_state, gender)
        personas.append(persona)

    # agent_idを振り直し
    for i, p in enumerate(personas):
        p["agent_id"] = i + 1

    # シャッフル
    random.shuffle(personas)
    for i, p in enumerate(personas):
        p["agent_id"] = i + 1

    return personas


def main():
    """メイン関数"""
    random.seed(42)  # 再現性のため

    personas = generate_500_personas()

    # CSVに出力
    script_dir = os.path.dirname(os.path.abspath(__file__))
    output_path = os.path.join(script_dir, "..", "Assets", "Config", "personas.csv")
    output_path = os.path.normpath(output_path)

    # バックアップ
    backup_path = output_path.replace(".csv", "_backup_30.csv")
    if os.path.exists(output_path) and not os.path.exists(backup_path):
        import shutil
        shutil.copy(output_path, backup_path)
        print(f"バックアップを作成しました: {backup_path}")

    fieldnames = [
        "agent_id", "name", "role", "age_group", "speed_multiplier",
        "mental_state", "priority", "system_prompt_context",
        "home_location_category", "home_elevation", "home_structure",
        "residence_years", "local_knowledge_level", "current_location_reason",
        "past_disaster_experience", "physical_condition"
    ]

    with open(output_path, 'w', newline='', encoding='utf-8') as f:
        writer = csv.DictWriter(f, fieldnames=fieldnames)
        writer.writeheader()
        for persona in personas:
            writer.writerow(persona)

    print(f"500人のペルソナを生成しました: {output_path}")

    # 統計を表示
    print("\n=== 生成統計 ===")

    # mental_state分布
    ms_counts = {}
    for p in personas:
        ms = p["mental_state"]
        ms_counts[ms] = ms_counts.get(ms, 0) + 1
    print("\nmental_state分布:")
    for ms, count in sorted(ms_counts.items(), key=lambda x: -x[1]):
        print(f"  {ms}: {count}人 ({count/5:.1f}%)")

    # 年齢分布
    age_counts = {}
    for p in personas:
        age = p["age_group"].split("_")[0]
        age_counts[age] = age_counts.get(age, 0) + 1
    print("\n年齢分布:")
    for age in ["10s", "20s", "30s", "40s", "50s", "60s", "70s", "80s"]:
        count = age_counts.get(age, 0)
        print(f"  {age}: {count}人 ({count/5:.1f}%)")


if __name__ == "__main__":
    main()
