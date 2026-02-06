"""
分析スクリプト共通ユーティリティ

重複していたデータ読み込み関数を統合し、分析スクリプト間で共有する。
"""
from __future__ import annotations

from pathlib import Path
from typing import Optional, List, Dict, Any
import pandas as pd
import json

# グローバル変数: 実験結果ディレクトリ
_result_dir: Optional[Path] = None


def set_result_dir(path: Path) -> None:
    """
    実験結果ディレクトリを設定する

    Args:
        path: 実験結果ディレクトリのパス
    """
    global _result_dir
    _result_dir = path


def get_result_dir() -> Path:
    """
    実験結果ディレクトリを取得する

    Returns:
        実験結果ディレクトリのパス

    Raises:
        ValueError: ディレクトリが設定されていない場合
    """
    if _result_dir is None:
        raise ValueError(
            "RESULT_DIR が設定されていません。"
            "set_result_dir() または --log-dir コマンドライン引数でログディレクトリを指定してください。"
        )
    return _result_dir


def load_agent_data(condition: str, trial: int) -> Optional[pd.DataFrame]:
    """
    エージェントデータを読み込む

    Args:
        condition: 実験条件名（例: "LLM", "RuleBased"）
        trial: 試行番号

    Returns:
        エージェントデータのDataFrame、ファイルが存在しない場合はNone
    """
    filepath = get_result_dir() / f"{condition}_trial_{trial}_agents.csv"
    if filepath.exists():
        return pd.read_csv(filepath)
    return None


def load_action_data(condition: str, trial: int) -> Optional[pd.DataFrame]:
    """
    行動データを読み込む

    Args:
        condition: 実験条件名
        trial: 試行番号

    Returns:
        行動データのDataFrame、ファイルが存在しない場合はNone
    """
    filepath = get_result_dir() / f"{condition}_trial_{trial}_actions.csv"
    if filepath.exists():
        return pd.read_csv(filepath)
    return None


def load_summary_data(condition: str, trial: int) -> Optional[pd.DataFrame]:
    """
    サマリーデータを読み込む

    Args:
        condition: 実験条件名
        trial: 試行番号

    Returns:
        サマリーデータのDataFrame、ファイルが存在しない場合はNone
    """
    filepath = get_result_dir() / f"{condition}_trial_{trial}_summary.csv"
    if filepath.exists():
        return pd.read_csv(filepath)
    return None


def load_llm_decisions(agent_id: int) -> List[Dict[str, Any]]:
    """
    LLM決定ログを読み込む

    Args:
        agent_id: エージェントID

    Returns:
        LLM決定のリスト（JSONブロックのパース結果）
    """
    filepath = get_result_dir() / "llm_decisions" / f"{agent_id}.txt"
    if not filepath.exists():
        return []

    decisions = []

    with open(filepath, 'r', encoding='utf-8') as f:
        content = f.read()

    # JSONブロックを抽出（================で区切られている）
    blocks = content.split("=" * 80)

    for block in blocks:
        block = block.strip()
        if not block:
            continue

        # JSONセクションを探す
        json_start = block.find("{")
        json_end = block.rfind("}") + 1

        if json_start != -1 and json_end > json_start:
            try:
                json_str = block[json_start:json_end]
                decision = json.loads(json_str)
                decisions.append(decision)
            except json.JSONDecodeError:
                continue

    return decisions


def get_conditions() -> List[str]:
    """
    利用可能な実験条件のリストを取得

    Returns:
        条件名のリスト
    """
    result_dir = get_result_dir()
    conditions = set()

    for filepath in result_dir.glob("*_trial_*_agents.csv"):
        # ファイル名から条件名を抽出
        name = filepath.stem
        parts = name.rsplit("_trial_", 1)
        if len(parts) == 2:
            conditions.add(parts[0])

    return sorted(list(conditions))


def get_trial_count(condition: str) -> int:
    """
    指定条件の試行回数を取得

    Args:
        condition: 実験条件名

    Returns:
        試行回数
    """
    result_dir = get_result_dir()
    trials = []

    for filepath in result_dir.glob(f"{condition}_trial_*_agents.csv"):
        name = filepath.stem
        parts = name.rsplit("_trial_", 1)
        if len(parts) == 2:
            try:
                trial_part = parts[1].replace("_agents", "")
                trials.append(int(trial_part))
            except ValueError:
                continue

    return max(trials) + 1 if trials else 0
