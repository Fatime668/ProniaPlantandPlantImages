using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Pronia.DAL;
using Pronia.Extensions;
using Pronia.Models;
using Pronia.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Pronia.Areas.ProniaAdmin.Controllers
{
    [Area("ProniaAdmin")]
    public class PlantController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _env;

        public PlantController(AppDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }
        public async Task<IActionResult> Index()
        {
            List<Plant> plants = await _context.Plants.Include(p => p.PlantImages).ToListAsync();
            return View(plants);
        }
        public async Task<IActionResult> Create()
        {
            ViewBag.Colors = await _context.Colors.ToListAsync();
            ViewBag.Sizes = await _context.Sizes.ToListAsync();
            return View();
        }
        [HttpPost]
        [AutoValidateAntiforgeryToken]
        public async Task<IActionResult> Create(Plant plant)
        {
            ViewBag.Colors = await _context.Colors.ToListAsync();
            ViewBag.Sizes = await _context.Sizes.ToListAsync();
            if (!ModelState.IsValid) return View();
            if (plant.MainImage == null || plant.AnotherImages == null)
            {
                ModelState.AddModelError("", "Please, choose main image or another image");
                return View();
            }
            if (!plant.MainImage.IsOkay(1))
            {
                ModelState.AddModelError("MainImage", "Please, choose image file and max 1Mb");
                return View();
            }
            foreach (var image in plant.AnotherImages)
            {
                if (!image.IsOkay(1))
                {
                    ModelState.AddModelError("AnotherImage", "Please, choose image file and max 1Mb");
                    return View();
                }
            }

            plant.PlantImages = new List<PlantImage>();

            PlantImage mainImage = new PlantImage
            {
                ImagePath = await plant.MainImage.FileCreate(_env.WebRootPath, @"assets\images\website-images"),
                IsMain = true,
                Plant = plant
            };

            plant.PlantImages.Add(mainImage);
            foreach (var image in plant.AnotherImages)
            {
                PlantImage another = new PlantImage
                {
                    ImagePath = await image.FileCreate(_env.WebRootPath, @"assets\images\website-images"),
                    IsMain = false,
                    Plant = plant
                };
                plant.PlantImages.Add(another);
            }
            await _context.Plants.AddAsync(plant);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
        public async Task<IActionResult> Edit(int id)
        {
            ViewBag.Colors = await _context.Colors.ToListAsync();
            ViewBag.Sizes = await _context.Sizes.ToListAsync();
            Plant plant = await _context.Plants.Include(p=>p.PlantImages).FirstOrDefaultAsync(p => p.Id == id);
            if (plant == null) return NotFound();
            return View(plant);
        }
        [HttpPost]
        [AutoValidateAntiforgeryToken]
        public async Task<IActionResult> Edit(int id, Plant plant)
        {
            ViewBag.Colors = await _context.Colors.ToListAsync();
            ViewBag.Sizes = await _context.Sizes.ToListAsync();
            Plant existed = await _context.Plants.FirstOrDefaultAsync(p=>p.Id==id);
            if (existed == null) return View();
            return RedirectToAction(nameof(Index));
        }
        public async Task<IActionResult> Detail(int id)
        {
            Plant plant = await _context.Plants.Include(p => p.PlantImages).FirstOrDefaultAsync(p => p.Id == id);
            if (plant == null) return NotFound();
            return View(plant);
        }
        public async Task<IActionResult> Delete(int id)
        {
            Plant plant = await _context.Plants.Include(p=>p.PlantImages).FirstOrDefaultAsync(p => p.Id == id);
            if (plant == null) return NotFound();
            return View(plant);
        }
        [HttpPost]
        [AutoValidateAntiforgeryToken]
        [ActionName("Delete")]
        public async Task<IActionResult> DeletePlant(int id)
        {
            Plant plant = await _context.Plants.FirstOrDefaultAsync(p => p.Id == id);
            if (plant == null) return NotFound();
            _context.Plants.Remove(plant);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}
