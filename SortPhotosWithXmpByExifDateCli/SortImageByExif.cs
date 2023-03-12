using MetadataExtractor;

namespace SortPhotosWithXmpByExifDateCli;

internal class SortImageByExif : IRun
{
    private readonly DirectoryInfo _destinationDirectory;
    private readonly DirectoryInfo _sourceDirectory;
    private readonly IEnumerable<string> _extensions;
    private readonly ImagesAndXmpFoundStatistics _statistics;
    private bool _force;

    internal SortImageByExif(DirectoryInfo sourceDirectoryInfo, DirectoryInfo destinationDirectoryInfo, IEnumerable<string> extensions, bool force)
    {
        _sourceDirectory = sourceDirectoryInfo ?? throw new ArgumentNullException(nameof(sourceDirectoryInfo));
        _destinationDirectory = destinationDirectoryInfo ?? throw new ArgumentNullException(nameof(destinationDirectoryInfo));
        _extensions = extensions;
        _force = force;
        _statistics = new ImagesAndXmpFoundStatistics(force);
    }

    private IEnumerable<FileInfo> GetFileInfos() => 
        _sourceDirectory.EnumerateFiles("*.*", SearchOption.AllDirectories)
            .Where(f => _extensions.Any(e => f.Name.EndsWith(e, StringComparison.OrdinalIgnoreCase)));

    public IStatistics Run()
    {
        Console.WriteLine($"Starting {nameof(SortPhotosWithXmpByExifDateCli)}.{nameof(Run)} with search path: '{_sourceDirectory}' and destination path '{_destinationDirectory}'");

        foreach (var fileInfo in GetFileInfos())
        {               
            // Console.WriteLine($"Found photo {fileInfo}");
            var directories = ImageMetadataReader.ReadMetadata(fileInfo.FullName);
            try
            {
                Helpers.CheckForErrors(directories, fileInfo);
                var dateTime = Helpers.GetDateTimeFromImage(directories, fileInfo);
                var xmpFiles = Helpers.GetCorrespondingXmpFiles(fileInfo);
     
                Helpers.MoveImageAndXmpToExifPath(fileInfo, xmpFiles, dateTime, _destinationDirectory, _statistics, _force);
            }
            catch (Exception e)
            {
                Console.Error.WriteLine($"{fileInfo}: {e.Message}, {e}");
                Helpers.PrintMetadata(directories);
            }
        }

        var statistics = new DirectoriesDeletedStatistics(_force);
        Helpers.RecursivelyDeleteEmptyDirectories(_sourceDirectory, statistics, _force);
        return new ImagesAndXmpCopiedDirectoriesDeletedStatistics(_statistics, statistics);
    }
}
