Feature: EdgeHubMessageConverter

Scenario: Converting a received message to an event
	Given an EdgeHubMessageConverter with the following event types
		| Type                        |
		| SomeEdgeHubIncomingEvent    |
		| AnotherEdgeHubIncomingEvent |
		| SomeEdgeHubOutgoingEvent    |
		| AnotherEdgeHubOutgoingEvent |
	Then converting the following messages to events should give the expected result
		| EventType                   | Value | InputName    | Payload             |
		| SomeEdgeHubIncomingEvent    | 31415 | someinput    | { "value": 31415 }  |
		| SomeEdgeHubIncomingEvent    | 35415 | someinput    | { "value": 35415 }  |
		| SomeEdgeHubIncomingEvent    | 31645 | someinput    | { "value": 31645 }  |
		| SomeEdgeHubIncomingEvent    | 32415 | someinput    | { "value": 32415 }  |
		| SomeEdgeHubIncomingEvent    | 12345 | someinput    | { "value": 12345 }  |
		| AnotherEdgeHubIncomingEvent | this  | anotherinput | { "value": "this" } |
		| AnotherEdgeHubIncomingEvent | is    | anotherinput | { "value": "is" }   |
		| AnotherEdgeHubIncomingEvent | a     | anotherinput | { "value": "a" }    |
		| AnotherEdgeHubIncomingEvent | good  | anotherinput | { "value": "good" } |
		| AnotherEdgeHubIncomingEvent | test  | anotherinput | { "value": "test" } |

Scenario: Converting an event to a message
	Given an EdgeHubMessageConverter with the following event types
		| Type                        |
		| SomeEdgeHubIncomingEvent    |
		| AnotherEdgeHubIncomingEvent |
		| SomeEdgeHubOutgoingEvent    |
		| AnotherEdgeHubOutgoingEvent |
	Then converting the following events to messages should give the expected result
		| EventType                   | Value | OutputName    | Payload             |
		| SomeEdgeHubOutgoingEvent    | 31415 | someoutput    | { "value": 31415 }  |
		| SomeEdgeHubOutgoingEvent    | 35415 | someoutput    | { "value": 35415 }  |
		| SomeEdgeHubOutgoingEvent    | 31645 | someoutput    | { "value": 31645 }  |
		| SomeEdgeHubOutgoingEvent    | 32415 | someoutput    | { "value": 32415 }  |
		| SomeEdgeHubOutgoingEvent    | 12345 | someoutput    | { "value": 12345 }  |
		| AnotherEdgeHubOutgoingEvent | this  | anotheroutput | { "value": "this" } |
		| AnotherEdgeHubOutgoingEvent | is    | anotheroutput | { "value": "is" }   |
		| AnotherEdgeHubOutgoingEvent | a     | anotheroutput | { "value": "a" }    |
		| AnotherEdgeHubOutgoingEvent | good  | anotheroutput | { "value": "good" } |
		| AnotherEdgeHubOutgoingEvent | test  | anotheroutput | { "value": "test" } |