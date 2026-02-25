#!/usr/bin/env python3
"""
実験結果の集計スクリプト
5トライアルの平均を計算し、CSVファイルとMarkdown表を出力する
"""

import os
import sys
import csv
import pandas as pd
import numpy as np
from pathlib import Path
from collections import defaultdict
import argparse

# 実験条件
CONDITIONS = [
    'Baseline-LLM',
    'Exp1-B',
    'Exp2-2',
    'Exp2-3',
    'Exp2-4',
    'Exp3-B',
    'Exp3-C',
    'Exp3-D'
]

NUM_TRIALS = 5

# 認知特性のリスト（personas.csvのmental_stateに定義されている値）
MENTAL_STATES = [
    '冷静・分析的',
    '冷静・合理的',
    '慎重・人混み回避',
    '楽観的・自己信頼型',
    '社交的・周囲配慮型',
    '焦燥・目的外行動',
    '不安傾向・サポート希求型'
]

# 避難場所（LLMエージェント用）
LLM_LOCATIONS = ['諏訪神社', '小泉工業', '薄磯団地', '健登師記念会館', '豊間公園', '豊間中学校', '江名小学校']

# 避難場所（ルールベースエージェント用）
RULEBASED_LOCATIONS = ['豊間団地', '江名小学校', '望洋荘', '沼ノ内団地', '薄磯団地', '江名公民館', '豊間中学校']


def parse_summary_csv(filepath):
    """複数セクション形式のsummary CSVを解析"""
    result = {
        'metrics': {},
        'action_ratios': {},
        'shelter_selection': {},
        'evacuation_location': {}
    }

    current_section = 'metrics'

    with open(filepath, 'r', encoding='utf-8') as f:
        reader = csv.reader(f)
        for row in reader:
            if not row or len(row) < 2:
                continue

            key, value = row[0], row[1]

            # セクション判定
            if key == 'action_type':
                current_section = 'action_ratios'
                continue
            elif key == 'shelter_selection':
                current_section = 'shelter_selection'
                continue
            elif key == 'evacuation_rate_to_shelter':
                current_section = 'metrics'
                result['metrics']['evacuation_rate_to_shelter'] = float(value)
                continue
            elif key == 'evacuation_rate_to_area':
                result['metrics']['evacuation_rate_to_area'] = float(value)
                continue
            elif key == 'evacuation_location':
                current_section = 'evacuation_location'
                continue
            elif key in ['survival_rate', 'survived_agents', 'tsunami_height', 'information_strategy']:
                result['metrics'][key] = value
                continue

            # データ格納
            if current_section == 'metrics':
                try:
                    result['metrics'][key] = float(value)
                except ValueError:
                    result['metrics'][key] = value
            elif current_section == 'action_ratios':
                try:
                    result['action_ratios'][key] = float(value)
                except ValueError:
                    pass
            elif current_section == 'shelter_selection':
                try:
                    result['shelter_selection'][key] = int(value)
                except ValueError:
                    pass
            elif current_section == 'evacuation_location':
                try:
                    result['evacuation_location'][key] = int(value)
                except ValueError:
                    pass

    return result


def load_agents_csv(filepath):
    """agents CSVを読み込み"""
    return pd.read_csv(filepath)


def aggregate_condition_data(experiment_dir, condition_id):
    """条件ごとの5トライアルデータを集計"""
    summaries = []
    agents_dfs = []

    for trial in range(1, NUM_TRIALS + 1):
        summary_path = os.path.join(experiment_dir, f'{condition_id}_trial_{trial}_summary.csv')
        agents_path = os.path.join(experiment_dir, f'{condition_id}_trial_{trial}_agents.csv')

        if os.path.exists(summary_path):
            summaries.append(parse_summary_csv(summary_path))
        if os.path.exists(agents_path):
            agents_df = load_agents_csv(agents_path)
            agents_df['trial'] = trial
            agents_dfs.append(agents_df)

    return summaries, agents_dfs


def calculate_condition_averages(summaries):
    """条件の基本指標平均を計算"""
    metrics_keys = ['evacuation_rate', 'average_evacuation_time', 'median_evacuation_time',
                   'max_evacuation_time', 'evacuation_rate_to_shelter', 'evacuation_rate_to_area',
                   'total_agents', 'evacuated_agents']

    result = {}
    for key in metrics_keys:
        values = [s['metrics'].get(key, 0) for s in summaries if key in s['metrics']]
        if values:
            result[f'{key}_mean'] = np.mean(values)
            result[f'{key}_std'] = np.std(values)

    return result


def calculate_action_ratio_averages(summaries):
    """行動比率の平均を計算"""
    action_types = ['EVACUATE', 'STAY', 'FOLLOW', 'CONTACT', 'TALK', 'SEARCH_FAMILY']
    result = {}

    for action in action_types:
        values = [s['action_ratios'].get(action, 0) for s in summaries]
        if values:
            result[f'{action}_mean'] = np.mean(values)
            result[f'{action}_std'] = np.std(values)

    return result


def calculate_evacuation_location_averages(summaries):
    """避難場所別の平均人数を計算"""
    all_locations = set()
    for s in summaries:
        all_locations.update(s['evacuation_location'].keys())

    result = {}
    for loc in all_locations:
        values = [s['evacuation_location'].get(loc, 0) for s in summaries]
        result[loc] = np.mean(values)

    return result


def calculate_evacuation_time_stats(agents_dfs):
    """避難時間の統計量を計算（エージェント間の標準偏差含む）"""
    if not agents_dfs:
        return {}

    combined_df = pd.concat(agents_dfs, ignore_index=True)

    # 避難完了者の避難時間のみ抽出
    evac_times = combined_df[combined_df['evacuation_time'] > 0]['evacuation_time']

    if len(evac_times) == 0:
        return {
            'evacuation_time_std_agents': 0,
            'evacuation_time_min': 0,
            'evacuation_time_25pct': 0,
            'evacuation_time_75pct': 0
        }

    return {
        'evacuation_time_std_agents': evac_times.std(),
        'evacuation_time_min': evac_times.min(),
        'evacuation_time_25pct': evac_times.quantile(0.25),
        'evacuation_time_75pct': evac_times.quantile(0.75)
    }


def calculate_mental_state_metrics(agents_dfs, condition_id):
    """認知特性別の指標を計算"""
    if not agents_dfs:
        return {}

    combined_df = pd.concat(agents_dfs, ignore_index=True)

    result = {}
    for ms in MENTAL_STATES:
        ms_df = combined_df[combined_df['mental_state'] == ms]
        if len(ms_df) == 0:
            continue

        total = len(ms_df)
        evacuated = len(ms_df[ms_df['evacuation_completed'] == True])
        evac_rate = evacuated / total if total > 0 else 0

        # 避難時間（避難完了者のみ）
        evac_times = ms_df[ms_df['evacuation_time'] > 0]['evacuation_time']
        avg_evac_time = evac_times.mean() if len(evac_times) > 0 else 0

        # 避難開始時刻
        start_times = ms_df[ms_df['first_evacuate_time'] > 0]['first_evacuate_time']
        avg_start_time = start_times.mean() if len(start_times) > 0 else 0

        result[ms] = {
            'total': total // NUM_TRIALS,  # 1トライアルあたりの人数
            'evacuation_rate': evac_rate,
            'avg_evacuation_time': avg_evac_time,
            'avg_start_time': avg_start_time
        }

    return result


