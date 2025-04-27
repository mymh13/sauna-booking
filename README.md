Info to be added in the future, until then, this is the project structure:

```css
sauna-booking/                          Root folder of the project/repository (monorepo)
├── .github/
│   └── workflows/
│       └── deploy.yaml                 GitHub Actions workflow to build and deploy backend (Cloud) and frontend (FTP)
│
├── api/                                .NET API, ("backend")
│   ├── .local-data/                    Local production only: not tracked by Git
│   │   └── sauna-booking.db            Local Database for development only, not tracked
│   ├── Controllers/
│   │   └── AuthController.cs           TODO:
│   ├── Data/
│   │   └── SaunaBookingDbContext.cs    TODO:
│   ├── Migrations/
│   │   └── *                           Entity Framework Migration-files
│   ├── Models/         
│   │   ├── Booking.cs                  TODO:             
│   │   ├── LoginRequest.cs             TODO:
│   │   ├── LoginResponse.cs            TODO:
│   │   └── User.cs                     TODO:
│   ├── Properties/
│   │   └── launchSettingss.json        Local development server profiles (port, environment, SSL, etc.)
│   ├── Services/                       Business logic classes (e.g., BookingService, ValidationService)
│   ├── appsettings.Development.json    Development environment settings (local database config, logging level)
│   ├── appsettings*.json               Placeholder for additional environments (e.g., Production)
│   ├── Program.cs                      Application startup code (currently the main logic runs here directly)
│   └── SaunaBookingApi.csproj          .NET project file for the backend
│
├── client/                             Blazor WASM project, ("frontend")
│   ├── Layout/
│   │   ├── MainLayout.razor            Main layout wrapper for pages
│   │   ├── MainLayout.razor.css        Styling for MainLayout
│   │   ├── NavMenu.razor               Navigation menu component
│   │   └── NavMenu.razor.css           Styling for NavMenu
│   ├── Models/                         NOTE! Blazor WASM cannot reference backend projects, so we need frontend Models too
│   │   ├── LoginRequest.cs             See /Models above!
│   │   ├── LoginResponse.cs            See /Models above!
│   ├── Pages/
│   │   ├── Home.razor                  Landing page of the frontend
│   │   ├── Login.razor                 TODO: 
│   │   └── Login.razor.css             TODO: 
│   ├── Properties/
│   │   └── launchSsettings.json        Frontend local development server profiles
│   ├── Services/
│   │   └── AuthService.cs              TODO: 
│   ├── wwwroot/
│   │   ├── css/
│   │   │   └── app.css                 General frontend custom styles         
│   │   ├── lib/
│   │   │   └── *                       Bootstrap and external libraries
│   │   ├── favicon.ico                 Favicon for browser tabs (legacy format)
│   │   ├── favicon.webp                Modern favicon format
│   │   └── index.html                  HTML bootstrapper for Blazor WASM app
│   ├── _Imports.razor                  Common Razor imports for all frontend components
│   ├── App.razor                       Root component for Blazor routing
│   ├── client.csproj                   .NET project file for the frontend
│   └── Program.cs                      Startup configuration for Blazor client
│
├── client_publish/                     Non-tracked (by Git) directory where CI/CD publish and then ship code
│   └── wwwroot/
│
├── .gitignore                          Git rules for ignoring binaries, temp files, secrets, etc.
├── LICENSE                             Open-source license for the project
├── README.md                           This file: basic project overview and structure
└── sauna-booking.sln                   VS solution file tying together api/ and client/
```