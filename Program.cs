

try
{
    using StreamReader reader = new("C:\\Users\\jacqu\\Chirp.CLI\\chirp_cli_db.csv");
    String text = reader.ReadToEnd();
    
    Console.WriteLine(text);
}
catch (IOException e)
{
    Console.WriteLine("This file could not be read:");
    //Console.WriteLine(e);
}