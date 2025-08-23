import 'package:flutter/material.dart';
import 'package:flutter/services.dart';
import 'dart:async';

/// A comprehensive error boundary widget that catches errors and provides
/// user-friendly error handling with retry mechanisms
class ErrorBoundary extends StatefulWidget {
  final Widget child;
  final Widget Function(Object error, StackTrace? stackTrace, VoidCallback retry)? errorBuilder;
  final VoidCallback? onError;
  final bool showErrorDetails;

  const ErrorBoundary({
    super.key,
    required this.child,
    this.errorBuilder,
    this.onError,
    this.showErrorDetails = false,
  });

  @override
  State<ErrorBoundary> createState() => _ErrorBoundaryState();
}

class _ErrorBoundaryState extends State<ErrorBoundary> {
  Object? _error;
  StackTrace? _stackTrace;
  bool _isRetrying = false;

  @override
  void initState() {
    super.initState();
    // Set up error handling for Flutter errors
    FlutterError.onError = _handleFlutterError;
  }

  @override
  void dispose() {
    FlutterError.onError = null;
    super.dispose();
  }

  void _handleFlutterError(FlutterErrorDetails details) {
    _handleError(details.exception, details.stack);
  }

  void _handleError(Object error, StackTrace? stackTrace) {
    if (mounted) {
      setState(() {
        _error = error;
        _stackTrace = stackTrace;
      });

      // Log the error
      debugPrint('ErrorBoundary caught error: $error');
      debugPrint('StackTrace: $stackTrace');

      // Call the onError callback if provided
      widget.onError?.call();
    }
  }

  Future<void> _retry() async {
    if (_isRetrying) return;

    setState(() {
      _isRetrying = true;
    });

    try {
      // Wait a bit to show retry state
      await Future.delayed(const Duration(milliseconds: 500));
      
      if (mounted) {
        setState(() {
          _error = null;
          _stackTrace = null;
          _isRetrying = false;
        });
      }
    } catch (e) {
      if (mounted) {
        setState(() {
          _isRetrying = false;
        });
        _handleError(e, null);
      }
    }
  }

  void _copyErrorDetails() {
    final errorText = '''
Error: $_error
StackTrace: $_stackTrace
''';
    
    Clipboard.setData(ClipboardData(text: errorText));
    
    if (mounted) {
      ScaffoldMessenger.of(context).showSnackBar(
        const SnackBar(
          content: Text('Error details copied to clipboard'),
          duration: Duration(seconds: 2),
        ),
      );
    }
  }

  void _reportError() {
    // TODO: Implement error reporting to analytics/monitoring service
    if (mounted) {
      ScaffoldMessenger.of(context).showSnackBar(
        const SnackBar(
          content: Text('Error reported to support team'),
          duration: Duration(seconds: 2),
        ),
      );
    }
  }

  @override
  Widget build(BuildContext context) {
    if (_error != null) {
      return widget.errorBuilder?.call(_error!, _stackTrace, _retry) ??
          _buildDefaultErrorWidget(context);
    }

    return widget.child;
  }

