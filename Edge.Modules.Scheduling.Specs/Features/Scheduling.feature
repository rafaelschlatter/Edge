Feature: EdgeHub Functionality

Scenario: Loading Scheduling module into context
	Given an application with the following registrations
		| Kind   | Type          |
		| Module | EventHandling |
		| Module | Scheduling    |
	Then starting lifetime scope should succeed