﻿using System;
using System.Runtime.InteropServices;

namespace OpenGL
{
    partial class Gl
    {
        /// <summary>
        /// Shortcut for quickly generating a single buffer id without creating an array to
        /// pass to the gl function.  Calls Gl.GenBuffers(1, id).
        /// </summary>
        /// <returns>The ID of the generated buffer.  0 on failure.</returns>
        public static uint GenBuffer()
        {
            uint[] id = new uint[1];
            Gl.GenBuffers(1, id);
            return id[0];
        }

        /// <summary>
        /// Shortcut for quickly generating a single texture id without creating an array to
        /// pass to the gl function.  Calls Gl.GenTexture(1, id).
        /// </summary>
        /// <returns>The ID of the generated texture.  0 on failure.</returns>
        public static uint GenTexture()
        {
            uint[] id = new uint[1];
            Gl.GenTextures(1, id);
            return id[0];
        }

        /// <summary>
        /// Shortcut for quickly generating a single vertex array id without creating an array to
        /// pass to the gl function.  Calls Gl.GenVertexArrays(1, id).
        /// </summary>
        /// <returns>The ID of the generated vertex array.  0 on failure.</returns>
        public static uint GenVertexArray()
        {
            uint[] id = new uint[1];
            Gl.GenVertexArrays(1, id);
            return id[0];
        }

        /// <summary>
        /// Shortcut for quickly generating a single framebuffer object without creating an array
        /// to pass to the gl function.  Calls Gl.GenFramebuffers(1, id).
        /// </summary>
        /// <returns>The ID of the generated framebuffer.  0 on failure.</returns>
        public static uint GenFramebuffer()
        {
            uint[] id = new uint[1];
            Gl.GenFramebuffers(1, id);
            return id[0];
        }

        /// <summary>
        /// Shortcut for quickly generating a single renderbuffer object without creating an array
        /// to pass to the gl function.  Calls Gl.GenRenderbuffers(1, id).
        /// </summary>
        /// <returns>The ID of the generated framebuffer.  0 on failure.</returns>
        public static uint GenRenderbuffer()
        {
            uint[] id = new uint[1];
            Gl.GenRenderbuffers(1, id);
            return id[0];
        }

        /// <summary>
        /// Gets the program info from a shader program.
        /// </summary>
        /// <param name="program">The ID of the shader program.</param>
        public static string GetProgramInfoLog(UInt32 program)
        {
            int[] length = new int[1];
            Gl.GetProgramiv(program, ProgramParameter.InfoLogLength, length);
            if (length[0] == 0) return String.Empty;
            System.Text.StringBuilder sb = new System.Text.StringBuilder(length[0]);
            Gl.GetProgramInfoLog(program, sb.Capacity, length, sb);
            return sb.ToString();
        }

        /// <summary>
        /// Gets the program info from a shader program.
        /// </summary>
        /// <param name="program">The ID of the shader program.</param>
        public static string GetShaderInfoLog(UInt32 shader)
        {
            int[] length = new int[1];
            Gl.GetShaderiv(shader, ShaderParameter.InfoLogLength, length);
            if (length[0] == 0) return String.Empty;
            System.Text.StringBuilder sb = new System.Text.StringBuilder(length[0]);
            Gl.GetShaderInfoLog(shader, sb.Capacity, length, sb);
            return sb.ToString();
        }

        /// <summary>
        /// Replaces the source code in a shader object.
        /// </summary>
        /// <param name="shader">Specifies the handle of the shader object whose source code is to be replaced.</param>
        /// <param name="source">Specifies a string containing the source code to be loaded into the shader.</param>
        public static void ShaderSource(UInt32 shader, string source)
        {
            ShaderSource(shader, 1, new string[] { source }, new int[] { source.Length });
        }

        /// <summary>
        /// Creates and initializes a buffer object's data store.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="target">Specifies the target buffer object.</param>
        /// <param name="size">Specifies the size in bytes of the buffer object's new data store.</param>
        /// <param name="data">Specifies a pointer to data that will be copied into the data store for initialization, or NULL if no data is to be copied.</param>
        /// <param name="usage">Specifies expected usage pattern of the data store.</param>
        public static void BufferData<T>(BufferTarget target, Int32 size, [InAttribute, OutAttribute] T[] data, BufferUsageHint usage)
            where T : struct
        {
            GCHandle data_ptr = GCHandle.Alloc(data, GCHandleType.Pinned);
            try
            {
                Delegates.glBufferData(target, new IntPtr(size), data_ptr.AddrOfPinnedObject(), usage);
            }
            finally
            {
                data_ptr.Free();
            }
        }

