Feature: EventHubBridge

Scenario: Starting an application with an EventHub Bridge
	Given an application with the following registrations
		| Kind      | Type                      |
		| Module    | EventHandling             |
		| Module    | EventHub                  |
		| EventType | SomeEventHubIncomingEvent |
		| EventType | SomeEventHubOutgoingEvent |
	And the following client mocks
		| ClientType              | ConnectionType             |
		| IEventHubConsumerClient | IncomingEventHubConnection |
		| IEventHubProducerClient | OutgoingEventHubConnection |
	And the application is running
    Then the following clients will be connected
		| ClientType              | ConnectionType             |
		| IEventHubConsumerClient | IncomingEventHubConnection |
		| IEventHubProducerClient | OutgoingEventHubConnection |

Scenario: Receiving data from the EventHubConsumerClient
	Given an application with the following registrations
		| Kind      | Type                      |
		| Module    | EventHandling             |
		| Module    | EventHub                  |
		| EventType | SomeEventHubIncomingEvent |
		| EventType | SomeEventHubOutgoingEvent |
	And the following client mocks
		| ClientType              | ConnectionType             |
		| IEventHubConsumerClient | IncomingEventHubConnection |
	And the application is running
	When clients receive the following data
		| ClientType              | ConnectionType             | Payload            |
		| IEventHubConsumerClient | IncomingEventHubConnection | { "value": 12345 } |
		| IEventHubConsumerClient | IncomingEventHubConnection | { "value": 23456 } |
		| IEventHubConsumerClient | IncomingEventHubConnection | { "value": 34567 } |
	Then the following events are produced
		| EventType                 | Value |
		| SomeEventHubIncomingEvent | 12345 |
		| SomeEventHubIncomingEvent | 23456 |
		| SomeEventHubIncomingEvent | 34567 |

Scenario: Sending events to the EventHubProducerClient
	Given an application with the following registrations
		| Kind      | Type                      |
		| Module    | EventHandling             |
		| Module    | EventHub                  |
		| EventType | SomeEventHubIncomingEvent |
		| EventType | SomeEventHubOutgoingEvent |
	And the following client mocks
		| ClientType              | ConnectionType             |
		| IEventHubProducerClient | OutgoingEventHubConnection |
	And the application is running
	When the following events are produced
		| EventType                 | Value |
		| SomeEventHubOutgoingEvent | 31415 |
		| SomeEventHubOutgoingEvent | 31416 |
		| SomeEventHubOutgoingEvent | 31417 |
	Then clients send the following data
		| ClientType              | ConnectionType             | Payload            |
		| IEventHubProducerClient | OutgoingEventHubConnection | { "value": 31415 } |
		| IEventHubProducerClient | OutgoingEventHubConnection | { "value": 31416 } |
		| IEventHubProducerClient | OutgoingEventHubConnection | { "value": 31417 } |