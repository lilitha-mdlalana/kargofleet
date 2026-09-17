import 'package:fleeter/features/auth/data/auth_api.dart';
import 'package:fleeter/features/auth/login_screen.dart';
import 'package:flutter/material.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:http/http.dart' as http;
import 'package:http/testing.dart';

void main() {
  testWidgets('blocks submit when phone is empty', (tester) async {
    await tester.pumpWidget(const MaterialApp(home: LoginScreen()));

    await tester.tap(find.text('Continue'));
    await tester.pump();

    expect(find.text('Phone number is required'), findsOneWidget);
  });

  testWidgets('navigates to OtpScreen on a successful login call', (tester) async {
    final client = MockClient((request) async {
      return http.Response('{"message":"OTP sent"}', 202);
    });

    await tester.pumpWidget(MaterialApp(home: LoginScreen(authApi: AuthApi(client: client))));

    await tester.enterText(find.byType(TextFormField), '+27821234567');
    await tester.tap(find.text('Continue'));
    await tester.pumpAndSettle();

    expect(find.text('Verify your number'), findsOneWidget);
  });
}
