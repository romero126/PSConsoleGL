using System;
using System.Collections;
using System.Collections.Generic;

// https://github.com/hackerb9/vt340test/blob/849d7b7c46d8eedec5aeaa3dadb26fa66081dd29/colormap/cursorblink.sh#L135
// XTerm's capabilities are documented in the foot terminal documentation,
// https://manpages.ubuntu.com/manpages/questing/man7/foot-ctlseqs.7.html

namespace PSConsoleGL.Terminal {
    public class XTermCellSize {
        public int Width { get; private set; }
        public int Height { get; private set; }

        // <Summary>
        // Initializes a new instance of the XTermCellSize class.
        // This returns the terminal cell size in characters.
        
        // </Summary>
        public XTermCellSize() {
            // Query the terminal for its cell size using the control sequence using [16t
            // Expected response format is: "\x1b[6;<height>;<width>t"
            string response = Terminal.GetControlSequenceResponse("[16t");
            
            try {
                var parts = response.Split(';', 't');
                this.Width = int.Parse(parts[2]);
                this.Height = int.Parse(parts[1]);
            } catch {
                // If parsing fails, we already have the default values set
                this.Width = 10; // Default width
                this.Height = 20; // Default height
            }
        }
    }
    
    // Device Control function
    


    public enum TerminalVersion {
        // https://invisible-island.net/xterm/ctlseqs/ctlseqs.html#h3-Device-Control-functions
        // These are some example terminal identifiers we use for everything.. 
        VT100 = 0,
        VT220 = 1,
        VT240 = 2,
        VT330 = 18,
        VT340 = 19,
        VT320 = 24,
        VT382 = 32,
        VT420 = 41,
        VT510 = 61, // Windows Terminal
        VT520 = 64,
        VT525 = 65
    }

    public enum TerminalCapability {
        Unknown                             = 0,
        Columns132                          = 1,
        PrinterPort                         = 2,
        Sixel                               = 4,
        // Is 5 Reserved
        SelectiveErase                      = 6,
        SoftCharacterSet                    = 7,
        UserDefinedKeys                     = 8,
        NationalReplacementCharacterSets    = 9,
        // Is 10 Reserved?
        // Is 11 Reserved?
        YugoslavianSCS                      = 12,
        // Is 13 Reserved?
        EightBitInterfaceArchitecture       = 14,
        TechnicalCharacterSet               = 15,
        // Is 16 Reserved?
        
        LocatorPort                         = 16, // Added from https://invisible-island.net/xterm/ctlseqs/ctlseqs.html#h3-Device-Control-functions
        TerminalStateInterogation           = 17, // Added from https://invisible-island.net/xterm/ctlseqs/ctlseqs.html#h3-Device-Control-functions

        WindowingCapability                 = 18,
        Sessions                            = 19,
        // Is 20 Reserved?
        HorizontalScrolling                 = 21,
        Color                               = 22,
        Greek                               = 23,
        Turkish                             = 24,
        // Is 25 Reserved?
        // Is 26 Reserved?
        // Is 27 Reserved?

        RectangularAreaOperations           = 28,
        AnsiTextLocator                     = 29, // Added from https://invisible-island.net/xterm/ctlseqs/ctlseqs.html#h3-Device-Control-functions
        // Is 29 Reserved?
        // Is 30 Reserved?
        // Is 31 Reserved?

        TextMacros                          = 32,
        // Is 33 Reserved?
        // Is 34 Reserved?
        // Is 35 Reserved?
        // Is 36 Reserved?
        // Is 37 Reserved?
        // Is 38 Reserved?
        // Is 39 Reserved?
        // Is 40 Reserved?
        // Is 41 Reserved?


        ISOLatin2CharacterSet               = 42,
        // Is 43 Reserved?
        PCTerm                              = 44,
        SoftKeyMap                          = 45,
        ASCIIEmulation                      = 46
    }

    public class Terminal {
        // Create a dictionary of terminal capabilities
        internal static Dictionary<int, string> TerminalCapabilitiesDictionary = new Dictionary<int, string> {
            {  1, "132 columns" },
            {  2, "Printer port" },
            {  4, "Sixel" },
            {  6, "Selective erase" },
            {  7, "Soft character set (DRCS)" },
            {  8, "User-defined keys (UDKs)" },
            {  9, "National replacement character sets (NRCS) (International terminal only)" },
            { 12, "Yugoslavian (SCS)" },
            { 14, "Eight-bit interface architecture" },
            { 15, "Technical character set" },
            { 16, "Locator port" }, // Added from https://invisible-island.net/xterm/ctlseqs/ctlseqs.html#h3-Device-Control-functions
            { 17, "Terminal state interrogation" }, // Added from https://invisible-island.net/xterm/ctlseqs/ctlseqs.html#h3-Device-Control-functions
            { 18, "Windowing capability" },
            { 19, "Sessions" },
            { 21, "Horizontal scrolling" },
            { 23, "Greek" },
            { 24, "Turkish" },
            { 28, "Rectangular area operations" },
            { 29, "Ansi text locator" }, // Added from https://invisible-island.net/xterm/ctlseqs/ctlseqs.html#h3-Device-Control-functions
            { 32, "Text macros" },
            { 42, "ISO Latin-2 character set" },
            { 44, "PCTerm" },
            { 45, "Soft key map" },
            { 46, "ASCII emulation" }
        };

