using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.IO;
using System.Threading.Tasks;

[ApiController]
[Route("api/receipt/upload")]
public class ReceiptController : ControllerBase
{
	private static string[] Scopes = { DriveService.Scope.Drive };
	private static string ApplicationName = "Google Drive API";
	private static string ServiceAccountKeyPath = "C:\\Users\\jakub\\Downloads\\credentials.json";

	[HttpPost]
	public async Task<IActionResult> Upload([FromForm] string fileName, [FromForm] IFormFile file)
	{
		if (file == null || file.Length == 0)
		{
			return BadRequest("No file uploaded.");
		}

		try
		{
			Console.WriteLine($"Received file: {file.FileName}");

			GoogleCredential credential;
			using (var stream = new FileStream(ServiceAccountKeyPath, FileMode.Open, FileAccess.Read))
			{
				credential = GoogleCredential.FromStream(stream)
					.CreateScoped(Scopes);
			}

			var service = new DriveService(new BaseClientService.Initializer()
			{
				HttpClientInitializer = credential,
				ApplicationName = ApplicationName,
			});

			var fileMetadata = new Google.Apis.Drive.v3.Data.File()
			{
				Name = fileName,
				Parents = new List<string> { "1I1d5Qvrqntmiy9QyVgTaTrzSXrlHQriW" }, // ID folderu docelowego
			};

			using (var memoryStream = new MemoryStream())
			{
				await file.CopyToAsync(memoryStream);
				var fileStream = new MemoryStream(memoryStream.ToArray());

				var request = service.Files.Create(fileMetadata, fileStream, file.ContentType);
				request.Fields = "id";
				var response = request.Upload();
				var uploadedFile = request.ResponseBody;

				return Ok($"File uploaded successfully. File ID: {uploadedFile.Id}");
			}
		}
		catch (Exception ex)
		{
			Console.WriteLine($"Error uploading file: {ex.Message}. Stack Trace: {ex.StackTrace}");
			return StatusCode(500, $"Error uploading file: {ex.Message}. Stack Trace: {ex.StackTrace}");
		}
	}
}
