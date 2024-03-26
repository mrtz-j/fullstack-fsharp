FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine

# Install necessary dependencies
RUN apk add --no-cache ca-certificates bash openssl curl

RUN rm /etc/ssl/openssl.cnf

COPY dist/ /app

WORKDIR /app
CMD dotnet Server.dll
