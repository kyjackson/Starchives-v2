# Starchives 2.0 - Blazor Edition

## About

Starchives is a caption search engine for videos on the Star Citizen YouTube channel.
A word or phrase can be entered into the site's home page search bar, and all videos with
matching captions (auto-generated, mostly) will be returned, with timestamped links to the
locations of the matching captions in each video.
<br/><br/>
This project is the next major iteration of the [original Starchives](https://github.com/kyjackson/starchives) project,
with the main highlight being that this version is a Blazor Web App conversion of the first
project, which was based on Node.js. This conversion was undertaken by the project's creator 
as an effort to quickly gain experience working with the ASP.NET Core ecosystem, with the goals
of being able to more readily provide support for Starchives and other unrelated ASP.NET Core projects, 
as well as creating a template that can be used by anyone for both educational purposes and for recreating this
project to work with any other YouTube channels desired.



## Usage

### Starchives Web App
TODO - How to use the Starchives site

### Starchives Template
TODO - How to use the Starchives project to set up a similar search engine for any other channel

#### Prerequisites
TODO - What are specific minimum requirements to use your packages? Consider excluding this section if your package works without any additional setup beyond simple package installation.



## Additional documentation

### Bootstrap
The [Bootstrap frontend toolkit](https://getbootstrap.com/docs/5.3/getting-started/introduction/) is used for the site's UI design.

### YouTube
The [.NET client library](https://googleapis.dev/dotnet/Google.Apis.YouTube.v3/latest/api/Google.Apis.YouTube.v3.html) for the [YouTube Data API](https://developers.google.com/youtube/v3/getting-started) 
is used for retrieving the latest data for all videos on the RSI channel.

The [Youtube Explode library](https://github.com/Tyrrrz/YoutubeExplode) is used for retrieving new caption tracks in a cost-efficient manner.

### Entity Framework
The [EFCore.BulkExtensions.SqlServer library](https://www.nuget.org/packages/EFCore.BulkExtensions.SqlServer#readme-body-tab) is used for 
bulk CRUD operations on data stored in the Starchives database.

### Serilog
The [Serilog library](https://github.com/serilog/serilog) is used as the base for logging in this project.

The [Serilog ASP.NET Core package](https://github.com/serilog/serilog-aspnetcore) is used for routing ASP.NET Core-specific
logs through the Serilog platform.
