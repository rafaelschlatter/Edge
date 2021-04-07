Feature: AsyncTasks

Scenario: Adding Async tasks to application
	Given an ApplicationBuilder
	And three async tasks
	When the application is built
	And the application is started
	Then the application should run all async tasks
