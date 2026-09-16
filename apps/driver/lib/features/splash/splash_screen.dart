import 'package:flutter/material.dart';
import 'package:fleeter/core/theme/app_theme.dart';

class SplashScreen extends StatelessWidget {
  const SplashScreen({super.key});

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      body: SafeArea(
        child: Center(
          child: Column(
            mainAxisAlignment: MainAxisAlignment.center,
            children: [
              Container(
                width: 96,
                height: 96,
                decoration: BoxDecoration(
                  color: AppColors.primary,
                  borderRadius: BorderRadius.circular(24),
                ),
                child: const Icon(
                  Icons.local_shipping_outlined,
                  color: Colors.white,
                  size: 48,
                ),
              ),
              const SizedBox(height: 24),
              Text(
                'KargoFleet',
                style: Theme.of(context).textTheme.headlineMedium,
              ),
              const SizedBox(height: 8),
              Text(
                'Fleet operations, simplified',
                style: Theme.of(context).textTheme.bodyMedium,
              ),
              const SizedBox(height: 48),
              const SizedBox(
                width: 24,
                height: 24,
                child: CircularProgressIndicator(
                  strokeWidth: 2.5,
                  color: AppColors.primary,
                ),
              ),
            ],
          ),
        ),
      ),
    );
  }
}
