//
// [Authorize]
// [HttpGet("GetTwoFactorStatus")]
// public async Task<IActionResult> GetTwoFactorStatus()
// {
//     try
//     {
//         var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
//         var user = await dbContext.Users.FirstOrDefaultAsync(u => u.Id == userId);
//
//         if (user != null)
//         {
//             return Ok(new { twoFactorEnabled = user.TwoFactorEnabled });
//         }
//         else
//         {
//             return NotFound("User not found");
//         }
//     }
//     catch (Exception ex)
//     {
//         return StatusCode(500, "Error:" + ex.Message);
//     }
// }
// [Authorize]
// [HttpGet("GetTwoFactorKey")]
// public async Task<IActionResult> GetTwoFactorKey()
// {
//     try
//     {
//         var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
//         var user = await dbContext.Users.FirstOrDefaultAsync(u => u.Id == userId);
//
//         if (user != null)
//         {
//             string GoogleAuthKey = "";
//             string QrImageUrl = "";
//             const string validChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
//             Random random = new Random();
//             for (int i = 0; i < 10; i++)
//             {
//                 GoogleAuthKey += validChars[random.Next(0, validChars.Length)];
//             }
//             TwoFactorAuthenticator TwoFacAuth = new TwoFactorAuthenticator();
//             var setupInfo = TwoFacAuth.GenerateSetupCode("ExpensionTracker", user.UserName, ConvertSecretToBytes(GoogleAuthKey, false), 200);
//             QrImageUrl = setupInfo.QrCodeSetupImageUrl;
//             return Ok(new { authKey = GoogleAuthKey, barcodeImageUrl = QrImageUrl });
//         }
//         else
//         {
//             return NotFound("User not found");
//         }
//     }
//     catch (Exception ex)
//     {
//         return StatusCode(500, "Error:" + ex.Message);
//     }
// }
//
// [Authorize]
// [HttpPost("enableTwoFactor")]
// public async Task<IActionResult> enableTwoFactor()
// {
//     try
//     {
//         var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
//         var user = await dbContext.Users.FirstOrDefaultAsync(u => u.Id == userId);
//
//         if (user != null)
//         {
//             string requestBody;
//             using (var reader = new StreamReader(Request.Body))
//             {
//                 requestBody = await reader.ReadToEndAsync();
//             }
//             dynamic data = JObject.Parse(requestBody);
//             string authKey = data.authKey;
//             string enteredAuthKey = data.enteredAuthKey;
//             TwoFactorAuthenticator TwoFacAuth = new TwoFactorAuthenticator();
//             bool isValid = TwoFacAuth.ValidateTwoFactorPIN(authKey, enteredAuthKey, TimeSpan.FromSeconds(15));
//
//             if (isValid)
//             {
//                 user.TwoFactorEnabled = true;
//                 user.GoogleAuthKey = authKey;
//                 await dbContext.SaveChangesAsync();
//
//                 return Ok(true);
//             }
//
//             return Ok(false);
//         }
//         else
//         {
//             return NotFound("User not found");
//         }
//     }
//     catch (Exception ex)
//     {
//         return StatusCode(500, "Error:" + ex.Message);
//     }
// }
// [Authorize]
// [HttpPost("disableTwoFactor")]
// public async Task<IActionResult> disableTwoFactor()
// {
//     try
//     {
//         var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
//         var user = await dbContext.Users.FirstOrDefaultAsync(u => u.Id == userId);
//
//         if (user != null)
//         {
//             string requestBody;
//             using (var reader = new StreamReader(Request.Body))
//             {
//                 requestBody = await reader.ReadToEndAsync();
//             }
//             dynamic data = JObject.Parse(requestBody);
//             string enteredAuthKey = data.enteredAuthKey;
//             TwoFactorAuthenticator TwoFacAuth = new TwoFactorAuthenticator();
//             bool isValid = TwoFacAuth.ValidateTwoFactorPIN(user.GoogleAuthKey, enteredAuthKey, TimeSpan.FromSeconds(15));
//
//             if (isValid)
//             {
//                 user.TwoFactorEnabled = false;
//                 user.GoogleAuthKey = null;
//                 await dbContext.SaveChangesAsync();
//                 return Ok(true);
//             }
//
//             return Ok(false);
//         }
//         else
//         {
//             return NotFound("User not found");
//         }
//     }
//     catch (Exception ex)
//     {
//         return StatusCode(500, "Error:" + ex.Message);
//     }
// }