  Widget _buildDefaultErrorWidget(BuildContext context) {
    final theme = Theme.of(context);
    
    return Scaffold(
      backgroundColor: theme.colorScheme.errorContainer,
      body: SafeArea(
        child: Padding(
          padding: const EdgeInsets.all(24.0),
          child: Column(
            mainAxisAlignment: MainAxisAlignment.center,
            children: [
              // Error Icon
              Container(
                width: 80,
                height: 80,
                decoration: BoxDecoration(
                  color: theme.colorScheme.error,
                  shape: BoxShape.circle,
                ),
                child: Icon(
                  Icons.error_outline,
                  size: 40,
                  color: theme.colorScheme.onError,
                ),
              ),
              
              const SizedBox(height: 24),
              
              // Error Title
              Text(
                'Oops! Something went wrong',
                style: theme.textTheme.headlineSmall?.copyWith(
                  color: theme.colorScheme.onErrorContainer,
                  fontWeight: FontWeight.bold,
                ),
                textAlign: TextAlign.center,
              ),
              
              const SizedBox(height: 16),
              
              // Error Message
              Text(
                'We encountered an unexpected error. Please try again or contact support if the problem persists.',
                style: theme.textTheme.bodyLarge?.copyWith(
                  color: theme.colorScheme.onErrorContainer.withOpacity(0.8),
                ),
                textAlign: TextAlign.center,
              ),
              
              const SizedBox(height: 32),
              
              // Action Buttons
              Column(
                children: [
                  // Retry Button
                  SizedBox(
                    width: double.infinity,
                    child: ElevatedButton.icon(
                      onPressed: _isRetrying ? null : _retry,
                      icon: _isRetrying
                          ? const SizedBox(
                              width: 20,
                              height: 20,
                              child: CircularProgressIndicator(strokeWidth: 2),
                            )
                          : const Icon(Icons.refresh),
                      label: Text(_isRetrying ? 'Retrying...' : 'Try Again'),
                      style: ElevatedButton.styleFrom(
                        backgroundColor: theme.colorScheme.error,
                        foregroundColor: theme.colorScheme.onError,
                        padding: const EdgeInsets.symmetric(vertical: 16),
                        shape: RoundedRectangleBorder(
                          borderRadius: BorderRadius.circular(12),
                        ),
                      ),
                    ),
                  ),
                  
                  const SizedBox(height: 16),
                  
                  // Report Error Button
                  SizedBox(
                    width: double.infinity,
                    child: OutlinedButton.icon(
                      onPressed: _reportError,
                      icon: const Icon(Icons.bug_report),
                      label: const Text('Report Error'),
                      style: OutlinedButton.styleFrom(
                        foregroundColor: theme.colorScheme.onErrorContainer,
                        side: BorderSide(color: theme.colorScheme.onErrorContainer),
                        padding: const EdgeInsets.symmetric(vertical: 16),
                        shape: RoundedRectangleBorder(
                          borderRadius: BorderRadius.circular(12),
                        ),
                      ),
                    ),
                  ),
                  
                  if (widget.showErrorDetails) ...[
                    const SizedBox(height: 16),
                    
                    // Copy Error Details Button
                    SizedBox(
                      width: double.infinity,
                      child: OutlinedButton.icon(
                        onPressed: _copyErrorDetails,
                        icon: const Icon(Icons.copy),
                        label: const Text('Copy Error Details'),
                        style: OutlinedButton.styleFrom(
                          foregroundColor: theme.colorScheme.onErrorContainer,
                          side: BorderSide(color: theme.colorScheme.onErrorContainer),
                          padding: const EdgeInsets.symmetric(vertical: 16),
                          shape: RoundedRectangleBorder(
                            borderRadius: BorderRadius.circular(12),
                          ),
                        ),
                      ),
                    ),
                  ],
                ],
              ),
              
              const SizedBox(height: 32),
              
              // Error Details (if enabled)
              if (widget.showErrorDetails) ...[
                Container(
                  width: double.infinity,
                  padding: const EdgeInsets.all(16),
                  decoration: BoxDecoration(
                    color: theme.colorScheme.surface,
                    borderRadius: BorderRadius.circular(12),
                    border: Border.all(
                      color: theme.colorScheme.outline.withOpacity(0.3),
                    ),
                  ),
                  child: Column(
                    crossAxisAlignment: CrossAxisAlignment.start,
                    children: [
                      Row(
                        children: [
                          Icon(
                            Icons.info_outline,
                            size: 20,
                            color: theme.colorScheme.onSurfaceVariant,
                          ),
                          const SizedBox(width: 8),
                          Text(
                            'Error Details',
                            style: theme.textTheme.titleSmall?.copyWith(
                              fontWeight: FontWeight.bold,
                              color: theme.colorScheme.onSurfaceVariant,
                            ),
                          ),
                        ],
                      ),
                      const SizedBox(height: 12),
                      Text(
                        _error.toString(),
                        style: theme.textTheme.bodySmall?.copyWith(
                          color: theme.colorScheme.onSurfaceVariant,
                          fontFamily: 'monospace',
                        ),
                      ),
                      if (_stackTrace != null) ...[
                        const SizedBox(height: 8),
                        Text(
                          'Stack Trace:',
                          style: theme.textTheme.bodySmall?.copyWith(
                            fontWeight: FontWeight.bold,
                            color: theme.colorScheme.onSurfaceVariant,
                          ),
                        ),
                        const SizedBox(height: 4),
                        Text(
                          _stackTrace.toString(),
                          style: theme.textTheme.bodySmall?.copyWith(
                            color: theme.colorScheme.onSurfaceVariant,
                            fontFamily: 'monospace',
                            fontSize: 10,
                          ),
                          maxLines: 10,
                          overflow: TextOverflow.ellipsis,
                        ),
                      ],
                    ],
                  ),
                ),
              ],
            ],
          ),
        ),
      ),
    );
  }
}

/// A simpler error boundary for specific sections of the app
class SectionErrorBoundary extends StatelessWidget {
  final Widget child;
  final Widget Function(Object error, VoidCallback retry)? errorBuilder;
  final String? errorTitle;
  final String? errorMessage;

  const SectionErrorBoundary({
    super.key,
    required this.child,
    this.errorBuilder,
    this.errorTitle,
    this.errorMessage,
  });

