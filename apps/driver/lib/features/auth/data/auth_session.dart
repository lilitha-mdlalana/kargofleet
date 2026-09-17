import 'package:fleeter/features/auth/data/auth_api.dart';

class AuthSession {
  AuthSession._();

  static final AuthSession instance = AuthSession._();

  String? token;
  AuthUser? user;

  bool get isAuthenticated => token != null;
}
