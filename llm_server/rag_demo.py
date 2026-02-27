"""
RAG記憶検索デモWebページ

避難シミュレーションの長期記憶検索（RAG）がどう機能するかを
ブラウザ上で試せるスタンドアロンのデモツール。

起動方法:
    cd llm_server
    python rag_demo.py
"""

import asyncio
import json
import os
import sys
from http.server import HTTPServer, BaseHTTPRequestHandler
from pathlib import Path

from dotenv import load_dotenv
from openai import AsyncOpenAI

from memory_manager import MemoryManager

# ── 定数 ──────────────────────────────────────────────

TYPE_LABELS = {
    "regional_knowledge": "地域知識",
    "persona_knowledge": "自己認識",
    "disaster_experience": "過去の経験",
    "drill_knowledge": "避難訓練の知識",
    "social_knowledge": "社会的知識",
}

DEFAULT_PORT = 8080
PROJECT_ROOT = Path(__file__).resolve().parents[1]
LOG_BASE_DIR = PROJECT_ROOT / "Logs" / "experiment_results"

# ── 非同期ブリッジ ─────────────────────────────────────

_loop = asyncio.new_event_loop()


def _run_async(coro):
    """asyncio コルーチンを同期的に実行する"""
    return _loop.run_until_complete(coro)


# ── グローバル参照 ─────────────────────────────────────

manager: MemoryManager = None
openai_model: str = "gpt-4o-mini"

# ── ヘルパー関数 ───────────────────────────────────────


def build_demo_prompt(memories: list) -> str:
    """検索結果を【あなたの記憶・知識】形式に変換する"""
    if not memories:
        return "(検索結果なし)"

    lines = ["【あなたの記憶・知識】", ""]
    for mem in memories:
        memory_type = mem.get("memory_type", "unknown")
        content = mem.get("content", "")
        label = TYPE_LABELS.get(memory_type, "記憶")
        lines.append(f"[{label}] {content}")
    return "\n".join(lines)


async def _call_llm(system_prompt: str, user_prompt: str) -> str:
    """OpenAI Chat API を呼び出す"""
    client = AsyncOpenAI(api_key=os.environ.get("OPENAI_API_KEY"))
    resp = await client.chat.completions.create(
        model=openai_model,
        messages=[
            {"role": "system", "content": system_prompt},
            {"role": "user", "content": user_prompt},
        ],
        temperature=0.7,
        max_tokens=1024,
    )
    return resp.choices[0].message.content


# ── ログ読み込みヘルパー ──────────────────────────────

def _list_experiments() -> list:
    """rag_queries.jsonl を持つ実験一覧を返す"""
    experiments = []
    if not LOG_BASE_DIR.exists():
        return experiments
    for exp_dir in sorted(LOG_BASE_DIR.iterdir(), reverse=True):
        if not exp_dir.is_dir():
            continue
        jsonl_path = exp_dir / "rag_queries.jsonl"
        if jsonl_path.exists():
            line_count = sum(1 for _ in jsonl_path.open("r", encoding="utf-8"))
            experiments.append({
                "experiment_id": exp_dir.name,
                "query_count": line_count,
            })
    return experiments


def _load_rag_queries(experiment_id: str) -> dict:
    """指定実験のRAGクエリログ全件 + サマリー統計を返す"""
    jsonl_path = LOG_BASE_DIR / experiment_id / "rag_queries.jsonl"
    if not jsonl_path.exists():
        return {"error": "not found"}

    queries = []
    for line in jsonl_path.open("r", encoding="utf-8"):
        line = line.strip()
        if line:
            try:
                queries.append(json.loads(line))
            except json.JSONDecodeError:
                continue

    # サマリー統計
    total = len(queries)
    agents = set()
    total_results = 0
    memory_type_counts = {}
    has_results_count = 0

    for q in queries:
        if q.get("agent_id"):
            agents.add(q["agent_id"])
        rc = q.get("results_count", 0)
        total_results += rc
        if rc > 0:
            has_results_count += 1
        for r in q.get("results", []):
            mt = r.get("memory_type", "unknown")
            memory_type_counts[mt] = memory_type_counts.get(mt, 0) + 1

    summary = {
        "total_queries": total,
        "avg_results": round(total_results / total, 2) if total > 0 else 0,
        "agent_count": len(agents),
        "has_results_count": has_results_count,
        "no_results_count": total - has_results_count,
        "memory_type_counts": memory_type_counts,
    }

    return {"queries": queries, "summary": summary}


# ── HTML テンプレート ──────────────────────────────────

