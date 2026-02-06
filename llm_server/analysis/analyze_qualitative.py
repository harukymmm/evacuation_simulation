#!/usr/bin/env python3
"""
論文のための定性的分析スクリプト
実験結果から以下の分析を行う:
1. 避難失敗者の類型化
2. LLM推論パターンの抽出
3. 認知特性別の行動プロファイル
4. 時間軸での行動変化
5. 避難所選択パターン
"""

import os
import sys
import json
import pandas as pd
import numpy as np
from pathlib import Path
from collections import defaultdict
import re
import argparse

from utils import set_result_dir, get_result_dir, load_agent_data, load_action_data

def analyze_failure_types(condition: str = "Baseline-LLM", trial: int = 1):
    """
    Phase 4: 避難失敗者の類型化
    - 完全待機型: 一度も移動せず
    - 家族優先型: SEARCH_FAMILY/CONTACT多用
    - 追従迷走型: FOLLOW多用だが到達せず
    - 遅延開始型: 遅くに避難開始したが間に合わず
    """
    agents_df = load_agent_data(condition, trial)
    actions_df = load_action_data(condition, trial)

    if agents_df is None or actions_df is None:
        return None

    # 失敗者を抽出
    failed_agents = agents_df[agents_df['survived'] == False]

    failure_types = {
        'complete_stay': [],      # 完全待機型
        'family_priority': [],    # 家族優先型
        'follow_wandering': [],   # 追従迷走型
        'late_start': [],         # 遅延開始型
        'other': []               # その他
    }

    failure_details = []

    for _, agent in failed_agents.iterrows():
        agent_id = agent['agent_id']
        agent_actions = actions_df[actions_df['agent_id'] == agent_id]

        # 行動カウント
        action_counts = agent_actions['action_type'].value_counts().to_dict()
        total_actions = len(agent_actions)

        evacuate_count = action_counts.get('EVACUATE', 0)
        stay_count = action_counts.get('STAY', 0)
        follow_count = action_counts.get('FOLLOW', 0)
        search_family_count = action_counts.get('SEARCH_FAMILY', 0)
        contact_count = action_counts.get('CONTACT', 0)
        talk_count = action_counts.get('TALK', 0)

        # 最初のEVACUATE時刻
        first_evac_time = agent['first_evacuate_time']

        # 類型判定
        failure_type = 'other'
        reason = ''

        if first_evac_time == -1 and evacuate_count == 0 and follow_count == 0:
            # 一度も移動しなかった
            if stay_count / max(total_actions, 1) > 0.8:
                failure_type = 'complete_stay'
                reason = f'全{total_actions}回中{stay_count}回がSTAY'
            elif (search_family_count + contact_count) / max(total_actions, 1) > 0.3:
                failure_type = 'family_priority'
                reason = f'SEARCH_FAMILY {search_family_count}回, CONTACT {contact_count}回'
            else:
                failure_type = 'complete_stay'
                reason = f'避難開始せず（STAY {stay_count}回）'
        elif (search_family_count + contact_count) / max(total_actions, 1) > 0.3:
            failure_type = 'family_priority'
            reason = f'SEARCH_FAMILY {search_family_count}回, CONTACT {contact_count}回'
        elif follow_count > 5 and evacuate_count == 0:
            failure_type = 'follow_wandering'
            reason = f'FOLLOW {follow_count}回したが避難所未到達'
        elif first_evac_time > 900:  # 15分以上経過後に開始
            failure_type = 'late_start'
            reason = f'避難開始時刻: {first_evac_time:.1f}秒後'
        elif follow_count > 3:
            failure_type = 'follow_wandering'
            reason = f'FOLLOW {follow_count}回したが避難所未到達'
        else:
            failure_type = 'other'
            reason = f'行動: EVAC{evacuate_count}, STAY{stay_count}, FOLLOW{follow_count}'

        failure_types[failure_type].append(agent_id)

        # 詳細情報を記録
        failure_details.append({
            'agent_id': agent_id,
            'persona_name': agent['persona_name'],
            'mental_state': agent['mental_state'],
            'failure_type': failure_type,
            'reason': reason,
            'first_evacuate_time': first_evac_time,
            'total_stay_duration': agent['total_stay_duration'],
            'follow_count': follow_count,
            'search_family_count': search_family_count,
            'contact_count': contact_count,
            'total_actions': total_actions,
            'final_elevation': agent['final_elevation']
        })

    # 統計まとめ
    total_failed = len(failed_agents)
    stats = {
        'total_failed': total_failed,
        'complete_stay': len(failure_types['complete_stay']),
        'family_priority': len(failure_types['family_priority']),
        'follow_wandering': len(failure_types['follow_wandering']),
        'late_start': len(failure_types['late_start']),
        'other': len(failure_types['other'])
    }

    # 割合計算
    for key in ['complete_stay', 'family_priority', 'follow_wandering', 'late_start', 'other']:
        stats[f'{key}_pct'] = stats[key] / max(total_failed, 1) * 100

    return {
        'stats': stats,
        'details': failure_details,
        'types': failure_types
    }

