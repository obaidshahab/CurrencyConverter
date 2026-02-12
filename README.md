# Currency Converter API

A lightweight ASP.NET Core Web API for converting currencies using real-time exchange rates.
Includes JWT authentication, request logging, and automated tests.

# Features

JWT-based authentication.
Currency conversion endpoint.
Exchange rate lookup.
Request audit logging.
xUnit integration testing support.
Structured logging.
Swagger API documentation.

# Setup
Run the application locally on your machine.

A Postman collection and environment file are included in the repository for easy testing.

Start by executing the Login API using the following credentials:
username: admin
password: 123

Upon successful authentication, an access token will be generated. The token is automatically stored in the Postman environment and applied to all subsequent API requests in the collection.

# Deployment Strategy

To support deployments across multiple environments, the repository uses dedicated branches:

dev — development environment

qa — quality assurance testing

uat — user acceptance testing

master — production

Each environment should be deployed from its corresponding branch to ensure proper isolation, testing, and release control.

# Environment Configuration

The Currency Converter API base URL is stored in appsettings.json.

This value can be parameterized through CI/CD pipelines, allowing environment-specific configuration without modifying source code.

Each environment (dev, qa, uat, production) can use its own API endpoint.