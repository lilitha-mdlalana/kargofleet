import 'package:fleeter/core/config/map_config.dart';
import 'package:fleeter/core/theme/app_theme.dart';
import 'package:fleeter/features/dashboard/data/dashboard_api.dart';
import 'package:fleeter/features/map/data/route_service.dart';
import 'package:flutter/material.dart';
import 'package:flutter_map/flutter_map.dart';

class MapScreen extends StatefulWidget {
  const MapScreen({super.key, required this.trip, this.routeService});

  final DashboardTrip trip;
  final RouteService? routeService;

  @override
  State<MapScreen> createState() => _MapScreenState();
}

class _MapScreenState extends State<MapScreen> {
  late final RouteService _service = widget.routeService ?? RouteService();
  late final Future<RouteResult> _future;
  final MapController _mapController = MapController();

  @override
  void initState() {
    super.initState();
    _future = _service.getRoute(origin: widget.trip.origin, destination: widget.trip.destination);
  }

  void _fitBounds(RouteResult result) {
    WidgetsBinding.instance.addPostFrameCallback((_) {
      _mapController.fitCamera(
        CameraFit.bounds(
          bounds: LatLngBounds.fromPoints(result.points),
          padding: const EdgeInsets.all(48),
        ),
      );
    });
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      body: Stack(
        children: [
          FutureBuilder<RouteResult>(
            future: _future,
            builder: (context, snapshot) {
              if (snapshot.connectionState != ConnectionState.done) {
                return const Center(child: CircularProgressIndicator());
              }
              if (snapshot.hasError) {
                final message = snapshot.error is RouteServiceException
                    ? (snapshot.error as RouteServiceException).message
                    : 'Could not calculate a route';
                return Center(
                  child: Padding(
                    padding: const EdgeInsets.all(24),
                    child: Text(message, style: const TextStyle(color: Colors.red)),
                  ),
                );
              }

              final result = snapshot.data!;
              _fitBounds(result);

              return FlutterMap(
                mapController: _mapController,
                options: MapOptions(initialCenter: result.origin, initialZoom: 10),
                children: [
                  TileLayer(
                    urlTemplate: MapConfig.tileUrlTemplate,
                    userAgentPackageName: MapConfig.tileUserAgentPackageName,
                  ),
                  PolylineLayer(
                    polylines: [
                      Polyline(points: result.points, color: AppColors.primary, strokeWidth: 4),
                    ],
                  ),
                  MarkerLayer(
                    markers: [
                      Marker(
                        point: result.origin,
                        child: const Icon(Icons.circle, color: AppColors.textPrimary, size: 16),
                      ),
                      Marker(
                        point: result.destination,
                        child: const Icon(Icons.circle, color: AppColors.primary, size: 16),
                      ),
                    ],
                  ),
                ],
              );
            },
          ),
          Positioned(
            top: 16,
            left: 16,
            child: SafeArea(
              child: _BackButton(onTap: () => Navigator.of(context).pop()),
            ),
          ),
          Positioned(
            top: 16,
            left: 16,
            right: 16,
            child: SafeArea(
              child: Padding(
                padding: const EdgeInsets.only(top: 56),
                child: Card(
                  child: Padding(
                    padding: const EdgeInsets.all(16),
                    child: Column(
                      crossAxisAlignment: CrossAxisAlignment.start,
                      children: [
                        _AddressRow(color: AppColors.textPrimary, text: widget.trip.origin),
                        const SizedBox(height: 8),
                        _AddressRow(color: AppColors.primary, text: widget.trip.destination),
                      ],
                    ),
                  ),
                ),
              ),
            ),
          ),
          Positioned(
            left: 16,
            right: 16,
            bottom: 16,
            child: SafeArea(
              top: false,
              child: FutureBuilder<RouteResult>(
                future: _future,
                builder: (context, snapshot) {
                  if (snapshot.connectionState != ConnectionState.done || !snapshot.hasData) {
                    return const SizedBox.shrink();
                  }
                  final result = snapshot.data!;
                  final km = (result.distanceMeters / 1000).toStringAsFixed(1);
                  final minutes = (result.durationSeconds / 60).round();
                  return Card(
                    child: Padding(
                      padding: const EdgeInsets.all(16),
                      child: Column(
                        crossAxisAlignment: CrossAxisAlignment.start,
                        children: [
                          Text('$km km', style: Theme.of(context).textTheme.titleMedium),
                          const SizedBox(height: 4),
                          Text('$minutes min', style: Theme.of(context).textTheme.bodyMedium),
                        ],
                      ),
                    ),
                  );
                },
              ),
            ),
          ),
        ],
      ),
    );
  }
}

class _BackButton extends StatelessWidget {
  const _BackButton({required this.onTap});

  final VoidCallback onTap;

  @override
  Widget build(BuildContext context) {
    return Material(
      color: AppColors.textPrimary,
      borderRadius: BorderRadius.circular(12),
      child: InkWell(
        borderRadius: BorderRadius.circular(12),
        onTap: onTap,
        child: const Padding(
          padding: EdgeInsets.all(12),
          child: Icon(Icons.arrow_back, color: Colors.white, size: 20),
        ),
      ),
    );
  }
}

class _AddressRow extends StatelessWidget {
  const _AddressRow({required this.color, required this.text});

  final Color color;
  final String text;

  @override
  Widget build(BuildContext context) {
    return Row(
      children: [
        Icon(Icons.circle, color: color, size: 10),
        const SizedBox(width: 8),
        Expanded(child: Text(text, style: Theme.of(context).textTheme.bodyMedium)),
      ],
    );
  }
}
