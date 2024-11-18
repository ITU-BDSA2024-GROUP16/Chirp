using System.Runtime.CompilerServices;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.CodeAnalysis.FlowAnalysis.DataFlow.DisposeAnalysis;
using Xunit;

namespace Chirp.Web.Playwright.Test
{
    using Microsoft.Playwright;
    using NUnit.Framework;
    using System.Text.RegularExpressions;
    
    [TestFixture]
    class UITest : PageTest, IClassFixture<CustomTestWebApplicationFactory>, IDisposable
    {
        private IBrowserContext? _context;
        private IBrowser? _browser;
        private CustomTestWebApplicationFactory _factory;
        private string _serverAddress;
        private HttpClient _client;

        [SetUp]
        public async Task SetUp()
        {
            _factory = new CustomTestWebApplicationFactory();
            _serverAddress = _factory.ServerAddress;
            _client = _factory.CreateClient(new WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = true,
                HandleCookies = true,
            });
        
            await InitializeBrowserAndCreateBrowserContextAsync();
            
            var test = TestContext.CurrentContext.Test;

            // Check if the test is marked with the "SkipSetUp" category
            if (!test.Properties["Category"].Contains("SkipSetUp"))
            {
                await SetUpRegisterAndLogin();
            }
        }

        
        private async Task SetUpRegisterAndLogin()
        {
            var _page = await _context!.NewPageAsync();
            await _page.GotoAsync(_serverAddress);
            
            //first register user, because a new in memory database is created for each test. 
            await _page.GetByRole(AriaRole.Link, new () { NameString = "Register" }).ClickAsync();
            await _page.WaitForURLAsync(new Regex("/Identity/Account/Register"));
            await _page.GetByLabel("Username").ClickAsync();
            await _page.GetByLabel("Username").FillAsync("Cecilie");
            await _page.GetByLabel("Username").PressAsync("Tab");
            await _page.GetByPlaceholder("name@example.com").FillAsync("ceel@itu.dk");
            await _page.GetByRole(AriaRole.Textbox, new() { NameString = "Password" }).ClickAsync();
            await _page.GetByRole(AriaRole.Textbox, new() { NameString = "Password" }).FillAsync("Cecilie1234!");
            await _page.GetByRole(AriaRole.Textbox, new() { NameString = "Password" }).PressAsync("Tab");
            await _page.GetByLabel("Confirm Password").FillAsync("Cecilie1234!");
            await _page.GetByRole(AriaRole.Button, new() { NameString = "Register" }).ClickAsync();
            await _page.WaitForURLAsync(new Regex("/Identity/Account/RegisterConfirmation"));
            await _page.GetByRole(AriaRole.Link, new() { NameString = "Click here to confirm your account" }).ClickAsync();
            await _page.WaitForURLAsync(new Regex("/Identity/Account/ConfirmEmail"));
            
            //next login to account that has just been made by user
            await _page.GetByRole(AriaRole.Link, new() { NameString = "Login" }).ClickAsync();
            await _page.GetByPlaceholder("name@example.com").ClickAsync();
            await _page.GetByPlaceholder("name@example.com").FillAsync("ceel@itu.dk");
            await _page.GetByPlaceholder("password").ClickAsync();
            await _page.GetByPlaceholder("password").FillAsync("Cecilie1234!");
            await _page.GetByRole(AriaRole.Button, new() { NameString = "Log in" }).ClickAsync();
        }

        [TearDown]
        public async Task TearDown()
        {
            Dispose();
        }

        private async Task InitializeBrowserAndCreateBrowserContextAsync()
        {
            var playwright = await Microsoft.Playwright.Playwright.CreateAsync();
            _browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
            {
                Headless = false, //Set to false if you want to see the browser
            });
            
