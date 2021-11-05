Feature: MqttTopicMapper

Scenario: Mapping broker topic to event type
	Given an MqttTopicMapper with the following event types
		| Type                     |
		| SomeMqttIncomingEvent    |
		| AnotherMqttIncomingEvent |
		| SomeMqttOutgoingEvent    |
		| AnotherMqttOutgoingEvent |
	Then the MqttTopicMapper will give the following output
		| Connection           | Topic                         | OutputType               |
		| MqttBrokerConnection | site1/area1/input/sensor1     | SomeMqttIncomingEvent    |
		| MqttBrokerConnection | site1/area2/input/pressure    | AnotherMqttIncomingEvent |
		| MqttBrokerConnection | site1/area2/input/humidity    | AnotherMqttIncomingEvent |
		| MqttBrokerConnection | site1/area2/input/awesomeness | AnotherMqttIncomingEvent |
		| MqttBrokerConnection | site1/area2/input/temperature | AnotherMqttIncomingEvent |