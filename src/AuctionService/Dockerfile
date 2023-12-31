## Start of common section for all services.

# 1. Get base image (as build)
FROM mcr.microsoft.com/dotnet/sdk:7.0 as build
# 2. Set our work directory (inside Docker's file system)
WORKDIR /app
# 3. Expose port (80) we are going to be using internally inside Docker.
EXPOSE 80

# 4. Copy all .csproj files and restore as distinct layers. Use of the same COPY command
# for every dockerfile in the project to take advantage of docker caching.

# COPY (from computer) (WORKDIR inside Docker's filesystem).

# 4.1. Copy ROOT SOLUTION FILE into Docker's file system (using the exact same name.)
COPY auctions.sln auctions.sln

# 4.2. Copy EVERY .csproj file necessary (for our project) into Docker's file system
COPY src/AuctionService/AuctionService.csproj src/AuctionService/AuctionService.csproj
COPY src/GatewayService/GatewayService.csproj src/GatewayService/GatewayService.csproj
COPY src/IdentityService/IdentityService.csproj src/IdentityService/IdentityService.csproj
COPY src/SearchService/SearchService.csproj src/SearchService/SearchService.csproj
COPY src/BiddingService/BiddingService.csproj src/BiddingService/BiddingService.csproj
COPY src/NotificationService/NotificationService.csproj src/NotificationService/NotificationService.csproj
COPY src/Contracts/Contracts.csproj src/Contracts/Contracts.csproj
COPY tests/AuctionService.UnitTests/AuctionService.UnitTests.csproj tests/AuctionService.UnitTests/AuctionService.UnitTests.csproj
COPY tests/AuctionService.IntegrationTests/AuctionService.IntegrationTests.csproj tests/AuctionService.IntegrationTests/AuctionService.IntegrationTests.csproj

# 5. Restore EVERY package dependency this ROOT project files and all the SUB project files have.
RUN dotnet restore auctions.sln

## End of common section for all services.

# 6. Copy this service's app folders over (rest of the code) and all code it depends on.
COPY src/AuctionService src/AuctionService
COPY src/Contracts src/Contracts

# 7. Change the work directory (directory must exist before changing to it).
WORKDIR /app/src/AuctionService

# 8. Publish this service using release configuration (-c) and release them
# inside the directory (inside Docker's file system) app/src/out .
RUN dotnet publish -c Release -o /app/src/out

# 9. Build runtime image.
# Give us access to a smaller dotnet run time (aspnet).
FROM mcr.microsoft.com/dotnet/aspnet:7.0

# 10. Switch work directory to /app
WORKDIR /app

# 11. Copy all the directory content (.) inside /app/src/out
COPY --from=build /app/src/out .

# 12. Specify entrypoint for this service.
ENTRYPOINT [ "dotnet", "AuctionService.dll" ]