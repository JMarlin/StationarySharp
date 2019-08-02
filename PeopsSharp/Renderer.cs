﻿using System;
using System.Threading;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Collections.Generic;

namespace StationarySharp {

    class Renderer {

        private float _FocalLength = 0.0f;
        private Thread _RenderThread;
        private Form _TargetForm;
        private WNDPROC _wndproc;
        private GAMELOGIC _gameLogic;
        public List<Polygon> _ZList = new List<Polygon>();

        public List<Object3D> Objects = new List<Object3D>();

        public delegate bool GAMELOGIC();
        public delegate IntPtr WNDPROC(IntPtr hwnd, int message, IntPtr wparam, IntPtr lparam);

        [DllImport("gpuPeopsSoft.dll", CallingConvention = CallingConvention.StdCall)]
        private static extern long GPUinit();

        [DllImport("gpuPeopsSoft.dll")]
        private static extern long GPUopen(IntPtr hWnd, WNDPROC new_wndproc);

        [DllImport("gpuPeopsSoft.dll")]
        private static extern void GPU_Update();

        [DllImport("gpuPeopsSoft.dll")]
        private static extern void GPUwriteStatus(UInt32 data);

        [DllImport("gpuPeopsSoft.dll")]
        private static extern void GPUwriteData(UInt32 data);

        private static UInt32 GPU_DATA_CMD(UInt32 c, UInt32 o) {

            return ((((c) & 0x7) << 5) | ((o) & 0x1F));
        }

        private const UInt16 PRIM_POLY = 0x01;
        private const UInt16 PRIM_LINE = 0x02;
        private const UInt16 PRIM_SPRITE = 0x04;
        private const UInt16 OPT_TGE = 0x01;
        private const UInt16 OPT_ABE = 0x02;
        private const UInt16 OPT_TME = 0x04;
        private const UInt16 OPT_VTX = 0x08;
        private const UInt16 OPT_IIP = 0x10;

        private const UInt16 SCREEN_WIDTH = 320;
        private const UInt16 SCREEN_HEIGHT = 240;
        private const UInt32 SCREEN_PIXELS = SCREEN_WIDTH * SCREEN_HEIGHT;
        private const float SCREEN_DEPTH = 100.0f;

        //Convert a point scaled such that 1.0, 1.0 is at the upper right-hand
        //corner of the screen and -1.0, -1.0 is at the bottom right to pixel coords
        private UInt16 TO_SCREEN_Y(float y) { return ((UInt16)((SCREEN_HEIGHT - (y* SCREEN_HEIGHT)) / 2.0)); }
        private UInt16 TO_SCREEN_X(float x) { return ((UInt16)((SCREEN_WIDTH + (x* SCREEN_HEIGHT)) / 2.0)); }

        private void _RenderLoop() {

            while(this._gameLogic()) {

                this._ClearFramebuffer(Color15.FromRGB(0x0, 0x0, 0x1D).raw_value);

                foreach (Object3D object3d in this.Objects)
                    this.ObjectRender(object3d);

                _ZList.Sort((t1, t2) => { float val = t2.z - t1.z; return val < 0 ? -1 : val > 0 ? 1 : 0; });
                foreach (Polygon triangle in _ZList)
                    this._DrawTriangle(triangle);
                _ZList.Clear();
                GPU_Update();
            }
        }

        public Renderer(Form target_form, WNDPROC window_proc, GAMELOGIC new_game_logic) {

            this._TargetForm = target_form;
            this._wndproc = window_proc;
            this._gameLogic = new_game_logic;

            Renderer.GPUinit();
            Renderer.GPUopen(this._TargetForm.Handle, window_proc);
            this._DoGPUStartup();
            this.SetFOV(45);

            this._RenderThread = new Thread(this._RenderLoop);
            this._RenderThread.Start();
        }

        public void Exit() {

            if (this._RenderThread.IsAlive)
                this._RenderThread.Abort();
        }

        public static float DegreeToRad(float degrees) {

            return (degrees * (float)Math.PI) / 180.0f;
        }

