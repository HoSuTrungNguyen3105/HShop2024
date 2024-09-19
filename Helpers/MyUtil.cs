using System.Text;

namespace HShop2024.Helpers
{
	public class MyUtil
	{
		public static string UploadHinh(IFormFile Hinh, string folder)
		{
			try
			{
				var fullPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Hinh", folder, Hinh.FileName);
				using (var myfile = new FileStream(fullPath, FileMode.CreateNew))
				{
					Hinh.CopyTo(myfile);
				}
				return Hinh.FileName;
			}
			catch (Exception ex)
			{
				return string.Empty;
			}
		}
		public static async Task<string> UploadHinhAsync(IFormFile file, string folder)
		{
			if (file == null || file.Length == 0)
				return null;

			string fileName = Path.GetFileNameWithoutExtension(file.FileName);
			string extension = Path.GetExtension(file.FileName);
			fileName = $"{fileName}_{DateTime.Now.ToString("yyyyMMddHHmmss")}{extension}";
			string uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Hinh", folder);
			string filePath = Path.Combine(uploadPath, fileName);

			if (!Directory.Exists(uploadPath))
				Directory.CreateDirectory(uploadPath);

			using (var fileStream = new FileStream(filePath, FileMode.Create))
			{
				await file.CopyToAsync(fileStream);
			}

			return $"/Hinh/{folder}/{fileName}";
		}
		public static string GenerateRamdomKey(int length = 5)
		{
			var pattern = @"qazwsxedcrfvtgbyhnujmiklopQAZWSXEDCRFVTGBYHNUJMIKLOP!";
			var sb = new StringBuilder();
			var rd = new Random();
			for (int i = 0; i < length; i++)
			{
				sb.Append(pattern[rd.Next(0, pattern.Length)]);
			}

			return sb.ToString();
		}
	}
}
