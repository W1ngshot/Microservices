using Microsoft.AspNetCore.Mvc;
using Minio;
using Minio.DataModel.Args;
using Minio.Exceptions;

namespace FileMicroservice.Api.Controllers;

[ApiController]
[Route("/api/images")]
public class ImageController(IMinioClient minioClient) : ControllerBase
{
    private const string BucketName = "images";
    private const long MaxFileSize = 5 * 1024 * 1024; // 5 MB
    private readonly string[] _allowedMimeTypes = ["image/jpeg", "image/png", "image/gif"];

    [HttpPost("upload")]
    public async Task<IActionResult> UploadImage([FromForm] IFormFile file)
    {
        //Вынести в валидатор
        if (file is null || file.Length == 0)
            return BadRequest("Файл не выбран или пуст.");

        if (!_allowedMimeTypes.Contains(file.ContentType))
            return BadRequest("Недопустимый тип файла. Поддерживаются только изображения JPEG, PNG или GIF.");

        if (file.Length > MaxFileSize)
            return BadRequest($"Файл превышает максимальный размер {MaxFileSize / (1024 * 1024)} MB.");
        //Выносить до сюда

        var bucketExists = await minioClient.BucketExistsAsync(new BucketExistsArgs().WithBucket(BucketName));
        if (!bucketExists)
        {
            await minioClient.MakeBucketAsync(new MakeBucketArgs().WithBucket(BucketName));
        }

        // 6. Генерация уникального имени файла
        var fileName = Guid.NewGuid() + Path.GetExtension(file.FileName);

        try
        {
            // 7. Загрузка файла в MinIO
            await using (var stream = file.OpenReadStream())
            {
                await minioClient.PutObjectAsync(
                    new PutObjectArgs()
                        .WithBucket(BucketName)
                        .WithObject(fileName)
                        .WithStreamData(stream)
                        .WithObjectSize(file.Length)
                        .WithContentType(file.ContentType));
            }

            // 8. Возвращение успешного ответа с метаданными файла
            return Ok(new
            {
                FileName = fileName,
                OriginalFileName = file.FileName,
                ContentType = file.ContentType,
                Size = file.Length,
                UploadDate = DateTime.UtcNow
            });
        }
        catch (MinioException ex)
        {
            return StatusCode(500, $"Ошибка при загрузке файла: {ex.Message}");
        }
    }
}