        public void SetFOV(float fov_angle) {
            
            this._FocalLength = 1.0f / (2.0f * (float)Math.Tan(DegreeToRad(fov_angle) / 2.0f));
        }

        private int _DoGPUStartup() {

            GPUwriteStatus(0x00000000); //Reset GPU
            GPUwriteStatus(0x06CDA1F4); //Set horizontal display range to the max and min values of 0x1F4 - 0xCDA
            GPUwriteStatus(0x07040010); //Set vertical display range to the default NTSC values of 0x010-0x100	 
            GPUwriteStatus(0x08000001); //Set video mode to NTSC 15-bit non-interlaced 320x240
            GPUwriteStatus(0x05000000); //Set x/y start of display area to (0,0)
            GPUwriteData(0xE1000300); //Set draw mode
            GPUwriteData(0xE3000000); //Set framebuffer drawing area top left to (0, 0)
            GPUwriteData(0xE403C140); //Set framebuffer drawing area bottom right to (320, 240)
            GPUwriteData(0xE5000000); //Set framebuffer drawing area offset to
            GPUwriteStatus(0x03000000); //Enable the GPU

            return 1; 
        }

        private void _DrawTri(UInt16 x0, UInt16 y0, UInt16 x1, UInt16 y1, UInt16 x2, UInt16 y2, byte r, byte g, byte b) {

            //Poly, one color, flat shaded
            GPUwriteData(
                (GPU_DATA_CMD(PRIM_POLY, 0) << 24) |
                (UInt32)b << 16 |
                (UInt32)g << 8 |
                (UInt32)r
            );

            //Vertex 1
            GPUwriteData(((UInt32)y0 << 16) | (UInt32)x0);

            //Vertex 2
            GPUwriteData(((UInt32)y1 << 16) | (UInt32)x1);

            //Vertex 3
            GPUwriteData(((UInt32)y2 << 16) | (UInt32)x2);
        }

        private void _UploadImageData(UInt32[] data, UInt16 x, UInt16 y, UInt16 h, UInt16 w) {
	
	        int pixel_count = (h * w) >> 1;
	        int i;
	
	        GPUwriteData(0x01000000); //Reset the command buffer
	        GPUwriteData(0xA0000000); //Copy image data to GPU command
	        GPUwriteData(((UInt32)y << 16) | (UInt32)x); //Send x and y of destination
	        GPUwriteData(((UInt32)h << 16) | (UInt32)w); //Send h and w of image
	
	        for(i = 0; i < pixel_count; i++)
	            GPUwriteData(data[i]);
        }

        private void _ClearFramebuffer(UInt16 val) {
	
	        int pixel_count = (1024 * 512) >> 1;
	        int i;
	
	        GPUwriteData(0x01000000); //Reset the command buffer
	        GPUwriteData(0xA0000000); //Copy image data to GPU command
	        GPUwriteData(0x00000000); //Send x and y of destination
	        GPUwriteData((512 << 16) | 1024); //Send h and w of image
	
	        for(i = 0; i < pixel_count; i++)
	            GPUwriteData(((UInt32)val << 16) | (UInt32)val);
        }

        //Make sure our texture is in the vram
        private static UInt32[] texture = new UInt32[] {
                0x80008000, 0x80008000, 0x80008000, 0x801F8000,
                0x80008000, 0xFFFFFFFF, 0xFFFFFFFF, 0x8000FFFF,
                0xFFFF8000, 0xFFFF8000, 0xFFFFFFFF, 0x8000FFFF,
                0xFFFF8000, 0x8000FFFF, 0xFFFFFFFF, 0x8000FFFF,
                0xFFFF8000, 0xFFFFFFFF, 0xFFFF8000, 0x8000FFFF,
                0xFFFF8000, 0xFFFFFFFF, 0x8000FFFF, 0x8000FFFF,
                0xFFFF8000, 0xFFFFFFFF, 0xFFFFFFFF, 0x80008000,
                0x800083E0, 0x80008000, 0x80008000, 0x80008000
            };