        // Sets up the terminal 
        private static bool altBufferEnabled;
        public static bool AltBufferEnabled {
            get { return altBufferEnabled; }
            set {
                Console.Write("\x1b[?1049" + (value ? "h" : "l"));
                altBufferEnabled = value;
            }
        }
        private static bool cellMotionMouseTrackingEnabled;
        public static bool CellMotionMouseTrackingEnabled {
            get { return cellMotionMouseTrackingEnabled; }
            set {
                Console.Write("\x1b[?1002" + (value ? "h" : "l"));
                cellMotionMouseTrackingEnabled = value;
            }
        }

        private static bool sgrMouseModeEnabled;
        public static bool SGRMouseModeEnabled {
            get { return sgrMouseModeEnabled; }
            set {
                Console.Write("\x1b[?1006" + (value ? "h" : "l"));
                sgrMouseModeEnabled = value;
            }
        }

        private static bool bracketedPasteEnabled;
        public static bool BracketedPasteEnabled {
            get { return bracketedPasteEnabled; }
            set {
                Console.Write("\x1b[?2004" + (value ? "h" : "l"));
                bracketedPasteEnabled = value;
            }
        }

        private static bool xtermAltSendsEscapeEnabled;
        public static bool XTermAltSendsEscapeEnabled {
            get { return xtermAltSendsEscapeEnabled; }
            set {
                Console.Write("\x1b[?1036" + (value ? "h" : "l"));
                xtermAltSendsEscapeEnabled = value;
            }
        }

        // public static bool XTermCtrlSendsEscapeEnabled {
        //     get { return xtermCtrlSendsEscapeEnabled; }
        //     set {
        //         Console.Write("\x1b[?1037" + (value ? "h" : "l"));
        //         xtermCtrlSendsEscapeEnabled = value;
        //     };
        // }
        
        private static TerminalCapability[] XTermCapability(string response) {
            List<TerminalCapability> capabilities = new List<TerminalCapability>();

            int offset = response.IndexOf(';');
            string[] items = response.Substring( offset+1, response.Length-offset-2 ).Split(';');
            foreach (string item in items) {
                int.TryParse(item, out int capability);
                if (!capabilities.Contains((TerminalCapability)capability)) {
                    capabilities.Add((TerminalCapability)capability);
                }
            }
            return capabilities.ToArray();
        }

        public static TerminalCapability[] XTermCapability() {
            string response = GetControlSequenceResponse("[c");
            return XTermCapability(response);
        }

        public static bool XTermCapability(TerminalCapability capability) {
            // Check if the terminal supports the given capability
            TerminalCapability[] terminalCapabilities = XTermCapability();
            foreach (TerminalCapability item in terminalCapabilities) {
                if (item == capability) {
                    return true;
                }
            }
            return false;
        }

        private static string XTermVersion(string version) {
            

            //int offset = response.IndexOf(';');
            return "";
        }

        public static TerminalVersion XTermVersion() {
            return TerminalVersion.VT100;
        }

        public static void PrintXTermCapabilities() {
            // Get the terminal capabilities
            var capabilities = XTermCapability();
            if (capabilities.Length == 0) {
                Console.WriteLine("No terminal capabilities found.");
                return;
            }
            Console.WriteLine("Terminal capabilities:");
            foreach (var capability in capabilities) {
                if (TerminalCapabilitiesDictionary.TryGetValue((int)capability, out string description)) {
                    Console.WriteLine($"Capability: {(int)capability} - {description}");
                } else {
                    Console.WriteLine($"Capability: {(int)capability} - Unknown");
                }
            }
        }

        internal static string GetControlSequenceResponse(string controlSequence) {
            // Send the control sequence to the terminal
            Console.Write("\x1b" + controlSequence);

            // Read the terminal response
            string response = string.Empty;
            while (Console.KeyAvailable == false)
            {
                System.Threading.Thread.Sleep(10);
            }
            while (Console.KeyAvailable) {
                var keyInfo = Console.ReadKey(intercept: true);
                response += keyInfo.KeyChar;
            }
            return response;
        }
    }

//    public class TerminalConsoleWriter : System.IO.Stream
//    {
//        private static System.IO.Stream ConsoleBuffer;
//        
//        public static System.IO.Stream GetConsoleWriter() {
//            return null;
//        }
//
//    }
}