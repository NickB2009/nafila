import 'package:flutter/material.dart';

class CheckInState extends ChangeNotifier {
  bool _isCheckedIn = false;
  String? _selectedSalonId;
  DateTime? _checkInTime;
  
  bool get isCheckedIn => _isCheckedIn;
  String? get selectedSalonId => _selectedSalonId;
  DateTime? get checkInTime => _checkInTime;
  
  void checkIn(String salonId) {
    _isCheckedIn = true;
    _selectedSalonId = salonId;
    _checkInTime = DateTime.now();
    notifyListeners();
  }
  
  void checkOut() {
    _isCheckedIn = false;
    _selectedSalonId = null;
    _checkInTime = null;
    notifyListeners();
  }
  
  void updateSalonId(String salonId) {
    _selectedSalonId = salonId;
    notifyListeners();
  }
} 