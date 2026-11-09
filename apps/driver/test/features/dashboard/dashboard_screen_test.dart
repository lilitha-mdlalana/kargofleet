import 'dart:convert';

import 'package:fleeter/features/auth/data/auth_session.dart';
import 'package:fleeter/features/dashboard/dashboard_screen.dart';
import 'package:fleeter/features/dashboard/data/dashboard_api.dart';
import 'package:fleeter/features/map/map_screen.dart';
import 'package:fleeter/features/vehicle/vehicle_screen.dart';
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

  testWidgets('tapping the trip card navigates to MapScreen when a trip exists', (tester) async {
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
    await tester.pumpAndSettle();

    await tester.tap(find.textContaining('Cape Town'));
    await tester.pumpAndSettle();

    expect(find.byType(MapScreen), findsOneWidget);
  });

  testWidgets('shows Check In button for a scheduled trip', (tester) async {
    AuthSession.instance.token = 'fake-token';
    final client = MockClient((request) async => http.Response(
          jsonEncode({
            'driver': {'id': '1', 'name': 'Jane Doe', 'phone': '+27821234567'},
            'vehicle': null,
            'trip': {'id': '3', 'origin': 'Cape Town', 'destination': 'Stellenbosch', 'status': 'scheduled'},
          }),
          200,
        ));

    await tester.pumpWidget(MaterialApp(
      home: DashboardScreen(dashboardApi: DashboardApi(client: client)),
    ));
    await tester.pumpAndSettle();

    expect(find.text('Check In'), findsOneWidget);
    expect(find.text('Check Out'), findsNothing);
  });

  testWidgets('tapping Check In calls start and flips UI to Check Out', (tester) async {
    AuthSession.instance.token = 'fake-token';
    final client = MockClient((request) async {
      if (request.method == 'POST' && request.url.path.endsWith('/start')) {
        return http.Response(
          jsonEncode({
            'id': '3',
            'origin': 'Cape Town',
            'destination': 'Stellenbosch',
            'status': 'active',
            'startedAt': DateTime.now().toUtc().toIso8601String(),
          }),
          200,
        );
      }
      return http.Response(
        jsonEncode({
          'driver': {'id': '1', 'name': 'Jane Doe', 'phone': '+27821234567'},
          'vehicle': null,
          'trip': {'id': '3', 'origin': 'Cape Town', 'destination': 'Stellenbosch', 'status': 'scheduled'},
        }),
        200,
      );
    });

    await tester.pumpWidget(MaterialApp(
      home: DashboardScreen(dashboardApi: DashboardApi(client: client)),
    ));
    await tester.pumpAndSettle();

    await tester.tap(find.text('Check In'));
    await tester.pumpAndSettle();

    expect(find.text('Check Out'), findsOneWidget);
    expect(find.text('Check In'), findsNothing);
  });

  testWidgets('shows Check Out button for an active trip', (tester) async {
    AuthSession.instance.token = 'fake-token';
    final client = MockClient((request) async => http.Response(
          jsonEncode({
            'driver': {'id': '1', 'name': 'Jane Doe', 'phone': '+27821234567'},
            'vehicle': null,
            'trip': {
              'id': '3',
              'origin': 'Cape Town',
              'destination': 'Stellenbosch',
              'status': 'active',
              'startedAt': DateTime.now().toUtc().toIso8601String(),
            },
          }),
          200,
        ));

    await tester.pumpWidget(MaterialApp(
      home: DashboardScreen(dashboardApi: DashboardApi(client: client)),
    ));
    await tester.pumpAndSettle();

    expect(find.text('Check Out'), findsOneWidget);
    expect(find.text('Check In'), findsNothing);
  });

  testWidgets('shows no action button for a completed trip', (tester) async {
    AuthSession.instance.token = 'fake-token';
    final client = MockClient((request) async => http.Response(
          jsonEncode({
            'driver': {'id': '1', 'name': 'Jane Doe', 'phone': '+27821234567'},
            'vehicle': null,
            'trip': {
              'id': '3',
              'origin': 'Cape Town',
              'destination': 'Stellenbosch',
              'status': 'completed',
              'startedAt': DateTime.now().toUtc().subtract(const Duration(hours: 1)).toIso8601String(),
              'endedAt': DateTime.now().toUtc().toIso8601String(),
            },
          }),
          200,
        ));

    await tester.pumpWidget(MaterialApp(
      home: DashboardScreen(dashboardApi: DashboardApi(client: client)),
    ));
    await tester.pumpAndSettle();

    expect(find.text('Check In'), findsNothing);
    expect(find.text('Check Out'), findsNothing);
  });

  testWidgets('Driver Detail and Transport Tool placeholder cards are inert', (tester) async {
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

    expect(find.text('Vehicle Detail'), findsOneWidget);
    expect(find.text('Driver Detail'), findsOneWidget);
    expect(find.text('Transport Tool'), findsOneWidget);

    await tester.tap(find.text('Driver Detail'));
    await tester.pumpAndSettle();
    expect(find.byType(DashboardScreen), findsOneWidget);

    await tester.tap(find.text('Transport Tool'));
    await tester.pumpAndSettle();
    expect(find.byType(DashboardScreen), findsOneWidget);
  });

  testWidgets('Vehicle Detail card is not tappable when no vehicle assigned', (tester) async {
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

    await tester.tap(find.text('Vehicle Detail'));
    await tester.pumpAndSettle();

    expect(find.byType(DashboardScreen), findsOneWidget);
    expect(find.byType(VehicleScreen), findsNothing);
  });

  testWidgets('tapping Vehicle Detail navigates to VehicleScreen when a vehicle is assigned', (tester) async {
    AuthSession.instance.token = 'fake-token';
    final client = MockClient((request) async => http.Response(
          jsonEncode({
            'driver': {'id': '1', 'name': 'Jane Doe', 'phone': '+27821234567'},
            'vehicle': {'id': '2', 'name': 'Toyota Quantum', 'registration': 'CA 123-456'},
            'trip': null,
          }),
          200,
        ));

    await tester.pumpWidget(MaterialApp(
      home: DashboardScreen(dashboardApi: DashboardApi(client: client)),
    ));
    await tester.pumpAndSettle();

    await tester.tap(find.text('Vehicle Detail'));
    await tester.pumpAndSettle();

    expect(find.byType(VehicleScreen), findsOneWidget);
  });
}
