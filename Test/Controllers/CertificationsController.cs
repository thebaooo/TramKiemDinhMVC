using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ExcelDataReader;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Test.Data;
using Test.Models;
using X.PagedList;

namespace Test.Controllers
{
    [Authorize(Roles = "Admin, Chuyên viên, Cán bộ, Lãnh đạo")]

    public class CertificationsController : Controller
    {
        private readonly TestContext _context;

        public CertificationsController(TestContext context)
        {
            _context = context;
        }


        public async Task<IActionResult> Index(string searchString, int? page)
        {
            IQueryable<Certification> certifications = _context.Certification;
            var userName = HttpContext.User.Identity.Name;
            ViewData["UserName"] = userName;
            var userProvince = User.Claims.FirstOrDefault(c => c.Type == "ProvinceName")?.Value;
            var isAdmin = User.IsInRole("Admin");

            if (!isAdmin && !string.IsNullOrEmpty(userProvince))
            {
                certifications = certifications.Where(c => c.ProvinceName == userProvince);
            }
            else if (!isAdmin)
            {
                // Nếu không phải là admin và không có thông tin tỉnh của người dùng, không cho phép xem bất kỳ bản ghi nào
                certifications = certifications.Where(c => false);
            }

            if (!string.IsNullOrEmpty(searchString))
            {
                certifications = certifications.Where(n =>
                    n.CertificateNumber.Contains(searchString) ||
                    n.StationCode.Contains(searchString) ||
                    n.Note.Contains(searchString) ||
                    n.ProvinceName.Contains(searchString) ||
                    n.InspectionYear.ToString().Contains(searchString));
                ViewBag.SearchMessage = $"Kết quả tìm kiếm cho: '{searchString}'";
            }
            else
            {
                // Clear filter if search string is empty
                ViewBag.CurrentFilter = null;
            }

            ViewBag.CurrentFilter = searchString; // Update ViewBag.CurrentFilter here

            int pageSize = 10;
            int pageNumber = page ?? 1;
            return View(await certifications.ToPagedListAsync(pageNumber, pageSize));
        }


        public IActionResult Upload()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Upload([FromForm] IFormFile file)
        {
            try
            {
                if (file == null || file.Length == 0)
                {
                    return BadRequest("Không có file");
                }

                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "Uploads");

                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                var filePath = Path.Combine(uploadsFolder, file.FileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                using (var stream = System.IO.File.Open(filePath, FileMode.Open, FileAccess.Read))
                {
                    using (var reader = ExcelReaderFactory.CreateReader(stream))
                    {
                        bool isHeaderSkipped = false;

                        do
                        {
                            while (reader.Read())
                            {
                                if (!isHeaderSkipped)
                                {
                                    isHeaderSkipped = true;
                                    continue;
                                }
                                Certification c = new Certification();
                                c.CertificateNumber = reader.GetValue(1)?.ToString(); // Dòng 1
                                c.StationCode = reader.GetValue(2)?.ToString(); // Dòng 2

                                // Xử lý dòng 3
                                string inspectionDateString = reader.GetValue(3)?.ToString();
                                if (!string.IsNullOrEmpty(inspectionDateString))
                                {
                                    // Thử chuyển đổi thành DateTime với định dạng dd/MM/yyyy
                                    if (DateTime.TryParseExact(inspectionDateString, "dd/MM/yy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime inspectionDate))
                                    {
                                        // Nếu năm nhỏ hơn 100, giả sử là năm trong thế kỷ hiện tại
                                        if (inspectionDate.Year < 100)
                                        {
                                            // Xác định năm là 2023
                                            int currentCentury = DateTime.Now.Year / 100;
                                            inspectionDate = new DateTime(2000 + inspectionDate.Year, inspectionDate.Month, inspectionDate.Day);
                                        }

                                        c.InspectionDate = inspectionDate;
                                    }
                                    else
                                    {
                                        throw new FormatException("Ngày kiểm tra không hợp lệ");
                                    }
                                }
                                else
                                {
                                    // Xử lý khi giá trị ngày kiểm tra trống
                                    // Có thể gán một giá trị mặc định hoặc bỏ qua
                                }

                                c.Note = reader.GetValue(4)?.ToString(); // Dòng 4

                                // Xử lý dòng 5 (số int)
                                int inspectionYear;
                                if (int.TryParse(reader.GetValue(5)?.ToString(), out inspectionYear))
                                {
                                    c.InspectionYear = inspectionYear;
                                }
                                else
                                {
                                    throw new FormatException("Năm kiểm tra không hợp lệ");
                                }

                                c.ProvinceName = reader.GetValue(6)?.ToString(); // Dòng 6

                                c.SentTime = DateTime.Now;

                                c.FilePath = null;

                                _context.Add(c);
                                await _context.SaveChangesAsync();
                            }
                        } while (reader.NextResult());
                    }
                }
                return Ok("Hoàn tất");
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }


        public async Task<IActionResult> ViewPdf(int id)
        {
            var certification = await _context.Certification.FirstOrDefaultAsync(c => c.CertificationId == id);

            if (certification == null || string.IsNullOrEmpty(certification.FilePath))
            {
                return NotFound();
            }

            return PhysicalFile(certification.FilePath, "application/pdf");
        }

        public async Task<IActionResult> DownloadPdf(int id)
        {
            var certification = await _context.Certification.FirstOrDefaultAsync(c => c.CertificationId == id);

            if (certification == null || string.IsNullOrEmpty(certification.FilePath))
            {
                return NotFound();
            }

            var fileName = $"{certification.CertificateNumber}.pdf";

            return PhysicalFile(certification.FilePath, "application/pdf", fileName);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var certification = await _context.Certification.FirstOrDefaultAsync(m => m.CertificationId == id);

            if (certification == null)
            {
                return NotFound();
            }

            return View(certification);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var certification = await _context.Certification.FindAsync(id);

            if (certification != null)
            {
                _context.Certification.Remove(certification);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        private async Task<bool> CertificationExistsAsync(int id)
        {
            return await _context.Certification.AnyAsync(e => e.CertificationId == id);
        }
    }
}
