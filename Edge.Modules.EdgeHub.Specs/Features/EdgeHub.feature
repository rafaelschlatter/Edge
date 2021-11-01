Feature: EdgeHubBridge

Scenario: Starting an application with an EdgeHub Bridge
	Given an application with the following registrations
		| Kind      | Type                        |
		| Module    | EventHandling               |
		| Module    | EdgeHub                     |
		| EventType | SomeEdgeHubIncomingEvent    |
		| EventType | AnotherEdgeHubIncomingEvent |
		| EventType | SomeEdgeHubOutgoingEvent    |
		| EventType | AnotherEdgeHubOutgoingEvent |
	And the following client mocks
		| ClientType       |
		| IIotModuleClient |
	And the application is running
	Then the following clients will be connected
		| ClientType       |
		| IIotModuleClient |
	And clients will subscribe to the following topics
		| ClientType       | InputName    |
		| IIotModuleClient | someinput    |
		| IIotModuleClient | anotherinput |

Scenario: Receiving EdgeHub messages
	Given an application with the following registrations
		| Kind      | Type                        |
		| Module    | EventHandling               |
		| Module    | EdgeHub                     |
		| EventType | SomeEdgeHubIncomingEvent    |
		| EventType | AnotherEdgeHubIncomingEvent |
		| EventType | SomeEdgeHubOutgoingEvent    |
		| EventType | AnotherEdgeHubOutgoingEvent |
	And the following client mocks
		| ClientType       |
		| IIotModuleClient |
	And the application is running
	When clients receive the following data
		| ClientType       | InputName    | Payload             |
		| IIotModuleClient | someinput    | { "value": 31415 }  |
		| IIotModuleClient | someinput    | { "value": 32415 }  |
		| IIotModuleClient | anotherinput | { "value": "this" } |
		| IIotModuleClient | anotherinput | { "value": "is" }   |
		| IIotModuleClient | someinput    | { "value": 33415 }  |
		| IIotModuleClient | anotherinput | { "value": "a" }    |
		| IIotModuleClient | anotherinput | { "value": "test" } |
	Then the following events are produced
		| EventType                   | Value |
		| SomeEdgeHubIncomingEvent    | 31415 |
		| SomeEdgeHubIncomingEvent    | 32415 |
		| AnotherEdgeHubIncomingEvent | this  |
		| AnotherEdgeHubIncomingEvent | is    |
		| SomeEdgeHubIncomingEvent    | 33415 |
		| AnotherEdgeHubIncomingEvent | a     |
		| AnotherEdgeHubIncomingEvent | test  |

Scenario: Sending EdgeHub messages
	Given an application with the following registrations
		| Kind      | Type                        |
		| Module    | EventHandling               |
		| Module    | EdgeHub                     |
		| EventType | SomeEdgeHubIncomingEvent    |
		| EventType | AnotherEdgeHubIncomingEvent |
		| EventType | SomeEdgeHubOutgoingEvent    |
		| EventType | AnotherEdgeHubOutgoingEvent |
	And the following client mocks
		| ClientType       |
		| IIotModuleClient |
	And the application is running
	When the following events are produced
		| EventType                   | Value |
		| SomeEdgeHubOutgoingEvent    | 31415 |
		| SomeEdgeHubOutgoingEvent    | 31416 |
		| AnotherEdgeHubOutgoingEvent | this  |
		| AnotherEdgeHubOutgoingEvent | is    |
		| SomeEdgeHubOutgoingEvent    | 31417 |
		| AnotherEdgeHubOutgoingEvent | a     |
		| AnotherEdgeHubOutgoingEvent | test  |
	Then clients send the following data
		| ClientType       | OutputName    | Payload             |
		| IIotModuleClient | someoutput    | { "value": 31415 }  |
		| IIotModuleClient | someoutput    | { "value": 31416 }  |
		| IIotModuleClient | anotheroutput | { "value": "this" } |
		| IIotModuleClient | anotheroutput | { "value": "is" }   |
		| IIotModuleClient | someoutput    | { "value": 31417 }  |
		| IIotModuleClient | anotheroutput | { "value": "a" }    |
		| IIotModuleClient | anotheroutput | { "value": "test" } |