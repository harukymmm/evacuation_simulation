import json
import random
from faker import Faker

# 日本語データの生成器
fake = Faker('ja_JP')

def generate_realistic_persona(num_personas=5):
    personas = []
    
    for _ in range(num_personas):
        # --- 1. 基本属性の生成 ---
        uuid = fake.uuid4()
        profile = fake.profile()
        name = profile['name']
        age = random.randint(10, 90) # 10歳〜90歳
        
        # --- 2. 家族構成と社会的役割の推論 ---
        # 年齢に基づいて家族構成を確率的に決定
        if age < 18:
            family_type = "親と同居"
            role = "被保護者"
        elif 18 <= age < 30:
            family_type = random.choice(["単身", "親と同居"])
            role = "単独行動者"
        elif 30 <= age < 65:
            family_type = random.choice(["夫婦のみ", "子供あり", "親と同居(介護)"])
            role = "保護者/リーダー"
        else: # 65歳以上
            family_type = random.choice(["夫婦のみ", "独居", "子供と同居"])
            role = "要支援者候補"

        # --- 3. 身体能力（移動速度）の算出 ---
        # 基本速度 1.2m/s から年齢と健康状態で減衰させる
        base_speed = random.uniform(1.0, 1.4)
        health_status = "健康"
        
        if age >= 75:
            health_status = random.choice(["健康", "足腰が弱い", "要介護(車椅子)"])
            if health_status == "足腰が弱い":
                base_speed *= 0.6
            elif health_status == "要介護(車椅子)":
                base_speed *= 0.3 # 誰かの介助がないと動けないレベル
        elif age <= 10:
            base_speed *= 0.7 # 子供

        # --- 4. 心理特性（避難トリガー）の設定 ---
        # これが「いつ逃げるか」に影響する
        psychology_types = [
            {"type": "正常性バイアス強", "desc": "警報が鳴っても『誤報だろう』と考え、周囲が動くまで動かない。"},
            {"type": "慎重・心配性", "desc": "わずかな予兆ですぐに避難準備を始める。"},
            {"type": "同調性重視", "desc": "近所の人が逃げ始めたら自分も逃げる。"},
            {"type": "パニック傾向", "desc": "情報が入ると混乱し、冷静な判断ができなくなる可能性がある。"}
        ]
        # 高齢者ほど正常性バイアスがかかりやすい傾向を反映（確率調整）
        if age > 60:
            psycho = random.choices(psychology_types, weights=[50, 20, 20, 10])[0]
        else:
            psycho = random.choices(psychology_types, weights=[30, 30, 30, 10])[0]

        # --- 5. 情報リテラシーと地域知識 ---
        is_local = random.choices([True, False], weights=[0.9, 0.1])[0] # 1割は観光客/来訪者
        
        info_channel = "テレビ・防災無線"
        if age < 50:
            info_channel = "SNS・スマホアプリ"
        elif 50 <= age < 70:
            info_channel = "テレビ・スマホニュース"
            
        # --- 6. LLM用システムプロンプトの生成 ---
        # 構造化データだけでなく、LLMが「演じる」ための自然言語テキストを生成
        
        context_text = (
            f"あなたの名前は{name}、{age}歳です。家族構成は{family_type}です。"
            f"現在の健康状態は「{health_status}」で、移動速度は{'普通' if base_speed > 1.0 else '遅い'}です。"
            f"性格・避難行動の特徴として、{psycho['desc']}"
            f"{'地元住民なので裏道を知っています。' if is_local else 'この地域には詳しくないため、案内看板や他者に依存します。'}"
            f"主な情報源は{info_channel}です。"
            f"もし災害が発生した場合、あなたは上記の性格と身体能力、家族への責任に基づいて行動を決定してください。"
        )

        # データ構築
        persona_data = {
            "id": uuid,
            "basic_info": {
                "name": name,
                "age": age,
                "family": family_type,
                "role": role
            },
            "physical_param": {
                "health": health_status,
                "speed_m_s": round(base_speed, 2)
            },
            "psychological_param": {
                "bias_type": psycho['type'],
                "description": psycho['desc']
            },
            "context_param": {
                "is_local": is_local,
                "info_source": info_channel
            },
            "llm_system_prompt": context_text  # ここをUnityのLLMに入力する
        }
        
        personas.append(persona_data)

    return personas

# 実行してJSONを出力
generated_data = generate_realistic_persona(3)
print(json.dumps(generated_data, indent=2, ensure_ascii=False))