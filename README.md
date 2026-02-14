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

This project supports containerized deployment with CI/CD and Kubernetes autoscaling.

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

---
CI/CD Pipeline
---
An Azure DevOps pipeline is configured via:

azure-pipelines.yml


The pipeline performs:

Restore dependencies

Build the application

Run unit tests

Generate code coverage

Build Docker image

Push image to container registry

Deploy to Kubernetes cluster

Pipeline triggers automatically on commits to configured branches.

---
Horizontal Pod Autoscaling
---
The application supports automatic scaling based on CPU usage.

HPA configuration:

Minimum replicas: 2

Maximum replicas: 10

Target CPU utilization: 70%

Kubernetes automatically scales pods under load.

# Logging & Monitoring

The application supports structured logging and distributed tracing.

Features include:

Request correlation

Client IP tracking

JWT ClientId logging

Response time metrics

External API traceability

Logs can be exported to centralized monitoring platforms.

