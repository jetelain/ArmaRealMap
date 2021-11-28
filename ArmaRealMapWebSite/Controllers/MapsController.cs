using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ArmaRealMapWebSite.Entities;
using ArmaRealMapWebSite.Entities.Maps;
using Microsoft.AspNetCore.Authorization;
using ArmaRealMapWebSite.Models;

namespace ArmaRealMapWebSite.Controllers
{
    public class MapsController : Controller
    {
        private readonly ArmaRealMapContext _context;

        public MapsController(ArmaRealMapContext context)
        {
            _context = context;
        }

        // GET: Maps
        public async Task<IActionResult> Index()
        {
            return View(await _context.Map.ToListAsync());
        }

        // GET: Maps/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var map = await _context.Map
                .FirstOrDefaultAsync(m => m.MapID == id);
            if (map == null)
            {
                return NotFound();
            }

            return View(map);
        }

        // GET: Maps/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Maps/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "Admin")]
        public async Task<IActionResult> Create([Bind("MapID,Name,Label,Workshop,GridSize,CellSize,Resolution,TerrainRegion,MgrsBottomLeft")] Map map)
        {
            if (ModelState.IsValid)
            {
                _context.Add(map);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(map);
        }

        public async Task<IActionResult> GameIssue(string name, double x, double y)
        {
            var map = await _context.Map
                .FirstOrDefaultAsync(m => m.Name == name);
            if (map == null)
            {
                return NotFound();
            }
            var vm = new GameIssueViewModel();
            vm.Map = map;
            vm.X = x;
            vm.Y = y;
            vm.Place = map.FromGameCoordinates(x, y);
            return View(vm);
        }

        [Authorize(Policy = "Admin")]
        // GET: Maps/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var map = await _context.Map.FindAsync(id);
            if (map == null)
            {
                return NotFound();
            }
            return View(map);
        }

        // POST: Maps/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "Admin")]
        public async Task<IActionResult> Edit(int id, [Bind("MapID,Name,Label,Workshop,GridSize,CellSize,Resolution,TerrainRegion,MgrsBottomLeft")] Map map)
        {
            if (id != map.MapID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(map);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!MapExists(map.MapID))
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
            return View(map);
        }

        // GET: Maps/Delete/5
        [Authorize(Policy = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var map = await _context.Map
                .FirstOrDefaultAsync(m => m.MapID == id);
            if (map == null)
            {
                return NotFound();
            }

            return View(map);
        }

        // POST: Maps/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var map = await _context.Map.FindAsync(id);
            _context.Map.Remove(map);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool MapExists(int id)
        {
            return _context.Map.Any(e => e.MapID == id);
        }
    }
}
