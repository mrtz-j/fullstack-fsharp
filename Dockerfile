FROM mcr.microsoft.com/dotnet/aspnet:7.0-alpine

# Install necessary dependencies
RUN apk add --no-cache ca-certificates bash openssl curl

RUN rm /etc/ssl/openssl.cnf

COPY deploy/ /app

WORKDIR /app
CMD dotnet Server.dll
