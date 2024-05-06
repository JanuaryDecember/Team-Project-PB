using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;


[ApiController]
[Route("api/receipt/upload")]
public class ReceiptController : ControllerBase
{
	private static string[] Scopes = { DriveService.Scope.Drive };
	private static string ApplicationName = "Google Drive API";
	private static string ServiceAccountKeyPath = "C:\\Users\\0Xandra\\source\\credentials.json";

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

			// Pobranie Id użytkownika
			var folderName = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

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

			var folderId = await FindUserFolder(service, folderName);
			if (string.IsNullOrEmpty(folderId))
			{
				folderId = await CreateUserFolder(service, folderName);
			}


			var fileMetadata = new Google.Apis.Drive.v3.Data.File()
			{
				Name = fileName,
				Parents = new List<string> { folderId },
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

	private async Task<string> FindUserFolder(DriveService service, string folderName)
	{
		try
		{
			var listRequest = service.Files.List();
			listRequest.Q = $"name = '{folderName}' and mimeType = 'application/vnd.google-apps.folder' and '1I1d5Qvrqntmiy9QyVgTaTrzSXrlHQriW' in parents";
			var folders = await listRequest.ExecuteAsync();

			if (folders.Files.Any())
			{
				Console.WriteLine($"User folder exists. ID: {folders.Files.First().Id}");
				return folders.Files.First().Id;
			}
			else
			{
				Console.WriteLine($"User folder does not exist.");
				return null;
			}
		}
		catch (Exception ex)
		{
			Console.WriteLine($"Error finding user folder: {ex.Message}. Stack Trace: {ex.StackTrace}");
			throw;
		}
	}
	private async Task<string> CreateUserFolder(DriveService service, string folderName)
	{
		try
		{
			Console.WriteLine($"Creating new user folder");

			var folderMetadata = new Google.Apis.Drive.v3.Data.File()
			{
				Name = folderName,
				MimeType = "application/vnd.google-apps.folder",
				Parents = new List<string> { "1I1d5Qvrqntmiy9QyVgTaTrzSXrlHQriW" },
			};

			var request = service.Files.Create(folderMetadata);
			request.Fields = "id";

			var folder = await request.ExecuteAsync();
			Console.WriteLine($"New user folder created. ID: {folder.Id}");

			return folder.Id;
		}
		catch (Exception ex)
		{
			Console.WriteLine($"Error creating user folder: {ex.Message}. Stack Trace: {ex.StackTrace}");
			throw;
		}
	}

}
