Feature: EventHubEventDataConverter

Scenario: Converting a received EventData to an event
	Given an EventHubEventDataConverter with the following event types
		| Type                      |
		| SomeEventHubIncomingEvent |
		| SomeEventHubOutgoingEvent |
	Then converting the following EventData to events should give the expected result
		| EventType                 | Value | Connection                 | Payload            |
		| SomeEventHubIncomingEvent | 31415 | IncomingEventHubConnection | { "value": 31415 } |
		| SomeEventHubIncomingEvent | 35415 | IncomingEventHubConnection | { "value": 35415 } |
		| SomeEventHubIncomingEvent | 31645 | IncomingEventHubConnection | { "value": 31645 } |
		| SomeEventHubIncomingEvent | 32415 | IncomingEventHubConnection | { "value": 32415 } |
		| SomeEventHubIncomingEvent | 12345 | IncomingEventHubConnection | { "value": 12345 } |

Scenario: Converting an event to an EventData
	Given an EventHubEventDataConverter with the following event types
		| Type                      |
		| SomeEventHubIncomingEvent |
		| SomeEventHubOutgoingEvent |
	Then converting the following events to EventData should give the expected result
		| EventType                 | Value | Connection         | Payload            |
		| SomeEventHubOutgoingEvent | 31415 | OutgoingEventHubConnection | { "value": 31415 } |
		| SomeEventHubOutgoingEvent | 35415 | OutgoingEventHubConnection | { "value": 35415 } |
		| SomeEventHubOutgoingEvent | 31645 | OutgoingEventHubConnection | { "value": 31645 } |
		| SomeEventHubOutgoingEvent | 32415 | OutgoingEventHubConnection | { "value": 32415 } |
		| SomeEventHubOutgoingEvent | 12345 | OutgoingEventHubConnection | { "value": 12345 } |