        /// <summary>
        /// Creates a standard VBO of type T.
        /// </summary>
        /// <typeparam name="T">The type of the data being stored in the VBO (make sure it's byte aligned).</typeparam>
        /// <param name="target">The VBO BufferTarget (usually ArrayBuffer or ElementArrayBuffer).</param>
        /// <param name="data">The data to store in the VBO.</param>
        /// <param name="hint">The buffer usage hint (usually StaticDraw).</param>
        /// <returns></returns>
        public static uint CreateVBO<T>(BufferTarget target, [InAttribute, OutAttribute] T[] data, BufferUsageHint hint)
            where T : struct
        {
            uint vboHandle = Gl.GenBuffer();
            if (vboHandle == 0) return 0;

            Gl.BindBuffer(target, vboHandle);
            Gl.BufferData<T>(target, data.Length * Marshal.SizeOf(typeof(T)), data, hint);
            Gl.BindBuffer(target, 0);
            return vboHandle;
        }

        public static uint CreateVBO<T>(BufferTarget target, [InAttribute, OutAttribute] T[] data, BufferUsageHint hint, int length)
            where T : struct
        {
            uint vboHandle = Gl.GenBuffer();
            if (vboHandle == 0) return 0;

            Gl.BindBuffer(target, vboHandle);
            Gl.BufferData<T>(target, length * Marshal.SizeOf(typeof(T)), data, hint);
            Gl.BindBuffer(target, 0);
            return vboHandle;
        }

        #region CreateInterleavedVBO
        public static uint CreateInterleavedVBO(BufferTarget target, Vector3[] data1, Vector3[] data2, BufferUsageHint hint)
        {
            if (data2.Length != data1.Length) throw new Exception("Data lengths must be identical to construct an interleaved VBO.");

            float[] interleaved = new float[data1.Length * 6];

            for (int i = 0, j = 0; i < data1.Length; i++)
            {
                interleaved[j++] = data1[i].x;
                interleaved[j++] = data1[i].y;
                interleaved[j++] = data1[i].z;

                interleaved[j++] = data2[i].x;
                interleaved[j++] = data2[i].y;
                interleaved[j++] = data2[i].z;
            }

            return CreateVBO<float>(target, interleaved, hint);
        }

        public static uint CreateInterleavedVBO(BufferTarget target, Vector3[] data1, Vector3[] data2, Vector2[] data3, BufferUsageHint hint)
        {
            if (data2.Length != data1.Length || data3.Length != data1.Length) throw new Exception("Data lengths must be identical to construct an interleaved VBO.");

            float[] interleaved = new float[data1.Length * 8];

            for (int i = 0, j = 0; i < data1.Length; i++)
            {
                interleaved[j++] = data1[i].x;
                interleaved[j++] = data1[i].y;
                interleaved[j++] = data1[i].z;

                interleaved[j++] = data2[i].x;
                interleaved[j++] = data2[i].y;
                interleaved[j++] = data2[i].z;

                interleaved[j++] = data3[i].x;
                interleaved[j++] = data3[i].y;
            }

            return CreateVBO<float>(target, interleaved, hint);
        }

        public static uint CreateInterleavedVBO(BufferTarget target, Vector3[] data1, Vector3[] data2, Vector3[] data3, BufferUsageHint hint)
        {
            if (data2.Length != data1.Length || data3.Length != data1.Length) throw new Exception("Data lengths must be identical to construct an interleaved VBO.");

            float[] interleaved = new float[data1.Length * 9];

            for (int i = 0, j = 0; i < data1.Length; i++)
            {
                interleaved[j++] = data1[i].x;
                interleaved[j++] = data1[i].y;
                interleaved[j++] = data1[i].z;

                interleaved[j++] = data2[i].x;
                interleaved[j++] = data2[i].y;
                interleaved[j++] = data2[i].z;

                interleaved[j++] = data3[i].x;
                interleaved[j++] = data3[i].y;
                interleaved[j++] = data3[i].z;
            }

            return CreateVBO<float>(target, interleaved, hint);
        }

