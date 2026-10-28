import 'package:fleeter/core/config/api_config.dart';
import 'package:flutter/foundation.dart';
import 'package:flutter_test/flutter_test.dart';

void main() {
  tearDown(() {
    debugDefaultTargetPlatformOverride = null;
  });

  test('defaults to the Android emulator alias on Android', () {
    debugDefaultTargetPlatformOverride = TargetPlatform.android;
    expect(ApiConfig.baseUrl, 'http://10.0.2.2:3000');
  });

  test('defaults to localhost on iOS, since 10.0.2.2 does not resolve there', () {
    debugDefaultTargetPlatformOverride = TargetPlatform.iOS;
    expect(ApiConfig.baseUrl, 'http://localhost:3000');
  });

  test('defaults to localhost on macOS', () {
    debugDefaultTargetPlatformOverride = TargetPlatform.macOS;
    expect(ApiConfig.baseUrl, 'http://localhost:3000');
  });
}
