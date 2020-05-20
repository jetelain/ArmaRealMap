using System;

namespace lambertcs
{
	public class Lambert
	{
		private static double latitudeFromLatitudeISO(double latISo, double e, double eps) {

			double phi0 = 2 * Math.Atan(Math.Exp(latISo)) - LambertZone.M_PI_2;
			double phiI = 2 * Math.Atan(Math.Pow((1 + e * Math.Sin(phi0)) / (1 - e * Math.Sin(phi0)), e / 2d) * Math.Exp(latISo)) - LambertZone.M_PI_2;
			double delta = Math.Abs(phiI - phi0);

			while (delta > eps) {
				phi0 = phiI;
				phiI = 2 * Math.Atan(Math.Pow((1 + e * Math.Sin(phi0)) / (1 - e * Math.Sin(phi0)), e / 2d) * Math.Exp(latISo)) - LambertZone.M_PI_2;
				delta = Math.Abs(phiI - phi0);
			}

			return phiI;
		}

		private static Point lambertToGeographic(Point org, LambertZone zone, double lonMeridian, double e, double eps)
		{
			double n = zone.n();
			double C = zone.c();
			double xs = zone.xs();
			double ys = zone.ys();

			double x = org.x;
			double y = org.y;


			double lon, gamma, R, latIso;

			R = Math.Sqrt((x - xs) * (x - xs) + (y - ys) * (y - ys));

			gamma = Math.Atan((x-xs)/(ys-y));

			lon = lonMeridian + gamma/n;

			latIso = -1/n*Math.Log(Math.Abs(R/C));

			double lat = latitudeFromLatitudeISO(latIso, e, eps);

			Point dest = new Point(lon,lat,0);
			return dest;
		}

		private static double lambertNormal(double lat, double a, double e)
		{

			return a/Math.Sqrt(1-e*e*Math.Sin(lat)*Math.Sin(lat));
		}

		private static Point geographicToCartesian(double lon, double lat, double he, double a, double e)
		{
			double N = lambertNormal(lat, a, e);

			Point pt = new Point(0,0,0);

			pt.x = (N+he)*Math.Cos(lat)*Math.Cos(lon);
			pt.y = (N+he)*Math.Cos(lat)*Math.Sin(lon);
			pt.z = (N*(1-e*e)+he)*Math.Sin(lat);

			return pt;
		}

		private static Point cartesianToGeographic(Point org, double meridien, double a, double e, double eps)
		{
			double x = org.x, y = org.y, z = org.z;

			double lon = meridien + Math.Atan(y/x);

			double module = Math.Sqrt(x*x + y*y);

			double phi0 = Math.Atan(z/(module*(1-(a*e*e)/Math.Sqrt(x*x+y*y+z*z))));
			double phiI = Math.Atan(z/module/(1-a*e*e*Math.Cos(phi0)/(module * Math.Sqrt(1-e*e*Math.Sin(phi0)*Math.Sin(phi0)))));
			double delta= Math.Abs(phiI - phi0);
			while(delta > eps)
			{
				phi0 = phiI;
				phiI = Math.Atan(z/module/(1-a*e*e*Math.Cos(phi0)/(module * Math.Sqrt(1-e*e*Math.Sin(phi0)*Math.Sin(phi0)))));
				delta= Math.Abs(phiI - phi0);

			}

			double he = module/Math.Cos(phiI) - a/Math.Sqrt(1-e*e*Math.Sin(phiI)*Math.Sin(phiI));

			Point pt = new Point(lon,phiI,he);

			return pt;
		}

		public static Point convertToWGS84(Point org, Zone zone){

			var lzone = new LambertZone (zone);

			if(zone == Zone.Lambert93)
			{
				return lambertToGeographic(org,lzone,LambertZone.LON_MERID_IERS,LambertZone.E_WGS84,LambertZone.DEFAULT_EPS);
			}
			else {
				Point pt1 =  lambertToGeographic(org, lzone, LambertZone.LON_MERID_PARIS, LambertZone.E_CLARK_IGN, LambertZone.DEFAULT_EPS);

				Point pt2 = geographicToCartesian(pt1.x, pt1.y, pt1.z, LambertZone.A_CLARK_IGN, LambertZone.E_CLARK_IGN);

				pt2.translate(-168,-60,320);

				//WGS84 refers to greenwich
				return cartesianToGeographic(pt2, LambertZone.LON_MERID_GREENWICH, LambertZone.A_WGS84, LambertZone.E_WGS84, LambertZone.DEFAULT_EPS);
			}
		}

		public static Point convertToWGS84(double x, double y, Zone zone){

			Point pt = new Point(x,y,0);
			return convertToWGS84(pt, zone);
		}

		public static Point convertToWGS84Deg(double x, double y, Zone zone){

			Point pt = new Point(x,y,0);
			pt = convertToWGS84 (pt, zone);
			pt.toDegree();
			return pt;


		}
	}
}