            _context = await _browser.NewContextAsync(new BrowserNewContextOptions());
        }
        
        [Test, Category("SkipSetUp")]
        public async Task UsersCanRegister()
        {
            var _page = await _context!.NewPageAsync();
            await _page.GotoAsync(_serverAddress);
            
            await _page.GetByRole(AriaRole.Link, new () { NameString = "Register" }).ClickAsync();
            await _page.WaitForURLAsync(new Regex("/Identity/Account/Register"));

            //Username
            var usernameInput = _page.GetByLabel("Username");
            await usernameInput.ClickAsync();
            await Expect(usernameInput).ToBeFocusedAsync();
            await usernameInput.FillAsync("Cecilie");
            await Expect(usernameInput).ToHaveValueAsync("Cecilie");
            await _page.GetByLabel("Username").PressAsync("Tab");
            
            //Email
            var emailInput = _page.GetByPlaceholder("name@example.com");
            await emailInput.FillAsync("ceel@itu.dk");
            await Expect(emailInput).ToHaveValueAsync("ceel@itu.dk");
            
            //password
            var passwordInput = _page.GetByRole(AriaRole.Textbox, new() { NameString = "Password" });
            await passwordInput.ClickAsync();
            await passwordInput.FillAsync("Johan1234!");
            await Expect(passwordInput).ToHaveValueAsync("Johan1234!");
            await passwordInput.PressAsync("Tab");
            await Expect(passwordInput).Not.ToBeFocusedAsync();

            var confirmPassword = _page.GetByLabel("Confirm Password");
            await confirmPassword.FillAsync("Johan1234!");
            await Expect(confirmPassword).ToHaveValueAsync("Johan1234!");
            
            //click on register button
            await _page.GetByRole(AriaRole.Button, new() { NameString = "Register" }).ClickAsync();
            await Expect(_page).ToHaveURLAsync(new Regex("/Identity/Account/RegisterConfirmation"));
            
            //click on confirm account link
            await _page.GetByRole(AriaRole.Link, new() { NameString = "Click here to confirm your account" }).ClickAsync();
            await Expect(_page).ToHaveURLAsync(new Regex("/Identity/Account/ConfirmEmail"));
        }

        [Test, Category("SkipSetUp")]
        public async Task UserCanRegisterAndLogin()
        {
            //go to base server address
            var _page = await _context!.NewPageAsync();
            await _page.GotoAsync(_serverAddress);
            
            //first register user, because a new in memory database is created for each test. 
            await _page.GetByRole(AriaRole.Link, new () { NameString = "Register" }).ClickAsync();
            await _page.WaitForURLAsync(new Regex("/Identity/Account/Register"));
            await _page.GetByLabel("Username").ClickAsync();
            await _page.GetByLabel("Username").FillAsync("Cecilie");
            await _page.GetByLabel("Username").PressAsync("Tab");
            await _page.GetByPlaceholder("name@example.com").FillAsync("ceel@itu.dk");
            await Task.Delay(2000);
            await _page.GetByRole(AriaRole.Textbox, new() { NameString = "Password" }).ClickAsync();
            await _page.GetByRole(AriaRole.Textbox, new() { NameString = "Password" }).FillAsync("Cecilie1234!");
            await _page.GetByRole(AriaRole.Textbox, new() { NameString = "Password" }).PressAsync("Tab");
            await _page.GetByLabel("Confirm Password").FillAsync("Cecilie1234!");
            await Task.Delay(2000);
            await _page.GetByRole(AriaRole.Button, new() { NameString = "Register" }).ClickAsync();
            await _page.WaitForURLAsync(new Regex("/Identity/Account/RegisterConfirmation"));
            await _page.GetByRole(AriaRole.Link, new() { NameString = "Click here to confirm your account" }).ClickAsync();
            await _page.WaitForURLAsync(new Regex("/Identity/Account/ConfirmEmail"));
            await Task.Delay(2000);
            
            //next login to account that has just been made by user
            await _page.GetByRole(AriaRole.Link, new() { NameString = "Login" }).ClickAsync();
            await Expect(_page).ToHaveURLAsync(new Regex("/Identity/Account/Login"));
            await Task.Delay(2000);
            
            //fill in email
            var emailField = _page.GetByPlaceholder("name@example.com");
            await emailField.ClickAsync();
            await emailField.FillAsync("ceel@itu.dk");
            await Expect(emailField).ToHaveValueAsync("ceel@itu.dk");
            await Task.Delay(2000);
            
            //fill in password
            var passwordField = _page.GetByPlaceholder("password");
            await passwordField.ClickAsync();
            await passwordField.FillAsync("Cecilie1234!");
            await Expect(passwordField).ToHaveValueAsync("Cecilie1234!");
            await Task.Delay(2000);
            
            //log in button
            await _page.GetByRole(AriaRole.Button, new() { NameString = "Log in" }).ClickAsync();
            await Task.Delay(2000);
        }

        [Test]
        public async Task UserCanShareCheep()
        {
            var _page = await _context!.NewPageAsync();
            await _page.GotoAsync(_serverAddress);
            
            //send cheep   
            var cheepTextField = _page.Locator("input[id='Text']");
            await cheepTextField.ClickAsync();
            await Expect(cheepTextField).ToBeFocusedAsync();
            
            await cheepTextField.FillAsync("Hello, my group is the best group");
            await Expect(cheepTextField).ToHaveValueAsync("Hello, my group is the best group");
            
            await _page.GetByRole(AriaRole.Button, new() { NameString = "Share" }).ClickAsync();
            
            //check if there is a cheep with that text on the page after share button has been clicked. 
            var cheep = _page.Locator("#messagelist p#cheep", new() { HasTextString = "Hello, my group is the best group" });
            await Expect(cheep).ToBeVisibleAsync();
            
            await Expect(_page).ToHaveURLAsync(new Regex(_serverAddress));
        }
        
        [Test]
        public async Task UserCanGoToMyTimelineByClickingOnMyTimeline()
        {
            var _page = await _context!.NewPageAsync();
            await _page.GotoAsync(_serverAddress);
            
            await _page.GetByRole(AriaRole.Link, new() { NameString = "my timeline" }).ClickAsync();
            await Expect(_page).ToHaveURLAsync(new Regex(_serverAddress + $"Cecilie"));
        }
        
        [Test]
        public async Task UserCanGoToPublicTimeline()
        {
            var _page = await _context!.NewPageAsync();
            await _page.GotoAsync(_serverAddress);
            
            await _page.GetByRole(AriaRole.Link, new() { NameString = "public timeline" }).ClickAsync();
            await Expect(_page).ToHaveURLAsync(new Regex(_serverAddress));
        }
        
        [Test]
        public async Task UserCanChangeAccountInformation()
        {
            var _page = await _context!.NewPageAsync();
            await _page.GotoAsync(_serverAddress);
            
            //go to account
            await _page.GetByRole(AriaRole.Link, new() { NameString = "Account" }).ClickAsync();
            //await _page.WaitForURLAsync("http://localhost:5273/Identity/Account/Manage");
            await Expect(_page).ToHaveURLAsync(new Regex(_serverAddress + $"Identity/Account/Manage"));
            
            //change username 
            var usernameField = _page.GetByPlaceholder("Username"); 
            await usernameField.ClickAsync();
            await usernameField.FillAsync("JohanIngeholm");
            await Expect(usernameField).ToHaveValueAsync("JohanIngeholm");
            
            //enter phonenumber
            var phonenumberField = _page.GetByPlaceholder("Please enter your phone number.");
            await phonenumberField.ClickAsync();
            await phonenumberField.FillAsync("31690155");
            await Expect(phonenumberField).ToHaveValueAsync("31690155");
            
            //save changes
            await _page.GetByRole(AriaRole.Button, new() { NameString = "Save" }).ClickAsync();
            await Expect(_page).ToHaveURLAsync(new Regex(_serverAddress + $"Identity/Account/Manage"));
            
            //text with changes has been saved is visible on screen to illustrate save button has been pressed.
            var textSavings = _page.GetByText("Your profile has been updated");
            await textSavings.ClickAsync();
            await Expect(_page.Locator("text=Your profile has been updated")).ToBeVisibleAsync();
        }
        
        [Test]
        public async Task UserCanChangeEmail()
        {
            var _page = await _context!.NewPageAsync();
            await _page.GotoAsync(_serverAddress);
            
            //go to account
            await _page.GetByRole(AriaRole.Link, new() { NameString = "Account" }).ClickAsync();
            await Expect(_page).ToHaveURLAsync(new Regex(_serverAddress + $"Identity/Account/Manage"));
            await Task.Delay(1000);

            
            //go to email in account
            await _page.GetByRole(AriaRole.Link, new() { NameString = "Email" }).ClickAsync();
            await Expect(_page).ToHaveURLAsync(new Regex(_serverAddress + $"Identity/Account/Manage/Email"));
            await Task.Delay(1000);

            
            //enter new email
            var emailField = _page.GetByPlaceholder("Please enter new email");
            await emailField.ClickAsync();
            await emailField.FillAsync("jing@itu.dk");
            await Expect(emailField).ToHaveValueAsync("jing@itu.dk");
            await Task.Delay(1000);

            
            //change email button
            await _page.GetByRole(AriaRole.Button, new() { NameString = "Change email" }).ClickAsync();
            await Task.Delay(1000);
            
            await _page.GetByRole(AriaRole.Link, new() { NameString = "Account" }).ClickAsync();
            await Expect(_page).ToHaveURLAsync(new Regex(_serverAddress + $"Identity/Account/Manage"));
            
            var emailFieldInAccount = _page.GetByPlaceholder("Email");
            await Expect(emailFieldInAccount).ToHaveValueAsync("jing@itu.dk");
            await Task.Delay(1000);
            
            await Expect(_page).ToHaveURLAsync(new Regex(_serverAddress + $"Identity/Account/Manage"));
        }

        [Test]
        public async Task UserCanLogOut()
        {
            var _page = await _context!.NewPageAsync();
            await _page.GotoAsync(_serverAddress);
            
            await _page.GetByRole(AriaRole.Link, new() { NameString = "public timeline" }).ClickAsync();
            await Expect(_page).ToHaveURLAsync(_serverAddress);
            await Task.Delay(4000);
            
            //user can log out
            await _page.GetByRole(AriaRole.Button, new() { NameString = "Logout" }).ClickAsync();
            await Task.Delay(4000);
            await Expect(_page).ToHaveURLAsync(new Regex(_serverAddress + $"Identity/Account/Logout"));
            await Task.Delay(2000);
        }
        
        
        //dispose browser and context after each test
        public void Dispose()
        {
           _context?.DisposeAsync().GetAwaiter().GetResult();
           _browser?.DisposeAsync().GetAwaiter().GetResult();
           _factory?.DisposeAsync().GetAwaiter().GetResult();
           _serverAddress = null;
           _client = null;
            
        
        }
    }
}