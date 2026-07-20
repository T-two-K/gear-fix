# ⚙️ Gear fix
Deskpot application developed in WPF, сreated to help people who are not familiar with cars, as well as beginner mechanics, so that they can quickly identify possible diagnoses. Third-party APIs have been integrated into the application, such as: Gemini API, API NHTSA, 2ГИС API. The app also allows you to add and edit records about the user's vehicle and any malfunctions it may have. All app data is stored in an encrypted JSON file locally on the user's computer. Encryption in the app is performed using AES-GCM technology, with the hash key derived from the password, which is then passed to the Argon2 algorithm.
## ⚠️ Warning! 
It's recommended to enable VPN before launching the app for users located in restricted countries. If this doesn't apply to you, you can ignore this section.
## 🛠️ Stack
***
  ### 🗣️ Languages
    - C# 14 / .NET 10
    - JavaScript (ESMAScript 2025)
    - CSS3
    - HTML5 
  ### 🎞️ Frameworks
    - WPF (latest on 20.07.2026)
  ### 📚 Libraries
    - Leaflet.js (maps | latest on 20.07.2026)
    - CommunityToolkit.MVVM (8.4.2)
    - Microsoft.Extensions.DependencyInjection (10.0.9)
    - Konscious.Security.Cryptography.Argon2 (1.3.1)
    - Microsoft.Web.WebView2 (1.0.4022.49)
    - System.Text.Json
  ### 🏗️ Architecture
    - MVVM Pattern
    - Repository Pattern
    - DTO Pattern
    - Dependency Injections
  ### 📱 Integrated API 
    - Google Gemini API
    - NHTSA API
    - 2ГИС API
  ### 🗃️ Data management
    - Json
    - LINQ
***
## ⏳ Implementation process
At start I prepared models, that will be use in the future. Set up them and connected with each other (added relationships). But I didn't know how to work with     APIs, so I haden't implemented models fot it (for now).

Then I started implementing services for password hashing, data encryption, and file handling. When I had done that, I decided to create one service that would join them (ManageDataService).

Afterwards, I began studying APIs: how to work with them and what needed to be done to use them in my application. Then, I developed models for working with the APIs I needed.

Next, I developed the login and registration windows. I linked the windows to their View Models, which already contained the IManageDataService interface.

The next step was to create the application's main menu, where the user would have access to all CRUD operations related to their vehicle card.

After the main menu, I began developing the diagnostics window. This required setting up a connection with the Google Gemini API, creating a field with instructions for the AI ​​advisor, and connecting to the official NHTSA (National Highway Traffic Safety Administration) API. Next, I designed a window with a list of errors sent by the AI ​​advisor in a special JSON format.

After the diagnostics window, I decided to add the ability to change the password and API key, in case the user's password was stolen or the user simply decided to change their API key. I also decided to add the ability to delete the main application file, which stores all of the user's confidential information.

Finally, I decided to add a local map to the app. I decided to use a ready-made map from the Leaflet.js library, but it only worked in the browser, so I integrated a separate component: WebView via a Microsoft library. To retrieve workshop data, I decided to use the 2GIS API, as they offer free access to their databases for one month through a demo API key.

To connect the browser component with my desktop application, I developed a separate JavaScript file that sends POST requests via WebView to the application, which I process in the application. This JavaScript file also receives requests from my application using WebView commands and executes the corresponding commands. The request looks like this:
```csharp
  await _webView.CoreWebView2.ExecuteScriptAsync($"showUserLocation({correctLat}, {correctLon}, {radiusKm})");
```
At the end of development, minor bugs and errors were fixed, after which manual testing was completed.
## 💭 How can it be improved?
- Add AI from various organizations (Claude, GPT, Grok);
- Connect more free databases containing information about vehicle malfunctions and repairs;
- Add the ability to attach various files to the request field (photos, audio messages);
- Add a route plotting function to the nearest service station to the map;
- Add the ability to change the application theme.
## 🚀 How to run it
1. Copy the repository to your computer (git clone "https://github.com/T-two-K/gear-fix.git" or download ZIP).
2. If you have Visual Studio click to the file GearFix.csproj and then use shortcut "Ctrl + f5", if you don't then open powershell (or other command line) and complete the following commands:
```powershell
  winget install Microsoft.DotNet.SDK.10
  cd gear-fix #(or full path to this folder, for example: C:/Users/user123/gear-fix)
  dotnet restore
  dotnet build
  dotnet run
```
## 💡 How to use it
