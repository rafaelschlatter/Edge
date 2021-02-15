Feature: EdgeHub Functionality

Scenario: Loading EdgeHub module into context
	Given an Autofac context
	Given module RaaLabs.Edge.Modules.EdgeHub.EdgeHub from RaaLabs.Edge.Modules.EdgeHub is registered

	When Building application container
	Then Starting lifetime scope should succeed

Scenario: Receiving an event from EdgeHub
	Given an Autofac context
	Given module RaaLabs.Edge.Modules.Logging.Logging from RaaLabs.Edge.Modules.Logging is registered
	Given module RaaLabs.Edge.Modules.EventHandling.EventHandling from RaaLabs.Edge.Modules.EventHandling is registered
	And module RaaLabs.Edge.Modules.EdgeHub.EdgeHub from RaaLabs.Edge.Modules.EdgeHub is registered
	And IntegerInputHandler is registered

	Given application container has been built

	Given an instance of IntegerInputHandler named inputHandler1
	And   an instance of IntegerInputHandler named inputHandler2

	When EdgeHub input IntegerInput receives 1
	And  EdgeHub input IntegerInput receives 26

	Then IntegerInputHandler inputHandler1 receives [1, 26]
	And IntegerInputHandler inputHandler2 receives [1, 26]

Scenario: Sending an event to EdgeHub
	Given an Autofac context
	Given module RaaLabs.Edge.Modules.Logging.Logging from RaaLabs.Edge.Modules.Logging is registered
	Given module RaaLabs.Edge.Modules.EventHandling.EventHandling from RaaLabs.Edge.Modules.EventHandling is registered
	And module RaaLabs.Edge.Modules.EdgeHub.EdgeHub from RaaLabs.Edge.Modules.EdgeHub is registered
	And IntegerOutputHandler is registered

	Given application container has been built

	Given an instance of IntegerOutputHandler named outputHandler

	When IntegerOutputHandler outputHandler sends 32
	And  IntegerOutputHandler outputHandler sends 37
	And  IntegerOutputHandler outputHandler sends 14
	And  IntegerOutputHandler outputHandler sends 123

	Then EdgeHub output IntegerOutput sends [32, 37, 14, 123]
