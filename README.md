Info to be added in the future

```bash
sauna-booking/                      ← GitHub root
├── .git/                           ← Git config
├── .gitignore
├── LICENSE
├── README.md                       ← this file
├── api/                            ← the .NET "backend" API
│   ├── SaunaBookingApi.csproj
│   ├── Program.cs
│   ├── appsettings*.json
│   └── [Models, Services, etc.]    ← upcoming folder structure
├── client/                         ← Blazor WASM project, "frontend"
│   ├── client.csproj
│   ├── Program.cs
│   ├── App.razor
│   ├── _Imports.razor
│   ├── Layout/
│   ├── obj/
│   ├── Pages/
│   ├── Properties/
│   └── wwwroot/
└── scripts/                        ← Deploy scripts, GitHub Actions, CI/CD
```