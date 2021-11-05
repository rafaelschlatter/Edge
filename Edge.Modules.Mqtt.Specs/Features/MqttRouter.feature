Feature: MqttRouter

Scenario: Mapping topic to target
	Given an MqttRouter with the following routes
		| Pattern           | Target      |
		| site1/+/somewhere | somewhere   |
		| site1/+/elsewhere | elsewhere   |
		| site2/this-place  | this-place  |
		| site2/that-place  | that-place  |
		| +/+/noisy-area    | noisy-area  |
		| +/+/silent-area   | silent-area |
		| unspecified/+     | wildcard    |
		| big-place/#       | big-place   |
	Then the MqttRouter will give the following output
		| Topic                        | Target      |
		| site1/coffee-room/somewhere  | somewhere   |
		| site1/coffee-room/elsewhere  | elsewhere   |
		| site1/coffee-room/noisy-area | noisy-area  |
		| site2/break-room/silent-area | silent-area |
		| site2/this-place             | this-place  |
		| site2/that-place             | that-place  |
		| unspecified/cabin            | wildcard    |
		| unspecified/gas-station      | wildcard    |
		| big-place/on-the-sofa        | big-place   |
		| big-place/second-floor/sink  | big-place   |