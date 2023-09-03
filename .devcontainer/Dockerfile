FROM mcr.microsoft.com/dotnet/sdk:7.0-alpine

ENV PNPM_VERSION "8.6.6"

# Install node, 7zip, git, curl, bash and process tools
RUN apk add --no-cache git nodejs p7zip procps libgdiplus curl bash coreutils

RUN set -eux; \
  curl -fsSL "https://github.com/pnpm/pnpm/releases/download/v${PNPM_VERSION}/pnpm-linuxstatic-x64" -o /bin/pnpm; \
  chmod +x /bin/pnpm;

# Trouble brewing
RUN rm /etc/ssl/openssl.cnf

# add dotnet tools to path to pick up fake and paket installation
ENV PATH="/root/.dotnet/tools:${PATH}"
