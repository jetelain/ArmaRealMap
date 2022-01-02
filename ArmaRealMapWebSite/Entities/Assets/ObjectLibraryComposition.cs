using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ArmaRealMapWebSite.Entities.Assets
{
    public class ObjectLibraryComposition
    {
        public int ObjectLibraryCompositionID { get; set; }

        public int ObjectLibraryID { get; set; }
        public ObjectLibrary ObjectLibrary { get; set; }

        [Display(Name = "Probabilité")]
        [Range(0, 1)]
        public float? Probability { get; set; }

        public float Width { get; set; }
        public float Depth { get; set; }
        public float Height { get; set; }
        public List<ObjectLibraryCompositionAsset> Assets { get; set; }

        [NotMapped]
        public string Definition { get; set; }
    }
}