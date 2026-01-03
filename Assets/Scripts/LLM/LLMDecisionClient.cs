using System;
using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using LLM;
using UnityEngine;

namespace LLM
{
    public class LLMDecisionClient : MonoBehaviour
    {
        [SerializeField] private string serverUrl = "ws://localhost:8765";
        [SerializeField] private float requestTimeoutSeconds = 10f;

        private ClientWebSocket _socket;
        private CancellationTokenSource _cts;
        private readonly ConcurrentDictionary<string, TaskCompletionSource<string>> _pendingRequests = new();
        private Task _receiveLoopTask;
        private readonly object _connectionLock = new();

        private async void OnEnable()
        {
            await EnsureConnectionAsync();
        }

        private async void OnDisable()
        {
            await CloseAsync();
        }

        private async Task EnsureConnectionAsync()
        {
            if (_socket is { State: WebSocketState.Open })
            {
                return;
            }

            lock (_connectionLock)
            {
                if (_socket is { State: WebSocketState.Open })
                {
                    return;
                }

                _cts?.Cancel();
                _cts?.Dispose();
                _cts = new CancellationTokenSource();
                _socket?.Dispose();
                _socket = new ClientWebSocket();
            }

            try
            {
                await _socket.ConnectAsync(new Uri(serverUrl), _cts.Token);
                _receiveLoopTask = Task.Run(() => ReceiveLoopAsync(_cts.Token));
            }
            catch (Exception ex)
            {
                Debug.LogError($"[LLMDecisionClient] 接続に失敗しました: {ex.Message}");
                throw;
            }
        }

        private async Task CloseAsync()
        {
            if (_socket == null)
            {
                return;
            }

            try
            {
                _cts?.Cancel();
                if (_socket.State == WebSocketState.Open)
                {
                    await _socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "shutdown", CancellationToken.None);
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[LLMDecisionClient] 切断時に例外: {ex.Message}");
            }
            finally
            {
                _socket.Dispose();
                _socket = null;
            }
        }

        public Task<LLMActionResponse> RequestDecisionAsync(LLMActionRequest request, CancellationToken cancellationToken = default)
        {
            var payload = JsonUtility.ToJson(request);
            return SendRequestAsync<LLMActionResponse>(payload, request.request_id, cancellationToken);
        }

        public Task<LLMEvacDecisionResponse> RequestEvacueeDecisionAsync(LLMEvacDecisionRequest request, CancellationToken cancellationToken = default)
        {
            var payload = JsonUtility.ToJson(request);
            return SendRequestAsync<LLMEvacDecisionResponse>(payload, request.request_id, cancellationToken);
        }

        public Task<LLMConversationResponseResponse> RequestConversationResponseAsync(LLMConversationResponseRequest request, CancellationToken cancellationToken = default)
        {
            var payload = JsonUtility.ToJson(request);
            return SendRequestAsync<LLMConversationResponseResponse>(payload, request.request_id, cancellationToken);
        }

        public Task<LLMFamilyContactResponseResponse> RequestFamilyContactResponseAsync(LLMFamilyContactResponseRequest request, CancellationToken cancellationToken = default)
        {
            var payload = JsonUtility.ToJson(request);
            return SendRequestAsync<LLMFamilyContactResponseResponse>(payload, request.request_id, cancellationToken);
        }

        private async Task<T> SendRequestAsync<T>(string payloadJson, string requestId, CancellationToken cancellationToken)
        {
            await EnsureConnectionAsync();

            var buffer = Encoding.UTF8.GetBytes(payloadJson);
            var messageSegment = new ArraySegment<byte>(buffer);
            var tcs = new TaskCompletionSource<string>(TaskCreationOptions.RunContinuationsAsynchronously);
            _pendingRequests[requestId] = tcs;

            using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(_cts.Token, cancellationToken);
            linkedCts.CancelAfter(TimeSpan.FromSeconds(requestTimeoutSeconds));

            try
            {
                await _socket.SendAsync(messageSegment, WebSocketMessageType.Text, true, linkedCts.Token);

                using (linkedCts.Token.Register(() => tcs.TrySetCanceled()))
                {
                    var responseJson = await tcs.Task;
                    return JsonUtility.FromJson<T>(responseJson);
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[LLMDecisionClient] リクエスト送信に失敗: {ex.Message}");
                _pendingRequests.TryRemove(requestId, out _);
                throw;
            }
        }

        private async Task ReceiveLoopAsync(CancellationToken token)
        {
            var buffer = new byte[8192];

            try
            {
                while (!token.IsCancellationRequested && _socket is { State: WebSocketState.Open })
                {
                    var builder = new StringBuilder();
                    WebSocketReceiveResult result;
                    do
                    {
                        result = await _socket.ReceiveAsync(new ArraySegment<byte>(buffer), token);
                        if (result.MessageType == WebSocketMessageType.Close)
                        {
                            await _socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "server closed", CancellationToken.None);
                            return;
                        }
                        var chunk = Encoding.UTF8.GetString(buffer, 0, result.Count);
                        builder.Append(chunk);
                    } while (!result.EndOfMessage);

                    var message = builder.ToString();
                    var envelope = JsonUtility.FromJson<ResponseEnvelope>(message);
                    if (!string.IsNullOrEmpty(envelope.request_id) &&
                        _pendingRequests.TryRemove(envelope.request_id, out var tcs))
                    {
                        tcs.TrySetResult(message);
                    }
                }
            }
            catch (OperationCanceledException)
            {
                // ignore
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[LLMDecisionClient] 受信ループで例外: {ex.Message}");
            }
        }
    }
}

