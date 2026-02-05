#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
避難開始時刻の分布を分析するスクリプト
実験2の4条件（ベースライン、正常性バイアス、同調バイアス、複合バイアス）について
避難開始時刻の分布を統計的に分析する
"""

import pandas as pd
import numpy as np
import os
from pathlib import Path
import json

# 実験結果のディレクトリ
EXPERIMENT_DIR = Path("/Users/harukidoi/code/itolabo/simulation/evacuation_simulation/Logs/experiment_results/20260125_014206")

# 実験条件の定義
CONDITIONS = {
    "Baseline-LLM": "条件1（ベースライン）",
    "Exp2-2": "条件2（正常性バイアス）",
    "Exp2-3": "条件3（同調バイアス）",
    "Exp2-4": "条件4（複合）"
}

def load_evacuation_start_times(condition_prefix, num_trials=5):
    """
    指定した条件の全トライアルから避難開始時刻を読み込む
    """
    all_start_times = []

    for trial in range(1, num_trials + 1):
        agents_file = EXPERIMENT_DIR / f"{condition_prefix}_trial_{trial}_agents.csv"

        if not agents_file.exists():
            print(f"警告: {agents_file} が見つかりません")
            continue

        df = pd.read_csv(agents_file)

        # first_evacuate_timeが0以上の値（避難を開始した）エージェントのみ抽出
        # NaNや負の値は除外
        start_times = df[df['first_evacuate_time'] >= 0]['first_evacuate_time'].values
        all_start_times.extend(start_times)

    return np.array(all_start_times)

def calculate_statistics(start_times):
    """
    避難開始時刻の統計量を計算
    """
    if len(start_times) == 0:
        return None

    return {
        "count": len(start_times),
        "mean": np.mean(start_times),
        "median": np.median(start_times),
        "std": np.std(start_times),
        "min": np.min(start_times),
        "max": np.max(start_times),
        "q25": np.percentile(start_times, 25),
        "q75": np.percentile(start_times, 75),
    }

def analyze_distribution_bins(start_times, bin_edges):
    """
    指定した時間区分での分布を計算
    """
    if len(start_times) == 0:
        return None

    hist, _ = np.histogram(start_times, bins=bin_edges)
    percentages = (hist / len(start_times)) * 100

    return {
        "counts": hist.tolist(),
        "percentages": percentages.tolist()
    }

def main():
    print("=" * 80)
    print("避難開始時刻の分布分析")
    print("=" * 80)

    results = {}

    # 時間区分の定義（秒単位）
    # 0-2分、2-5分、5-10分、10-20分、20-30分
    bin_edges = [0, 120, 300, 600, 1200, 1800]
    bin_labels = ["0-2分", "2-5分", "5-10分", "10-20分", "20-30分"]

    for condition_key, condition_name in CONDITIONS.items():
        print(f"\n{condition_name} ({condition_key})")
        print("-" * 80)

        # 避難開始時刻を読み込み
        start_times = load_evacuation_start_times(condition_key)

        if len(start_times) == 0:
            print(f"  データなし")
            continue

        # 統計量を計算
        stats = calculate_statistics(start_times)

        print(f"  サンプル数: {stats['count']}")
        print(f"  平均: {stats['mean']:.2f}秒 ({stats['mean']/60:.2f}分)")
        print(f"  中央値: {stats['median']:.2f}秒 ({stats['median']/60:.2f}分)")
        print(f"  標準偏差: {stats['std']:.2f}秒 ({stats['std']/60:.2f}分)")
        print(f"  最小値: {stats['min']:.2f}秒")
        print(f"  最大値: {stats['max']:.2f}秒 ({stats['max']/60:.2f}分)")
        print(f"  第1四分位: {stats['q25']:.2f}秒 ({stats['q25']/60:.2f}分)")
        print(f"  第3四分位: {stats['q75']:.2f}秒 ({stats['q75']/60:.2f}分)")

        # 時間区分別の分布
        dist = analyze_distribution_bins(start_times, bin_edges)
        print(f"\n  時間区分別分布:")
        for i, label in enumerate(bin_labels):
            print(f"    {label}: {dist['counts'][i]}名 ({dist['percentages'][i]:.1f}%)")

        # 結果を保存
        results[condition_key] = {
            "condition_name": condition_name,
            "statistics": stats,
            "distribution": {
                "bin_edges": bin_edges,
                "bin_labels": bin_labels,
                "counts": dist['counts'],
                "percentages": dist['percentages']
            }
        }

    # 結果をJSONファイルに保存
    output_file = EXPERIMENT_DIR / "evacuation_start_time_analysis.json"
    with open(output_file, 'w', encoding='utf-8') as f:
        json.dump(results, f, ensure_ascii=False, indent=2)

    print(f"\n結果を {output_file} に保存しました")

    # LaTeX表形式で出力
    print("\n" + "=" * 80)
    print("LaTeX表形式の出力")
    print("=" * 80)

    print("\n% 避難開始時刻の統計量")
    print("\\begin{table}[htbp]")
    print("\\centering")
    print("\\caption{認知バイアス条件別の避難開始時刻の統計量}")
    print("\\label{tab:evacuation-start-time-stats}")
    print("\\footnotesize")
    print("\\begin{tabular}{|l|c|c|c|c|}")
    print("\\hline")
    print("\\textbf{統計量} & \\textbf{条件1} & \\textbf{条件2} & \\textbf{条件3} & \\textbf{条件4} \\\\")
    print(" & （ベースライン） & （正常性バイアス） & （同調バイアス） & （複合） \\\\")
    print("\\hline")

    # 平均値の行
    means = [results[k]["statistics"]["mean"] for k in ["Baseline-LLM", "Exp2-2", "Exp2-3", "Exp2-4"]]
    print(f"平均（秒） & {means[0]:.2f} & {means[1]:.2f} & {means[2]:.2f} & {means[3]:.2f} \\\\")

    # 中央値の行
    medians = [results[k]["statistics"]["median"] for k in ["Baseline-LLM", "Exp2-2", "Exp2-3", "Exp2-4"]]
    print(f"中央値（秒） & {medians[0]:.2f} & {medians[1]:.2f} & {medians[2]:.2f} & {medians[3]:.2f} \\\\")

    # 標準偏差の行
    stds = [results[k]["statistics"]["std"] for k in ["Baseline-LLM", "Exp2-2", "Exp2-3", "Exp2-4"]]
    print(f"標準偏差（秒） & {stds[0]:.2f} & {stds[1]:.2f} & {stds[2]:.2f} & {stds[3]:.2f} \\\\")

    # 最小値の行
    mins = [results[k]["statistics"]["min"] for k in ["Baseline-LLM", "Exp2-2", "Exp2-3", "Exp2-4"]]
    print(f"最小値（秒） & {mins[0]:.2f} & {mins[1]:.2f} & {mins[2]:.2f} & {mins[3]:.2f} \\\\")

    # 最大値の行
    maxs = [results[k]["statistics"]["max"] for k in ["Baseline-LLM", "Exp2-2", "Exp2-3", "Exp2-4"]]
    print(f"最大値（秒） & {maxs[0]:.2f} & {maxs[1]:.2f} & {maxs[2]:.2f} & {maxs[3]:.2f} \\\\ \\hline")

    print("\\end{tabular}")
    print("\\end{table}")

    # 時間区分別分布の表
    print("\n% 避難開始時刻の時間区分別分布")
    print("\\begin{table}[htbp]")
    print("\\centering")
    print("\\caption{認知バイアス条件別の避難開始時刻の分布}")
    print("\\label{tab:evacuation-start-time-distribution}")
    print("\\footnotesize")
    print("\\begin{tabular}{|l|c|c|c|c|}")
    print("\\hline")
    print("\\textbf{時間区分} & \\textbf{条件1} & \\textbf{条件2} & \\textbf{条件3} & \\textbf{条件4} \\\\")
    print(" & （ベースライン） & （正常性バイアス） & （同調バイアス） & （複合） \\\\")
    print("\\hline")

    for i, label in enumerate(bin_labels):
        percentages = [results[k]["distribution"]["percentages"][i] for k in ["Baseline-LLM", "Exp2-2", "Exp2-3", "Exp2-4"]]
        print(f"{label} & {percentages[0]:.1f}\\% & {percentages[1]:.1f}\\% & {percentages[2]:.1f}\\% & {percentages[3]:.1f}\\% \\\\")

    print("\\hline")
    print("\\end{tabular}")
    print("\\end{table}")

if __name__ == "__main__":
    main()
