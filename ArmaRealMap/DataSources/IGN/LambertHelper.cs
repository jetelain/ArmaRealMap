using System;
using System.Collections.Generic;
using System.Text;
using CoordinateSharp;
using lambertcs;

namespace ArmaRealMap
{
    public static class LambertHelper
    {
        public static double[] WGS84ToLambert93(Coordinate coordinate)
        {
            return WGS84ToLambert93(coordinate.Latitude.ToDouble(), coordinate.Longitude.ToDouble());
        }

        public static double[] WGS84ToLambert93(double latitude, double longitude)
        {
            /**** Conversion latitude,longitude en coordonée lambert 93 ****/
            // Projection conforme sécante, algo détailler dans NTG_71.pdf : http://www.ign.fr/affiche_rubrique.asp?rbr_id=1700&lng_id=FR
            //  > ACCUEIL > L'offre IGN Pro > Géodésie > RGF93 > Outils 

            //variables:
            var a = LambertZone.A_WGS84;//6378137; //demi grand axe de l'ellipsoide (m)
            var e = LambertZone.E_WGS84;// 0.08181919106; //première excentricité de l'ellipsoide
            var l0 = (Math.PI / 180) * 3;
            var lc = l0;
            var phi0 = (Math.PI / 180) * 46.5; //latitude d'origine en radian
            var phi1 = (Math.PI / 180) * 44; //1er parallele automécoïque
            var phi2 = (Math.PI / 180) * 49; //2eme parallele automécoïque

            var x0 = 700000; //coordonnées à l'origine
            var y0 = 6600000; //coordonnées à l'origine

            var phi = (Math.PI / 180) * latitude;
            var l = (Math.PI / 180) * longitude;

            //calcul des grandes normales
            var gN1 = a / Math.Sqrt(1 - e * e * Math.Sin(phi1) * Math.Sin(phi1));
            var gN2 = a / Math.Sqrt(1 - e * e * Math.Sin(phi2) * Math.Sin(phi2));

            //calculs des latitudes isométriques
            var gl1 = Math.Log(Math.Tan(Math.PI / 4 + phi1 / 2) * Math.Pow((1 - e * Math.Sin(phi1)) / (1 + e * Math.Sin(phi1)), e / 2));
            var gl2 = Math.Log(Math.Tan(Math.PI / 4 + phi2 / 2) * Math.Pow((1 - e * Math.Sin(phi2)) / (1 + e * Math.Sin(phi2)), e / 2));
            var gl0 = Math.Log(Math.Tan(Math.PI / 4 + phi0 / 2) * Math.Pow((1 - e * Math.Sin(phi0)) / (1 + e * Math.Sin(phi0)), e / 2));
            var gl = Math.Log(Math.Tan(Math.PI / 4 + phi / 2) * Math.Pow((1 - e * Math.Sin(phi)) / (1 + e * Math.Sin(phi)), e / 2));

            //calcul de l'exposant de la projection
            var n = (Math.Log((gN2 * Math.Cos(phi2)) / (gN1 * Math.Cos(phi1)))) / (gl1 - gl2);//ok

            //calcul de la constante de projection
            var c = ((gN1 * Math.Cos(phi1)) / n) * Math.Exp(n * gl1);//ok

            //calcul des coordonnées
            var ys = y0 + c * Math.Exp(-1 * n * gl0);

            var x93 = x0 + c * Math.Exp(-1 * n * gl) * Math.Sin(n * (l - lc));
            var y93 = ys - c * Math.Exp(-1 * n * gl) * Math.Cos(n * (l - lc));


            double[] tabXY = new double[2];

            tabXY[0] = x93;
            tabXY[1] = y93;

            return tabXY;
        }
    }
}
