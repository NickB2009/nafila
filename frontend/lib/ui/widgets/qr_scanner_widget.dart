import 'package:flutter/material.dart';
import 'package:mobile_scanner/mobile_scanner.dart';
import '../../models/qr_code_models.dart';
import '../../services/qr_code_service.dart';

/// Widget for scanning QR codes with camera
class QrScannerWidget extends StatefulWidget {
  final Function(QrScanResult) onQrScanned;
  final VoidCallback? onPermissionDenied;
  final String? instructionText;
  final bool showFlashToggle;
  final bool showGalleryButton;

  const QrScannerWidget({
    super.key,
    required this.onQrScanned,
    this.onPermissionDenied,
    this.instructionText,
    this.showFlashToggle = true,
    this.showGalleryButton = false,
  });

  @override
  State<QrScannerWidget> createState() => _QrScannerWidgetState();
}

class _QrScannerWidgetState extends State<QrScannerWidget> {
  MobileScannerController? controller;
  QrCodeService? _qrService;
  bool _isFlashOn = false;
  bool _hasPermission = false;
  bool _isScanning = true;

  @override
  void initState() {
    super.initState();
    controller = MobileScannerController();
    _initializeQrService();
    _checkPermission();
  }

  Future<void> _initializeQrService() async {
    _qrService = await QrCodeService.create();
  }

  Future<void> _checkPermission() async {
    if (_qrService == null) return;
    
    final hasPermission = await _qrService!.checkCameraPermission();
    if (!hasPermission) {
      final granted = await _qrService!.requestCameraPermission();
      if (!granted) {
        widget.onPermissionDenied?.call();
        return;
      }
    }
    
    setState(() {
      _hasPermission = true;
    });
  }

  @override
  void reassemble() {
    super.reassemble();
    if (controller != null) {
      controller!.stop();
      controller!.start();
    }
  }

  @override
  Widget build(BuildContext context) {
    if (!_hasPermission) {
      return _buildPermissionDenied(context);
    }

    return Stack(
      children: [
        MobileScanner(
          controller: controller,
          onDetect: (capture) {
            final List<Barcode> barcodes = capture.barcodes;
            for (final barcode in barcodes) {
              if (barcode.rawValue != null && _isScanning) {
                _handleQrScanned(barcode.rawValue!);
                break;
              }
            }
          },
        ),
        _buildOverlay(context),
      ],
    );
  }

  Widget _buildPermissionDenied(BuildContext context) {
    final theme = Theme.of(context);
    
    return Container(
      padding: const EdgeInsets.all(24),
      child: Column(
        mainAxisAlignment: MainAxisAlignment.center,
        children: [
          Icon(
            Icons.camera_alt_outlined,
            size: 64,
            color: theme.colorScheme.onSurfaceVariant,
          ),
          const SizedBox(height: 16),
          Text(
            'Camera Permission Required',
            style: theme.textTheme.headlineSmall?.copyWith(
              fontWeight: FontWeight.bold,
            ),
            textAlign: TextAlign.center,
          ),
          const SizedBox(height: 8),
          Text(
            'Please allow camera access to scan QR codes',
            style: theme.textTheme.bodyMedium?.copyWith(
              color: theme.colorScheme.onSurfaceVariant,
            ),
            textAlign: TextAlign.center,
          ),
          const SizedBox(height: 24),
          ElevatedButton(
            onPressed: _checkPermission,
            child: const Text('Grant Permission'),
          ),
        ],
      ),
    );
  }

  Widget _buildOverlay(BuildContext context) {
    final theme = Theme.of(context);
    
    return Column(
      children: [
        // Top overlay with instructions
        Container(
          width: double.infinity,
          padding: const EdgeInsets.all(24),
          decoration: BoxDecoration(
            gradient: LinearGradient(
              begin: Alignment.topCenter,
              end: Alignment.bottomCenter,
              colors: [
                Colors.black.withOpacity(0.7),
                Colors.transparent,
              ],
            ),
          ),
          child: SafeArea(
            child: Column(
              children: [
                Text(
                  widget.instructionText ?? 'Scan QR Code',
                  style: theme.textTheme.titleLarge?.copyWith(
                    color: Colors.white,
                    fontWeight: FontWeight.bold,
                  ),
                  textAlign: TextAlign.center,
                ),
                const SizedBox(height: 8),
                Text(
                  'Point your camera at the QR code',
                  style: theme.textTheme.bodyMedium?.copyWith(
                    color: Colors.white70,
                  ),
                  textAlign: TextAlign.center,
                ),
              ],
            ),
          ),
        ),
        const Spacer(),
        // Bottom overlay with controls
        Container(
          width: double.infinity,
          padding: const EdgeInsets.all(24),
          decoration: BoxDecoration(
            gradient: LinearGradient(
              begin: Alignment.bottomCenter,
              end: Alignment.topCenter,
              colors: [
                Colors.black.withOpacity(0.7),
                Colors.transparent,
              ],
            ),
          ),
          child: SafeArea(
            child: Row(
              mainAxisAlignment: MainAxisAlignment.spaceEvenly,
              children: [
                if (widget.showFlashToggle)
                  _buildControlButton(
                    context,
                    icon: _isFlashOn ? Icons.flash_on : Icons.flash_off,
                    label: 'Flash',
                    onPressed: _toggleFlash,
                  ),
                _buildControlButton(
                  context,
                  icon: _isScanning ? Icons.pause : Icons.play_arrow,
                  label: _isScanning ? 'Pause' : 'Resume',
                  onPressed: _toggleScanning,
                ),
                if (widget.showGalleryButton)
                  _buildControlButton(
                    context,
                    icon: Icons.photo_library,
                    label: 'Gallery',
                    onPressed: _openGallery,
                  ),
              ],
            ),
          ),
        ),
      ],
    );
  }

