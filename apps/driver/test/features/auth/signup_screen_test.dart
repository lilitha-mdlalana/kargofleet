import 'package:fleeter/features/auth/data/auth_api.dart';
import 'package:fleeter/features/auth/signup_screen.dart';
import 'package:flutter/material.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:http/http.dart' as http;
import 'package:http/testing.dart';

void main() {
  testWidgets('blocks submit when fields are empty', (tester) async {
    await tester.pumpWidget(const MaterialApp(home: SignupScreen()));

    await tester.tap(find.text('Sign up'));
    await tester.pump();

    expect(find.text('Name is required'), findsOneWidget);
    expect(find.text('Phone number is required'), findsOneWidget);
  });

  testWidgets('navigates to OtpScreen on a successful signup call', (tester) async {
    final client = MockClient((request) async {
      return http.Response('{"message":"OTP sent"}', 202);
    });

    await tester.pumpWidget(MaterialApp(home: SignupScreen(authApi: AuthApi(client: client))));

    await tester.enterText(find.byType(TextFormField).first, 'Ada Lovelace');
    await tester.enterText(find.byType(TextFormField).last, '+27821234567');
    await tester.tap(find.text('Sign up'));
    await tester.pumpAndSettle();

    expect(find.text('Verify your number'), findsOneWidget);
  });
}
