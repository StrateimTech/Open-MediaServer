# Open-MediaServer [![.NET](https://github.com/StrateimTech/Open-MediaServer/actions/workflows/dotnet.yml/badge.svg)](https://github.com/StrateimTech/Open-MediaServer/actions/workflows/dotnet.yml) [![CodeQL](https://github.com/StrateimTech/Open-MediaServer/actions/workflows/codeql.yml/badge.svg)](https://github.com/StrateimTech/Open-MediaServer/actions/workflows/codeql.yml)
Media sharing server application focused on simple video/picture distribution

## Note
This project is currently at the stage where it can functionally be run on a production server without too many hiccups.
<br>Example's running O.M.S.:
* https://media.strateim.tech/ (Personal production instance)

## Features
* Frontend web interface
* Individual user accounts
* LZ4 lossless media compression on large files
* Dynamic media id's to keep file name integrity
* Unlisted media (Like youtube)
* Large variety of file format support (Not just videos/images)
* Configurable domain support
* Configurable file upload size limit
* Discord meta tags supported (OpenGraph)
* [API Analytics](https://www.apianalytics.dev/) (https://github.com/tom-draper/api-analytics)

## Built using
* .NET 6
* Swagger
* SQLite

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
