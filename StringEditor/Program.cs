using UAssetEditor;

Console.Write("Enter the file path for the uasset > ");
var file = Console.ReadLine().Replace("\"", string.Empty);
var uasset = new UAsset(file);
uasset.ReadHeader();
var end = uasset.ReadBytes((int)(uasset.BaseStream.Length - uasset.Position));

while (true)
{
    var names = uasset.NameMap.Strings;
    Console.Clear();

    for (int i = 0; i < names.Count; i++)
        Console.WriteLine($"{i + 1}) '{names.ElementAt(i)}'");

    Console.WriteLine("\nEnter Q to save and exit.");
    Console.Write("Enter the number of the string you want to edit > ");
    var numberStr = Console.ReadLine();

    if (numberStr?.ToLower() == "q")
    {
        var writer = new Writer(File.OpenWrite(Path.GetFileNameWithoutExtension(file) + " Edited.uasset"));
        uasset.WriteHeader(writer);
        writer.WriteBytes(end);
        writer.Close();
        break;
    }

    var number = Convert.ToInt32(numberStr);
    Console.Clear();
    
    var old = names[number - 1];

    Console.WriteLine($"Replacing string '{old}'\n");
    Console.Write("Enter a new string > ");

    var str = Console.ReadLine() ?? "None";
    names[number - 1] = str;

    Console.WriteLine($"\nReplaced '{old}' with '{str}'.");
    Console.WriteLine("\nPress any key to return to strings...");
    Console.ReadKey();
}