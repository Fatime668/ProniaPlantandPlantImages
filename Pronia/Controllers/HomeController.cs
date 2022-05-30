using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Pronia.DAL;
using Pronia.Models;
using Pronia.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Pronia.Controllers
{
    public class HomeController : Controller
    {
        private readonly AppDbContext _context;

        public HomeController(AppDbContext context)
        {
            _context = context;
        }
        public async Task<IActionResult> Index()
        {
            HomeVM model = new HomeVM
            {
                Sliders = await _context.Sliders.ToListAsync(),
                Clients = await _context.Clients.ToListAsync(),
                Plants = await _context.Plants.Include(p => p.PlantImages).ToListAsync()
        };
            
            return View(model);
        }
    }
}
