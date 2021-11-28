using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ArmaRealMapWebSite.Entities.Maps;
using CoordinateSharp;

namespace ArmaRealMapWebSite.Models
{
    public class GameIssueViewModel
    {
        public Map Map { get; internal set; }
        public double X { get; internal set; }
        public double Y { get; internal set; }
        public Coordinate Place { get; internal set; }
    }
}
