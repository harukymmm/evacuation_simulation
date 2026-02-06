#!/usr/bin/env python3
"""
認知特性別避難完了率の分析スクリプト
Baseline-LLMとBaseline-RuleBased（Exp1-B）の実験結果を比較
"""

import pandas as pd
import os
from collections import defaultdict

# 実験結果ディレクトリ
EXPERIMENT_DIR = "/Users/harukidoi/code/itolabo/simulation/evacuation_simulation/Logs/experiment_results/20260125_014206"

# 7つの認知特性
COGNITIVE_STATES = [
    "冷静・分析的",
    "冷静・合理的",
    "慎重・人混み恐怖",
    "楽観的・自己信頼型",
    "社交的・周囲配慮型",
    "焦燥・目的外行動",
    "不安傾向・サポート希求型"
]

def analyze_experiment(experiment_name, num_trials=5):
    """
    指定された実験の全トライアルを分析し、認知特性別の避難完了率を計算
    """
    # 各認知特性の避難完了数と総数を集計
    cognitive_stats = defaultdict(lambda: {"completed": 0, "total": 0})

    for trial in range(1, num_trials + 1):
        agents_file = os.path.join(EXPERIMENT_DIR, f"{experiment_name}_trial_{trial}_agents.csv")

        if not os.path.exists(agents_file):
            print(f"警告: {agents_file} が見つかりません")
            continue

        # agents.csvを読み込み
        df = pd.read_csv(agents_file)

        # 各認知特性ごとに集計
        for state in COGNITIVE_STATES:
            # mental_stateが一致するエージェントを抽出
            state_agents = df[df['mental_state'] == state]

            # 総数を加算
            cognitive_stats[state]["total"] += len(state_agents)

            # evacuation_completedがTrueのエージェント数を加算
            completed = state_agents[state_agents['evacuation_completed'] == True]
            cognitive_stats[state]["completed"] += len(completed)

    # 避難完了率を計算（5トライアルの平均）
    completion_rates = {}
    for state in COGNITIVE_STATES:
        total = cognitive_stats[state]["total"]
        completed = cognitive_stats[state]["completed"]

        if total > 0:
            # 5トライアルの平均避難完了率
            avg_rate = (completed / total) * 100
            completion_rates[state] = {
                "completed": completed,
                "total": total,
                "rate": avg_rate
            }
        else:
            completion_rates[state] = {
                "completed": 0,
                "total": 0,
                "rate": 0.0
            }

    return completion_rates

def print_results(experiment_name, results):
    """結果を表示"""
    print(f"\n{'='*80}")
    print(f"{experiment_name} - 認知特性別避難完了率（5トライアル平均）")
    print(f"{'='*80}")
    print(f"{'認知特性':<25} {'避難完了数':<12} {'総数':<8} {'完了率':<10}")
    print(f"{'-'*80}")

    for state in COGNITIVE_STATES:
        stats = results[state]
        print(f"{state:<25} {stats['completed']:<12} {stats['total']:<8} {stats['rate']:>6.2f}%")

    # 全体の完了率も計算
    total_completed = sum(r['completed'] for r in results.values())
    total_agents = sum(r['total'] for r in results.values())
    overall_rate = (total_completed / total_agents * 100) if total_agents > 0 else 0.0

    print(f"{'-'*80}")
    print(f"{'全体':<25} {total_completed:<12} {total_agents:<8} {overall_rate:>6.2f}%")
    print(f"{'='*80}")

def compare_results(llm_results, rule_results):
    """LLMとルールベースの結果を比較"""
    print(f"\n{'='*100}")
    print(f"認知特性別避難完了率の比較（5トライアル平均）")
    print(f"{'='*100}")
    print(f"{'認知特性':<25} {'LLM完了率':<15} {'ルール完了率':<15} {'差分':<15}")
    print(f"{'-'*100}")

    for state in COGNITIVE_STATES:
        llm_rate = llm_results[state]['rate']
        rule_rate = rule_results[state]['rate']
        diff = llm_rate - rule_rate

        diff_str = f"{diff:+.2f}%"
        print(f"{state:<25} {llm_rate:>6.2f}% {'':<7} {rule_rate:>6.2f}% {'':<7} {diff_str:<15}")

    # 全体の比較
    llm_total_completed = sum(r['completed'] for r in llm_results.values())
    llm_total_agents = sum(r['total'] for r in llm_results.values())
    llm_overall_rate = (llm_total_completed / llm_total_agents * 100) if llm_total_agents > 0 else 0.0

    rule_total_completed = sum(r['completed'] for r in rule_results.values())
    rule_total_agents = sum(r['total'] for r in rule_results.values())
    rule_overall_rate = (rule_total_completed / rule_total_agents * 100) if rule_total_agents > 0 else 0.0

    overall_diff = llm_overall_rate - rule_overall_rate

    print(f"{'-'*100}")
    print(f"{'全体':<25} {llm_overall_rate:>6.2f}% {'':<7} {rule_overall_rate:>6.2f}% {'':<7} {overall_diff:+.2f}%")
    print(f"{'='*100}")

def main():
    print("認知特性別避難完了率の分析を開始します...")

    # Baseline-LLMの分析
    print("\n[1/2] Baseline-LLMを分析中...")
    llm_results = analyze_experiment("Baseline-LLM")
    print_results("Baseline-LLM（LLMエージェント）", llm_results)

    # Exp1-B（ルールベース）の分析
    print("\n[2/2] Exp1-B（Baseline-RuleBased）を分析中...")
    rule_results = analyze_experiment("Exp1-B")
    print_results("Exp1-B（ルールベースエージェント）", rule_results)

    # 比較結果の表示
    compare_results(llm_results, rule_results)

    print("\n分析が完了しました。")

if __name__ == "__main__":
    main()
