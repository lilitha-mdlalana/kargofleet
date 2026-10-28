import 'package:fleeter/features/auth/data/auth_session.dart';
import 'package:fleeter/features/dashboard/data/dashboard_api.dart';
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

  @override
  void initState() {
    super.initState();
    _future = _api.fetchDashboard(token: AuthSession.instance.token ?? '');
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
                          : Text('${data.vehicle!.name} • ${data.vehicle!.registration}'),
                    ),
                  ),
                  const SizedBox(height: 16),
                  Card(
                    child: Padding(
                      padding: const EdgeInsets.all(16),
                      child: data.trip == null
                          ? const Text('No trip scheduled')
                          : Text('${data.trip!.origin} → ${data.trip!.destination} (${data.trip!.status})'),
                    ),
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
