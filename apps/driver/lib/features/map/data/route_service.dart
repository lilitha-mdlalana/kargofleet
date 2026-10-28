import 'dart:convert';

import 'package:fleeter/core/config/map_config.dart';
import 'package:http/http.dart' as http;
import 'package:latlong2/latlong.dart';

class RouteServiceException implements Exception {
  RouteServiceException(this.message);

  final String message;

  @override
  String toString() => message;
}

class RouteResult {
  const RouteResult({
    required this.origin,
    required this.destination,
    required this.points,
    required this.distanceMeters,
    required this.durationSeconds,
  });

  final LatLng origin;
  final LatLng destination;
  final List<LatLng> points;
  final double distanceMeters;
  final double durationSeconds;
}

class RouteService {
  RouteService({http.Client? client}) : _client = client ?? http.Client();

  final http.Client _client;

  Future<LatLng> _geocode(String place) async {
    final uri = Uri.parse('${MapConfig.geocodingBaseUrl}/search').replace(
      queryParameters: {'q': place, 'format': 'json', 'limit': '1'},
    );
    final response = await _client.get(uri, headers: {'User-Agent': MapConfig.tileUserAgentPackageName});
    if (response.statusCode != 200) {
      throw RouteServiceException('Could not look up "$place"');
    }
    final results = jsonDecode(response.body) as List<dynamic>;
    if (results.isEmpty) {
      throw RouteServiceException('No location found for "$place"');
    }
    final first = results.first as Map<String, dynamic>;
    return LatLng(double.parse(first['lat'] as String), double.parse(first['lon'] as String));
  }

  Future<RouteResult> getRoute({required String origin, required String destination}) async {
    final originLatLng = await _geocode(origin);
    final destinationLatLng = await _geocode(destination);

    final uri = Uri.parse(
      '${MapConfig.routingBaseUrl}/route/v1/driving/'
      '${originLatLng.longitude},${originLatLng.latitude};'
      '${destinationLatLng.longitude},${destinationLatLng.latitude}'
      '?overview=full&geometries=geojson',
    );
    final response = await _client.get(uri);
    if (response.statusCode != 200) {
      throw RouteServiceException('Could not calculate a route');
    }
    final body = jsonDecode(response.body) as Map<String, dynamic>;
    final routes = body['routes'] as List<dynamic>?;
    if (body['code'] != 'Ok' || routes == null || routes.isEmpty) {
      throw RouteServiceException('No route found between these locations');
    }
    final route = routes.first as Map<String, dynamic>;
    final geometry = route['geometry'] as Map<String, dynamic>;
    final coords = (geometry['coordinates'] as List<dynamic>)
        .map((c) => LatLng((c as List<dynamic>)[1] as double, c[0] as double))
        .toList();

    return RouteResult(
      origin: originLatLng,
      destination: destinationLatLng,
      points: coords,
      distanceMeters: (route['distance'] as num).toDouble(),
      durationSeconds: (route['duration'] as num).toDouble(),
    );
  }
}
