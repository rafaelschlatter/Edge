# RaaLabs Edge Framework

[![Coverage](https://sonarcloud.io/api/project_badges/measure?project=RaaLabs_Edge&metric=coverage)](https://sonarcloud.io/dashboard?id=RaaLabs_Edge)

# About
This framework is created to ease the building of applications in the RaaLabs ecosystem.
It focuses heavily on application execution mainly through propagation of asynchronous events,
and provides an event propagation backbone as a central module. It is designed for Dependency Injection
using Autofac modules.

# Modules

## [Core](Edge/README.md)

This module contains the bare minimum of requirements that an application will need:
- Application builder with type registration for Autofac context
- Application class, supporting handlers and async tasks
- Logging using Serilog

## [EventHandling](Edge.Modules.EventHandling/README.md)

This module provides the application with an event handling backbone. Can be thought of as a
replacement for Kafka for communication between application components, but with a "push" configuration
rather than a "pull" configuration.

## [Configuration](Edge.Modules.Configuration/README.md)

This module allows the application to load configuration files upon type activation.

## [EdgeHub](Edge.Modules.EdgeHub/README.md)

This module bridges the communication with EdgeHub using the EventHandling module.
