import 'dart:convert';

import 'package:fleeter/core/config/api_config.dart';
import 'package:http/http.dart' as http;

class AuthApiException implements Exception {
  AuthApiException(this.message);

  final String message;

  @override
  String toString() => message;
}

class AuthUser {
  const AuthUser({required this.id, required this.name, required this.phone});

  final String id;
  final String name;
  final String phone;

  factory AuthUser.fromJson(Map<String, dynamic> json) => AuthUser(
        id: json['id'] as String,
        name: json['name'] as String,
        phone: json['phone'] as String,
      );
}

class VerifyResult {
  const VerifyResult({required this.token, required this.user});

  final String token;
  final AuthUser user;
}

class AuthApi {
  AuthApi({http.Client? client}) : _client = client ?? http.Client();

  final http.Client _client;

  Uri _uri(String path) => Uri.parse('${ApiConfig.baseUrl}$path');

  Future<void> signup({required String name, required String phone}) async {
    final response = await _client.post(
      _uri('/auth/signup'),
      headers: const {'Content-Type': 'application/json'},
      body: jsonEncode({'name': name, 'phone': phone}),
    );
    if (response.statusCode != 202) {
      throw AuthApiException(_extractMessage(response));
    }
  }

  Future<void> login({required String phone}) async {
    final response = await _client.post(
      _uri('/auth/login'),
      headers: const {'Content-Type': 'application/json'},
      body: jsonEncode({'phone': phone}),
    );
    if (response.statusCode != 202) {
      throw AuthApiException(_extractMessage(response));
    }
  }

  Future<VerifyResult> verify({required String phone, required String code}) async {
    final response = await _client.post(
      _uri('/auth/verify'),
      headers: const {'Content-Type': 'application/json'},
      body: jsonEncode({'phone': phone, 'code': code}),
    );
    if (response.statusCode != 200) {
      throw AuthApiException(_extractMessage(response));
    }
    final body = jsonDecode(response.body) as Map<String, dynamic>;
    return VerifyResult(
      token: body['token'] as String,
      user: AuthUser.fromJson(body['user'] as Map<String, dynamic>),
    );
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
