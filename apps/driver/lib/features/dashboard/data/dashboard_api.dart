import 'dart:convert';

import 'package:fleeter/core/config/api_config.dart';
import 'package:http/http.dart' as http;

class DashboardApiException implements Exception {
  DashboardApiException(this.message);

  final String message;

  @override
  String toString() => message;
}

class DashboardDriver {
  const DashboardDriver({required this.id, required this.name, required this.phone, this.license});

  final String id;
  final String name;
  final String phone;
  final String? license;

  factory DashboardDriver.fromJson(Map<String, dynamic> json) => DashboardDriver(
        id: json['id'] as String,
        name: json['name'] as String,
        phone: json['phone'] as String,
        license: json['license'] as String?,
      );
}

class DashboardVehicle {
  const DashboardVehicle({required this.id, required this.name, required this.registration});

  final String id;
  final String name;
  final String registration;

  factory DashboardVehicle.fromJson(Map<String, dynamic> json) => DashboardVehicle(
        id: json['id'] as String,
        name: json['name'] as String,
        registration: json['registration'] as String,
      );
}

class DashboardTrip {
  const DashboardTrip({
    required this.id,
    required this.origin,
    required this.destination,
    required this.status,
    this.startedAt,
    this.endedAt,
  });

  final String id;
  final String origin;
  final String destination;
  final String status;
  final DateTime? startedAt;
  final DateTime? endedAt;

  factory DashboardTrip.fromJson(Map<String, dynamic> json) => DashboardTrip(
        id: json['id'] as String,
        origin: json['origin'] as String,
        destination: json['destination'] as String,
        status: json['status'] as String,
        startedAt: json['startedAt'] == null ? null : DateTime.parse(json['startedAt'] as String),
        endedAt: json['endedAt'] == null ? null : DateTime.parse(json['endedAt'] as String),
      );
}

class DashboardData {
  const DashboardData({required this.driver, this.vehicle, this.trip});

  final DashboardDriver driver;
  final DashboardVehicle? vehicle;
  final DashboardTrip? trip;

  factory DashboardData.fromJson(Map<String, dynamic> json) => DashboardData(
        driver: DashboardDriver.fromJson(json['driver'] as Map<String, dynamic>),
        vehicle: json['vehicle'] == null
            ? null
            : DashboardVehicle.fromJson(json['vehicle'] as Map<String, dynamic>),
        trip: json['trip'] == null ? null : DashboardTrip.fromJson(json['trip'] as Map<String, dynamic>),
      );
}

class DashboardApi {
  DashboardApi({http.Client? client}) : _client = client ?? http.Client();

  final http.Client _client;

  Future<DashboardData> fetchDashboard({required String token}) async {
    final response = await _client.get(
      Uri.parse('${ApiConfig.baseUrl}/driver/dashboard'),
      headers: {'Authorization': 'Bearer $token'},
    );
    if (response.statusCode != 200) {
      throw DashboardApiException(_extractMessage(response));
    }
    return DashboardData.fromJson(jsonDecode(response.body) as Map<String, dynamic>);
  }

  Future<DashboardTrip> startTrip({required String token, required String tripId}) async {
    final response = await _client.post(
      Uri.parse('${ApiConfig.baseUrl}/trips/$tripId/start'),
      headers: {'Authorization': 'Bearer $token'},
    );
    if (response.statusCode != 200) {
      throw DashboardApiException(_extractMessage(response));
    }
    return DashboardTrip.fromJson(jsonDecode(response.body) as Map<String, dynamic>);
  }

  Future<DashboardTrip> endTrip({required String token, required String tripId}) async {
    final response = await _client.post(
      Uri.parse('${ApiConfig.baseUrl}/trips/$tripId/end'),
      headers: {'Authorization': 'Bearer $token'},
    );
    if (response.statusCode != 200) {
      throw DashboardApiException(_extractMessage(response));
    }
    return DashboardTrip.fromJson(jsonDecode(response.body) as Map<String, dynamic>);
  }

  String _extractMessage(http.Response response) {
    try {
      final body = jsonDecode(response.body) as Map<String, dynamic>;
      return body['message'] as String? ?? 'Something went wrong';
    } catch (_) {
      return 'Something went wrong';
    }
  }
}
