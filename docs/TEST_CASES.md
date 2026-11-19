# Test Cases

Categories are NUnit categories. SpecFlow tags (`@live`, `@virtual`, `@smoke`) become categories too, so one filter works for both projects, for example `--filter "Category=virtual"`.

| ID | Area | Title | Expected result | Category | Where |
|---|---|---|---|---|---|
| TC-200 | REST | List posts | 200 and a non-empty list | live, smoke | `PostsApiTests` |
| TC-201 | REST | Single post matches schema | 200, schema valid, id = 1 | live, smoke | `PostsApiTests` |
| TC-202 | REST | Create a post | 201, data echoed, id present | live | `PostsApiTests` |
| TC-203 | REST | Update a post | 200 with the new title | live | `PostsApiTests` |
| TC-204 | REST | Delete a post | 200 | live | `PostsApiTests` |
| TC-205 | REST | Unknown resource | 404 | live | `PostsApiTests` |
| TC-206 | REST | Filter by query parameter | Every item has the requested userId | live | `PostsApiTests` |
| TC-207 | REST | Response time | Within the configured limit | live | `PostsApiTests` |
| TC-208 | REST | Chained calls | Value from call 1 works in call 2 | live | `PostsApiTests` |
| TC-209 | REST | Data driven ids 1 to 5 | 200 and matching id for each | live | `PostsApiTests` |
| TC-210 | Virtual REST | Happy path from stub | 200 with stubbed data | virtual, smoke | `VirtualizedRestTests` |
| TC-211 | Virtual REST | Unauthorized | 401 and error message | virtual | `VirtualizedRestTests` |
| TC-212 | Virtual REST | Server error | 500 and error message | virtual | `VirtualizedRestTests` |
| TC-213 | Virtual REST | Slow dependency | 200 after at least the simulated delay | virtual | `VirtualizedRestTests` |
| TC-220 | SOAP | Add | `AddResult` = 5 | virtual, smoke | `SoapServiceTests` |
| TC-221 | SOAP | Divide by zero | 500 and fault "Division by zero" | virtual | `SoapServiceTests` |
| TC-230 | DB | API matches database | Status, currency and amount equal | virtual, db | `ApiVsDatabaseTests` |
| TC-231 | DB | Duplicate detection | SQL finds the duplicate transaction | virtual, db | `ApiVsDatabaseTests` |
| BDD-1 | BDD | Get an existing post | 200 and id 1 | live, smoke | `Posts.feature` |
| BDD-2 | BDD | Unknown post | 404 | live | `Posts.feature` |
| BDD-3 | BDD | Status codes from a virtual service | 200, 401 and 500 as stubbed | virtual, smoke | `Posts.feature` (outline, 3 examples) |
