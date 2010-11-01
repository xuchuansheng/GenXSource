// Surfaces.cpp : Defines the initialization routines for the DLL.
//

#include "stdafx.h"
#include "resource.h"
#include <afxdllx.h>

#ifdef _DEBUG
#define new DEBUG_NEW
#undef THIS_FILE
static char THIS_FILE[] = __FILE__;
#endif

unsigned int	 endID = 0;
unsigned int	 startID = 0;

REGIME			 active_regime = REGIME_NONE;

#include "Commands//FaceCommand.h"
#include "Commands//Coons2Command.h"
#include "Commands//Coons3Command.h"
#include "Commands//Coons4Command.h"
#include "Commands//SplineSurf.h"
#include "Commands//LinearSurf.h"

extern "C"  AFX_EXTENSION_MODULE SurfacesDLL = { NULL, NULL };

extern "C" int APIENTRY
DllMain(HINSTANCE hInstance, DWORD dwReason, LPVOID lpReserved)
{
	// Remove this if you use lpReserved
	UNREFERENCED_PARAMETER(lpReserved);

	if (dwReason == DLL_PROCESS_ATTACH)
	{
		TRACE0("Surfaces.DLL Initializing!\n");
		
		// Extension DLL one-time initialization
		if (!AfxInitExtensionModule(SurfacesDLL, hInstance))
			return 0;

		new CDynLinkLibrary(SurfacesDLL);
	}
	else if (dwReason == DLL_PROCESS_DETACH)
	{
		TRACE0("Surfaces.DLL Terminating!\n");
		
		AfxTermExtensionModule(SurfacesDLL);
	}
	return 1;   // ok
}

extern "C" AFX_EXT_API void GetPluginInfo(PLUGIN_INFO* plInfo)
{
	SWITCH_RESOURCE
	plInfo->plugin_type = PLUGIN_TOOLBAR;
	plInfo->menu_string.LoadString(IDS_MENU_STRING);
	plInfo->show_after_load = true;
	plInfo->in_trial_version = true;
	plInfo->plugin_version = 1;
	plInfo->NuGenDimension_version = 1;
	plInfo->kernel_version = 1;
}

extern "C" AFX_EXT_API void ResetNames()
{
	face_name_index = 1;
	coons_name_index = 1;
	spline_surface_index = 1;
	linear_surface_index = 1;
}

extern "C" AFX_EXT_API void GetToolbar(unsigned int start_ID_from_app, 
											   CToolBar* pToolbar, 
											   CWnd* pPar)
{
	SWITCH_RESOURCE
	startID = endID = start_ID_from_app;

	if (!pToolbar->CreateEx(pPar, TBSTYLE_FLAT, WS_CHILD | CBRS_TOP
		| CBRS_GRIPPER | CBRS_TOOLTIPS | CBRS_FLYBY | CBRS_SIZE_DYNAMIC) ||
		!pToolbar->LoadToolBar(MAKEINTRESOURCE(IDR_TOOLBAR)))
	{
		TRACE0("Failed to create toolbar 2D\n");
		return;      // fail to create
	}

	//Load32BitmapOnToolbar(pToolbar,IDB_TOOLBAR32_BMP);

	pToolbar->GetToolBarCtrl().SetCmdID(0,endID++);
	pToolbar->GetToolBarCtrl().SetCmdID(1,endID++);
	pToolbar->GetToolBarCtrl().SetCmdID(2,endID++);
	pToolbar->GetToolBarCtrl().SetCmdID(3,endID++);
	pToolbar->GetToolBarCtrl().SetCmdID(4,endID++);
	pToolbar->GetToolBarCtrl().SetCmdID(5,endID++);

	pToolbar->EnableDocking(CBRS_ALIGN_ANY);

	ResetNames();
}


