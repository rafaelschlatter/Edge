Feature: EdgeHub Functionality

Scenario: Loading Configuration module into context
	Given an Autofac context
	Given module RaaLabs.Edge.Modules.Configuration.Configuration from RaaLabs.Edge.Modules.Configuration is registered

	When Building application container
	Then Starting lifetime scope should succeed

Scenario: Resolving a configuration object
	Given an Autofac context
	Given module RaaLabs.Edge.Modules.Configuration.Configuration from RaaLabs.Edge.Modules.Configuration is registered

	Given application container has been built
