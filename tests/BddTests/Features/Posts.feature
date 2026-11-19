Feature: Posts API
  As a consumer of the posts service
  I want the API to behave as documented
  So that I can rely on it in my application

  @live @smoke
  Scenario: Get an existing post
    Given the posts API is available
    When I request the post with id 1
    Then the response status is 200
    And the post id in the response is 1

  @live
  Scenario: An unknown post is not found
    Given the posts API is available
    When I request the post with id 999999
    Then the response status is 404

  @virtual @smoke
  Scenario Outline: The client handles different status codes from a virtual service
    Given a virtual service that answers GET /users/<id> with status <status>
    When I send GET /users/<id>
    Then the response status is <status>

    Examples:
      | id  | status |
      | 1   | 200    |
      | 401 | 401    |
      | 500 | 500    |