def analyze_reasoning_patterns(condition: str = "Baseline-LLM", trial: int = 1):
    """
    Phase 2: LLMの推論パターン分析
    避難成功者と失敗者の推論を比較
    """
    agents_df = load_agent_data(condition, trial)
    actions_df = load_action_data(condition, trial)

    if agents_df is None or actions_df is None:
        return None

    # 成功者と失敗者を分離
    succeeded = agents_df[agents_df['survived'] == True]['agent_id'].tolist()
    failed = agents_df[agents_df['survived'] == False]['agent_id'].tolist()

    reasoning_analysis = {
        'succeeded_stay_reasons': [],
        'failed_stay_reasons': [],
        'succeeded_evacuate_reasons': [],
        'failed_evacuate_reasons': [],
        'transition_moments': []  # 様子見→避難への転換点
    }

    # 成功者の推論
    for agent_id in succeeded[:20]:  # サンプル
        agent_actions = actions_df[actions_df['agent_id'] == agent_id]

        # STAY時の推論
        stay_actions = agent_actions[agent_actions['action_type'] == 'STAY']
        for _, row in stay_actions.iterrows():
            reasoning_analysis['succeeded_stay_reasons'].append({
                'agent_id': agent_id,
                'timestamp': row['timestamp'],
                'reasoning': row['reasoning']
            })

        # EVACUATE時の推論
        evac_actions = agent_actions[agent_actions['action_type'] == 'EVACUATE']
        for _, row in evac_actions.head(1).iterrows():  # 最初のEVACUATEのみ
            reasoning_analysis['succeeded_evacuate_reasons'].append({
                'agent_id': agent_id,
                'timestamp': row['timestamp'],
                'reasoning': row['reasoning']
            })

        # 転換点の特定（STAY→EVACUATEの直前直後）
        prev_action = None
        for _, row in agent_actions.iterrows():
            if prev_action is not None and prev_action['action_type'] == 'STAY' and row['action_type'] == 'EVACUATE':
                reasoning_analysis['transition_moments'].append({
                    'agent_id': agent_id,
                    'before_timestamp': prev_action['timestamp'],
                    'before_reasoning': prev_action['reasoning'],
                    'after_timestamp': row['timestamp'],
                    'after_reasoning': row['reasoning']
                })
            prev_action = row.to_dict()

    # 失敗者の推論
    for agent_id in failed[:20]:  # サンプル
        agent_actions = actions_df[actions_df['agent_id'] == agent_id]

        # STAY時の推論
        stay_actions = agent_actions[agent_actions['action_type'] == 'STAY']
        for _, row in stay_actions.head(3).iterrows():  # 最初の3回
            reasoning_analysis['failed_stay_reasons'].append({
                'agent_id': agent_id,
                'timestamp': row['timestamp'],
                'reasoning': row['reasoning']
            })

    return reasoning_analysis

