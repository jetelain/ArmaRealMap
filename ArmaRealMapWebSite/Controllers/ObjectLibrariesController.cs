using System;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using ArmaRealMap.Core;
using ArmaRealMap.Core.ObjectLibraries;
using ArmaRealMapWebSite.Entities;
using ArmaRealMapWebSite.Entities.Assets;
using ArmaRealMapWebSite.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ArmaRealMapWebSite.Controllers
{
    public class ObjectLibrariesController : Controller
    {
        private readonly ArmaRealMapContext _context;

        public ObjectLibrariesController(ArmaRealMapContext context)
        {
            _context = context;
        }

        // GET: ObjectLibraries
        public async Task<IActionResult> Index(TerrainRegion? terrainRegion)
        {
            var vm = new LibrariesViewModel();
            vm.TerrainRegion = terrainRegion;
            if (terrainRegion != null)
            {
                vm.Libraries = await _context.ObjectLibraries.Where(t => t.TerrainRegion == terrainRegion).ToListAsync();
            }
            else
            {
                vm.Libraries = await _context.ObjectLibraries.ToListAsync();
            }
            var metadata = MetadataProvider.GetMetadataForType(typeof(ObjectCategory));
            var dict = metadata.EnumGroupedDisplayNamesAndValues.ToDictionary(e => Enum.Parse<ObjectCategory>(e.Value), e => e.Key.Name);
            vm.Libraries = vm.Libraries.OrderBy(l => dict[l.ObjectCategory]).ThenBy(l => l.TerrainRegion).ToList();
            return View(vm);
        }

        public async Task<IActionResult> Export()
        {
            var alldata = await _context.ObjectLibraries
                .Include(l => l.Assets).ThenInclude(a => a.Asset)
                .Include(l => l.Compositions).ThenInclude(a => a.Assets).ThenInclude(a => a.Asset)
                .ToListAsync();

            return Json(
                alldata
                    .Select(
                    l => new JsonObjectLibrary()
                    {
                        Category = l.ObjectCategory,
                        Density = l.Density,
                        Terrain = l.TerrainRegion,
                        Probability = l.Probability,
                        Objects = l.Assets.Select(a => new SingleObjet()
                        {
                            CX = a.Asset.CX,
                            CY = a.Asset.CY,
                            CZ = a.Asset.CZ,
                            Depth = a.Asset.Depth,
                            Height = a.Asset.Height,
                            MaxZ = a.MaxZ,
                            MinZ = a.MinZ,
                            Name = a.Asset.Name,
                            ClusterName = a.Asset.ClusterName,
                            PlacementRadius = a.PlacementRadius,
                            PlacementProbability = a.Probability,
                            ReservedRadius = a.ReservedRadius,
                            Width = a.Asset.Width
                        }).ToList(),
                        Compositions = l.Compositions.Select(c => new Composition()
                        {
                            Depth = c.Depth,
                            Height = c.Height,
                            Objects = c.Assets.Select(a => new CompositionObject()
                            {
                                Angle = a.Angle,
                                Depth = a.Asset.Depth,
                                Height = a.Asset.Height,
                                Name = a.Asset.Name,
                                Width = a.Asset.Width,
                                X = a.X,
                                Y = a.Y,
                                Z = a.Z
                            }).ToList(),
                            Width = c.Width
                        }).ToList()
                    })
                    .ToList(),
                    new JsonSerializerOptions
                    {
                        Converters = { new JsonStringEnumConverter() },
                        WriteIndented = true
                    }
                );
        }

        // GET: ObjectLibraries/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var objectLibrary = await _context.ObjectLibraries
                .FirstOrDefaultAsync(m => m.ObjectLibraryID == id);
            if (objectLibrary == null)
            {
                return NotFound();
            }

            objectLibrary.Assets = await _context.ObjectLibraryAssets
                .Include(a => a.Asset)
                .Where(a => a.ObjectLibraryID == objectLibrary.ObjectLibraryID)
                .ToListAsync();
            objectLibrary.Compositions = await _context.ObjectLibraryComposition
    .Where(a => a.ObjectLibraryID == objectLibrary.ObjectLibraryID)
    .ToListAsync();
            return View(objectLibrary);
        }

        // GET: ObjectLibraries/Create
        [Authorize(Policy = "Admin")]
        public IActionResult Create()
        {
            return View();
        }

        // POST: ObjectLibraries/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [Authorize(Policy = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ObjectLibraryID,Name,TerrainRegion,ObjectCategory,Density,Probability")] ObjectLibrary objectLibrary)
        {
            if (ModelState.IsValid)
            {
                _context.Add(objectLibrary);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Details), new { id = objectLibrary.ObjectLibraryID });
            }
            return View(objectLibrary);
        }

        // GET: ObjectLibraries/Edit/5
        [Authorize(Policy = "Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var objectLibrary = await _context.ObjectLibraries.FindAsync(id);
            if (objectLibrary == null)
            {
                return NotFound();
            }
            return View(objectLibrary);
        }





        // POST: ObjectLibraries/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [Authorize(Policy = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ObjectLibraryID,Name,TerrainRegion,ObjectCategory,Density,Probability")] ObjectLibrary objectLibrary)
        {
            if (id != objectLibrary.ObjectLibraryID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(objectLibrary);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ObjectLibraryExists(objectLibrary.ObjectLibraryID))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Details), new { id = objectLibrary.ObjectLibraryID });
            }
            return View(objectLibrary);
        }
        [Authorize(Policy = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EqualProbability(int id)
        {
            var assets = await _context.ObjectLibraryAssets.Where(o => o.ObjectLibraryID == id).ToListAsync();
            foreach(var asset in assets)
            {
                asset.Probability = 1f / assets.Count;
                _context.Update(asset);
            }
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Details), new { id });
        }

        // GET: ObjectLibraries/Delete/5
        [Authorize(Policy = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var objectLibrary = await _context.ObjectLibraries
                .FirstOrDefaultAsync(m => m.ObjectLibraryID == id);
            if (objectLibrary == null)
            {
                return NotFound();
            }

            return View(objectLibrary);
        }

        // POST: ObjectLibraries/Delete/5
        [Authorize(Policy = "Admin")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var objectLibrary = await _context.ObjectLibraries.FindAsync(id);
            _context.ObjectLibraries.Remove(objectLibrary);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ObjectLibraryExists(int id)
        {
            return _context.ObjectLibraries.Any(e => e.ObjectLibraryID == id);
        }
    }
}
