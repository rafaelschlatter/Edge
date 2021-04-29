Feature: EdgeTesting

Scenario: Handling events
    Given a handler of type DummyHandler
    When the following events of type IncomingDummyEvent is produced
        | Payload     |
        | test        |
        | second test |
        | third test  |
    Then the following events of type OutgoingDummyEvent is produced
        | Payload                |
        | testtest               |
        | second testsecond test |
        | third testthird test   |
    And the following events of type OtherOutgoingDummyEvent is produced
        | Payload                |
        | testtest               |
        | second testsecond test |
        | third testthird test   |
