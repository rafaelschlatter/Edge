Feature: IotHubMessageConverter

Scenario: Converting a received message to an event
	Given an IotHubMessageConverter with the following event types
		| Type                    |
		| SomeIotHubIncomingEvent |
		| SomeIotHubOutgoingEvent |
	Then converting the following messages to events should give the expected result
		| EventType               | Value | Connection       | Payload            |
		| SomeIotHubIncomingEvent | 31415 | IotHubConnection | { "value": 31415 } |
		| SomeIotHubIncomingEvent | 35415 | IotHubConnection | { "value": 35415 } |
		| SomeIotHubIncomingEvent | 31645 | IotHubConnection | { "value": 31645 } |
		| SomeIotHubIncomingEvent | 32415 | IotHubConnection | { "value": 32415 } |
		| SomeIotHubIncomingEvent | 12345 | IotHubConnection | { "value": 12345 } |

Scenario: Converting an event to a message
	Given an IotHubMessageConverter with the following event types
		| Type                    |
		| SomeIotHubIncomingEvent |
		| SomeIotHubOutgoingEvent |
	Then converting the following events to messages should give the expected result
		| EventType               | Value | Connection       | Payload            |
		| SomeIotHubOutgoingEvent | 31415 | IotHubConnection | { "value": 31415 } |
		| SomeIotHubOutgoingEvent | 35415 | IotHubConnection | { "value": 35415 } |
		| SomeIotHubOutgoingEvent | 31645 | IotHubConnection | { "value": 31645 } |
		| SomeIotHubOutgoingEvent | 32415 | IotHubConnection | { "value": 32415 } |
		| SomeIotHubOutgoingEvent | 12345 | IotHubConnection | { "value": 12345 } |