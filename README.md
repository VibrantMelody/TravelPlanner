# TravelPlanner

**TravelPlanner** is a mobile application developed as part of a college project. It is designed to streamline the process of planning and managing trips, offering a user-friendly interface and robust functionality for both regular users and administrators.

## Project Overview

The application is built using Xamarin.Forms, targeting Android devices (MonoAndroid, Version=v13.0) with shared logic implemented in .NET Standard 2.0. It follows the MVVM (Model-View-ViewModel) design pattern to ensure a clean separation of concerns and maintainable code.

## Key Features

### User Features
- **Authentication**: Secure user registration, login, and password reset functionality.
- **Trip Management**: 
  - Create, edit, and delete trips.
  - Add and manage participants for trips.
- **Responsive UI**: A clean and intuitive interface optimized for Android devices.

### Admin Features
- **User Management**: Add, edit, and delete users.
- **Statistics Dashboard**: View trip and user statistics in a dedicated admin interface.
- **Trip Management**: Admins can manage all trips and participants.

### Additional Functionalities
- **Popups**: Modular popups for adding users, managing trips, and displaying statistics.
- **Local Storage**: SQLite database for storing user and trip data.
- **Permissions**: Utilizes the Plugin.Permissions library for handling runtime permissions.

## Architecture

The project is structured to ensure scalability and maintainability:
- **Model**: Defines data models for users, trips, and participants.
- **View**: Contains XAML pages for the user interface.
- **ViewModel**: Implements business logic and data binding using the MVVM pattern.
- **DatabaseHelper**: Handles database operations with SQLite.

## Purpose

This project was developed as a college assignment to demonstrate proficiency in mobile app development, .NET technologies, and user interface design. It serves as a practical learning experience in building cross-platform applications.

---
