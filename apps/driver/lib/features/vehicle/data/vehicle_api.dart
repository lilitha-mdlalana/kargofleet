import 'dart:convert';

import 'package:fleeter/core/config/api_config.dart';
import 'package:fleeter/features/dashboard/data/dashboard_api.dart';
import 'package:http/http.dart' as http;

class VehicleApiException implements Exception {
  VehicleApiException(this.message);

  final String message;

  @override
  String toString() => message;
}

class VehicleApi {
  VehicleApi({http.Client? client}) : _client = client ?? http.Client();

  final http.Client _client;

  Future<DashboardVehicle> fetchVehicle({required String token}) async {
    final response = await _client.get(
      Uri.parse('${ApiConfig.baseUrl}/vehicle'),
      headers: {'Authorization': 'Bearer $token'},
    );
    if (response.statusCode != 200) {
      throw VehicleApiException(_extractMessage(response));
    }
    return DashboardVehicle.fromJson(jsonDecode(response.body) as Map<String, dynamic>);
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
