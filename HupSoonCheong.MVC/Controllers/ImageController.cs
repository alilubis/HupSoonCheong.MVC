using HupSoonCheong.MVC.Models;
using HupSoonCheong.MVC.Models.ApplicationDbContext;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HupSoonCheong.MVC.Controllers
{
    public class ImageController : Controller
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly IWebHostEnvironment _environment;
        private readonly ILogger<ImageController> _logger;

        public ImageController(ApplicationDbContext dbContext, IWebHostEnvironment environment, ILogger<ImageController> logger)
        {
            _dbContext = dbContext;
            _environment = environment;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var containers = new List<Container>();
            try
            {
                containers = await _dbContext.Containers
                            .Include(c => c.Photos)
                            .Select(c => new Container
                            {
                                Id = c.Id,
                                Name = c.Name,
                                Photos = c.Photos.Select(p => new Photo
                                {
                                    Id = p.Id,
                                    FileName = p.FileName,
                                    FilePath = p.FilePath,
                                }).ToList()
                            })
                            .ToListAsync();
                _logger.LogInformation("Log message in the Containers Index() method");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving all containers.");
            }

            return View(containers);
        }

        [HttpPost]
        public async Task<IActionResult> CreateContainer(string name, List<IFormFile> files)
        {
            try
            {
                if (files == null || files.Count == 0)
                {
                    return RedirectToAction(nameof(Index));
                }

                var container = new Container
                {
                    Name = name
                };

                foreach (var file in files)
                {
                    if (file == null || file.Length == 0)
                    {
                        continue;
                    }

                    string uniqueFilename = $"{Path.GetFileNameWithoutExtension(file.FileName)}_{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
                    string filePath = Path.Combine(_environment.WebRootPath, "uploads", uniqueFilename);

                    try
                    {
                        var fileStream = new FileStream(filePath, FileMode.Create);
                        await file.CopyToAsync(fileStream);
                        fileStream.Close();
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error occurred while add file to webrootpath.");
                    }

                    var photo = new Photo
                    {
                        FileName = uniqueFilename,
                        FilePath = filePath,
                        ContainerId = container.Id,
                        Container = container
                    };

                    container.Photos.Add(photo);
                }

                await _dbContext.Containers.AddAsync(container);
                await _dbContext.SaveChangesAsync();
                _logger.LogInformation("Log message in the Containers CreateContainer() method");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while create container.");
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> DeleteContainer(int id)
        {
            try
            {
                var container = await _dbContext.Containers.Include(c => c.Photos).FirstOrDefaultAsync(c => c.Id == id);
                if (container == null)
                {
                    return NotFound();
                }

                foreach (var photo in container.Photos)
                {
                    _dbContext.Photos.Remove(photo);
                    try
                    {
                        var filePath = Path.Combine(_environment.WebRootPath, "uploads", photo.FileName);
                        using (FileStream fs = new FileStream(filePath, FileMode.Open))
                        {
                            fs.Close();
                        }
                        System.IO.File.Delete(filePath);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error occurred while delete file from webrootpath.");
                    }
                }

                _dbContext.Containers.Remove(container);
                await _dbContext.SaveChangesAsync();
                _logger.LogInformation("Log message in the Containers DeleteContainer() method");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while delete container.");
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
