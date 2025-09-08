import 'package:flutter/material.dart';
import 'package:provider/provider.dart';
import '../../controllers/app_controller.dart';
import '../../models/auth_models.dart';
import '../../utils/phone_formatter.dart';

class RegisterScreen extends StatefulWidget {
  const RegisterScreen({super.key});

  @override
  State<RegisterScreen> createState() => _RegisterScreenState();
}

class _RegisterScreenState extends State<RegisterScreen> {
  final GlobalKey<FormState> _formKey = GlobalKey<FormState>();
  final TextEditingController _fullNameController = TextEditingController();
  final TextEditingController _emailController = TextEditingController();
  final TextEditingController _phoneNumberController = TextEditingController();
  final TextEditingController _passwordController = TextEditingController();
  bool _obscure = true;
  bool _acceptTerms = false;
  String? _error;
  bool _isPasswordStrong = false;

  @override
  void dispose() {
    _fullNameController.dispose();
    _emailController.dispose();
    _phoneNumberController.dispose();
    _passwordController.dispose();
    super.dispose();
  }

  String? _validateEmail(String? value) {
    if (value == null || value.trim().isEmpty) return 'Informe o e-mail';
    final emailRegex = RegExp(r'^[\w-\.]+@([\w-]+\.)+[\w-]{2,4}$');
    if (!emailRegex.hasMatch(value.trim())) return 'Informe um e-mail válido';
    return null;
  }

  String? _validatePassword(String? value) {
    if (value == null || value.isEmpty) {
      return 'Senha é obrigatória';
    }
    
    if (value.length < 8) {
      return 'A senha deve ter ao menos 8 caracteres';
    }
    
    if (!RegExp(r'[A-Z]').hasMatch(value)) {
      return 'A senha deve conter ao menos uma letra maiúscula';
    }
    
    if (!RegExp(r'[a-z]').hasMatch(value)) {
      return 'A senha deve conter ao menos uma letra minúscula';
    }
    
    if (!RegExp(r'[0-9]').hasMatch(value)) {
      return 'A senha deve conter ao menos um número';
    }
    
    return null;
  }
  
  String? _validatePhoneNumber(String? value) {
    if (value == null || value.trim().isEmpty) {
      return 'Número de telefone é obrigatório';
    }
    
    // Remove all non-digit characters
    final digitsOnly = value.replaceAll(RegExp(r'[^0-9]'), '');
    
    // Brazilian phone number should have 10 or 11 digits (with area code)
    if (digitsOnly.length < 10 || digitsOnly.length > 11) {
      return 'Número de telefone inválido';
    }
    
    return null;
  }
  
  String? _validateFullName(String? value) {
    if (value == null || value.trim().isEmpty) {
      return 'Nome completo é obrigatório';
    }
    
    if (value.trim().split(' ').length < 2) {
      return 'Informe seu nome completo';
    }
    
    return null;
  }
  
  void _checkPasswordStrength(String password) {
    setState(() {
      _isPasswordStrong = _validatePassword(password) == null;
    });
  }