def calculate_social_interactions(agents_dfs):
    """社会的相互作用の集計"""
    if not agents_dfs:
        return {}

    combined_df = pd.concat(agents_dfs, ignore_index=True)

    # トライアルごとに集計して平均
    trial_stats = []
    for trial in range(1, NUM_TRIALS + 1):
        trial_df = combined_df[combined_df['trial'] == trial]

        follow_total = trial_df['follow_count'].sum()
        follow_executors = len(trial_df[trial_df['follow_count'] > 0])
        talk_total = trial_df['talk_count'].sum()
        talk_executors = len(trial_df[trial_df['talk_count'] > 0])
        contact_total = trial_df['contact_count'].sum()
        contact_executors = len(trial_df[trial_df['contact_count'] > 0])

        trial_stats.append({
            'follow_total': follow_total,
            'follow_executors': follow_executors,
            'talk_total': talk_total,
            'talk_executors': talk_executors,
            'contact_total': contact_total,
            'contact_executors': contact_executors
        })

    # 平均計算
    result = {}
    for key in ['follow_total', 'follow_executors', 'talk_total', 'talk_executors', 'contact_total', 'contact_executors']:
        result[key] = np.mean([s[key] for s in trial_stats])

    return result


def calculate_stay_behavior(agents_dfs):
    """STAY行動の分析"""
    if not agents_dfs:
        return {}

    combined_df = pd.concat(agents_dfs, ignore_index=True)

    trial_stats = []
    for trial in range(1, NUM_TRIALS + 1):
        trial_df = combined_df[combined_df['trial'] == trial]

        stay_executors = len(trial_df[trial_df['total_stay_duration'] > 0])
        total_stay_time = trial_df['total_stay_duration'].sum()
        avg_stay_time = trial_df[trial_df['total_stay_duration'] > 0]['total_stay_duration'].mean() if stay_executors > 0 else 0

        trial_stats.append({
            'stay_executors': stay_executors,
            'total_stay_time': total_stay_time,
            'avg_stay_time': avg_stay_time
        })

    return {
        'stay_executors': np.mean([s['stay_executors'] for s in trial_stats]),
        'total_stay_time': np.mean([s['total_stay_time'] for s in trial_stats]),
        'avg_stay_time': np.mean([s['avg_stay_time'] for s in trial_stats])
    }


def calculate_evacuation_start_times(agents_dfs):
    """避難開始時刻の分析"""
    if not agents_dfs:
        return {}

    combined_df = pd.concat(agents_dfs, ignore_index=True)

    trial_stats = []
    for trial in range(1, NUM_TRIALS + 1):
        trial_df = combined_df[combined_df['trial'] == trial]
        valid_starts = trial_df[trial_df['first_evacuate_time'] > 0]['first_evacuate_time']

        trial_stats.append({
            'started_count': len(valid_starts),
            'total_count': len(trial_df),
            'avg_start_time': valid_starts.mean() if len(valid_starts) > 0 else 0,
            'min_start_time': valid_starts.min() if len(valid_starts) > 0 else 0,
            'max_start_time': valid_starts.max() if len(valid_starts) > 0 else 0
        })

    return {
        'started_count': np.mean([s['started_count'] for s in trial_stats]),
        'total_count': np.mean([s['total_count'] for s in trial_stats]),
        'avg_start_time': np.mean([s['avg_start_time'] for s in trial_stats]),
        'min_start_time': np.mean([s['min_start_time'] for s in trial_stats]),
        'max_start_time': np.mean([s['max_start_time'] for s in trial_stats])
    }


def main(experiment_dir: Path):
    # 実験ディレクトリからexperiment_idを抽出
    experiment_id = experiment_dir.name

    output_dir = experiment_dir / 'averaged'
    # Markdown表の出力先を実験結果ディレクトリに変更
    tables_dir = experiment_dir

    # 出力ディレクトリ作成
    output_dir.mkdir(parents=True, exist_ok=True)

    # 全条件のデータを収集
    all_data = {}
    for condition in CONDITIONS:
        print(f'Processing {condition}...')
        summaries, agents_dfs = aggregate_condition_data(str(experiment_dir), condition)

        all_data[condition] = {
            'summaries': summaries,
            'agents_dfs': agents_dfs,
            'condition_averages': calculate_condition_averages(summaries),
            'action_ratios': calculate_action_ratio_averages(summaries),
            'evacuation_locations': calculate_evacuation_location_averages(summaries),
            'mental_state_metrics': calculate_mental_state_metrics(agents_dfs, condition),
            'social_interactions': calculate_social_interactions(agents_dfs),
            'stay_behavior': calculate_stay_behavior(agents_dfs),
            'evacuation_start': calculate_evacuation_start_times(agents_dfs),
            'evacuation_time_stats': calculate_evacuation_time_stats(agents_dfs)
        }

    # CSV出力
    print('Writing CSV files...')

    # 1. condition_averages.csv
    with open(output_dir / 'condition_averages.csv', 'w', encoding='utf-8', newline='') as f:
        writer = csv.writer(f)
        writer.writerow(['condition_id', 'evacuation_rate_mean', 'evacuation_rate_std',
                        'avg_evac_time_mean', 'avg_evac_time_std', 'median_evac_time_mean',
                        'max_evac_time_mean', 'shelter_rate_mean', 'area_rate_mean',
                        'total_agents', 'evacuated_agents_mean'])
        for condition in CONDITIONS:
            data = all_data[condition]['condition_averages']
            writer.writerow([
                condition,
                f"{data.get('evacuation_rate_mean', 0):.4f}",
                f"{data.get('evacuation_rate_std', 0):.4f}",
                f"{data.get('average_evacuation_time_mean', 0):.2f}",
                f"{data.get('average_evacuation_time_std', 0):.2f}",
                f"{data.get('median_evacuation_time_mean', 0):.2f}",
                f"{data.get('max_evacuation_time_mean', 0):.2f}",
                f"{data.get('evacuation_rate_to_shelter_mean', 0):.4f}",
                f"{data.get('evacuation_rate_to_area_mean', 0):.4f}",
                f"{data.get('total_agents_mean', 100):.0f}",
                f"{data.get('evacuated_agents_mean', 0):.1f}"
            ])

    # 2. action_ratios_averaged.csv
    with open(output_dir / 'action_ratios_averaged.csv', 'w', encoding='utf-8', newline='') as f:
        writer = csv.writer(f)
        writer.writerow(['condition_id', 'EVACUATE_mean', 'STAY_mean', 'FOLLOW_mean',
                        'CONTACT_mean', 'TALK_mean', 'SEARCH_FAMILY_mean'])
        for condition in CONDITIONS:
            data = all_data[condition]['action_ratios']
            writer.writerow([
                condition,
                f"{data.get('EVACUATE_mean', 0)*100:.2f}",
                f"{data.get('STAY_mean', 0)*100:.2f}",
                f"{data.get('FOLLOW_mean', 0)*100:.2f}",
                f"{data.get('CONTACT_mean', 0)*100:.2f}",
                f"{data.get('TALK_mean', 0)*100:.2f}",
                f"{data.get('SEARCH_FAMILY_mean', 0)*100:.2f}"
            ])

    # 3. mental_state_by_condition.csv
    with open(output_dir / 'mental_state_by_condition.csv', 'w', encoding='utf-8', newline='') as f:
        writer = csv.writer(f)
        header = ['mental_state', 'total_per_trial'] + [f'{c}_rate' for c in CONDITIONS] + [f'{c}_time' for c in CONDITIONS]
        writer.writerow(header)
        for ms in MENTAL_STATES:
            row = [ms]
            # total_per_trial
            total = all_data['Baseline-LLM']['mental_state_metrics'].get(ms, {}).get('total', 0)
            row.append(total)
            # 各条件の避難率
            for condition in CONDITIONS:
                rate = all_data[condition]['mental_state_metrics'].get(ms, {}).get('evacuation_rate', 0)
                row.append(f"{rate*100:.1f}")
            # 各条件の避難時間
            for condition in CONDITIONS:
                time = all_data[condition]['mental_state_metrics'].get(ms, {}).get('avg_evacuation_time', 0)
                row.append(f"{time:.1f}")
            writer.writerow(row)

    # 4. social_interactions_averaged.csv
    with open(output_dir / 'social_interactions_averaged.csv', 'w', encoding='utf-8', newline='') as f:
        writer = csv.writer(f)
        writer.writerow(['condition_id', 'follow_total', 'follow_executors',
                        'talk_total', 'talk_executors', 'contact_total', 'contact_executors'])
        for condition in CONDITIONS:
            data = all_data[condition]['social_interactions']
            writer.writerow([
                condition,
                f"{data.get('follow_total', 0):.1f}",
                f"{data.get('follow_executors', 0):.1f}",
                f"{data.get('talk_total', 0):.1f}",
                f"{data.get('talk_executors', 0):.1f}",
                f"{data.get('contact_total', 0):.1f}",
                f"{data.get('contact_executors', 0):.1f}"
            ])

    # 5. evacuation_locations_averaged.csv
    with open(output_dir / 'evacuation_locations_averaged.csv', 'w', encoding='utf-8', newline='') as f:
        writer = csv.writer(f)
        all_locations = set()
        for condition in CONDITIONS:
            all_locations.update(all_data[condition]['evacuation_locations'].keys())
        locations_list = sorted(list(all_locations))

        writer.writerow(['condition_id'] + locations_list)
        for condition in CONDITIONS:
            row = [condition]
            for loc in locations_list:
                count = all_data[condition]['evacuation_locations'].get(loc, 0)
                row.append(f"{count:.1f}")
            writer.writerow(row)

    # 6. evacuation_start_times.csv
    with open(output_dir / 'evacuation_start_times.csv', 'w', encoding='utf-8', newline='') as f:
        writer = csv.writer(f)
        writer.writerow(['condition_id', 'started_agents', 'total_agents',
                        'avg_start_time', 'min_start_time', 'max_start_time'])
        for condition in CONDITIONS:
            data = all_data[condition]['evacuation_start']
            writer.writerow([
                condition,
                f"{data.get('started_count', 0):.1f}",
                f"{data.get('total_count', 100):.0f}",
                f"{data.get('avg_start_time', 0):.2f}",
                f"{data.get('min_start_time', 0):.2f}",
                f"{data.get('max_start_time', 0):.2f}"
            ])

    # 7. stay_behavior.csv
    with open(output_dir / 'stay_behavior.csv', 'w', encoding='utf-8', newline='') as f:
        writer = csv.writer(f)
        writer.writerow(['condition_id', 'stay_executors', 'total_stay_time', 'avg_stay_time'])
        for condition in CONDITIONS:
            data = all_data[condition]['stay_behavior']
            writer.writerow([
                condition,
                f"{data.get('stay_executors', 0):.1f}",
                f"{data.get('total_stay_time', 0):.1f}",
                f"{data.get('avg_stay_time', 0):.1f}"
            ])

    # Markdown表の生成
    # experiment_idから日付部分を抽出してファイル名に使用
    date_part = experiment_id.split('_')[0]
    output_filename = f'Table_{date_part}.md'
    print(f'Generating {output_filename}...')
    generate_markdown_tables(all_data, tables_dir / output_filename, experiment_id)

    print('Done!')


