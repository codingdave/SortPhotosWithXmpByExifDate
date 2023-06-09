using System.Diagnostics;
using Microsoft.Extensions.Logging;

namespace SortPhotosWithXmpByExifDateCli.ErrorCollection;

public class ErrorCollection : IErrorCollection, IReadOnlyErrorCollection
{
    private readonly ILogger _logger;

    public ErrorCollection(ILogger logger)
    {
        _logger = logger;
    }

    public IReadOnlyList<IError> Errors => _errors;
    private readonly List<IError> _errors = new();

    public void Add(IError error)
    {
        var existingError = _errors.FirstOrDefault(e => string.Equals(e.File, error.File));
        if (existingError == null)
        {
            _errors.Add(error);
        }
        else
        {
            existingError.AddMessage(error.ErrorMessage);
        }
    }
}