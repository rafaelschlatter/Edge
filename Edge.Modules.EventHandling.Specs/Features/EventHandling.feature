Feature: EventHandling

Scenario: Loading EventHandling module into context
	Given an Autofac context
	Given module RaaLabs.Edge.Modules.EventHandling.EventHandling from RaaLabs.Edge.Modules.EventHandling is registered

	When Building application container
	Then Starting lifetime scope should succeed

Scenario: Sending message from a producer to a consumer
	Given an Autofac context
	Given module RaaLabs.Edge.Modules.EventHandling.EventHandling from RaaLabs.Edge.Modules.EventHandling is registered

	Given a producer for System.Int32 named IntProducer
	And a consumer for System.Int32 named IntConsumer

	Given application container has been built

	Given an instance of IntProducer named producer1
	And an instance of IntConsumer named consumer1

	When producer1 produces 1
	Then consumer1 receives [1]

Scenario: Sending message from a producer to multiple consumers
	Given an Autofac context
	Given module RaaLabs.Edge.Modules.EventHandling.EventHandling from RaaLabs.Edge.Modules.EventHandling is registered

	Given a producer for System.Int32 named IntProducer
	And a consumer for System.Int32 named IntConsumer

	Given application container has been built

	Given an instance of IntProducer named producer1
	And an instance of IntConsumer named consumer1
	And an instance of IntConsumer named consumer2
	And an instance of IntConsumer named consumer3

	When producer1 produces 15
	Then consumer1 receives [15]
	And  consumer2 receives [15]
	And  consumer3 receives [15]

Scenario: Sending message from multiple producers to a single consumer
	Given an Autofac context
	Given module RaaLabs.Edge.Modules.EventHandling.EventHandling from RaaLabs.Edge.Modules.EventHandling is registered

	Given a producer for System.Int32 named IntProducer
	And a consumer for System.Int32 named IntConsumer

	Given application container has been built

	Given an instance of IntProducer named producer1
	And an instance of IntProducer named producer2
	And an instance of IntProducer named producer3
	And an instance of IntConsumer named consumer1

	When producer1 produces 65
	And producer2 produces 23
	And producer3 produces 14
	Then consumer1 receives [65, 23, 14]
