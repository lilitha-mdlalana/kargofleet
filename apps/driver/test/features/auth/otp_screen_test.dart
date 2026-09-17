import 'package:fleeter/features/auth/data/auth_api.dart';
import 'package:fleeter/features/auth/otp_screen.dart';
import 'package:flutter/material.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:http/http.dart' as http;
import 'package:http/testing.dart';

void main() {
  testWidgets('blocks submit when the code is not 6 digits', (tester) async {
    await tester.pumpWidget(const MaterialApp(
      home: OtpScreen(phone: '+27821234567', mode: OtpMode.login),
    ));

    await tester.enterText(find.byType(TextFormField), '123');
    await tester.tap(find.text('Verify'));
    await tester.pump();

    expect(find.text('Enter the 6-digit code'), findsOneWidget);
  });

  testWidgets('shows the server error when the code is rejected', (tester) async {
    final client = MockClient((request) async {
      return http.Response('{"message":"Invalid or expired code"}', 401);
    });

    await tester.pumpWidget(MaterialApp(
      home: OtpScreen(
        phone: '+27821234567',
        mode: OtpMode.login,
        authApi: AuthApi(client: client),
      ),
    ));

    await tester.enterText(find.byType(TextFormField), '123456');
    await tester.tap(find.text('Verify'));
    await tester.pumpAndSettle();

    expect(find.text('Invalid or expired code'), findsOneWidget);
  });
}
