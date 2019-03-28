# resource-finder

Microservices / RabbitMQ / Blockchain / CosmosDB / Azure KeyVault / Mobile / Azure Cog Search

Digital agency Contoso requires a solution that allow to internal employees upload content and search them depending on the project, the only restriction they have is that all content needs to be validated by the audit company Northwind in order to prevent any Copyright, once documents have been validated they will be visible for the rest of the organization.

The Microsoft Innovation team is responsible for the project and now are preparing a draft taking some ideas from the executives, they want to build a mobile app that the executive team has been requesting for a while, this application will be ready to register users and start the workflow they prepared in the cloud.

The technology stack used in this project:

- Docker
- Kubernetes (tentative)
- Azure Storage
- Azure CosmosDB
- Azure Blockchain Quorum
- Azure Cognitive Search
- Microservices using .NET Core
- Azure Mobile features
- Xamarin Mobile

Details:

The application will have are two roles for users: approver and user, as user they will have ability to upload files (pdf, jpg, doc) and search them, all the content once have been submitted is pending for approval, approvers should receive an email/push notification to open the pending approval to accept or reject the content, once the request for upload has been approved it will be saved as immutable stamp in the Blockchain and will be able for all users to search it.
