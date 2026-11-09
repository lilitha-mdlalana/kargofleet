import 'dart:async';

import 'package:fleeter/core/theme/app_theme.dart';
import 'package:fleeter/features/auth/data/auth_session.dart';
import 'package:fleeter/features/dashboard/data/dashboard_api.dart';
import 'package:fleeter/features/map/map_screen.dart';
import 'package:flutter/material.dart';

class DashboardScreen extends StatefulWidget {
  const DashboardScreen({super.key, this.dashboardApi});

  final DashboardApi? dashboardApi;

  @override
  State<DashboardScreen> createState() => _DashboardScreenState();
}

class _DashboardScreenState extends State<DashboardScreen> {
  late final DashboardApi _api = widget.dashboardApi ?? DashboardApi();
  late final Future<DashboardData> _future;

  DashboardTrip? _trip;
  bool _isTripActionLoading = false;
  String? _tripActionError;
  Timer? _tickTimer;

  @override
  void initState() {
    super.initState();
    _future = _api.fetchDashboard(token: AuthSession.instance.token ?? '');
  }

  @override
  void dispose() {
    _tickTimer?.cancel();
    super.dispose();
  }

  void _syncTicker() {
    final shouldTick = _trip?.status == 'active';
    if (shouldTick && _tickTimer == null) {
      _tickTimer = Timer.periodic(const Duration(seconds: 30), (_) {
        if (mounted) setState(() {});
      });
    } else if (!shouldTick && _tickTimer != null) {
      _tickTimer!.cancel();
      _tickTimer = null;
    }
  }

  Future<void> _handleTripAction() async {
    if (_trip == null) return;
    setState(() {
      _isTripActionLoading = true;
      _tripActionError = null;
    });
    try {
      final token = AuthSession.instance.token ?? '';
      final updated = _trip!.status == 'scheduled'
          ? await _api.startTrip(token: token, tripId: _trip!.id)
          : await _api.endTrip(token: token, tripId: _trip!.id);
      setState(() {
        _trip = updated;
        _syncTicker();
      });
    } on DashboardApiException catch (e) {
      setState(() => _tripActionError = e.message);
    } catch (_) {
      setState(() => _tripActionError = 'Could not reach the server');
    } finally {
      if (mounted) setState(() => _isTripActionLoading = false);
    }
  }

  String _formatElapsed(Duration d) {
    final h = d.inHours;
    final m = d.inMinutes.remainder(60);
    return h > 0 ? '${h}h ${m}m' : '${m}m';
  }

  Widget _buildWorkingTime(DashboardTrip trip) {
    if (trip.status == 'active' && trip.startedAt != null) {
      return Text('Working time: ${_formatElapsed(DateTime.now().toUtc().difference(trip.startedAt!))}');
    }
    if (trip.status == 'completed' && trip.startedAt != null && trip.endedAt != null) {
      return Text('Total time: ${_formatElapsed(trip.endedAt!.difference(trip.startedAt!))}');
    }
    return const Text('Not started yet');
  }

