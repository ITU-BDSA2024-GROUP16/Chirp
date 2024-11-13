namespace Chirp.Web.Playwright.Test
{
    using Microsoft.Playwright;
    using NUnit.Framework;
    using System.Text.RegularExpressions;
    
    [TestFixture]
    class UITest : PageTest
    {
        private string _uniqueUsername = $"Johan{Guid.NewGuid()}";
        private string _uniqueEmail = $"{Guid.NewGuid()}@itu.dk";
        
        [Test]
        public async Task startupPage()
        {
            await Page.GotoAsync("http://localhost:5273/");
            await Page.GetByRole(AriaRole.Link, new () { NameString = "Register" }).ClickAsync();
            await Page.WaitForURLAsync("http://localhost:5273/Identity/Account/Register");
            await Expect(Page).ToHaveURLAsync("http://localhost:5273/Identity/Account/Register");
        }
        
        [Test]
        public async Task UsersCanRegister()
        {
            await Page.GotoAsync("http://localhost:5273/");
            await Page.GetByRole(AriaRole.Link, new () { NameString = "Register" }).ClickAsync();
            await Page.WaitForURLAsync("http://localhost:5273/Identity/Account/Register");

            //Username
            var usernameInput = Page.GetByLabel("Username");
            await usernameInput.ClickAsync();
            await Expect(usernameInput).ToBeFocusedAsync();
            await usernameInput.FillAsync("Johan2");
            await Expect(usernameInput).ToHaveValueAsync("Johan2");
            await Page.GetByLabel("Username").PressAsync("Tab");
            
            //Email
            var emailInput = Page.GetByPlaceholder("name@example.com");
            await emailInput.FillAsync("jing2@itu.dk");
            await Expect(emailInput).ToHaveValueAsync("jing2@itu.dk");
            
            //password
            var passwordInput = Page.GetByRole(AriaRole.Textbox, new() { NameString = "Password" });
            await passwordInput.ClickAsync();
            await passwordInput.FillAsync("Johan1234!");
            await Expect(passwordInput).ToHaveValueAsync("Johan1234!");
            await passwordInput.PressAsync("Tab");
            await Expect(passwordInput).Not.ToBeFocusedAsync();

            var confirmPassword = Page.GetByLabel("Confirm Password");
            await confirmPassword.FillAsync("Johan1234!");
            await Expect(confirmPassword).ToHaveValueAsync("Johan1234!");
            
            //click on register button
            await Page.GetByRole(AriaRole.Button, new() { NameString = "Register" }).ClickAsync();
            await Expect(Page).ToHaveURLAsync(new Regex("http://localhost:5273/Identity/Account/RegisterConfirmation"));
            
            //click on confirm account link
            await Page.GetByRole(AriaRole.Link, new() { NameString = "Click here to confirm your account" }).ClickAsync();
            await Expect(Page).ToHaveURLAsync(new Regex("http://localhost:5273/Identity/Account/ConfirmEmail"));
        }

        [Test]
        public async Task UserCanLogin()
        {
            await Page.GotoAsync("http://localhost:5273/");
            await Page.GetByRole(AriaRole.Link, new() { NameString = "Login" }).ClickAsync();
            await Page.WaitForURLAsync("http://localhost:5273/Identity/Account/Login");
            await Expect(Page).ToHaveURLAsync(new Regex("http://localhost:5273/Identity/Account/Login"));
            
            //fill in email
            var emailField = Page.GetByPlaceholder("name@example.com");
            await emailField.ClickAsync();
            await emailField.FillAsync(_uniqueEmail);
            await Expect(emailField).ToHaveValueAsync(_uniqueEmail);
            
            //fill in password
            var passwordField = Page.GetByPlaceholder("password");
            await passwordField.ClickAsync();
            await passwordField.FillAsync("Johan1234!");
            await Expect(passwordField).ToHaveValueAsync("Johan1234!");
            
            //log in button
            await Page.GetByRole(AriaRole.Button, new() { NameString = "Log in" }).ClickAsync();
            await Expect(Page).ToHaveURLAsync(new Regex("http://localhost:5273/"));
        }

        [Test]
        public async Task UserCanShareCheep()
        {
            await Page.GotoAsync("http://localhost:5273/");

            var cheepTextField = Page.Locator("input[name=\"Text\"]");
            await cheepTextField.ClickAsync();
            await Expect(cheepTextField).ToBeFocusedAsync();
            
            await cheepTextField.FillAsync("Hello, my group is the best group!");
            await Expect(cheepTextField).ToHaveValueAsync("Hello, my group is the best group!");
            
            await Page.GetByRole(AriaRole.Button, new() { NameString = "Share" }).ClickAsync();
            
            await Page.WaitForURLAsync("http://localhost:5273/");
            
        }
        
        [Test]
        public async Task UserCanGoToMyTimelineByClickingOnName()
        {
            await Page.GotoAsync("http://localhost:5273/");
            
            await Page.GetByRole(AriaRole.Link, new() { NameString = "Johan" }).ClickAsync();
            await Page.WaitForURLAsync("http://localhost:5273/Johan");
        }
        
        public async Task UserCanGoToMyTimelineByClickingOnMyTimeline()
        {
            await Page.GotoAsync("http://localhost:5273/");
            
            await Page.GetByRole(AriaRole.Link, new() { NameString = "my timeline" }).ClickAsync();
            await Page.WaitForURLAsync("http://localhost:5273/Johan");
        }
        
        public async Task UserCanGoToPublicTimeline()
        {
            await Page.GotoAsync("http://localhost:5273/");
            
            await Page.GetByRole(AriaRole.Link, new() { NameString = "public timeline" }).ClickAsync();
            await Page.WaitForURLAsync("http://localhost:5273/");
        }
        
        public async Task UserCanChangeAccountInformation()
        {
            await Page.GotoAsync("http://localhost:5273/");
            
            //go to account
            await Page.GetByRole(AriaRole.Link, new() { NameString = "Account" }).ClickAsync();
            await Page.WaitForURLAsync("http://localhost:5273/Identity/Account/Manage");
            
            //change username 
            await Page.GetByPlaceholder("Username").ClickAsync();
            await Page.GetByPlaceholder("Username").FillAsync("JohanIngeholm");
            await Page.GetByRole(AriaRole.Button, new() { NameString = "Save" }).ClickAsync();
            await Page.WaitForURLAsync("http://localhost:5273/Identity/Account/Manage");
            
            //enter phonenumber
            await Page.GetByPlaceholder("Please enter your phone number.").ClickAsync();
            await Page.GetByPlaceholder("Please enter your phone number.").FillAsync("31690155");
            
            //save changes
            await Page.GetByRole(AriaRole.Button, new() { NameString = "Save" }).ClickAsync();
            await Page.WaitForURLAsync("http://localhost:5273/Identity/Account/Manage");
            await Page.GetByText("Your profile has been updated").ClickAsync();
        }
        
        public async Task UserCanChangeEmail()
        {
            await Page.GotoAsync("http://localhost:5273/");
            
            //go to account
            await Page.GetByRole(AriaRole.Link, new() { NameString = "Account" }).ClickAsync();
            await Page.WaitForURLAsync("http://localhost:5273/Identity/Account/Manage");
            
            //go to email in account
            await Page.GetByRole(AriaRole.Link, new() { NameString = "Email" }).ClickAsync();
            await Page.WaitForURLAsync("http://localhost:5273/Identity/Account/Manage/Email");
            
            //enter new email
            await Page.GetByPlaceholder("Please enter new email").ClickAsync();
            await Page.GetByPlaceholder("Please enter new email").FillAsync("jing1@itu.dk");
            await Page.GetByRole(AriaRole.Button, new() { NameString = "Change email" }).ClickAsync();
            await Page.WaitForURLAsync("http://localhost:5273/Identity/Account/Manage/Email");
            
        }

        public async Task UserCanLogOut()
        {
            await Page.GotoAsync("http://localhost:5273/");
            
            await Page.GetByRole(AriaRole.Link, new() { NameString = "public timeline" }).ClickAsync();
            await Page.WaitForURLAsync("http://localhost:5273/");
            
            //user can log out
            await Page.GetByRole(AriaRole.Button, new() { NameString = "Logout" }).ClickAsync();
            await Page.WaitForURLAsync("http://localhost:5273/Identity/Account/Logout");
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