Feature: MqttBridge

Scenario: Starting an application with an Mqtt Bridge
	Given an application with the following registrations
		| Kind      | Type                     |
		| Module    | EventHandling            |
		| Module    | Mqtt                     |
		| EventType | SomeMqttIncomingEvent    |
		| EventType | AnotherMqttIncomingEvent |
		| EventType | SomeMqttOutgoingEvent    |
		| EventType | AnotherMqttOutgoingEvent |
	And the following client mocks
		| ClientType        | ConnectionType       |
		| IMqttBrokerClient | MqttBrokerConnection |
	And the application is running
	Then the following clients will be connected
		| ClientType        | ConnectionType       |
		| IMqttBrokerClient | MqttBrokerConnection |
	And clients will subscribe to the following topics
		| ClientType        | ConnectionType       | Topic                     |
		| IMqttBrokerClient | MqttBrokerConnection | site1/area1/input/sensor1 |
		| IMqttBrokerClient | MqttBrokerConnection | site1/area2/input/+       |

Scenario: Receiving MQTT messages from a broker
	Given an application with the following registrations
		| Kind      | Type                     |
		| Module    | EventHandling            |
		| Module    | Mqtt                     |
		| EventType | SomeMqttIncomingEvent    |
		| EventType | AnotherMqttIncomingEvent |
		| EventType | SomeMqttOutgoingEvent    |
		| EventType | AnotherMqttOutgoingEvent |
	And the following client mocks
		| ClientType        | ConnectionType       |
		| IMqttBrokerClient | MqttBrokerConnection |
	And the application is running
	When clients receive the following data
		| ClientType        | ConnectionType       | Topic                           | Payload            |
		| IMqttBrokerClient | MqttBrokerConnection | site1/area1/input/sensor1       | { "value": 31415 } |
		| IMqttBrokerClient | MqttBrokerConnection | site1/area1/input/sensor1       | { "value": 31415 } |
		| IMqttBrokerClient | MqttBrokerConnection | site1/area2/input/SomeSensor    | { "value": 27183 } |
		| IMqttBrokerClient | MqttBrokerConnection | site1/area2/input/AnotherSensor | { "value": 28183 } |
		| IMqttBrokerClient | MqttBrokerConnection | site1/area1/input/sensor1       | { "value": 31415 } |
	Then the following events are produced
		| EventType                | Value | Sensor        |
		| SomeMqttIncomingEvent    | 31415 |               |
		| SomeMqttIncomingEvent    | 31415 |               |
		| AnotherMqttIncomingEvent | 27183 | SomeSensor    |
		| AnotherMqttIncomingEvent | 28183 | AnotherSensor |
		| SomeMqttIncomingEvent    | 31415 |               |

Scenario: Sending MQTT messages to a broker
	Given an application with the following registrations
		| Kind      | Type                     |
		| Module    | EventHandling            |
		| Module    | Mqtt                     |
		| EventType | SomeMqttIncomingEvent    |
		| EventType | AnotherMqttIncomingEvent |
		| EventType | SomeMqttOutgoingEvent    |
		| EventType | AnotherMqttOutgoingEvent |
	And the following client mocks
		| ClientType        | ConnectionType       |
		| IMqttBrokerClient | MqttBrokerConnection |
	And the application is running
	When the following events are produced
		| EventType                | Value | Sensor        |
		| SomeMqttOutgoingEvent    | 31415 |               |
		| SomeMqttOutgoingEvent    | 31416 |               |
		| AnotherMqttOutgoingEvent | 27183 | SomeSensor    |
		| AnotherMqttOutgoingEvent | 27184 | AnotherSensor |
		| SomeMqttOutgoingEvent    | 31417 |               |
	Then clients send the following data
		| ClientType        | ConnectionType       | Topic                            | Payload                                       |
		| IMqttBrokerClient | MqttBrokerConnection | site1/area1/output/sensor1       | { "value": 31415 }                            |
		| IMqttBrokerClient | MqttBrokerConnection | site1/area1/output/sensor1       | { "value": 31416 }                            |
		| IMqttBrokerClient | MqttBrokerConnection | site1/area2/output/SomeSensor    | { "value": 27183, "sensor": "SomeSensor" }    |
		| IMqttBrokerClient | MqttBrokerConnection | site1/area2/output/AnotherSensor | { "value": 27184, "sensor": "AnotherSensor" } |
		| IMqttBrokerClient | MqttBrokerConnection | site1/area1/output/sensor1       | { "value": 31417 }                            |