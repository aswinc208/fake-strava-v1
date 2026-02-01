using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

public class OcrController : Controller
{
    [HttpGet]
    public IActionResult Index()
    {
        return View();
    }

    [HttpPost]
    public IActionResult Upload(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return Content("No file");

        var ocrDir = "/app/ocr-data";
        var inputPath = Path.Combine(ocrDir, "input.png");
        var outputPath = Path.Combine(ocrDir, "output.txt");

        using (var stream = new FileStream(inputPath, FileMode.Create))
            file.CopyTo(stream);

        var process = new Process();
        process.StartInfo.FileName = "docker";
        process.StartInfo.Arguments = "exec tesseract tesseract /data/input.png /data/output --oem 3 --psm 11 -l eng";
        process.StartInfo.RedirectStandardOutput = true;
        process.StartInfo.UseShellExecute = false;
        process.Start();
        process.WaitForExit();

        var text = System.IO.File.Exists(outputPath)
            ? System.IO.File.ReadAllText(outputPath)
            : "OCR failed";

        return Content(text);
    }
}
