using System;
using System.Collections.Generic;
using System.Management.Automation;
using PSConsoleGL.Terminal.Drawing;

namespace PSConsoleGL.Cmdlets
{
    [Cmdlet(VerbsCommon.New, "Framebuffer")]
    public class NewFramebufferCmdlet : PSCmdlet
    {
        [Parameter(Mandatory = true)]
        public int Width { get; set; }

        [Parameter(Mandatory = true)]
        public int Height { get; set; }

        private FrameBuffer frameBuffer;

        //protected override void ProcessRecord()
        protected override void EndProcessing()
        {
            if (Width <= 0 || Height <= 0) {
                throw new ArgumentException("Width and Height must be greater than zero.");
            }

            if (Width == null && Height == null) {
                frameBuffer = new FrameBuffer();
                WriteObject(frameBuffer);
                return;
            } else if (Width == null || Height == null) {
                throw new ArgumentNullException("Width and Height cannot be null.");
            }

            frameBuffer = new FrameBuffer(Width, Height);
            WriteObject(frameBuffer);
        }

        

        
    }

    [Cmdlet(VerbsCommon.Clear, "Framebuffer", DefaultParameterSetName = "ARGB")]
    public class ClearFramebufferCmdlet : PSCmdlet
    {
        [Parameter(Mandatory = true)]
        public FrameBuffer FrameBuffer { get; set; }

        [Parameter(Mandatory = false)]
        public int A { get; set; } = 255;

        [Parameter(Mandatory = false)]
        public int R { get; set; } = 0;

        [Parameter(Mandatory = false)]
        public int G { get; set; } = 0;

        [Parameter(Mandatory = false)]
        public int B { get; set; } = 0;

        protected override void ProcessRecord()
        {
            FrameBuffer.Clear(A, R, G, B);
            WriteObject(FrameBuffer);
        }
    }
}