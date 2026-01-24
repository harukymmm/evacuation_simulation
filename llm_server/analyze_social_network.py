#!/usr/bin/env python3
"""
社会的相互作用のネットワーク分析
FOLLOW/TALK/CONTACT行動の連鎖を分析
"""

import os
import json
import pandas as pd
import numpy as np
from pathlib import Path
from collections import defaultdict
import re

RESULT_DIR = Path("/Users/harukidoi/code/itolabo/simulation/evacuation_simulation/Logs/experiment_results/20260122_020101")

def load_agent_data(condition: str, trial: int) -> pd.DataFrame:
    """エージェントデータを読み込む"""
    filepath = RESULT_DIR / f"{condition}_trial_{trial}_agents.csv"
    if filepath.exists():
        return pd.read_csv(filepath)
    return None

def load_action_data(condition: str, trial: int) -> pd.DataFrame:
    """行動データを読み込む"""
    filepath = RESULT_DIR / f"{condition}_trial_{trial}_actions.csv"
    if filepath.exists():
        return pd.read_csv(filepath)
    return None

def analyze_follow_network(condition: str = "Baseline-LLM", trial: int = 1):
    """
    FOLLOW行動のネットワーク分析
    誰が誰を追従したかのネットワークを構築
    """
    actions_df = load_action_data(condition, trial)
    agents_df = load_agent_data(condition, trial)

    if actions_df is None or agents_df is None:
        return None

    # FOLLOWアクションを抽出（target_shelterにはFOLLOW先のIDが入っている場合がある）
    follow_actions = actions_df[actions_df['action_type'] == 'FOLLOW'].copy()

    # エージェント情報を辞書化
    agent_info = {}
    for _, agent in agents_df.iterrows():
        agent_info[agent['agent_id']] = {
            'name': agent['persona_name'],
            'mental_state': agent['mental_state'],
            'survived': agent['survived'],
            'evacuation_completed': agent['evacuation_completed']
        }

    # ネットワーク構築のための統計
    follow_stats = {
        'total_follow_actions': len(follow_actions),
        'unique_followers': follow_actions['agent_id'].nunique(),
        'follow_by_mental_state': defaultdict(int),
        'follow_chains': [],
        'early_evacuators_followed': 0,  # 早期避難者が追従された回数
    }

    # 認知特性別のFOLLOW回数
    for _, action in follow_actions.iterrows():
        agent_id = action['agent_id']
        if agent_id in agent_info:
            mental_state = agent_info[agent_id]['mental_state']
            follow_stats['follow_by_mental_state'][mental_state] += 1

    # FOLLOWした人の生存率
    followers = follow_actions['agent_id'].unique()
    follow_survived = sum(1 for f in followers if agent_info.get(f, {}).get('survived', False))
    follow_stats['follower_survival_rate'] = follow_survived / max(len(followers), 1) * 100

    # FOLLOWしなかった人の生存率
    all_agents = set(agents_df['agent_id'].tolist())
    non_followers = all_agents - set(followers)
    non_follow_survived = sum(1 for f in non_followers if agent_info.get(f, {}).get('survived', False))
    follow_stats['non_follower_survival_rate'] = non_follow_survived / max(len(non_followers), 1) * 100

    # FOLLOW回数別の生存率
    follow_counts = follow_actions.groupby('agent_id').size()
    follow_count_survival = defaultdict(lambda: {'total': 0, 'survived': 0})

    for agent_id, count in follow_counts.items():
        bucket = '1-2' if count <= 2 else ('3-5' if count <= 5 else '6+')
        follow_count_survival[bucket]['total'] += 1
        if agent_info.get(agent_id, {}).get('survived', False):
            follow_count_survival[bucket]['survived'] += 1

    follow_stats['survival_by_follow_count'] = {
        k: v['survived'] / max(v['total'], 1) * 100
        for k, v in follow_count_survival.items()
    }

    return follow_stats

