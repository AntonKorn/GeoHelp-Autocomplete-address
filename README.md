# GeoHelp - Autocomplete cities, countries, streets, houses

We often meet the need of lisiting the Cities, Countries and addresses in 
some various UIs. Usually it is needed to fill user's personal data in initial registration.
Usual flow - user sees 2 dropdowns with countries and cities.
Ones country is selected, cities dropdown enables and values are
filtered based on selected country.

Also, there potentially may be one or 2 dropdowns for the street and the house number.

Recently with my friends I was working on the hobby project, and we figured 
that it is hard or impossible to find the server provider of this data that is fast+free+customizable.
However, all data that we need to solve this problem is now available for free.
Therefore, I quickly made the skeleton that can be used to maintain this task.

## Tech stack
The data is stored in **MongoDB** database. It is generated and then seeded with **2 .NET console** applications.

End web application is read only and is created with **ASP.NET Core Web API**.

## Usage

If you have an orders website and need to implement autocompletions of the addresses,
you can use the API provided by the repository. To bring up your own deployment, perform
the following steps:

1. Export from OSM the data needed for your busyness using the DataLoader project
2. Bring up free MongoDB cluster in mongodb atlass
3. Seed the data using DataSeeder project, make sure to specify the configuration and move seed files to correct location
4. Create web application in Somee and deploy API code there

If any question or problem erises, feel free to open an issue

## Architecture

In general, this project can be used in the following way:
1. Seeding data is loaded from https://wiki.openstreetmap.org/wiki/Overpass_API using DataLoader project
2. Result files are taken from /outputs folder and placed under /Seeds of DataSeeder console app. Later, with the connection string specified this app will prepare the collections, data, and indexes to the mongo database specified
3. Web API application (GeoHelp.Web) can now be launched and linked to the seeded database.

In the current project, there is also seeding data included under DataLoader/Seeding. This data is loaded from **Overpass API** with the instructions provided above

## Free deployment options

You can easily deploy this application using conditionally free resources like https://somee.com/ and https://www.mongodb.com/atlas/database.

## Demo

http://geohelp.somee.com/swagger/index.html

The application is deployed under this url. The deployment is free, therefore it is significantly throttled

## How can I get Latitude and Longitude of the house?

For this, overpass API mast be executed on the UI. The following query will solve the problem:

```
[out:json];
nrw({osmId from response under Buildings})["building"];
out center;
```

This query will return the item with latitude and longitued. It wasn't added to neither the API or the data to support free deployment capability.
