namespace StationarySharp {

    class ScreenTriangle {

        public ScreenPoint[] p = new ScreenPoint[3];
        float z;
        Color24 c;

        public ScreenTriangle(ScreenPoint p0, ScreenPoint p1, ScreenPoint p2, Color24 c) {
	
	        float avg_z, min_z, max_z;

	        max_z = p0.z;
	
	        if(p1.z > max_z)
	            max_z = p1.z;
		
	        if(p2.z > max_z)
	            max_z = p2.z;
		
	        min_z = p0.z;
	
	        if(p1.z < min_z)
	            min_z = p1.z;
		
	        if(p2.z < min_z)
	            min_z = p2.z;
	 	
	        avg_z = (min_z + max_z)/2;
		 
	        this.p[0] = p0;
	        this.p[1] = p1;
            this.p[2] = p2;
	
	        this.z = avg_z;
	        this.c = c;	
        }
    }
}
