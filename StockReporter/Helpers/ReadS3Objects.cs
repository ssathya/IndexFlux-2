using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using StockReporter.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockReporter.Helpers
{
    public class ReadS3Objects
    {
		private string _bucketName;
		private RegionEndpoint _region;

		public ReadS3Objects(string bucketName, RegionEndpoint region)
		{
			_bucketName = bucketName;
			if (region == null)
			{
				_region = RegionEndpoint.USEast2;
			}
			else
			{
				_region = region;
			}
			
		}
		public async Task<string> GetDataFromS3(string objectName)
		{
			var responseBody = "";
			try
			{
				IAmazonS3 client = new AmazonS3Client(_region);
				var request = new GetObjectRequest
				{
					BucketName = _bucketName,
					Key = objectName
				};
				using (var response = await client.GetObjectAsync(request))
				using (var responseStream = response.ResponseStream)
					using(var reader = new StreamReader(responseStream))
				{
					responseBody = await reader.ReadToEndAsync();
				}
				return responseBody.Derypt();
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Something went wrong while reading S3");
				Console.WriteLine(ex.Message);
				Console.WriteLine("Terminating application");
				throw;
			}
		}
    }
}
