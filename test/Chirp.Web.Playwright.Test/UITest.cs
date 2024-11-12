namespace Chirp.Web.Playwright.Test
{
    using Microsoft.Playwright;
    using NUnit.Framework;
    using System.Text.RegularExpressions;
    
    [TestFixture]
    class UITest : PageTest
    {
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
            await usernameInput.FillAsync("Johan");
            await Expect(usernameInput).ToHaveValueAsync("Johan");
            await Page.GetByLabel("Username").PressAsync("Tab");
            
            //Email
            var emailInput = Page.GetByPlaceholder("name@example.com");
            await emailInput.FillAsync("jing@itu.dk");
            await Expect(emailInput).ToHaveValueAsync("jing@itu.dk");
            
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
            var registerButton = Page.GetByRole(AriaRole.Button, new () { NameString = "Register" });
            await registerButton.ClickAsync();
            await Page.WaitForURLAsync("http://localhost:5273/Identity/Account/RegisterConfirmation?email=jing@itu.dk&returnUrl=%2F");

            await Expect(Page).ToHaveURLAsync("http://localhost:5273/Identity/Account/RegisterConfirmation?email=jing@itu.dk&returnUrl=%2F");
            
            //confirm account 
            var confirmAccount =
                Page.GetByRole(AriaRole.Link, new() { NameString = "Click here to confirm your account" });
            await confirmAccount.ClickAsync();
            await Page.WaitForURLAsync(
                "http://localhost:5273/Identity/Account/ConfirmEmail?userId=c86a18e6-9f6e-4a31-a30d-1d849ece7432&code=Q2ZESjhCSmt5Syt3OU1WTWgrQi9Zb0djazVoKy9GckNpM25PRWt2U0IwWTdadlFtUXBIZU9jNFZ0dUZ5VzFDdlYrQkM3RndxclkrMjFhNFdVUndCV3ROVkRSblYzSUgrOCs1ZXBOQ3RSWE1nTEUzc0c2Z1EzMVJQVzBqZmg4TGllTm54MERTa0pnOXhmbzVORmZuVkVydDBSYkFaQUtJL3pCZTlzQTJUYktMdFpQeVdIbWlzdjBIT0pMaVcwaHRkZnlmb2twWDhYK0tuZmY3S25DOGF2bHc0T0lnaXRFQUMzTzdrVk94OG9MUmp5bmh1RDB4VEIrS1NPbVJuamZYUUtYN1JTZz09&returnUrl=%2F");
            
            await Expect(Page).ToHaveURLAsync(
                "http://localhost:5273/Identity/Account/ConfirmEmail?userId=fc081e91-8332-4619-847d-00c2b8f67d8b&code=Q2ZESjhCSmt5Syt3OU1WTWgrQi9Zb0djazVoOXhPZVJOdSswclcwSWdBSEpjcjlDVk9vL3hmWTY1RzNTcXFJUG5hREFZdzZLdHZKQVZ5SEsvK0FxT1Z1U2RUcU9KdFdGbkovQlNiOTd5RHRyZFZpVjh0dVlOWEMrV1I3NHRBdkJVRVZUczVVd00zNnJyTkxvMmhxc09XczdMU0ZBQ1dvYVZjVkZKdXd6b2hRTDJMK0dIQWVOVytORkVmb1pFay83RElvUDF5eEtFUlZtZTRMOU96cjFSSGJtUWxSOWpENXVPUUd5alZaMFE1d2t2NHY0V3V0ZlMyVHpybWJGUE12a1JwMk8yZz09&returnUrl=%2F");
        }

        /*
        public async Task Main()
        {

            await Page.GotoAsync("http://localhost:5273/");
            await Page.GetByRole(AriaRole.Link, new () { NameString = "Register" }).ClickAsync();
            await Page.WaitForURLAsync("http://localhost:5273/Identity/Account/Register");

            await Page.GetByLabel("Username").ClickAsync();
            await Page.GetByLabel("Username").FillAsync("Johan");
            await Page.GetByLabel("Username").PressAsync("Tab");
            await Page.GetByPlaceholder("name@example.com").FillAsync("jing@itu.dk");
            await Page.GetByRole(AriaRole.Textbox, new() { Name = "Password" }).ClickAsync();
            await Page.GetByRole(AriaRole.Textbox, new() { Name = "Password" }).FillAsync("Johan1234!");
            await Page.GetByRole(AriaRole.Textbox, new() { Name = "Password" }).PressAsync("Tab");
            await Page.GetByLabel("Confirm Password").FillAsync("Johan1234!");
            await Page.GetByRole(AriaRole.Button, new() { Name = "Register" }).ClickAsync();
            await Page.WaitForURLAsync(
                "http://localhost:5273/Identity/Account/RegisterConfirmation?email=jing@itu.dk&returnUrl=%2F");
            await Page.GetByRole(AriaRole.Link, new() { Name = "Click here to confirm your account" }).ClickAsync();
            await Page.WaitForURLAsync(
                "http://localhost:5273/Identity/Account/ConfirmEmail?userId=c86a18e6-9f6e-4a31-a30d-1d849ece7432&code=Q2ZESjhCSmt5Syt3OU1WTWgrQi9Zb0djazVoKy9GckNpM25PRWt2U0IwWTdadlFtUXBIZU9jNFZ0dUZ5VzFDdlYrQkM3RndxclkrMjFhNFdVUndCV3ROVkRSblYzSUgrOCs1ZXBOQ3RSWE1nTEUzc0c2Z1EzMVJQVzBqZmg4TGllTm54MERTa0pnOXhmbzVORmZuVkVydDBSYkFaQUtJL3pCZTlzQTJUYktMdFpQeVdIbWlzdjBIT0pMaVcwaHRkZnlmb2twWDhYK0tuZmY3S25DOGF2bHc0T0lnaXRFQUMzTzdrVk94OG9MUmp5bmh1RDB4VEIrS1NPbVJuamZYUUtYN1JTZz09&returnUrl=%2F");
            await Page.GetByRole(AriaRole.Link, new() { Name = "Login" }).ClickAsync();
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
        */
    }
}