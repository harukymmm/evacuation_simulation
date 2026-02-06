"""
ロギングユーティリティモジュール

print()ベースのログ出力を統一的なloggingモジュールベースに置き換えるための基盤。
"""
from __future__ import annotations

import logging
import sys
from pathlib import Path
from typing import Optional


def get_logger(name: str, level: int = logging.INFO) -> logging.Logger:
    """
    指定された名前のロガーを取得する。

    Args:
        name: ロガー名（通常は __name__ を使用）
        level: ログレベル（デフォルト: INFO）

    Returns:
        設定済みのLoggerインスタンス
    """
    logger = logging.getLogger(name)

    # 既にハンドラーが設定されている場合は再設定しない
    if not logger.handlers:
        handler = logging.StreamHandler(sys.stdout)
        handler.setFormatter(logging.Formatter(
            '%(asctime)s [%(name)s] %(levelname)s: %(message)s',
            datefmt='%Y-%m-%d %H:%M:%S'
        ))
        logger.addHandler(handler)
        logger.setLevel(level)
        # 親ロガーへの伝播を防ぐ
        logger.propagate = False

    return logger


def setup_file_logging(
    logger: logging.Logger,
    log_dir: Path,
    filename: str = "server.log"
) -> None:
    """
    ファイルへのログ出力を追加する。

    Args:
        logger: 対象のロガー
        log_dir: ログファイルを保存するディレクトリ
        filename: ログファイル名
    """
    log_dir.mkdir(parents=True, exist_ok=True)
    file_handler = logging.FileHandler(
        log_dir / filename,
        encoding='utf-8'
    )
    file_handler.setFormatter(logging.Formatter(
        '%(asctime)s [%(name)s] %(levelname)s: %(message)s',
        datefmt='%Y-%m-%d %H:%M:%S'
    ))
    logger.addHandler(file_handler)
