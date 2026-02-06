"""
長期記憶管理モジュール（RAG）

numpy + JSONベースのベクトル検索を実装。
地域知識、ペルソナ情報、災害経験などの長期記憶を管理する。
"""

import json
import os
from pathlib import Path
from typing import List, Dict, Any, Optional

import numpy as np
from openai import AsyncOpenAI

from logger import get_logger

logger = get_logger(__name__)

# デフォルトの設定
EMBEDDING_MODEL = "text-embedding-3-small"
EMBEDDING_DIMENSIONS = 1536
SIMILARITY_THRESHOLD = 0.75
DEFAULT_TOP_K = 3


class MemoryManager:
    """
    長期記憶の管理クラス

    - JSONファイルから記憶データを読み込み
    - OpenAI Embedding APIでベクトル化
    - コサイン類似度で検索
    """

    def __init__(self, openai_client: Optional[AsyncOpenAI] = None):
        """
        初期化

        Args:
            openai_client: OpenAI APIクライアント（Noneの場合は検索不可）
        """
        self.openai = openai_client
        self.memories: List[Dict[str, Any]] = []
        self.embeddings: Optional[np.ndarray] = None
        self.is_initialized = False
        self._embedding_cache: Dict[str, np.ndarray] = {}

    async def load_memories(self, json_path: str) -> bool:
        """
        JSONファイルから記憶を読み込み、ベクトル化

        Args:
            json_path: JSONファイルのパス

        Returns:
            成功した場合True
        """
        if not os.path.exists(json_path):
            logger.warning(f"記憶ファイルが見つかりません: {json_path}")
            return False

        try:
            with open(json_path, 'r', encoding='utf-8') as f:
                self.memories = json.load(f)

            if not self.memories:
                logger.warning("記憶データが空です")
                return False

            logger.info(f"{len(self.memories)}件の記憶を読み込みました")

            # ベクトル化
            if self.openai:
                await self._vectorize_memories()
                self.is_initialized = True
                logger.info(f"ベクトル化完了（{self.embeddings.shape if self.embeddings is not None else 'N/A'}）")
            else:
                logger.warning("OpenAIクライアントがないため、ベクトル化をスキップ")
                self.is_initialized = False

            return True

        except json.JSONDecodeError as e:
            logger.error(f"JSONパースエラー: {e}")
            return False
        except Exception as e:
            logger.error(f"記憶読み込みエラー: {e}")
            return False

    async def _vectorize_memories(self):
        """全記憶をベクトル化"""
        if not self.openai or not self.memories:
            return

        texts = [m.get('content', '') for m in self.memories]

        try:
            # バッチでベクトル化（OpenAI APIは最大2048トークンまで1リクエストで処理可能）
            response = await self.openai.embeddings.create(
                model=EMBEDDING_MODEL,
                input=texts
            )
            self.embeddings = np.array([d.embedding for d in response.data])
        except Exception as e:
            logger.error(f"ベクトル化エラー: {e}")
            self.embeddings = None

    async def _get_embedding(self, text: str) -> Optional[np.ndarray]:
        """
        テキストの埋め込みベクトルを取得（キャッシュ付き）

        Args:
            text: ベクトル化するテキスト

        Returns:
            埋め込みベクトル（1536次元）
        """
        if not self.openai:
            return None

        # キャッシュ確認
        if text in self._embedding_cache:
            return self._embedding_cache[text]

        try:
            response = await self.openai.embeddings.create(
                model=EMBEDDING_MODEL,
                input=[text]
            )
            embedding = np.array(response.data[0].embedding)
            self._embedding_cache[text] = embedding
            return embedding
        except Exception as e:
            logger.error(f"埋め込み取得エラー: {e}")
            return None

    async def search(
        self,
        query: str,
        agent_id: Optional[int] = None,
        memory_types: Optional[List[str]] = None,
        top_k: int = DEFAULT_TOP_K,
        threshold: float = SIMILARITY_THRESHOLD
    ) -> List[Dict[str, Any]]:
        """
        関連する記憶を検索

        Args:
            query: 検索クエリ
            agent_id: エージェントID（指定時はそのエージェントの記憶 + 共通記憶を検索）
            memory_types: フィルタする記憶タイプ（例: ["regional_knowledge", "disaster_experience"]）
            top_k: 返す最大件数
            threshold: 類似度閾値（0.0-1.0）

        Returns:
            関連する記憶のリスト（類似度順）
        """
        if not self.is_initialized or self.embeddings is None:
            return []

        # クエリをベクトル化
        query_vec = await self._get_embedding(query)
        if query_vec is None:
            return []

        # コサイン類似度を計算
        # 注: embeddings は正規化されていると仮定（OpenAI Embeddingは正規化済み）
        similarities = np.dot(self.embeddings, query_vec)

        # フィルタリングと結果収集
        results = []
        for i, sim in enumerate(similarities):
            mem = self.memories[i]

            # agent_idでフィルタ（共通知識 or 特定エージェント）
            mem_agent_id = mem.get('agent_id')
            if agent_id is not None:
                # agent_idがNone（共通知識）または一致するもののみ
                if mem_agent_id is not None and mem_agent_id != agent_id:
                    continue

            # memory_typeでフィルタ
            if memory_types:
                if mem.get('memory_type') not in memory_types:
                    continue

            # 閾値チェック
            if sim >= threshold:
                results.append({
                    'content': mem.get('content', ''),
                    'memory_type': mem.get('memory_type', 'unknown'),
                    'agent_id': mem_agent_id,
                    'similarity': float(sim),
                    'metadata': mem.get('metadata', {})
                })

        # 類似度でソートして上位k件を返す
        results.sort(key=lambda x: x['similarity'], reverse=True)
        return results[:top_k]

    async def search_for_situation(
        self,
        current_situation: str,
        agent_id: Optional[int] = None,
        top_k: int = DEFAULT_TOP_K
    ) -> List[Dict[str, Any]]:
        """
        現在の状況に関連する記憶を検索（簡易版）

        Args:
            current_situation: 現在の状況説明
            agent_id: エージェントID
            top_k: 返す最大件数

        Returns:
            関連する記憶のリスト
        """
        return await self.search(
            query=current_situation,
            agent_id=agent_id,
            top_k=top_k
        )

    def get_statistics(self) -> Dict[str, Any]:
        """記憶の統計情報を取得"""
        if not self.memories:
            return {"total": 0}

        stats = {
            "total": len(self.memories),
            "by_type": {},
            "shared": 0,
            "agent_specific": 0
        }

        for mem in self.memories:
            mem_type = mem.get('memory_type', 'unknown')
            stats["by_type"][mem_type] = stats["by_type"].get(mem_type, 0) + 1

            if mem.get('agent_id') is None:
                stats["shared"] += 1
            else:
                stats["agent_specific"] += 1

        return stats


