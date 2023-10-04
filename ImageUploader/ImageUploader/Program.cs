using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using System.Text.Json;



var app = WebApplication.Create();
app.MapGet("/", (HttpContext context) =>
{
    context.Response.ContentType = "text/html";
    string htmlCode = @"
    <!DOCTYPE html>
<html>

<head>
    <title>Image Uploader Form</title>
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <style>
        @import url('https://fonts.googleapis.com/css2?family=Poppins:wght@400;600;900&display=swap');

        body {
            font-family: 'Poppins', sans-serif;
            background-color: #f3f5f8;
            margin: 20px;
        }

        h1 {
            text-align: center;
        }

        form {
            width: 400px;
            margin: 40px auto;
            text-align: center;
        }

        label {
            display: block;
            margin-bottom: 5px;
        }

        input[type=""text""],
        input[type=""file""] {
            width: 100%;
            padding: 8px;
            margin-bottom: 10px;
            border: 1px solid #ccc;
            border-radius: 4px;
        }

        input[type=""submit""] {
            background-color: #4CAF50;
            color: white;
            padding: 10px 20px;
            border: none;
            border-radius: 4px;
            cursor: pointer;
        }

        input[type=""submit""]:hover {
            background-color: #45a049;
        }

        .error {
            color: #ff0000;
            font-size: 14px;
            margin-top: 5px;
        }
    </style>

</head>

<body>
    <h1>Image Uploader Form</h1>
    <form name=""imageForm""  method=""post"" enctype=""multipart/form-data"">

        <label for=""imagetitle""> Title of the image:</label>
        <input type=""text"" id=""title"" name=""title"" ><br><br>

        <label for=""imagefile""> Select an image file (JPEG, PNG, GIF):</label>
        <input type=""file"" id=""imagefile"" name=""imagefile"" accept="".JPEG, .jpg, .png, .gif"" required><br><br>

        <input type=""submit"" value=""Upload"">
    </form>
</body>

</html>
";
    return context.Response.WriteAsync(htmlCode);

});
app.MapPost("/", async (HttpContext context) =>
{
    IFormCollection form = await context.Request.ReadFormAsync();
    string? title = form["title"];
    var file = form.Files.GetFile("imagefile");

    if (string.IsNullOrEmpty(title))
    {
        return Results.BadRequest("Image title is required.");
    }
    if (file == null || file.Length == 0)
    {
        return Results.BadRequest("Image file is required.");
    }
    var allowedExtentions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
    var fileExtension = Path.GetExtension(file.FileName).ToLower();
    if (!allowedExtentions.Contains(fileExtension))
    {
        return Results.BadRequest("Invalid file extension, Only JPG, PNG, and GIF are allowed.");

    }
    //saving to disk

    var imageID = Guid.NewGuid().ToString();
    var imagePath = Path.Combine(Directory.GetCurrentDirectory(), "picture", $"{imageID}{Path.GetExtension(file.FileName)}");
    using (var fileStream = new FileStream(imagePath, FileMode.Create))
    {
        await file.CopyToAsync(fileStream);
    }
    //metadata
    var imageData = new
    {
        Title = title,
        Id = imageID,
        imgPath= imagePath,
        FileName = file.FileName,
    };
   

    var jsonData = JsonSerializer.Serialize(imageData); // convert to JSON
    var jsonPath = Path.Combine(Directory.GetCurrentDirectory(), "imageData.json");//json file path
    File.AppendAllText(jsonPath, $"{jsonData}{Environment.NewLine}"); //append the jsonData to the file

    var imageURL = $"/picture/{imageID}";
    return Results.Redirect(imageURL);
});

app.MapGet("/picture/{id}", async(string id , HttpContext context) =>
{
    var jsonPath = Path.Combine(Directory.GetCurrentDirectory(), "imageData.json");
   
    var allLines = await File.ReadAllLinesAsync(jsonPath);
    
    var allImages = new List<Image>();
    
    foreach (var line in allLines)
    {
        var img = JsonSerializer.Deserialize<Image>(line);
        allImages.Add(img);
    }

   if(allImages.Count == 0)
    {
        return Results.NotFound("No images exists.");
    }
    
   var image = allImages.FirstOrDefault(i => i.Id == id); //find the image with this id
   if(image == null)
    {
        return Results.NotFound("Image Not Found.");
    }
    byte[] imageBytes = await File.ReadAllBytesAsync(image.imgPath);
    string imgbase64 = Convert.ToBase64String(imageBytes);

    context.Response.ContentType = "text/html";
    var htmlCode = $@"
     <!DOCTYPE html>
<html>

<head>
    <title>Image Uploader Form</title>
    <meta name=""viewport"" content=""width=device-width"" , initial-scale=""1.0"">
    <style>
        @import url('https://fonts.googleapis.com/css2?family=Poppins:wght@400;600;900&display=swap');

        body {{
      display: flex;
      font-family: 'Poppins', sans-serif;
      background-color: #f3f5f8;
      justify-content: center;
      align-items: center;
      height: 100vh;
      
      flex-direction: column;
    }}
    .card {{
      width: 300px;
      border: 1px solid #ccc;
      border-radius: 5px;
      padding: 10px;
      box-shadow: 0 2px 4px rgba(0, 0, 0, 0.1);
    }}

    .card img {{
      width: 100%;
      height: auto;
      border-radius: 5px;
    }}

    .card h2 {{
      margin-top: 10px;
      font-size: 18px;
      text-align: center;
    }}
    </style>

</head>

<body>
    <h1 style=""text-align: center;"" >The Uploaded Image</h1> 
    <div class=""card"">
        <img src=""data:image/png;base64,{imgbase64}""  alt=""Title"">
        <h2>{image.Title}</h2>
      </div>
  
</body>

</html>
";

    return Results.Text(htmlCode, "text/html");

});


app.Run();
public class Image
{
    public string Title { get; set; }
    public string Id { get; set; }
    public string imgPath { get; set; }
    public string FileName { get; set; }
    
   
}
