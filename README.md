# Messenjoor - Realtime Chat App using Blazor WASM and SignalR

> Messenjoor is a fullstack realtime chat app using Asp.Net Core Blazor Web Assembly (WASM) and SignalR Core with SQL Server and EF Core


## To Run Locally
- Clone the Repo
    `git clone https://github.com/Joorak/Messenjoor `
    
- Restore the packages (Rebuild the solution)
    
- Set the `Messenjoor.Server` project as startup project
    
- Change the Database connection string in `appsettings.Development.json` file in `Messenjoor.Server` Project
    ```
    "ConnectionStrings": {
        "Chat": "Data Source=.;Initial Catalog=Messenjoor; Integrated Security=True; Trusted_Connection=True;"
     }
     ``` 
     
- Run the migrations - to update the database using following Package Manager Console Command
    
    `Update-Database`

- Run the solution

- Congratulations, Messenjoor  app is running
