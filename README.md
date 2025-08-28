# Auctionsite

## Description
A modern web-based auction platform where users can create ads, bid in real-time, chat, and manage their auctions seamlessly. Built with .NET 9 and ASP.NET Core MVC and Razor Pages

## Project Status
This project was developed during a 3-month internship at Lexicon. It represents the initial implementation of an auction marketplace. Future development will be continued by the next internship team, to further enhance and expand the functionality for the client.

## Features
- **User Accounts:** Registration, login, role-based access
- **Advertisements:** Create, edit, browse, and favorite ads
- **Bidding:** Real-time bidding system with notifications
- **Ad Management:** Admin review and approval system
- **Ad Images:** Upload and manage images with Sortable.js
- **Sharing:** Share ad links to external platforms
- **Communication:** Chat between users
- **Search & Filter:** Find ads by categories, keywords, or filters
- **Notifications:** Email and in-app notifications for bids and messages
- **User Profiles:** Manage personal information and settings
- **Security:** Password recovery and authentication flows
- **Responsive Design:** Optimized UI for both desktop and mobile

## Technology Stack
- **Backend:** C#, .NET 9, ASP.NET Core MVC, Entity Framework Core, LINQ, SQL Server  
- **Frontend:** Razor Pages & Razor Views, HTML, CSS, Bootstrap, JavaScript
- **Libraries / APIs:** MailKit, MimeKit, Google APIs, SignalR, Identity, Sortable.js, Toastr.js   

## My Contribution
Implemented features for creating, editing and browsing ads, as well as an admin flow for review. Built the category system and designed database structures. Enabled ad sharing functionality for external platforms and ensured a responsive and user-friendly interface.
- Ad management (CRUD)
- Ad image upload and management via Sortable.js
- Ad sharing functionality to external platforms
- Admin flow for review
- Category system
- Database designs

## Installation
1. Clone the repository:
```bash
git clone https://github.com/Kvidi/Auktionssite.git 
``` 
2. Open the solution in Visual Studio 2022 (`Auctionsite.sln`)
3. Restore NuGet packages.
4. Copy the placeholder `appsettings.Template.json` to `appsettings.Development.json`  
    (`appsettings.Development.json` is ignored by Git for security reasons).  

    Then provide your own values for:
    - **Database connection string**
    - **Google Authentication:**
        - `GoogleClientId`
        - `GoogleClientSecret`
    - **Email (SMTP) settings:**
        - `SmtpServer`
        - `SmtpPort`
        - `SmtpUsername`
        - `SmtpPassword`
        - `SenderEmail`
        - `SenderName`

5. Run the database migrations to set up the database schema.
   - Open the Package Manager Console in Visual Studio.
   - Run the command:
		```powershell
		Update-Database
		```
6. Build and run the application.

## Usage

### For Users
- Register a new account.
- Log in to access full features.
- Browse, create, and favorite ads.
- Place bids in real-time auctions.
- Use the chat feature to communicate with other users.
- Share ad links on social media or via email.
- Manage your profile.

### For Administrators
- Review and approve ads
- View users