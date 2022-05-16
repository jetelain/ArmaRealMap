using System;
using System.Linq;
using System.Threading.Tasks;
using ArmaRealMapWebSite.Entities;
using ArmaRealMapWebSite.Entities.Maps;
using ArmaRealMapWebSite.Models;
using CoordinateSharp;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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

        private static int[] GridSizes = new[] { 2048, 4096, 8192 };

        public IActionResult SelectLocation(double? lat1, double? lon1, double? lat2, double? lon2)
        {
            if ( lat1 != null && lon2 != null && lat2 != null && lon2 != null)
            {
                var p1 = new CoordinateSharp.Coordinate(lat1.Value, lon1.Value);
                var p2 = new CoordinateSharp.Coordinate(lat2.Value, lon2.Value);
                
                var distance = p1.Get_Distance_From_Coordinate(p2, Shape.Ellipsoid);
                
                var center = new CoordinateSharp.Coordinate(lat1.Value, lon1.Value);
                center.Move(p2, distance.Meters / 2, Shape.Ellipsoid);
                
                var bearing = distance.Bearing * Math.PI / 180;
                var width = Math.Cos(bearing) * distance.Meters;
                var height = Math.Sin(bearing) * distance.Meters;
                
                var size = Math.Max(width, height);
                var nsize = Math.Floor(size / 1024d) * 1024;
                var rsize = Math.Max(Math.Min(81_920, nsize), 10_240);

                var sw = new Coordinate(center.Latitude.ToDouble(), center.Longitude.ToDouble());
                sw.Move(rsize * Math.Sqrt(2) / 2, 180 + 45, Shape.Ellipsoid);

                var map = new Map();
                map.MgrsBottomLeft = sw.MGRS.ToString();
                
                foreach(var gridSize in GridSizes)
                {
                    map.GridSize = gridSize;
                    map.CellSize = rsize / gridSize;
                    if (map.CellSize < 8)
                    {
                        break;
                    }
                }

                if ( map.SizeInMeters > 40960 )
                {
                    map.Resolution = 2;
                }
                else
                {
                    map.Resolution = 1;
                }
                return View(map);
            }
            return View();
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "Admin")]
        public IActionResult SelectLocation([Bind("GridSize,CellSize,Resolution,MgrsBottomLeft")] Map map)
        {
            return View("Create", map);
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
            [Authorize(Policy = "Admin")]
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
