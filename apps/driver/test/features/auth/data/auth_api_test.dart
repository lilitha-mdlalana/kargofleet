import 'dart:convert';

import 'package:fleeter/features/auth/data/auth_api.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:http/http.dart' as http;
import 'package:http/testing.dart';

void main() {
  group('AuthApi.login', () {
    test('completes without error on a 202 response', () async {
      final client = MockClient((request) async {
        expect(request.url.path, '/auth/login');
        return http.Response(jsonEncode({'message': 'OTP sent'}), 202);
      });
      final api = AuthApi(client: client);

      await api.login(phone: '+27821234567');
    });

    test('throws AuthApiException with the server message on failure', () async {
      final client = MockClient((request) async {
        return http.Response(
          jsonEncode({'message': 'No account with that phone number'}),
          404,
        );
      });
      final api = AuthApi(client: client);

      await expectLater(
        () => api.login(phone: '+27821234567'),
        throwsA(
          isA<AuthApiException>().having(
            (e) => e.message,
            'message',
            'No account with that phone number',
          ),
        ),
      );
    });
  });

  group('AuthApi.verify', () {
    test('parses the token and user from a 200 response', () async {
      final client = MockClient((request) async {
        return http.Response(
          jsonEncode({
            'token': 'jwt-token',
            'user': {'id': 'u1', 'name': 'Ada', 'phone': '+27821234567'},
          }),
          200,
        );
      });
      final api = AuthApi(client: client);

      final result = await api.verify(phone: '+27821234567', code: '123456');

      expect(result.token, 'jwt-token');
      expect(result.user.name, 'Ada');
    });
  });
}
