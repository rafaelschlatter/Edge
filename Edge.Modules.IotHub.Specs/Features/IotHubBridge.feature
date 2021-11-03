Feature: IotHubBridge

Scenario: Starting an application with an IotHub Bridge
	Given an application with the following registrations
		| Kind      | Type                    |
		| Module    | EventHandling           |
		| Module    | IotHub                  |
		| EventType | SomeIotHubIncomingEvent |
		| EventType | SomeIotHubOutgoingEvent |
	And the following client mocks
		| ClientType    | ConnectionType   |
		| IIotHubClient | IotHubConnection |
	And the application is running
	Then the following clients will be connected
		| ClientType    | ConnectionType   |
		| IIotHubClient | IotHubConnection |

Scenario: Receiving IotHub messages from an IotHub
	Given an application with the following registrations
		| Kind      | Type                    |
		| Module    | EventHandling           |
		| Module    | IotHub                  |
		| EventType | SomeIotHubIncomingEvent |
		| EventType | SomeIotHubOutgoingEvent |
	And the following client mocks
		| ClientType    | ConnectionType   |
		| IIotHubClient | IotHubConnection |
	And the application is running
	When clients receive the following data
		| ClientType    | ConnectionType   | Payload            |
		| IIotHubClient | IotHubConnection | { "value": 31415 } |
		| IIotHubClient | IotHubConnection | { "value": 32415 } |
		| IIotHubClient | IotHubConnection | { "value": 33415 } |
	Then the following events are produced in any order
		| EventType               | Value |
		| SomeIotHubIncomingEvent | 31415 |
		| SomeIotHubIncomingEvent | 32415 |
		| SomeIotHubIncomingEvent | 33415 |

Scenario: Sending IotHub messages to an IotHub
	Given an application with the following registrations
		| Kind      | Type                    |
		| Module    | EventHandling           |
		| Module    | IotHub                  |
		| EventType | SomeIotHubIncomingEvent |
		| EventType | SomeIotHubOutgoingEvent |
	And the following client mocks
		| ClientType    | ConnectionType   |
		| IIotHubClient | IotHubConnection |
	And the application is running
	When the following events are produced
		| EventType               | Value |
		| SomeIotHubOutgoingEvent | 31415 |
		| SomeIotHubOutgoingEvent | 31416 |
		| SomeIotHubOutgoingEvent | 31417 |
	Then clients send the following data
		| ClientType    | ConnectionType   | Payload            |
		| IIotHubClient | IotHubConnection | { "value": 31415 } |
		| IIotHubClient | IotHubConnection | { "value": 31416 } |
		| IIotHubClient | IotHubConnection | { "value": 31417 } |