def analyze_mental_state_profile(condition: str = "Baseline-LLM"):
    """
    Phase 5: 認知特性別の詳細プロファイル
    """
    all_agents = []
    all_actions = []

    for trial in range(1, 6):
        agents_df = load_agent_data(condition, trial)
        actions_df = load_action_data(condition, trial)
        if agents_df is not None:
            agents_df['trial'] = trial
            all_agents.append(agents_df)
        if actions_df is not None:
            actions_df['trial'] = trial
            all_actions.append(actions_df)

    if not all_agents:
        return None

    agents_df = pd.concat(all_agents, ignore_index=True)
    actions_df = pd.concat(all_actions, ignore_index=True)

    mental_states = agents_df['mental_state'].unique()
    profiles = {}

    for state in mental_states:
        state_agents = agents_df[agents_df['mental_state'] == state]

        # 基本統計
        profile = {
            'count': len(state_agents),
            'evacuation_rate': state_agents['evacuation_completed'].mean() * 100,
            'survival_rate': state_agents['survived'].mean() * 100,
            'avg_first_evacuate_time': state_agents[state_agents['first_evacuate_time'] > 0]['first_evacuate_time'].mean(),
            'avg_stay_duration': state_agents['total_stay_duration'].mean(),
            'avg_follow_count': state_agents['follow_count'].mean(),
            'avg_contact_count': state_agents['contact_count'].mean(),
            'avg_search_family_count': state_agents['search_family_count'].mean(),
            'never_started_rate': (state_agents['first_evacuate_time'] == -1).mean() * 100
        }

        # 行動分布（この認知特性のエージェントの行動）
        state_agent_ids = state_agents['agent_id'].unique()
        state_actions = actions_df[actions_df['agent_id'].isin(state_agent_ids)]
        if len(state_actions) > 0:
            action_dist = state_actions['action_type'].value_counts(normalize=True) * 100
            profile['action_distribution'] = action_dist.to_dict()

        profiles[state] = profile

    return profiles

def analyze_temporal_behavior(condition: str = "Baseline-LLM", trial: int = 1):
    """
    Phase 7: 時間軸での行動変化分析
    """
    actions_df = load_action_data(condition, trial)
    agents_df = load_agent_data(condition, trial)

    if actions_df is None:
        return None

    # 時間帯ごとの行動分布
    time_bins = [0, 120, 300, 600, 900, 1200, 1500, 1800]
    time_labels = ['0-2min', '2-5min', '5-10min', '10-15min', '15-20min', '20-25min', '25-30min']

    actions_df['time_bin'] = pd.cut(actions_df['timestamp'], bins=time_bins, labels=time_labels, right=False)

    temporal_stats = []
    for label in time_labels:
        bin_actions = actions_df[actions_df['time_bin'] == label]
        if len(bin_actions) == 0:
            continue

        action_counts = bin_actions['action_type'].value_counts()
        total = len(bin_actions)

        stat = {
            'time_bin': label,
            'total_actions': total,
            'EVACUATE': action_counts.get('EVACUATE', 0),
            'STAY': action_counts.get('STAY', 0),
            'FOLLOW': action_counts.get('FOLLOW', 0),
            'CONTACT': action_counts.get('CONTACT', 0),
            'SEARCH_FAMILY': action_counts.get('SEARCH_FAMILY', 0),
            'TALK': action_counts.get('TALK', 0)
        }

        # 割合も計算
        for action in ['EVACUATE', 'STAY', 'FOLLOW', 'CONTACT', 'SEARCH_FAMILY', 'TALK']:
            stat[f'{action}_pct'] = stat[action] / max(total, 1) * 100

        temporal_stats.append(stat)

    # 避難開始者数の時間推移
    evacuation_starts = []
    for t in range(0, 1800, 60):  # 1分間隔
        # この時点までに避難を開始したエージェント数
        started = len(agents_df[(agents_df['first_evacuate_time'] >= 0) & (agents_df['first_evacuate_time'] <= t)])
        evacuation_starts.append({
            'time_sec': t,
            'time_min': t / 60,
            'cumulative_started': started,
            'cumulative_rate': started / len(agents_df) * 100
        })

    return {
        'action_distribution_by_time': temporal_stats,
        'evacuation_start_timeline': evacuation_starts
    }