# グローバルインスタンス
_memory_manager: Optional[MemoryManager] = None


async def initialize_memory_manager(
    openai_client: Optional[AsyncOpenAI],
    memories_path: Optional[str] = None
) -> Optional[MemoryManager]:
    """
    メモリマネージャーを初期化

    Args:
        openai_client: OpenAI APIクライアント
        memories_path: 記憶JSONファイルのパス（Noneの場合はデフォルトパスを使用）

    Returns:
        初期化されたMemoryManager
    """
    global _memory_manager

    if memories_path is None:
        # デフォルトパス
        base_dir = Path(__file__).parent
        memories_path = str(base_dir / "data" / "memories.json")

        # memories.jsonがなければmemories_sample.jsonを使用
        if not os.path.exists(memories_path):
            sample_path = str(base_dir / "data" / "memories_sample.json")
            if os.path.exists(sample_path):
                memories_path = sample_path
                logger.info(f"サンプルファイルを使用: {memories_path}")

    _memory_manager = MemoryManager(openai_client)

    if os.path.exists(memories_path):
        success = await _memory_manager.load_memories(memories_path)
        if success:
            stats = _memory_manager.get_statistics()
            logger.info(f"初期化完了 - 統計: {stats}")
            return _memory_manager
        else:
            logger.warning("記憶の読み込みに失敗しました")
    else:
        logger.warning(f"記憶ファイルが見つかりません: {memories_path}")
        logger.warning("RAG機能は無効化されます")

    return _memory_manager


def get_memory_manager() -> Optional[MemoryManager]:
    """グローバルなMemoryManagerインスタンスを取得"""
    return _memory_manager
