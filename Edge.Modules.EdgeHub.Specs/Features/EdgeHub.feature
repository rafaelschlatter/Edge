Feature: EdgeHub Functionality

Scenario: Loading EdgeHub module into context
	Given an application with the following registrations
		| Kind    | Type                |
		| Module  | EventHandling       |
		| Module  | EdgeHub             |
	
	Then starting lifetime scope should succeed

Scenario: Receiving an event from EdgeHub
	Given an application with the following registrations
		| Kind    | Type						|
		| Module  | EventHandling				|
		| Module  | EdgeHub                     |
		| Handler | IntegerInputHandler         |
		| Handler | IntegerInputSquaringHandler |
	
	And a lifetime scope with the following instances
		| Name                  | Type                        |
		| inputHandler1         | IntegerInputHandler         |
		| inputHandler2         | IntegerInputHandler         |
		| inputSquaringHandler1 | IntegerInputSquaringHandler |

	When EdgeHub input IntegerInput receives the following values
		| Value |
		| 1     |
		| 3     |
		| 6     |

	Then handlers should receive the following values
		| inputHandler1 | inputHandler2 | inputSquaringHandler1 |
		| 1             | 1             | 1                     |
		| 3             | 3             | 9                     |
		| 6             | 6             | 36                    |

Scenario: Sending an event to EdgeHub
	Given an application with the following registrations
		| Kind    | Type						|
		| Module  | EventHandling				|
		| Module  | EdgeHub                     |
		| Handler | IntegerOutputHandler        |
	
	And a lifetime scope with the following instances
		| Name                  | Type                  |
		| outputHandler         | IntegerOutputHandler  |

	When event producer outputHandler produces the following values
		| Value |
		| 1     |
		| 4     |
		| 23    |
		| 15    |

	Then EdgeHub output IntegerOutput sends the following values
		| Value |
		| 1     |
		| 4     |
		| 23    |
		| 15    |