extern "C" AFX_EXT_API   HBITMAP  GetToolbarBitmap(LPWORD wC)
{
	HBITMAP hBitmap = NULL;
	/*if ( NULL == hInst )
	hInst = ::AfxFindResourceHandle( IDB_TOOLBAR32_BMP, RT_BITMAP);*/
	HRSRC hRsrc = ::FindResource(SurfacesDLL.hResource, 
		MAKEINTRESOURCE(IDB_TOOLBAR32_BMP), RT_BITMAP);
	if ( hRsrc ){
		HGLOBAL hglb = LoadResource(SurfacesDLL.hResource, hRsrc);
		if ( hglb ){
			// ������ ���������
			LPBITMAPINFO pbi = (LPBITMAPINFO)LockResource(hglb);
			if (pbi ) {
				/*if ( lpnWidth )
				*lpnWidth = pbi->bmiHeader.biWidth;
				if ( lpnHeight )
				*lpnHeight = pbi->bmiHeader.biHeight;*/
				if ( wC )
					*wC = pbi->bmiHeader.biBitCount;
				// ������ ������
				HDC hdc = GetDC( NULL );
				//hBitmap = CreateDIBitmap( hdc, &pbi->bmiHeader, CBM_INIT, pbi->bmiColors, pbi, DIB_RGB_COLORS);
				//				BYTE* pData = (BYTE*)pbi + sizeof(BITMAPINFO) + pbi->bmiHeader.biClrUsed * sizeof(COLORREF);
				BYTE* pData = (BYTE*)pbi + sizeof(BITMAPINFOHEADER) + pbi->bmiHeader.biClrUsed * sizeof(COLORREF);
				hBitmap = CreateDIBitmap( hdc, &pbi->bmiHeader, CBM_INIT, (void*)pData, pbi, DIB_RGB_COLORS);
				// hBitmap = CreateDIBSection( hdc, pbi, DIB_RGB_COLORS, (LPVOID*)&pbi->bmiColors, NULL, NULL );
				::ReleaseDC	(NULL, hdc);
			}
			FreeResource( hglb );
		}	
	}
	return hBitmap;
}


extern "C" AFX_EXT_API   HBITMAP  GetObjectBitmap(const char* obID, LPWORD wC)
{
	HBITMAP hBitmap = NULL;
	HRSRC hRsrc = NULL;
	if (strcmp("{F3CB6AB8-ED48-4e10-BE2F-7FF8B1BBE8A3}",obID)==0)
	{
		hRsrc = ::FindResource(SurfacesDLL.hResource, 
			MAKEINTRESOURCE(IDB_FACE_OBJ_BMP), RT_BITMAP);
		goto lbl;
	}
	if (strcmp("{F7325996-1640-4cf8-BA6A-F18415B512CB}",obID)==0)
	{
		hRsrc = ::FindResource(SurfacesDLL.hResource, 
			MAKEINTRESOURCE(IDB_COONS_OBJ_BMP), RT_BITMAP);
		goto lbl;	
	}
	if (strcmp("{3C633C06-8A86-468f-B54F-EBE54D763665}",obID)==0)
	{
		hRsrc = ::FindResource(SurfacesDLL.hResource, 
			MAKEINTRESOURCE(IDB_LINEAR_BMP), RT_BITMAP);
		goto lbl;	
	}
	if (strcmp("{8809183F-18CC-49ee-8B5E-9570D3AF3A6A}",obID)==0)
	{
		hRsrc = ::FindResource(SurfacesDLL.hResource, 
			MAKEINTRESOURCE(IDB_SECT_OBJ_BMP), RT_BITMAP);
		goto lbl;
	}
lbl:
	if ( hRsrc ){
		HGLOBAL hglb = LoadResource(SurfacesDLL.hResource, hRsrc);
		if ( hglb ){
			// ������ ���������
			LPBITMAPINFO pbi = (LPBITMAPINFO)LockResource(hglb);
			if (pbi ) {
				if ( wC )
					*wC = pbi->bmiHeader.biBitCount;
				// ������ ������
				HDC hdc = GetDC( NULL );
				BYTE* pData = (BYTE*)pbi + sizeof(BITMAPINFOHEADER) + pbi->bmiHeader.biClrUsed * sizeof(COLORREF);
				hBitmap = CreateDIBitmap( hdc, &pbi->bmiHeader, CBM_INIT, (void*)pData, pbi, DIB_RGB_COLORS);
				::ReleaseDC	(NULL, hdc);
			}
			FreeResource( hglb );
		}	
	}
	return hBitmap;
}

extern "C" AFX_EXT_API   void  GetFirstAndLastIDs(unsigned int& stID, unsigned int& eID)
{
	stID = startID;
	eID  = endID-1;
}

extern "C" AFX_EXT_API   void GetSupportedObjectIDs(std::vector<const char*>& objIDs)
{
	objIDs.push_back("{F3CB6AB8-ED48-4e10-BE2F-7FF8B1BBE8A3}"); 
	objIDs.push_back("{F7325996-1640-4cf8-BA6A-F18415B512CB}"); 
	objIDs.push_back("{3C633C06-8A86-468f-B54F-EBE54D763665}");
	objIDs.push_back("{8809183F-18CC-49ee-8B5E-9570D3AF3A6A}");
}

