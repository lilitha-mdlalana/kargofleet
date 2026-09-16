import 'package:flutter/material.dart';

class AppColors {
  AppColors._();

  static const primary = Color(0xFF2E9E4F);
  static const background = Color(0xFFFFFFFF);
  static const surface = Color(0xFFF6F8F6);
  static const textPrimary = Color(0xFF1A1C1A);
  static const textSecondary = Color(0xFF6B756E);
}

class AppTheme {
  AppTheme._();

  static ThemeData get light {
    return ThemeData(
      useMaterial3: true,
      scaffoldBackgroundColor: AppColors.background,
      colorScheme: ColorScheme.fromSeed(
        seedColor: AppColors.primary,
        primary: AppColors.primary,
        surface: AppColors.background,
      ),
      textTheme: const TextTheme(
        headlineMedium: TextStyle(
          fontWeight: FontWeight.w600,
          color: AppColors.textPrimary,
        ),
        bodyMedium: TextStyle(color: AppColors.textSecondary),
      ),
      cardTheme: CardThemeData(
        color: AppColors.surface,
        elevation: 0,
        shape: RoundedRectangleBorder(
          borderRadius: BorderRadius.circular(16),
        ),
      ),
    );
  }
}
