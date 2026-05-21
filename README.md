# Apple Podcast Transcriber



A .NET 10 full-stack web application designed to automatically download podcasts from Apple Podcasts and transcribe them into text using a local OpenAI Whisper model. The architecture is optimized for fault tolerance, efficient resource management, and a highly responsive user interface.



## Demo

<img width="852" height="480" alt="Video Project 2 (2)" src="https://github.com/user-attachments/assets/fef0566c-f0e2-48d6-b2bb-8f158057b5ef" />


## Core Architecture & Features



* **Throttling Pipeline:** Uses `SemaphoreSlim` to strictly isolate and limit resource usage. AI transcription is locked to a single thread to protect the CPU/GPU from overloading.

* **Network Optimization:** Configured `HttpClient` with automatic GZip, Brotli, and Deflate decompression to reduce incoming bandwidth consumption.

* **Memory-Efficient I/O:** Audio files are downloaded via stream reading (`HttpCompletionOption.ResponseHeadersRead`), bypassing RAM buffering. The system automatically cleans up partial or corrupted files upon timeout or connection failure.

* **Database Idempotency:** Utilizes Entity Framework Core with SQLite to prevent duplicate processing. Attempting to add an existing podcast reuses the previously downloaded data.



## Prerequisites



* .NET 10 SDK or newer.

* A modern multi-core CPU. If you plan to use **GPU acceleration** for significantly faster Whisper transcription, ensure you have the appropriate drivers and runtimes pre-installed on your system 

* Apple Podcasts Bearer Token (required for Apple API authentication).

* Whisper Model in GGML format (e.g., `ggml-small.bin`).



## Tech Stack

Backend: ASP.NET Core Minimal APIs (.NET 10), Task-based Concurrency.



* Frontend: Blazor Web App, JS Interop, Bootstrap 5.

* Database: SQLite, Entity Framework Core.

* AI Processing: Whisper.net.



## Installation & Setup



### 1. Clone the Repository

```bash

git clone [https://github.com/YOUR_USERNAME/ApplePodcastTranscription.git](https://github.com/YOUR_USERNAME/ApplePodcastTranscription.git)

cd ApplePodcastTranscription 

```

### 2. Configure API-token

```bash

dotnet user-secrets set "AppleBearer" "YOUR_REAL_BEARER_TOKEN"

```

### 3. Add AI Model

Create a `ApplePodcastTranscription/ApplePodcastTranscription/Resources/STTModels/WhisperSmall` directory in the project root and place your downloaded `ggml-small.bin` file inside.



### 4. Add database file

Create a `ApplePodcastTranscription/ApplePodcastTranscription/Resources/Database` directory in the project root and drop an empty file named `database.sqlite` inside it. Necessary tables will be generated automatically on the first run.



### 5. Finally, run the app!

``` bash

dotnet run 

