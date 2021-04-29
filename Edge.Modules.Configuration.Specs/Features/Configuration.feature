Feature: Configuration Functionality

Scenario: Loading Configuration module into context
	Given an ApplicationBuilder
	Given Configuration module is registered
	Given application has been built

	Then Starting lifetime scope should succeed

Scenario: Resolving a configuration object from the data directory
	Given an ApplicationBuilder
	Given Configuration module is registered
	Given a mock filesystem containing configuration file in the data directory

	Given application has been built
	When Resolving configuration class
	Then the configuration object should contain correct configuration data

Scenario: Resolving a configuration object from the config directory
	Given an ApplicationBuilder
	Given Configuration module is registered
	Given a mock filesystem containing configuration file in the config directory

	Given application has been built
	When Resolving configuration class
	Then the configuration object should contain correct configuration data

Scenario: Changing a configuration object while application is running
	Given an ApplicationBuilder
	Given Configuration module is registered
	Given a mock filesystem containing configuration file in the data directory

	Given application has been built
	And application has been running for one second
	When configuration file is changed
	Then application restart will be triggered within two seconds
