using System.Diagnostics;
using System.Drawing;
using System.Numerics;
using System.Text;

Console.WriteLine("Spritesheet Slicer by rob5300.");

//Get target file/folder from args, or ask user to enter it.
string target = null;
if(args.Length >= 1)
{
    target = args[0];
}
if(string.IsNullOrEmpty(target))
{
    Console.WriteLine("-= Enter file path or folder path:");
    target = Console.ReadLine().Replace("\"", "");
}

//Ask if global dimension should be used for all images or to ask per image.
SizeInput sharedInput = null;
if(args.Length >= 4)
{
    try
    {
        var enumNames = new List<string>(Enum.GetNames(typeof(SizeType)));
        SizeType type = (SizeType)enumNames.FindIndex(0, x => x.ToLower() == args[1]);
        int x = Convert.ToInt32(args[2]);
        int y = Convert.ToInt32(args[3]);
        sharedInput = new() { type = type, w = x, h = y };
    }
    catch
    {
        Console.WriteLine("Failed to parse args, should be in the format: 'filepath' 'grid / dimensions' 'x' 'y'");
        return;
    }    
}

if(sharedInput == null)
{
    Console.WriteLine("-= Use same output size for all images?");
    string answer = Console.ReadLine().ToLower();
    if (answer == "yes" || answer == "y")
    {
        sharedInput = GetSizeFromUser();
    }
}

if (string.IsNullOrEmpty(target) || target == "*")
{
    target = Directory.GetCurrentDirectory();
}
else if (!File.Exists(target) || Directory.Exists(target))
{
    target = Path.Combine(Directory.GetCurrentDirectory(), target);
}

if (File.Exists(target) || Directory.Exists(target))
{
    Stopwatch stopwatch = new Stopwatch();
    stopwatch.Start();

    //See if this is a file or folder
    string[] files;
    if(Directory.Exists(target))
    {
        files = Directory.GetFiles(target, "*.png");
    }
    else
    {
        files = new string[] { target };
    }

    Log.WriteLine($"-= Will process '{files.Length}' files.");
    UpdateFiles(files);

    stopwatch.Stop();
    Console.WriteLine($"Done! Took {stopwatch.Elapsed}.");
}

/// <summary>
/// Update a list of files
/// </summary>
void UpdateFiles(string[] fileList)
{
    StringBuilder logBuilder = new();

    foreach (string _file in fileList)
    {
        ProcessFile(_file, logBuilder);
        Log.WriteLine(logBuilder.ToString());
        logBuilder.Clear();
    }
}

/// <summary>
/// Update a single file, using the provided updaters. Write log data to given StringBuilder.
/// </summary>
void ProcessFile(string file, StringBuilder logBuilder)
{
    logBuilder.AppendLine($"\n-= Processing File: '{file}'");

    Bitmap image = new Bitmap(file);

    SpritesheetSlicer slicer = new(image, Path.GetFileNameWithoutExtension(file));
    slicer.logBuilder = logBuilder;

    SizeInput size = sharedInput;

    if (sharedInput == null)
    {
        //Ask user for size if needed
        Log.WriteLine(logBuilder.ToString());
        logBuilder.Clear();
        size = GetSizeFromUser();
    }

    var images = slicer.SliceUsingInput(size);

    string fileDir = Path.GetDirectoryName(file);
    string newPathDir = Path.Combine(fileDir, slicer.Name);

    if (!Directory.Exists(newPathDir))
    {
        Directory.CreateDirectory(newPathDir);
    }

    slicer.WriteImagesToDisk(newPathDir, images);
    slicer.WriteMKSForSprites(images.Count, newPathDir);

    logBuilder.AppendLine($"-= Updated '${file}'");
}

SizeInput GetSizeFromUser()
{
    SizeInput sizeinput = new();

    while (sizeinput.type == SizeType.Unknown)
    {
        Console.WriteLine("-= Choose Size Type:\n 1: Grid Size.\n 2: Dimensions.");
        string input = Console.ReadLine();
        try
        {
            int inputNum = Convert.ToInt32(input);
            if (inputNum > 0 && inputNum <= 2)
            {
                sizeinput.type = (SizeType)inputNum;
            }
            else
            {
                Console.WriteLine("Invalid type choice, input number.");
            }
        }
        catch
        {
            Console.WriteLine("Invalid type choice, input number.");
        }
    }

    bool sizeValid = false;
    while (!sizeValid)
    {
        Console.WriteLine("-= Input width and height: (e.g. 256x256)");
        string inputSize = Console.ReadLine();
        string[] inputSizes = inputSize.Split("x");
        if (inputSize.Length < 2)
        {
            inputSizes = inputSize.Split(" ");
        }

        try
        {
            sizeinput.w = Convert.ToInt32(inputSizes[0]);
            sizeinput.h = Convert.ToInt32(inputSizes[1]);
            sizeValid = true;
        }
        catch
        {
            Console.WriteLine("Could not parse size, input 2 integer numbers seperated by a 'x' or space.");
        }
    }

    return sizeinput;
}

public enum SizeType { Unknown, Grid, Dimensions };

public class SizeInput
{
    public SizeType type;
    public int w;
    public int h;
}