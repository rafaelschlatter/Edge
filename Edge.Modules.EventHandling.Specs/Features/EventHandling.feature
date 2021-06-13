Feature: EventHandling

Scenario: Loading EdgeHub module into context
	Given an application with the following registrations
		| Kind   | Type          |
		| Module | EventHandling |
	Then starting lifetime scope should succeed

Scenario: Sending a message from a producer to multiple consumers
	Given an application with the following registrations
		| Kind   | Type             |
		| Module | EventHandling    |
		| Type   | Producer         |
		| Type   | Consumer         |
		| Type   | SquaringConsumer |
	And the application is running
	And a lifetime scope with the following instances
		| Name      | Type             |
		| producer  | Producer         |
		| consumer1 | Consumer         |
		| consumer2 | SquaringConsumer |
	When event producer producer produces the following values
		| Value |
		| 1     |
		| 4     |
		| 23    |
		| 15    |
	Then handlers should receive the following values
		| consumer1 | consumer2 |
		| 1         | 1         |
		| 4         | 16        |
		| 23        | 529       |
		| 15        | 225       |

Scenario: Sending a message from multiple producers to a single consumer
	Given an application with the following registrations
		| Kind   | Type          |
		| Module | EventHandling |
		| Type   | Producer      |
		| Type   | Consumer      |
	And the application is running
	And a lifetime scope with the following instances
		| Name      | Type     |
		| producer1 | Producer |
		| producer2 | Producer |
		| consumer1 | Consumer |
	When event producer producer1 produces the following values
		| Value |
		| 1     |
		| 4     |
	And event producer producer2 produces the following values
		| Value |
		| 23    |
		| 15    |
	Then handlers should receive the following values
		| consumer1 |
		| 1         |
		| 4         |
		| 23        |
		| 15        |

Scenario: Sending a message from an async producer to a sync consumer
	Given an application with the following registrations
		| Kind   | Type          |
		| Module | EventHandling |
		| Type   | AsyncProducer |
		| Type   | Consumer      |
	And the application is running
	And a lifetime scope with the following instances
		| Name      | Type          |
		| producer1 | AsyncProducer |
		| consumer1 | Consumer      |
	When event producer producer1 produces the following values
		| Value |
		| 1     |
		| 4     |
		| 23    |
		| 15    |
	Then handlers should receive the following values
		| consumer1 |
		| 1         |
		| 4         |
		| 23        |
		| 15        |

Scenario: Sending a message from a sync producer to an async consumer
	Given an application with the following registrations
		| Kind   | Type          |
		| Module | EventHandling |
		| Type   | Producer      |
		| Type   | AsyncConsumer |
	And the application is running
	And a lifetime scope with the following instances
		| Name      | Type          |
		| producer1 | Producer      |
		| consumer1 | AsyncConsumer |
	When event producer producer1 produces the following values
		| Value |
		| 1     |
		| 4     |
		| 23    |
		| 15    |
	Then handlers should receive the following values
		| consumer1 |
		| 1         |
		| 4         |
		| 23        |
		| 15        |

Scenario: Sending a message from an async producer to an async consumer
	Given an application with the following registrations
		| Kind   | Type          |
		| Module | EventHandling |
		| Type   | AsyncProducer |
		| Type   | AsyncConsumer |
	And the application is running
	And a lifetime scope with the following instances
		| Name      | Type          |
		| producer1 | AsyncProducer |
		| consumer1 | AsyncConsumer |
	When event producer producer1 produces the following values
		| Value |
		| 1     |
		| 4     |
		| 23    |
		| 15    |
	Then handlers should receive the following values
		| consumer1 |
		| 1         |
		| 4         |
		| 23        |
		| 15        |