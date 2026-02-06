#!/usr/bin/env python3
"""
社会的行動（TALK/CONTACT）の影響分析スクリプト
会話後の行動変化、返答内容と次の行動の関係、目標変化を分析

分析内容:
1. 会話履歴のパースと構造化
2. TALK後の行動転換率分析
3. 会話返答カテゴリの分類と次の行動との関係
4. TALK前後の長期目標・中期計画の変化分析
5. CONTACT後の行動変化分析
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

from utils import set_result_dir, get_result_dir, load_agent_data

# 会話返答のカテゴリ定義（キーワードベース）- 拡充版
RESPONSE_CATEGORIES = {
    "避難促進": [
        # 基本的な避難表現
        "逃げ", "避難", "高台", "津波", "危険", "急い", "早く", "走", "すぐ",
        # 場所への言及
        "中田山", "望洋荘", "八坂神社", "山", "丘", "高い", "上",
        # 緊急性の表現
        "今すぐ", "急がないと", "間に合わない", "来る", "近づいて",
        # 勧誘表現
        "行こう", "逃げよう", "向かおう", "一緒に",
        # 危機意識
        "やばい", "まずい", "ダメ", "危ない", "怖い"
    ],
    "情報提供": [
        # 地名・場所
        "中田山", "望洋荘", "八坂神社", "避難所", "学校", "公民館",
        # 警報関連
        "警報", "サイレン", "放送", "ニュース", "ラジオ", "テレビ",
        # 地理情報
        "海抜", "メートル", "標高", "距離", "近く", "遠く",
        # 状況説明
        "聞いた", "見た", "らしい", "そうだ", "みたい"
    ],
    "消極的": [
        # 拒否・断り
        "急いでいる", "すみません", "ごめん", "今は", "ちょっと",
        # 不明・無知
        "わからない", "知らない", "わかりません", "知りません",
        # 無関心
        "自分で", "一人で", "勝手に", "関係ない"
    ],
    "様子見": [
        # 楽観
        "大丈夫", "平気", "問題ない", "心配ない",
        # 待機
        "まだ", "様子", "落ち着", "待", "確認",
        # 過去経験への言及
        "前も", "いつも", "経験", "慣れて"
    ],
    "家族関連": [
        # 家族
        "家族", "親", "子供", "妻", "夫", "息子", "娘", "母", "父",
        # 連絡
        "連絡", "電話", "メール", "LINE",
        # 感情
        "心配", "探し", "会い", "合流", "迎え", "待ち合わせ"
    ],
    "同調": [
        # 一緒に行動
        "一緒に", "ついて", "みんな", "周り", "他の人",
        # 行動への同意
        "そうしよう", "わかった", "行く", "従う"
    ]
}

# 長期目標のカテゴリ定義
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


def parse_conversation_history(user_prompt: str) -> List[Dict]:
    """
    ユーザープロンプトから会話履歴をパースする

    フォーマット:
    【直近の会話履歴】
    これまでにX回の会話を行っています。直近5件:

    会話相手: [相手名]（話題: [トピック]）
    自分: [自分のメッセージ]
    [相手名]: [相手の返答]
    """
    conversations = []

    # 会話履歴セクションを探す
    history_match = re.search(r'【直近の会話履歴】(.*?)(?=【|$)', user_prompt, re.DOTALL)
    if not history_match:
        return conversations

    history_text = history_match.group(1)

    # 各会話をパース
    # パターン: 会話相手: XXX（話題: YYY）
    conversation_pattern = r'会話相手:\s*([^（]+)（話題:\s*([^）]+)）\s*自分:\s*(.+?)\s*\1:\s*(.+?)(?=\n\n|会話相手:|$)'

    matches = re.finditer(conversation_pattern, history_text, re.DOTALL)
    for match in matches:
        partner_name = match.group(1).strip()
        topic = match.group(2).strip()
        my_message = match.group(3).strip()
        partner_response = match.group(4).strip()

        conversations.append({
            "partner_name": partner_name,
            "topic": topic,
            "my_message": my_message,
            "partner_response": partner_response
        })

    return conversations


def categorize_response(response_text: str) -> Tuple[str, List[str]]:
    """
    会話返答をカテゴリに分類
    戻り値: (主カテゴリ, マッチしたキーワードリスト)
    """
    if not response_text:
        return "不明", []

    # 各カテゴリのキーワードをチェック
    category_scores = {}
    matched_keywords = {}

    for category, keywords in RESPONSE_CATEGORIES.items():
        matches = [kw for kw in keywords if kw in response_text]
        category_scores[category] = len(matches)
        matched_keywords[category] = matches

    # 最もスコアの高いカテゴリを返す
    if max(category_scores.values()) > 0:
        best_category = max(category_scores, key=category_scores.get)
        return best_category, matched_keywords[best_category]

    return "その他", []


def categorize_goal(goal_text: str) -> str:
    """長期目標をカテゴリに分類"""
    if not goal_text:
        return "不明"

    category_scores = {}
    for category, keywords in GOAL_CATEGORIES.items():
        score = sum(1 for kw in keywords if kw in goal_text)
        category_scores[category] = score

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


def load_llm_decisions_with_prompts(agent_id: int) -> List[Dict]:
    """
    LLM決定ログを読み込み、プロンプト情報も含める
    会話履歴をパースするために、USER PROMPTセクションも抽出する
    """
    filepath = get_result_dir() / "llm_decisions" / f"{agent_id}.txt"
    if not filepath.exists():
        return []

    with open(filepath, 'r', encoding='utf-8') as f:
        content = f.read()

    decisions = []

    # ================で区切られたブロックを抽出
    blocks = content.split("=" * 80)

    i = 0
    while i < len(blocks):
        block = blocks[i].strip()
        if not block:
            i += 1
            continue

        # JSONブロックを探す
        if block.startswith("{"):
            try:
                # JSONの終わりを探す（\n\nまたはブロック終端）
                json_end = block.find("\n\n")
                if json_end == -1:
                    json_end = len(block)
                json_str = block[:json_end].strip()

                # 不完全なJSONを修正
                open_braces = json_str.count('{')
                close_braces = json_str.count('}')
                json_str += '}' * (open_braces - close_braces)

                data = json.loads(json_str)

                # 同じブロック内のプロンプト情報を抽出
                user_prompt = ""
                prompt_match = re.search(r'\[USER PROMPT\].*?-+\n(.*?)(?=={80}|$)', block, re.DOTALL)
                if prompt_match:
                    user_prompt = prompt_match.group(1).strip()

                # 次のブロックにプロンプトがある場合
                if i + 1 < len(blocks) and "[SYSTEM PROMPT]" in blocks[i + 1]:
                    prompt_block = blocks[i + 1]
                    user_prompt_match = re.search(r'\[USER PROMPT\].*?-+\n(.*?)$', prompt_block, re.DOTALL)
                    if user_prompt_match:
                        user_prompt = user_prompt_match.group(1).strip()

                # 会話履歴をパース
                conversations = parse_conversation_history(user_prompt)

                data["_user_prompt"] = user_prompt
                data["_conversations"] = conversations
                data["_conversation_count"] = len(conversations)

                decisions.append(data)

            except json.JSONDecodeError:
                pass

        i += 1

    return decisions


def analyze_talk_transitions(decisions: List[Dict]) -> Dict:
    """
    TALK行動後の行動転換を分析
    """
    results = {
        "total_talk_actions": 0,
        "talk_to_next_action": defaultdict(int),
        "talk_response_to_next_action": defaultdict(lambda: defaultdict(int)),
        "response_category_details": defaultdict(list),
        "goal_changes_after_talk": [],
        "example_conversations": []
    }

    for i, decision in enumerate(decisions):
        output = decision.get("output", {})
        action_type = output.get("action_type", "")

        if action_type == "TALK":
            results["total_talk_actions"] += 1

            # 次の意思決定を確認
            if i + 1 < len(decisions):
                next_decision = decisions[i + 1]
                next_output = next_decision.get("output", {})
                next_action = next_output.get("action_type", "")

                results["talk_to_next_action"][next_action] += 1

                # 次の意思決定時点での会話履歴を確認
                next_conversations = next_decision.get("_conversations", [])
                if next_conversations:
                    # 最新の会話の返答カテゴリを取得
                    latest_conv = next_conversations[-1]
                    response_text = latest_conv.get("partner_response", "")
                    response_category, matched_keywords = categorize_response(response_text)

                    results["talk_response_to_next_action"][response_category][next_action] += 1

                    # 詳細情報を保存
                    results["response_category_details"][response_category].append({
                        "response_text": response_text,
                        "matched_keywords": matched_keywords,
                        "next_action": next_action
                    })

                    # 目標変化の確認
                    current_goal = extract_goal_from_output(output)
                    next_goal = extract_goal_from_output(next_output)
                    current_goal_cat = categorize_goal(current_goal)
                    next_goal_cat = categorize_goal(next_goal)

                    if current_goal_cat != next_goal_cat:
                        results["goal_changes_after_talk"].append({
                            "timestamp": decision.get("episode_elapsed_time", 0),
                            "before_goal": current_goal,
                            "before_goal_category": current_goal_cat,
                            "after_goal": next_goal,
                            "after_goal_category": next_goal_cat,
                            "response_category": response_category,
                            "matched_keywords": matched_keywords,
                            "next_action": next_action,
                            "conversation": latest_conv
                        })

                    # 例として保存（最大10件）
                    if len(results["example_conversations"]) < 10:
                        results["example_conversations"].append({
                            "timestamp": decision.get("episode_elapsed_time", 0),
                            "conversation": latest_conv,
                            "response_category": response_category,
                            "matched_keywords": matched_keywords,
                            "next_action": next_action,
                            "goal_changed": current_goal_cat != next_goal_cat
                        })

    return results


def analyze_contact_transitions(decisions: List[Dict]) -> Dict:
    """
    CONTACT行動後の行動転換を分析
    """
    results = {
        "total_contact_actions": 0,
        "contact_to_next_action": defaultdict(int),
        "goal_changes_after_contact": []
    }

    for i, decision in enumerate(decisions):
        output = decision.get("output", {})
        action_type = output.get("action_type", "")

        if action_type == "CONTACT":
            results["total_contact_actions"] += 1

            if i + 1 < len(decisions):
                next_decision = decisions[i + 1]
                next_output = next_decision.get("output", {})
                next_action = next_output.get("action_type", "")

                results["contact_to_next_action"][next_action] += 1

                # 目標変化の確認
                current_goal = extract_goal_from_output(output)
                next_goal = extract_goal_from_output(next_output)
                current_goal_cat = categorize_goal(current_goal)
                next_goal_cat = categorize_goal(next_goal)

                if current_goal_cat != next_goal_cat:
                    results["goal_changes_after_contact"].append({
                        "timestamp": decision.get("episode_elapsed_time", 0),
                        "before_goal": current_goal,
                        "before_goal_category": current_goal_cat,
                        "after_goal": next_goal,
                        "after_goal_category": next_goal_cat,
                        "next_action": next_action
                    })

    return results


def analyze_all_agents():
    """全エージェントの社会的行動を分析"""

    # Baseline-LLM条件のエージェントを分析
    agents_df = load_agent_data("Baseline-LLM", 1)
    if agents_df is None:
        print("エージェントデータが見つかりません")
        return None

    # 全体の集計結果
    aggregate_results = {
        "total_agents_analyzed": 0,
        "agents_with_talk": 0,
        "agents_with_contact": 0,
        "total_talk_actions": 0,
        "total_contact_actions": 0,
        "talk_to_next_action": defaultdict(int),
        "talk_response_to_next_action": defaultdict(lambda: defaultdict(int)),
        "response_category_details": defaultdict(list),
        "contact_to_next_action": defaultdict(int),
        "goal_changes_after_talk": [],
        "goal_changes_after_contact": [],
        "example_conversations": [],
        "by_survival_status": {
            "survived": {
                "talk_to_next_action": defaultdict(int),
                "response_to_next_action": defaultdict(lambda: defaultdict(int))
            },
            "died": {
                "talk_to_next_action": defaultdict(int),
                "response_to_next_action": defaultdict(lambda: defaultdict(int))
            }
        }
    }

    # 各エージェントを分析
    llm_decisions_dir = get_result_dir() / "llm_decisions"
    if not llm_decisions_dir.exists():
        print("LLM決定ログディレクトリが見つかりません")
        return None

    agent_files = list(llm_decisions_dir.glob("*.txt"))
    print(f"分析対象: {len(agent_files)}エージェント")

    for agent_file in agent_files:
        # 数値でないファイル名はスキップ
        try:
            agent_id = int(agent_file.stem)
        except ValueError:
            continue

        # エージェントの生存状態を取得
        agent_row = agents_df[agents_df['agent_id'] == agent_id]
        survived = agent_row['survived'].values[0] if len(agent_row) > 0 else None
        survival_key = "survived" if survived else "died"

        # 決定ログを読み込み
        decisions = load_llm_decisions_with_prompts(agent_id)
        if not decisions:
            continue

        aggregate_results["total_agents_analyzed"] += 1

        # TALK分析
        talk_results = analyze_talk_transitions(decisions)
        if talk_results["total_talk_actions"] > 0:
            aggregate_results["agents_with_talk"] += 1
            aggregate_results["total_talk_actions"] += talk_results["total_talk_actions"]

            for action, count in talk_results["talk_to_next_action"].items():
                aggregate_results["talk_to_next_action"][action] += count
                aggregate_results["by_survival_status"][survival_key]["talk_to_next_action"][action] += count

            for resp_cat, actions in talk_results["talk_response_to_next_action"].items():
                for action, count in actions.items():
                    aggregate_results["talk_response_to_next_action"][resp_cat][action] += count
                    aggregate_results["by_survival_status"][survival_key]["response_to_next_action"][resp_cat][action] += count

            # 返答カテゴリ詳細
            for resp_cat, details in talk_results["response_category_details"].items():
                aggregate_results["response_category_details"][resp_cat].extend(details)

            aggregate_results["goal_changes_after_talk"].extend(talk_results["goal_changes_after_talk"])

            # 例として追加（最大20件）
            for example in talk_results["example_conversations"]:
                if len(aggregate_results["example_conversations"]) < 20:
                    example["agent_id"] = agent_id
                    example["survived"] = survived
                    aggregate_results["example_conversations"].append(example)

        # CONTACT分析
        contact_results = analyze_contact_transitions(decisions)
        if contact_results["total_contact_actions"] > 0:
            aggregate_results["agents_with_contact"] += 1
            aggregate_results["total_contact_actions"] += contact_results["total_contact_actions"]

            for action, count in contact_results["contact_to_next_action"].items():
                aggregate_results["contact_to_next_action"][action] += count

            aggregate_results["goal_changes_after_contact"].extend(contact_results["goal_changes_after_contact"])

    return aggregate_results


def generate_report(results: Dict) -> str:
    """分析結果からマークダウンレポートを生成"""

    report = []
    report.append("# 社会的行動（TALK/CONTACT）の影響分析\n")
    report.append(f"分析日時: {pd.Timestamp.now().strftime('%Y-%m-%d %H:%M:%S')}\n")

    # 1. 基本統計
    report.append("## 1. 基本統計\n")
    report.append(f"- 分析エージェント数: {results['total_agents_analyzed']}")
    report.append(f"- TALK行動を行ったエージェント: {results['agents_with_talk']} ({results['agents_with_talk']/results['total_agents_analyzed']*100:.1f}%)")
    report.append(f"- CONTACT行動を行ったエージェント: {results['agents_with_contact']} ({results['agents_with_contact']/results['total_agents_analyzed']*100:.1f}%)")
    report.append(f"- TALK行動総数: {results['total_talk_actions']}")
    report.append(f"- CONTACT行動総数: {results['total_contact_actions']}\n")

    # 2. TALK後の行動転換
    report.append("## 2. TALK後の行動転換\n")
    report.append("### 2.1 TALK直後の次の行動分布\n")
    report.append("| 次の行動 | 件数 | 割合 |")
    report.append("|---------|------|------|")

    total_talk = sum(results["talk_to_next_action"].values())
    for action, count in sorted(results["talk_to_next_action"].items(), key=lambda x: -x[1]):
        pct = count / total_talk * 100 if total_talk > 0 else 0
        report.append(f"| {action} | {count} | {pct:.1f}% |")
    report.append("")

    # 3. 会話返答カテゴリと次の行動の関係
    report.append("### 2.2 会話返答カテゴリと次の行動の関係\n")
    report.append("| 返答カテゴリ | EVACUATE | STAY | FOLLOW | TALK | その他 | 合計 |")
    report.append("|------------|----------|------|--------|------|--------|------|")

    for resp_cat in ["避難促進", "情報提供", "消極的", "様子見", "家族関連", "同調", "その他"]:
        if resp_cat in results["talk_response_to_next_action"]:
            actions = results["talk_response_to_next_action"][resp_cat]
            total = sum(actions.values())
            evacuate = actions.get("EVACUATE", 0)
            stay = actions.get("STAY", 0)
            follow = actions.get("FOLLOW", 0)
            talk = actions.get("TALK", 0)
            other = total - evacuate - stay - follow - talk
            report.append(f"| {resp_cat} | {evacuate} | {stay} | {follow} | {talk} | {other} | {total} |")
    report.append("")

    # 4. 避難行動への転換率
    report.append("### 2.3 会話返答カテゴリ別のEVACUATE転換率\n")
    report.append("| 返答カテゴリ | EVACUATE転換率 | 件数 |")
    report.append("|------------|---------------|------|")

    for resp_cat in ["避難促進", "情報提供", "消極的", "様子見", "家族関連", "同調", "その他"]:
        if resp_cat in results["talk_response_to_next_action"]:
            actions = results["talk_response_to_next_action"][resp_cat]
            total = sum(actions.values())
            evacuate = actions.get("EVACUATE", 0)
            rate = evacuate / total * 100 if total > 0 else 0
            report.append(f"| {resp_cat} | {rate:.1f}% | {total} |")
    report.append("")

    # 4.5 返答カテゴリの例
    report.append("### 2.4 各返答カテゴリの典型例\n")
    for resp_cat in ["避難促進", "情報提供", "消極的", "様子見", "家族関連", "同調"]:
        details = results.get("response_category_details", {}).get(resp_cat, [])
        if details:
            report.append(f"**{resp_cat}**:")
            shown = 0
            seen_responses = set()
            for detail in details:
                if detail["response_text"] not in seen_responses and shown < 3:
                    report.append(f"- 「{detail['response_text'][:50]}{'...' if len(detail['response_text']) > 50 else ''}」")
                    report.append(f"  - マッチキーワード: {', '.join(detail['matched_keywords'][:5])}")
                    seen_responses.add(detail["response_text"])
                    shown += 1
            report.append("")

    # 5. 生存者/非生存者別の分析
    report.append("## 3. 生存状態別の分析\n")
    report.append("### 3.1 生存者のTALK後行動分布\n")

    survived_actions = results["by_survival_status"]["survived"]["talk_to_next_action"]
    total_survived = sum(survived_actions.values())
    if total_survived > 0:
        report.append("| 次の行動 | 件数 | 割合 |")
        report.append("|---------|------|------|")
        for action, count in sorted(survived_actions.items(), key=lambda x: -x[1]):
            pct = count / total_survived * 100
            report.append(f"| {action} | {count} | {pct:.1f}% |")
    else:
        report.append("（データなし）")
    report.append("")

    report.append("### 3.2 非生存者のTALK後行動分布\n")

    died_actions = results["by_survival_status"]["died"]["talk_to_next_action"]
    total_died = sum(died_actions.values())
    if total_died > 0:
        report.append("| 次の行動 | 件数 | 割合 |")
        report.append("|---------|------|------|")
        for action, count in sorted(died_actions.items(), key=lambda x: -x[1]):
            pct = count / total_died * 100
            report.append(f"| {action} | {count} | {pct:.1f}% |")
    else:
        report.append("（データなし）")
    report.append("")

    # 6. 目標変化事例
    report.append("## 4. TALK後の目標変化事例\n")
    goal_changes = results["goal_changes_after_talk"]
    report.append(f"TALK後に長期目標が変化した件数: {len(goal_changes)}\n")

    if goal_changes:
        report.append("### 代表的な事例\n")
        for i, change in enumerate(goal_changes[:5]):
            report.append(f"#### 事例{i+1}")
            report.append(f"- 会話時刻: {change['timestamp']:.1f}秒")
            report.append(f"- 会話相手: {change['conversation'].get('partner_name', '不明')}")
            report.append(f"- 会話内容: 「{change['conversation'].get('my_message', '')}」")
            report.append(f"- 返答: 「{change['conversation'].get('partner_response', '')}」")
            report.append(f"- 返答カテゴリ: {change['response_category']} (キーワード: {', '.join(change.get('matched_keywords', [])[:3])})")
            report.append(f"- 目標変化: {change['before_goal_category']} → {change['after_goal_category']}")
            report.append(f"- 次の行動: {change['next_action']}")
            report.append("")

    # 7. 会話事例
    report.append("## 5. 会話と行動変化の事例\n")
    examples = results["example_conversations"]

    for i, example in enumerate(examples[:10]):
        conv = example["conversation"]
        report.append(f"### 事例{i+1} (エージェント{example['agent_id']}, {'生存' if example['survived'] else '死亡'})")
        report.append(f"- 会話時刻: {example['timestamp']:.1f}秒")
        report.append(f"- 会話相手: {conv.get('partner_name', '不明')}")
        report.append(f"- 話題: {conv.get('topic', '不明')}")
        report.append(f"- 自分: 「{conv.get('my_message', '')}」")
        report.append(f"- 相手: 「{conv.get('partner_response', '')}」")
        report.append(f"- 返答カテゴリ: {example['response_category']} (キーワード: {', '.join(example.get('matched_keywords', [])[:3])})")
        report.append(f"- 次の行動: {example['next_action']}")
        report.append(f"- 目標変化: {'あり' if example['goal_changed'] else 'なし'}")
        report.append("")

    # 8. CONTACT分析
    report.append("## 6. CONTACT後の行動転換\n")

    total_contact = sum(results["contact_to_next_action"].values())
    if total_contact > 0:
        report.append("| 次の行動 | 件数 | 割合 |")
        report.append("|---------|------|------|")
        for action, count in sorted(results["contact_to_next_action"].items(), key=lambda x: -x[1]):
            pct = count / total_contact * 100
            report.append(f"| {action} | {count} | {pct:.1f}% |")
    else:
        report.append("（CONTACTデータなし）")
    report.append("")

    # 9. 考察
    report.append("## 7. 考察\n")

    # EVACUATE転換率を計算
    evacuate_after_talk = results["talk_to_next_action"].get("EVACUATE", 0)
    evacuate_rate = evacuate_after_talk / total_talk * 100 if total_talk > 0 else 0

    report.append(f"### 7.1 TALK行動の避難促進効果\n")
    report.append(f"- TALK後にEVACUATEを選択した割合: {evacuate_rate:.1f}%")

    # 返答カテゴリ別の効果
    if "避難促進" in results["talk_response_to_next_action"]:
        promote_actions = results["talk_response_to_next_action"]["避難促進"]
        promote_total = sum(promote_actions.values())
        promote_evacuate = promote_actions.get("EVACUATE", 0)
        promote_rate = promote_evacuate / promote_total * 100 if promote_total > 0 else 0
        report.append(f"- 「避難促進」返答後のEVACUATE率: {promote_rate:.1f}%")

    if "消極的" in results["talk_response_to_next_action"]:
        passive_actions = results["talk_response_to_next_action"]["消極的"]
        passive_total = sum(passive_actions.values())
        passive_evacuate = passive_actions.get("EVACUATE", 0)
        passive_rate = passive_evacuate / passive_total * 100 if passive_total > 0 else 0
        report.append(f"- 「消極的」返答後のEVACUATE率: {passive_rate:.1f}%")

    if "情報提供" in results["talk_response_to_next_action"]:
        info_actions = results["talk_response_to_next_action"]["情報提供"]
        info_total = sum(info_actions.values())
        info_evacuate = info_actions.get("EVACUATE", 0)
        info_rate = info_evacuate / info_total * 100 if info_total > 0 else 0
        report.append(f"- 「情報提供」返答後のEVACUATE率: {info_rate:.1f}%")

    report.append("")
    report.append("### 7.2 生存者と非生存者の違い\n")

    # 生存者のEVACUATE転換率
    survived_evacuate = survived_actions.get("EVACUATE", 0)
    survived_rate = survived_evacuate / total_survived * 100 if total_survived > 0 else 0

    died_evacuate = died_actions.get("EVACUATE", 0)
    died_rate = died_evacuate / total_died * 100 if total_died > 0 else 0

    report.append(f"- 生存者のTALK後EVACUATE転換率: {survived_rate:.1f}%")
    report.append(f"- 非生存者のTALK後EVACUATE転換率: {died_rate:.1f}%")

    # 目標変化の分析
    report.append("")
    report.append("### 7.3 会話による目標変化\n")
    goal_changes = results["goal_changes_after_talk"]
    report.append(f"- TALK後に長期目標が変化した件数: {len(goal_changes)}")

    if goal_changes:
        # 変化パターンの集計
        change_patterns = defaultdict(int)
        for change in goal_changes:
            pattern = f"{change['before_goal_category']} → {change['after_goal_category']}"
            change_patterns[pattern] += 1

        report.append("\n目標変化パターン:")
        for pattern, count in sorted(change_patterns.items(), key=lambda x: -x[1])[:5]:
            report.append(f"- {pattern}: {count}件")

    return "\n".join(report)


def main():
    """メイン処理"""
    print("社会的行動分析を開始します...")

    # 分析実行
    results = analyze_all_agents()

    if results is None:
        print("分析に失敗しました")
        return

    # JSON出力
    output_json_path = get_result_dir() / "social_interaction_analysis.json"

    # defaultdictを通常のdictに変換し、numpy型をPython型に変換
    def convert_for_json(obj):
        if isinstance(obj, defaultdict):
            return {k: convert_for_json(v) for k, v in obj.items()}
        elif isinstance(obj, dict):
            return {k: convert_for_json(v) for k, v in obj.items()}
        elif isinstance(obj, list):
            return [convert_for_json(item) for item in obj]
        elif isinstance(obj, (np.bool_, np.integer)):
            return int(obj) if isinstance(obj, np.integer) else bool(obj)
        elif isinstance(obj, np.floating):
            return float(obj)
        elif isinstance(obj, np.ndarray):
            return obj.tolist()
        return obj

    results_for_json = convert_for_json(results)

    with open(output_json_path, 'w', encoding='utf-8') as f:
        json.dump(results_for_json, f, ensure_ascii=False, indent=2)
    print(f"JSON出力: {output_json_path}")

    # マークダウンレポート生成
    report = generate_report(results)
    output_md_path = get_result_dir() / "social_interaction_analysis.md"
    with open(output_md_path, 'w', encoding='utf-8') as f:
        f.write(report)
    print(f"レポート出力: {output_md_path}")

    # サマリー表示
    print("\n=== 分析サマリー ===")
    print(f"分析エージェント数: {results['total_agents_analyzed']}")
    print(f"TALK行動を行ったエージェント: {results['agents_with_talk']}")
    print(f"TALK行動総数: {results['total_talk_actions']}")

    total_talk = sum(results["talk_to_next_action"].values())
    if total_talk > 0:
        evacuate_after_talk = results["talk_to_next_action"].get("EVACUATE", 0)
        print(f"TALK後のEVACUATE転換率: {evacuate_after_talk/total_talk*100:.1f}%")

    print(f"目標変化事例数: {len(results['goal_changes_after_talk'])}")


if __name__ == "__main__":
    parser = argparse.ArgumentParser(description='社会的行動（TALK/CONTACT）の影響分析スクリプト')
    parser.add_argument('log_dir', type=str, help='分析対象のログディレクトリパス')
    args = parser.parse_args()

    result_dir = Path(args.log_dir)
    if not result_dir.exists():
        print(f"エラー: 指定されたディレクトリが存在しません: {result_dir}")
        sys.exit(1)

    set_result_dir(result_dir)
    main()
