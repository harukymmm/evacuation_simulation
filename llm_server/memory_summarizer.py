"""
短期記憶（行動履歴）管理モジュール

LLM APIを使用せず、直近の行動履歴をそのまま保持する軽量版。
レート制限を回避しつつ、必要な情報をプロンプトに渡す。
"""

from typing import List, Dict, Any, Optional


# 保持する履歴の最大件数
MAX_RECENT_ACTIONS = 3          # 直近の行動履歴
MAX_RECENT_CONVERSATIONS = 5    # 直近の会話履歴
MAX_RECENT_CONTACTS = 5         # 直近の連絡履歴


def get_recent_actions(
    action_history: List[Dict[str, Any]],
    max_count: int = MAX_RECENT_ACTIONS
) -> List[Dict[str, Any]]:
    """
    直近の行動履歴を取得する

    Args:
        action_history: 全行動履歴リスト
        max_count: 取得する最大件数

    Returns:
        直近の行動履歴（最大max_count件）
    """
    if not action_history:
        return []

    # 直近のmax_count件を返す
    return action_history[-max_count:]


def get_recent_conversations(
    conversation_history: List[Dict[str, Any]],
    max_count: int = MAX_RECENT_CONVERSATIONS
) -> List[Dict[str, Any]]:
    """
    直近の会話履歴を取得する

    Args:
        conversation_history: 全会話履歴リスト
        max_count: 取得する最大件数

    Returns:
        直近の会話履歴（最大max_count件）
    """
    if not conversation_history:
        return []

    return conversation_history[-max_count:]


def get_recent_contacts(
    contact_history: List[Dict[str, Any]],
    max_count: int = MAX_RECENT_CONTACTS
) -> List[Dict[str, Any]]:
    """
    直近の家族連絡履歴を取得する

    Args:
        contact_history: 全連絡履歴リスト
        max_count: 取得する最大件数

    Returns:
        直近の連絡履歴（最大max_count件）
    """
    if not contact_history:
        return []

    return contact_history[-max_count:]


def should_summarize(action_history_payload: Dict[str, Any]) -> bool:
    """
    行動履歴を要約すべきかどうかを判定

    注意: この関数は互換性のために残していますが、
    新しい実装ではLLMによる要約を行わないため常にFalseを返します。

    Args:
        action_history_payload: ActionHistoryPayloadの辞書表現

    Returns:
        常にFalse（LLM要約は無効化）
    """
    return False


# 以下は後方互換性のために残す（使用しない）
async def summarize_action_history(
    openai_client,
    model: str,
    action_history: List[Dict[str, Any]],
    existing_summary: Optional[str] = None
) -> Optional[str]:
    """
    過去の行動履歴を要約する（後方互換性のためのスタブ）

    注意: この関数は互換性のために残していますが、
    新しい実装ではLLMによる要約を行わないためNoneを返します。

    Args:
        openai_client: OpenAI APIクライアント（使用しない）
        model: 使用するモデル名（使用しない）
        action_history: 要約対象の行動履歴リスト（使用しない）
        existing_summary: 既存の要約（使用しない）

    Returns:
        常にNone（LLM要約は無効化）
    """
    return None
