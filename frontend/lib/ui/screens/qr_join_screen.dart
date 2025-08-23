import 'package:flutter/material.dart';
import 'package:provider/provider.dart';
import '../../models/qr_code_models.dart';
import '../../models/public_salon.dart';
import '../../controllers/app_controller.dart';

import '../widgets/qr_scanner_widget.dart';
import '../widgets/bottom_nav_bar.dart';
import 'anonymous_join_queue_screen.dart';

/// Screen for joining queue via QR code scan
class QrJoinScreen extends StatefulWidget {
  final QrJoinData? initialQrData; // For direct URL navigation

  const QrJoinScreen({
    super.key,
    this.initialQrData,
  });

  @override
  State<QrJoinScreen> createState() => _QrJoinScreenState();
}

class _QrJoinScreenState extends State<QrJoinScreen> {
  QrJoinData? _scannedQrData;
  PublicSalon? _salon;
  bool _isLoading = false;
  String? _errorMessage;
  bool _showScanner = true;

  @override
  void initState() {
    super.initState();
    if (widget.initialQrData != null) {
      _initializeQrData();
    }
  }

  Future<void> _initializeQrData() async {
    await _handleQrData(widget.initialQrData!);
  }

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);
    
    return Scaffold(
      backgroundColor: theme.colorScheme.surface,
      appBar: AppBar(
        title: const Text('Join Queue'),
        backgroundColor: theme.colorScheme.surface,
        foregroundColor: theme.colorScheme.onSurface,
        elevation: 0,
      ),
      body: SafeArea(
        child: _buildBody(context),
      ),
      bottomNavigationBar: const BottomNavBar(currentIndex: 0),
    );
  }

  Widget _buildBody(BuildContext context) {
    if (_isLoading) {
      return _buildLoadingState(context);
    }

    if (_errorMessage != null) {
      return _buildErrorState(context);
    }

    if (_scannedQrData != null && _salon != null) {
      return _buildJoinConfirmation(context);
    }

    if (_showScanner) {
      return _buildScannerView(context);
    }

    return _buildInitialState(context);
  }

  Widget _buildScannerView(BuildContext context) {
    return Column(
      children: [
        Expanded(
          child: QrScannerWidget(
            onQrScanned: _handleQrScanResult,
            onPermissionDenied: () {
              setState(() {
                _errorMessage = 'Camera permission is required to scan QR codes';
                _showScanner = false;
              });
            },
            instructionText: 'Scan the salon\'s QR code',
          ),
        ),
        Container(
          padding: const EdgeInsets.all(16),
          child: Column(
            children: [
              Text(
                'Point your camera at the QR code provided by the salon',
                style: Theme.of(context).textTheme.bodyMedium?.copyWith(
                  color: Theme.of(context).colorScheme.onSurfaceVariant,
                ),
                textAlign: TextAlign.center,
              ),
              const SizedBox(height: 16),
              TextButton(
                onPressed: () {
                  setState(() {
                    _showScanner = false;
                  });
                },
                child: const Text('Enter code manually'),
              ),
            ],
          ),
        ),
      ],
    );
  }

  Widget _buildInitialState(BuildContext context) {
    final theme = Theme.of(context);
    
    return Padding(
      padding: const EdgeInsets.all(24),
      child: Column(
        mainAxisAlignment: MainAxisAlignment.center,
        children: [
          Icon(
            Icons.qr_code_scanner,
            size: 80,
            color: theme.colorScheme.primary,
          ),
          const SizedBox(height: 24),
          Text(
            'Join Queue with QR Code',
            style: theme.textTheme.headlineSmall?.copyWith(
              fontWeight: FontWeight.bold,
            ),
            textAlign: TextAlign.center,
          ),
          const SizedBox(height: 16),
          Text(
            'Scan the QR code displayed at the salon to quickly join their queue',
            style: theme.textTheme.bodyLarge?.copyWith(
              color: theme.colorScheme.onSurfaceVariant,
            ),
            textAlign: TextAlign.center,
          ),
          const SizedBox(height: 32),
          SizedBox(
            width: double.infinity,
            child: ElevatedButton.icon(
              onPressed: () {
                setState(() {
                  _showScanner = true;
                });
              },
              icon: const Icon(Icons.qr_code_scanner),
              label: const Text('Scan QR Code'),
              style: ElevatedButton.styleFrom(
                padding: const EdgeInsets.symmetric(vertical: 16),
              ),
            ),
          ),
          const SizedBox(height: 16),
          SizedBox(
            width: double.infinity,
            child: OutlinedButton.icon(
              onPressed: _showManualEntryDialog,
              icon: const Icon(Icons.keyboard),
              label: const Text('Enter Code Manually'),
              style: OutlinedButton.styleFrom(
                padding: const EdgeInsets.symmetric(vertical: 16),
              ),
            ),
          ),
        ],
      ),
    );
  }

  Widget _buildLoadingState(BuildContext context) {
    return const Center(
      child: Column(
        mainAxisAlignment: MainAxisAlignment.center,
        children: [
          CircularProgressIndicator(),
          SizedBox(height: 16),
          Text('Loading salon information...'),
        ],
      ),
    );
  }

  Widget _buildErrorState(BuildContext context) {
    final theme = Theme.of(context);
    
    return Padding(
      padding: const EdgeInsets.all(24),
      child: Column(
        mainAxisAlignment: MainAxisAlignment.center,
        children: [
          Icon(
            Icons.error_outline,
            size: 64,
            color: theme.colorScheme.error,
          ),
          const SizedBox(height: 16),
          Text(
            'Unable to Join Queue',
            style: theme.textTheme.headlineSmall?.copyWith(
              fontWeight: FontWeight.bold,
              color: theme.colorScheme.error,
            ),
            textAlign: TextAlign.center,
          ),
          const SizedBox(height: 8),
          Text(
            _errorMessage!,
            style: theme.textTheme.bodyMedium?.copyWith(
              color: theme.colorScheme.onSurfaceVariant,
            ),
            textAlign: TextAlign.center,
          ),
          const SizedBox(height: 24),
          Row(
            children: [
              Expanded(
                child: OutlinedButton(
                  onPressed: () {
                    setState(() {
                      _errorMessage = null;
                      _showScanner = true;
                    });
                  },
                  child: const Text('Try Again'),
                ),
              ),
              const SizedBox(width: 16),
              Expanded(
                child: ElevatedButton(
                  onPressed: () => Navigator.of(context).pop(),
                  child: const Text('Go Back'),
                ),
              ),
            ],
          ),
        ],
      ),
    );
  }

  Widget _buildJoinConfirmation(BuildContext context) {
    final theme = Theme.of(context);
    final qrData = _scannedQrData!;
    final salon = _salon!;
    
    return Padding(
      padding: const EdgeInsets.all(24),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          // QR Code Status
          Container(
            padding: const EdgeInsets.all(16),
            decoration: BoxDecoration(
              color: theme.colorScheme.primaryContainer,
              borderRadius: BorderRadius.circular(12),
            ),
            child: Row(
              children: [
                Icon(
                  Icons.qr_code,
                  color: theme.colorScheme.onPrimaryContainer,
                ),
                const SizedBox(width: 12),
                Expanded(
                  child: Column(
                    crossAxisAlignment: CrossAxisAlignment.start,
                    children: [
                      Text(
                        'QR Code Scanned Successfully',
                        style: theme.textTheme.titleMedium?.copyWith(
                          fontWeight: FontWeight.bold,
                          color: theme.colorScheme.onPrimaryContainer,
                        ),
                      ),
                      if (qrData.formattedExpiryTime != null)
                        Text(
                          'Expires in ${qrData.formattedExpiryTime}',
                          style: theme.textTheme.bodySmall?.copyWith(
                            color: theme.colorScheme.onPrimaryContainer,
                          ),
                        ),
                    ],
                  ),
                ),
              ],
            ),
          ),
          const SizedBox(height: 24),
          
          // Salon Information
          Text(
            'Join Queue At:',
            style: theme.textTheme.labelLarge?.copyWith(
              color: theme.colorScheme.onSurfaceVariant,
            ),
          ),
          const SizedBox(height: 8),
          Card(
            child: Padding(
              padding: const EdgeInsets.all(16),
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  Text(
                    salon.name,
                    style: theme.textTheme.titleLarge?.copyWith(
                      fontWeight: FontWeight.bold,
                    ),
                  ),
                  const SizedBox(height: 8),
                  Row(
                    children: [
                      Icon(
                        Icons.location_on,
                        size: 16,
                        color: theme.colorScheme.onSurfaceVariant,
                      ),
                      const SizedBox(width: 4),
                      Expanded(
                        child: Text(
                          salon.address,
                          style: theme.textTheme.bodyMedium?.copyWith(
                            color: theme.colorScheme.onSurfaceVariant,
                          ),
                        ),
                      ),
                    ],
                  ),
                  const SizedBox(height: 8),
                  Row(
                    children: [
                      Icon(
                        Icons.people,
                        size: 16,
                        color: theme.colorScheme.onSurfaceVariant,
                      ),
                      const SizedBox(width: 4),
                      Text(
                        '${salon.queueLength} people in queue',
                        style: theme.textTheme.bodyMedium?.copyWith(
                          color: theme.colorScheme.onSurfaceVariant,
                        ),
                      ),
                      const Spacer(),
                      Container(
                        padding: const EdgeInsets.symmetric(
                          horizontal: 8,
                          vertical: 4,
                        ),
                        decoration: BoxDecoration(
                          color: salon.isOpen ? Colors.green : Colors.red,
                          borderRadius: BorderRadius.circular(12),
                        ),
                        child: Text(
                          salon.isOpen ? 'Open' : 'Closed',
                          style: theme.textTheme.bodySmall?.copyWith(
                            color: Colors.white,
                            fontWeight: FontWeight.bold,
                          ),
                        ),
                      ),
                    ],
                  ),
                ],
              ),
            ),
          ),
          
          const Spacer(),
          
          // Join Button
          SizedBox(
            width: double.infinity,
            child: ElevatedButton(
              onPressed: salon.isOpen ? _joinQueue : null,
              style: ElevatedButton.styleFrom(
                padding: const EdgeInsets.symmetric(vertical: 16),
              ),
              child: Text(
                salon.isOpen ? 'Join Queue' : 'Salon is Closed',
                style: const TextStyle(
                  fontSize: 16,
                  fontWeight: FontWeight.bold,
                ),
              ),
            ),
          ),
          const SizedBox(height: 16),
          SizedBox(
            width: double.infinity,
            child: OutlinedButton(
              onPressed: () {
                setState(() {
                  _scannedQrData = null;
                  _salon = null;
                  _showScanner = true;
                });
              },
              child: const Text('Scan Another Code'),
            ),
          ),
        ],
      ),
    );
  }

  Future<void> _handleQrScanResult(QrScanResult result) async {
    if (!result.isValid) {
      setState(() {
        _errorMessage = result.errorMessage ?? 'Invalid QR code';
        _showScanner = false;
      });
      return;
    }

    await _handleQrData(result.joinData!);
  }

  Future<void> _handleQrData(QrJoinData qrData) async {
    setState(() {
      _isLoading = true;
      _errorMessage = null;
      _scannedQrData = qrData;
    });

    try {
      // Get salon information
      final appController = Provider.of<AppController>(context, listen: false);
      await appController.anonymous.loadPublicSalons();
      
      // Get salons from controller state
      final salons = appController.anonymous.nearbySalons;
      
      // Find the salon by location ID
      final salon = salons.firstWhere(
        (s) => s.id == qrData.locationId,
        orElse: () => throw Exception('Salon not found'),
      );

      setState(() {
        _salon = salon;
        _isLoading = false;
        _showScanner = false;
      });
    } catch (e) {
      setState(() {
        _errorMessage = 'Unable to load salon information: ${e.toString()}';
        _isLoading = false;
        _showScanner = false;
      });
    }
  }

  Future<void> _joinQueue() async {
    if (_salon == null || _scannedQrData == null) return;

    try {
      // Navigate to the anonymous join queue screen
      Navigator.of(context).pushReplacement(
        MaterialPageRoute(
          builder: (context) => AnonymousJoinQueueScreen(
            salon: _salon!,
          ),
        ),
      );
    } catch (e) {
      ScaffoldMessenger.of(context).showSnackBar(
        SnackBar(
          content: Text('Failed to join queue: ${e.toString()}'),
          backgroundColor: Theme.of(context).colorScheme.error,
        ),
      );
    }
  }

  void _showManualEntryDialog() {
    final controller = TextEditingController();
    
    showDialog(
      context: context,
      builder: (context) => AlertDialog(
        title: const Text('Enter QR Code URL'),
        content: Column(
          mainAxisSize: MainAxisSize.min,
          children: [
            const Text(
              'Paste the QR code URL or enter the salon code manually:',
            ),
            const SizedBox(height: 16),
            TextField(
              controller: controller,
              decoration: const InputDecoration(
                hintText: 'https://app.eutonafila.com/join?locationId=...',
                border: OutlineInputBorder(),
              ),
              maxLines: 3,
            ),
          ],
        ),
        actions: [
          TextButton(
            onPressed: () => Navigator.of(context).pop(),
            child: const Text('Cancel'),
          ),
          ElevatedButton(
            onPressed: () async {
              final url = controller.text.trim();
              if (url.isNotEmpty) {
                Navigator.of(context).pop();
                final qrData = QrJoinData.fromUrl(url);
                if (qrData != null) {
                  await _handleQrData(qrData);
                } else {
                  setState(() {
                    _errorMessage = 'Invalid URL format';
                  });
                }
              }
            },
            child: const Text('Join'),
          ),
        ],
      ),
    );
  }
}
