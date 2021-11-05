Feature: TimescaledbBridge

Scenario: Starting an application with a Timescaledb Bridge
	Given an application with the following registrations
		| Kind      | Type                    |
		| Module    | EventHandling           |
		| Module    | Timescaledb             |
		| EventType | SomeTimescaledbEvent    |
		| EventType | AnotherTimescaledbEvent |
		| EventType | ThirdTimescaledbEvent   |
	And the following client mocks
		| ClientType         | ConnectionType               |
		| ITimescaledbClient | SomeTimescaledbConnection    |
		| ITimescaledbClient | AnotherTimescaledbConnection |
	And the application is running
	Then the following clients will be connected
		| ClientType         | ConnectionType               |
		| ITimescaledbClient | SomeTimescaledbConnection    |
		| ITimescaledbClient | AnotherTimescaledbConnection |

Scenario: Inserting events into a Timescaledb database
	Given an application with the following registrations
		| Kind      | Type                    |
		| Module    | EventHandling           |
		| Module    | Timescaledb             |
		| EventType | SomeTimescaledbEvent    |
		| EventType | AnotherTimescaledbEvent |
		| EventType | ThirdTimescaledbEvent   |
	And the following client mocks
		| ClientType         | ConnectionType               |
		| ITimescaledbClient | SomeTimescaledbConnection    |
		| ITimescaledbClient | AnotherTimescaledbConnection |
	And the application is running
	When the following events are produced
		| EventType               | Value  |
		| SomeTimescaledbEvent    | 31415  |
		| SomeTimescaledbEvent    | 31416  |
		| AnotherTimescaledbEvent | hello  |
		| AnotherTimescaledbEvent | this   |
		| AnotherTimescaledbEvent | is     |
		| AnotherTimescaledbEvent | a      |
		| AnotherTimescaledbEvent | test   |
		| ThirdTimescaledbEvent   | 3.1415 |
		| ThirdTimescaledbEvent   | 3.2415 |
		| ThirdTimescaledbEvent   | 3.3415 |
		| ThirdTimescaledbEvent   | 3.4415 |
		| ThirdTimescaledbEvent   | 3.5415 |
		| SomeTimescaledbEvent    | 31420  |
	Then clients send the following data
		| ClientType         | ConnectionType               | Value  |
		| ITimescaledbClient | SomeTimescaledbConnection    | 31415  |
		| ITimescaledbClient | SomeTimescaledbConnection    | 31416  |
		| ITimescaledbClient | AnotherTimescaledbConnection | hello  |
		| ITimescaledbClient | AnotherTimescaledbConnection | this   |
		| ITimescaledbClient | AnotherTimescaledbConnection | is     |
		| ITimescaledbClient | AnotherTimescaledbConnection | a      |
		| ITimescaledbClient | AnotherTimescaledbConnection | test   |
		| ITimescaledbClient | SomeTimescaledbConnection    | 3.1415 |
		| ITimescaledbClient | SomeTimescaledbConnection    | 3.2415 |
		| ITimescaledbClient | SomeTimescaledbConnection    | 3.3415 |
		| ITimescaledbClient | SomeTimescaledbConnection    | 3.4415 |
		| ITimescaledbClient | SomeTimescaledbConnection    | 3.5415 |
		| ITimescaledbClient | SomeTimescaledbConnection    | 31420  |