HTML_TEMPLATE = r"""<!DOCTYPE html>
<html lang="ja">
<head>
<meta charset="UTF-8">
<meta name="viewport" content="width=device-width, initial-scale=1.0">
<title>RAG記憶検索デモ</title>
<style>
*{box-sizing:border-box;margin:0;padding:0}
body{font-family:-apple-system,BlinkMacSystemFont,"Hiragino Kaku Gothic ProN","Segoe UI",sans-serif;
     background:#f5f5f5;color:#333;line-height:1.6}
.container{max-width:800px;margin:0 auto;padding:16px}
h1{font-size:1.4em;margin-bottom:4px}
.subtitle{color:#666;font-size:.9em;margin-bottom:16px}

/* タブナビゲーション */
.tab-nav{display:flex;gap:0;margin-bottom:16px;border-bottom:2px solid #e0e0e0}
.tab-btn{padding:10px 20px;border:none;background:none;font-size:1em;font-weight:500;
         cursor:pointer;color:#888;border-bottom:3px solid transparent;margin-bottom:-2px;transition:all .2s}
.tab-btn:hover{color:#5c6bc0}
.tab-btn.active{color:#5c6bc0;border-bottom-color:#5c6bc0}
.tab-content{display:none}
.tab-content.active{display:block}

/* セクション */
.section{background:#fff;border-radius:8px;padding:16px;margin-bottom:16px;
         box-shadow:0 1px 3px rgba(0,0,0,.1)}
.section-title{font-size:1.05em;font-weight:bold;margin-bottom:12px;
               padding-bottom:8px;border-bottom:2px solid #e0e0e0}

/* 統計 */
.stats{display:flex;flex-wrap:wrap;gap:8px}
.stat-badge{padding:4px 12px;border-radius:16px;font-size:.85em;font-weight:500}
.stat-total{background:#e8eaf6;color:#283593}
.stat-regional{background:#e3f2fd;color:#1565c0}
.stat-disaster{background:#fff3e0;color:#e65100}
.stat-drill{background:#f3e5f5;color:#6a1b9a}
.stat-social{background:#fffde7;color:#f57f17}
.stat-persona{background:#e8f5e9;color:#2e7d32}

/* 検索フォーム */
.query-input{width:100%;padding:10px 12px;border:2px solid #ddd;border-radius:6px;
             font-size:1em;transition:border-color .2s}
.query-input:focus{outline:none;border-color:#5c6bc0}
.presets{display:flex;flex-wrap:wrap;gap:6px;margin:10px 0}
.preset-btn{padding:4px 12px;border:1px solid #bbb;border-radius:16px;
            background:#fff;cursor:pointer;font-size:.85em;transition:all .15s}
.preset-btn:hover{background:#e8eaf6;border-color:#5c6bc0}
.toggle-group{display:flex;align-items:center;gap:12px;margin:10px 0}
.toggle-label{font-weight:500}
.toggle-wrap{display:flex;gap:0}
.toggle-wrap label{padding:6px 16px;border:1px solid #bbb;cursor:pointer;font-size:.9em;
                   transition:all .15s;user-select:none}
.toggle-wrap label:first-child{border-radius:6px 0 0 6px}
.toggle-wrap label:last-child{border-radius:0 6px 6px 0}
.toggle-wrap input{display:none}
.toggle-wrap input:checked+span{background:#5c6bc0;color:#fff;border-color:#5c6bc0}
.toggle-wrap label:has(input:checked){background:#5c6bc0;color:#fff;border-color:#5c6bc0}
.toggle-hint{color:#888;font-size:.8em;margin-left:4px}
.search-btn{display:block;width:100%;padding:10px;margin-top:12px;border:none;
            border-radius:6px;background:#5c6bc0;color:#fff;font-size:1em;font-weight:bold;
            cursor:pointer;transition:background .2s}
.search-btn:hover{background:#3f51b5}
.search-btn:disabled{background:#bbb;cursor:not-allowed}

/* 結果カード */
.result-card{border-radius:6px;padding:12px;margin-bottom:8px;border-left:4px solid #ccc}
.result-card.regional_knowledge{background:#e3f2fd;border-left-color:#1565c0}
.result-card.disaster_experience{background:#fff3e0;border-left-color:#e65100}
.result-card.drill_knowledge{background:#f3e5f5;border-left-color:#6a1b9a}
.result-card.social_knowledge{background:#fffde7;border-left-color:#f57f17}
.result-card.persona_knowledge{background:#e8f5e9;border-left-color:#2e7d32}
.card-header{display:flex;justify-content:space-between;align-items:center;margin-bottom:6px}
.card-type{font-weight:bold;font-size:.85em}
.card-score{font-size:.8em;color:#666}
.score-bar{height:4px;border-radius:2px;background:#ddd;margin-top:2px;width:120px;display:inline-block;vertical-align:middle}
.score-fill{height:100%;border-radius:2px;background:#5c6bc0}
.card-content{font-size:.95em}
.card-meta{margin-top:6px;font-size:.75em;color:#888}

/* プロンプトプレビュー */
.prompt-box{background:#263238;color:#e0e0e0;padding:14px;border-radius:6px;
            font-family:"Fira Code",monospace;font-size:.85em;white-space:pre-wrap;
            line-height:1.5;overflow-x:auto;max-height:300px;overflow-y:auto}

/* LLM応答 */
.llm-btn{display:block;width:100%;padding:10px;border:2px solid #e65100;border-radius:6px;
         background:#fff;color:#e65100;font-size:.95em;font-weight:bold;cursor:pointer;
         transition:all .2s}
.llm-btn:hover{background:#fff3e0}
.llm-btn:disabled{border-color:#bbb;color:#bbb;cursor:not-allowed}
.llm-response{background:#fafafa;border:1px solid #ddd;border-radius:6px;padding:14px;
              font-size:.95em;line-height:1.7;white-space:pre-wrap}
.model-info{font-size:.8em;color:#888;margin-top:8px}

/* ローディング */
.loading{text-align:center;padding:20px;color:#888}
.loading::after{content:"";display:inline-block;width:16px;height:16px;border:2px solid #888;
               border-top-color:transparent;border-radius:50%;animation:spin .6s linear infinite;
               margin-left:8px;vertical-align:middle}
@keyframes spin{to{transform:rotate(360deg)}}
.hidden{display:none}

/* デバッグ */
.debug-toggle{font-size:.8em;color:#888;cursor:pointer;text-decoration:underline;margin-top:8px}
.debug-box{background:#f5f5f5;border:1px solid #ddd;border-radius:6px;padding:10px;
           margin-top:8px;font-family:monospace;font-size:.8em;white-space:pre-wrap;max-height:200px;overflow-y:auto}

/* ── ログビューア ── */
.exp-list{list-style:none}
.exp-item{padding:10px 12px;border:1px solid #e0e0e0;border-radius:6px;margin-bottom:6px;
          cursor:pointer;transition:all .15s;display:flex;justify-content:space-between;align-items:center}
.exp-item:hover{background:#e8eaf6;border-color:#5c6bc0}
.exp-item.selected{background:#e8eaf6;border-color:#5c6bc0;font-weight:bold}
.exp-id{font-family:monospace;font-size:.95em}
.exp-count{font-size:.85em;color:#666}

.log-summary{display:flex;flex-wrap:wrap;gap:12px;margin-bottom:16px}
.summary-card{background:#f5f5f5;border-radius:8px;padding:10px 16px;text-align:center;min-width:100px}
.summary-value{font-size:1.5em;font-weight:bold;color:#5c6bc0}
.summary-label{font-size:.8em;color:#888}

.log-filters{display:flex;flex-wrap:wrap;gap:8px;margin-bottom:12px;align-items:center}
.log-filters select{padding:6px 10px;border:1px solid #ddd;border-radius:6px;font-size:.9em}
.log-filters label{font-size:.9em;font-weight:500}

.log-entry{border:1px solid #e0e0e0;border-radius:6px;padding:12px;margin-bottom:8px}
.log-entry-header{display:flex;flex-wrap:wrap;gap:8px;align-items:center;margin-bottom:8px}
.log-agent{font-weight:bold;font-size:.9em;background:#e8eaf6;color:#283593;padding:2px 8px;border-radius:4px}
.log-time{font-size:.85em;color:#888;font-family:monospace}
.log-training{font-size:.8em;padding:2px 8px;border-radius:4px}
.log-training.trained{background:#e8f5e9;color:#2e7d32}
.log-training.untrained{background:#ffebee;color:#c62828}
.log-query{font-size:.95em;margin-bottom:8px;padding:6px 10px;background:#fafafa;border-radius:4px;border-left:3px solid #5c6bc0}
.log-results-empty{font-size:.85em;color:#888;font-style:italic}

.pagination{display:flex;justify-content:center;align-items:center;gap:8px;margin-top:16px}
.pagination button{padding:6px 14px;border:1px solid #ddd;border-radius:6px;background:#fff;
                   cursor:pointer;font-size:.9em;transition:all .15s}
.pagination button:hover:not(:disabled){background:#e8eaf6;border-color:#5c6bc0}
.pagination button:disabled{color:#bbb;cursor:not-allowed}
.pagination .page-info{font-size:.9em;color:#666}
</style>
</head>
<body>
<div class="container">
  <h1>RAG記憶検索デモ</h1>
  <p class="subtitle">避難シミュレーション 長期記憶検索テスト</p>

  <!-- タブナビゲーション -->
  <div class="tab-nav">
    <button class="tab-btn active" onclick="switchTab('demo')">RAG検索デモ</button>
    <button class="tab-btn" onclick="switchTab('logs')">シミュレーションログ</button>
  </div>

  <!-- ===== デモタブ ===== -->
  <div class="tab-content active" id="tab-demo">

  <!-- 統計 -->
  <div class="section">
    <div class="section-title">記憶データ統計</div>
    <div class="stats" id="stats"><span class="loading">読み込み中</span></div>
  </div>

  <!-- 検索条件 -->
  <div class="section">
    <div class="section-title">検索条件</div>
    <input class="query-input" id="query" placeholder="例: 津波が来たらどこに逃げればいい？">
    <div class="presets">
      <button class="preset-btn" data-q="津波が来たらどこに逃げればいい？">津波避難</button>
      <button class="preset-btn" data-q="避難所はどこにある？">避難所</button>
      <button class="preset-btn" data-q="家族の安否を確認したい">家族安否</button>
      <button class="preset-btn" data-q="地震が起きたらまず何をすべき？">地震直後</button>
      <button class="preset-btn" data-q="高齢者を連れて避難したい">高齢者避難</button>
    </div>
    <div class="toggle-group">
      <span class="toggle-label">避難訓練経験:</span>
      <div class="toggle-wrap">
        <label><input type="radio" name="training" value="true" checked><span>ON</span></label>
        <label><input type="radio" name="training" value="false"><span>OFF</span></label>
      </div>
      <span class="toggle-hint" id="training-hint">→ 全記憶タイプ検索 (最大5件)</span>
    </div>
    <button class="search-btn" id="search-btn" onclick="doSearch()">検索実行</button>
  </div>

  <!-- 検索結果 -->
  <div class="section hidden" id="results-section">
    <div class="section-title">検索結果 (<span id="result-count">0</span>件ヒット)</div>
    <div id="results"></div>
    <span class="debug-toggle" onclick="toggleDebug('search-debug')">検索パラメータを表示</span>
    <div class="debug-box hidden" id="search-debug"></div>
  </div>

  <!-- プロンプトプレビュー -->
  <div class="section hidden" id="prompt-section">
    <div class="section-title">プロンプトプレビュー（LLMに渡される形式）</div>
    <div class="prompt-box" id="prompt-preview"></div>
    <button class="llm-btn" id="llm-btn" style="margin-top:12px" onclick="doGenerate()">
      LLM応答を生成（APIコスト発生）
    </button>
  </div>

  <!-- LLM応答 -->
  <div class="section hidden" id="llm-section">
    <div class="section-title">LLM応答</div>
    <div class="llm-response" id="llm-response"></div>
    <div class="model-info" id="model-info"></div>
    <span class="debug-toggle" onclick="toggleDebug('llm-debug')">送信プロンプト詳細を表示</span>
    <div class="debug-box hidden" id="llm-debug"></div>
  </div>

  </div><!-- /tab-demo -->

  <!-- ===== ログビューアタブ ===== -->
  <div class="tab-content" id="tab-logs">

  <!-- 実験一覧 -->
  <div class="section">
    <div class="section-title">実験一覧</div>
    <div id="exp-list"><span class="loading">読み込み中</span></div>
  </div>

  <!-- サマリー統計 -->
  <div class="section hidden" id="log-summary-section">
    <div class="section-title">サマリー統計</div>
    <div class="log-summary" id="log-summary"></div>
  </div>

  <!-- RAGクエリログ -->
  <div class="section hidden" id="log-entries-section">
    <div class="section-title">RAGクエリログ (<span id="log-total">0</span>件)</div>
    <div class="log-filters">
      <label>エージェント:</label>
      <select id="filter-agent" onchange="applyLogFilters()"><option value="">全て</option></select>
      <label>記憶タイプ:</label>
      <select id="filter-memtype" onchange="applyLogFilters()">
        <option value="">全て</option>
        <option value="regional_knowledge">地域知識</option>
        <option value="persona_knowledge">自己認識</option>
        <option value="disaster_experience">過去の経験</option>
        <option value="drill_knowledge">避難訓練の知識</option>
        <option value="social_knowledge">社会的知識</option>
      </select>
      <label>結果:</label>
      <select id="filter-hasresults" onchange="applyLogFilters()">
        <option value="">全て</option>
        <option value="yes">結果あり</option>
        <option value="no">結果なし</option>
      </select>
    </div>
    <div id="log-entries"></div>
    <div class="pagination" id="log-pagination"></div>
  </div>

  </div><!-- /tab-logs -->

</div>

<script>
let lastSearchResults = [];
let lastPromptPreview = "";

// ── ログビューア状態 ──
let logAllQueries = [];
let logFilteredQueries = [];
let logCurrentPage = 0;
const LOG_PAGE_SIZE = 50;

// ── タブ切り替え ──
function switchTab(tab) {
  document.querySelectorAll(".tab-btn").forEach(b => b.classList.remove("active"));
  document.querySelectorAll(".tab-content").forEach(c => c.classList.remove("active"));
  document.querySelector(`.tab-btn[onclick="switchTab('${tab}')"]`).classList.add("active");
  document.getElementById("tab-" + tab).classList.add("active");
  if (tab === "logs") loadExperiments();
}

// ── 初期化 ──
document.addEventListener("DOMContentLoaded", () => {
  loadStats();
  // プリセットボタン
  document.querySelectorAll(".preset-btn").forEach(btn => {
    btn.addEventListener("click", () => {
      document.getElementById("query").value = btn.dataset.q;
    });
  });
  // トグルヒント更新
  document.querySelectorAll('input[name="training"]').forEach(r => {
    r.addEventListener("change", updateHint);
  });
  // Enter キーで検索
  document.getElementById("query").addEventListener("keydown", e => {
    if (e.key === "Enter") doSearch();
  });
});

function getTraining() {
  return document.querySelector('input[name="training"]:checked').value === "true";
}

function updateHint() {
  const hint = document.getElementById("training-hint");
  hint.textContent = getTraining()
    ? "→ 全記憶タイプ検索 (最大5件)"
    : "→ 訓練知識を除外 (最大3件)";
}

// ── 統計読み込み ──
async function loadStats() {
  try {
    const res = await fetch("/api/stats");
    const data = await res.json();
    const el = document.getElementById("stats");
    const typeClass = {
      regional_knowledge: "stat-regional",
      disaster_experience: "stat-disaster",
      drill_knowledge: "stat-drill",
      social_knowledge: "stat-social",
      persona_knowledge: "stat-persona"
    };
    const typeLabel = {
      regional_knowledge: "地域知識",
      disaster_experience: "災害経験",
      drill_knowledge: "訓練知識",
      social_knowledge: "社会知識",
      persona_knowledge: "自己認識"
    };
    let html = `<span class="stat-badge stat-total">全${data.total}件</span>`;
    for (const [k, v] of Object.entries(data.by_type || {})) {
      const cls = typeClass[k] || "stat-total";
      const lbl = typeLabel[k] || k;
      html += `<span class="stat-badge ${cls}">${lbl}: ${v}</span>`;
    }
    el.innerHTML = html;
  } catch (e) {
    document.getElementById("stats").textContent = "統計の読み込みに失敗しました";
  }
}

// ── 検索 ──
async function doSearch() {
  const query = document.getElementById("query").value.trim();
  if (!query) { alert("クエリを入力してください"); return; }

  const btn = document.getElementById("search-btn");
  btn.disabled = true; btn.textContent = "検索中...";

  // LLMセクションを隠す
  hide("llm-section");

  try {
    const res = await fetch("/api/search", {
      method: "POST",
      headers: {"Content-Type": "application/json"},
      body: JSON.stringify({ query, has_evacuation_training: getTraining() })
    });
    const data = await res.json();
    lastSearchResults = data.results || [];
    lastPromptPreview = data.prompt_preview || "";

    // 結果表示
    show("results-section");
    document.getElementById("result-count").textContent = lastSearchResults.length;
    const container = document.getElementById("results");
    if (lastSearchResults.length === 0) {
      container.innerHTML = '<p style="color:#888;padding:8px">該当する記憶が見つかりませんでした。閾値を下回った可能性があります。</p>';
    } else {
      container.innerHTML = lastSearchResults.map(renderCard).join("");
    }

    // デバッグ
    document.getElementById("search-debug").textContent = JSON.stringify(data.search_params, null, 2);

    // プロンプトプレビュー
    show("prompt-section");
    document.getElementById("prompt-preview").textContent = lastPromptPreview;
    document.getElementById("llm-btn").disabled = false;
  } catch (e) {
    alert("検索エラー: " + e.message);
  } finally {
    btn.disabled = false; btn.textContent = "検索実行";
  }
}

function renderCard(mem) {
  const typeLabel = {
    regional_knowledge: "地域知識",
    persona_knowledge: "自己認識",
    disaster_experience: "過去の経験",
    drill_knowledge: "避難訓練の知識",
    social_knowledge: "社会的知識"
  };
  const pct = Math.round(mem.similarity * 100);
  const meta = mem.metadata || {};
  const metaStr = Object.keys(meta).length > 0
    ? `<div class="card-meta">${Object.entries(meta).map(([k,v])=>`${k}: ${v}`).join(" | ")}</div>`
    : "";
  return `<div class="result-card ${mem.memory_type}">
    <div class="card-header">
      <span class="card-type">${typeLabel[mem.memory_type] || mem.memory_type}</span>
      <span class="card-score">類似度: ${mem.similarity.toFixed(3)}
        <span class="score-bar"><span class="score-fill" style="width:${pct}%"></span></span>
      </span>
    </div>
    <div class="card-content">${escapeHtml(mem.content)}</div>
    ${metaStr}
  </div>`;
}

// ── LLM応答生成 ──
async function doGenerate() {
  const query = document.getElementById("query").value.trim();
  if (!query || lastSearchResults.length === 0) return;

  const btn = document.getElementById("llm-btn");
  btn.disabled = true; btn.textContent = "生成中...";

  show("llm-section");
  document.getElementById("llm-response").innerHTML = '<div class="loading">LLM応答を生成中</div>';
  document.getElementById("model-info").textContent = "";

  try {
    const res = await fetch("/api/generate", {
      method: "POST",
      headers: {"Content-Type": "application/json"},
      body: JSON.stringify({
        query,
        memories: lastSearchResults,
        has_evacuation_training: getTraining()
      })
    });
    const data = await res.json();
    if (data.error) throw new Error(data.error);

    document.getElementById("llm-response").textContent = data.response;
    document.getElementById("model-info").textContent = `モデル: ${data.model}`;
    document.getElementById("llm-debug").textContent =
      "=== System Prompt ===\n" + data.system_prompt +
      "\n\n=== User Prompt ===\n" + data.user_prompt;
  } catch (e) {
    document.getElementById("llm-response").textContent = "エラー: " + e.message;
  } finally {
    btn.disabled = false; btn.textContent = "LLM応答を生成（APIコスト発生）";
  }
}

// ══════════════════════════════════════════
// ログビューア
// ══════════════════════════════════════════

async function loadExperiments() {
  const el = document.getElementById("exp-list");
  el.innerHTML = '<span class="loading">読み込み中</span>';
  try {
    const res = await fetch("/api/experiments");
    const data = await res.json();
    if (!data.experiments || data.experiments.length === 0) {
      el.innerHTML = '<p style="color:#888">RAGクエリログを持つ実験がありません。シミュレーションを実行してください。</p>';
      return;
    }
    el.innerHTML = '<ul class="exp-list">' + data.experiments.map(exp =>
      `<li class="exp-item" onclick="loadExperimentLog('${exp.experiment_id}', this)">
        <span class="exp-id">${exp.experiment_id}</span>
        <span class="exp-count">${exp.query_count}件のクエリ</span>
      </li>`
    ).join("") + '</ul>';
  } catch (e) {
    el.innerHTML = '<p style="color:#c62828">実験一覧の読み込みに失敗しました</p>';
  }
}

async function loadExperimentLog(expId, itemEl) {
  // 選択状態
  document.querySelectorAll(".exp-item").forEach(el => el.classList.remove("selected"));
  if (itemEl) itemEl.classList.add("selected");

  show("log-summary-section");
  show("log-entries-section");
  document.getElementById("log-summary").innerHTML = '<span class="loading">読み込み中</span>';
  document.getElementById("log-entries").innerHTML = '<span class="loading">読み込み中</span>';

  try {
    const res = await fetch(`/api/experiments/${expId}/rag_queries`);
    const data = await res.json();
    if (data.error) throw new Error(data.error);

    logAllQueries = data.queries || [];
    const s = data.summary || {};

    // サマリー統計
    const summaryHtml = `
      <div class="summary-card"><div class="summary-value">${s.total_queries || 0}</div><div class="summary-label">総クエリ数</div></div>
      <div class="summary-card"><div class="summary-value">${s.avg_results || 0}</div><div class="summary-label">平均結果件数</div></div>
      <div class="summary-card"><div class="summary-value">${s.agent_count || 0}</div><div class="summary-label">エージェント数</div></div>
      <div class="summary-card"><div class="summary-value">${s.has_results_count || 0}</div><div class="summary-label">結果あり</div></div>
      <div class="summary-card"><div class="summary-value">${s.no_results_count || 0}</div><div class="summary-label">結果なし</div></div>
    `;
    document.getElementById("log-summary").innerHTML = summaryHtml;

    // エージェントフィルタ更新
    const agentSelect = document.getElementById("filter-agent");
    const agents = [...new Set(logAllQueries.map(q => q.agent_id).filter(Boolean))].sort();
    agentSelect.innerHTML = '<option value="">全て</option>' +
      agents.map(a => `<option value="${a}">${a}</option>`).join("");

    // フィルタリセット＆表示
    document.getElementById("filter-memtype").value = "";
    document.getElementById("filter-hasresults").value = "";
    applyLogFilters();
  } catch (e) {
    document.getElementById("log-summary").innerHTML = '<p style="color:#c62828">読み込みエラー</p>';
    document.getElementById("log-entries").innerHTML = '';
  }
}

function applyLogFilters() {
  const agentFilter = document.getElementById("filter-agent").value;
  const memtypeFilter = document.getElementById("filter-memtype").value;
  const hasresultsFilter = document.getElementById("filter-hasresults").value;

  logFilteredQueries = logAllQueries.filter(q => {
    if (agentFilter && q.agent_id !== agentFilter) return false;
    if (memtypeFilter) {
      const types = (q.results || []).map(r => r.memory_type);
      if (!types.includes(memtypeFilter)) return false;
    }
    if (hasresultsFilter === "yes" && (q.results_count || 0) === 0) return false;
    if (hasresultsFilter === "no" && (q.results_count || 0) > 0) return false;
    return true;
  });

  logCurrentPage = 0;
  document.getElementById("log-total").textContent = logFilteredQueries.length;
  renderLogPage();
}

function renderLogPage() {
  const start = logCurrentPage * LOG_PAGE_SIZE;
  const end = Math.min(start + LOG_PAGE_SIZE, logFilteredQueries.length);
  const pageQueries = logFilteredQueries.slice(start, end);

  const typeLabel = {
    regional_knowledge: "地域知識",
    persona_knowledge: "自己認識",
    disaster_experience: "過去の経験",
    drill_knowledge: "避難訓練の知識",
    social_knowledge: "社会的知識"
  };

  const html = pageQueries.map(q => {
    const time = (q.timestamp || "").split(" ")[1] || "";
    const trainingCls = q.has_training ? "trained" : "untrained";
    const trainingLabel = q.has_training ? "訓練済み" : "未訓練";

    let resultsHtml;
    if (!q.results || q.results.length === 0) {
      resultsHtml = '<div class="log-results-empty">結果なし（閾値以上の記憶が見つかりませんでした）</div>';
    } else {
      resultsHtml = q.results.map(r => {
        const pct = Math.round((r.similarity || 0) * 100);
        const lbl = typeLabel[r.memory_type] || r.memory_type;
        return `<div class="result-card ${r.memory_type}" style="margin-bottom:4px;padding:8px">
          <div class="card-header">
            <span class="card-type">${lbl}</span>
            <span class="card-score">${(r.similarity || 0).toFixed(3)}
              <span class="score-bar"><span class="score-fill" style="width:${pct}%"></span></span>
            </span>
          </div>
          <div class="card-content" style="font-size:.9em">${escapeHtml(r.content || "")}</div>
        </div>`;
      }).join("");
    }

    return `<div class="log-entry">
      <div class="log-entry-header">
        <span class="log-agent">${escapeHtml(q.agent_id || "unknown")}${q.agent_id_int != null ? " (ID:" + q.agent_id_int + ")" : ""}</span>
        <span class="log-time">${time}</span>
        <span class="log-training ${trainingCls}">${trainingLabel}</span>
        <span style="font-size:.8em;color:#888">ep:${q.episode_id != null ? q.episode_id : "-"} t:${q.episode_elapsed_time != null ? q.episode_elapsed_time + "s" : "-"}</span>
      </div>
      <div class="log-query">${escapeHtml(q.query || "")}</div>
      ${resultsHtml}
    </div>`;
  }).join("");

  document.getElementById("log-entries").innerHTML = html || '<p style="color:#888">該当するログがありません</p>';

  // ページネーション
  const totalPages = Math.ceil(logFilteredQueries.length / LOG_PAGE_SIZE);
  const pagEl = document.getElementById("log-pagination");
  if (totalPages <= 1) {
    pagEl.innerHTML = "";
  } else {
    pagEl.innerHTML = `
      <button onclick="logGoPage(${logCurrentPage - 1})" ${logCurrentPage === 0 ? "disabled" : ""}>前へ</button>
      <span class="page-info">${logCurrentPage + 1} / ${totalPages}</span>
      <button onclick="logGoPage(${logCurrentPage + 1})" ${logCurrentPage >= totalPages - 1 ? "disabled" : ""}>次へ</button>
    `;
  }
}

function logGoPage(page) {
  const totalPages = Math.ceil(logFilteredQueries.length / LOG_PAGE_SIZE);
  if (page < 0 || page >= totalPages) return;
  logCurrentPage = page;
  renderLogPage();
  document.getElementById("log-entries-section").scrollIntoView({behavior: "smooth"});
}

// ── ユーティリティ ──
function show(id) { document.getElementById(id).classList.remove("hidden"); }
function hide(id) { document.getElementById(id).classList.add("hidden"); }
function toggleDebug(id) {
  document.getElementById(id).classList.toggle("hidden");
}
function escapeHtml(s) {
  return s.replace(/&/g,"&amp;").replace(/</g,"&lt;").replace(/>/g,"&gt;").replace(/"/g,"&quot;");
}
</script>
</body>
</html>"""

