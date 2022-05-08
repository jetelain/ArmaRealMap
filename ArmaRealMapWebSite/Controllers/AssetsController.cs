using System;
using System.IO;
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
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace ArmaRealMapWebSite.Controllers
{
    public class AssetsController : Controller
    {
        private readonly ArmaRealMapContext _context;

        public AssetsController(ArmaRealMapContext context)
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
            if (vm.Name != null)
            {
                var pattern = "%" + vm.Name + "%";
                assetsContext = assetsContext.Where(a => EF.Functions.Like(a.Name, pattern) || EF.Functions.Like(a.ModelPath, pattern));
            }
            vm.Results = await assetsContext.OrderBy(a => a.Name).Take(1000).ToListAsync();
            vm.Mods = new SelectList(_context.GameMods, "GameModID", "Name");
            vm.DbCount = await _context.Assets.CountAsync();
            return View(vm);
        }

        public async Task<IActionResult> ModelsInfo()
        {
            var alldata = await _context.Assets.Include(a => a.GameMod)
                .ToListAsync();

            return Json(
                alldata
                    .Select(
                    l => new JsonModelInfo()
                    {
                        Name = l.Name,
                        Path = l.ModelPath,
                        BoundingCenterX = l.BoundingCenterX,
                        BoundingCenterY = l.BoundingCenterY,
                        BoundingCenterZ = l.BoundingCenterZ,
                        Bundle = l.GameMod.Name.Replace("Base game", "a3").ToLowerInvariant() + "_" + l.AssetCategory.ToString().ToLowerInvariant()
                    })
                    .ToList(),
                    new JsonSerializerOptions
                    {
                        Converters = { new JsonStringEnumConverter() },
                        WriteIndented = true
                    }
                );
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
        [Authorize(Policy = "Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var asset = await _context.Assets.FindAsync(id);
            if (asset == null)
            {
                return NotFound();
            }
            return View(asset);
        }

        // POST: Assets/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "Admin")]
        public async Task<IActionResult> Edit(int id, [Bind("AssetID,ClassName,CX,CY,CZ,Width,Depth,Height,AssetCategory,ClusterName")] Asset update)
        {
            if (id != update.AssetID)
            {
                return NotFound();
            }
            if (ModelState.IsValid)
            {
                try
                {
                    var asset = await _context.Assets.FindAsync(id);
                    asset.ClassName = update.ClassName;
                    asset.AssetCategory = update.AssetCategory;
                    asset.CX = update.CX;
                    asset.CY = update.CY;
                    asset.CZ = update.CZ;
                    asset.Width = update.Width;
                    asset.Depth = update.Depth;
                    asset.Height = update.Height;
                    asset.ClusterName = update.ClusterName;
                    _context.Update(asset);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AssetExists(update.AssetID))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Details),new { id });
            }
            return View(update);
        }

        // GET: Assets/Delete/5
        [Authorize(Policy = "Admin")]
        public async Task<IActionResult> Delete(int? id)
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

        // POST: Assets/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var asset = await _context.Assets.FindAsync(id);
            _context.Assets.Remove(asset);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool AssetExists(int id)
        {
            return _context.Assets.Any(e => e.AssetID == id);
        }


        [Authorize(Policy = "Admin")]
        public IActionResult UploadModelsInfo()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "Admin")]
        public async Task<IActionResult> UploadModelsInfo(IFormFile file)
        {
            JsonModelInfo[] models;
            using (var reader = new StreamReader(file.OpenReadStream()))
            {
                models = JsonSerializer.Deserialize<JsonModelInfo[]>(reader.ReadToEnd());
            }
            var alldata = await _context.Assets.ToListAsync();
            foreach(var model in models)
            {
                var data = alldata.FirstOrDefault(m => string.Equals(m.ModelPath,model.Path, StringComparison.OrdinalIgnoreCase));
                if (data != null)
                {
                    data.BoundingCenterX = model.BoundingCenterX;
                    data.BoundingCenterY = model.BoundingCenterY;
                    data.BoundingCenterZ = model.BoundingCenterZ;
                    _context.Update(data);
                }
            }
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}
