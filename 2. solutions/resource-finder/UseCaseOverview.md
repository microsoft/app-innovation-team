## Background and Story

In this hypothetical scenario, a media company (Contoso Inc) works regularly with 4 Digital Agencies (A,B,C,D) on a regular basis. 

These agencies are content creators, and Contoso is a content owner. To facilitate work on a shared, auditable ledger, they use a permissioned blockchain network + IPFS file storage to manage content between the 5 of them. 

Each agency has within their own technology architecture, a Quorum node (with a public key identifying it on the network) and an IPFS node for sharing content. When files are written to an IPFS node, the node sends back a hash that can be used a resolver for that particular file. Any member on the network can ask their node to resolve that hash, and get the file in return. 

Contoso has created a request for proposal and has asked for new designs for their company logo. 

All digital agencies must submit their proposals to Contoso on the network, but agencies can share their content on a need-to-know basis. Some agencies may choose to partner together for proposals, and some may not. 

On the blockchain, the proposals are goverened by smart contracts, and privacy is supported through the use of private transactions in Quorum. IPFS is used to share content. 

Once Contoso selects a winner for the proposal, they will create a new, publically available smart contract for all to see. This content is also moved to a blob storage container that is indexed with Azure Cognitive search. 

## Roles

In a production scenario, each company would maintain
their own identity management solution for their employees. In this example, we will be using a single Active Directory tentant that has 5 groups, each group representing a specific company. 

## Tech Stack

The technology stack used in this project:

- Docker
- Kubernetes (tentative)
- Azure Storage
- Azure CosmosDB
- Azure Blockchain as a Service
- Azure Cognitive Search
- Microservices using .NET Core
- Azure Mobile features
- Xamarin Mobile

Details:

The application will have are two roles for users: approver and user, as user they will have ability to upload files (pdf, jpg, doc) and search them, all the content once have been submitted is pending for approval, approvers should receive an email/push notification to open the pending approval to accept or reject the content, once the request for upload has been approved it will be saved as immutable stamp in the Blockchain and will be able for all users to search it.