  Widget _buildControlButton(
    BuildContext context, {
    required IconData icon,
    required String label,
    required VoidCallback onPressed,
  }) {
    return Column(
      mainAxisSize: MainAxisSize.min,
      children: [
        Container(
          decoration: BoxDecoration(
            color: Colors.white.withOpacity(0.2),
            shape: BoxShape.circle,
          ),
          child: IconButton(
            onPressed: onPressed,
            icon: Icon(icon, color: Colors.white),
            iconSize: 28,
          ),
        ),
        const SizedBox(height: 4),
        Text(
          label,
          style: Theme.of(context).textTheme.bodySmall?.copyWith(
            color: Colors.white,
          ),
        ),
      ],
    );
  }

  void _handleQrScanned(String rawData) {
    if (_qrService == null) return;
    
    // Pause scanning to prevent multiple scans
    setState(() {
      _isScanning = false;
    });
    
    final result = _qrService!.parseQrCode(rawData);
    widget.onQrScanned(result);
    
    // Resume scanning after a delay
    Future.delayed(const Duration(seconds: 2), () {
      if (mounted) {
        setState(() {
          _isScanning = true;
        });
      }
    });
  }

  Future<void> _toggleFlash() async {
    if (controller != null) {
      await controller!.toggleTorch();
      setState(() {
        _isFlashOn = !_isFlashOn;
      });
    }
  }

  void _toggleScanning() {
    setState(() {
      _isScanning = !_isScanning;
    });
    
    if (_isScanning) {
      controller?.start();
    } else {
      controller?.stop();
    }
  }

  void _openGallery() {
    // TODO: Implement gallery QR code scanning
    // This would require image_picker and qr_code_tools packages
    ScaffoldMessenger.of(context).showSnackBar(
      const SnackBar(
        content: Text('Gallery scanning not yet implemented'),
      ),
    );
  }

  @override
  void dispose() {
    controller?.dispose();
    super.dispose();
  }
}

/// Full-screen QR scanner screen
class QrScannerScreen extends StatelessWidget {
  final Function(QrScanResult) onQrScanned;
  final String? title;
  final String? instructionText;

  const QrScannerScreen({
    super.key,
    required this.onQrScanned,
    this.title,
    this.instructionText,
  });

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      backgroundColor: Colors.black,
      appBar: AppBar(
        title: Text(title ?? 'Scan QR Code'),
        backgroundColor: Colors.transparent,
        foregroundColor: Colors.white,
        elevation: 0,
      ),
      body: QrScannerWidget(
        onQrScanned: (result) {
          onQrScanned(result);
          Navigator.of(context).pop();
        },
        onPermissionDenied: () {
          Navigator.of(context).pop();
          ScaffoldMessenger.of(context).showSnackBar(
            const SnackBar(
              content: Text('Camera permission is required to scan QR codes'),
              backgroundColor: Colors.red,
            ),
          );
        },
        instructionText: instructionText,
      ),
    );
  }
}

/// Compact QR scanner widget for embedding in other screens
class CompactQrScanner extends StatelessWidget {
  final Function(QrScanResult) onQrScanned;
  final double height;
  final String? instructionText;

  const CompactQrScanner({
    super.key,
    required this.onQrScanned,
    this.height = 200,
    this.instructionText,
  });

  @override
  Widget build(BuildContext context) {
    return Container(
      height: height,
      decoration: BoxDecoration(
        borderRadius: BorderRadius.circular(12),
        border: Border.all(
          color: Theme.of(context).colorScheme.outline,
        ),
      ),
      clipBehavior: Clip.hardEdge,
      child: QrScannerWidget(
        onQrScanned: onQrScanned,
        instructionText: instructionText,
        showFlashToggle: false,
        showGalleryButton: false,
      ),
    );
  }
}
