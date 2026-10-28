import 'package:flutter/foundation.dart';

class ApiConfig {
  ApiConfig._();

  static const _override = String.fromEnvironment('API_BASE_URL');

  /// Explicit --dart-define=API_BASE_URL=... always wins. Otherwise picks a
  /// default that works out of the box per platform: 10.0.2.2 is an
  /// Android-emulator-only alias for the host's localhost and does not
  /// resolve on iOS, web, or a physical device. Uses defaultTargetPlatform
  /// (not dart:io's Platform) so this stays valid on web builds too.
  static String get baseUrl {
    if (_override.isNotEmpty) return _override;
    if (!kIsWeb && defaultTargetPlatform == TargetPlatform.android) {
      return 'http://10.0.2.2:3000';
    }
    return 'http://localhost:3000';
  }
}