def analyze_conformity_effect(trial: int = 1):
    """
    同調バイアス条件と通常条件でのFOLLOW行動比較
    """
    conditions = {
        'Baseline-LLM': 'ベースライン',
        'Exp2-3': '同調バイアス'
    }

    comparison = {}

    for condition, label in conditions.items():
        agents_df = load_agent_data(condition, trial)
        actions_df = load_action_data(condition, trial)

        if agents_df is None or actions_df is None:
            continue

        follow_actions = actions_df[actions_df['action_type'] == 'FOLLOW']

        # FOLLOW回数の分布
        follow_per_agent = follow_actions.groupby('agent_id').size()

        comparison[label] = {
            'total_follow_actions': len(follow_actions),
            'unique_followers': follow_per_agent.nunique() if len(follow_per_agent) > 0 else 0,
            'avg_follow_per_follower': follow_per_agent.mean() if len(follow_per_agent) > 0 else 0,
            'max_follow_count': follow_per_agent.max() if len(follow_per_agent) > 0 else 0,
            'evacuation_rate': agents_df['evacuation_completed'].mean() * 100,
            'survival_rate': agents_df['survived'].mean() * 100
        }

    return comparison

def analyze_early_evacuator_influence(condition: str = "Baseline-LLM", trial: int = 1):
    """
    早期避難者が周囲に与えた影響の分析
    発災直後（2分以内）に避難を開始したエージェントをリーダーとして、
    その後の周囲の行動変化を追跡
    """
    agents_df = load_agent_data(condition, trial)
    actions_df = load_action_data(condition, trial)

    if agents_df is None or actions_df is None:
        return None

    # 早期避難者（2分以内に避難開始）
    early_evacuators = agents_df[
        (agents_df['first_evacuate_time'] > 0) &
        (agents_df['first_evacuate_time'] <= 120)
    ]['agent_id'].tolist()

    # 遅延避難者（5分以降に避難開始）
    late_evacuators = agents_df[
        (agents_df['first_evacuate_time'] > 300)
    ]['agent_id'].tolist()

    # 避難未開始者
    never_evacuated = agents_df[
        agents_df['first_evacuate_time'] == -1
    ]['agent_id'].tolist()

    # 早期避難者の認知特性
    early_mental_states = agents_df[agents_df['agent_id'].isin(early_evacuators)]['mental_state'].value_counts()

    # 遅延避難者の認知特性
    late_mental_states = agents_df[agents_df['agent_id'].isin(late_evacuators)]['mental_state'].value_counts()

    # 早期避難者のFOLLOW行動（他者を引っ張ったか）
    early_evac_follow = actions_df[
        (actions_df['agent_id'].isin(early_evacuators)) &
        (actions_df['action_type'] == 'FOLLOW')
    ]

    result = {
        'early_evacuators_count': len(early_evacuators),
        'late_evacuators_count': len(late_evacuators),
        'never_evacuated_count': len(never_evacuated),
        'early_evacuators_mental_states': early_mental_states.to_dict(),
        'late_evacuators_mental_states': late_mental_states.to_dict(),
        'early_evacuators_survival_rate': agents_df[agents_df['agent_id'].isin(early_evacuators)]['survived'].mean() * 100,
        'late_evacuators_survival_rate': agents_df[agents_df['agent_id'].isin(late_evacuators)]['survived'].mean() * 100 if late_evacuators else 0,
    }

    return result

def analyze_information_cascade():
    """
    条件間での情報カスケード（同調行動の連鎖）の比較
    """
    conditions = ['Baseline-LLM', 'Exp2-2', 'Exp2-3', 'Exp2-4']
    cascade_stats = {}

    for condition in conditions:
        all_follow_times = []
        all_follow_counts = []

        for trial in range(1, 6):
            agents_df = load_agent_data(condition, trial)
            actions_df = load_action_data(condition, trial)

            if agents_df is None or actions_df is None:
                continue

            # 時間帯ごとのFOLLOW行動数
            follow_actions = actions_df[actions_df['action_type'] == 'FOLLOW']

            time_bins = [0, 120, 300, 600, 900, 1200, 1500, 1800]
            follow_by_time = pd.cut(follow_actions['timestamp'], bins=time_bins).value_counts().sort_index()

            all_follow_counts.append(len(follow_actions))

        cascade_stats[condition] = {
            'avg_follow_count': np.mean(all_follow_counts) if all_follow_counts else 0,
            'std_follow_count': np.std(all_follow_counts) if all_follow_counts else 0
        }

    return cascade_stats

