# Stargate

***

## Astronaut Career Tracking System (ACTS)

ACTS is used as a tool to maintain a record of all the People that have served as Astronauts. When serving as an Astronaut, your *Job* (Duty) is tracked by your Rank, Title and the Start and End Dates of the Duty.

The People that exist in this system are not all Astronauts. ACTS maintains a master list of People and Duties that are updated from an external service (not controlled by ACTS). The update schedule is determined by the external service.

## Definitions

1. A person's astronaut assignment is the Astronaut Duty.
1. A person's current astronaut information is stored in the Astronaut Detail table.
1. A person's list of astronaut assignments is stored in the Astronaut Duty table.

## Requirements

##### Enhance the Stargate API (Required)

The REST API is expected to do the following:

1. Retrieve a person by name.
1. Retrieve all people.
1. Add/update a person by name.
1. Retrieve Astronaut Duty by name.
1. Add an Astronaut Duty.

##### Implement a user interface: (Encouraged)

The UI is expected to do the following:

1. Successfully implement a web application that demonstrates production level quality. Angular is preferred.
1. Implement call(s) to retrieve an individual's astronaut duties.
1. Display the progress of the process and the results in a visually sophisticated and appealing manner.

## Tasks

Overview
Examine the code, find and resolve any flaws, if any exist. Identify design patterns and follow or change them. Provide fix(es) and be prepared to describe the changes.

1. Generate the database
   * This is your source and storage location
1. Enforce the rules
1. Improve defensive coding
1. Add unit tests
   * identify the most impactful methods requiring tests
   * reach >50% code coverage
1. Implement process logging
   * Log exceptions
   * Log successes
   * Store the logs in the database

## Rules

1. A Person is uniquely identified by their Name.
1. A Person who has not had an astronaut assignment will not have Astronaut records.
1. A Person will only ever hold one current Astronaut Duty Title, Start Date, and Rank at a time.
1. A Person's Current Duty will not have a Duty End Date.
1. A Person's Previous Duty End Date is set to the day before the New Astronaut Duty Start Date when a new Astronaut Duty is received for a Person.
1. A Person is classified as 'Retired' when a Duty Title is 'RETIRED'.
1. A Person's Career End Date is one day before the Retired Duty Start Date.

## Interview Notes - Stargate API Code Review

### Note 1: DTO Pattern Inconsistency
Query handlers return EF entities directly, risking serialization issues and exposing database structure. Recommend using DTOs (like PersonAstronaut) consistently across all endpoints.

### Note 2: Naming Convention Issue
Context class named StargateContext but database is Starbase — renamed to StarbaseContext for consistency.

### Note 3: SQL Injection Risk
String interpolation in queries exposes SQL injection vulnerabilities. Use parameterized queries throughout.

### Note 4: Incorrect Query Reference
GetAstronautDutiesByName was incorrectly calling GetPersonByName — fixed to call correct handler.

### Note 5: Missing Error Handling
CreateAstronautDuty endpoint lacked try-catch block — added exception handling for consistency.

### Note 6: Case-Sensitive Comparisons
String comparisons weren't normalized — implemented .NormalizeNameOrTitle() extension for consistent matching.

### Note 7: Data Integrity - Removed Denormalized Fields
Removed CurrentDuty and CurrentRank from Person table. Future-dated duties would corrupt these values. Current duty now derived from AstronautDuty table where DutyEndDate IS NULL.

### Note 8: Business Rules Enforcement
Implemented strict duty assignment rules:

- No future-dating allowed
- One duty per day per person
- Backdated duties adjust existing duty boundaries (end date = new start date - 1)
- Current duty determination: DutyStartDate <= TODAY AND (DutyEndDate IS NULL OR DutyEndDate >= TODAY)
- Retirement interpretation: Terminal duty with title 'RETIRED' — multiple retirements permitted via new assignments