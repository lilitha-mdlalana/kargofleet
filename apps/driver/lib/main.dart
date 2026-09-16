import 'package:flutter/material.dart';
import 'package:fleeter/core/theme/app_theme.dart';
import 'package:fleeter/features/splash/splash_screen.dart';

void main() {
  runApp(const KargoFleetApp());
}

class KargoFleetApp extends StatelessWidget {
  const KargoFleetApp({super.key});

  @override
  Widget build(BuildContext context) {
    return MaterialApp(
      title: 'KargoFleet',
      debugShowCheckedModeBanner: false,
      theme: AppTheme.light,
      home: const SplashScreen(),
    );
  }
}
