# auctions

## Description

This code-along project is a microservices based web application made as a part of the course [Build a Microservices app with .Net and NextJS from scratch](https://www.udemy.com/course/build-a-microservices-app-with-dotnet-and-nextjs-from-scratch/). 

The objective of the project was to build a microservices based backend using Entity Framework to code the different REST APIs, unit and integration tests with XUnit. 

This project also served as an introduction to NextJS which was used to build the frontend and Identity Server which was used to provide authentication. 
Other tools used were RabbitMQ to create a service bus that allows services to communicate with each other, YARP to setup a gateway to redirect requests to get the correct service and, SignalR to send notifications to the clients. 


##  Functionality

This web application allows customers to create a user, create auction for cars and set bids for existent auctions. 

## Structure

### Backend 

- Auction service: Create, delete and update. It also has an endpoint to search auctions but the web application uses the search service. Each time an auction is created, finished, updated or deleted it uses the service bus to send to other services so they can update their databases.

- Search service: Allows the web client to search auctions created.

- Bidding service: Allows the web client to set bids on the different auctions. Each time a bid is placed, the service sends a message through the service bus to indicate to other services they should update auction information (highest bid and bidder) in their respective databases. Uses gRPC to retrieve auctions from the auctions service the service might have missed / not been able to add to its database.

- Identity service: IdentityServer project that uses ASP.NET Identity for user management. Used to créate and authentícate users and to secure most of the different service’s endpoints.

- Notification service: The web client remains connected to this service. When an auction is created, finishes or a bid is placed, this service sends notifications to each client connected to it.

- Gateway service: Reverse proxy to redirect requests to the correct service using YARP.

- Contracts: Contains the different events sent and consumed by the services through the service bus.

### Additional information

Auction, search and bidding services have a separate database. The different databases share information about the auction and item. The bidding service’s database contains information about every bid placed on each auction.

### Tests

Integration and unit tests were added for the auction service.

### Frontend

NextJSWeb client built using with Tailwind CSS, Flowbite, SignalR, and Zustand that allows users to search, create, update, delete auctions and to make bids for existent auctions.

## Installation
1. [Update OS host file](https://www.hostinger.com/tutorials/how-to-edit-hosts-file) to add app.auctions.com, id.auctions.com, and api.auctions.com.
2. Install [mkcert](https://github.com/FiloSottile/mkcert).
3. Go to the devcerts folder.
4. Use mkcert to generate a development certificate for this project by running the following command:
```
mkcert -key-file auctions.com.key -cert-file auctions.com.crt app.auctions.com api.auctions.com id.auctions.com
```
5. Build and run the container by running this command:
```
docker compose up -d 
```
6. Go to [app.auctions.com](app.auctions.com) to enter the application.

## Screenshots

![1](https://github.com/fsv2860/auctions/assets/47431198/068d0da6-dc2c-4c02-9414-a8b9faf88357)
![3](https://github.com/fsv2860/auctions/assets/47431198/854c09e8-f347-4101-8282-0e41b346ba54)
![5](https://github.com/fsv2860/auctions/assets/47431198/3cce8595-548d-490e-a7cd-c7067ac9944b)
![6](https://github.com/fsv2860/auctions/assets/47431198/0efe59ee-c92b-4eee-9482-85b443057d3b)
