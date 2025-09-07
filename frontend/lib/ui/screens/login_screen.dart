import 'package:flutter/material.dart';
import 'package:provider/provider.dart';
import '../../controllers/app_controller.dart';
import '../../models/public_salon.dart';
import 'anonymous_join_queue_screen.dart';
import '../../models/auth_models.dart';

class LoginScreen extends StatefulWidget {
  const LoginScreen({super.key});

  @override
  State<LoginScreen> createState() => _LoginScreenState();
}

class _LoginScreenState extends State<LoginScreen> {
  final GlobalKey<FormState> _formKey = GlobalKey<FormState>();
  final TextEditingController _usernameController = TextEditingController();
  final TextEditingController _passwordController = TextEditingController();
  final TextEditingController _twoFactorController = TextEditingController();
  bool _obscure = true;
  String? _error;
  bool _showSuccessMessage = false;

  @override
  void initState() {
    super.initState();
    // Handle prefilled credentials from registration
    WidgetsBinding.instance.addPostFrameCallback((_) {
      // Check if widget is still mounted before accessing context
      if (!mounted) return;
      
      final args = ModalRoute.of(context)?.settings.arguments as Map<String, dynamic>?;
      if (args != null) {
        if (args['prefillUsername'] != null) {
          _usernameController.text = args['prefillUsername'];
        }
        if (args['prefillPassword'] != null) {
          _passwordController.text = args['prefillPassword'];
        }
        if (args['showSuccessMessage'] == true) {
          if (mounted) {
            setState(() {
              _showSuccessMessage = true;
            });
            // Hide success message after 5 seconds
            Future.delayed(const Duration(seconds: 5), () {
              if (mounted) {
                setState(() {
                  _showSuccessMessage = false;
                });
              }
            });
          }
        }
      }
    });
  }

  @override
  void dispose() {
    _usernameController.dispose();
    _passwordController.dispose();
    _twoFactorController.dispose();
    super.dispose();
  }

  Future<void> _handleLogin(BuildContext context) async {
    final app = Provider.of<AppController>(context, listen: false);
    final auth = app.auth;
    setState(() => _error = null);

    if (!_formKey.currentState!.validate()) return;

    final username = _usernameController.text.trim();
    final password = _passwordController.text;

    print('📝 Login attempt:');
    print('   Username: $username');
    print('   Password length: ${password.length}');

    // Try login with username first
    var request = LoginRequest(
      username: username,
      password: password,
    );

    print('🚀 Calling auth.login() with username...');
    var success = await auth.login(request);

    if (!mounted) return;

    print('📊 Login result (username): $success');
    print('🔍 Requires 2FA: ${auth.requiresTwoFactor}');

    // If login failed and username looks like an email, try with email field
    if (!success && !auth.requiresTwoFactor && username.contains('@')) {
      print('🔄 Username login failed, trying as email...');
      
      // Clear previous error
      auth.clearError();
      
      success = await auth.login(request); // Same request, backend should handle email
      
      if (!mounted) return;
      print('📊 Login result (email): $success');
    }

    if (auth.requiresTwoFactor) {
      print('🔐 Two-factor authentication required');
      if (mounted) {
        setState(() {});
      }
      return;
    }

    if (success) {
      print('✅ Login successful, switching to authenticated mode...');
      
      try {
        // Optional sanity check: verify token by fetching profile
        print('🔍 Verifying profile...');
        final profile = await auth.getProfile();
        print('✅ Profile verified: $profile');
        
        await app.switchToAuthenticatedMode();
        if (!mounted) return;
        print('✅ Navigating to home...');
        Navigator.pushReplacementNamed(context, '/home');
        return;
      } catch (e) {
        print('❌ Profile verification failed: $e');
        if (mounted) {
          setState(() => _error = 'Sessão inválida. Tente novamente.');
        }
        return;
      }
    } else {
      print('❌ Login failed');
      print('🔍 Auth error: ${auth.error}');
      
      // Provide helpful error message
      String errorMessage = auth.error ?? 'Falha no login';
      if (errorMessage.toLowerCase().contains('invalid username or password')) {
        errorMessage = 'Usuário ou senha inválidos. Verifique suas credenciais e tente novamente.';
      }
      
      if (mounted) {
        setState(() => _error = errorMessage);
      }
    }
  }