# ── HTTP ハンドラ ──────────────────────────────────────


class DemoHandler(BaseHTTPRequestHandler):
    """デモ用HTTPリクエストハンドラ"""

    def do_GET(self):
        if self.path == "/":
            self._respond_html(HTML_TEMPLATE)
        elif self.path == "/api/stats":
            stats = manager.get_statistics()
            self._respond_json(stats)
        elif self.path == "/api/experiments":
            self._respond_json({"experiments": _list_experiments()})
        elif self.path.startswith("/api/experiments/") and self.path.endswith("/rag_queries"):
            # /api/experiments/{id}/rag_queries
            parts = self.path.split("/")
            # parts = ['', 'api', 'experiments', '{id}', 'rag_queries']
            if len(parts) == 5:
                exp_id = parts[3]
                result = _load_rag_queries(exp_id)
                if "error" in result:
                    self._respond_json(result, 404)
                else:
                    self._respond_json(result)
            else:
                self._respond_json({"error": "Not Found"}, 404)
        else:
            self._respond_json({"error": "Not Found"}, 404)

    def do_POST(self):
        body = self._read_body()
        if body is None:
            return

        if self.path == "/api/search":
            self._handle_search(body)
        elif self.path == "/api/generate":
            self._handle_generate(body)
        else:
            self._respond_json({"error": "Not Found"}, 404)

    # ── 検索 API ──

    def _handle_search(self, body: dict):
        query = body.get("query", "")
        has_training = body.get("has_evacuation_training", False)

        if not query:
            self._respond_json({"error": "query is required"}, 400)
            return

        # 訓練フラグに基づくパラメータ決定
        if has_training:
            memory_types = None
            top_k = 5
        else:
            memory_types = [
                "regional_knowledge", "persona_knowledge",
                "disaster_experience", "social_knowledge",
            ]
            top_k = 3

        results = _run_async(
            manager.search(
                query=query,
                agent_id=None,
                memory_types=memory_types,
                top_k=top_k,
                threshold=0.3,
            )
        )

        # 日本語ラベルを付与
        for r in results:
            r["type_label"] = TYPE_LABELS.get(r.get("memory_type", ""), "記憶")

        prompt_preview = build_demo_prompt(results)

        self._respond_json({
            "results": results,
            "prompt_preview": prompt_preview,
            "search_params": {
                "query": query,
                "has_evacuation_training": has_training,
                "memory_types": memory_types or "全タイプ",
                "top_k": top_k,
                "threshold": 0.3,
            },
        })

    # ── LLM応答生成 API ──

    def _handle_generate(self, body: dict):
        query = body.get("query", "")
        memories = body.get("memories", [])

        if not query:
            self._respond_json({"error": "query is required"}, 400)
            return

        system_prompt = (
            "あなたはいわき市の住民です。"
            "以下の記憶・知識を参考に、住民として自然に回答してください。"
        )
        user_prompt = build_demo_prompt(memories) + f"\n\n質問: {query}"

        try:
            response_text = _run_async(_call_llm(system_prompt, user_prompt))
        except Exception as e:
            self._respond_json({"error": str(e)}, 500)
            return

        self._respond_json({
            "response": response_text,
            "system_prompt": system_prompt,
            "user_prompt": user_prompt,
            "model": openai_model,
        })

    # ── レスポンスヘルパー ──

    def _read_body(self) -> dict | None:
        try:
            length = int(self.headers.get("Content-Length", 0))
            raw = self.rfile.read(length)
            return json.loads(raw.decode("utf-8"))
        except Exception as e:
            self._respond_json({"error": f"Invalid JSON: {e}"}, 400)
            return None

    def _respond_json(self, data: dict, status: int = 200):
        payload = json.dumps(data, ensure_ascii=False).encode("utf-8")
        self.send_response(status)
        self.send_header("Content-Type", "application/json; charset=utf-8")
        self.send_header("Content-Length", str(len(payload)))
        self.end_headers()
        self.wfile.write(payload)

    def _respond_html(self, html: str):
        payload = html.encode("utf-8")
        self.send_response(200)
        self.send_header("Content-Type", "text/html; charset=utf-8")
        self.send_header("Content-Length", str(len(payload)))
        self.end_headers()
        self.wfile.write(payload)

    def log_message(self, format, *args):
        """アクセスログを簡潔にする"""
        if "/api/" in (args[0] if args else ""):
            print(f"[RAG Demo] {args[0]}")


