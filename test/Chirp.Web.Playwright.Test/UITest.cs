using System.Runtime.CompilerServices;
using Microsoft.AspNetCore.Mvc.Testing;
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
        }
        
        private async Task InitializeBrowserAndCreateBrowserContextAsync()
        {
            var playwright = await Microsoft.Playwright.Playwright.CreateAsync();
            _browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
            {
                Headless = true, //Set to false if you want to see the browser
            });
            
            _context = await _browser.NewContextAsync(new BrowserNewContextOptions());
        }

        [Test]
        public async Task startupPage()
        {
            var _page = await _context!.NewPageAsync();
            await _page.GotoAsync(_serverAddress);
            
            Console.WriteLine(_serverAddress);
            await _page.GetByRole(AriaRole.Link, new () { NameString = "Register" }).ClickAsync();
            await Expect(_page).ToHaveURLAsync(new Regex("/Identity/Account/Register"));
        }
        
        [Test]
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

        [Test]
        public async Task UserCanLogin()
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
            await Expect(_page).ToHaveURLAsync(new Regex("/Identity/Account/Login"));
            
            //fill in email
            var emailField = _page.GetByPlaceholder("name@example.com");
            await emailField.ClickAsync();
            await emailField.FillAsync("ceel@itu.dk");
            await Expect(emailField).ToHaveValueAsync("ceel@itu.dk");
            
            //fill in password
            var passwordField = _page.GetByPlaceholder("password");
            await passwordField.ClickAsync();
            await passwordField.FillAsync("Cecilie1234!");
            await Expect(passwordField).ToHaveValueAsync("Cecilie1234!");
            
            //log in button
            await _page.GetByRole(AriaRole.Button, new() { NameString = "Log in" }).ClickAsync();
            await Expect(_page).ToHaveURLAsync(_serverAddress);
        }

        [Test]
        public async Task UserCanShareCheep()
        {
            var _page = await _context!.NewPageAsync();
            await _page.GotoAsync(_serverAddress);
            
            //need to register and login before because test user is not saved. A new database is created for each test. 

            var cheepTextField = _page.Locator("input[name=\"Text\"]");
            await cheepTextField.ClickAsync();
            await Expect(cheepTextField).ToBeFocusedAsync();
            
            await cheepTextField.FillAsync("Hello, my group is the best group!");
            await Expect(cheepTextField).ToHaveValueAsync("Hello, my group is the best group!");
            
            await _page.GetByRole(AriaRole.Button, new() { NameString = "Share" }).ClickAsync();
            
            //check if there is a cheep with that text on the page after share button has been clicked. 
            var cheep = _page.GetByText("Hello, my group is the best group!");
            await Expect(cheep).ToHaveValueAsync("Hello, my group is the best group!");
            
            await Expect(_page).ToHaveURLAsync(new Regex(_serverAddress));
        }
        
        [Test]
        public async Task UserCanGoToMyTimelineByClickingOnName()
        {
            var _page = await _context!.NewPageAsync();
            await _page.GotoAsync(_serverAddress);
            
            //need to register and login before because test user is not saved. A new database is created for each test. 
            
            await _page.GetByRole(AriaRole.Link, new() { NameString = "Johan" }).ClickAsync();
            //await _page.WaitForURLAsync(_serverAddress + $"/Johan");
            await Expect(_page).ToHaveURLAsync(new Regex(_serverAddress + $"/Johan"));
        }
        
        [Test]
        public async Task UserCanGoToMyTimelineByClickingOnMyTimeline()
        {
            var _page = await _context!.NewPageAsync();
            await _page.GotoAsync(_serverAddress);
            
            //need to register and login before because test user is not saved. A new database is created for each test. 
            
            await _page.GetByRole(AriaRole.Link, new() { NameString = "my timeline" }).ClickAsync();
            //await _page.WaitForURLAsync(_serverAddress + $"/Johan");
            await Expect(_page).ToHaveURLAsync(new Regex(_serverAddress + $"/Johan"));
        }
        
        [Test]
        public async Task UserCanGoToPublicTimeline()
        {
            var _page = await _context!.NewPageAsync();
            await _page.GotoAsync(_serverAddress);
            
            //need to register and login before because test user is not saved. A new database is created for each test. 
            
            await _page.GetByRole(AriaRole.Link, new() { NameString = "public timeline" }).ClickAsync();
            //await _page.WaitForURLAsync(_serverAddress);
            await Expect(_page).ToHaveURLAsync(new Regex(_serverAddress));
        }
        
        [Test]
        public async Task UserCanChangeAccountInformation()
        {
            var _page = await _context!.NewPageAsync();
            await _page.GotoAsync(_serverAddress);
            
            //need to register and login before because test user is not saved. A new database is created for each test. 
            
            //go to account
            await _page.GetByRole(AriaRole.Link, new() { NameString = "Account" }).ClickAsync();
            //await _page.WaitForURLAsync("http://localhost:5273/Identity/Account/Manage");
            await Expect(_page).ToHaveURLAsync(new Regex(_serverAddress + $"/Identity/Account/Manage"));
            
            //change username 
            var usernameField = _page.Locator("Username"); 
            await usernameField.ClickAsync();
            await usernameField.FillAsync("JohanIngeholm");
            await Expect(usernameField).ToHaveValueAsync("JohanIngeholm");
            
            //enter phonenumber
            var phonenumberField = _page.Locator("PhoneNumber");
            await phonenumberField.ClickAsync();
            await phonenumberField.FillAsync("31690155");
            await Expect(phonenumberField).ToHaveValueAsync("31690155");
            
            //save changes
            await _page.GetByRole(AriaRole.Button, new() { NameString = "Save" }).ClickAsync();
            //await _page.WaitForURLAsync(new Regex(_serverAddress + $"/Identity/Account/Manage"));
            await Expect(_page).ToHaveURLAsync(new Regex(_serverAddress + $"/Identity/Account/Manage"));
            await _page.GetByText("Your profile has been updated").ClickAsync();
            
            //text with changes has been saved is visible on screen to illustrate save button has been pressed.
            var textSavings = _page.GetByText("Your profile has been updated");
            await textSavings.ClickAsync();
            await Expect(textSavings).ToBeVisibleAsync();
        }
        
        [Test]
        public async Task UserCanChangeEmail()
        {
            var _page = await _context!.NewPageAsync();
            await _page.GotoAsync(_serverAddress);
            
            //need to register and login before because test user is not saved. A new database is created for each test. 
            
            //go to account
            await _page.GetByRole(AriaRole.Link, new() { NameString = "Account" }).ClickAsync();
            await Expect(_page).ToHaveURLAsync(new Regex(_serverAddress + $"/Identity/Account/Manage"));
            
            //go to email in account
            await _page.GetByRole(AriaRole.Link, new() { NameString = "Email" }).ClickAsync();
            //await _page.WaitForURLAsync("http://localhost:5273/Identity/Account/Manage/Email");
            await Expect(_page).ToHaveURLAsync(new Regex(_serverAddress + $"/Identity/Account/Manage/Email"));
            
            //enter new email
            var emailField = _page.GetByPlaceholder("Please enter new email");
            await emailField.ClickAsync();
            await emailField.FillAsync("jing1@itu.dk");
            await Expect(emailField).ToHaveValueAsync("jing1@itu.dk");
            
            //change email button
            await _page.GetByRole(AriaRole.Button, new() { NameString = "Change email" }).ClickAsync();
            //await _page.WaitForURLAsync("http://localhost:5273/Identity/Account/Manage/Email");
            //maybe change so a text is shown when changing email,
            //or check if its possible to test with expect that old email is being changed in field above. 
            await Expect(_page).ToHaveURLAsync(new Regex(_serverAddress + $"/Identity/Account/Manage/Email"));
        }

        [Test]
        public async Task UserCanLogOut()
        {
            var _page = await _context!.NewPageAsync();
            await _page.GotoAsync(_serverAddress);
            
            //need to register and login before because test user is not saved. A new database is created for each test. 

            await _page.GetByRole(AriaRole.Link, new() { NameString = "public timeline" }).ClickAsync();
            //await _page.WaitForURLAsync(_serverAddress);
            await Expect(_page).ToHaveURLAsync(_serverAddress);
            
            //user can log out
            await _page.GetByRole(AriaRole.Button, new() { NameString = "Logout" }).ClickAsync();
            //await _page.WaitForURLAsync("http://localhost:5273/Identity/Account/Logout");
            await Expect(_page).ToHaveURLAsync(new Regex(_serverAddress + $"/Identity/Account/Logout"));
        }
        
        //dispose browser and context after each test
        public void Dispose()
        {
            _context?.DisposeAsync().GetAwaiter().GetResult();
            _browser?.DisposeAsync().GetAwaiter().GetResult();
        }
        
        /*
        public async Task Main()
        {

            await Page.GotoAsync("http://localhost:5273/");
            
            //share cheep
            await page.Locator("input[name=\"Text\"]").ClickAsync();
            await page.Locator("input[name=\"Text\"]").FillAsync("Hello, my group is the best group!");
            await page.GetByRole(AriaRole.Button, new() { Name = "Share" }).ClickAsync();
            await page.WaitForURLAsync("http://localhost:5273/");
            
            //go to my timeline by clicking name
            await page.GetByRole(AriaRole.Link, new() { Name = "Johan" }).ClickAsync();
            await page.WaitForURLAsync("http://localhost:5273/Johan");
            
            //go to public timeline
            await page.GetByRole(AriaRole.Link, new() { Name = "public timeline" }).ClickAsync();
            await page.WaitForURLAsync("http://localhost:5273/");
            
            //go to my timeline by clicking my timeline
            await page.GetByRole(AriaRole.Link, new() { Name = "my timeline" }).ClickAsync();
            await page.WaitForURLAsync("http://localhost:5273/Johan");
            
            //go to account an enter phonenumber
            await page.GetByRole(AriaRole.Link, new() { Name = "Account" }).ClickAsync();
            await page.WaitForURLAsync("http://localhost:5273/Identity/Account/Manage");
            await page.GetByPlaceholder("Please enter your phone number.").ClickAsync();
            await page.GetByPlaceholder("Please enter your phone number.").FillAsync("31690155");
            await page.GetByRole(AriaRole.Button, new() { Name = "Save" }).ClickAsync();
            await page.WaitForURLAsync("http://localhost:5273/Identity/Account/Manage");
            await page.GetByText("Your profile has been updated").ClickAsync();
            
            //update username 
            await page.GetByPlaceholder("Username").ClickAsync();
            await page.GetByPlaceholder("Username").FillAsync("JohanIngeholm");
            await page.GetByRole(AriaRole.Button, new() { Name = "Save" }).ClickAsync();
            await page.WaitForURLAsync("http://localhost:5273/Identity/Account/Manage");
            
            //update email
            await page.GetByRole(AriaRole.Link, new() { Name = "Email" }).ClickAsync();
            await page.WaitForURLAsync("http://localhost:5273/Identity/Account/Manage/Email");
            await page.GetByPlaceholder("Please enter new email").ClickAsync();
            await page.GetByPlaceholder("Please enter new email").FillAsync("jing1@itu.dk");
            await page.GetByRole(AriaRole.Button, new() { Name = "Change email" }).ClickAsync();
            await page.WaitForURLAsync("http://localhost:5273/Identity/Account/Manage/Email");
            
            //click on profile 
            await page.GetByRole(AriaRole.Link, new() { Name = "Profile" }).ClickAsync();
            await page.WaitForURLAsync("http://localhost:5273/Identity/Account/Manage");
            
            //see my timeline with new username
            await page.GetByRole(AriaRole.Link, new() { Name = "my timeline" }).ClickAsync();
            await page.WaitForURLAsync("http://localhost:5273/JohanIngeholm");
            await page.GetByRole(AriaRole.Link, new() { Name = "JohanIngeholm" }).ClickAsync();
            await page.WaitForURLAsync("http://localhost:5273/JohanIngeholm");
            
            //go to public timeline
            await page.GetByRole(AriaRole.Link, new() { Name = "public timeline" }).ClickAsync();
            await page.WaitForURLAsync("http://localhost:5273/");
            
            //user can log out
            await page.GetByRole(AriaRole.Button, new() { Name = "Logout" }).ClickAsync();
            await page.WaitForURLAsync("http://localhost:5273/Identity/Account/Logout");
        }
        */
    }
}