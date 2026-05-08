using Compressor.Models;

namespace Compressor.Services;

public interface IProcessor
{
    Task<bool> CompressFile(CompressFileParameters parameters);
}