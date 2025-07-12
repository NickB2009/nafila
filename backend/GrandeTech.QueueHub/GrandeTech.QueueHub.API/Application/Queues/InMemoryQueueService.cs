using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Grande.Fila.API.Application.Queues
{
    public class InMemoryQueueService : IQueueService, IHostedService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<InMemoryQueueService> _logger;
        private readonly Channel<QueueMessageWrapper> _channel;
        private readonly ConcurrentDictionary<MessagePriority, Channel<QueueMessageWrapper>> _priorityChannels;
        private readonly ConcurrentDictionary<Guid, QueueMessageWrapper> _delayedMessages;
        private readonly Timer _delayedMessageTimer;
        private readonly CancellationTokenSource _cancellationTokenSource;
        private Task? _processingTask;
        private Task? _delayedProcessingTask;

        // Statistics
        private long _processedMessages = 0;
        private long _failedMessages = 0;
        private DateTime _lastProcessedAt = DateTime.UtcNow;
        private readonly ConcurrentQueue<string> _recentErrors = new();

        public InMemoryQueueService(IServiceProvider serviceProvider, ILogger<InMemoryQueueService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
            _cancellationTokenSource = new CancellationTokenSource();

            // Create main channel for normal priority messages
            _channel = Channel.CreateUnbounded<QueueMessageWrapper>();

            // Create priority channels
            _priorityChannels = new ConcurrentDictionary<MessagePriority, Channel<QueueMessageWrapper>>();
            foreach (MessagePriority priority in Enum.GetValues<MessagePriority>())
            {
                _priorityChannels[priority] = Channel.CreateUnbounded<QueueMessageWrapper>();
            }

            _delayedMessages = new ConcurrentDictionary<Guid, QueueMessageWrapper>();
            _delayedMessageTimer = new Timer(ProcessDelayedMessages, null, TimeSpan.FromSeconds(30), TimeSpan.FromSeconds(30));
        }

        public async Task<bool> EnqueueAsync<T>(T message, CancellationToken cancellationToken = default) where T : class
        {
            if (message is not QueueMessage queueMessage)
            {
                _logger.LogError("Message must inherit from QueueMessage. Received: {MessageType}", typeof(T).Name);
                return false;
            }

            return await EnqueueWithPriorityAsync(message, (int)queueMessage.Priority, cancellationToken);
        }

        public Task<bool> EnqueueDelayedAsync<T>(T message, TimeSpan delay, CancellationToken cancellationToken = default) where T : class
        {
            if (message is not QueueMessage queueMessage)
            {
                _logger.LogError("Message must inherit from QueueMessage. Received: {MessageType}", typeof(T).Name);
                return Task.FromResult(false);
            }

            var wrapper = new QueueMessageWrapper
            {
                Message = queueMessage,
                MessageType = typeof(T),
                EnqueuedAt = DateTime.UtcNow,
                ProcessAfter = DateTime.UtcNow.Add(delay)
            };

            queueMessage.DelayUntil = delay;
            _delayedMessages[queueMessage.Id] = wrapper;

            _logger.LogDebug("Enqueued delayed message {MessageId} of type {MessageType} with delay {Delay}",
                queueMessage.Id, typeof(T).Name, delay);

            return Task.FromResult(true);
        }

        public async Task<bool> EnqueueWithPriorityAsync<T>(T message, int priority = 0, CancellationToken cancellationToken = default) where T : class
        {
            if (message is not QueueMessage queueMessage)
            {
                _logger.LogError("Message must inherit from QueueMessage. Received: {MessageType}", typeof(T).Name);
                return false;
            }

            var messagePriority = (MessagePriority)Math.Max(0, Math.Min(3, priority));
            queueMessage.Priority = messagePriority;

            var wrapper = new QueueMessageWrapper
            {
                Message = queueMessage,
                MessageType = typeof(T),
                EnqueuedAt = DateTime.UtcNow,
                ProcessAfter = DateTime.UtcNow
            };

            try
            {
                var channel = _priorityChannels[messagePriority];
                await channel.Writer.WriteAsync(wrapper, cancellationToken);

                _logger.LogDebug("Enqueued message {MessageId} of type {MessageType} with priority {Priority}",
                    queueMessage.Id, typeof(T).Name, messagePriority);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to enqueue message {MessageId} of type {MessageType}",
                    queueMessage.Id, typeof(T).Name);
                return false;
            }
        }

        public Task<int> GetQueueLengthAsync(CancellationToken cancellationToken = default)
        {
            var totalCount = 0;
            foreach (var channel in _priorityChannels.Values)
            {
                totalCount += channel.Reader.Count;
            }
            totalCount += _delayedMessages.Count;
            return Task.FromResult(totalCount);
        }

        public async Task<QueueHealthStatus> GetHealthStatusAsync(CancellationToken cancellationToken = default)
        {
            var pendingMessages = await GetQueueLengthAsync(cancellationToken);
            var errors = _recentErrors.ToArray();

            return new QueueHealthStatus
            {
                IsHealthy = errors.Length == 0 && pendingMessages < 10000, // Arbitrary threshold
                PendingMessages = pendingMessages,
                ProcessedMessages = (int)_processedMessages,
                FailedMessages = (int)_failedMessages,
                LastProcessedAt = _lastProcessedAt,
                Errors = errors
            };
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Starting InMemoryQueueService");
            
            _processingTask = Task.Run(() => ProcessMessagesAsync(_cancellationTokenSource.Token), cancellationToken);
            _delayedProcessingTask = Task.Run(() => ProcessDelayedMessagesAsync(_cancellationTokenSource.Token), cancellationToken);
            
            return Task.CompletedTask;
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Stopping InMemoryQueueService");
            
            _cancellationTokenSource.Cancel();
            _delayedMessageTimer?.Dispose();

            // Complete all channels safely
            foreach (var channel in _priorityChannels.Values)
            {
                try
                {
                    if (!channel.Writer.TryComplete())
                    {
                        // Channel was already completed, which is fine
                        _logger.LogDebug("Channel was already completed during shutdown");
                    }
                }
                catch (InvalidOperationException)
                {
                    // Channel was already closed, which is expected during shutdown
                    _logger.LogDebug("Channel was already closed during shutdown");
                }
            }

            // Wait for processing tasks to complete
            if (_processingTask != null)
                await _processingTask;
            if (_delayedProcessingTask != null)
                await _delayedProcessingTask;
        }

        private async Task ProcessMessagesAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Started message processing task");

            try
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    QueueMessageWrapper? wrapper = null;

                    // Process messages by priority (Critical -> High -> Normal -> Low)
                    var priorities = new[] { MessagePriority.Critical, MessagePriority.High, MessagePriority.Normal, MessagePriority.Low };
                    
                    foreach (var priority in priorities)
                    {
                        var channel = _priorityChannels[priority];
                        if (channel.Reader.TryRead(out wrapper))
                        {
                            break;
                        }
                    }

                    if (wrapper == null)
                    {
                        // No messages available, wait a bit
                        await Task.Delay(100, cancellationToken);
                        continue;
                    }

                    await ProcessMessageAsync(wrapper, cancellationToken);
                }
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("Message processing task was cancelled");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in message processing task");
            }
        }

        private async Task ProcessDelayedMessagesAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Started delayed message processing task");

            try
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    await Task.Delay(TimeSpan.FromSeconds(10), cancellationToken);
                    ProcessDelayedMessages(null);
                }
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("Delayed message processing task was cancelled");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in delayed message processing task");
            }
        }

        private void ProcessDelayedMessages(object? state)
        {
            var now = DateTime.UtcNow;
            var messagesToProcess = new List<QueueMessageWrapper>();

            foreach (var kvp in _delayedMessages)
            {
                if (kvp.Value.ProcessAfter <= now)
                {
                    messagesToProcess.Add(kvp.Value);
                }
            }

            foreach (var wrapper in messagesToProcess)
            {
                if (_delayedMessages.TryRemove(wrapper.Message.Id, out _))
                {
                    var priority = wrapper.Message.Priority;
                    var channel = _priorityChannels[priority];
                    
                    if (channel.Writer.TryWrite(wrapper))
                    {
                        _logger.LogDebug("Moved delayed message {MessageId} to processing queue", wrapper.Message.Id);
                    }
                    else
                    {
                        _logger.LogWarning("Failed to move delayed message {MessageId} to processing queue", wrapper.Message.Id);
                    }
                }
            }
        }

        private async Task ProcessMessageAsync(QueueMessageWrapper wrapper, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogDebug("Processing message {MessageId} of type {MessageType}",
                    wrapper.Message.Id, wrapper.MessageType.Name);

                using var scope = _serviceProvider.CreateScope();
                var handlerType = typeof(IQueueMessageHandler<>).MakeGenericType(wrapper.MessageType);
                var handler = scope.ServiceProvider.GetService(handlerType);

                if (handler == null)
                {
                    _logger.LogWarning("No handler found for message type {MessageType}", wrapper.MessageType.Name);
                    Interlocked.Increment(ref _failedMessages);
                    AddRecentError($"No handler found for message type {wrapper.MessageType.Name}");
                    return;
                }

                var handleMethod = handlerType.GetMethod("HandleAsync");
                if (handleMethod == null)
                {
                    _logger.LogError("HandleAsync method not found on handler for {MessageType}", wrapper.MessageType.Name);
                    Interlocked.Increment(ref _failedMessages);
                    AddRecentError($"HandleAsync method not found on handler for {wrapper.MessageType.Name}");
                    return;
                }

                var result = await (Task<bool>)handleMethod.Invoke(handler, new object[] { wrapper.Message, cancellationToken })!;

                if (result)
                {
                    Interlocked.Increment(ref _processedMessages);
                    _lastProcessedAt = DateTime.UtcNow;
                    _logger.LogDebug("Successfully processed message {MessageId}", wrapper.Message.Id);
                }
                else
                {
                    await HandleFailedMessage(wrapper, "Handler returned false", cancellationToken);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing message {MessageId} of type {MessageType}",
                    wrapper.Message.Id, wrapper.MessageType.Name);
                await HandleFailedMessage(wrapper, ex.Message, cancellationToken);
            }
        }

        private async Task HandleFailedMessage(QueueMessageWrapper wrapper, string error, CancellationToken cancellationToken)
        {
            wrapper.Message.RetryCount++;
            Interlocked.Increment(ref _failedMessages);
            AddRecentError($"Message {wrapper.Message.Id}: {error}");

            if (wrapper.Message.RetryCount < wrapper.Message.MaxRetries)
            {
                _logger.LogWarning("Retrying message {MessageId} (attempt {RetryCount}/{MaxRetries})",
                    wrapper.Message.Id, wrapper.Message.RetryCount, wrapper.Message.MaxRetries);

                // Exponential backoff for retries
                var delay = TimeSpan.FromSeconds(Math.Pow(2, wrapper.Message.RetryCount));
                await EnqueueDelayedAsync(wrapper.Message, delay, cancellationToken);
            }
            else
            {
                _logger.LogError("Message {MessageId} failed after {MaxRetries} attempts. Moving to dead letter queue.",
                    wrapper.Message.Id, wrapper.Message.MaxRetries);
                // In a real implementation, you'd move this to a dead letter queue
                // For now, we'll just log it
            }
        }

        private void AddRecentError(string error)
        {
            _recentErrors.Enqueue($"{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}: {error}");
            
            // Keep only last 100 errors
            while (_recentErrors.Count > 100)
            {
                _recentErrors.TryDequeue(out _);
            }
        }

        public void Dispose()
        {
            _cancellationTokenSource?.Cancel();
            _delayedMessageTimer?.Dispose();
            _cancellationTokenSource?.Dispose();
        }
    }

    public class QueueMessageWrapper
    {
        public required QueueMessage Message { get; set; }
        public required Type MessageType { get; set; }
        public DateTime EnqueuedAt { get; set; }
        public DateTime ProcessAfter { get; set; }
    }
} 