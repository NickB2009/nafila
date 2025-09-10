import 'package:flutter/material.dart';
import 'package:flutter/services.dart';
import '../../utils/phone_formatter.dart';
import '../../utils/phone_cache.dart';

/// Enhanced phone number input widget with international support and caching
class PhoneNumberInput extends StatefulWidget {
  final TextEditingController controller;
  final String? labelText;
  final String? hintText;
  final String? errorText;
  final bool enabled;
  final TextInputAction? textInputAction;
  final VoidCallback? onEditingComplete;
  final ValueChanged<String>? onChanged;
  final FormFieldValidator<String>? validator;

  const PhoneNumberInput({
    super.key,
    required this.controller,
    this.labelText,
    this.hintText,
    this.errorText,
    this.enabled = true,
    this.textInputAction,
    this.onEditingComplete,
    this.onChanged,
    this.validator,
  });

  @override
  State<PhoneNumberInput> createState() => _PhoneNumberInputState();
}

class _PhoneNumberInputState extends State<PhoneNumberInput> {
  final FocusNode _focusNode = FocusNode();
  List<String> _recentPhones = [];
  bool _showSuggestions = false;

  @override
  void initState() {
    super.initState();
    _loadCachedPhone();
    _loadRecentPhones();
    _focusNode.addListener(_onFocusChange);
  }

  @override
  void dispose() {
    _focusNode.removeListener(_onFocusChange);
    _focusNode.dispose();
    super.dispose();
  }

  void _onFocusChange() {
    if (_focusNode.hasFocus && _recentPhones.isNotEmpty) {
      setState(() {
        _showSuggestions = true;
      });
    } else {
      setState(() {
        _showSuggestions = false;
      });
    }
  }

  Future<void> _loadCachedPhone() async {
    if (widget.controller.text.isEmpty) {
      final cachedPhone = await PhoneCache.getLastPhoneNumber();
      if (cachedPhone != null && mounted) {
        setState(() {
          widget.controller.text = PhoneNumberFormatter.getDisplayFormat(cachedPhone);
        });
      }
    }
  }

  Future<void> _loadRecentPhones() async {
    final recentPhones = await PhoneCache.getRecentPhoneNumbers();
    if (mounted) {
      setState(() {
        _recentPhones = recentPhones;
      });
    }
  }

  void _onPhoneChanged(String value) {
    // Save to cache when user types
    if (value.isNotEmpty) {
      final cleanPhone = PhoneNumberFormatter.getCleanPhoneNumber(value);
      PhoneCache.savePhoneNumber(cleanPhone);
    }
    
    widget.onChanged?.call(value);
  }

  void _selectRecentPhone(String phone) {
    setState(() {
      widget.controller.text = PhoneNumberFormatter.getDisplayFormat(phone);
      _showSuggestions = false;
    });
    _focusNode.unfocus();
    widget.onChanged?.call(widget.controller.text);
  }

  @override
  Widget build(BuildContext context) {
    return Column(
      crossAxisAlignment: CrossAxisAlignment.start,
      children: [
        TextFormField(
          controller: widget.controller,
          focusNode: _focusNode,
          enabled: widget.enabled,
          keyboardType: TextInputType.phone,
          textInputAction: widget.textInputAction,
          onEditingComplete: widget.onEditingComplete,
          onChanged: _onPhoneChanged,
          validator: widget.validator ?? _defaultValidator,
          inputFormatters: [
            PhoneNumberFormatter(),
            LengthLimitingTextInputFormatter(20), // Reasonable limit for international numbers
          ],
          decoration: InputDecoration(
            labelText: widget.labelText ?? 'Número de Telefone',
            hintText: widget.hintText ?? '+55 (11) 99999-9999',
            errorText: widget.errorText,
            prefixIcon: const Icon(Icons.phone_outlined),
            suffixIcon: widget.controller.text.isNotEmpty
                ? IconButton(
                    icon: const Icon(Icons.clear),
                    onPressed: () {
                      widget.controller.clear();
                      widget.onChanged?.call('');
                    },
                  )
                : _recentPhones.isNotEmpty
                    ? IconButton(
                        icon: const Icon(Icons.history),
                        onPressed: () {
                          setState(() {
                            _showSuggestions = !_showSuggestions;
                          });
                        },
                      )
                    : null,
            helperText: 'Suporta números internacionais (+55, +1, etc.)',
          ),
        ),
        if (_showSuggestions && _recentPhones.isNotEmpty) ...[
          const SizedBox(height: 8),
          Container(
            decoration: BoxDecoration(
              color: Theme.of(context).colorScheme.surface,
              borderRadius: BorderRadius.circular(8),
              border: Border.all(
                color: Theme.of(context).colorScheme.outline.withOpacity(0.3),
              ),
            ),
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                Padding(
                  padding: const EdgeInsets.all(8.0),
                  child: Text(
                    'Números recentes:',
                    style: Theme.of(context).textTheme.bodySmall?.copyWith(
                      color: Theme.of(context).colorScheme.onSurfaceVariant,
                    ),
                  ),
                ),
                ..._recentPhones.map((phone) => ListTile(
                  dense: true,
                  leading: const Icon(Icons.phone, size: 18),
                  title: Text(
                    PhoneNumberFormatter.getDisplayFormat(phone),
                    style: Theme.of(context).textTheme.bodyMedium,
                  ),
                  subtitle: Text(
                    _getCountryName(phone),
                    style: Theme.of(context).textTheme.bodySmall,
                  ),
                  onTap: () => _selectRecentPhone(phone),
                )).toList(),
                const SizedBox(height: 4),
              ],
            ),
          ),
        ],
      ],
    );
  }

  String? _defaultValidator(String? value) {
    if (value == null || value.trim().isEmpty) {
      return 'Número de telefone é obrigatório';
    }

    if (!PhoneNumberFormatter.isValidPhoneNumber(value)) {
      return 'Número de telefone inválido';
    }

    return null;
  }

  String _getCountryName(String phone) {
    final countryCode = PhoneNumberFormatter.getCountryCode(phone);
    switch (countryCode) {
      case '55':
        return 'Brasil';
      case '1':
        return 'EUA/Canadá';
      case '44':
        return 'Reino Unido';
      case '49':
        return 'Alemanha';
      case '33':
        return 'França';
      case '39':
        return 'Itália';
      case '34':
        return 'Espanha';
      case '351':
        return 'Portugal';
      case '54':
        return 'Argentina';
      case '52':
        return 'México';
      default:
        return 'Internacional';
    }
  }
}

/// Helper function to get clean phone number from PhoneNumberInput
String getCleanPhoneNumber(TextEditingController controller) {
  return PhoneNumberFormatter.getCleanPhoneNumber(controller.text);
}
