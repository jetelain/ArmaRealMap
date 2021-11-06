using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ArmaRealMapWebSite.Entities.Assets;
using ArmaRealMapWebSite.Models;
using Microsoft.AspNetCore.Authorization;
using ArmaRealMap.Core.ObjectLibraries;

namespace ArmaRealMapWebSite.Controllers
{
    public class AssetsController : Controller
    {
        private readonly AssetsContext _context;

        public AssetsController(AssetsContext context)
        {
            _context = context;
        }

        // GET: Assets
        //public async Task<IActionResult> Index()
        //{
        //    return await Index(new AssetsViewModel());
        //}

        public async Task<IActionResult> Index(AssetsViewModel vm)
        {
            IQueryable<Asset> assetsContext = _context.Assets.Include(a => a.GameMod);
            if (vm.GameModID != null)
            {
                assetsContext = assetsContext.Where(a => a.GameModID == vm.GameModID);
            }
            if (vm.AssetCategory != null)
            {
                assetsContext = assetsContext.Where(a => a.AssetCategory == vm.AssetCategory);
            }
            if (vm.TerrainRegion != null)
            {
                assetsContext = assetsContext.Where(a => (a.TerrainRegions & vm.TerrainRegion) == vm.TerrainRegion);
            }
            vm.Results = await assetsContext.ToListAsync();
            vm.Mods = new SelectList(_context.GameMods, "GameModID", "Name");
            return View(vm);
        }


        // GET: Assets/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var asset = await _context.Assets
                .Include(a => a.GameMod)
                .FirstOrDefaultAsync(m => m.AssetID == id);
            if (asset == null)
            {
                return NotFound();
            }

            return View(asset);
        }

        [Authorize(Policy = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddRegion(int id, TerrainRegion region)
        {
            var asset = await _context.Assets.FirstOrDefaultAsync(m => m.AssetID == id);
            if (asset == null)
            {
                return NotFound();
            }
            asset.TerrainRegions |= region;
            _context.Update(asset);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Details),new { id });
        }

        [Authorize(Policy = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveRegion(int id, TerrainRegion region)
        {
            var asset = await _context.Assets.FirstOrDefaultAsync(m => m.AssetID == id);
            if (asset == null)
            {
                return NotFound();
            }
            asset.TerrainRegions &= ~region;
            _context.Update(asset);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Details), new { id });
        }

        [Authorize(Policy = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SetCategory(int id, AssetCategory assetCategory)
        {
            var asset = await _context.Assets.FirstOrDefaultAsync(m => m.AssetID == id);
            if (asset == null)
            {
                return NotFound();
            }
            asset.AssetCategory = assetCategory;
            _context.Update(asset);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Details), new { id });
        }

        [ResponseCache(Duration = 604800)]
        public async Task<IActionResult> Preview(int id, int size)
        {
            var asset = await _context.AssetPreviews
                .OrderByDescending(m => m.Width)
                .FirstOrDefaultAsync(m => m.AssetID == id && m.Width <= size);
            if (asset == null)
            {
                return NotFound();
            }
            return File(asset.Data, asset.Width == 1920 ? "image/jpeg" : "image/png");
        }

        //// GET: Assets/Create
        //public IActionResult Create()
        //{
        //    ViewData["GameModID"] = new SelectList(_context.GameMods, "GameModID", "GameModID");
        //    return View();
        //}

        //// POST: Assets/Create
        //// To protect from overposting attacks, enable the specific properties you want to bind to.
        //// For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> Create([Bind("AssetID,Name,ClassName,ModelPath,Width,Depth,Height,CX,CY,CZ,TerrainRegions,AssetCategory,TerrainBuilderTemplateXML,GameModID,MaxZ,MaxY,MaxX,MinZ,MinY,MinX,BoundingSphereDiameter")] Asset asset)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        _context.Add(asset);
        //        await _context.SaveChangesAsync();
        //        return RedirectToAction(nameof(Index));
        //    }
        //    ViewData["GameModID"] = new SelectList(_context.GameMods, "GameModID", "GameModID", asset.GameModID);
        //    return View(asset);
        //}

        //// GET: Assets/Edit/5
        //public async Task<IActionResult> Edit(int? id)
        //{
        //    if (id == null)
        //    {
        //        return NotFound();
        //    }

        //    var asset = await _context.Assets.FindAsync(id);
        //    if (asset == null)
        //    {
        //        return NotFound();
        //    }
        //    ViewData["GameModID"] = new SelectList(_context.GameMods, "GameModID", "GameModID", asset.GameModID);
        //    return View(asset);
        //}

        //// POST: Assets/Edit/5
        //// To protect from overposting attacks, enable the specific properties you want to bind to.
        //// For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> Edit(int id, [Bind("AssetID,Name,ClassName,ModelPath,Width,Depth,Height,CX,CY,CZ,TerrainRegions,AssetCategory,TerrainBuilderTemplateXML,GameModID,MaxZ,MaxY,MaxX,MinZ,MinY,MinX,BoundingSphereDiameter")] Asset asset)
        //{
        //    if (id != asset.AssetID)
        //    {
        //        return NotFound();
        //    }

        //    if (ModelState.IsValid)
        //    {
        //        try
        //        {
        //            _context.Update(asset);
        //            await _context.SaveChangesAsync();
        //        }
        //        catch (DbUpdateConcurrencyException)
        //        {
        //            if (!AssetExists(asset.AssetID))
        //            {
        //                return NotFound();
        //            }
        //            else
        //            {
        //                throw;
        //            }
        //        }
        //        return RedirectToAction(nameof(Index));
        //    }
        //    ViewData["GameModID"] = new SelectList(_context.GameMods, "GameModID", "GameModID", asset.GameModID);
        //    return View(asset);
        //}

        //// GET: Assets/Delete/5
        //public async Task<IActionResult> Delete(int? id)
        //{
        //    if (id == null)
        //    {
        //        return NotFound();
        //    }

        //    var asset = await _context.Assets
        //        .Include(a => a.GameMod)
        //        .FirstOrDefaultAsync(m => m.AssetID == id);
        //    if (asset == null)
        //    {
        //        return NotFound();
        //    }

        //    return View(asset);
        //}

        //// POST: Assets/Delete/5
        //[HttpPost, ActionName("Delete")]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> DeleteConfirmed(int id)
        //{
        //    var asset = await _context.Assets.FindAsync(id);
        //    _context.Assets.Remove(asset);
        //    await _context.SaveChangesAsync();
        //    return RedirectToAction(nameof(Index));
        //}

        //private bool AssetExists(int id)
        //{
        //    return _context.Assets.Any(e => e.AssetID == id);
        //}
    }
}
