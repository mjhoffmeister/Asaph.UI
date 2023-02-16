using Asaph.UI;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MudBlazor.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

IConfiguration configuration = builder.Build().Configuration;

// Reference config values that will be used multiple times
string? backendApiBaseUrl = configuration["BackendApi:BaseUrl"];
string? backendApiScope = configuration["BackendApi:Scope"];

// Add "Anonymous" HTTP client used for initially retrieving API documentation
if (backendApiBaseUrl != null && backendApiScope != null)
{
    builder.Services.AddHttpClient(
        "Anonymous",
        client => client.BaseAddress = new Uri(backendApiBaseUrl));

	// Add "WebApi" HTTP client used for API requests and configure it to use access tokens
	builder.Services
		.AddHttpClient("WebApi", client => client.BaseAddress = new Uri(backendApiBaseUrl))
		.AddHttpMessageHandler(sp => sp.GetRequiredService<AuthorizationMessageHandler>()
			.ConfigureHandler(
				authorizedUrls: new[] { backendApiBaseUrl },
				scopes: new[] { backendApiScope }));
}

// Configure MSAL authentication
builder.Services.AddMsalAuthentication(options =>
{
    builder.Configuration.Bind("AzureAdb2c", options.ProviderOptions.Authentication);

    options.ProviderOptions.DefaultAccessTokenScopes.Add("openid");
    options.ProviderOptions.DefaultAccessTokenScopes.Add("offline_access");

    if (backendApiScope != null)
        options.ProviderOptions.DefaultAccessTokenScopes.Add(backendApiScope);

    options.ProviderOptions.LoginMode = "redirect";
});

builder.Services.AddMudServices();

await builder.Build().RunAsync();