        public static uint CreateInterleavedVBO(BufferTarget target, Vector3[] data1, Vector3[] data2, Vector3[] data3, Vector2[] data4, BufferUsageHint hint)
        {
            if (data2.Length != data1.Length || data3.Length != data1.Length) throw new Exception("Data lengths must be identical to construct an interleaved VBO.");

            float[] interleaved = new float[data1.Length * 11];

            for (int i = 0, j = 0; i < data1.Length; i++)
            {
                interleaved[j++] = data1[i].x;
                interleaved[j++] = data1[i].y;
                interleaved[j++] = data1[i].z;

                interleaved[j++] = data2[i].x;
                interleaved[j++] = data2[i].y;
                interleaved[j++] = data2[i].z;

                interleaved[j++] = data3[i].x;
                interleaved[j++] = data3[i].y;
                interleaved[j++] = data3[i].z;

                interleaved[j++] = data4[i].x;
                interleaved[j++] = data4[i].y;
            }

            return CreateVBO<float>(target, interleaved, hint);
        }
        #endregion

        public static uint CreateVAO(ShaderProgram program, uint vbo, int[] sizes, VertexAttribPointerType[] types, BufferTarget[] targets, string[] names, int stride, uint eboHandle)
        {
            uint vaoHandle = Gl.GenVertexArray();
            Gl.BindVertexArray(vaoHandle);

            int offset = 0;

            for (uint i = 0; i < names.Length; i++)
            {
                Gl.EnableVertexAttribArray(i);
                Gl.BindBuffer(targets[i], vbo);
                Gl.VertexAttribPointer(i, sizes[i], types[i], true, stride, new IntPtr(offset));
                Gl.BindAttribLocation(program.ProgramID, i, names[i]);
            }

            Gl.BindBuffer(BufferTarget.ElementArrayBuffer, eboHandle);
            Gl.BindVertexArray(0);

            return vaoHandle;
        }
        
        private static int _version = 0;
        
        public static int Version()
        {
            if (_version != 0) return _version;	// cache the version information
            
            try
            {
                string version = System.Runtime.InteropServices.Marshal.PtrToStringAnsi(Gl.GetString(StringName.Version));
                return (_version = int.Parse(version.Substring(0, version.IndexOf('.'))));
            }
            catch (Exception)
            {
                Console.WriteLine("Error while retrieving the OpenGL version.");
                return 0;
            }
        }

        /// <summary>
        /// Installs a program object as part of current rendering state.
        /// </summary>
        /// <param name="Program">Specifies the handle of the program object whose executables are to be used as part of current rendering state.</param>
        public static void UseProgram(ShaderProgram Program)
        {
            currentProgram = Program.ProgramID;
            Gl.UseProgram(currentProgram);
        }

        /// <summary>
        /// Bind a named texture to a texturing target
        /// </summary>
        /// <param name="Texture">Specifies the texture.</param>
        public static void BindTexture(Texture Texture)
        {
            Gl.BindTexture(Texture.TextureTarget, Texture.TextureID);
        }

        /// <summary>
        /// Return the value of the selected parameter.
        /// </summary>
        /// <param name="name">Specifies the parameter value to be returned.</param>
        public static int GetInteger(GetPName name)
        {
            int[] temp = new int[1];
            GetIntegerv(name, temp);
            return temp[0];
        }

        public static void UniformMatrix4fv(int location, Matrix4 matrix)
        {
            Gl.UniformMatrix4fv(location, 1, false, matrix.ToFloat());
        }

        private static uint currentProgram = 0;

        public static uint CurrentProgram { get { return currentProgram; } }

        /// <summary>
        /// Get the index of a uniform block in the provided shader program.
        /// Note:  This method will use the provided shader program, so make sure to
        /// store which program is currently active and reload it if required.
        /// </summary>
        /// <param name="program">The shader program that contains the uniform block.</param>
        /// <param name="uniformBlockName">The uniform block name.</param>
        /// <returns>The index of the uniform block.</returns>
        public static uint GetUniformBlockIndex(ShaderProgram program, string uniformBlockName)
        {
            program.Use();  // take care of a crash that can occur on NVIDIA drivers by using the program first
            return GetUniformBlockIndex(program.ProgramID, uniformBlockName);
        }

        public static void BindBuffer<T>(VBO<T> buffer) 
            where T : struct
        {
            Gl.BindBuffer(buffer.BufferTarget, buffer.vboID);
        }

        public static void BindBufferToShaderAttribute<T>(VBO<T> buffer, ShaderProgram program, string attributeName) 
            where T : struct
        {
            uint location = (uint)Gl.GetAttribLocation(program.ProgramID, attributeName);

            Gl.EnableVertexAttribArray(location);
            Gl.BindBuffer(buffer);
            Gl.VertexAttribPointer(location, buffer.Size, buffer.PointerType, true, Marshal.SizeOf(typeof(T)), IntPtr.Zero);
        }
    }
}
