FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

COPY *.csproj ./
RUN dotnet restore

COPY . ./
RUN dotnet publish -c Release -o out

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/out .

# Install netcat (nc)
# RUN apt-get update && apt-get install -y netcat

# Install netcat-openbsd and clean up in the same layer to reduce image size
RUN apt-get update && \
    apt-get install -y netcat-openbsd dnsutils && \
    rm -rf /var/lib/apt/lists/*

COPY wait-for-it.sh .

# RUN chmod +x wait-for-it.sh
# # Convert CRLF to LF (in case the file has Windows line endings)
# RUN sed -i 's/\r$//' wait-for-it.sh

EXPOSE 80

ENTRYPOINT ["dotnet", "ProductService.dll"]
# ENTRYPOINT ["/bin/bash", "./wait-for-it.sh", "productdb", "1433", "dotnet", "ProductService.dll"]
