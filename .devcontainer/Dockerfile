FROM mcr.microsoft.com/dotnet/sdk:8.0-alpine

# Install node, 7zip, git, curl, bash and process tools
RUN apk add --no-cache git p7zip procps libgdiplus curl bash coreutils jq

# Install Bun
RUN curl -fsSL https://bun.sh/install | bash

# Trouble brewing
RUN rm /etc/ssl/openssl.cnf

# add dotnet tools to path to pick up fake and paket installation
ENV PATH="/root/.dotnet/tools:${PATH}"

# Copy endpoint specific user settings into container to specify
# .NET Core should be used as the runtime.
COPY settings.vscode.json /root/.vscode-remote/data/Machine/settings.json