        private void _DrawTriTextured(UInt16 x0, UInt16 y0, byte v0, byte u0, 
                                      UInt16 x1, UInt16 y1, byte v1, byte u1, 
                                      UInt16 x2, UInt16 y2, byte v2, byte u2, 
                                      Color24 c) {
	
	        this._UploadImageData(texture, 0, 256, 8, 8);
	
            //Poly, one color, flat shaded
            GPUwriteData((GPU_DATA_CMD(PRIM_POLY, OPT_TME) << 24) | (c.raw_value & 0x00FFFFFF));
    
            //Vertex 1
            GPUwriteData(((UInt32)y0 << 16) | (UInt32)x0);
    
	        //Clut ID and texture location 1
	        GPUwriteData(0x00000000 | ((UInt32)v0 << 8) | ((UInt32)u0)); //no CLUT, v = 8, u = 8
	
            //Vertex 2
            GPUwriteData(((UInt32)y1 << 16) | (UInt32)x1);
    
	        //Texture page info and second texture location
	        GPUwriteData(0x01900000 | ((UInt32)v1 << 8) | ((UInt32)u1)); //Use 15-bit direct texture at (0,256) -- v = 8, u = 0
	
            //Vertex 3
            GPUwriteData(((UInt32)y2 << 16) | (UInt32)x2);
	
	        //Third texture location
	        GPUwriteData(0x00000000 | ((UInt32)v2 << 8) | ((UInt32)u2)); //v = 0, u = 0
        }

        private void _DrawQuadTextured(UInt16 x0, UInt16 y0, byte v0, byte u0, 
                                       UInt16 x1, UInt16 y1, byte v1, byte u1,
                                       UInt16 x2, UInt16 y2, byte v2, byte u2,
                                       UInt16 x3, UInt16 y3, byte v3, byte u3, Color24 c) {

            this._UploadImageData(texture, 0, 256, 8, 8);
	
            //Poly, one color, flat shaded
            GPUwriteData(((UInt32)0x2C << 24) | c.raw_value);
    
            //Vertex 1
            GPUwriteData(((UInt32)y0 << 16) | (UInt32)x0);

            //Clut ID and texture location 1
            GPUwriteData(0x00000000 | ((UInt32)v0 << 8) | ((UInt32)u0)); //no CLUT, v = 8, u = 8

            //Vertex 2
            GPUwriteData(((UInt32)y1 << 16) | (UInt32)x1);

            //Texture page info and second texture location
            GPUwriteData(0x01900000 | ((UInt32)v1 << 8) | ((UInt32)u1)); //Use 15-bit direct texture at (0,256) -- v = 8, u = 0

            //Vertex 3
            GPUwriteData(((UInt32)y2 << 16) | (UInt32)x2);

            //Third texture location
            GPUwriteData(0x00000000 | ((UInt32)v2 << 8) | ((UInt32)u2)); //v = 0, u = 0

            //Vertex 4
            GPUwriteData(((UInt32)y3 << 16) | (UInt32)x3);

            //Fourth texture location
            GPUwriteData(0x00000000 | ((UInt32)v3 << 8) | ((UInt32)u3)); //v = 0, u = 0
        }

        private void _Project(Vertex v, ref ScreenPoint p) {

            float delta = (v.z == 0.0f) ? 1.0f : (this._FocalLength/v.z);

            p.x = TO_SCREEN_X(v.x * delta);
            p.y = TO_SCREEN_Y(v.y * delta);
            p.z = v.z;
        }

