Feature: Circuit Breaker

Scenario: Transition to tripped state
	Given I have a circuit breaker with settings
	| Failure Threshold |
	| 1                 |
	When I execute the failing IO method through the Circuit Breaker
	Then the Circuit Breaker state should be Tripped
