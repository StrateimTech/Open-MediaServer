# Open-MediaServer [![.NET](https://github.com/StrateimTech/Open-MediaServer/actions/workflows/dotnet.yml/badge.svg)](https://github.com/StrateimTech/Open-MediaServer/actions/workflows/dotnet.yml) [![CodeQL](https://github.com/StrateimTech/Open-MediaServer/actions/workflows/codeql.yml/badge.svg)](https://github.com/StrateimTech/Open-MediaServer/actions/workflows/codeql.yml)
File sharing server application focused on simple video/picture distribution

## Features
* Frontend web interface
* Individual user accounts
* LZ4 lossless media compression on large files
* Dynamic media id's to keep file name integrity
* Unlisted media (Like youtube)
* Large variety of file format support (Not just media)
* Configurable domain support
* Configurable file upload size limit
* Discord meta tags supported (OpenGraph)

## Built using
* .NET 6
* Swagger
* SqLite

## Supports
* Windows
* Linux

## Installation
### Development
```
git clone https://github.com/StrateimTech/Open-MediaServer.git
cd Open-MediaServer
dotnet run --project Open-MediaServer
```