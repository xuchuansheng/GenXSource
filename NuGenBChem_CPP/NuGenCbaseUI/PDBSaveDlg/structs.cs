using System;
using System.Text;
using System.Drawing;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using NGVChem;

namespace NGVChem
{
	#region WINDOWINFO

    [StructLayout(LayoutKind.Sequential)]
    internal struct WINDOWINFO
    {
       public UInt32 cbSize;
       public RECT   rcWindow;
       public RECT   rcClient;
       public UInt32 dwStyle;
       public UInt32 dwExStyle;
       public UInt32 dwWindowStatus;
       public UInt32 cxWindowBorders;
       public UInt32 cyWindowBorders;
       public UInt16 atomWindowType;
       public UInt16 wCreatorVersion;
   }
    #endregion

    #region POINT

	[StructLayout(LayoutKind.Sequential)]
	internal struct POINT
	{
		public int x;
		public int y;

        #region Constructors
        public POINT(int x, int y)
		{
			this.x = x;
			this.y = y;
		}

        public POINT(Point point)
        {
            x = point.X;
            y = point.Y;
        }
        #endregion
    }
	#endregion

    #region RECT

    [StructLayout(LayoutKind.Sequential)]
    internal struct RECT 
    { 
        public int left; 
        public int top; 
        public int right; 
        public int bottom;

        #region Properties

        public POINT Location
        {
            get {return new POINT((int) left, (int) top);}
            set
            {
                right   -= (left -  value.x);
                bottom  -= (bottom -  value.y);
                left    =  value.x;
                top     =  value.y;
            }
        }

        internal uint Width
        {
            get {return (uint)Math.Abs(right - left);}
            set {right = left + (int)value;}
        }

        internal uint Height
        {
            get {return (uint)Math.Abs(bottom - top);}
            set {bottom = top + (int)value;}
        }
        #endregion

        #region Overrides
        public override string ToString()
        {
            return left + ":" + top + ":" + right + ":" + bottom;
        }
        #endregion
    }
    #endregion

    #region WINDOWPOS

    [StructLayout(LayoutKind.Sequential)]
	internal struct WINDOWPOS
	{
		public IntPtr   hwnd;
		public IntPtr   hwndAfter;
		public int      x;
		public int      y;
		public int      cx;
		public int      cy;
		public uint     flags;

        #region Overrides
        public override string ToString()
        {
            return x + ":" + y + ":" + cx + ":" + cy + ":" + ((SWP_Flags) flags).ToString();
        }
        #endregion
    }
    #endregion

    #region NMHDR
    
    [StructLayout(LayoutKind.Sequential)]
    internal struct NMHDR 
    {
        public IntPtr  hwndFrom;
        public uint    idFrom;
        public uint    code;
    } 
    #endregion

    #region OPENFILENAME

    /// <summary>
    /// Defines the shape of hook procedures that can be called by the OpenFileDialog
    /// </summary>
    internal delegate IntPtr OfnHookProc(IntPtr hWnd, UInt16 msg, Int32 wParam, Int32 lParam);
    /// <summary>
    /// See the documentation for OPENFILENAME
    /// </summary>
    //typedef struct tagOFN { 
    //  DWORD         lStructSize; 
    //  HWND          hwndOwner; 
    //  HINSTANCE     hInstance; 
    //  LPCTSTR       lpstrFilter; 
    //  LPTSTR        lpstrCustomFilter; 
    //  DWORD         nMaxCustFilter; 
    //  DWORD         nFilterIndex; 
    //  LPTSTR        lpstrFile; 
    //  DWORD         nMaxFile; 
    //  LPTSTR        lpstrFileTitle; 
    //  DWORD         nMaxFileTitle; 
    //  LPCTSTR       lpstrInitialDir; 
    //  LPCTSTR       lpstrTitle; 
    //  DWORD         Flags; 
    //  WORD          nFileOffset; 
    //  WORD          nFileExtension; 
    //  LPCTSTR       lpstrDefExt; 
    //  LPARAM        lCustData; 
    //  LPOFNHOOKPROC lpfnHook; 
    //  LPCTSTR       lpTemplateName; 
    //#if (_WIN32_WINNT >= 0x0500)
    //  void *        pvReserved;
    //  DWORD         dwReserved;
    //  DWORD         FlagsEx;
    //#endif // (_WIN32_WINNT >= 0x0500)
    //} OPENFILENAME, *LPOPENFILENAME;
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto )]
    internal struct OPENFILENAME
    {
        public UInt32 lStructSize;
        public IntPtr hwndOwner;
        public IntPtr hInstance;
        public String lpstrFilter;
        public String lpstrCustomFilter;
        public UInt32 nMaxCustFilter;
        public Int32 nFilterIndex;
        public String lpstrFile;
        public UInt32 nMaxFile;
        public String lpstrFileTitle;
        public UInt32 nMaxFileTitle;
        public String lpstrInitialDir;
        public String lpstrTitle;
        public UInt32 Flags;
        public UInt16 nFileOffset;
        public UInt16 nFileExtension;
        public String lpstrDefExt;
        public IntPtr lCustData;
        public OfnHookProc lpfnHook;
        public String lpTemplateName;
        public IntPtr pvReserved;
        public UInt32 dwReserved;
        public UInt32 FlagsEx;
    };
    #endregion
    #region OFNOTIFY

    [StructLayout(LayoutKind.Sequential)]
    internal struct OFNOTIFY 
    {
        public NMHDR    hdr;
        public IntPtr OpenFileName;
        public IntPtr   fileNameShareViolation;
    }
    #endregion
}