  Future<void> _handleVerify2FA(BuildContext context) async {
    final app = Provider.of<AppController>(context, listen: false);
    final auth = app.auth;
    if (mounted) {
      setState(() => _error = null);
    }

    if (_twoFactorController.text.trim().isEmpty) {
      if (mounted) {
        setState(() => _error = 'Informe o código de verificação');
      }
      return;
    }

    final ok = await auth.verifyTwoFactor(_twoFactorController.text.trim());
    if (!mounted) return;

    if (ok) {
      await app.switchToAuthenticatedMode();
      if (!mounted) return;
      Navigator.pushReplacementNamed(context, '/home');
    } else {
      if (mounted) {
        setState(() => _error = auth.error ?? 'Falha na verificação');
      }
    }
  }

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);
    final colors = theme.colorScheme;
    final app = Provider.of<AppController>(context);
    final auth = app.auth;

    final args = ModalRoute.of(context)?.settings.arguments;
    PublicSalon? targetSalon;
    bool fromCheckIn = false;
    if (args is Map) {
      if (args['salon'] is PublicSalon) targetSalon = args['salon'] as PublicSalon;
      if (args['fromCheckIn'] is bool) fromCheckIn = args['fromCheckIn'] as bool;
      final prefillUsername = args['prefillUsername'] as String?;
      final prefillPassword = args['prefillPassword'] as String?;
      if (prefillUsername != null && _usernameController.text.isEmpty) {
        _usernameController.text = prefillUsername;
      }
      if (prefillPassword != null && _passwordController.text.isEmpty) {
        _passwordController.text = prefillPassword;
      }
    }

    return Scaffold(
      backgroundColor: colors.surface,
      appBar: AppBar(
        title: const Text('Entrar'),
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
              constraints: const BoxConstraints(maxWidth: 480),
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.stretch,
                children: [
                  _buildHeader(context),
                  const SizedBox(height: 16),
                  if (fromCheckIn && targetSalon != null)
                    _buildGuestBanner(context, targetSalon),
                  if (fromCheckIn && targetSalon != null)
                    const SizedBox(height: 8),
                  if (_error != null)
                    _buildErrorBanner(theme, _error!),
                  if (_error != null)
                    const SizedBox(height: 8),
                  if (_showSuccessMessage)
                    _buildSuccessBanner(theme, 'Conta criada com sucesso! Você pode fazer login agora.'),
                  if (_showSuccessMessage)
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
                              controller: _usernameController,
                              decoration: const InputDecoration(
                                labelText: 'Usuário',
                                prefixIcon: Icon(Icons.person_outline),
                              ),
                              textInputAction: TextInputAction.next,
                              validator: (v) => (v == null || v.trim().isEmpty) ? 'Informe o usuário' : null,
                            ),
                            const SizedBox(height: 12),
                            TextFormField(
                              controller: _passwordController,
                              decoration: InputDecoration(
                                labelText: 'Senha',
                                prefixIcon: const Icon(Icons.lock_outline),
                                suffixIcon: IconButton(
                                  onPressed: () => setState(() => _obscure = !_obscure),
                                  icon: Icon(_obscure ? Icons.visibility : Icons.visibility_off),
                                ),
                              ),
                              obscureText: _obscure,
                              validator: (v) => (v == null || v.isEmpty) ? 'Informe a senha' : null,
                            ),
                            const SizedBox(height: 16),
                            SizedBox(
                              width: double.infinity,
                              child: ElevatedButton(
                                onPressed: auth.isLoading ? null : () => _handleLogin(context),
                                child: auth.isLoading
                                    ? const SizedBox(
                                        width: 20,
                                        height: 20,
                                        child: CircularProgressIndicator(strokeWidth: 2),
                                      )
                                    : const Text('Entrar'),
                              ),
                            ),
                const SizedBox(height: 8),
                OutlinedButton.icon(
                  onPressed: auth.isLoading
                      ? null
                      : () async {
                          await app.auth.demoLogin(username: _usernameController.text.trim().isEmpty ? 'demo' : _usernameController.text.trim());
                          if (!mounted) return;
                          Navigator.pushReplacementNamed(context, '/home');
                        },
                  icon: const Icon(Icons.play_arrow),
                  label: const Text('Entrar em modo demo'),
                ),
                          ],
                        ),
                      ),
                    ),
                  ),
                  if (auth.requiresTwoFactor) ...[
                    const SizedBox(height: 12),
                    Card(
                      shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(16)),
                      elevation: 0,
                      color: colors.surface,
                      child: Padding(
                        padding: const EdgeInsets.all(16),
                        child: Column(
                          crossAxisAlignment: CrossAxisAlignment.stretch,
                          children: [
                            Text('Verificação em duas etapas', style: theme.textTheme.titleMedium?.copyWith(fontWeight: FontWeight.bold)),
                            const SizedBox(height: 8),
                            TextFormField(
                              controller: _twoFactorController,
                              decoration: const InputDecoration(
                                labelText: 'Código de verificação',
                                prefixIcon: Icon(Icons.verified_user_outlined),
                              ),
                              keyboardType: TextInputType.number,
                            ),
                            const SizedBox(height: 12),
                            ElevatedButton(
                              onPressed: auth.isLoading ? null : () => _handleVerify2FA(context),
                              child: auth.isLoading
                                  ? const SizedBox(width: 20, height: 20, child: CircularProgressIndicator(strokeWidth: 2))
                                  : const Text('Verificar'),
                            ),
                          ],
                        ),
                      ),
                    ),
                  ],
                  const SizedBox(height: 24),
                  TextButton(
                    onPressed: () => Navigator.pushNamed(context, '/register'),
                    child: const Text('Criar conta'),
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
              child: Icon(Icons.key, color: colors.primary),
            ),
            const SizedBox(width: 12),
            Text(
              'Bem-vindo de volta',
              style: theme.textTheme.titleLarge?.copyWith(fontWeight: FontWeight.bold),
            ),
          ],
        ),
        const SizedBox(height: 8),
        Text(
          'Acesse sua conta para continuar',
          style: theme.textTheme.bodyMedium?.copyWith(color: theme.colorScheme.onSurfaceVariant),
        ),
      ],
    );
  }

  Widget _buildSuccessBanner(ThemeData theme, String message) {
    return Container(
      width: double.infinity,
      padding: const EdgeInsets.all(12),
      decoration: BoxDecoration(
        color: Colors.green.withOpacity(0.1),
        borderRadius: BorderRadius.circular(12),
        border: Border.all(color: Colors.green.withOpacity(0.2)),
      ),
      child: Row(
        children: [
          const Icon(Icons.check_circle_outline, color: Colors.green),
          const SizedBox(width: 8),
          Expanded(
            child: Text(
              message,
              style: theme.textTheme.bodyMedium?.copyWith(color: Colors.green.shade700),
            ),
          ),
        ],
      ),
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

  Widget _buildGuestBanner(BuildContext context, PublicSalon salon) {
    final theme = Theme.of(context);
    final colors = theme.colorScheme;
    return Container(
      padding: const EdgeInsets.all(12),
      decoration: BoxDecoration(
        color: colors.primaryContainer,
        borderRadius: BorderRadius.circular(12),
        border: Border.all(color: colors.onPrimaryContainer.withOpacity(0.15)),
      ),
      child: Row(
        children: [
          Icon(Icons.person_outline, color: colors.onPrimaryContainer),
          const SizedBox(width: 8),
          Expanded(
            child: Text(
              'Você estava iniciando um check-in. Pode continuar como convidado ou fazer login.',
              style: theme.textTheme.bodyMedium?.copyWith(color: colors.onPrimaryContainer),
            ),
          ),
          const SizedBox(width: 8),
          OutlinedButton(
            onPressed: () {
              Navigator.of(context).push(
                MaterialPageRoute(
                  builder: (_) => AnonymousJoinQueueScreen(salon: salon),
                ),
              );
            },
            child: const Text('Convidado'),
          ),
        ],
      ),
    );
  }
}


