using System.CommandLine;

namespace SortPhotosWithXmpByExifDateCli;

internal class CommandLine
{
    private static IEnumerable<string> _extensions = new List<string>()
    {
        ".jpg",
        ".nef",
        ".gif",
        ".mp4",
        ".png",
        ".cr3"
    };

    private static Option<object?> GetOffsetOption()
    {
        // To workaround the following issue we return an object instead of a struct 
        // "resource": "/home/david/projects/SortPhotosWithXmpByExifDate/SortPhotosWithXmpByExifDateCli/CommandLine.cs",
        // "message": "Argument 4: cannot convert from 'System.CommandLine.Option<System.TimeSpan?>' to 'System.CommandLine.Binding.IValueDescriptor<System.TimeSpan>' [SortPhotosWithXmpByExifDateCli]",
        // "startLineNumber": 148,
        return new Option<object?>(
            name: "--offset",
            description: "The offset that should be added to the images.",
            isDefault: true,
            parseArgument: result =>
            {
                var offset = result.Tokens.SingleOrDefault()?.Value;
                if (offset == null)
                {
                    result.ErrorMessage = "No argument given";
                    return null;
                }

                if (!TimeSpan.TryParse(offset, out var ret))
                {
                    result.ErrorMessage = $"cannot parse TimeSpan '{offset}'";
                    return null;
                }

                return ret;
            }
        );
    }
    
    private static Option<DirectoryInfo?> GetDestinationOption()
    {
        return new Option<DirectoryInfo?>(
            name: "--destination",
            description: "The destination directory that contains the data.",
            isDefault: true,
            parseArgument: result =>
            {
                var filePath = result.Tokens.SingleOrDefault()?.Value;
                if (filePath == null)
                {
                    result.ErrorMessage = "No argument given";
                    return null;
                }

                filePath = Helpers.FixPath(filePath);

                if (!Directory.Exists(filePath))
                {
                    Directory.CreateDirectory(filePath);
                }

                return new DirectoryInfo(filePath);
            }
        );
    }

    private static Option<DirectoryInfo?> GetSourceOption()
    {
        return new Option<DirectoryInfo?>(
            name: "--source",
            description: "The source directory that contains the data.",
            isDefault: true,
            parseArgument: result =>
            {
                var filePath = result.Tokens.SingleOrDefault()?.Value;
                if (filePath == null)
                {
                    result.ErrorMessage = "No argument given";
                    return null;
                }

                filePath = Helpers.FixPath(filePath);

                if (!Directory.Exists(filePath))
                {
                    result.ErrorMessage = "Source directory does not exist";
                    return null;
                }
                else
                {
                    return new DirectoryInfo(filePath);
                }
            }
        );
    }
    
    public static async Task<int> InitCommandLine(string[] args)
    {
        Statistics statistics = new();
        
        var sourceOption = GetSourceOption();
        var destinationOption = GetDestinationOption();
        var offsetOption = GetOffsetOption();

        var rearrangeByExifCommand = new Command("rearrangeByExif",
            "Scan source dir and move photos and videos to destination directory in subdirectories given by the Exif information. Xmp files are placed accordingly.")
        {
            sourceOption,
            destinationOption
        };
        rearrangeByExifCommand.SetHandler(SortImagesByExif!, sourceOption, destinationOption);

        var checkIfFileNameContainsDateDifferentToExifDatesCommand = new Command(
            "checkIfFileNameContainsDateDifferentToExifDates",
            "check if image timestamp differs from exif and rename file")
        {
            sourceOption
        };
        checkIfFileNameContainsDateDifferentToExifDatesCommand.SetHandler(
            CheckIfFileNameContainsDateDifferentToExifDates!, sourceOption);

        var rearrangeByCameraManufacturerCommand = new Command("rearrangeByCameraManufacturer",
            "Find all images of certain camera. Sort into camera subdirectories. Keep layout but prepend camera manufacturer.")
        {
            sourceOption,
            destinationOption
        };
        rearrangeByCameraManufacturerCommand.SetHandler(SortImagesByManufacturer!, sourceOption, destinationOption);

        var rearrangeBySoftwareCommand = new Command("rearrangeBySoftware",
            "Find all F-Spot images. They might be wrong. Compare them. Keep layout but prepend software that was creating images.")
        {
            sourceOption,
            destinationOption
        };
        rearrangeBySoftwareCommand.SetHandler(RearrangeBySoftware!, sourceOption, destinationOption);

        var fixExifDateByOffsetCommand = new Command(
            "fixExifDateByOffset",
            "Fix their exif by identifying the offset.")
        {
            sourceOption,
            offsetOption,
        };
        fixExifDateByOffsetCommand.SetHandler<DirectoryInfo, object>(FixExifDateByOffset!, sourceOption!, offsetOption!);

        var rootCommand = new RootCommand("Rearrange files containing Exif data")
        {
            TreatUnmatchedTokensAsErrors = true
        };
        rootCommand.AddCommand(rearrangeByExifCommand);
        rootCommand.AddCommand(checkIfFileNameContainsDateDifferentToExifDatesCommand);
        rootCommand.AddCommand(rearrangeByCameraManufacturerCommand);
        rootCommand.AddCommand(rearrangeBySoftwareCommand);
        rootCommand.AddCommand(fixExifDateByOffsetCommand);

        return await rootCommand.InvokeAsync(args);
    }

    private static void FixExifDateByOffset(DirectoryInfo directory, object offset)
    {
        Run(new FixExifDateByOffset(directory, (TimeSpan) offset));
    }

    private static void SortImagesByExif(DirectoryInfo sourcePath, DirectoryInfo destinationPath)
    {
        Run(new SortImageByExif(sourcePath, destinationPath, _extensions));
    }

    private static void RearrangeBySoftware(DirectoryInfo source, DirectoryInfo destination)
    {
        Run(new RearrangeBySoftware(source, destination));
    }

    private static void SortImagesByManufacturer(DirectoryInfo source, DirectoryInfo destination)
    { 
        Run(new SortImagesByManufacturer(source, destination));
    }

    private static void CheckIfFileNameContainsDateDifferentToExifDates(DirectoryInfo source)
    {  
        Run(new CheckIfFileNameContainsDateDifferentToExifDates(source));
    }
    
    private static void Run(IRun f)
    {
        try
        {
            Console.WriteLine(f.Run());
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        };
    }
}