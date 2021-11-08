Feature: TemplatedString

Scenario: Extracting variables from string
	Given the templated string "this is a {SomeField} templated string containing {OtherField}"
	Then we should expect the following mapping from input string to mapped variable
		| Input                                                                     | SomeField    | OtherField             |
		| this is a cool templated string containing nothing                        | cool         | nothing                |
		| this is a spaced value templated string containing a template with spaces | spaced value | a template with spaces |
	Then we should expect the following mapping from input variable to output string
		| SomeField         | OtherField                 | Output                                                                             |
		| weird and strange | information within letters | this is a weird and strange templated string containing information within letters |