def main():
    print("=" * 80)
    print("社会的相互作用のネットワーク分析")
    print("=" * 80)

    # FOLLOW行動のネットワーク分析
    print("\n[1] FOLLOW行動の基本統計")
    print("-" * 40)

    follow_stats = analyze_follow_network("Baseline-LLM", 1)
    if follow_stats:
        print(f"総FOLLOW行動数: {follow_stats['total_follow_actions']}")
        print(f"FOLLOWを行ったエージェント数: {follow_stats['unique_followers']}")
        print(f"\nFOLLOWした人の生存率: {follow_stats['follower_survival_rate']:.1f}%")
        print(f"FOLLOWしなかった人の生存率: {follow_stats['non_follower_survival_rate']:.1f}%")

        print(f"\n認知特性別のFOLLOW回数:")
        for state, count in sorted(follow_stats['follow_by_mental_state'].items(), key=lambda x: -x[1]):
            print(f"  {state}: {count}回")

        print(f"\nFOLLOW回数別の生存率:")
        for bucket, rate in follow_stats['survival_by_follow_count'].items():
            print(f"  {bucket}回: {rate:.1f}%")

    # 同調バイアス条件との比較
    print("\n[2] 同調バイアス条件との比較")
    print("-" * 40)

    conformity = analyze_conformity_effect(1)
    if conformity:
        for label, stats in conformity.items():
            print(f"\n{label}:")
            print(f"  総FOLLOW行動数: {stats['total_follow_actions']}")
            print(f"  FOLLOWを行った人数: {stats['unique_followers']}")
            print(f"  1人あたり平均FOLLOW回数: {stats['avg_follow_per_follower']:.1f}")
            print(f"  避難完了率: {stats['evacuation_rate']:.1f}%")
            print(f"  生存率: {stats['survival_rate']:.1f}%")

    # 早期避難者の影響
    print("\n[3] 早期避難者と遅延避難者の比較")
    print("-" * 40)

    influence = analyze_early_evacuator_influence("Baseline-LLM", 1)
    if influence:
        print(f"早期避難者（2分以内）: {influence['early_evacuators_count']}名")
        print(f"  生存率: {influence['early_evacuators_survival_rate']:.1f}%")
        print(f"  認知特性分布:")
        for state, count in sorted(influence['early_evacuators_mental_states'].items(), key=lambda x: -x[1])[:3]:
            print(f"    {state}: {count}名")

        print(f"\n遅延避難者（5分以降）: {influence['late_evacuators_count']}名")
        print(f"  生存率: {influence['late_evacuators_survival_rate']:.1f}%")
        print(f"  認知特性分布:")
        for state, count in sorted(influence['late_evacuators_mental_states'].items(), key=lambda x: -x[1])[:3]:
            print(f"    {state}: {count}名")

        print(f"\n避難未開始者: {influence['never_evacuated_count']}名")

    # 情報カスケード分析
    print("\n[4] 条件間のFOLLOW行動比較")
    print("-" * 40)

    cascade = analyze_information_cascade()
    if cascade:
        condition_labels = {
            'Baseline-LLM': 'ベースライン',
            'Exp2-2': '正常性バイアス',
            'Exp2-3': '同調バイアス',
            'Exp2-4': '複合バイアス'
        }
        for condition, stats in cascade.items():
            label = condition_labels.get(condition, condition)
            print(f"{label}: 平均FOLLOW {stats['avg_follow_count']:.1f}回 (±{stats['std_follow_count']:.1f})")

    # 結果を保存
    output = {
        'follow_network': follow_stats,
        'conformity_comparison': conformity,
        'early_late_comparison': influence,
        'cascade_stats': cascade
    }

    # numpy型をJSONに変換
    def convert_for_json(obj):
        if isinstance(obj, (np.int64, np.int32)):
            return int(obj)
        elif isinstance(obj, (np.float64, np.float32)):
            return float(obj)
        elif isinstance(obj, np.ndarray):
            return obj.tolist()
        elif isinstance(obj, dict):
            return {k: convert_for_json(v) for k, v in obj.items()}
        elif isinstance(obj, list):
            return [convert_for_json(v) for v in obj]
        return obj

    output_converted = convert_for_json(output)

    output_path = RESULT_DIR / "social_network_analysis.json"
    with open(output_path, 'w', encoding='utf-8') as f:
        json.dump(output_converted, f, ensure_ascii=False, indent=2)

    print(f"\n結果を保存: {output_path}")

if __name__ == "__main__":
    main()
