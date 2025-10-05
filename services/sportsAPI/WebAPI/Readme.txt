
RUN THIS TO SETUP DB

 dotnet ef migrations add InitialCreate --context AppDbContext
 dotnet ef database update --context AppDbContext