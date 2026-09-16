import 'package:flutter/material.dart';
import 'package:flutter_test/flutter_test.dart';

import 'package:fleeter/main.dart';

void main() {
  testWidgets('Splash screen shows app name', (WidgetTester tester) async {
    await tester.pumpWidget(const KargoFleetApp());

    expect(find.text('KargoFleet'), findsOneWidget);
    expect(find.byType(CircularProgressIndicator), findsOneWidget);
  });
}
