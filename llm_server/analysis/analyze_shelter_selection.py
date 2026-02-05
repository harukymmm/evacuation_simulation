#!/usr/bin/env python3
"""
避難所選択の分析スクリプト
実験3の各条件における避難所選択の分布（標高ごとの割合）を分析
"""

import pandas as pd
import json
from pathlib import Path
from collections import defaultdict
import numpy as np

LOG_DIR = Path("Logs/experiment_results/20260125_014206")

# 実験3の条件マッピング
EXP3_CONDITIONS = {
    "Baseline-LLM": "条件A（標準）",
    "Exp3-B": "条件B（切迫感高）",
    "Exp3-C": "条件C（具体性高）",
    "Exp3-D": "条件D（両方高）"
}

def load_agent_data(condition, trial):
    """指定条件・トライアルのエージェントデータを読み込み"""
    file_path = LOG_DIR / f"{condition}_trial_{trial}_agents.csv"
    if not file_path.exists():
        return None
    return pd.read_csv(file_path)

def analyze_shelter_selection_by_elevation(condition, num_trials=5):
    """標高別の避難所選択割合を分析"""
    all_data = []

    for trial in range(1, num_trials + 1):
        df = load_agent_data(condition, trial)
        if df is None:
            continue

        # 避難完了したエージェントのみ
        evacuated = df[df['evacuation_completed'] == True].copy()
        all_data.append(evacuated)

    if not all_data:
        return None

    # 全トライアルを結合
    combined = pd.concat(all_data, ignore_index=True)

    # 標高カテゴリに分類
    def categorize_elevation(elev):
        if pd.isna(elev):
            return "不明"
        elif elev < 10:
            return "10m未満"
        elif elev < 15:
            return "10-15m"
        elif elev < 20:
            return "15-20m"
        elif elev < 25:
            return "20-25m"
        elif elev < 30:
            return "25-30m"
        else:
            return "30m以上"

    combined['elevation_category'] = combined['final_elevation'].apply(categorize_elevation)

    # 標高カテゴリごとの集計
    elevation_counts = combined['elevation_category'].value_counts()
    total = len(combined)

    # パーセンテージ計算
    elevation_percentages = (elevation_counts / total * 100).round(1)

    # 避難所ごとの集計（上位5つ）
    shelter_counts = combined['final_shelter'].value_counts().head(5)

    # 平均標高
    avg_elevation = combined['final_elevation'].mean()

    return {
        'elevation_distribution': elevation_percentages.to_dict(),
        'top_shelters': shelter_counts.to_dict(),
        'average_elevation': avg_elevation,
        'total_evacuated': total
    }

def analyze_all_conditions():
    """全条件の避難所選択を分析"""
    results = {}

    for condition_code, condition_name in EXP3_CONDITIONS.items():
        print(f"分析中: {condition_name} ({condition_code})")
        analysis = analyze_shelter_selection_by_elevation(condition_code)

        if analysis:
            results[condition_name] = analysis
            print(f"  避難完了者数: {analysis['total_evacuated']}")
            print(f"  平均標高: {analysis['average_elevation']:.2f}m")
            print(f"  標高分布: {analysis['elevation_distribution']}")
            print()

    return results

def create_comparison_table(results):
    """条件間比較テーブルを作成"""
    # 標高カテゴリの順序を定義
    categories = ["10m未満", "10-15m", "15-20m", "20-25m", "25-30m", "30m以上", "不明"]

    table_data = []
    for condition_name, data in results.items():
        row = {
            '条件': condition_name,
            '避難完了者数': data['total_evacuated'],
            '平均標高(m)': f"{data['average_elevation']:.2f}"
        }

        # 各標高カテゴリの割合を追加
        for cat in categories:
            percentage = data['elevation_distribution'].get(cat, 0.0)
            row[cat] = f"{percentage:.1f}%"

        table_data.append(row)

    return pd.DataFrame(table_data)

