﻿using System;
using System.Drawing;

namespace OpenGL
{
    public class FBO : IDisposable
    {
        #region Properties
        /// <summary>
        /// The ID for the entire framebuffer object.
        /// </summary>
        public uint BufferID { get; private set; }

        /// <summary>
        /// The IDs for each of the renderbuffer attachments.
        /// </summary>
        public uint[] TextureID { get; private set; }

        /// <summary>
        /// The ID for the single depth buffer attachment.
        /// </summary>
        public uint DepthID { get; private set; }

        /// <summary>
        /// The size (in pixels) of all renderbuffers associated with this framebuffer.
        /// </summary>
        public Size Size { get; private set; }

        /// <summary>
        /// The attachments used by this framebuffer.
        /// </summary>
        public FramebufferAttachment[] Attachments { get; private set; }

        /// <summary>
        /// The internal pixel format for each of the renderbuffers (depth buffer not included).
        /// </summary>
        public PixelInternalFormat Format { get; private set; }

        private bool mipmaps;
        #endregion

        #region Constructor and Destructor
        /// <summary>
        /// Creates a framebuffer object and its associated resources (depth and pbuffers).
        /// </summary>
        /// <param name="Size">Specifies the size (in pixels) of the framebuffer and it's associated buffers.</param>
        /// <param name="Attachments">Specifies the attachments to use for the pbuffers.</param>
        /// <param name="Format">Specifies the internal pixel format for the pbuffers.</param>
        public FBO(Size Size, FramebufferAttachment[] Attachments, PixelInternalFormat Format, bool Mipmaps)
        {
            this.Size = Size;
            this.Attachments = Attachments;
            this.Format = Format;
            this.mipmaps = Mipmaps;

            // First create the framebuffer
            BufferID = Gl.GenFramebuffer();
            Gl.BindFramebuffer(FramebufferTarget.Framebuffer, BufferID);

            // Create and attach a 24-bit depth buffer to the framebuffer
            DepthID = Gl.GenRenderbuffer();
            Gl.BindRenderbuffer(RenderbufferTarget.Renderbuffer, DepthID);
            Gl.RenderbufferStorage(RenderbufferTarget.Renderbuffer, RenderbufferStorage.DepthComponent24, Size.Width, Size.Height);

            // Create n texture buffers (known by the number of attachments)
            TextureID = new uint[Attachments.Length];
            Gl.GenTextures(Attachments.Length, TextureID);

            // Bind the n texture buffers to the framebuffer
            for (int i = 0; i < Attachments.Length; i++)
            {
                Gl.BindTexture(TextureTarget.Texture2D, TextureID[i]);
                Gl.TexImage2D(TextureTarget.Texture2D, 0, Format, Size.Width, Size.Height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, IntPtr.Zero);
                if (Mipmaps)
                {
                    Gl.TexParameteri(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, 9729); // public const int GL_LINEAR = 9729;
                    Gl.TexParameteri(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, 9987); // public const int GL_LINEAR_MIPMAP_LINEAR = 9987;
                    Gl.GenerateMipmap(GenerateMipmapTarget.Texture2D);
                }
                Gl.FramebufferTexture(FramebufferTarget.Framebuffer, Attachments[i], TextureID[i], 0);
            }

            // Build the framebuffer and check for errors
            Gl.FramebufferRenderbuffer(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthAttachment, RenderbufferTarget.Renderbuffer, DepthID);

            FramebufferErrorCode status = Gl.CheckFramebufferStatus(FramebufferTarget.Framebuffer);
            if (status != FramebufferErrorCode.FramebufferComplete)
            {
                Console.WriteLine("Frame buffer did not compile correctly.  Returned {0}, glError: {1}", status.ToString(), Gl.GetError().ToString());
            }

            Gl.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
        }

        /// <summary>
        /// Check to ensure that the FBO was disposed of properly.
        /// </summary>
        ~FBO()
        {
            if (DepthID != 0 || BufferID != 0 || TextureID != null)
            {
                System.Diagnostics.Debug.Fail("FBO was not disposed of properly.");
            }
        }
        #endregion

        #region Enable and Disable
        /// <summary>
        /// Binds the framebuffer and all of the renderbuffers.
        /// Clears the buffer bits and sets viewport size.
        /// Perform all rendering after this call.
        /// </summary>
        public void Enable()
        {
            DrawBuffersEnum[] buffers = new DrawBuffersEnum[Attachments.Length];

            Gl.BindFramebuffer(FramebufferTarget.Framebuffer, BufferID);
            for (int i = 0; i < Attachments.Length; i++)
            {
                Gl.BindTexture(TextureTarget.Texture2D, TextureID[i]);
                Gl.FramebufferTexture(FramebufferTarget.Framebuffer, Attachments[i], TextureID[i], 0);
                buffers[i] = (DrawBuffersEnum)Attachments[i];
            }
            if (Attachments.Length > 1) Gl.DrawBuffers(Attachments.Length, buffers);

            Gl.Viewport(0, 0, Size.Width, Size.Height);
            if (mipmaps) Gl.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
        }

        /// <summary>
        /// Unbinds the framebuffer and then generates the mipmaps of each renderbuffer.
        /// </summary>
        public void Disable()
        {
            Gl.BindFramebuffer(FramebufferTarget.Framebuffer, 0);

            // have to generate mipmaps here
            for (int i = 0; i < Attachments.Length && mipmaps; i++)
            {
                Gl.BindTexture(TextureTarget.Texture2D, TextureID[i]);
                Gl.GenerateMipmap(GenerateMipmapTarget.Texture2D);
            }
        }
        #endregion

        #region IDisposable
        public void Dispose()
        {
            if (DepthID != 0 || BufferID != 0 || TextureID != null)
            {
                Gl.DeleteTextures(TextureID.Length, TextureID);
                Gl.DeleteFramebuffers(1, new uint[] { BufferID });
                Gl.DeleteRenderbuffers(1, new uint[] { DepthID });

                BufferID = 0;
                DepthID = 0;
                TextureID = null;
            }
        }
        #endregion

        #region Sample Shader
        public static string vertexShaderSource = @"
#version 330

uniform mat4 projection_matrix;
uniform mat4 modelview_matrix;
uniform float animation_factor;

in vec3 in_position;
in vec3 in_normal;
in vec2 in_uv;

out vec2 uv;

void main(void)
{
  vec4 pos2 = projection_matrix * modelview_matrix * vec4(in_normal, 1);
  vec4 pos1 = projection_matrix * modelview_matrix * vec4(in_position, 1);

  uv = in_uv;
  
  gl_Position = mix(pos2, pos1, animation_factor);
}";

        public static string fragmentShaderSource = @"
#version 330

uniform sampler2D active_texture;

in vec2 uv;

out vec4 out_frag_color;

void main(void)
{
  out_frag_color = mix(texture2D(active_texture, uv), vec4(1, 1, 1, 1), 0.05);
}";
        #endregion
    }
}
