using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ArmaRealMapWebSite.Entities.Assets;
using ArmaRealMap.Core.ObjectLibraries;
using ArmaRealMapWebSite.Entities;
using Microsoft.AspNetCore.Authorization;
using ArmaRealMap.Core;

namespace ArmaRealMapWebSite.Controllers
{
    public class ObjectLibraryAssetsController : Controller
    {
        private readonly ArmaRealMapContext _context;

        public ObjectLibraryAssetsController(ArmaRealMapContext context)
        {
            _context = context;
        }

        // GET: ObjectLibraryAssets/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var objectLibraryAsset = await _context.ObjectLibraryAssets
                .Include(o => o.Asset)
                .Include(o => o.ObjectLibrary)
                .FirstOrDefaultAsync(m => m.ObjectLibraryAssetID == id);
            if (objectLibraryAsset == null)
            {
                return NotFound();
            }

            return View(objectLibraryAsset);
        }

        // GET: ObjectLibraryAssets/Create

        [Authorize(Policy = "Admin")]
        public async Task<IActionResult> Create(int objectLibraryID)
        {
            var objectLibraryAsset = new ObjectLibraryAsset();
            objectLibraryAsset.ObjectLibraryID = objectLibraryID;
            await Prepare(objectLibraryAsset);
            return View(objectLibraryAsset);
        }

        private async Task Prepare(ObjectLibraryAsset objectLibraryAsset)
        {
            objectLibraryAsset.ObjectLibrary = await _context.ObjectLibraries.FindAsync(objectLibraryAsset.ObjectLibraryID);

            //ViewData["AssetID"] = new SelectList(await _context.Assets
            //    .Where(a => (a.TerrainRegions & objectLibraryAsset.ObjectLibrary.TerrainRegion) != 0)
            //    .ToListAsync(), 
            //    "AssetID",
            //    "Name", 
            //    objectLibraryAsset.AssetID);

            if (objectLibraryAsset.ObjectLibrary.TerrainRegion != TerrainRegion.Unknown)
            {
                ViewData["RegionAssets"] = await _context.Assets.Where(a => (a.TerrainRegions & objectLibraryAsset.ObjectLibrary.TerrainRegion) != 0).ToListAsync();
            }
            else
            {
                ViewData["RegionAssets"] = await _context.Assets.ToListAsync();
            }
            ViewData["LibraryAssets"] = await _context.ObjectLibraryAssets.Where(a => a.ObjectLibraryID == objectLibraryAsset.ObjectLibraryID && a.ObjectLibraryAssetID != objectLibraryAsset.ObjectLibraryAssetID).Select(a => a.Asset).ToListAsync();

        }

        // POST: ObjectLibraryAssets/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "Admin")]
        public async Task<IActionResult> Create([Bind("ObjectLibraryAssetID,ObjectLibraryID,AssetID,Probability,PlacementRadius,ReservedRadius,MaxZ,MinZ")] ObjectLibraryAsset objectLibraryAsset)
        {
            if (ModelState.IsValid)
            {
                await FillInfos(objectLibraryAsset);
                _context.Add(objectLibraryAsset);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(ObjectLibrariesController.Details), "ObjectLibraries", new { id = objectLibraryAsset.ObjectLibraryID });
            }
            await Prepare(objectLibraryAsset);
            return View(objectLibraryAsset);
        }

        private async Task FillInfos(ObjectLibraryAsset objectLibraryAsset)
        {
            objectLibraryAsset.ObjectLibrary = await _context.ObjectLibraries.FindAsync(objectLibraryAsset.ObjectLibraryID);

            if (objectLibraryAsset.PlacementRadius == null && objectLibraryAsset.ObjectLibrary.ObjectCategory.HasPlacementRadius())
            {
                objectLibraryAsset.Asset = await _context.Assets.FindAsync(objectLibraryAsset.AssetID);
                objectLibraryAsset.PlacementRadius = (float)Math.Round(Math.Max(objectLibraryAsset.Asset.Width, objectLibraryAsset.Asset.Depth) / 2, 1);
            }
        }

        // GET: ObjectLibraryAssets/Edit/5

        [Authorize(Policy = "Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var objectLibraryAsset = await _context.ObjectLibraryAssets.FindAsync(id);
            if (objectLibraryAsset == null)
            {
                return NotFound();
            }

            await Prepare(objectLibraryAsset);
            return View(objectLibraryAsset);
        }

        // POST: ObjectLibraryAssets/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "Admin")]
        public async Task<IActionResult> Edit(int id, [Bind("ObjectLibraryAssetID,ObjectLibraryID,AssetID,Probability,PlacementRadius,ReservedRadius,MaxZ,MinZ")] ObjectLibraryAsset objectLibraryAsset)
        {
            if (id != objectLibraryAsset.ObjectLibraryAssetID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    await FillInfos(objectLibraryAsset);
                    _context.Update(objectLibraryAsset);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ObjectLibraryAssetExists(objectLibraryAsset.ObjectLibraryAssetID))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(ObjectLibrariesController.Details), "ObjectLibraries",new { id = objectLibraryAsset.ObjectLibraryID });
            }
            await Prepare(objectLibraryAsset);
            return View(objectLibraryAsset);
        }

        // GET: ObjectLibraryAssets/Delete/5
        [Authorize(Policy = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var objectLibraryAsset = await _context.ObjectLibraryAssets
                .Include(o => o.Asset)
                .Include(o => o.ObjectLibrary)
                .FirstOrDefaultAsync(m => m.ObjectLibraryAssetID == id);
            if (objectLibraryAsset == null)
            {
                return NotFound();
            }

            return View(objectLibraryAsset);
        }

        // POST: ObjectLibraryAssets/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var objectLibraryAsset = await _context.ObjectLibraryAssets.FindAsync(id);
            _context.ObjectLibraryAssets.Remove(objectLibraryAsset);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(ObjectLibrariesController.Details), "ObjectLibraries", new { id = objectLibraryAsset.ObjectLibraryID });
        }

        private bool ObjectLibraryAssetExists(int id)
        {
            return _context.ObjectLibraryAssets.Any(e => e.ObjectLibraryAssetID == id);
        }
    }
}
