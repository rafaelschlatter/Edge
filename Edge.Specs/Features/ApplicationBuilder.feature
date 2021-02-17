Feature: ApplicationBuilder

Scenario: Building an Application from an ApplicationBuilder
	Given an ApplicationBuilder
	When the application is built
	Then the application should be able to run
