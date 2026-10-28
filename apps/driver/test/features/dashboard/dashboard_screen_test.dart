import 'dart:convert';

import 'package:fleeter/features/auth/data/auth_session.dart';
import 'package:fleeter/features/dashboard/dashboard_screen.dart';
import 'package:fleeter/features/dashboard/data/dashboard_api.dart';
import 'package:flutter/material.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:http/http.dart' as http;
import 'package:http/testing.dart';

void main() {
  testWidgets('shows loading then driver/vehicle/trip info', (tester) async {
    AuthSession.instance.token = 'fake-token';
    final client = MockClient((request) async => http.Response(
          jsonEncode({
            'driver': {'id': '1', 'name': 'Jane Doe', 'phone': '+27821234567', 'license': 'C1'},
            'vehicle': {'id': '2', 'name': 'Toyota Quantum', 'registration': 'CA 123-456'},
            'trip': {'id': '3', 'origin': 'Cape Town', 'destination': 'Stellenbosch', 'status': 'scheduled'},
          }),
          200,
        ));

    await tester.pumpWidget(MaterialApp(
      home: DashboardScreen(dashboardApi: DashboardApi(client: client)),
    ));

    expect(find.byType(CircularProgressIndicator), findsOneWidget);

    await tester.pumpAndSettle();

    expect(find.text('Hi, Jane Doe'), findsOneWidget);
    expect(find.textContaining('Toyota Quantum'), findsOneWidget);
    expect(find.textContaining('Cape Town'), findsOneWidget);
  });

  testWidgets('shows empty state when no vehicle/trip assigned', (tester) async {
    AuthSession.instance.token = 'fake-token';
    final client = MockClient((request) async => http.Response(
          jsonEncode({
            'driver': {'id': '1', 'name': 'Jane Doe', 'phone': '+27821234567'},
            'vehicle': null,
            'trip': null,
          }),
          200,
        ));

    await tester.pumpWidget(MaterialApp(
      home: DashboardScreen(dashboardApi: DashboardApi(client: client)),
    ));

    await tester.pumpAndSettle();

    expect(find.text('No vehicle assigned yet'), findsOneWidget);
    expect(find.text('No trip scheduled'), findsOneWidget);
  });
}
