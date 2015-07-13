using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v2;
using Google.Apis.Drive.v2.Data;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DriveQuickstart
{
    class Program
    {
        static string[] Scopes = { DriveService.Scope.DriveReadonly };
        static string ApplicationName = "Profil Group Backup Tool";

        static void Main(string[] args)
        {
            UserCredential credential;

            using (var stream =
                new FileStream("client_secret.json", FileMode.Open, FileAccess.Read))
            {
                string credPath = System.Environment.GetFolderPath(
                    System.Environment.SpecialFolder.Personal);
                credPath = Path.Combine(credPath, ".credentials");

                credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                    GoogleClientSecrets.Load(stream).Secrets,
                    Scopes,
                    "user",
                    CancellationToken.None,
                    new FileDataStore(credPath, true)).Result;
            }

            var service = new DriveService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = ApplicationName,
            });

            if (!Directory.Exists("backups"))
                Directory.CreateDirectory("backups");

            string[] existingFiles = Directory.GetFiles("backups");
            foreach (var item in existingFiles)
            {
                System.IO.File.Delete(item);
            }

            var fileLines=System.IO.File.ReadAllLines(@"fileIds.txt");

            foreach (var fileLine in fileLines)
            {
                var fileMetadata = fileLine.Split(new char[] { ':' });
                var fileName = fileMetadata[0];
                var fileId = fileMetadata[1];

                FilesResource.GetRequest listRequest = service.Files.Get(fileId);

                var fileResource = listRequest.Execute();

                foreach (var item in fileResource.ExportLinks)
                {
                    var extension = item.Key.Split(new char[] { '/' })[1];
                    var link = item.Value;
                    var filename = fileResource.Title + "." + extension;
                    
                    downloadFile(service, link, @"backups\" + filename);
                }
            }
        }

        public static Boolean downloadFile(DriveService _service, string link, string _saveTo)
        {

            if (!String.IsNullOrEmpty(link))
            {
                try
                {
                    var x = _service.HttpClient.GetByteArrayAsync(link);
                    byte[] arrBytes = x.Result;
                    System.IO.File.WriteAllBytes(_saveTo, arrBytes);
                    return true;
                }
                catch (Exception e)
                {
                    Console.WriteLine("An error occurred: " + e.Message);
                    return false;
                }
            }
            else
            {
                // The file doesn't have any content stored on Drive.
                return false;
            }
        }
    }
}