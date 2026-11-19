# C# API Automation Framework

A C# (.NET 8) API test automation framework for **REST and SOAP services**, built with **RestSharp, NUnit and SpecFlow**. It also covers **backend validation with SQL**, **service virtualization** with WireMock.Net, CI on **Azure Pipelines** (and GitHub Actions), and **Parasoft SOAtest assets** (OpenAPI and WSDL) with a guide to how the code maps to SOAtest.

## What is in it
- **REST tests** with a service-object layer, JSON schema validation (NJsonSchema), data-driven tests, chained calls and response-time checks
- **SOAP tests**: an XML envelope sent over HTTP with `HttpClient`, with values read from the XML and a SOAP fault case
- **BDD with SpecFlow**: Gherkin feature files, step definitions, hooks, context injection and a Scenario Outline
- **Service virtualization**: WireMock.Net stands in for real services, so error codes, slow responses and SOAP faults can be tested offline and repeatably
- **API vs database validation**: after an API call, SQL confirms the stored data matches (in-memory SQLite stands in for SQL Server or MySQL)
- **Configuration** from `appsettings.json` with environment variable overrides
- **CI**: `azure-pipelines.yml` and `.github/workflows/ci.yml`
- **Parasoft SOAtest**: importable `parasoft/openapi/posts-api.yaml` and `parasoft/soap/calculator.wsdl`, plus `parasoft/README.md`

## Project structure
```
src/ApiFramework/                    reusable framework library
  Config/Settings.cs                 settings (appsettings.json + environment variables)
  Http/ApiClient.cs                  RestSharp wrapper (GetAsync, PostAsync ...)
  Http/ApiResponse.cs                status code, body, elapsed time, JSON helpers
  Http/SoapClient.cs                 SOAP over HttpClient, reads XML values
  Api/PostsApi.cs                    service object: sends requests, returns the response
  Models/Post.cs                     request/response model
  Data/TestDataFactory.cs            unique test data
  Db/DbValidator.cs                  ADO.NET helper for SQL validation (parameterized)
  Validation/SchemaValidator.cs      JSON schema validation
tests/ApiTests/                      NUnit 4 tests
  PostsApiTests.cs                   REST on a live API                 (category: live)
  VirtualizedRestTests.cs            REST error and slow scenarios      (category: virtual)
  SoapServiceTests.cs                SOAP request and fault             (category: virtual)
  ApiVsDatabaseTests.cs              API vs SQL validation              (categories: virtual, db)
  Schemas/post-schema.json           JSON schema
tests/BddTests/                      SpecFlow (BDD) tests
  Features/Posts.feature             Gherkin scenarios
  Steps/PostsSteps.cs                step definitions
  Support/ApiContext.cs, Hooks.cs    scenario state and cleanup
parasoft/                            SOAtest import files and guide
docs/TEST_CASES.md                   test case list
azure-pipelines.yml                  Azure DevOps pipeline
```

Design points:
- **Tests only assert.** `PostsApi` sends the request and returns an `ApiResponse`, so one method serves positive and negative tests.
- **The HTTP library is hidden** behind `ApiClient`. Swapping RestSharp for `HttpClient` changes one file.
- **Steps stay thin.** SpecFlow step definitions call the framework, the same way UI steps would call page objects.
- **Virtualization** makes tests deterministic. A real service cannot easily return a 500, a SOAP fault or a 1.5 second delay on demand.
- **SQL uses parameters**, so values are never concatenated into the query.

## Prerequisites
.NET 8 SDK.

## Run
```bash
dotnet test tests/ApiTests                                   # all API tests (live ones need internet)
dotnet test tests/ApiTests --filter "Category=virtual"       # offline tests only
dotnet test tests/ApiTests --filter "Category=live"          # live API tests only
dotnet test tests/ApiTests --filter "Category=smoke"         # smoke subset
dotnet test tests/BddTests --filter "Category=virtual"       # BDD scenarios, offline
dotnet test tests/BddTests                                   # all BDD scenarios
ApiBaseUrl=https://other-host.example.com dotnet test tests/ApiTests   # another environment
```
On Windows PowerShell, set the variable first: `$env:ApiBaseUrl="https://other-host.example.com"`.

## CI
- **Azure Pipelines** (`azure-pipelines.yml`): triggers on pushes and pull requests to `main`, installs the SDK, restores, runs the virtualized API and BDD tests with published results, then runs the live smoke tests without blocking the pipeline on a network problem.
- **GitHub Actions** (`.github/workflows/ci.yml`): the same flow, with results uploaded as an artifact.

## Parasoft SOAtest
SOAtest is licensed software and is not run here. `parasoft/` has the OpenAPI and WSDL files to import into it, and a guide mapping each part of this framework to the SOAtest concept. See `parasoft/README.md`.

## Notes and limits
- **SpecFlow** is named in many job descriptions, so it is used here. Its original maintainers have ended active development, and **Reqnroll** is the community-maintained successor with nearly identical syntax. Moving over is mostly a package and namespace change.
- The BDD project uses NUnit 3 because SpecFlow 3.9 needs it, so it is separate from the NUnit 4 API test project.
- The live tests use the public JSONPlaceholder API, which does not persist data. Create, update and delete return realistic responses but nothing is stored.
- The SOAP service is virtualized for repeatability. The WSDL can be pointed at a real service.
- The database check uses in-memory SQLite. Swap the connection for SQL Server or MySQL for a real project.

## Roadmap
UI tests with Selenium and the Page Object Model on the same BDD base, an authentication flow (OAuth tokens), and richer reporting.