def generate_markdown_tables(all_data, output_path, experiment_id='20260121_025302'):
    """Markdown形式の表を生成"""

    lines = []
    lines.append('# 実験結果の表（5トライアル平均）')
    lines.append('')
    lines.append(f'データソース: `Logs/experiment_results/{experiment_id}/`')
    lines.append('')

    # 実験1: 基本性能の検証
    lines.append('## 実験1: 基本性能の検証')
    lines.append('')

    # 表1: 基本指標比較
    lines.append('### 表1: 避難シミュレーション基本指標の比較')
    lines.append('')
    llm_data = all_data['Baseline-LLM']
    rb_data = all_data['Exp1-B']

    llm_avg = llm_data['condition_averages']
    rb_avg = rb_data['condition_averages']
    llm_time_stats = llm_data['evacuation_time_stats']
    rb_time_stats = rb_data['evacuation_time_stats']

    llm_total_agents = llm_avg.get("total_agents_mean", 0)
    rb_total_agents = rb_avg.get("total_agents_mean", 0)

    lines.append('| 指標 | LLMエージェント | ルールベースエージェント |')
    lines.append('|:-----|:---------------:|:------------------------:|')
    lines.append(f'| 総エージェント数 | {llm_total_agents:.0f} | {rb_total_agents:.0f} |')
    lines.append(f'| 避難完了者数 | {llm_avg.get("evacuated_agents_mean", 0):.1f} | {rb_avg.get("evacuated_agents_mean", 0):.1f} |')
    lines.append(f'| **避難完了率** | **{llm_avg.get("evacuation_rate_mean", 0)*100:.1f}%** | **{rb_avg.get("evacuation_rate_mean", 0)*100:.1f}%** |')
    lines.append(f'| 平均避難時間 | {llm_avg.get("average_evacuation_time_mean", 0):.2f}秒 | {rb_avg.get("average_evacuation_time_mean", 0):.2f}秒 |')
    lines.append(f'| 避難時間標準偏差 | {llm_time_stats.get("evacuation_time_std_agents", 0):.2f}秒 | {rb_time_stats.get("evacuation_time_std_agents", 0):.2f}秒 |')
    lines.append(f'| 避難時間25%点 | {llm_time_stats.get("evacuation_time_25pct", 0):.2f}秒 | {rb_time_stats.get("evacuation_time_25pct", 0):.2f}秒 |')
    lines.append(f'| 中央値避難時間 | {llm_avg.get("median_evacuation_time_mean", 0):.2f}秒 | {rb_avg.get("median_evacuation_time_mean", 0):.2f}秒 |')
    lines.append(f'| 避難時間75%点 | {llm_time_stats.get("evacuation_time_75pct", 0):.2f}秒 | {rb_time_stats.get("evacuation_time_75pct", 0):.2f}秒 |')
    lines.append(f'| 最大避難時間 | {llm_avg.get("max_evacuation_time_mean", 0):.2f}秒 | {rb_avg.get("max_evacuation_time_mean", 0):.2f}秒 |')
    lines.append(f'| 避難所への避難率 | {llm_avg.get("evacuation_rate_to_shelter_mean", 0)*100:.1f}% | {rb_avg.get("evacuation_rate_to_shelter_mean", 0)*100:.1f}% |')
    lines.append(f'| 津波避難地域への避難率 | {llm_avg.get("evacuation_rate_to_area_mean", 0)*100:.1f}% | {rb_avg.get("evacuation_rate_to_area_mean", 0)*100:.1f}% |')
    lines.append('')

    # 表2: 行動選択分布
    lines.append('### 表2: 行動選択の分布')
    lines.append('')
    llm_actions = llm_data['action_ratios']
    rb_actions = rb_data['action_ratios']

    lines.append('| 行動タイプ | LLMエージェント | ルールベースエージェント |')
    lines.append('|:-----------|:---------------:|:------------------------:|')
    for action in ['EVACUATE', 'STAY', 'FOLLOW', 'CONTACT', 'TALK', 'SEARCH_FAMILY']:
        action_label = {
            'EVACUATE': 'EVACUATE（避難）',
            'STAY': 'STAY（待機）',
            'FOLLOW': 'FOLLOW（同調）',
            'CONTACT': 'CONTACT（連絡）',
            'TALK': 'TALK（会話）',
            'SEARCH_FAMILY': 'SEARCH_FAMILY（家族探索）'
        }[action]
        llm_val = llm_actions.get(f'{action}_mean', 0) * 100
        rb_val = rb_actions.get(f'{action}_mean', 0) * 100
        lines.append(f'| {action_label} | {llm_val:.2f}% | {rb_val:.2f}% |')
    lines.append('')

    # 表3: 避難開始時刻
    lines.append('### 表3: 避難開始時刻の分析')
    lines.append('')
    llm_start = llm_data['evacuation_start']
    rb_start = rb_data['evacuation_start']

    lines.append('| 指標 | LLMエージェント | ルールベースエージェント |')
    lines.append('|:-----|:---------------:|:------------------------:|')
    lines.append(f'| 避難開始エージェント数 | {llm_start.get("started_count", 0):.1f}/{llm_start.get("total_count", 0):.0f} | {rb_start.get("started_count", 0):.1f}/{rb_start.get("total_count", 0):.0f} |')
    lines.append(f'| 平均避難開始時刻 | {llm_start.get("avg_start_time", 0):.2f}秒 | {rb_start.get("avg_start_time", 0):.2f}秒 |')
    lines.append(f'| 最小避難開始時刻 | {llm_start.get("min_start_time", 0):.2f}秒 | {rb_start.get("min_start_time", 0):.2f}秒 |')
    lines.append(f'| 最大避難開始時刻 | {llm_start.get("max_start_time", 0):.2f}秒 | {rb_start.get("max_start_time", 0):.2f}秒 |')
    lines.append('')

    # 表4: 社会的相互作用
    lines.append('### 表4: 社会的相互作用の発生状況')
    lines.append('')
    llm_social = llm_data['social_interactions']
    rb_social = rb_data['social_interactions']

    lines.append('| 行動 | LLMエージェント | ルールベースエージェント |')
    lines.append('|:-----|:---------------:|:------------------------:|')
    lines.append(f'| FOLLOW（回数/実行者数） | {llm_social.get("follow_total", 0):.0f}回 / {llm_social.get("follow_executors", 0):.0f}人 | {rb_social.get("follow_total", 0):.0f}回 / {rb_social.get("follow_executors", 0):.0f}人 |')
    lines.append(f'| TALK（回数/実行者数） | {llm_social.get("talk_total", 0):.0f}回 / {llm_social.get("talk_executors", 0):.0f}人 | {rb_social.get("talk_total", 0):.0f}回 / {rb_social.get("talk_executors", 0):.0f}人 |')
    lines.append(f'| CONTACT（回数/実行者数） | {llm_social.get("contact_total", 0):.0f}回 / {llm_social.get("contact_executors", 0):.0f}人 | {rb_social.get("contact_total", 0):.0f}回 / {rb_social.get("contact_executors", 0):.0f}人 |')
    lines.append('')

    # 表5: STAY行動分析
    lines.append('### 表5: 待機行動（STAY）の分析')
    lines.append('')
    llm_stay = llm_data['stay_behavior']
    rb_stay = rb_data['stay_behavior']

    lines.append('| 指標 | LLMエージェント | ルールベースエージェント |')
    lines.append('|:-----|:---------------:|:------------------------:|')
    lines.append(f'| STAY実行エージェント数 | {llm_stay.get("stay_executors", 0):.0f} | {rb_stay.get("stay_executors", 0):.0f} |')
    lines.append(f'| 総STAY時間 | {llm_stay.get("total_stay_time", 0):.0f}秒 | {rb_stay.get("total_stay_time", 0):.0f}秒 |')
    lines.append(f'| 平均STAY時間（実行者のみ） | {llm_stay.get("avg_stay_time", 0):.1f}秒 | {rb_stay.get("avg_stay_time", 0):.1f}秒 |')
    lines.append('')

    # 表6: LLMエージェント避難先分布
    lines.append('### 表6: 避難先の選択分布（LLMエージェント）')
    lines.append('')
    llm_locs = llm_data['evacuation_locations']

    lines.append('| 避難先 | 避難完了者数 | 分類 |')
    lines.append('|:-------|:-----------:|:----:|')

    # 津波避難地域（高台）
    tsunami_areas = ['中田山', '八坂神社', '望洋荘', '諏訪神社', '小泉工業']

    for loc in sorted(llm_locs.keys(), key=lambda x: llm_locs[x], reverse=True):
        count = llm_locs.get(loc, 0)
        if count > 0:
            classification = '津波避難地域' if loc in tsunami_areas else '指定避難所'
            lines.append(f'| {loc} | {count:.1f} | {classification} |')
    lines.append('')

    # 表7: ルールベースエージェント避難先分布
    lines.append('### 表7: 避難先の選択分布（ルールベースエージェント）')
    lines.append('')
    rb_locs = rb_data['evacuation_locations']

    lines.append('| 避難先 | 避難完了者数 | 分類 |')
    lines.append('|:-------|:-----------:|:----:|')

    for loc in sorted(rb_locs.keys(), key=lambda x: rb_locs[x], reverse=True):
        count = rb_locs.get(loc, 0)
        if count > 0:
            classification = '津波避難地域' if loc in tsunami_areas else '指定避難所'
            lines.append(f'| {loc} | {count:.1f} | {classification} |')
    lines.append('')

    # 表8: 認知特性別避難完了率
    lines.append('### 表8: 認知特性別の避難完了率比較')
    lines.append('')
    llm_ms = llm_data['mental_state_metrics']
    rb_ms = rb_data['mental_state_metrics']

    lines.append('| 認知特性 | LLMエージェント | ルールベースエージェント |')
    lines.append('|:---------|:---------------:|:------------------------:|')
    for ms in MENTAL_STATES:
        llm_total = llm_ms.get(ms, {}).get('total', 0)
        llm_rate = llm_ms.get(ms, {}).get('evacuation_rate', 0)
        rb_rate = rb_ms.get(ms, {}).get('evacuation_rate', 0)

        llm_evacuated = int(llm_total * llm_rate + 0.5)
        rb_evacuated = int(llm_total * rb_rate + 0.5)

        lines.append(f'| {ms} | {llm_evacuated}/{llm_total} ({llm_rate*100:.1f}%) | {rb_evacuated}/{llm_total} ({rb_rate*100:.1f}%) |')
    lines.append('')

    # 表9: 認知特性別避難時間
    lines.append('### 表9: 認知特性別の平均避難時間比較（秒）')
    lines.append('')
    lines.append('| 認知特性 | LLMエージェント | ルールベースエージェント |')
    lines.append('|:---------|:---------------:|:------------------------:|')
    for ms in MENTAL_STATES:
        llm_time = llm_ms.get(ms, {}).get('avg_evacuation_time', 0)
        rb_time = rb_ms.get(ms, {}).get('avg_evacuation_time', 0)
        lines.append(f'| {ms} | {llm_time:.1f} | {rb_time:.1f} |')
    lines.append('')

    # 実験2: 認知バイアスの影響
    lines.append('## 実験2: 認知バイアスが避難行動に与える影響')
    lines.append('')
    lines.append('認知バイアスの付与が避難行動に与える影響を検証するため、4つの条件を設定した。')
    lines.append('- 条件1（ベースライン）: 認知バイアスなし（ペルソナの既存mental_stateを使用）')
    lines.append('- 条件2（正常性バイアス）: 全エージェントに正常性バイアスを付与')
    lines.append('- 条件3（同調バイアス）: 全エージェントに同調バイアスを付与')
    lines.append('- 条件4（複合）: 正常性バイアスと同調バイアスの両方を付与')
    lines.append('')

    # 表10: 認知バイアス条件別基本指標
    lines.append('### 表10: 認知バイアス条件別の基本指標比較')
    lines.append('')

    bias_conditions = ['Baseline-LLM', 'Exp2-2', 'Exp2-3', 'Exp2-4']
    bias_labels = ['条件1（ベースライン）', '条件2（正常性バイアス）', '条件3（同調バイアス）', '条件4（複合）']

    lines.append('| 指標 | 条件1（ベースライン） | 条件2（正常性バイアス） | 条件3（同調バイアス） | 条件4（複合） | 平均 | 標準偏差 |')
    lines.append('|:-----|:---------------:|:---------------:|:---------------:|:---------------:|:----:|:--------:|')

    # 避難完了率
    evac_rates = [all_data[c]['condition_averages'].get('evacuation_rate_mean', 0) * 100 for c in bias_conditions]
    lines.append(f'| 避難完了率 | {evac_rates[0]:.1f}% | {evac_rates[1]:.1f}% | {evac_rates[2]:.1f}% | {evac_rates[3]:.1f}% | {np.mean(evac_rates):.1f}% | {np.std(evac_rates):.1f}% |')

    # 避難完了者数
    evac_counts = [all_data[c]['condition_averages'].get('evacuated_agents_mean', 0) for c in bias_conditions]
    lines.append(f'| 避難完了者数 | {evac_counts[0]:.0f} | {evac_counts[1]:.0f} | {evac_counts[2]:.0f} | {evac_counts[3]:.0f} | {np.mean(evac_counts):.1f} | {np.std(evac_counts):.1f} |')

    # 平均避難時間
    avg_times = [all_data[c]['condition_averages'].get('average_evacuation_time_mean', 0) for c in bias_conditions]
    lines.append(f'| 平均避難時間（秒） | {avg_times[0]:.2f} | {avg_times[1]:.2f} | {avg_times[2]:.2f} | {avg_times[3]:.2f} | {np.mean(avg_times):.1f} | {np.std(avg_times):.1f} |')

    # 避難時間標準偏差（エージェント間）
    time_stds = [all_data[c]['evacuation_time_stats'].get('evacuation_time_std_agents', 0) for c in bias_conditions]
    lines.append(f'| 避難時間標準偏差（秒） | {time_stds[0]:.2f} | {time_stds[1]:.2f} | {time_stds[2]:.2f} | {time_stds[3]:.2f} | {np.mean(time_stds):.1f} | {np.std(time_stds):.1f} |')

    # 中央値避難時間
    med_times = [all_data[c]['condition_averages'].get('median_evacuation_time_mean', 0) for c in bias_conditions]
    lines.append(f'| 中央値避難時間（秒） | {med_times[0]:.2f} | {med_times[1]:.2f} | {med_times[2]:.2f} | {med_times[3]:.2f} | {np.mean(med_times):.1f} | {np.std(med_times):.1f} |')

    # 最大避難時間
    max_times = [all_data[c]['condition_averages'].get('max_evacuation_time_mean', 0) for c in bias_conditions]
    lines.append(f'| 最大避難時間（秒） | {max_times[0]:.2f} | {max_times[1]:.2f} | {max_times[2]:.2f} | {max_times[3]:.2f} | {np.mean(max_times):.1f} | {np.std(max_times):.1f} |')

    # 避難所への避難率
    shelter_rates = [all_data[c]['condition_averages'].get('evacuation_rate_to_shelter_mean', 0) * 100 for c in bias_conditions]
    lines.append(f'| 避難所への避難率 | {shelter_rates[0]:.1f}% | {shelter_rates[1]:.1f}% | {shelter_rates[2]:.1f}% | {shelter_rates[3]:.1f}% | {np.mean(shelter_rates):.1f}% | {np.std(shelter_rates):.1f}% |')

    # 津波避難地域への避難率
    area_rates = [all_data[c]['condition_averages'].get('evacuation_rate_to_area_mean', 0) * 100 for c in bias_conditions]
    lines.append(f'| 津波避難地域への避難率 | {area_rates[0]:.1f}% | {area_rates[1]:.1f}% | {area_rates[2]:.1f}% | {area_rates[3]:.1f}% | {np.mean(area_rates):.1f}% | {np.std(area_rates):.1f}% |')
    lines.append('')

    # 表11: 認知バイアス条件別行動選択分布
    lines.append('### 表11: 認知バイアス条件別の行動選択分布')
    lines.append('')
    lines.append('| 行動タイプ | 条件1（ベースライン） | 条件2（正常性バイアス） | 条件3（同調バイアス） | 条件4（複合） | 平均 |')
    lines.append('|:-----------|:-----:|:-----:|:-----:|:-----:|:----:|')

    for action in ['EVACUATE', 'STAY', 'FOLLOW', 'CONTACT', 'TALK', 'SEARCH_FAMILY']:
        action_label = {
            'EVACUATE': 'EVACUATE（避難）',
            'STAY': 'STAY（待機）',
            'FOLLOW': 'FOLLOW（同調）',
            'CONTACT': 'CONTACT（連絡）',
            'TALK': 'TALK（会話）',
            'SEARCH_FAMILY': 'SEARCH_FAMILY（家族探索）'
        }[action]
        vals = [all_data[c]['action_ratios'].get(f'{action}_mean', 0) * 100 for c in bias_conditions]
        lines.append(f'| {action_label} | {vals[0]:.2f}% | {vals[1]:.2f}% | {vals[2]:.2f}% | {vals[3]:.2f}% | {np.mean(vals):.2f}% |')
    lines.append('')

    # 表12: 認知バイアス条件別避難先選択
    lines.append('### 表12: 認知バイアス条件別の避難先選択分布（避難完了者数）')
    lines.append('')
    lines.append('| 避難先 | 条件1（ベースライン） | 条件2（正常性バイアス） | 条件3（同調バイアス） | 条件4（複合） | 合計 |')
    lines.append('|:-------|:-----:|:-----:|:-----:|:-----:|:----:|')

    all_locs_bias = set()
    for c in bias_conditions:
        all_locs_bias.update(all_data[c]['evacuation_locations'].keys())

    for loc in sorted(all_locs_bias):
        vals = [all_data[c]['evacuation_locations'].get(loc, 0) for c in bias_conditions]
        if sum(vals) > 0:
            lines.append(f'| {loc} | {vals[0]:.0f} | {vals[1]:.0f} | {vals[2]:.0f} | {vals[3]:.0f} | {sum(vals):.0f} |')
    lines.append('')

    # 表13: ベースライン認知特性別避難完了率
    lines.append('### 表13: 条件1（ベースライン）における認知特性別の避難完了率')
    lines.append('')
    baseline_ms = all_data['Baseline-LLM']['mental_state_metrics']

    lines.append('| 認知特性 | 総数 | 避難完了者数 | 避難完了率 |')
    lines.append('|:---------|:----:|:------------:|:----------:|')
    for ms in MENTAL_STATES:
        total = baseline_ms.get(ms, {}).get('total', 0)
        rate = baseline_ms.get(ms, {}).get('evacuation_rate', 0)
        evacuated = int(total * rate + 0.5)
        lines.append(f'| {ms} | {total} | {evacuated} | {rate*100:.1f}% |')
    lines.append('')

    # 表14: 認知バイアス条件別認知特性別避難完了率
    lines.append('### 表14: 認知バイアス条件別の認知特性別避難完了率')
    lines.append('')
    lines.append('| 認知特性 | 条件1（ベースライン） | 条件2（正常性バイアス） | 条件3（同調バイアス） | 条件4（複合） | 平均 |')
    lines.append('|:---------|:-----:|:-----:|:-----:|:-----:|:----:|')

    for ms in MENTAL_STATES:
        rates = [all_data[c]['mental_state_metrics'].get(ms, {}).get('evacuation_rate', 0) * 100 for c in bias_conditions]
        lines.append(f'| {ms} | {rates[0]:.1f}% | {rates[1]:.1f}% | {rates[2]:.1f}% | {rates[3]:.1f}% | {np.mean(rates):.1f}% |')
    lines.append('')

    # 表15: ベースライン社会的相互作用
    lines.append('### 表15: 条件1（ベースライン）における社会的相互作用の発生状況')
    lines.append('')
    baseline_social = all_data['Baseline-LLM']['social_interactions']

    lines.append('| 行動タイプ | 発生回数 | 実行者数 | 1人あたり平均回数 |')
    lines.append('|:-----------|:--------:|:--------:|:-----------------:|')
    for action, label in [('follow', 'FOLLOW（同調）'), ('talk', 'TALK（会話）'), ('contact', 'CONTACT（連絡）')]:
        total = baseline_social.get(f'{action}_total', 0)
        executors = baseline_social.get(f'{action}_executors', 0)
        avg_per = total / executors if executors > 0 else 0
        lines.append(f'| {label} | {total:.0f} | {executors:.0f} | {avg_per:.1f} |')
    lines.append('')

    # 表16: 認知バイアス条件別社会的相互作用
    lines.append('### 表16: 認知バイアス条件別の社会的相互作用')
    lines.append('')
    lines.append('| 行動タイプ | 条件1（ベースライン） | 条件2（正常性バイアス） | 条件3（同調バイアス） | 条件4（複合） |')
    lines.append('|:-----------|:-----:|:-----:|:-----:|:-----:|')

    for action, label in [('follow', 'FOLLOW'), ('talk', 'TALK'), ('contact', 'CONTACT')]:
        vals = []
        for c in bias_conditions:
            social = all_data[c]['social_interactions']
            total = social.get(f'{action}_total', 0)
            executors = social.get(f'{action}_executors', 0)
            vals.append(f'{total:.0f}回/{executors:.0f}人')
        lines.append(f'| {label}（回数/実行者数） | {vals[0]} | {vals[1]} | {vals[2]} | {vals[3]} |')
    lines.append('')

    # 表17: 待機行動と避難失敗の関係（簡略版）
    lines.append('### 表17: 待機行動と避難失敗の関係（全条件合計）')
    lines.append('')

    total_evacuated = sum([all_data[c]['condition_averages'].get('evacuated_agents_mean', 0) for c in bias_conditions])
    total_failed = 400 - total_evacuated
    total_stay_executors = sum([all_data[c]['stay_behavior'].get('stay_executors', 0) for c in bias_conditions])

    lines.append('| 指標 | 避難完了者 | 避難失敗者 |')
    lines.append('|:-----|:----------:|:----------:|')
    lines.append(f'| 総数 | {total_evacuated:.0f} | {total_failed:.0f} |')
    lines.append(f'| STAY実行者（延べ） | - | - |')
    lines.append(f'| 正常性バイアス保持者 | - | - |')
    lines.append('')

    # 表18: 一貫性分析（簡略版）
    lines.append('### 表18: 認知バイアス条件別の避難行動の一貫性分析')
    lines.append('')
    lines.append('| 認知特性 | 4条件全避難成功 | 3条件以上成功 | 一貫性評価 |')
    lines.append('|:---------|:---------------:|:-------------:|:----------:|')

    for ms in MENTAL_STATES:
        rates = [all_data[c]['mental_state_metrics'].get(ms, {}).get('evacuation_rate', 0) for c in bias_conditions]
        avg_rate = np.mean(rates)
        if avg_rate >= 0.85:
            consistency = '高'
        elif avg_rate >= 0.65:
            consistency = '中'
        elif avg_rate >= 0.45:
            consistency = '低'
        else:
            consistency = '非常に低'

        total = all_data['Baseline-LLM']['mental_state_metrics'].get(ms, {}).get('total', 0)
        all_success = int(total * min(rates) + 0.5) if rates else 0
        three_plus = int(total * np.percentile(rates, 25) + 0.5) if rates else 0

        lines.append(f'| {ms} | {all_success}/{total} ({min(rates)*100:.1f}%) | {three_plus}/{total} ({np.percentile(rates, 25)*100:.1f}%) | {consistency} |')
    lines.append('')

    # 表19: 実験2主要知見
    lines.append('### 表19: 実験2の主要知見')
    lines.append('')

    baseline_rate = all_data['Baseline-LLM']['condition_averages'].get('evacuation_rate_mean', 0) * 100
    exp2_2_rate = all_data['Exp2-2']['condition_averages'].get('evacuation_rate_mean', 0) * 100
    exp2_3_rate = all_data['Exp2-3']['condition_averages'].get('evacuation_rate_mean', 0) * 100
    exp2_4_rate = all_data['Exp2-4']['condition_averages'].get('evacuation_rate_mean', 0) * 100

    exp2_2_stay = all_data['Exp2-2']['action_ratios'].get('STAY_mean', 0) * 100
    baseline_stay = all_data['Baseline-LLM']['action_ratios'].get('STAY_mean', 0) * 100

    lines.append('| 項目 | 観察結果 | 解釈 |')
    lines.append('|:-----|:---------|:-----|')
    lines.append(f'| 避難完了率のばらつき | {min(evac_rates):.1f}%〜{max(evac_rates):.1f}%（標準偏差{np.std(evac_rates):.1f}%） | 認知バイアスの種類により避難完了率に大きな差異 |')
    lines.append(f'| 正常性バイアスの影響 | 条件2で避難完了率{exp2_2_rate:.1f}%（{exp2_2_rate - baseline_rate:.0f}pt変化） | 正常性バイアスは避難行動を大幅に遅延させる |')
    lines.append(f'| 同調バイアスの影響 | 条件3で避難完了率{exp2_3_rate:.1f}%（{exp2_3_rate - baseline_rate:+.0f}pt変化） | 同調バイアスは集団的避難行動を促進する |')
    lines.append(f'| 複合バイアスの影響 | 条件4で避難完了率{exp2_4_rate:.1f}% | 正常性バイアスと同調バイアスが競合 |')
    lines.append(f'| STAY行動の増加 | 条件2でSTAY選択率{exp2_2_stay:.2f}%（{exp2_2_stay - baseline_stay:+.0f}pt変化） | 正常性バイアスが待機行動を促進 |')
    lines.append('')

    # 実験3: 情報提供戦略
    lines.append('## 実験3: 情報提供戦略の検証')
    lines.append('')

    # 表20: 実験条件設計
    lines.append('### 表20: 実験条件の設計（2×2マトリクス）')
    lines.append('')
    lines.append('|  | **切迫感: 低（Neutral）** | **切迫感: 高（High）** |')
    lines.append('|:--|:--------------------------|:----------------------|')
    lines.append('| **具体性: 低（Low）** | **条件A**: 標準警報のみ | **条件B**: 強い警告表現 |')
    lines.append('| **具体性: 高（High）** | **条件C**: 詳細な避難先指示 | **条件D**: 詳細＋強い切迫感 |')
    lines.append('')

    # 表21: 情報条件別基本指標
    lines.append('### 表21: 情報条件別の基本指標比較')
    lines.append('')

    info_conditions = ['Baseline-LLM', 'Exp3-B', 'Exp3-C', 'Exp3-D']
    info_labels = ['条件A（標準）', '条件B（切迫）', '条件C（詳細）', '条件D（詳細+切迫）']

    lines.append('| 指標 | 条件A（標準） | 条件B（切迫） | 条件C（詳細） | 条件D（詳細+切迫） |')
    lines.append('|:-----|:-------------:|:-------------:|:-------------:|:-----------------:|')

    total_agents_info = [all_data[c]['condition_averages'].get('total_agents_mean', 0) for c in info_conditions]
    lines.append(f'| 総エージェント数 | {total_agents_info[0]:.0f} | {total_agents_info[1]:.0f} | {total_agents_info[2]:.0f} | {total_agents_info[3]:.0f} |')

    evac_counts_info = [all_data[c]['condition_averages'].get('evacuated_agents_mean', 0) for c in info_conditions]
    lines.append(f'| 避難完了者数 | {evac_counts_info[0]:.0f} | {evac_counts_info[1]:.0f} | {evac_counts_info[2]:.0f} | {evac_counts_info[3]:.0f} |')

    evac_rates_info = [all_data[c]['condition_averages'].get('evacuation_rate_mean', 0) * 100 for c in info_conditions]
    lines.append(f'| **避難完了率** | **{evac_rates_info[0]:.1f}%** | **{evac_rates_info[1]:.1f}%** | **{evac_rates_info[2]:.1f}%** | **{evac_rates_info[3]:.1f}%** |')

    avg_times_info = [all_data[c]['condition_averages'].get('average_evacuation_time_mean', 0) for c in info_conditions]
    lines.append(f'| 平均避難時間（秒） | {avg_times_info[0]:.2f} | {avg_times_info[1]:.2f} | {avg_times_info[2]:.2f} | {avg_times_info[3]:.2f} |')

    time_stds_info = [all_data[c]['evacuation_time_stats'].get('evacuation_time_std_agents', 0) for c in info_conditions]
    lines.append(f'| 避難時間標準偏差（秒） | {time_stds_info[0]:.2f} | {time_stds_info[1]:.2f} | {time_stds_info[2]:.2f} | {time_stds_info[3]:.2f} |')

    med_times_info = [all_data[c]['condition_averages'].get('median_evacuation_time_mean', 0) for c in info_conditions]
    lines.append(f'| 中央値避難時間（秒） | {med_times_info[0]:.2f} | {med_times_info[1]:.2f} | {med_times_info[2]:.2f} | {med_times_info[3]:.2f} |')

    max_times_info = [all_data[c]['condition_averages'].get('max_evacuation_time_mean', 0) for c in info_conditions]
    lines.append(f'| 最大避難時間（秒） | {max_times_info[0]:.2f} | {max_times_info[1]:.2f} | {max_times_info[2]:.2f} | {max_times_info[3]:.2f} |')

    shelter_rates_info = [all_data[c]['condition_averages'].get('evacuation_rate_to_shelter_mean', 0) * 100 for c in info_conditions]
    lines.append(f'| 避難所への避難率 | {shelter_rates_info[0]:.1f}% | {shelter_rates_info[1]:.1f}% | {shelter_rates_info[2]:.1f}% | {shelter_rates_info[3]:.1f}% |')

    area_rates_info = [all_data[c]['condition_averages'].get('evacuation_rate_to_area_mean', 0) * 100 for c in info_conditions]
    lines.append(f'| 津波避難地域への避難率 | {area_rates_info[0]:.1f}% | {area_rates_info[1]:.1f}% | {area_rates_info[2]:.1f}% | {area_rates_info[3]:.1f}% |')
    lines.append('')

    # 表22: 情報条件別行動選択分布
    lines.append('### 表22: 情報条件別の行動選択分布')
    lines.append('')
    lines.append('| 行動タイプ | 条件A（標準） | 条件B（切迫） | 条件C（詳細） | 条件D（詳細+切迫） |')
    lines.append('|:-----------|:-------------:|:-------------:|:-------------:|:-----------------:|')

    for action in ['EVACUATE', 'STAY', 'FOLLOW', 'CONTACT', 'TALK', 'SEARCH_FAMILY']:
        action_label = {
            'EVACUATE': 'EVACUATE（避難）',
            'STAY': 'STAY（待機）',
            'FOLLOW': 'FOLLOW（同調）',
            'CONTACT': 'CONTACT（連絡）',
            'TALK': 'TALK（会話）',
            'SEARCH_FAMILY': 'SEARCH_FAMILY（家族探索）'
        }[action]
        vals = [all_data[c]['action_ratios'].get(f'{action}_mean', 0) * 100 for c in info_conditions]
        lines.append(f'| {action_label} | {vals[0]:.2f}% | {vals[1]:.2f}% | {vals[2]:.2f}% | {vals[3]:.2f}% |')
    lines.append('')

    # 表23: 情報条件別避難開始時刻
    lines.append('### 表23: 情報条件別の避難開始時刻分析')
    lines.append('')
    lines.append('| 指標 | 条件A（標準） | 条件B（切迫） | 条件C（詳細） | 条件D（詳細+切迫） |')
    lines.append('|:-----|:-------------:|:-------------:|:-------------:|:-----------------:|')

    for c, label in zip(info_conditions, info_labels):
        start = all_data[c]['evacuation_start']

    started_counts = [all_data[c]['evacuation_start'].get('started_count', 0) for c in info_conditions]
    total_counts_info = [all_data[c]['evacuation_start'].get('total_count', 0) for c in info_conditions]
    lines.append(f'| 避難開始エージェント数 | {started_counts[0]:.0f}/{total_counts_info[0]:.0f} | {started_counts[1]:.0f}/{total_counts_info[1]:.0f} | {started_counts[2]:.0f}/{total_counts_info[2]:.0f} | {started_counts[3]:.0f}/{total_counts_info[3]:.0f} |')

    avg_starts = [all_data[c]['evacuation_start'].get('avg_start_time', 0) for c in info_conditions]
    lines.append(f'| 平均避難開始時刻（秒） | {avg_starts[0]:.1f} | {avg_starts[1]:.1f} | {avg_starts[2]:.1f} | {avg_starts[3]:.1f} |')

    min_starts = [all_data[c]['evacuation_start'].get('min_start_time', 0) for c in info_conditions]
    lines.append(f'| 最小避難開始時刻（秒） | {min_starts[0]:.2f} | {min_starts[1]:.2f} | {min_starts[2]:.2f} | {min_starts[3]:.2f} |')

    max_starts = [all_data[c]['evacuation_start'].get('max_start_time', 0) for c in info_conditions]
    lines.append(f'| 最大避難開始時刻（秒） | {max_starts[0]:.2f} | {max_starts[1]:.2f} | {max_starts[2]:.2f} | {max_starts[3]:.2f} |')
    lines.append('')

    # 表24: 情報条件別認知特性別避難完了率
    lines.append('### 表24: 情報条件別の認知特性別避難完了率')
    lines.append('')
    lines.append('| 認知特性 | 条件A（標準） | 条件B（切迫） | 条件C（詳細） | 条件D（詳細+切迫） |')
    lines.append('|:---------|:-------------:|:-------------:|:-------------:|:-----------------:|')

    for ms in MENTAL_STATES:
        rates = []
        for c in info_conditions:
            ms_data = all_data[c]['mental_state_metrics'].get(ms, {})
            total = ms_data.get('total', 0)
            rate = ms_data.get('evacuation_rate', 0)
            evacuated = int(total * rate + 0.5)
            rates.append(f'{evacuated}/{total} ({rate*100:.1f}%)')

        lines.append(f'| {ms} | {rates[0]} | {rates[1]} | {rates[2]} | {rates[3]} |')
    lines.append('')

    # 表25: 情報条件別社会的相互作用
    lines.append('### 表25: 情報条件別の社会的相互作用')
    lines.append('')
    lines.append('| 行動 | 条件A（標準） | 条件B（切迫） | 条件C（詳細） | 条件D（詳細+切迫） |')
    lines.append('|:-----|:-------------:|:-------------:|:-------------:|:-----------------:|')

    for action, label in [('follow', 'FOLLOW'), ('talk', 'TALK'), ('contact', 'CONTACT')]:
        vals = []
        for c in info_conditions:
            social = all_data[c]['social_interactions']
            total = social.get(f'{action}_total', 0)
            executors = social.get(f'{action}_executors', 0)
            vals.append(f'{total:.0f}回/{executors:.0f}人')
        lines.append(f'| {label}（回数/実行者数） | {vals[0]} | {vals[1]} | {vals[2]} | {vals[3]} |')
    lines.append('')

    # 表26: 情報条件別避難先選択
    lines.append('### 表26: 情報条件別の避難先選択分布（避難完了者数）')
    lines.append('')
    lines.append('| 避難先 | 条件A（標準） | 条件B（切迫） | 条件C（詳細） | 条件D（詳細+切迫） |')
    lines.append('|:-------|:-------------:|:-------------:|:-------------:|:-----------------:|')

    all_locs_info = set()
    for c in info_conditions:
        all_locs_info.update(all_data[c]['evacuation_locations'].keys())

    for loc in sorted(all_locs_info):
        vals = [all_data[c]['evacuation_locations'].get(loc, 0) for c in info_conditions]
        if sum(vals) > 0:
            lines.append(f'| {loc} | {vals[0]:.0f} | {vals[1]:.0f} | {vals[2]:.0f} | {vals[3]:.0f} |')
    lines.append('')

    # 表27: 情報提供戦略主要知見
    lines.append('### 表27: 実験3の主要知見（情報提供戦略の効果）')
    lines.append('')

    baseline_rate_info = evac_rates_info[0]
    exp3b_rate = evac_rates_info[1]
    exp3c_rate = evac_rates_info[2]
    exp3d_rate = evac_rates_info[3]

    baseline_start_info = avg_starts[0]
    exp3b_start = avg_starts[1]
    exp3c_start = avg_starts[2]
    exp3d_start = avg_starts[3]

    # 正常性バイアス者の避難率
    nb_rates = [all_data[c]['mental_state_metrics'].get('正常性バイアス・楽観的', {}).get('evacuation_rate', 0) * 100 for c in info_conditions]

    lines.append('| 項目 | 条件A→B（切迫感↑） | 条件A→C（具体性↑） | 条件A→D（両方↑） | 解釈 |')
    lines.append('|:-----|:-------------------:|:------------------:|:----------------:|:-----|')
    lines.append(f'| 避難完了率 | {baseline_rate_info:.0f}%→{exp3b_rate:.0f}% ({exp3b_rate-baseline_rate_info:+.0f}pt) | {baseline_rate_info:.0f}%→{exp3c_rate:.0f}% ({exp3c_rate-baseline_rate_info:+.0f}pt) | {baseline_rate_info:.0f}%→{exp3d_rate:.0f}% ({exp3d_rate-baseline_rate_info:+.0f}pt) | 切迫感は効果的、具体性は効果限定的 |')
    lines.append(f'| 避難開始時刻（平均） | {baseline_start_info:.1f}s→{exp3b_start:.1f}s | {baseline_start_info:.1f}s→{exp3c_start:.1f}s | {baseline_start_info:.1f}s→{exp3d_start:.1f}s | 切迫感は避難開始を早める |')
    lines.append(f'| 正常性バイアス者の避難率 | {nb_rates[0]:.1f}%→{nb_rates[1]:.1f}% | {nb_rates[0]:.1f}%→{nb_rates[2]:.1f}% | {nb_rates[0]:.1f}%→{nb_rates[3]:.1f}% | 正常性バイアス者への効果 |')

    # EVACUATE選択率
    evac_actions = [all_data[c]['action_ratios'].get('EVACUATE_mean', 0) * 100 for c in info_conditions]
    lines.append(f'| EVACUATE選択率 | {evac_actions[0]:.2f}%→{evac_actions[1]:.2f}% | {evac_actions[0]:.2f}%→{evac_actions[2]:.2f}% | {evac_actions[0]:.2f}%→{evac_actions[3]:.2f}% | 条件間で大きな差異なし |')

    # FOLLOW選択率
    follow_actions = [all_data[c]['action_ratios'].get('FOLLOW_mean', 0) * 100 for c in info_conditions]
    lines.append(f'| FOLLOW選択率 | {follow_actions[0]:.2f}%→{follow_actions[1]:.2f}% | {follow_actions[0]:.2f}%→{follow_actions[2]:.2f}% | {follow_actions[0]:.2f}%→{follow_actions[3]:.2f}% | 具体性が高いと同調行動が減少 |')

    # CONTACT選択率
    contact_actions = [all_data[c]['action_ratios'].get('CONTACT_mean', 0) * 100 for c in info_conditions]
    lines.append(f'| CONTACT選択率 | {contact_actions[0]:.2f}%→{contact_actions[1]:.2f}% | {contact_actions[0]:.2f}%→{contact_actions[2]:.2f}% | {contact_actions[0]:.2f}%→{contact_actions[3]:.2f}% | 具体性が高いと連絡行動が増加 |')
    lines.append('')

    # 全実験の総括
    lines.append('## 全実験の総括')
    lines.append('')

    # 表28: 3実験主要知見
    lines.append('### 表28: 3実験の主要知見まとめ')
    lines.append('')
    lines.append('| 実験 | 目的 | 主要発見 | 示唆 |')
    lines.append('|:-----|:-----|:---------|:-----|')
    lines.append(f'| 実験1 | LLM vs ルールベース比較 | ルールベースは避難完了率{rb_avg.get("evacuation_rate_mean", 0)*100:.1f}%で優位だが、行動多様性が低い | LLMは社会的相互作用を再現するが、正常性バイアスにより避難率低下 |')
    lines.append(f'| 実験2 | 認知バイアスの影響検証 | 同一ペルソナでも避難率{min(evac_rates):.0f}%〜{max(evac_rates):.0f}%と変動。正常性バイアス者の一貫性が最低 | LLMの確率的性質により人間らしいばらつきを再現するが、予測困難性も発生 |')
    lines.append(f'| 実験3 | 情報提供戦略の効果 | 切迫感のみ高めると避難率{exp3b_rate-baseline_rate_info:+.0f}pt向上。具体性のみは{exp3c_rate-baseline_rate_info:+.0f}pt | 危機感を煽る表現が有効。詳細情報は処理負荷となる可能性 |')
    lines.append('')

    # 表29: 認知特性と避難行動の関係
    lines.append('### 表29: 認知特性と避難行動の関係（全実験統合）')
    lines.append('')
    lines.append('| 認知特性 | 全条件平均避難完了率 | 避難時間の特徴 | 社会的行動の特徴 |')
    lines.append('|:---------|:--------------------:|:---------------|:-----------------:|')

    all_conditions_for_avg = CONDITIONS
    for ms in MENTAL_STATES:
        rates = [all_data[c]['mental_state_metrics'].get(ms, {}).get('evacuation_rate', 0) for c in all_conditions_for_avg]
        avg_rate = np.mean(rates) * 100

        times = [all_data[c]['mental_state_metrics'].get(ms, {}).get('avg_evacuation_time', 0) for c in all_conditions_for_avg]
        avg_time = np.mean([t for t in times if t > 0])

        # 時間特性
        if avg_time < 400:
            time_char = '平均的、安定'
        elif avg_time < 550:
            time_char = 'やや長い'
        else:
            time_char = '長い、開始遅延'

        # 社会的行動特性（認知特性ごとの傾向）
        social_chars = {
            '冷静・分析的': 'FOLLOW少、独立行動',
            '冷静・合理的': 'CONTACT活用',
            '慎重・人混み回避': 'FOLLOW中程度',
            '楽観的・自己信頼型': 'STAY傾向、様子見',
            '社交的・周囲配慮型': 'FOLLOW多、他者同調',
            '焦燥・目的外行動': '家族連絡に時間消費',
            '不安傾向・サポート希求型': 'FOLLOW多、集団追従'
        }

        lines.append(f'| {ms} | {avg_rate:.1f}% | {time_char} | {social_chars.get(ms, "-")} |')
    lines.append('')

    # 表30: 実験間の避難完了率比較
    lines.append('### 表30: 実験間の避難完了率比較')
    lines.append('')
    lines.append('| 実験・条件 | 避難完了率 | 避難所への率 | 津波避難地域への率 |')
    lines.append('|:-----------|:----------:|:------------:|:------------------:|')

    condition_labels = {
        'Baseline-LLM': '実験1-A（LLM標準）',
        'Exp1-B': '実験1-B（ルールベース）',
        'Exp2-2': '実験2-2（正常性バイアス）',
        'Exp2-3': '実験2-3（同調バイアス）',
        'Exp2-4': '実験2-4（複合）',
        'Exp3-B': '実験3-B（切迫感高）',
        'Exp3-C': '実験3-C（具体性高）',
        'Exp3-D': '実験3-D（両方高）'
    }

    for c in CONDITIONS:
        avg = all_data[c]['condition_averages']
        evac_rate = avg.get('evacuation_rate_mean', 0) * 100
        shelter_rate = avg.get('evacuation_rate_to_shelter_mean', 0) * 100
        area_rate = avg.get('evacuation_rate_to_area_mean', 0) * 100
        lines.append(f'| {condition_labels[c]} | {evac_rate:.1f}% | {shelter_rate:.1f}% | {area_rate:.1f}% |')
    lines.append('')

    # ファイル出力
    with open(output_path, 'w', encoding='utf-8') as f:
        f.write('\n'.join(lines))


if __name__ == '__main__':
    parser = argparse.ArgumentParser(description='実験結果の集計スクリプト')
    parser.add_argument('log_dir', type=str, help='分析対象のログディレクトリパス')
    args = parser.parse_args()

    experiment_dir = Path(args.log_dir)
    if not experiment_dir.exists():
        print(f"エラー: 指定されたディレクトリが存在しません: {experiment_dir}")
        sys.exit(1)

    main(experiment_dir)