extern "C" AFX_EXT_API   void GetButtonState(unsigned int nID, bool& ch, bool& enbl)
{
	enbl = (sgGetScene()->GetObjectsList()->GetCount()>0);
	ch=false;
	if ((nID==startID) && (active_regime == REGIME_FACE))
	{
		ch=true;
	    return;
	}
	if ((nID==startID+1) && (active_regime == REGIME_COONS_2))
	{
		ch=true;
		return;
	}
	if ((nID==startID+2) && (active_regime == REGIME_COONS_3))
	{
		ch=true;
		return;
	}
	if ((nID==startID+3) && (active_regime == REGIME_COONS_4))
	{
		ch=true;
		return;
	}
	if ((nID==startID+4) && (active_regime == REGIME_LINEAR_SURF))
	{
		ch=true;
		return;
	}
	if ((nID==startID+5) && (active_regime == REGIME_SPLINE_SURF))
	{
		ch=true;
		return;
	}
}

CString temporary_message_str;
extern "C" AFX_EXT_API   LPCTSTR  GetStatusBarMessage(unsigned int nID)
{
	SWITCH_RESOURCE
	if (nID == startID)
	{
		temporary_message_str.LoadString(IDS_STBAR_ZERO);
		return temporary_message_str;
	}
	else if (nID == startID+1)
	{
		temporary_message_str.LoadString(IDS_STBAR_FIRST);
		return temporary_message_str;
	}
	else if (nID == startID+2)
	{
		temporary_message_str.LoadString(IDS_STBAR_SECOND);
		return temporary_message_str;
	}
	else if (nID == startID+3)
	{
		temporary_message_str.LoadString(IDS_STBAR_THIRD);
		return temporary_message_str;
	}
	else if (nID == startID+4)
	{
		temporary_message_str.LoadString(IDS_STBAR_FOURTH);
		return temporary_message_str;
	}
	else if (nID == startID+5)
	{
		temporary_message_str.LoadString(IDS_STBAR_FIVETH);
		return temporary_message_str;
	}
	else
		return "Unknown";
}

extern "C" AFX_EXT_API   LPCTSTR  GetTooltipMessage(unsigned int nID)
{
	SWITCH_RESOURCE
	if (nID == startID)
	{
		temporary_message_str.LoadString(IDS_TOOLTIP_ZERO);
		return temporary_message_str;
	}
	else if (nID == startID+1)
	{
		temporary_message_str.LoadString(IDS_TOOLTIP_FIRST);
		return temporary_message_str;
	}
	else if (nID == startID+2)
	{
		temporary_message_str.LoadString(IDS_TOOLTIP_SECOND);
		return temporary_message_str;
	}
	else if (nID == startID+3)
	{
		temporary_message_str.LoadString(IDS_TOOLTIP_THIRD);
		return temporary_message_str;
	}
	else if (nID == startID+4)
	{
		temporary_message_str.LoadString(IDS_TOOLTIP_FOURTH);
		return temporary_message_str;
	}
	else if (nID == startID+5)
	{
		temporary_message_str.LoadString(IDS_TOOLTIP_FIVETH);
		return temporary_message_str;
	}
	else
		return "Unknown";
}

static   ICommander*   global_current_commander = NULL;

extern "C" AFX_EXT_API   ICommander* GetNewCommander(unsigned int nID,IApplicationInterface* appI)
{
	if (nID == startID)
		{
			active_regime = REGIME_FACE;
			global_current_commander = new FaceCommand(appI);
			return global_current_commander;
		}
		else if (nID == startID+1)
		{
			active_regime = REGIME_COONS_2;
			global_current_commander = new Coons2Command(appI);
			return global_current_commander;
		}
		else if (nID == startID+2)
		{
			active_regime = REGIME_COONS_3;
			global_current_commander = new Coons3Command(appI);
			return global_current_commander;
		}
		else if (nID == startID+3)
		{
			active_regime = REGIME_COONS_4;
			global_current_commander = new Coons4Command(appI);
			return global_current_commander;
		}
		else if (nID == startID+4)
		{
			active_regime = REGIME_LINEAR_SURF;
			global_current_commander = new LinearSurfaceCommand(appI);
			return global_current_commander;
		}
		else if (nID == startID+5)
		{
			active_regime = REGIME_SPLINE_SURF;
			global_current_commander = new SplineSurfaceCommand(appI);
			return global_current_commander;
		}
		
	return NULL;
}

extern "C" AFX_EXT_API   ICommander* GetEditCommander(sgCObject* editableObj, IApplicationInterface* appI)
{
	return NULL;
}

extern "C" AFX_EXT_API   bool   FreeCommander(ICommander* cmndr)
{
	if ((global_current_commander!=NULL) && (global_current_commander==cmndr))
	{
		delete global_current_commander;
		global_current_commander = NULL;
		cmndr = NULL;
		active_regime = REGIME_NONE;
		return true;
	}
	else
	{
		ASSERT(0);
		return false;
	}
}
