# TvMazeScrapper

TvMazeScrapper is a .NET 8 web application that fetches and displays metadata about TV shows and their cast from the TVMaze API. The application is designed to handle large volumes of data efficiently, respecting API rate limits, and providing an optimized solution for fetching and persisting show metadata.

## Table of Contents

- [Features](#features)
- [Requirements](#requirements)
- [Services Overview](#services-overview)
- [Controllers Overview](#controllers-overview)
- [Usage](#usage)
- [Configuration](#configuration)
- [Testing](#testing)

## Features

- Fetches TV show and cast information from the TVMaze API.
- Handles API rate limiting using Polly for retry strategies.
- Optimized for performance with parallel processing and concurrency control.
- Provides a REST API to display TV shows and their cast.

## Requirements

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Visual Studio 2022](https://visualstudio.microsoft.com/vs/) or any other compatible IDE
- TVMaze API (No authentication required, but subject to rate limiting)

## Services Overview

### `FetchAllShowsAsyncService`
This service is responsible for fetching all TV shows from the TVMaze API. It:
- Uses `SemaphoreSlim` to limit the number of concurrent requests, ensuring that the API rate limit is respected.
- Implements retry policies with Polly to handle transient errors and rate limiting responses.
- Retrieves TV show information page-by-page and handles the pagination efficiently using parallel processing.

### `FetchCastForShowAsyncService`
This service is responsible for fetching the cast information for a specific TV show using the show's ID. It:
- Controls the number of concurrent API calls using `SemaphoreSlim` to respect the rate limits.
- Uses Polly to implement retry strategies for handling rate limit responses and other transient errors.
- Parses and processes the cast data into a list of `CastPerson` objects, handling different formats and ensuring proper deserialization.

### `ShowMetaDataService`
This service handles the storage and retrieval of TV show metadata in the application's database. It:
- Saves the JSON data of the fetched TV shows and their cast to the database.
- Ensures that there is only one record of metadata in the database by either updating or creating a new record as necessary.

## Controllers Overview

### `ShowsController`
This controller manages the application's endpoints related to TV shows and their cast. It:
- Provides endpoints for fetching a list of all TV shows along with their cast information.
- Implements methods to sync data from the TVMaze API manually.
- Uses services like `FetchAllShowsAsyncService` and `FetchCastForShowAsyncService` to retrieve data and `ShowMetaDataService` to save the data.
- Supports data synchronization and provides views for displaying the data in a user-friendly format.

## Usage
Navigating to 'Shows Json' in the Nav bar and click on Sync All will fetch all the pages 
- This will save the result in the database and display
