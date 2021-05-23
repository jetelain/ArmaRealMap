using System;

namespace lambertcs
{
	public enum Zone
	{
		LambertI = 0, LambertII = 1 , LambertIII = 2, LambertIV = 3, LambertIIExtended = 4, Lambert93 = 5
	}

	public class LambertZone{

		private readonly static double[] LAMBERT_N = {0.7604059656, 0.7289686274, 0.6959127966, 0.6712679322, 0.7289686274, 0.7256077650};
		private readonly static double[] LAMBERT_C = {11603796.98, 11745793.39, 11947992.52, 12136281.99, 11745793.39, 11754255.426};
		private readonly static double[] LAMBERT_XS = {600000.0, 600000.0, 600000.0, 234.358, 600000.0, 700000.0};
		private readonly static double[] LAMBERT_YS = {5657616.674, 6199695.768, 6791905.085, 7239161.542, 8199695.768, 12655612.050};

		public readonly static double M_PI_2 = Math.PI/2.0;
		public readonly static double DEFAULT_EPS = 1e-10 ;
		public readonly static double E_CLARK_IGN =  0.08248325676  ;
		public readonly static double E_WGS84 =  0.08181919106  ;

		public readonly static double A_CLARK_IGN = 6378249.2 ;
		public readonly static double A_WGS84 =  6378137.0  ;
		public readonly static double LON_MERID_PARIS = 0  ;
		public readonly static double LON_MERID_GREENWICH =0.04079234433 ;
		public readonly static double LON_MERID_IERS = 3.0*Math.PI/180.0;

		public LambertZone(Zone zone){
			this.lambertZone = zone;
		}

		public double n(){
			return LAMBERT_N[(int)lambertZone];
		}

		public  double c(){
			return LAMBERT_C[(int)lambertZone];
		}
		public  double xs(){
			return LAMBERT_XS[(int)lambertZone];
		}
		public  double ys(){
			return LAMBERT_YS[(int)lambertZone];
		}

		public Zone lambertZone{ get; private set;}

	}
}

