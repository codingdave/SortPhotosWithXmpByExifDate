using System.CommandLine;
using System.CommandLine.Parsing;

namespace SortPhotosWithXmpByExifDateCli;

internal static class OptionsHelper
{
    internal static Option<bool> GetForceOption()
    {
        return new Option<bool>(
            name: "--force",
            description: "Allow possibly destructive operations.",
            getDefaultValue: () => false
        );
    }

    internal static Option<bool> GetMoveOption()
    {
        return new Option<bool>(
            name: "--move",
            description: "Operation on files, move if true. Defaults to copy.",
            getDefaultValue: () => false
        );
    }

    internal static Option<int> GetSimilarityOption()
    {
        return new Option<int>
        (name: "--similarity",
        description: "Number to indicate similarity to detect an image as a duplicate of anothe. Defaults to 100%.",
        getDefaultValue: () => 100);
    }

    internal static Option<object?> GetOffsetOption()
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
                TimeSpan? ret = null;
                var offset = result.Tokens.SingleOrDefault()?.Value;
                if (offset == null)
                {
                    result.ErrorMessage = "No argument given";
                }
                else if (TimeSpan.TryParse(offset, out var parsed))
                {
                    ret = parsed;
                }
                else
                {
                    result.ErrorMessage = $"cannot parse TimeSpan '{offset}'";
                }

                return ret;
            }
        );
    }

    internal static Option<string?> GetDestinationOption()
    {
        return new Option<string?>(
            name: "--destination",
            description: "The destination directory that contains the data.",
            isDefault: true,
            parseArgument: result =>
            {
                string? ret = null;
                var filePath = result.Tokens.SingleOrDefault()?.Value;
                if (filePath == null)
                {
                    result.ErrorMessage = "No argument given";
                }
                else
                {
                    filePath = Path.GetFullPath(Helpers.FixPath(filePath));

                    if (!Directory.Exists(filePath))
                    {
                        Directory.CreateDirectory(filePath);
                    }

                    ret = filePath;
                }

                return ret;
            }
        );
    }

    internal static Option<string?> GetSourceOption()
    {
        return new Option<string?>(
            name: "--source",
            description: "The source directory that contains the data.",
            isDefault: true,
            parseArgument: result =>
            {
                string? ret = null;
                var filePath = result.Tokens.SingleOrDefault()?.Value;
                if (filePath == null)
                {
                    result.ErrorMessage = "No argument given";
                }
                else
                {
                    filePath = Path.GetFullPath(Helpers.FixPath(filePath));

                    if (!Directory.Exists(filePath))
                    {
                        result.ErrorMessage = "Source directory does not exist";
                    }
                    else
                    {
                        ret = filePath;
                    }
                }

                return ret;
            }
        );
    }
}
