#!/usr/bin/env python3
"""
LLMエージェントの推論分析スクリプト
長期目標・中期計画の成功者/失敗者間比較、目標切り替えタイミングの分析を行う

分析内容:
1. 長期目標(long_term_goal)のカテゴリ分類と集計
2. 中期計画(mid_term_plan)の分析
3. 避難成功者 vs 失敗者の推論比較
4. 認知特性別の推論パターン分析
5. 目標切り替えタイミングの分析
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
from typing import Dict, List, Tuple, Optional

# 実験結果のディレクトリ（デフォルト値、コマンドライン引数で上書き可能）
RESULT_DIR = None

def get_result_dir() -> Path:
    """実験結果ディレクトリを取得"""
    global RESULT_DIR
    if RESULT_DIR is None:
        raise ValueError("RESULT_DIR が設定されていません。コマンドライン引数でログディレクトリを指定してください。")
    return RESULT_DIR

# 長期目標のカテゴリ定義（キーワードベース）
GOAL_CATEGORIES = {
    "安全確保": [
        "安全", "避難", "逃げ", "高台", "高い場所", "津波", "逃れ", "守り", "生き", "助か"
    ],
    "家族合流": [
        "家族", "合流", "再会", "会いたい", "一緒", "迎え", "連絡", "安否", "心配"
    ],
    "情報収集": [
        "状況", "確認", "把握", "情報", "知りたい", "わからない", "様子", "判断"
    ],
    "様子見": [
        "落ち着", "待", "様子を見", "静観", "冷静", "慌てず", "焦らず"
    ]
}

# 中期計画の行動カテゴリ
PLAN_CATEGORIES = {
    "避難所移動": [
        "避難所", "高台", "向かう", "移動", "逃げる", "走る", "急ぐ"
    ],
    "家族関連": [
        "家族", "探す", "連絡", "電話", "合流", "迎え"
    ],
    "情報収集": [
        "確認", "聞く", "情報", "様子", "状況"
    ],
    "待機": [
        "待つ", "様子を見", "落ち着く", "静観"
    ],
    "同調": [
        "ついていく", "一緒に", "周り", "他の人", "従う"
    ]
}


def load_agent_data(condition: str, trial: int) -> Optional[pd.DataFrame]:
    """エージェントデータを読み込む"""
    filepath = get_result_dir() / f"{condition}_trial_{trial}_agents.csv"
    if filepath.exists():
        return pd.read_csv(filepath)
    return None


def load_action_data(condition: str, trial: int) -> Optional[pd.DataFrame]:
    """行動データを読み込む"""
    filepath = get_result_dir() / f"{condition}_trial_{trial}_actions.csv"
    if filepath.exists():
        return pd.read_csv(filepath)
    return None


def load_llm_decisions(agent_id: int) -> List[Dict]:
    """LLM決定ログを読み込む"""
    filepath = get_result_dir() / "llm_decisions" / f"{agent_id}.txt"
    if not filepath.exists():
        return []

    decisions = []
    current_json = ""
    in_json = False

    with open(filepath, 'r', encoding='utf-8') as f:
        content = f.read()

    # JSONブロックを抽出（================で区切られている）
    blocks = content.split("=" * 80)

    for block in blocks:
        block = block.strip()
        if not block:
            continue

        # JSONの開始を探す
        if block.startswith("{"):
            try:
                # 最初の}で終わる完全なJSONを見つける
                json_end = block.find("\n\n")
                if json_end == -1:
                    json_end = len(block)
                json_str = block[:json_end].strip()

                # 不完全なJSONを修正（閉じ括弧がない場合）
                open_braces = json_str.count('{')
                close_braces = json_str.count('}')
                json_str += '}' * (open_braces - close_braces)

                data = json.loads(json_str)
                decisions.append(data)
            except json.JSONDecodeError:
                continue

    return decisions


def categorize_goal(goal_text: str) -> str:
    """長期目標をカテゴリに分類"""
    if not goal_text:
        return "不明"

    goal_lower = goal_text.lower()

    # 各カテゴリのキーワードをチェック
    category_scores = {}
    for category, keywords in GOAL_CATEGORIES.items():
        score = sum(1 for kw in keywords if kw in goal_lower)
        category_scores[category] = score

    # 最もスコアの高いカテゴリを返す
    if max(category_scores.values()) > 0:
        return max(category_scores, key=category_scores.get)

    return "その他"


def categorize_plan(plan_text: str) -> str:
    """中期計画をカテゴリに分類"""
    if not plan_text:
        return "不明"

    plan_lower = plan_text.lower()

    # 各カテゴリのキーワードをチェック
    category_scores = {}
    for category, keywords in PLAN_CATEGORIES.items():
        score = sum(1 for kw in keywords if kw in plan_lower)
        category_scores[category] = score

    # 最もスコアの高いカテゴリを返す
    if max(category_scores.values()) > 0:
        return max(category_scores, key=category_scores.get)

    return "その他"


def extract_goal_from_output(output: Dict) -> str:
    """出力から長期目標テキストを抽出"""
    if not output:
        return ""

    long_term_goal = output.get("long_term_goal", {})

    if isinstance(long_term_goal, str):
        return long_term_goal
    elif isinstance(long_term_goal, dict):
        return long_term_goal.get("primary_goal", "")

    return ""


def extract_plan_from_output(output: Dict) -> str:
    """出力から中期計画テキストを抽出"""
    if not output:
        return ""

    mid_term_plan = output.get("mid_term_plan", {})

    if isinstance(mid_term_plan, str):
        return mid_term_plan
    elif isinstance(mid_term_plan, dict):
        steps = mid_term_plan.get("steps", [])
        if steps:
            return steps[0] if isinstance(steps, list) else str(steps)

    return ""


def analyze_agent_goals(agent_id: int, decisions: List[Dict]) -> Dict:
    """1エージェントの目標分析"""
    results = {
        "agent_id": agent_id,
        "total_decisions": len(decisions),
        "goal_sequence": [],
        "plan_sequence": [],
        "goal_transitions": [],
        "initial_goal": None,
        "initial_goal_category": None,
        "final_goal": None,
        "final_goal_category": None,
        "goal_category_counts": defaultdict(int),
        "plan_category_counts": defaultdict(int)
    }

    prev_goal_category = None

    for i, decision in enumerate(decisions):
        output = decision.get("output", {})
        timestamp = decision.get("episode_elapsed_time", 0)
        action_type = output.get("action_type", "")

        goal_text = extract_goal_from_output(output)
        plan_text = extract_plan_from_output(output)

        goal_category = categorize_goal(goal_text)
        plan_category = categorize_plan(plan_text)

        # シーケンスに追加
        results["goal_sequence"].append({
            "timestamp": timestamp,
            "goal_text": goal_text,
            "goal_category": goal_category,
            "action_type": action_type
        })
        results["plan_sequence"].append({
            "timestamp": timestamp,
            "plan_text": plan_text,
            "plan_category": plan_category,
            "action_type": action_type
        })

        # カウント
        results["goal_category_counts"][goal_category] += 1
        results["plan_category_counts"][plan_category] += 1

        # 初期・最終目標
        if i == 0:
            results["initial_goal"] = goal_text
            results["initial_goal_category"] = goal_category
        results["final_goal"] = goal_text
        results["final_goal_category"] = goal_category

        # 目標の切り替え検出
        if prev_goal_category and prev_goal_category != goal_category:
            results["goal_transitions"].append({
                "timestamp": timestamp,
                "from_category": prev_goal_category,
                "to_category": goal_category,
                "trigger_action": action_type
            })

        prev_goal_category = goal_category

    return results


def analyze_success_vs_failure(condition: str = "Baseline-LLM", trial: int = 1) -> Dict:
    """避難成功者 vs 失敗者の目標分析比較"""
    agents_df = load_agent_data(condition, trial)
    if agents_df is None:
        return {}

    succeeded_ids = agents_df[agents_df['survived'] == True]['agent_id'].tolist()
    failed_ids = agents_df[agents_df['survived'] == False]['agent_id'].tolist()

    results = {
        "succeeded": {
            "count": len(succeeded_ids),
            "initial_goal_distribution": defaultdict(int),
            "final_goal_distribution": defaultdict(int),
            "avg_goal_transitions": 0,
            "goal_category_overall": defaultdict(int),
            "examples": []
        },
        "failed": {
            "count": len(failed_ids),
            "initial_goal_distribution": defaultdict(int),
            "final_goal_distribution": defaultdict(int),
            "avg_goal_transitions": 0,
            "goal_category_overall": defaultdict(int),
            "examples": []
        }
    }

    # 成功者の分析
    total_transitions_succeeded = 0
    for agent_id in succeeded_ids:
        decisions = load_llm_decisions(agent_id)
        if not decisions:
            continue

        analysis = analyze_agent_goals(agent_id, decisions)

        if analysis["initial_goal_category"]:
            results["succeeded"]["initial_goal_distribution"][analysis["initial_goal_category"]] += 1
        if analysis["final_goal_category"]:
            results["succeeded"]["final_goal_distribution"][analysis["final_goal_category"]] += 1

        for cat, count in analysis["goal_category_counts"].items():
            results["succeeded"]["goal_category_overall"][cat] += count

        total_transitions_succeeded += len(analysis["goal_transitions"])

        # 例を収集（最初の5件）
        if len(results["succeeded"]["examples"]) < 5:
            agent_info = agents_df[agents_df['agent_id'] == agent_id].iloc[0]
            results["succeeded"]["examples"].append({
                "agent_id": agent_id,
                "persona_name": agent_info['persona_name'],
                "mental_state": agent_info['mental_state'],
                "initial_goal": analysis["initial_goal"],
                "final_goal": analysis["final_goal"],
                "transitions": analysis["goal_transitions"]
            })

    if succeeded_ids:
        results["succeeded"]["avg_goal_transitions"] = total_transitions_succeeded / len(succeeded_ids)

    # 失敗者の分析
    total_transitions_failed = 0
    for agent_id in failed_ids:
        decisions = load_llm_decisions(agent_id)
        if not decisions:
            continue

        analysis = analyze_agent_goals(agent_id, decisions)

        if analysis["initial_goal_category"]:
            results["failed"]["initial_goal_distribution"][analysis["initial_goal_category"]] += 1
        if analysis["final_goal_category"]:
            results["failed"]["final_goal_distribution"][analysis["final_goal_category"]] += 1

        for cat, count in analysis["goal_category_counts"].items():
            results["failed"]["goal_category_overall"][cat] += count

        total_transitions_failed += len(analysis["goal_transitions"])

        # 例を収集（最初の5件）
        if len(results["failed"]["examples"]) < 5:
            agent_info = agents_df[agents_df['agent_id'] == agent_id].iloc[0]
            results["failed"]["examples"].append({
                "agent_id": agent_id,
                "persona_name": agent_info['persona_name'],
                "mental_state": agent_info['mental_state'],
                "initial_goal": analysis["initial_goal"],
                "final_goal": analysis["final_goal"],
                "transitions": analysis["goal_transitions"]
            })

    if failed_ids:
        results["failed"]["avg_goal_transitions"] = total_transitions_failed / len(failed_ids)

    return results


def analyze_goal_transition_timing(condition: str = "Baseline-LLM", trial: int = 1) -> Dict:
    """目標切り替えタイミングの分析"""
    agents_df = load_agent_data(condition, trial)
    if agents_df is None:
        return {}

    all_agent_ids = agents_df['agent_id'].tolist()

    # 時間帯別の目標切り替え
    time_bins = [0, 120, 300, 600, 900, 1200, 1500, 1800]
    time_labels = ['0-2min', '2-5min', '5-10min', '10-15min', '15-20min', '20-25min', '25-30min']

    transition_by_time = {label: defaultdict(int) for label in time_labels}
    transition_patterns = defaultdict(int)  # "情報収集→安全確保" などのパターン

    # 「安全確保」への転換タイミング
    safety_transition_times = []

    for agent_id in all_agent_ids:
        decisions = load_llm_decisions(agent_id)
        if not decisions:
            continue

        analysis = analyze_agent_goals(agent_id, decisions)

        for transition in analysis["goal_transitions"]:
            timestamp = transition["timestamp"]
            from_cat = transition["from_category"]
            to_cat = transition["to_category"]

            # パターンカウント
            pattern = f"{from_cat}→{to_cat}"
            transition_patterns[pattern] += 1

            # 時間帯別カウント
            if timestamp is not None:
                for i, (start, end) in enumerate(zip(time_bins[:-1], time_bins[1:])):
                    if start <= timestamp < end:
                        transition_by_time[time_labels[i]][pattern] += 1
                        break

            # 「安全確保」への転換タイミング
            if to_cat == "安全確保" and from_cat != "安全確保":
                survived = agents_df[agents_df['agent_id'] == agent_id]['survived'].values[0]
                safety_transition_times.append({
                    "agent_id": agent_id,
                    "timestamp": timestamp,
                    "from_category": from_cat,
                    "survived": survived
                })

    return {
        "transition_patterns": dict(transition_patterns),
        "transition_by_time": {k: dict(v) for k, v in transition_by_time.items()},
        "safety_transition_times": safety_transition_times,
        "avg_safety_transition_time": np.mean([t["timestamp"] for t in safety_transition_times]) if safety_transition_times else None,
        "safety_transition_survival_rate": np.mean([t["survived"] for t in safety_transition_times]) * 100 if safety_transition_times else None
    }


def analyze_by_mental_state(condition: str = "Baseline-LLM", trial: int = 1) -> Dict:
    """認知特性別の目標パターン分析"""
    agents_df = load_agent_data(condition, trial)
    if agents_df is None:
        return {}

    mental_states = agents_df['mental_state'].unique()
    results = {}

    for state in mental_states:
        state_agents = agents_df[agents_df['mental_state'] == state]
        agent_ids = state_agents['agent_id'].tolist()

        state_results = {
            "count": len(agent_ids),
            "initial_goal_distribution": defaultdict(int),
            "goal_category_overall": defaultdict(int),
            "avg_goal_transitions": 0,
            "survival_rate": state_agents['survived'].mean() * 100
        }

        total_transitions = 0
        agents_with_data = 0

        for agent_id in agent_ids:
            decisions = load_llm_decisions(agent_id)
            if not decisions:
                continue

            agents_with_data += 1
            analysis = analyze_agent_goals(agent_id, decisions)

            if analysis["initial_goal_category"]:
                state_results["initial_goal_distribution"][analysis["initial_goal_category"]] += 1

            for cat, count in analysis["goal_category_counts"].items():
                state_results["goal_category_overall"][cat] += count

            total_transitions += len(analysis["goal_transitions"])

        if agents_with_data > 0:
            state_results["avg_goal_transitions"] = total_transitions / agents_with_data

        results[state] = state_results

    return results


def generate_paper_content(results: Dict) -> str:
    """論文に追記する内容を生成"""
    content = []

    content.append("=" * 80)
    content.append("長期目標・中期計画の分析結果")
    content.append("=" * 80)

    # 1. 成功者 vs 失敗者の比較
    if "success_vs_failure" in results:
        svf = results["success_vs_failure"]
        content.append("\n## 1. 避難成功者と失敗者の長期目標比較\n")

        content.append("### 初期目標の分布")
        content.append("| カテゴリ | 成功者 | 失敗者 |")
        content.append("|----------|--------|--------|")

        all_cats = set(svf["succeeded"]["initial_goal_distribution"].keys()) | set(svf["failed"]["initial_goal_distribution"].keys())
        for cat in sorted(all_cats):
            s_count = svf["succeeded"]["initial_goal_distribution"].get(cat, 0)
            f_count = svf["failed"]["initial_goal_distribution"].get(cat, 0)
            s_pct = s_count / max(svf["succeeded"]["count"], 1) * 100
            f_pct = f_count / max(svf["failed"]["count"], 1) * 100
            content.append(f"| {cat} | {s_count} ({s_pct:.1f}%) | {f_count} ({f_pct:.1f}%) |")

        content.append(f"\n平均目標切り替え回数: 成功者 {svf['succeeded']['avg_goal_transitions']:.2f}回, 失敗者 {svf['failed']['avg_goal_transitions']:.2f}回")

    # 2. 目標切り替えパターン
    if "transition_timing" in results:
        tt = results["transition_timing"]
        content.append("\n## 2. 目標切り替えパターン\n")

        content.append("### 主要な切り替えパターン")
        patterns = sorted(tt["transition_patterns"].items(), key=lambda x: -x[1])[:10]
        for pattern, count in patterns:
            content.append(f"  - {pattern}: {count}回")

        if tt["avg_safety_transition_time"]:
            content.append(f"\n「安全確保」への目標切り替え:")
            content.append(f"  - 平均切り替え時刻: {tt['avg_safety_transition_time']:.1f}秒")
            content.append(f"  - 切り替えた人の生存率: {tt['safety_transition_survival_rate']:.1f}%")

    # 3. 認知特性別
    if "by_mental_state" in results:
        bms = results["by_mental_state"]
        content.append("\n## 3. 認知特性別の初期目標\n")

        content.append("| 認知特性 | 安全確保 | 家族合流 | 情報収集 | 様子見 | 生存率 |")
        content.append("|----------|----------|----------|----------|--------|--------|")

        for state, data in bms.items():
            total = max(sum(data["initial_goal_distribution"].values()), 1)
            安全 = data["initial_goal_distribution"].get("安全確保", 0) / total * 100
            家族 = data["initial_goal_distribution"].get("家族合流", 0) / total * 100
            情報 = data["initial_goal_distribution"].get("情報収集", 0) / total * 100
            様子 = data["initial_goal_distribution"].get("様子見", 0) / total * 100
            content.append(f"| {state} | {安全:.1f}% | {家族:.1f}% | {情報:.1f}% | {様子:.1f}% | {data['survival_rate']:.1f}% |")

    return "\n".join(content)


def main():
    print("=" * 80)
    print("LLMエージェントの推論分析（長期目標・中期計画）")
    print("=" * 80)

    results = {}

    # 1. 成功者 vs 失敗者の分析
    print("\n[分析1] 避難成功者 vs 失敗者の目標比較")
    print("-" * 40)

    svf = analyze_success_vs_failure("Baseline-LLM", 1)
    results["success_vs_failure"] = svf

    if svf:
        print(f"\n成功者 ({svf['succeeded']['count']}名):")
        print("  初期目標分布:")
        for cat, count in sorted(svf["succeeded"]["initial_goal_distribution"].items(), key=lambda x: -x[1]):
            pct = count / max(svf["succeeded"]["count"], 1) * 100
            print(f"    {cat}: {count}名 ({pct:.1f}%)")

        print(f"\n失敗者 ({svf['failed']['count']}名):")
        print("  初期目標分布:")
        for cat, count in sorted(svf["failed"]["initial_goal_distribution"].items(), key=lambda x: -x[1]):
            pct = count / max(svf["failed"]["count"], 1) * 100
            print(f"    {cat}: {count}名 ({pct:.1f}%)")

        print(f"\n平均目標切り替え回数:")
        print(f"  成功者: {svf['succeeded']['avg_goal_transitions']:.2f}回")
        print(f"  失敗者: {svf['failed']['avg_goal_transitions']:.2f}回")

        # 具体例
        print("\n【成功者の例】")
        for ex in svf["succeeded"]["examples"][:3]:
            print(f"  {ex['persona_name']}({ex['mental_state']}):")
            print(f"    初期目標: 「{ex['initial_goal']}」")
            print(f"    最終目標: 「{ex['final_goal']}」")
            if ex['transitions']:
                print(f"    切り替え: {len(ex['transitions'])}回")

        print("\n【失敗者の例】")
        for ex in svf["failed"]["examples"][:3]:
            print(f"  {ex['persona_name']}({ex['mental_state']}):")
            print(f"    初期目標: 「{ex['initial_goal']}」")
            print(f"    最終目標: 「{ex['final_goal']}」")
            if ex['transitions']:
                print(f"    切り替え: {len(ex['transitions'])}回")

    # 2. 目標切り替えタイミングの分析
    print("\n[分析2] 目標切り替えタイミング")
    print("-" * 40)

    tt = analyze_goal_transition_timing("Baseline-LLM", 1)
    results["transition_timing"] = tt

    if tt:
        print("\n主要な切り替えパターン:")
        patterns = sorted(tt["transition_patterns"].items(), key=lambda x: -x[1])[:10]
        for pattern, count in patterns:
            print(f"  {pattern}: {count}回")

        if tt["avg_safety_transition_time"]:
            print(f"\n「安全確保」への目標切り替え:")
            print(f"  平均切り替え時刻: {tt['avg_safety_transition_time']:.1f}秒")
            print(f"  切り替えた人の生存率: {tt['safety_transition_survival_rate']:.1f}%")

        print("\n時間帯別の切り替え頻度:")
        for time_bin, patterns in tt["transition_by_time"].items():
            if patterns:
                total = sum(patterns.values())
                print(f"  {time_bin}: {total}回")

    # 3. 認知特性別の分析
    print("\n[分析3] 認知特性別の目標パターン")
    print("-" * 40)

    bms = analyze_by_mental_state("Baseline-LLM", 1)
    results["by_mental_state"] = bms

    if bms:
        for state, data in sorted(bms.items(), key=lambda x: -x[1]["survival_rate"]):
            print(f"\n{state} (生存率: {data['survival_rate']:.1f}%):")
            print("  初期目標:")
            total = max(sum(data["initial_goal_distribution"].values()), 1)
            for cat, count in sorted(data["initial_goal_distribution"].items(), key=lambda x: -x[1]):
                pct = count / total * 100
                print(f"    {cat}: {pct:.1f}%")
            print(f"  平均目標切り替え回数: {data['avg_goal_transitions']:.2f}回")

    # 論文用の内容を生成
    paper_content = generate_paper_content(results)
    print("\n" + paper_content)

    # 結果をJSONで保存
    def convert_for_json(obj):
        if isinstance(obj, (np.int64, np.int32)):
            return int(obj)
        elif isinstance(obj, (np.float64, np.float32)):
            return float(obj)
        elif isinstance(obj, np.ndarray):
            return obj.tolist()
        elif isinstance(obj, np.bool_):
            return bool(obj)
        elif isinstance(obj, defaultdict):
            return dict(obj)
        elif isinstance(obj, dict):
            return {k: convert_for_json(v) for k, v in obj.items()}
        elif isinstance(obj, list):
            return [convert_for_json(v) for v in obj]
        return obj

    output_converted = convert_for_json(results)

    output_path = get_result_dir() / "reasoning_analysis_results.json"
    with open(output_path, 'w', encoding='utf-8') as f:
        json.dump(output_converted, f, ensure_ascii=False, indent=2)

    print(f"\n結果を保存: {output_path}")

    # Markdown形式でも保存
    md_path = get_result_dir() / "reasoning_analysis_results.md"
    with open(md_path, 'w', encoding='utf-8') as f:
        f.write(paper_content)

    print(f"Markdown形式で保存: {md_path}")


if __name__ == "__main__":
    parser = argparse.ArgumentParser(description='LLMエージェントの推論分析スクリプト')
    parser.add_argument('log_dir', type=str, help='分析対象のログディレクトリパス')
    args = parser.parse_args()

    RESULT_DIR = Path(args.log_dir)
    if not RESULT_DIR.exists():
        print(f"エラー: 指定されたディレクトリが存在しません: {RESULT_DIR}")
        sys.exit(1)

    main()
