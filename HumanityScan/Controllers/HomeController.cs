using System.Diagnostics;
using HumanityScan.Models;
using Microsoft.AspNetCore.Mvc;
using Tesseract;

namespace HumanityScan.Controllers
{
    public class HomeController : Controller
    {
        private readonly IWebHostEnvironment _environment;

        public HomeController(IWebHostEnvironment environment)
        {
            _environment = environment;
        }

        public IActionResult Index()
        {
            ViewBag.CurrentCulture = System.Threading.Thread.CurrentThread.CurrentCulture.Name;
            return View();
        }

        [HttpPost]
        public IActionResult ExtractText(IFormFile imageFile)
        {
            if (imageFile == null || imageFile.Length == 0)
            {
                ViewBag.Error = "Lütfen bir resim dosyasý seçin.";
                return View("Index");
            }

            // Resmi geçici olarak kaydet
            var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads");
            Directory.CreateDirectory(uploadsFolder);
            var filePath = Path.Combine(uploadsFolder, imageFile.FileName);
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                imageFile.CopyTo(stream);
            }

            // OCR Ýþlemi
            string result;
            try
            {
                using (var engine = new TesseractEngine(Path.Combine(_environment.WebRootPath, "tessdata"), "tur", EngineMode.Default))
                using (var img = Pix.LoadFromFile(filePath))
                using (var page = engine.Process(img))
                {
                    result = page.GetText();
                }
            }
            catch (Exception ex)
            {
                ViewBag.Error = $"OCR Hatasý: {ex.Message}";
                return View("Index");
            }

            ViewBag.ExtractedText = result;
            return View("Index");
        }
    }
}
