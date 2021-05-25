Feature: Scheduling Functionality

Scenario: Loading Scheduling module into context
    Given an application with the following registrations
        | Kind   | Type          |
        | Module | EventHandling |
        | Module | Scheduling    |
    Then starting lifetime scope should succeed

Scenario: Running an application with a scheduled cron event
    And an application with the following registrations
        | Kind          | Type            |
        | Module        | EventHandling   |
        | Module        | Scheduling      |
        | Handler       | ScheduleHandler |
        | SingletonType | EventCounter    |
        | Type          | ScheduleConfig  |
    When the application has been running for 2000 milliseconds
    Then there should be at least this many events produced
        | EventType          | Count |
        | CronEvent          | 1     |
        | TypeScheduledEvent | 1     |
        | IntervalEvent      | 4     |
    And events with payload should contain the correct payload