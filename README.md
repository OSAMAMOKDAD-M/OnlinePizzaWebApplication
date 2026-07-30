# OnlinePizzaWebApplication
OnlinePizzaWebApplication is a web-based application designed to streamline the online pizza ordering process for customers and administrators. The system allows users to browse the menu, customize pizzas (size, crust, toppings), add items to a shopping cart, and complete orders through a simple and user-friendly interface.
contact me : mokdadvipr@gmail.com

## Configuration

The database connection string is not stored in source control. Provide it at runtime:

```
# environment variable (any OS / container)
ConnectionStrings__DefaultConnection="Server=...;Database=PizzaShop;..."

# or, for local development
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=(localdb)\MSSQLLocalDB;Database=PizzaShop;Trusted_Connection=True"
```

The app fails to start with a clear error if no connection string is configured.
Set `AllowedHosts` to the host name(s) the app is served from in each environment.
