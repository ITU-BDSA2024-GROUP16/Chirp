    using System.Net.Http.Json;

    namespace Chirp.CLI
    {
        using SimpleDB;
        using System.CommandLine;
        using System;

        
        class Program
        {
            
            private static readonly HttpClient httpClient = new HttpClient { BaseAddress = new Uri("https://teststuff1234.azurewebsites.net") };

            static async Task<int> Main(string[] args)
            {
                var rootCommand = new RootCommand("This application performs 'read' and 'write' operations.");

                var readCommand = new Command("--read", "Reads data from the database");
                readCommand.SetHandler(() => ReadCheeps());

                var dataArgument = new Argument<string[]>("message", "The message to be written")
                {
                    Arity = ArgumentArity.OneOrMore // Allows multiple words
                };

                var writeCommand = new Command("--write", "Writes data to the database")
                {
                    dataArgument 
                };

                // Ensure the handler accepts a string array
                writeCommand.SetHandler((string[] data) => CreateCheepFromCommandLine(data), dataArgument);

                rootCommand.AddCommand(readCommand);
                rootCommand.AddCommand(writeCommand);

                return await rootCommand.InvokeAsync(args);
            }
            
            
            static async Task ReadCheeps()
            {
                try
                {
                    var cheeps = await httpClient.GetFromJsonAsync<List<Cheep>>("/cheeps");
                    UserInterface.printCheeps(cheeps);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error reading cheeps: {ex.Message}");
                }
            }

            
            
            static async Task CreateCheepFromCommandLine(string[] data)
            {
                var message = string.Join(" ", data);
                var cheep = new Cheep
                {
                    Author = Environment.MachineName,
                    Message = message,
                    Timestamp = DateTimeOffset.Now.ToUnixTimeSeconds()
                };
                
                try
                {
                    var response = await httpClient.PostAsJsonAsync("/cheep", cheep);
                    response.EnsureSuccessStatusCode(); // Throws if the status code is not successful

                    Console.WriteLine($"Data '{message}' has been written to the database.");
                }   
                catch (HttpRequestException ex)
                {
                    Console.WriteLine($"Error posting cheep: {ex.Message}");
                }
            }
            
            /*
            static void ReadCheeps(CSVDatabase<Cheep> db)
            {
                
                
                var cheeps = db.Read();
                UserInterface.printCheeps(cheeps); // Assuming UserInterface is defined elsewhere
            }
            */
            /*
            static void CreateCheepFromCommandLine(CSVDatabase<Cheep> db, string[] data)
            {
                // Combine the array into a single message
                var message = string.Join(" ", data);
                var cheep = new Cheep
                {
                    Author = Environment.MachineName,
                    Message = message, // Store the combined message
                    Timestamp = DateTimeOffset.Now.ToUnixTimeSeconds()
                };

                db.Store(cheep); // Store the Cheep object
                Console.WriteLine($"Data '{message}' has been written to the database.");
            }
            */
        }
    }