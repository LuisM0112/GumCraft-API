namespace GumCraft_API.Services;

public class FileService
{
    public static async Task<string> SaveAsync(Stream stream, string folder, string name)
    {
        using MemoryStream streamAux = new MemoryStream();
        await stream.CopyToAsync(streamAux);
        byte[] bytes = streamAux.ToArray();

        return await SaveAsync(bytes, folder, name);
    }

    public static Task<string> SaveAsync(Stream stream, string name)
    {
        return SaveAsync(stream, string.Empty, name);
    }

    public static async Task<string> SaveAsync(byte[] bytes, string folder, string name)
    {
        string directory = Path.Combine("wwwroot", folder);
        Directory.CreateDirectory(directory);

        string absolutePath = Path.Combine(directory, name);
        await File.WriteAllBytesAsync(absolutePath, bytes);

        string relativePath = $"{folder}/{name}";

        return relativePath;
    }
}
