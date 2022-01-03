using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using ArmaRealMapWebSite.Entities;
using ArmaRealMapWebSite.Entities.Assets;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ArmaRealMapWebSite.Controllers
{
    public class ObjectLibraryCompositionsController : Controller
    {
        private readonly ArmaRealMapContext _context;

        public ObjectLibraryCompositionsController(ArmaRealMapContext context)
        {
            _context = context;
        }

        // GET: ObjectLibraryCompositions/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var objectLibraryComposition = await _context.ObjectLibraryComposition
                .Include(o => o.ObjectLibrary)
                .FirstOrDefaultAsync(m => m.ObjectLibraryCompositionID == id);
            if (objectLibraryComposition == null)
            {
                return NotFound();
            }

            return View(objectLibraryComposition);
        }

        // GET: ObjectLibraryCompositions/Create
        [Authorize(Policy = "Admin")]
        public async Task<IActionResult> Create(int objectLibraryID)
        {
            var objectLibraryComposition = new ObjectLibraryComposition();
            objectLibraryComposition.ObjectLibraryID = objectLibraryID;
            await Prepare(objectLibraryComposition);
            return View(objectLibraryComposition);
        }
        private async Task Prepare(ObjectLibraryComposition objectLibraryComposition)
        {
            objectLibraryComposition.ObjectLibrary = await _context.ObjectLibraries.FindAsync(objectLibraryComposition.ObjectLibraryID);
        }

        // POST: ObjectLibraryCompositions/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "Admin")]
        public async Task<IActionResult> Create([Bind("ObjectLibraryCompositionID,ObjectLibraryID,Probability,Width,Depth,Height,Definition")] ObjectLibraryComposition objectLibraryComposition)
        {
            objectLibraryComposition.Assets = await ProcessDefinition(objectLibraryComposition);
            if (ModelState.IsValid)
            {
                _context.Add(objectLibraryComposition);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(ObjectLibrariesController.Details), "ObjectLibraries", new { id = objectLibraryComposition.ObjectLibraryID });
            }
            await Prepare(objectLibraryComposition);
            return View(objectLibraryComposition);
        }

        private async Task<List<ObjectLibraryCompositionAsset>> ProcessDefinition(ObjectLibraryComposition objectLibraryComposition)
        {
            var assets = new List<ObjectLibraryCompositionAsset>();
            float maxX = float.MinValue, maxY = float.MinValue, minX = float.MaxValue, minY = float.MaxValue;

            foreach(var line in objectLibraryComposition.Definition.Split("\n"))
            {
                var tokens = line.Trim().Split(';');
                if (tokens.Length > 7)
                {
                    var name = tokens[0].Trim().Trim('"');
                    var asset = await _context.Assets.FirstOrDefaultAsync(a => a.Name == name);
                    if (asset == null)
                    {
                        ModelState.AddModelError("Definition", $"L'objet '{name}' est inconnu.");
                        return assets;
                    }
                    var x = float.Parse(tokens[1], CultureInfo.InvariantCulture);
                    var y = float.Parse(tokens[2], CultureInfo.InvariantCulture);
                    var angle = float.Parse(tokens[3], CultureInfo.InvariantCulture);
                    var z = float.Parse(tokens[7], CultureInfo.InvariantCulture) - 5f;
                    // TODO: Take into account angle on max/min
                    maxX = Math.Max(maxX, x - asset.CX + (asset.Width / 2));
                    maxY = Math.Max(maxY, y - asset.CY + (asset.Depth / 2));
                    minX = Math.Min(minX, x - asset.CX - (asset.Width / 2));
                    minY = Math.Min(minY, y - asset.CY - (asset.Depth / 2));
                    assets.Add(new ObjectLibraryCompositionAsset() { Angle = angle, Asset = asset, AssetID = asset.AssetID, Composition = objectLibraryComposition, X = x, Y = y, Z = z });
                }
            }
            if (maxX > 100000)
            {
                var cx = (maxX + minX) / 2;
                var cy = (maxY + minY) / 2;
                foreach (var asset in assets)
                {
                    asset.X -= cx;
                    asset.Y -= cy;
                }
            }
            if (objectLibraryComposition.Width == 0f)
            {
                objectLibraryComposition.Width = (maxX - minX);
            }
            if (objectLibraryComposition.Depth == 0f)
            {
                objectLibraryComposition.Depth = (maxY - minY);
            }
            if (objectLibraryComposition.Height == 0f)
            {
                objectLibraryComposition.Height = assets.Max(a => a.Asset.Height + a.Y);
            }
            return assets;
        }

        // GET: ObjectLibraryCompositions/Edit/5
        [Authorize(Policy = "Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var objectLibraryComposition = await _context.ObjectLibraryComposition.FindAsync(id);
            if (objectLibraryComposition == null)
            {
                return NotFound();
            }
            var assets = await _context.ObjectLibraryCompositionAssets.Where(a => a.ObjectLibraryCompositionID == objectLibraryComposition.ObjectLibraryCompositionID).Include(a => a.Asset).ToListAsync();
            objectLibraryComposition.Definition = string.Join('\n', assets.Select(a => FormattableString.Invariant($"\"{a.Asset.Name}\";{a.X};{a.Y};{a.Angle};0;0;1;{a.Z+5f}")));
            await Prepare(objectLibraryComposition);
            return View(objectLibraryComposition);
        }

        // POST: ObjectLibraryCompositions/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "Admin")]
        public async Task<IActionResult> Edit(int id, [Bind("ObjectLibraryCompositionID,ObjectLibraryID,Probability,Width,Depth,Height,Definition")] ObjectLibraryComposition objectLibraryComposition)
        {
            if (id != objectLibraryComposition.ObjectLibraryCompositionID)
            {
                return NotFound();
            }
            var assets = await ProcessDefinition(objectLibraryComposition);

            if (ModelState.IsValid)
            {
                try
                {
                    await UpdateAssets(objectLibraryComposition, assets);
                    _context.Update(objectLibraryComposition);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ObjectLibraryCompositionExists(objectLibraryComposition.ObjectLibraryCompositionID))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(ObjectLibrariesController.Details), "ObjectLibraries", new { id = objectLibraryComposition.ObjectLibraryID });
            }
            await Prepare(objectLibraryComposition);
            return View(objectLibraryComposition);
        }

        private async Task UpdateAssets(ObjectLibraryComposition objectLibraryComposition, List<ObjectLibraryCompositionAsset> assets)
        {
            objectLibraryComposition.Assets = await _context.ObjectLibraryCompositionAssets.Where(a => a.ObjectLibraryCompositionID == objectLibraryComposition.ObjectLibraryCompositionID).ToListAsync();
            if (assets.Count < objectLibraryComposition.Assets.Count)
            {
                foreach (var remove in objectLibraryComposition.Assets.Skip(assets.Count))
                {
                    _context.ObjectLibraryCompositionAssets.Remove(remove);
                }
                objectLibraryComposition.Assets = objectLibraryComposition.Assets.Take(assets.Count).ToList();
            }
            for(var i = 0; i < objectLibraryComposition.Assets.Count; ++i)
            {
                var target = objectLibraryComposition.Assets[i];
                var source = assets[i];
                target.Angle = source.Angle;
                target.X = source.X;
                target.Y = source.Y;
                target.Z = source.Z;
                target.AssetID = source.AssetID;
                target.Asset = source.Asset;
                _context.ObjectLibraryCompositionAssets.Update(target);
            }
            if (assets.Count > objectLibraryComposition.Assets.Count)
            {
                foreach (var add in assets.Skip(objectLibraryComposition.Assets.Count))
                {
                    add.ObjectLibraryCompositionID = objectLibraryComposition.ObjectLibraryCompositionID;
                    _context.ObjectLibraryCompositionAssets.Add(add);
                    objectLibraryComposition.Assets.Add(add);
                }
            }
        }

        // GET: ObjectLibraryCompositions/Delete/5
        [Authorize(Policy = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var objectLibraryComposition = await _context.ObjectLibraryComposition
                .Include(o => o.ObjectLibrary)
                .FirstOrDefaultAsync(m => m.ObjectLibraryCompositionID == id);
            if (objectLibraryComposition == null)
            {
                return NotFound();
            }

            return View(objectLibraryComposition);
        }

        // POST: ObjectLibraryCompositions/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var objectLibraryComposition = await _context.ObjectLibraryComposition.FindAsync(id);
            _context.ObjectLibraryComposition.Remove(objectLibraryComposition);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(ObjectLibrariesController.Details), "ObjectLibraries", new { id = objectLibraryComposition.ObjectLibraryID });
        }

        private bool ObjectLibraryCompositionExists(int id)
        {
            return _context.ObjectLibraryComposition.Any(e => e.ObjectLibraryCompositionID == id);
        }
    }
}