def analyze_shelter_choice_patterns():
    """
    Phase 6: 避難所選択パターンの分析
    """
    conditions = ['Baseline-LLM', 'Exp1-B', 'Exp2-2', 'Exp2-3', 'Exp2-4', 'Exp3-B', 'Exp3-C', 'Exp3-D']

    shelter_stats = {}

    for condition in conditions:
        all_agents = []
        for trial in range(1, 6):
            agents_df = load_agent_data(condition, trial)
            if agents_df is not None:
                all_agents.append(agents_df)

        if not all_agents:
            continue

        agents_df = pd.concat(all_agents, ignore_index=True)

        # 避難所ごとの選択率
        evacuated = agents_df[agents_df['evacuation_completed'] == True]
        shelter_counts = evacuated['final_shelter'].value_counts()
        total_evacuated = len(evacuated)

        shelter_rates = {}
        for shelter, count in shelter_counts.items():
            shelter_rates[shelter] = {
                'count': count / 5,  # 5トライアルの平均
                'rate': count / max(total_evacuated, 1) * 100
            }

        # 分散度（エントロピー的な指標）
        probs = shelter_counts.values / max(total_evacuated, 1)
        entropy = -np.sum(probs * np.log(probs + 1e-10))

        shelter_stats[condition] = {
            'total_evacuated': total_evacuated / 5,
            'shelter_distribution': shelter_rates,
            'diversity_entropy': entropy,
            'num_shelters_used': len(shelter_counts)
        }

    return shelter_stats

def generate_paper_examples():
    """
    論文に記載する具体例を生成
    """
    examples = {}

    # 1. 推論パターンの例
    reasoning = analyze_reasoning_patterns("Baseline-LLM", 1)
    if reasoning:
        examples['stay_reasoning_examples'] = reasoning['failed_stay_reasons'][:5]
        examples['transition_examples'] = reasoning['transition_moments'][:3]

    # 2. 失敗事例の例
    failure = analyze_failure_types("Baseline-LLM", 1)
    if failure:
        examples['failure_stats'] = failure['stats']
        # 各類型から1-2例を抽出
        for ftype in ['complete_stay', 'family_priority', 'follow_wandering', 'late_start']:
            type_examples = [d for d in failure['details'] if d['failure_type'] == ftype][:2]
            examples[f'{ftype}_examples'] = type_examples

    # 3. 認知特性別プロファイル
    profiles = analyze_mental_state_profile("Baseline-LLM")
    if profiles:
        examples['mental_state_profiles'] = profiles

    return examples

