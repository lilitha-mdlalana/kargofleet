import 'dart:convert';

import 'package:fleeter/features/map/data/route_service.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:http/http.dart' as http;
import 'package:http/testing.dart';

void main() {
  group('RouteService.getRoute', () {
    test('returns a RouteResult on a successful geocode + route', () async {
      final client = MockClient((request) async {
        if (request.url.path.contains('/search')) {
          final query = request.url.queryParameters['q'];
          if (query == 'Cape Town') {
            return http.Response(jsonEncode([{'lat': '-33.9249', 'lon': '18.4241'}]), 200);
          }
          return http.Response(jsonEncode([{'lat': '-33.9321', 'lon': '18.8602'}]), 200);
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

      final service = RouteService(client: client);

      final result = await service.getRoute(origin: 'Cape Town', destination: 'Stellenbosch');

      expect(result.origin.latitude, -33.9249);
      expect(result.destination.longitude, 18.8602);
      expect(result.points.length, 2);
      expect(result.distanceMeters, 50000.0);
      expect(result.durationSeconds, 3000.0);
    });

    test('throws RouteServiceException when geocoding finds nothing', () async {
      final client = MockClient((request) async => http.Response(jsonEncode([]), 200));
      final service = RouteService(client: client);

      expect(
        () => service.getRoute(origin: 'Nowhereville', destination: 'Stellenbosch'),
        throwsA(isA<RouteServiceException>()),
      );
    });

    test('throws RouteServiceException when OSRM finds no route', () async {
      final client = MockClient((request) async {
        if (request.url.path.contains('/search')) {
          return http.Response(jsonEncode([{'lat': '-33.9249', 'lon': '18.4241'}]), 200);
        }
        return http.Response(jsonEncode({'code': 'NoRoute', 'routes': []}), 200);
      });
      final service = RouteService(client: client);

      expect(
        () => service.getRoute(origin: 'Cape Town', destination: 'Stellenbosch'),
        throwsA(isA<RouteServiceException>()),
      );
    });
  });
}
