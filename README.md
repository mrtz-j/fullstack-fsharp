# Fullstack Template using F# and Fable

This repository contains a minimal [SAFE](https://safe-stack.github.io/docs/intro/)-style template for creating a fullstack F# project using Fable, Fable.Remoting and Saturn. 

To get started with a new project, click the **Use this template** button to create a new repository from this template, and check out the [docs on using templates](https://docs.github.com/en/github/creating-cloning-and-archiving-repositories/creating-a-repository-from-a-template).

## Requirements 

* [Dotnet SDK](https://www.microsoft.com/net/download/core) 7.0 or higher
* [Node.js](https://nodejs.org) 16 or higher
* An F# editor like Visual Studio, Visual Studio Code with [Ionide](http://ionide.io/) or [JetBrains Rider](https://www.jetbrains.com/rider/)

Or you can use the [VS Code Remote Container](https://code.visualstudio.com/docs/remote/containers?WT.mc_id=dotnet-33392-aapowell) for development, as it will set up all the required dependencies.

## Features 

### Server 

* [Saturn](https://saturnframework.org/) web server
* [Fable.Remoting](https://zaid-ajaj.github.io/Fable.Remoting/#/server-setup/saturn) for client-server communication
* Logging with [Serilog](https://serilog.net/)

### Client

* [Fable](https://fable.io/) for F# to JavaScript compilation
* RPC routing with [Fable.Remoting](https://zaid-ajaj.github.io/Fable.Remoting/#/client-setup/fable)
* Styling with [Fluent UI](https://github.com/sydsutton/FS.FluentUI)
* Shared client-server types

## Build

To concurrently run the server and client in watch mode use the following command in the build script:

```bash
dotnet run
```

Run server and compile frontend manually

```bash
# For Server
cd src/Server && dotnet run

# For Client
pnpm install .
dotnet tool restore
dotnet fable watch -s -o .build --run vite -c ../../vite.config.js
```
The backend server will default to port `8085`.

To access the frontend navigate to `http://localhost:8080/` in your preferred browser.

To build the server and client in release mode use the following command:

```bash
dotnet run bundle
```