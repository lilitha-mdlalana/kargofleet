import 'package:fleeter/features/auth/data/auth_session.dart';
import 'package:fleeter/features/dashboard/data/dashboard_api.dart';
import 'package:fleeter/features/vehicle/data/vehicle_api.dart';
import 'package:flutter/material.dart';

class VehicleScreen extends StatefulWidget {
  const VehicleScreen({super.key, this.vehicleApi});

  final VehicleApi? vehicleApi;

  @override
  State<VehicleScreen> createState() => _VehicleScreenState();
}

class _VehicleScreenState extends State<VehicleScreen> {
  late final VehicleApi _api = widget.vehicleApi ?? VehicleApi();
  late final Future<DashboardVehicle> _future;

  @override
  void initState() {
    super.initState();
    _future = _api.fetchVehicle(token: AuthSession.instance.token ?? '');
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(title: const Text('Vehicle Details')),
      body: SafeArea(
        child: FutureBuilder<DashboardVehicle>(
          future: _future,
          builder: (context, snapshot) {
            if (snapshot.connectionState != ConnectionState.done) {
              return const Center(child: CircularProgressIndicator());
            }
            if (snapshot.hasError) {
              final message = snapshot.error is VehicleApiException
                  ? (snapshot.error as VehicleApiException).message
                  : 'Could not reach the server';
              return Center(
                child: Text(message, style: const TextStyle(color: Colors.red)),
              );
            }
            final vehicle = snapshot.data!;
            return Padding(
              padding: const EdgeInsets.all(24),
              child: Card(
                child: Padding(
                  padding: const EdgeInsets.all(16),
                  child: Column(
                    crossAxisAlignment: CrossAxisAlignment.start,
                    children: [
                      Text(vehicle.name, style: Theme.of(context).textTheme.titleMedium),
                      const SizedBox(height: 4),
                      Text(vehicle.registration, style: Theme.of(context).textTheme.bodyMedium),
                    ],
                  ),
                ),
              ),
            );
          },
        ),
      ),
    );
  }
}
