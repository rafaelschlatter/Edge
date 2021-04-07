Feature: Configuration Functionality

Scenario: Loading Configuration module into context
	Given an ApplicationBuilder
	Given Configuration module is registered
	Given application has been built

	Then Starting lifetime scope should succeed

Scenario: Resolving a configuration object
	Given an ApplicationBuilder
	Given Configuration module is registered
	Given a mock filesystem containing configuration file

	Given application has been built
	When Resolving configuration class
	Then the configuration object should contain correct configuration data
