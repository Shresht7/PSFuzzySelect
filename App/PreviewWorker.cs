using System.Collections.Concurrent;
using System.Management.Automation;
using System.Management.Automation.Runspaces;

namespace PSFuzzySelect.App;

/// <summary>
/// Generates preview text on a background thread by invoking a PowerShell script per highlighted item.
/// </summary>
sealed class PreviewWorker : IDisposable
{
    private Runspace _runspace;

    private readonly BlockingCollection<object> _requests = new();

    private Thread _workerThread;

    private CancellationTokenSource _cts = new();

    private PowerShell? _powerShell;

    private readonly object _psLock = new();

    private ScriptBlock _scriptBlock;

    private readonly Action<Message> _enqueueMessage;

    private bool disposedValue;

    private volatile bool _isDisposing;

    /// <summary>
    /// Initializes a new preview worker and starts its background loop.
    /// </summary>
    /// <param name="scriptBlock">Script used to produce preview text for each item.</param>
    /// <param name="messageQueueCallback">Callback used to send UI update messages back to the engine.</param>
    public PreviewWorker(ScriptBlock scriptBlock, Action<Message> messageQueueCallback)
    {
        _scriptBlock = scriptBlock;
        _enqueueMessage = messageQueueCallback;

        _runspace = RunspaceFactory.CreateRunspace();
        _runspace.Open();

        _workerThread = new Thread(WorkerLoop) { IsBackground = true };
        _workerThread.Start();
    }

    /// <summary>
    /// Queues an item for preview generation.
    /// Calls made during teardown are safely ignored.
    /// </summary>
    /// <param name="item">The item to preview.</param>
    public void Enqueue(object item)
    {
        if (disposedValue || _isDisposing || _cts.IsCancellationRequested)
        {
            return;
        }

        try
        {
            _requests.Add(item, _cts.Token);
        }
        catch (ObjectDisposedException)
        {
            // Worker resources are already torn down.
        }
        catch (InvalidOperationException)
        {
            // Queue was completed while enqueueing.
        }
        catch (OperationCanceledException)
        {
            // Cancellation started while enqueueing.
        }
    }

    /// <summary>
    /// Continuously processes queued preview requests until cancellation.
    /// </summary>
    private void WorkerLoop()
    {
        try
        {
            foreach (var item in _requests.GetConsumingEnumerable(_cts.Token))
            {
                if (_cts.Token.IsCancellationRequested) break;

                var selected = item;
                // Debounce: get the latest result
                while (_requests.TryTake(out var latest))
                {
                    selected = latest;
                }

                lock (_psLock)
                {
                    if (_cts.Token.IsCancellationRequested) break;
                    _powerShell = PowerShell.Create();
                    _powerShell.Runspace = _runspace;
                }

                try
                {
                    _powerShell.AddScript("param($item, $s) $item | ForEach-Object { & ([scriptblock]::Create($s)) $_ } | Out-String")
                        .AddArgument(selected)
                        .AddArgument(_scriptBlock.ToString());


                    var result = _powerShell.BeginInvoke();

                    // Wait for completion, cancellation or timeout
                    int waitIdx = WaitHandle.WaitAny(new[] { result.AsyncWaitHandle, _cts.Token.WaitHandle }, 2000);

                    if (waitIdx == 0) // Completed
                    {
                        var output = _powerShell.EndInvoke(result);
                        string content = string.Join(Environment.NewLine, output);
                        _enqueueMessage(new UpdatePreview(content));
                    }
                    else // Timeout or cancelled
                    {
                        _powerShell?.Stop();
                        if (waitIdx == WaitHandle.WaitTimeout)
                        {
                            _enqueueMessage(new UpdatePreview("Preview timed out."));
                        }
                    }
                }
                catch (Exception ex)
                {
                    if (!_cts.Token.IsCancellationRequested)
                    {
                        _enqueueMessage(new UpdatePreview($"Error in preview generation: {ex.Message}"));
                    }
                }
                finally
                {
                    lock (_psLock)
                    {
                        _powerShell?.Dispose();
                        _powerShell = null;
                    }
                }
            }
        }
        catch (OperationCanceledException) { }
    }

    /// <summary>
    /// Releases managed resources and stops the background worker.
    /// </summary>
    /// <param name="disposing">True when called from <see cref="Dispose()"/>.</param>
    private void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                _isDisposing = true;
                _cts.Cancel(); // Signal the worker thread to stop processing new requests and exit
                _requests.CompleteAdding(); // Indicate that no more requests will be added to the queue

                // Attempt to stop any running PowerShell instance gracefully, with a lock to ensure thread safety
                lock (_psLock)
                {
                    try
                    {
                        _powerShell?.Stop();
                        _powerShell?.Dispose();
                        _powerShell = null;
                    }
                    catch { /* try to stop but ignore any exceptions */ }
                }

                // Wait for the worker thread to finish processing any remaining requests and exit gracefully,
                // with a timeout to prevent hanging indefinitely
                _workerThread.Join(5000);

                // Dispose and clean up resources used by the worker
                _requests.Dispose();
                _runspace.Close();
                _runspace.Dispose();
                _cts.Dispose();
            }

            disposedValue = true;
        }
    }

    // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
    // ~PreviewWorker()
    // {
    //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
    //     Dispose(disposing: false);
    // }

    public void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}