def main():
    print("=" * 80)
    print("論文のための定性的分析")
    print("=" * 80)

    # Phase 4: 避難失敗者の類型化
    print("\n[Phase 4] 避難失敗者の類型化分析")
    print("-" * 40)

    failure_analysis = analyze_failure_types("Baseline-LLM", 1)
    if failure_analysis:
        stats = failure_analysis['stats']
        print(f"総失敗者数: {stats['total_failed']}名")
        print(f"\n類型分布:")
        print(f"  完全待機型: {stats['complete_stay']}名 ({stats['complete_stay_pct']:.1f}%)")
        print(f"  家族優先型: {stats['family_priority']}名 ({stats['family_priority_pct']:.1f}%)")
        print(f"  追従迷走型: {stats['follow_wandering']}名 ({stats['follow_wandering_pct']:.1f}%)")
        print(f"  遅延開始型: {stats['late_start']}名 ({stats['late_start_pct']:.1f}%)")
        print(f"  その他: {stats['other']}名 ({stats['other_pct']:.1f}%)")

        # 具体例
        print("\n【具体例】")
        for detail in failure_analysis['details'][:5]:
            print(f"  {detail['persona_name']}({detail['mental_state']}): {detail['failure_type']}")
            print(f"    理由: {detail['reason']}")

    # Phase 2: 推論パターン分析
    print("\n[Phase 2] 推論パターン分析")
    print("-" * 40)

    reasoning = analyze_reasoning_patterns("Baseline-LLM", 1)
    if reasoning:
        print("\n【失敗者のSTAY時の推論例】")
        for r in reasoning['failed_stay_reasons'][:3]:
            print(f"  Agent {r['agent_id']} (t={r['timestamp']:.1f}s):")
            print(f"    「{r['reasoning']}」")

        print("\n【様子見→避難への転換点】")
        for t in reasoning['transition_moments'][:2]:
            print(f"  Agent {t['agent_id']}:")
            print(f"    Before (t={t['before_timestamp']:.1f}s): 「{t['before_reasoning']}」")
            print(f"    After (t={t['after_timestamp']:.1f}s): 「{t['after_reasoning']}」")

    # Phase 5: 認知特性別プロファイル
    print("\n[Phase 5] 認知特性別プロファイル")
    print("-" * 40)

    profiles = analyze_mental_state_profile("Baseline-LLM")
    if profiles:
        for state, profile in profiles.items():
            if profile['count'] > 0:
                print(f"\n{state}:")
                print(f"  避難完了率: {profile['evacuation_rate']:.1f}%")
                print(f"  生存率: {profile['survival_rate']:.1f}%")
                print(f"  平均避難開始時刻: {profile['avg_first_evacuate_time']:.1f}秒")
                print(f"  避難未開始率: {profile['never_started_rate']:.1f}%")
                print(f"  平均FOLLOW回数: {profile['avg_follow_count']:.1f}")

    # Phase 7: 時間軸分析
    print("\n[Phase 7] 時間軸での行動変化")
    print("-" * 40)

    temporal = analyze_temporal_behavior("Baseline-LLM", 1)
    if temporal:
        print("\n時間帯別の行動割合:")
        for stat in temporal['action_distribution_by_time']:
            print(f"  {stat['time_bin']}: EVAC {stat['EVACUATE_pct']:.1f}%, STAY {stat['STAY_pct']:.1f}%, FOLLOW {stat['FOLLOW_pct']:.1f}%")

    # Phase 6: 避難所選択パターン
    print("\n[Phase 6] 避難所選択パターン")
    print("-" * 40)

    shelter = analyze_shelter_choice_patterns()
    if shelter:
        for condition in ['Baseline-LLM', 'Exp1-B']:
            if condition in shelter:
                print(f"\n{condition}:")
                print(f"  避難完了者数: {shelter[condition]['total_evacuated']:.1f}")
                print(f"  利用避難所数: {shelter[condition]['num_shelters_used']}")
                print(f"  分散度(エントロピー): {shelter[condition]['diversity_entropy']:.2f}")
                print(f"  主要避難所:")
                for s, data in sorted(shelter[condition]['shelter_distribution'].items(),
                                     key=lambda x: -x[1]['rate'])[:3]:
                    print(f"    {s}: {data['rate']:.1f}%")

    # 結果をJSONで保存
    output = {
        'failure_analysis': failure_analysis,
        'reasoning_patterns': reasoning,
        'mental_state_profiles': profiles,
        'temporal_analysis': temporal,
        'shelter_patterns': shelter
    }

    output_path = get_result_dir() / "qualitative_analysis_results.json"

    # DataFrameやnumpy型をJSON変換可能な形式に
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

    with open(output_path, 'w', encoding='utf-8') as f:
        json.dump(output_converted, f, ensure_ascii=False, indent=2)

    print(f"\n結果を保存: {output_path}")

if __name__ == "__main__":
    parser = argparse.ArgumentParser(description='論文のための定性的分析スクリプト')
    parser.add_argument('log_dir', type=str, help='分析対象のログディレクトリパス')
    args = parser.parse_args()

    result_dir = Path(args.log_dir)
    if not result_dir.exists():
        print(f"エラー: 指定されたディレクトリが存在しません: {result_dir}")
        sys.exit(1)

    set_result_dir(result_dir)
    main()
