class MapConfig {
  MapConfig._();

  static const _geocodingOverride = String.fromEnvironment('GEOCODING_BASE_URL');
  static const _routingOverride = String.fromEnvironment('ROUTING_BASE_URL');
  static const _tileUrlOverride = String.fromEnvironment('TILE_URL_TEMPLATE');

  /// Nominatim (OpenStreetMap) geocoding endpoint. Overridable via
  /// --dart-define so the provider can be swapped without a code change,
  /// per CLAUDE.md's maps guidance.
  static String get geocodingBaseUrl =>
      _geocodingOverride.isNotEmpty ? _geocodingOverride : 'https://nominatim.openstreetmap.org';

  /// OSRM public demo routing server. Same swappability note as above --
  /// the public demo server has usage limits and isn't meant for production.
  static String get routingBaseUrl =>
      _routingOverride.isNotEmpty ? _routingOverride : 'https://router.project-osrm.org';

  /// OSM standard tile server. Fine for this dev-scale demo; a production
  /// deployment should move to a paid or self-hosted tile provider.
  static String get tileUrlTemplate =>
      _tileUrlOverride.isNotEmpty ? _tileUrlOverride : 'https://tile.openstreetmap.org/{z}/{x}/{y}.png';

  /// OSM's tile usage policy expects an identifiable User-Agent.
  static const tileUserAgentPackageName = 'com.kargofleet.driver';
}