        private void _DrawTriangle(Polygon polygon) {
    
            int i;
            ScreenPoint[] p = new ScreenPoint[4] { new ScreenPoint(), new ScreenPoint(), new ScreenPoint(), new ScreenPoint() };
            float[] vec_a = new float[3];
            float[] vec_b = new float[3];
            float[] cross = new float[3];
            float mag;
            float normal_angle;
            float lighting_pct;
            float r, g, b;
    
            //Don't draw the polygon if it's offscreen (This is redundant if this is being called by omit_z_and_render)
            if(polygon.v[0].z < 0 && polygon.v[1].z < 0 && polygon.v[2].z < 0)
                return;
    
            //Calculate the surface normal
            //subtract 3 from 2 and 1, translating it to the origin
            vec_a[0] = polygon.v[0].x - polygon.v[2].x;
            vec_a[1] = polygon.v[0].y - polygon.v[2].y;
            vec_a[2] = polygon.v[0].z - polygon.v[2].z;
            vec_b[0] = polygon.v[1].x - polygon.v[2].x;
            vec_b[1] = polygon.v[1].y - polygon.v[2].y;
            vec_b[2] = polygon.v[1].z - polygon.v[2].z;
    
            //calculate the cross product using 1 as vector a and 2 as vector b
            cross[0] = vec_a[1]*vec_b[2] - vec_a[2]*vec_b[1];
            cross[1] = vec_a[2]*vec_b[0] - vec_a[0]*vec_b[2];
            cross[2] = vec_a[0]*vec_b[1] - vec_a[1]*vec_b[0]; 
    
            //normalize the result vector
            mag = (float)Math.Sqrt(cross[0]*cross[0] + cross[1]*cross[1] + cross[2]*cross[2]);
            cross[0] /= mag;
            cross[1] /= mag;
            cross[2] /= mag;
        
            //Calculate the normal's angle vs the camera view direction
            normal_angle = (float)Math.Acos(-cross[2]);
    
            //If the normal is facing away from the camera, don't bother drawing it    
            //We're giving this a +5% tolerance
	        if(normal_angle * 0.99 > (Math.PI/2))        
                return;
	/*
            //Calculate the shading color based on the first vertex color and the
            //angle between the camera and the surface normal
            lighting_pct = 1.0f - ((2 * normal_angle)/(float)Math.PI);
            r = ((float)polygon.c.Red / 2.0f) * lighting_pct;
            r = r > 255.0 ? 255 : r;     
            g = ((float)polygon.c.Green / 2.0f) * lighting_pct;
            g = g > 255.0 ? 255 : g;
            b = ((float)polygon.c.Blue / 2.0f) * lighting_pct;
            b = b > 255.0 ? 255 : b;
            */

            //Move the vertices from world space to screen space
            i = 0;
            foreach (Vertex v in polygon.v) 
                this._Project(v, ref p[i++]);

            if(polygon.v.Length == 3)
                _DrawTriTextured(p[0].x, p[0].y, polygon.uv[0].v, polygon.uv[0].u, p[1].x, p[1].y, polygon.uv[1].v, polygon.uv[1].u, p[2].x, p[2].y, polygon.uv[2].v, polygon.uv[2].u, polygon.c);
            else
                _DrawQuadTextured(p[0].x, p[0].y, polygon.uv[0].v, polygon.uv[0].u, p[1].x, p[1].y, polygon.uv[1].v, polygon.uv[1].u, p[2].x, p[2].y, polygon.uv[2].v, polygon.uv[2].u, p[3].x, p[3].y, polygon.uv[3].v, polygon.uv[3].u, polygon.c);
        }
        /*
        void clip_and_render(Quad quad) {

            Triangle[] out_polygon = new Triangle[2] { new Triangle(new Vertex(), new Vertex(), new Vertex(), Color24.FromRGB(0, 0, 0)), new Triangle(new Vertex(), new Vertex(), new Vertex(), Color24.FromRGB(0, 0, 0)) };


        }
        */
        void clip_and_render(Polygon triangle) {    

            int count;
            bool on_second_iteration = false;
            int i;
            float plane_z = 0.2f;
            float scale_factor, du, dv, dx, dy, dz, ndz;
            bool[] point_marked = new bool[] {false, false, false};
            Vertex[] new_point = new Vertex[] { new Vertex(), new Vertex() }; 
            Polygon[] out_triangle = new Polygon[2] { new Polygon(new Vertex(), new TextureCoordinate(), new Vertex(), new TextureCoordinate(), new Vertex(), new TextureCoordinate(), Color24.FromRGB(0, 0, 0)), new Polygon(new Vertex(), new TextureCoordinate(), new Vertex(), new TextureCoordinate(), new Vertex(), new TextureCoordinate(), Color24.FromRGB(0, 0, 0)) };
            int[] _fixed = new int[2];
            int original;
    
            
            //Note that in the future we're also going to need to clip on the
            //'U', 'V' and color axes
            //TODO: We should really do backface culling even before we do this
            //      clipping. No need to do clipping on a triangle that's going
            //      to be hidden.
            while(true) {
        
                count = 0;
        
                //Check each point to see if it's greater than the plane
                for(i = 0; i < 3; i++) {
            
                    if((on_second_iteration && triangle.v[i].z > plane_z) || 
                      (!on_second_iteration && triangle.v[i].z < plane_z)) {

                        point_marked[i] = true;
                        count++;
                    } else {
                
                        point_marked[i] = false;
                    }
                }
            
                switch(count) {
            
                    //If all of the vertices were out of range, 
                    //skip drawing the whole thing entirely
                    case 3:
                        return;
                        break;
            
                    //If one vertex was out, find it's edge intersections and
                    //build two new triangles out of it
                    case 1:
                        //Figure out what the other two points are
                        _fixed[0] = point_marked[0] ? point_marked[1] ? 2 : 1 : 0;
                        _fixed[1] = _fixed[0] == 0 ? point_marked[1] ? 2 : 1 : _fixed[0] == 1 ? point_marked[0] ? 2 : 0 : point_marked[0] ? 1 : 0;
                        original = point_marked[0] ? 0 : point_marked[1] ? 1 : 2;
                
                        //Calculate the new intersection points
                        for(i = 0; i < 2; i++) {
                    
                            //x,y, and z 'length'
                            dx = triangle.v[original].x - triangle.v[_fixed[i]].x;
                            dy = triangle.v[original].y - triangle.v[_fixed[i]].y;
                            dz = triangle.v[original].z - triangle.v[_fixed[i]].z;
                            du = triangle.uv[original].u - triangle.uv[_fixed[i]].u;
                            dv = triangle.uv[original].v - triangle.uv[_fixed[i]].v;

                            //Set the known axis value
                            new_point[i].z = plane_z; //Replace this with a line function
                    
                            //z 'length' of new point
                            ndz = new_point[i].z - triangle.v[_fixed[i]].z;
                    
                            //ratio of new y-length to to old
                            scale_factor = ndz/dz; //For now, we're dealing with a plane orthogonal to the clipping axis and as such 
                                                   //we can't possibly have zero dy because that would place both the 'in' and 'out'
                                                   //vertexes behind the plane, which is obviously impossible, so we won't worry about
                                                   //that case until we start playing with sloped clipping planes
                    
                            //Scale the independent axis value by the scaling factor
                            //We can do this for other arbitrary axes in the future, such as U and V
                            new_point[i].x = scale_factor * dx + triangle.v[_fixed[i]].x;
                            new_point[i].y = scale_factor * dy + triangle.v[_fixed[i]].y;
                            //new_point[i].u = scale_factor * du + triangle.v[_fixed[i]].u;
                            //new_point[i].v = scale_factor * dv + triangle.v[_fixed[i]].v;

                            //Copy the color information
                            new_point[i].c = triangle.v[_fixed[i]].c.Clone();
                        }

                        //Test/draw the new triangles, maintaining the CW or CCW ordering
                        //Build the first triangle
                        out_triangle[0].v[original] = new_point[0].Clone();
                        out_triangle[0].v[_fixed[0]] = triangle.v[_fixed[0]].Clone();
                        out_triangle[0].v[_fixed[1]] = triangle.v[_fixed[1]].Clone();

                        //Build the second triangle    
                        out_triangle[1].v[original] = new_point[1].Clone(); 
                        out_triangle[1].v[_fixed[0]] = new_point[0].Clone();
                        out_triangle[1].v[_fixed[1]] = triangle.v[_fixed[1]].Clone();
                
                        //Run the new triangles through another round of processing
				        out_triangle[0].c = triangle.c.Clone();
				        out_triangle[1].c = triangle.c.Clone();
                        clip_and_render(out_triangle[0]);
                        clip_and_render(out_triangle[1]);
                
                        //Exit the function early for dat tail recursion              
                        return;
                        break;
            
                    case 2:
                        //Figure out which point we're keeping
                        original = point_marked[0] ? point_marked[1] ? 2 : 1 : 0;
                        _fixed[0] = point_marked[0] ? 0 : point_marked[1] ? 1 : 2;
                        _fixed[1] = _fixed[0] == 0 ? point_marked[1] ? 1 : 2 : _fixed[0] == 1 ? point_marked[0] ? 0 : 2 : point_marked[0] ? 0 : 1;
                            
                        //Calculate the new intersection points
                        for(i = 0; i < 2; i++) {
                    
                            //x,y, and z 'length'
                            dx = triangle.v[original].x - triangle.v[_fixed[i]].x;
                            dy = triangle.v[original].y - triangle.v[_fixed[i]].y;
                            dz = triangle.v[original].z - triangle.v[_fixed[i]].z;
                                       
                            //Set the known axis value
                            new_point[i].z = plane_z; //Replace this with a line function
                    
                            //z 'length' of new point
                            ndz = new_point[i].z - triangle.v[_fixed[i]].z;
                    
                            //ratio of new y-length to to old
                            scale_factor = ndz/dz; //For now, we're dealing with a plane orthogonal to the clipping axis and as such 
                                                   //we can't possibly have zero dy because that would place both the 'in' and 'out'
                                                   //vertexes behind the plane, which is obviously impossible, so we won't worry about
                                                   //that case until we start playing with sloped clipping planes
                    
                            //Scale the independent axis value by the scaling factor
                            //We can do this for other arbitrary axes in the future, such as U and V
                            new_point[i].x = scale_factor * dx + triangle.v[_fixed[i]].x;
                            new_point[i].y = scale_factor * dy + triangle.v[_fixed[i]].y;
                    
                            //Copy the color information
                            new_point[i].c = triangle.v[_fixed[i]].c;
                        }

                        //Start building the new triangles, maintaining the CW or CCW ordering 
                        out_triangle[0].v[original] = triangle.v[original].Clone();
                        out_triangle[0].v[_fixed[0]] = new_point[0].Clone();
                        out_triangle[0].v[_fixed[1]] = new_point[1].Clone();
                
                        //Send through processing again
				        out_triangle[0].c = triangle.c.Clone();
                        clip_and_render(out_triangle[0]);
                    
                        //Exit the function early for dat tail recursion  
                        return;
                        break;
            
                    //If there were no intersections we won't do anything and 
                    //allow execution to flow through
                    case 0:
                    default:
                        break; 
                }

                //If we hit case 0 both times above, all points on this
                //triangle lie in the drawable area and we can leave this
                //clipping loop and flow down to do the drawing of the 
                //processed triangle         
                if(on_second_iteration)
                    break;
        
                on_second_iteration = true;
                plane_z = SCREEN_DEPTH;
            }

            //If we got this far, the triangle is drawable. So we should do that. Or whatever.
            //this._DrawTriangle(triangle);
            triangle.UpdateZ();
            this._ZList.Add(triangle);
        }

        //Way faster, doesn't care about UV values, our camera shouldn't be going through objects anyhow
        private void omit_z_and_render(Polygon polygon) {

            foreach (Vertex v in polygon.v)
                if (v.z <= 0) return;

            polygon.UpdateZ();
            this._ZList.Add(polygon);
        }

        public void PolygonRender(Polygon polygon) {

            //clip_and_render(triangle);
            omit_z_and_render(polygon);
        }

        public void ObjectRender(Object3D object3d) {
    
            foreach(Polygon polygon in object3d.Polygons)        
                this.PolygonRender(polygon);
        }

        private void _CommitScene() {
	
            //TODO
	        //ZList_render();
	        //ZList_clear();
	        //updateDisplay();
        }
    }
}
