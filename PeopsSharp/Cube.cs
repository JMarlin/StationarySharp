using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace StationarySharp {

    class Cube : Object3D {

        public Cube(float s, Color24 c) : base() {

            Vertex[] temp_v = new Vertex[] {
                new Vertex(), new Vertex(), new Vertex(), new Vertex()
            };

            float half_s = s / 2;

            float[][] points =  new float[][] {
                new float[] {-half_s,  half_s, -half_s},
                new float[] { half_s,  half_s, -half_s},
                new float[] { half_s, -half_s, -half_s},
                new float[] {-half_s, -half_s, -half_s},
                new float[] {-half_s,  half_s,  half_s},
                new float[] { half_s,  half_s,  half_s},
                new float[] { half_s, -half_s,  half_s},
                new float[] {-half_s, -half_s,  half_s}
            };


            int[][] order = new int[][] {
                new int[] {7, 6, 5, 4},
                new int[] {3, 0, 1, 2},
                new int[] {4, 5, 1, 0},
                new int[] {6, 7, 3, 2},
                new int[] {6, 2, 1, 5},
                new int[] {7, 4, 0, 3}
            };

            TextureCoordinate[] uvs = new TextureCoordinate[]
            {
                new TextureCoordinate() { u = 0, v = 0 },
                new TextureCoordinate() { u = 8, v = 0 },
                new TextureCoordinate() { u = 8, v = 8 },
                new TextureCoordinate() { u = 0, v = 8 },
            };
    
            for(int i = 0; i < 6; i++) {
     
                temp_v[0].x = points[order[i][0]][0];
                temp_v[0].y = points[order[i][0]][1];
                temp_v[0].z = points[order[i][0]][2];

                temp_v[1].x = points[order[i][1]][0];
                temp_v[1].y = points[order[i][1]][1];
                temp_v[1].z = points[order[i][1]][2];

                temp_v[2].x = points[order[i][2]][0];
                temp_v[2].y = points[order[i][2]][1];
                temp_v[2].z = points[order[i][2]][2];

                temp_v[3].x = points[order[i][3]][0];
                temp_v[3].y = points[order[i][3]][1];
                temp_v[3].z = points[order[i][3]][2];

                this.AddPolygon(temp_v[0].Clone(), uvs[0].Clone(), temp_v[1].Clone(), uvs[1].Clone(), temp_v[2].Clone(), uvs[2].Clone(), c);
                this.AddPolygon(temp_v[0].Clone(), uvs[0].Clone(), temp_v[2].Clone(), uvs[2].Clone(), temp_v[3].Clone(), uvs[3].Clone(), c);
            }
        }
    }
}
