using System;


namespace lambertcs
{	
	public enum Unit{Degree, Grad, Radian, Meter};

	public class Point
	{
	
		private const double radianTodegree = 180.0/Math.PI;

		public Point (double x, double y, double z)
		{
			this.x = x;
			this.y = y;
			this.z = z;
		}
			
		public double x { get; set;}
		public double y { get; set;}
		public double z { get; set;}


		public void translate(double x , double y, double z){

			this.x+= x;
			this.y+= y;
			this.z+= z;
		}
		private void Scale(double scale){
			this.x *= scale;
			this.y *= scale;
			this.z *= scale;
		}

		public void toDegree(){
			Scale (radianTodegree);
		}
	}
}

