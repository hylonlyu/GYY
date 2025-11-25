# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Build Commands

```bash
# Build the entire solution
dotnet build GYY.sln

# Build specific projects
dotnet build GuaDan\GYY.csproj
dotnet build WeControl\WeControl.csproj
dotnet build GuaDan.Tests\GuaDan.Tests.csproj

# Build in Release mode
dotnet build GYY.sln --configuration Release

# Build in Debug mode
dotnet build GYY.sln --configuration Debug
```

## Test Commands

```bash
# Run all tests
dotnet test GuaDan.Tests\GuaDan.Tests.csproj

# Run tests with detailed output
dotnet test GuaDan.Tests\GuaDan.Tests.csproj --verbosity normal

# Run specific test method
dotnet test GuaDan.Tests\GuaDan.Tests.csproj --filter "TestName~TestServerStartStop"
```

## Project Architecture

This solution consists of three main projects:

### 1. GuaDan (Main Application)
- **Technology**: Windows Forms (.NET Framework 4.8)
- **Purpose**: Main GUI application called "GYY"
- **Key Components**:
  - `FrmGuaDan.cs` - Main application form
  - `FrmLogin.cs` - Login form
  - `FrmWebbrowser.cs` - Web browser form
  - `TcpServer.cs` - TCP server for network communication
  - `ExtendedWebBrowser.cs` - Enhanced web browser control
  - `CCMember.cs`, `UserInfo.cs` - User management classes
  - `BetResultInfo.cs`, `RaceInfoItem.cs` - Betting/racing data structures
  - `Connect.cs` - Network connectivity
  - `CryptoHelper.cs`, `Encryption.cs`, `Security.cs` - Security utilities
  - `GdConfig.cs`, `AppConfig.cs` - Configuration management
  - `Util.cs` - General utility functions

- **External Dependencies**:
  - Microsoft.Web.WebView2 for web content
  - HtmlAgilityPack for HTML parsing
  - Interop.MSScriptControl for script control
  - OCR library from external path

### 2. WeControl (Control Application)
- **Technology**: Windows Forms (.NET Framework 4.7.2)
- **Purpose**: UDP-based control application
- **Key Components**:
  - `Form1.cs` - Main form with UDP server functionality
  - Configurable UDP port (default 9000)
  - UDP client/server communication for remote control

### 3. GuaDan.Tests (Test Project)
- **Technology**: .NET 4.8 with MSTest
- **Purpose**: Unit tests for TcpServer functionality
- **Key Components**:
  - `TcpServerTests.cs` - Tests for TCP server start/stop and client connections
  - `Test1.cs` - Additional test file

## Network Architecture

The solution uses both TCP and UDP networking:
- **TCP Server** (GuaDan): Handles multiple client connections with proper connection management
- **UDP Communication** (WeControl): Lightweight messaging for control operations

## Development Notes

- The main entry point for GuaDan is in `Program.cs` which launches `FrmGuaDan`
- Applications are built with Chinese UI elements and comments
- Both WinForms applications target different .NET Framework versions
- The project includes robust exception handling in `GuaDan\Program.cs`
- External dependencies include custom OCR and HTML parsing libraries
- Configuration is managed through INI files and application settings

## Configuration Files

- `GuaDan\App.config` - Configuration for main application
- `WeControl\App.config` - Configuration for control application
- Settings stored in `Properties\Settings.settings` for WeControl