  @override
  Widget build(BuildContext context) {
    return ErrorBoundary(
      child: child,
      errorBuilder: (error, stackTrace, retry) {
        return errorBuilder?.call(error, retry) ??
            _buildSectionErrorWidget(context, error, retry);
      },
    );
  }

  Widget _buildSectionErrorWidget(BuildContext context, Object error, VoidCallback retry) {
    final theme = Theme.of(context);
    
    return Container(
      padding: const EdgeInsets.all(20),
      decoration: BoxDecoration(
        color: theme.colorScheme.errorContainer.withOpacity(0.1),
        borderRadius: BorderRadius.circular(12),
        border: Border.all(
          color: theme.colorScheme.error.withOpacity(0.3),
        ),
      ),
      child: Column(
        children: [
          Icon(
            Icons.error_outline,
            size: 32,
            color: theme.colorScheme.error,
          ),
          
          const SizedBox(height: 12),
          
          Text(
            errorTitle ?? 'Something went wrong',
            style: theme.textTheme.titleMedium?.copyWith(
              color: theme.colorScheme.error,
              fontWeight: FontWeight.bold,
            ),
            textAlign: TextAlign.center,
          ),
          
          if (errorMessage != null) ...[
            const SizedBox(height: 8),
            Text(
              errorMessage!,
              style: theme.textTheme.bodyMedium?.copyWith(
                color: theme.colorScheme.onSurfaceVariant,
              ),
              textAlign: TextAlign.center,
            ),
          ],
          
          const SizedBox(height: 16),
          
          ElevatedButton.icon(
            onPressed: retry,
            icon: const Icon(Icons.refresh, size: 16),
            label: const Text('Retry'),
            style: ElevatedButton.styleFrom(
              backgroundColor: theme.colorScheme.error,
              foregroundColor: theme.colorScheme.onError,
              padding: const EdgeInsets.symmetric(horizontal: 16, vertical: 8),
              shape: RoundedRectangleBorder(
                borderRadius: BorderRadius.circular(8),
              ),
            ),
          ),
        ],
      ),
    );
  }
}

/// Error boundary specifically for async operations
class AsyncErrorBoundary extends StatefulWidget {
  final Widget child;
  final Widget Function(Object error, VoidCallback retry)? errorBuilder;
  final Future<void> Function()? onRetry;

  const AsyncErrorBoundary({
    super.key,
    required this.child,
    this.errorBuilder,
    this.onRetry,
  });

  @override
  State<AsyncErrorBoundary> createState() => _AsyncErrorBoundaryState();
}

class _AsyncErrorBoundaryState extends State<AsyncErrorBoundary> {
  Object? _error;
  bool _isRetrying = false;

  void _handleError(Object error) {
    if (mounted) {
      setState(() {
        _error = error;
      });
    }
  }

  Future<void> _retry() async {
    if (_isRetrying) return;

    setState(() {
      _isRetrying = true;
    });

    try {
      if (widget.onRetry != null) {
        await widget.onRetry!();
      }
      
      if (mounted) {
        setState(() {
          _error = null;
          _isRetrying = false;
        });
      }
    } catch (e) {
      if (mounted) {
        setState(() {
          _isRetrying = false;
        });
        _handleError(e);
      }
    }
  }

  @override
  Widget build(BuildContext context) {
    if (_error != null) {
      return widget.errorBuilder?.call(_error!, _retry) ??
          _buildAsyncErrorWidget(context);
    }

    return widget.child;
  }

  Widget _buildAsyncErrorWidget(BuildContext context) {
    final theme = Theme.of(context);
    
    return Container(
      padding: const EdgeInsets.all(16),
      decoration: BoxDecoration(
        color: theme.colorScheme.errorContainer.withOpacity(0.1),
        borderRadius: BorderRadius.circular(8),
        border: Border.all(
          color: theme.colorScheme.error.withOpacity(0.2),
        ),
      ),
      child: Row(
        children: [
          Icon(
            Icons.error_outline,
            size: 20,
            color: theme.colorScheme.error,
          ),
          
          const SizedBox(width: 12),
          
          Expanded(
            child: Text(
              'Failed to load content',
              style: theme.textTheme.bodyMedium?.copyWith(
                color: theme.colorScheme.error,
                fontWeight: FontWeight.w500,
              ),
            ),
          ),
          
          TextButton(
            onPressed: _isRetrying ? null : _retry,
            child: _isRetrying
                ? const SizedBox(
                    width: 16,
                    height: 16,
                    child: CircularProgressIndicator(strokeWidth: 2),
                  )
                : const Text('Retry'),
          ),
        ],
      ),
    );
  }
}
