using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Test.Data;
using Test.Models;

namespace Test.Controllers
{
    [Authorize(Roles = "Admin, Chuyên viên")]

    public class PdfFilesController : Controller
    {
        private readonly TestContext _context;
        private readonly ILogger<PdfFilesController> _logger;
        public PdfFilesController(TestContext context, ILogger<PdfFilesController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: PdfFiles
        public async Task<IActionResult> Index()
        {
            var userName = HttpContext.User.Identity.Name;
            ViewData["UserName"] = userName;
            return View(await _context.PdfFile.ToListAsync());
        }

        // GET: PdfFiles/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var pdfFile = await _context.PdfFile
                .FirstOrDefaultAsync(m => m.PdfFileId == id);
            if (pdfFile == null)
            {
                return NotFound();
            }

            return View(pdfFile);
        }

        // GET: PdfFiles/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: PdfFiles/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("PdfFileId,FileName,FilePath,UploadDateTime")] PdfFile pdfFile)
        {
            if (ModelState.IsValid)
            {
                _context.Add(pdfFile);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(pdfFile);
        }

        public IActionResult Upload()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Upload([FromForm] ICollection<IFormFile> files)
        {
            try
            {
                if (files == null || files.Count == 0)
                {
                    return BadRequest("Không có file");
                }

                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "Uploads");

                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                foreach (var file in files)
                {
                    if (file == null || file.Length == 0)
                    {
                        continue; // Bỏ qua nếu không có file hoặc kích thước bằng 0
                    }

                    // Kiểm tra định dạng file
                    if (Path.GetExtension(file.FileName).ToLower() != ".pdf")
                    {
                        continue; // Bỏ qua nếu không phải là file PDF
                    }

                    var fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                    var filePath = Path.Combine(uploadsFolder, fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await file.CopyToAsync(stream);
                    }

                    // Tạo một đối tượng PdfFile để lưu trữ thông tin về file PDF
                    var pdfFile = new PdfFile
                    {
                        FileName = file.FileName,
                        FilePath = filePath,
                        UploadDateTime = DateTime.Now
                    };

                    // Lưu đối tượng PdfFile vào cơ sở dữ liệu
                    _context.Add(pdfFile);
                    await _context.SaveChangesAsync();

                    // Tìm Certification dựa trên 15 kí tự đầu tiên của FileName
                    var certificateNumber = file.FileName.Substring(0, Math.Min(15, file.FileName.Length));
                    var certification = _context.Certification.FirstOrDefault(c => c.CertificateNumber.StartsWith(certificateNumber));

                    if (certification != null)
                    {
                        // Nếu tìm thấy Certification, cập nhật đường dẫn đến file PDF trong Certification
                        certification.FilePath = filePath;
                        await _context.SaveChangesAsync();
                    }
                }

                return Ok("Hoàn tất");
            }
            catch (Exception ex)
            {
                
                return StatusCode(500, ex.InnerException.Message);
            }
        }



        // GET: PdfFiles/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var pdfFile = await _context.PdfFile.FindAsync(id);
            if (pdfFile == null)
            {
                return NotFound();
            }
            return View(pdfFile);
        }

        // POST: PdfFiles/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("PdfFileId,FileName,FilePath,UploadDateTime")] PdfFile pdfFile)
        {
            if (id != pdfFile.PdfFileId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(pdfFile);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PdfFileExists(pdfFile.PdfFileId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(pdfFile);
        }

        // GET: PdfFiles/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var pdfFile = await _context.PdfFile
                .FirstOrDefaultAsync(m => m.PdfFileId == id);
            if (pdfFile == null)
            {
                return NotFound();
            }

            return View(pdfFile);
        }

        // POST: PdfFiles/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var pdfFile = await _context.PdfFile.FindAsync(id);
            if (pdfFile != null)
            {
                _context.PdfFile.Remove(pdfFile);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool PdfFileExists(int id)
        {
            return _context.PdfFile.Any(e => e.PdfFileId == id);
        }
    }
}