# ── メイン ─────────────────────────────────────────────


def main():
    global manager, openai_model

    # .env 読み込み
    env_path = Path(__file__).parent / ".env"
    load_dotenv(env_path)

    api_key = os.environ.get("OPENAI_API_KEY")
    if not api_key:
        print("[RAG Demo] エラー: OPENAI_API_KEY が設定されていません")
        print("[RAG Demo] llm_server/.env に OPENAI_API_KEY を設定してください")
        sys.exit(1)

    openai_model = os.environ.get("OPENAI_MODEL", "gpt-4o-mini")

    # MemoryManager 初期化
    client = AsyncOpenAI(api_key=api_key)
    manager = MemoryManager(openai_client=client)

    # 記憶データ読み込み
    data_dir = Path(__file__).parent / "data"
    memories_path = data_dir / "memories.json"
    if not memories_path.exists():
        memories_path = data_dir / "memories_sample.json"

    success = _run_async(manager.load_memories(str(memories_path)))
    if not success:
        print("[RAG Demo] エラー: 記憶データの読み込みに失敗しました")
        sys.exit(1)

    stats = manager.get_statistics()
    print(f"[RAG Demo] {stats['total']}件の記憶を読み込みました")
    print(f"[RAG Demo] ベクトル化完了")

    # HTTP サーバー起動
    port = int(os.environ.get("RAG_DEMO_PORT", DEFAULT_PORT))
    server = HTTPServer(("0.0.0.0", port), DemoHandler)

    print(f"[RAG Demo] ========================================")
    print(f"[RAG Demo] ブラウザで http://localhost:{port} を開いてください")
    print(f"[RAG Demo] 終了: Ctrl+C")
    print(f"[RAG Demo] ========================================")

    try:
        server.serve_forever()
    except KeyboardInterrupt:
        print("\n[RAG Demo] 終了します")
        server.server_close()


if __name__ == "__main__":
    main()