  Future<void> _handleRegister(BuildContext context) async {
    final app = Provider.of<AppController>(context, listen: false);
    final auth = app.auth;
    setState(() {
      _error = null;
    });

    if (!_formKey.currentState!.validate()) return;
    if (!_acceptTerms) {
      setState(() => _error = 'Você precisa aceitar os termos para continuar');
      return;
    }

    final fullName = _fullNameController.text.trim();
    final email = _emailController.text.trim();
    final phoneNumber = PhoneFormatter.unformat(_phoneNumberController.text.trim());
    final password = _passwordController.text;

    print('📝 Registration attempt:');
    print('   Full Name: $fullName');
    print('   Email: $email');
    print('   Phone: $phoneNumber');
    print('   Password length: ${password.length}');

    final req = RegisterRequest(
      fullName: fullName,
      email: email,
      phoneNumber: phoneNumber,
      password: password,
    );

    print('🚀 Calling auth.register()...');
    final ok = await auth.register(req);
    
    if (!mounted) return;
    
    print('📊 Registration result: $ok');
    
    if (ok) {
      print('✅ Registration successful, attempting automatic login...');
      
      // Wait a moment for the account to be fully activated on the backend
      print('⏳ Waiting 2 seconds for account activation...');
      await Future.delayed(const Duration(seconds: 2));
      
      if (!mounted) return;
      
      // Try login with phone number
      var loggedIn = await auth.login(LoginRequest(phoneNumber: phoneNumber, password: password));
      
      if (!mounted) return;
      
      print('📊 Auto-login result: $loggedIn');
      
      if (loggedIn) {
        print('✅ Auto-login successful, switching to authenticated mode...');
        await app.switchToAuthenticatedMode();
        if (!mounted) return;
        print('✅ Navigating to home...');
        Navigator.pushReplacementNamed(context, '/home');
        return;
      } else {
        print('⚠️ Auto-login failed after multiple attempts, redirecting to login screen...');
        print('💡 User can try logging in manually with either username or email');
        ScaffoldMessenger.of(context).showSnackBar(
          const SnackBar(
            content: Text('Conta criada com sucesso! Faça login para continuar.'),
            backgroundColor: Colors.green,
          ),
        );
        Navigator.pushReplacementNamed(
          context,
          '/login',
          arguments: {
            'prefillPhoneNumber': phoneNumber,
            'prefillPassword': password,
            'showSuccessMessage': true,
          },
        );
      }
    } else {
      print('❌ Registration failed');
      print('🔍 Auth error: ${auth.error}');
      print('🔍 Field errors: ${auth.fieldErrors}');
      
      setState(() {
        _error = auth.error ?? 'Falha ao criar conta';
      });
    }
  }

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);
    final colors = theme.colorScheme;
    final app = Provider.of<AppController>(context);
    final auth = app.auth;

    final fullNameFieldError = auth.fieldErrors?['fullName'] ?? auth.fieldErrors?['FullName'];
    final emailFieldError = auth.fieldErrors?['email'] ?? auth.fieldErrors?['Email'];
    final phoneFieldError = auth.fieldErrors?['phoneNumber'] ?? auth.fieldErrors?['PhoneNumber'];
    final passwordFieldError = auth.fieldErrors?['password'] ?? auth.fieldErrors?['Password'];

    return Scaffold(
      backgroundColor: colors.surface,
      appBar: AppBar(
        title: const Text('Criar conta'),
        backgroundColor: colors.surface,
        elevation: 0,
        leading: IconButton(
          icon: const Icon(Icons.arrow_back, color: Colors.black),
          onPressed: () => Navigator.of(context).maybePop(),
        ),
      ),
      body: SafeArea(
        child: SingleChildScrollView(
          padding: const EdgeInsets.all(16),
          child: Center(
            child: ConstrainedBox(
              constraints: const BoxConstraints(maxWidth: 520),
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.stretch,
                children: [
                  _buildHeader(context),
                  const SizedBox(height: 16),
                  if (_error != null) _buildErrorBanner(theme, _error!),
                  const SizedBox(height: 8),
                  Card(
                    shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(16)),
                    elevation: 0,
                    color: colors.surface,
                    child: Padding(
                      padding: const EdgeInsets.all(16),
                      child: Form(
                        key: _formKey,
                        child: Column(
                          children: [
                            TextFormField(
                              controller: _fullNameController,
                              decoration: InputDecoration(
                                labelText: 'Nome Completo',
                                prefixIcon: const Icon(Icons.person_outline),
                                errorText: fullNameFieldError,
                              ),
                              validator: _validateFullName,
                              textInputAction: TextInputAction.next,
                              textCapitalization: TextCapitalization.words,
                            ),
                            const SizedBox(height: 12),
                            TextFormField(
                              controller: _emailController,
                              decoration: InputDecoration(
                                labelText: 'E-mail',
                                prefixIcon: const Icon(Icons.email_outlined),
                                errorText: emailFieldError,
                              ),
                              validator: _validateEmail,
                              keyboardType: TextInputType.emailAddress,
                              textInputAction: TextInputAction.next,
                            ),
                            const SizedBox(height: 12),
                            TextFormField(
                              controller: _phoneNumberController,
                              decoration: InputDecoration(
                                labelText: 'Número de Telefone',
                                prefixIcon: const Icon(Icons.phone_outlined),
                                errorText: phoneFieldError,
                                hintText: '(11) 99999-9999',
                              ),
                              validator: _validatePhoneNumber,
                              keyboardType: TextInputType.phone,
                              textInputAction: TextInputAction.next,
                              inputFormatters: [BrazilianPhoneInputFormatter()],
                            ),
                            const SizedBox(height: 12),
                            TextFormField(
                              controller: _passwordController,
                              decoration: InputDecoration(
                                labelText: 'Senha',
                                prefixIcon: const Icon(Icons.lock_outline),
                                suffixIcon: Row(
                                  mainAxisSize: MainAxisSize.min,
                                  children: [
                                    Icon(
                                      _isPasswordStrong ? Icons.check_circle : Icons.error,
                                      color: _isPasswordStrong ? Colors.green : Colors.red,
                                      size: 20,
                                    ),
                                    const SizedBox(width: 8),
                                    IconButton(
                                      onPressed: () => setState(() => _obscure = !_obscure),
                                      icon: Icon(_obscure ? Icons.visibility : Icons.visibility_off),
                                    ),
                                  ],
                                ),
                                errorText: passwordFieldError,
                                helperText: 'Mín. 8 caracteres, 1 maiúscula, 1 minúscula, 1 número',
                                helperMaxLines: 2,
                              ),
                              validator: _validatePassword,
                              obscureText: _obscure,
                              textInputAction: TextInputAction.done,
                              onChanged: _checkPasswordStrength,
                            ),
                            const SizedBox(height: 16),
                            CheckboxListTile(
                              value: _acceptTerms,
                              onChanged: (v) => setState(() => _acceptTerms = v ?? false),
                              controlAffinity: ListTileControlAffinity.leading,
                              title: const Text('Eu li e aceito os termos de uso e privacidade'),
                              contentPadding: EdgeInsets.zero,
                            ),
                            const SizedBox(height: 8),
                            SizedBox(
                              width: double.infinity,
                              child: ElevatedButton(
                                onPressed: auth.isLoading ? null : () => _handleRegister(context),
                                child: auth.isLoading
                                    ? const SizedBox(width: 20, height: 20, child: CircularProgressIndicator(strokeWidth: 2))
                                    : const Text('Criar conta'),
                              ),
                            ),
                          ],
                        ),
                      ),
                    ),
                  ),
                  const SizedBox(height: 24),
                  TextButton(
                    onPressed: () => Navigator.pushReplacementNamed(context, '/login'),
                    child: const Text('Já tenho conta'),
                  ),
                ],
              ),
            ),
          ),
        ),
      ),
    );
  }

  Widget _buildHeader(BuildContext context) {
    final theme = Theme.of(context);
    final colors = theme.colorScheme;
    return Column(
      crossAxisAlignment: CrossAxisAlignment.start,
      children: [
        Row(
          children: [
            Container(
              padding: const EdgeInsets.all(10),
              decoration: BoxDecoration(
                color: colors.primary.withOpacity(0.1),
                shape: BoxShape.circle,
              ),
              child: Icon(Icons.person_add_alt, color: colors.primary),
            ),
            const SizedBox(width: 12),
            Text(
              'Crie sua conta',
              style: theme.textTheme.titleLarge?.copyWith(fontWeight: FontWeight.bold),
            ),
          ],
        ),
        const SizedBox(height: 8),
        Text(
          'Aproveite todos os recursos do app',
          style: theme.textTheme.bodyMedium?.copyWith(color: theme.colorScheme.onSurfaceVariant),
        ),
      ],
    );
  }

  Widget _buildErrorBanner(ThemeData theme, String message) {
    return Container(
      width: double.infinity,
      padding: const EdgeInsets.all(12),
      decoration: BoxDecoration(
        color: theme.colorScheme.error.withOpacity(0.1),
        borderRadius: BorderRadius.circular(12),
        border: Border.all(color: theme.colorScheme.error.withOpacity(0.2)),
      ),
      child: Row(
        children: [
          Icon(Icons.error_outline, color: theme.colorScheme.error),
          const SizedBox(width: 8),
          Expanded(
            child: Text(
              message,
              style: theme.textTheme.bodyMedium?.copyWith(color: theme.colorScheme.error),
            ),
          ),
        ],
      ),
    );
  }
}


