Feature: MqttMessageConverter

Scenario: Converting an event to a message and back again
	Given an MqttMessageConverter with the following event types
		| Type                     |
		| SomeMqttIncomingEvent    |
		| AnotherMqttIncomingEvent |
		| SomeMqttOutgoingEvent    |
		| AnotherMqttOutgoingEvent |
	And a mocked TopicMapper with the following rules
		| Connection           | Route                        | Target                   |
		| MqttBrokerConnection | site1/area1/input/sensor1    | SomeMqttIncomingEvent    |
		| MqttBrokerConnection | site1/area2/input/[\w\d_-]+  | AnotherMqttIncomingEvent |
		| MqttBrokerConnection | site1/area1/output/sensor1   | SomeMqttOutgoingEvent    |
		| MqttBrokerConnection | site1/area2/output/[\w\d_-]+ | AnotherMqttOutgoingEvent |
	Then converting the following events to messages and back should succeed
		| Type                     | Sensor      | Value | ExpectedTopic                  |
		| SomeMqttIncomingEvent    |             | 31415 | site1/area1/input/sensor1      |
		| SomeMqttIncomingEvent    |             | 27183 | site1/area1/input/sensor1      |
		| AnotherMqttIncomingEvent | Somewhere   | 1618  | site1/area2/input/Somewhere    |
		| AnotherMqttIncomingEvent | Elsewhere   | 2618  | site1/area2/input/Elsewhere    |
		| SomeMqttOutgoingEvent    |             | 27183 | site1/area1/output/sensor1     |
		| SomeMqttOutgoingEvent    |             | 37183 | site1/area1/output/sensor1     |
		| AnotherMqttOutgoingEvent | ThisPlace   | 12345 | site1/area2/output/ThisPlace   |
		| AnotherMqttOutgoingEvent | ThatPlace   | 22345 | site1/area2/output/ThatPlace   |
		| AnotherMqttOutgoingEvent | ThosePlaces | 32345 | site1/area2/output/ThosePlaces |