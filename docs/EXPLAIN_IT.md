# How to explain this framework in an interview

Say this in your own words. It is written the way you would talk, and it is honest about what this project is.

## The 60-second version

"On the side I built an API automation framework in C#. It uses RestSharp for REST, NUnit for the tests, and SpecFlow for the BDD part. At Equiniti I test APIs with Postman and Swagger and check the data in MySQL, so I wanted to do the same thing in code, in the stack your team uses.

There's a layer that sends the requests, and the tests only do the assertions. I check the status code, the body, the JSON schema and the response time, and I chain calls where one response feeds the next. It also tests SOAP, which is an XML message over HTTP, including a fault.

I wrote some of the scenarios in SpecFlow, so the Given, When, Then lines are readable by the whole squad, and the step definitions just call the framework.

One thing I like is the service virtualization. I use WireMock as a stand-in service, so I can test a 500 error, a 401 or a slow response, which are hard to get from a real environment. There's also a test that calls the API and then runs SQL to confirm the stored data matches.

It runs from an Azure Pipelines file: restore, build, run the tests, publish the results. For Parasoft, I prepared an OpenAPI file and a WSDL that I can import, and I wrote down how each part maps to SOAtest."

## Be straight about three things
- **It is a personal project**, not something you built at Equiniti. Say "I built" and "on the side".
- **Your professional automation is in Python, Robot Framework and Selenium**, and you have not used C# at work (unless you have). Say you built this to move into C#, and that the concepts are the same.
- **You have not used Parasoft SOAtest on a project** (unless you have). Say what you prepared and what you understand.

## Likely follow-up questions

**Why RestSharp?**
"It's a popular REST client for .NET, easy to set up requests, query parameters and bodies. I wrapped it in my own ApiClient so the tests don't depend on it, and I could swap it for HttpClient."

**Why NUnit?**
"It's widely used with SpecFlow and Selenium in C#, it has `[TestCase]` for data-driven tests and categories for choosing which tests to run, like smoke or virtual."

**How does SpecFlow work?**
"I write scenarios in a feature file in Gherkin. Each step is bound to a C# method with `[Given]`, `[When]` or `[Then]`. There are hooks for setup and cleanup, and SpecFlow injects a context object into the step classes so the steps can share data in a scenario. The steps stay thin and call the framework."

**What's a Scenario Outline?**
"It runs one scenario with several rows of data from an Examples table. I used it to check 200, 401 and 500 from the virtual service."

**What is service virtualization and why use it?**
"It's a fake service that behaves like the real one. I use it when the real service isn't ready, is expensive, or can't be made to fail on demand. Then I can test errors, delays and edge cases, and the tests give the same result every time."

**How do you test a SOAP service?**
"It's an HTTP POST with an XML envelope and a SOAPAction header. I send the envelope, then read the XML in the response, like the result value, or the fault message for a bad request. I can generate the requests from the WSDL."

**How do you validate data in the database?**
"After the API call I run a SQL query for that record, using the transaction ID, and compare the fields with the response: status, amount and currency. I also check for duplicates with GROUP BY and HAVING. The values go in as parameters, not joined into the string."

**How would you run this in Azure Pipelines?**
"The YAML triggers on the main branch and pull requests. It installs the .NET SDK, restores and builds, runs the tests, and publishes the results so the team can see them in Azure DevOps. The build fails if a test fails. The fast offline tests run on every pull request."

**How do you handle secrets like passwords or tokens?**
"Not in the code. I'd keep them in pipeline variables or a secure library and read them as environment variables."

**You said SpecFlow. Is it still maintained?**
"Its original maintainers have stopped active development, and Reqnroll is the community-maintained successor with almost the same syntax. I used SpecFlow because the role asks for it, and moving over is mostly a package and namespace change."

**What would you improve?**
"Add UI tests with Selenium and page objects on the same BDD base, handle authentication flows like OAuth tokens, and add richer reporting."