  Widget? _buildTripActionButton(DashboardTrip trip) {
    if (trip.status == 'scheduled') {
      return FilledButton(
        onPressed: _isTripActionLoading ? null : _handleTripAction,
        child: const Text('Check In'),
      );
    }
    if (trip.status == 'active') {
      return FilledButton(
        style: FilledButton.styleFrom(backgroundColor: AppColors.textSecondary),
        onPressed: _isTripActionLoading ? null : _handleTripAction,
        child: const Text('Check Out'),
      );
    }
    return null;
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      body: SafeArea(
        child: FutureBuilder<DashboardData>(
          future: _future,
          builder: (context, snapshot) {
            if (snapshot.connectionState != ConnectionState.done) {
              return const Center(child: CircularProgressIndicator());
            }
            if (snapshot.hasError) {
              final message = snapshot.error is DashboardApiException
                  ? (snapshot.error as DashboardApiException).message
                  : 'Could not reach the server';
              return Center(
                child: Text(message, style: const TextStyle(color: Colors.red)),
              );
            }
            final data = snapshot.data!;
            if (_trip == null && data.trip != null) {
              _trip = data.trip;
              WidgetsBinding.instance.addPostFrameCallback((_) {
                if (mounted) _syncTicker();
              });
            }
            final trip = _trip;
            final actionButton = trip == null ? null : _buildTripActionButton(trip);

            return SingleChildScrollView(
              padding: const EdgeInsets.all(24),
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  Text('Hi, ${data.driver.name}', style: Theme.of(context).textTheme.headlineMedium),
                  const SizedBox(height: 24),
                  Card(
                    child: Padding(
                      padding: const EdgeInsets.all(16),
                      child: data.vehicle == null
                          ? const Text('No vehicle assigned yet')
                          : Column(
                              crossAxisAlignment: CrossAxisAlignment.start,
                              children: [
                                Text(
                                  data.vehicle!.name,
                                  style: Theme.of(context).textTheme.titleMedium,
                                ),
                                const SizedBox(height: 4),
                                Text(
                                  data.vehicle!.registration,
                                  style: Theme.of(context).textTheme.bodyMedium,
                                ),
                              ],
                            ),
                    ),
                  ),
                  const SizedBox(height: 16),
                  trip == null
                      ? const Card(
                          child: Padding(
                            padding: EdgeInsets.all(16),
                            child: Text('No trip scheduled'),
                          ),
                        )
                      : Card(
                          child: Column(
                            children: [
                              InkWell(
                                borderRadius: BorderRadius.circular(16),
                                onTap: () => Navigator.of(context).push(
                                  MaterialPageRoute(builder: (_) => MapScreen(trip: trip)),
                                ),
                                child: Padding(
                                  padding: const EdgeInsets.all(16),
                                  child: Column(
                                    crossAxisAlignment: CrossAxisAlignment.start,
                                    children: [
                                      Text(
                                        '${trip.origin} → ${trip.destination}',
                                        style: Theme.of(context).textTheme.titleMedium,
                                      ),
                                      const SizedBox(height: 4),
                                      Text(
                                        trip.status,
                                        style: Theme.of(context).textTheme.bodyMedium,
                                      ),
                                      const SizedBox(height: 4),
                                      _buildWorkingTime(trip),
                                    ],
                                  ),
                                ),
                              ),
                              if (actionButton != null)
                                Padding(
                                  padding: const EdgeInsets.fromLTRB(16, 0, 16, 16),
                                  child: Column(
                                    crossAxisAlignment: CrossAxisAlignment.stretch,
                                    children: [
                                      actionButton,
                                      if (_tripActionError != null) ...[
                                        const SizedBox(height: 8),
                                        Text(
                                          _tripActionError!,
                                          style: const TextStyle(color: Colors.red),
                                        ),
                                      ],
                                    ],
                                  ),
                                ),
                            ],
                          ),
                        ),
                  const SizedBox(height: 24),
                  Row(
                    children: const [
                      Expanded(
                        child: _PlaceholderNavCard(
                          icon: Icons.local_shipping_outlined,
                          label: 'Vehicle Detail',
                        ),
                      ),
                      SizedBox(width: 12),
                      Expanded(
                        child: _PlaceholderNavCard(icon: Icons.badge_outlined, label: 'Driver Detail'),
                      ),
                      SizedBox(width: 12),
                      Expanded(
                        child: _PlaceholderNavCard(icon: Icons.build_outlined, label: 'Transport Tool'),
                      ),
                    ],
                  ),
                ],
              ),
            );
          },
        ),
      ),
    );
  }
}

class _PlaceholderNavCard extends StatelessWidget {
  const _PlaceholderNavCard({required this.icon, required this.label});

  final IconData icon;
  final String label;

  @override
  Widget build(BuildContext context) {
    return Card(
      child: Padding(
        padding: const EdgeInsets.symmetric(vertical: 16),
        child: Column(
          children: [
            Icon(icon, color: AppColors.textSecondary),
            const SizedBox(height: 8),
            Text(label, textAlign: TextAlign.center, style: Theme.of(context).textTheme.bodyMedium),
          ],
        ),
      ),
    );
  }
}
