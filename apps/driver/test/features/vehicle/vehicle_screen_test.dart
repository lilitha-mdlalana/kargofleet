import 'dart:convert';

import 'package:fleeter/features/auth/data/auth_session.dart';
import 'package:fleeter/features/vehicle/data/vehicle_api.dart';
import 'package:fleeter/features/vehicle/vehicle_screen.dart';
import 'package:flutter/material.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:http/http.dart' as http;
import 'package:http/testing.dart';

void main() {
  testWidgets('shows loading then vehicle name/registration', (tester) async {
    AuthSession.instance.token = 'fake-token';
    final client = MockClient((request) async => http.Response(
          jsonEncode({'id': '2', 'name': 'Toyota Quantum', 'registration': 'CA 123-456'}),
          200,
        ));

    await tester.pumpWidget(MaterialApp(
      home: VehicleScreen(vehicleApi: VehicleApi(client: client)),
    ));

    expect(find.byType(CircularProgressIndicator), findsOneWidget);

    await tester.pumpAndSettle();

    expect(find.text('Toyota Quantum'), findsOneWidget);
    expect(find.text('CA 123-456'), findsOneWidget);
  });

  testWidgets('shows an error message when the fetch fails', (tester) async {
    AuthSession.instance.token = 'fake-token';
    final client = MockClient((request) async => http.Response(
          jsonEncode({'message': 'No vehicle assigned'}),
          404,
        ));

    await tester.pumpWidget(MaterialApp(
      home: VehicleScreen(vehicleApi: VehicleApi(client: client)),
    ));

    await tester.pumpAndSettle();

    expect(find.text('No vehicle assigned'), findsOneWidget);
  });
}