def analyze_shelter_diversity(condition, num_trials=5):
    """避難所選択の多様性（分散度）を分析"""
    all_shelters = []

    for trial in range(1, num_trials + 1):
        df = load_agent_data(condition, trial)
        if df is None:
            continue

        evacuated = df[df['evacuation_completed'] == True]
        all_shelters.extend(evacuated['final_shelter'].tolist())

    if not all_shelters:
        return None

    # ユニークな避難所数
    unique_shelters = len(set(all_shelters))

    # エントロピー計算（選択の分散度）
    shelter_counts = pd.Series(all_shelters).value_counts()
    proportions = shelter_counts / len(all_shelters)
    entropy = -np.sum(proportions * np.log2(proportions))

    # 最大エントロピー（全て均等に分散した場合）
    max_entropy = np.log2(unique_shelters) if unique_shelters > 0 else 0

    # 正規化エントロピー（0-1、1が最も分散）
    normalized_entropy = entropy / max_entropy if max_entropy > 0 else 0

    return {
        'unique_shelters': unique_shelters,
        'entropy': entropy,
        'max_entropy': max_entropy,
        'normalized_entropy': normalized_entropy
    }

def main():
    print("=" * 60)
    print("避難所選択分析（実験3）")
    print("=" * 60)
    print()

    # 全条件の分析
    results = analyze_all_conditions()

    # 比較テーブル作成
    print("\n【標高別避難所選択の比較】")
    comparison_table = create_comparison_table(results)
    print(comparison_table.to_string(index=False))
    print()

    # 避難所選択の多様性分析
    print("\n【避難所選択の多様性】")
    diversity_results = {}
    for condition_code, condition_name in EXP3_CONDITIONS.items():
        diversity = analyze_shelter_diversity(condition_code)
        if diversity:
            diversity_results[condition_name] = diversity
            print(f"{condition_name}:")
            print(f"  ユニークな避難所数: {diversity['unique_shelters']}")
            print(f"  選択の分散度(正規化エントロピー): {diversity['normalized_entropy']:.3f}")
            print()

    # 結果をJSONで保存
    output = {
        'shelter_selection_by_elevation': results,
        'shelter_diversity': diversity_results,
        'comparison_table': comparison_table.to_dict('records')
    }

    output_file = LOG_DIR / "shelter_selection_analysis.json"
    with open(output_file, 'w', encoding='utf-8') as f:
        json.dump(output, f, ensure_ascii=False, indent=2)

    print(f"\n結果を保存しました: {output_file}")

    # LaTeX形式のテーブルも生成
    print("\n【LaTeX形式テーブル】")
    print("\\begin{table}[htbp]")
    print("\\centering")
    print("\\caption{情報条件別の避難所標高分布}")
    print("\\label{tab:shelter-elevation-distribution}")
    print("\\small")
    print("\\begin{tabular}{|l|c|c|c|c|c|c|c|}")
    print("\\hline")
    print("\\textbf{条件} & \\textbf{10m未満} & \\textbf{10-15m} & \\textbf{15-20m} & \\textbf{20-25m} & \\textbf{25-30m} & \\textbf{30m以上} & \\textbf{平均} \\\\ \\hline")

    for condition_name in ["条件A（標準）", "条件B（切迫感高）", "条件C（具体性高）", "条件D（両方高）"]:
        if condition_name in results:
            data = results[condition_name]
            dist = data['elevation_distribution']
            avg = data['average_elevation']

            row = f"{condition_name.split('（')[0]}"
            for cat in ["10m未満", "10-15m", "15-20m", "20-25m", "25-30m", "30m以上"]:
                percentage = dist.get(cat, 0.0)
                row += f" & {percentage:.1f}\\%"
            row += f" & {avg:.1f}m \\\\ \\hline"
            print(row)

    print("\\end{tabular}")
    print("\\end{table}")

if __name__ == "__main__":
    main()
