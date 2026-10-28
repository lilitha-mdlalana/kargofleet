import 'dart:convert';

import 'package:fleeter/features/dashboard/data/dashboard_api.dart';
import 'package:fleeter/features/map/data/route_service.dart';
import 'package:fleeter/features/map/map_screen.dart';
import 'package:flutter/material.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:http/http.dart' as http;
import 'package:http/testing.dart';

void main() {
  const trip = DashboardTrip(
    id: '1',
    origin: 'Cape Town',
    destination: 'Stellenbosch',
    status: 'scheduled',
  );

  http.Client fakeClient() {
    return MockClient((request) async {
      if (request.url.path.contains('/search')) {
        return http.Response(jsonEncode([{'lat': '-33.9249', 'lon': '18.4241'}]), 200);
      }
      return http.Response(
        jsonEncode({
          'code': 'Ok',
          'routes': [
            {
              'distance': 50000.0,
              'duration': 3000.0,
              'geometry': {
                'coordinates': [
                  [18.4241, -33.9249],
                  [18.8602, -33.9321],
                ],
              },
            },
          ],
        }),
        200,
      );
    });
  }

  testWidgets('shows loading then route distance/duration and a back button', (tester) async {
    final routeService = RouteService(client: fakeClient());

    await tester.pumpWidget(MaterialApp(
      home: MapScreen(trip: trip, routeService: routeService),
    ));

    expect(find.byType(CircularProgressIndicator), findsOneWidget);

    await tester.pumpAndSettle();

    expect(find.text('50.0 km'), findsOneWidget);
    expect(find.text('50 min'), findsOneWidget);
    expect(find.byIcon(Icons.arrow_back), findsOneWidget);
    expect(find.textContaining('Cape Town'), findsOneWidget);
    expect(find.textContaining('Stellenbosch'), findsOneWidget);
  });

  testWidgets('shows an error message when the route cannot be calculated', (tester) async {
    final routeService = RouteService(client: MockClient((request) async => http.Response(jsonEncode([]), 200)));

    await tester.pumpWidget(MaterialApp(
      home: MapScreen(trip: trip, routeService: routeService),
    ));

    await tester.pumpAndSettle();

    expect(find.textContaining('No location found'), findsOneWidget);
  });
}
