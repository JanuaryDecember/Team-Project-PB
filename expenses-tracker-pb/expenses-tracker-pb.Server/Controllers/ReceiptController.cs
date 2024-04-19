using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Storage.v1;
using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/receipt")]
public class ReceiptController : ControllerBase
{
	private const string ProjectName = "My First Project";

	[HttpPost("upload")]
	public async Task<IActionResult> UploadFile([FromForm] IFormFile file)
	{
		if (file == null || file.Length == 0)
		{
			return BadRequest("Plik nie został przesłany.");
		}

		try
		{
			var clientId = "";
			var clientSecret = "";
			var scopes = new[] { @"https://www.googleapis.com/auth/devstorage.full_control" };

			var clientSecrets = new ClientSecrets
			{
				ClientId = clientId,
				ClientSecret = clientSecret
			};

			var cts = new CancellationTokenSource();
			var userCredential = await GoogleWebAuthorizationBroker.AuthorizeAsync(clientSecrets, scopes, "czyzia06@gmail.com", cts.Token);

			var service = new StorageService(new BaseClientService.Initializer
			{
				HttpClientInitializer = userCredential,
				ApplicationName = "Expenses Tracker"
			});

			var newBucket = new Google.Apis.Storage.v1.Data.Bucket()
			{
				Name = "nazwa_twojego_kubełka"
			};

			var newBucketRequest = service.Buckets.Insert(newBucket, ProjectName);
			await newBucketRequest.ExecuteAsync();

			var bucketsRequest = service.Buckets.List(ProjectName);
			var bucketsResponse = await bucketsRequest.ExecuteAsync();
			var buckets = bucketsResponse.Items;

			var bucketToUpload = buckets.FirstOrDefault()?.Name;

			var newObject = new Google.Apis.Storage.v1.Data.Object()
			{
				Bucket = bucketToUpload,
				Name = "some-file-" + new Random().Next(1, 666)
			};

			using (var fileStream = file.OpenReadStream())
			{
				var uploadRequest = service.Objects.Insert(newObject, bucketToUpload, fileStream, "image/png");
				await uploadRequest.UploadAsync();
			}

			return Ok(new { message = "Plik został pomyślnie przesłany." });
		}
		catch (Exception ex)
		{
			return StatusCode(500, "Wystąpił błąd podczas przesyłania pliku.");
		}
	}
}

