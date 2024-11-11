namespace Chirp.Web.Playwright.Test;

using Microsoft.Playwright;
using System;
using System.Threading.Tasks;
using Chirp.Web;

class EndToEndTests
{
    [Test]
    public async Task UsersCanRegisterAndLogin()
    {
        using var playwright = await Playwright.CreateAsync();
        await using var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
        {
            Headless = false,
        });
        var context = await browser.NewContextAsync();
        var page = await context.NewPageAsync();
        
        await page.GotoAsync("http://localhost:5273/");
        await page.Locator("Register").ClickAsync();
        await page.GetByText("Register").ClickAsync();
        await page.WaitForURLAsync("http://localhost:5273/Identity/Account/Register");
        
    }
    
    public static async Task Main()
    {
        using var playwright = await Playwright.CreateAsync();
        await using var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
        {
            Headless = false,
        });
        var context = await browser.NewContextAsync();
        var page = await context.NewPageAsync();
        await page.GotoAsync("http://localhost:5273/");
        await page.GetByRole(AriaRole.Link, new() { Name = "Register" }).ClickAsync();
        await page.WaitForURLAsync("http://localhost:5273/Identity/Account/Register");
        
        await page.GetByLabel("Username").ClickAsync();
        await page.GetByLabel("Username").FillAsync("Johan");
        await page.GetByLabel("Username").PressAsync("Tab");
        await page.GetByPlaceholder("name@example.com").FillAsync("jing@itu.dk");
        await page.GetByRole(AriaRole.Textbox, new() { Name = "Password" }).ClickAsync();
        await page.GetByRole(AriaRole.Textbox, new() { Name = "Password" }).FillAsync("Johan1234!");
        await page.GetByRole(AriaRole.Textbox, new() { Name = "Password" }).PressAsync("Tab");
        await page.GetByLabel("Confirm Password").FillAsync("Johan1234!");
        await page.GetByRole(AriaRole.Button, new() { Name = "Register" }).ClickAsync();
        await page.WaitForURLAsync(
            "http://localhost:5273/Identity/Account/RegisterConfirmation?email=jing@itu.dk&returnUrl=%2F");
        await page.GetByRole(AriaRole.Link, new() { Name = "Click here to confirm your account" }).ClickAsync();
        await page.WaitForURLAsync(
            "http://localhost:5273/Identity/Account/ConfirmEmail?userId=c86a18e6-9f6e-4a31-a30d-1d849ece7432&code=Q2ZESjhCSmt5Syt3OU1WTWgrQi9Zb0djazVoKy9GckNpM25PRWt2U0IwWTdadlFtUXBIZU9jNFZ0dUZ5VzFDdlYrQkM3RndxclkrMjFhNFdVUndCV3ROVkRSblYzSUgrOCs1ZXBOQ3RSWE1nTEUzc0c2Z1EzMVJQVzBqZmg4TGllTm54MERTa0pnOXhmbzVORmZuVkVydDBSYkFaQUtJL3pCZTlzQTJUYktMdFpQeVdIbWlzdjBIT0pMaVcwaHRkZnlmb2twWDhYK0tuZmY3S25DOGF2bHc0T0lnaXRFQUMzTzdrVk94OG9MUmp5bmh1RDB4VEIrS1NPbVJuamZYUUtYN1JTZz09&returnUrl=%2F");
        await page.GetByRole(AriaRole.Link, new() { Name = "Login" }).ClickAsync();
        await page.WaitForURLAsync("http://localhost:5273/Identity/Account/Login");
        await page.GetByPlaceholder("name@example.com").ClickAsync();
        await page.GetByPlaceholder("name@example.com").FillAsync("jing@itu.dk");
        await page.GetByPlaceholder("name@example.com").PressAsync("Tab");
        await page.GetByPlaceholder("password").FillAsync("Johan1234!");
        await page.GetByRole(AriaRole.Button, new() { Name = "Log in" }).ClickAsync();
        await page.WaitForURLAsync("http://localhost:5273/");
        await page.Locator("input[name=\"Text\"]").ClickAsync();
        await page.Locator("input[name=\"Text\"]").FillAsync("Hello, my group is the best group!");
        await page.GetByRole(AriaRole.Button, new() { Name = "Share" }).ClickAsync();
        await page.WaitForURLAsync("http://localhost:5273/");
        await page.GetByRole(AriaRole.Link, new() { Name = "Johan" }).ClickAsync();
        await page.WaitForURLAsync("http://localhost:5273/Johan");
        await page.GetByRole(AriaRole.Link, new() { Name = "public timeline" }).ClickAsync();
        await page.WaitForURLAsync("http://localhost:5273/");
        await page.GetByRole(AriaRole.Link, new() { Name = "my timeline" }).ClickAsync();
        await page.WaitForURLAsync("http://localhost:5273/Johan");
        await page.GetByRole(AriaRole.Link, new() { Name = "Account" }).ClickAsync();
        await page.WaitForURLAsync("http://localhost:5273/Identity/Account/Manage");
        await page.GetByPlaceholder("Please enter your phone number.").ClickAsync();
        await page.GetByPlaceholder("Please enter your phone number.").FillAsync("31690155");
        await page.GetByRole(AriaRole.Button, new() { Name = "Save" }).ClickAsync();
        await page.WaitForURLAsync("http://localhost:5273/Identity/Account/Manage");
        await page.GetByText("Your profile has been updated").ClickAsync();
        await page.GetByPlaceholder("Username").ClickAsync();
        await page.GetByPlaceholder("Username").FillAsync("JohanIngeholm");
        await page.GetByRole(AriaRole.Button, new() { Name = "Save" }).ClickAsync();
        await page.WaitForURLAsync("http://localhost:5273/Identity/Account/Manage");
        await page.GetByRole(AriaRole.Link, new() { Name = "Email" }).ClickAsync();
        await page.WaitForURLAsync("http://localhost:5273/Identity/Account/Manage/Email");
        await page.GetByPlaceholder("Please enter new email").ClickAsync();
        await page.GetByPlaceholder("Please enter new email").FillAsync("jing1@itu.dk");
        await page.GetByRole(AriaRole.Button, new() { Name = "Change email" }).ClickAsync();
        await page.WaitForURLAsync("http://localhost:5273/Identity/Account/Manage/Email");
        await page.GetByRole(AriaRole.Link, new() { Name = "Profile" }).ClickAsync();
        await page.WaitForURLAsync("http://localhost:5273/Identity/Account/Manage");
        await page.GetByRole(AriaRole.Link, new() { Name = "my timeline" }).ClickAsync();
        await page.WaitForURLAsync("http://localhost:5273/JohanIngeholm");
        await page.GetByRole(AriaRole.Link, new() { Name = "JohanIngeholm" }).ClickAsync();
        await page.WaitForURLAsync("http://localhost:5273/JohanIngeholm");
        await page.GetByRole(AriaRole.Link, new() { Name = "public timeline" }).ClickAsync();
        await page.WaitForURLAsync("http://localhost:5273/");
        await page.GetByRole(AriaRole.Button, new() { Name = "Logout" }).ClickAsync();
        await page.WaitForURLAsync("http://localhost:5273/Identity/Account/Logout");
    }
}