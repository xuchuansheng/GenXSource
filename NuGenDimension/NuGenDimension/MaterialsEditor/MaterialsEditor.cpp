// MaterialsEditor.cpp : Defines the class behaviors for the application.
//

#include "stdafx.h"
#include "MaterialsEditor.h"
#include "MainFrm.h"

#include "MaterialsEditorDoc.h"
#include "MaterialsEditorView.h"

#ifdef _DEBUG
#define new DEBUG_NEW
#endif


// CMaterialsEditorApp

BEGIN_MESSAGE_MAP(CMaterialsEditorApp, CWinApp)
	ON_COMMAND(ID_APP_ABOUT, OnAppAbout)
	// Standard file based document commands
	ON_COMMAND(ID_FILE_NEW, CWinApp::OnFileNew)
	ON_COMMAND(ID_FILE_OPEN, CWinApp::OnFileOpen)
END_MESSAGE_MAP()


// CMaterialsEditorApp construction

CMaterialsEditorApp::CMaterialsEditorApp()
{
	EnableHtmlHelp();
}


// The one and only CMaterialsEditorApp object

CMaterialsEditorApp theApp;


static UINT menuIDs[] = 
{
	1, // STUB
		ID_FILE_OPEN,

		NULL
};

// CMaterialsEditorApp initialization

BOOL CMaterialsEditorApp::InitInstance()
{
	// InitCommonControls() is required on Windows XP if an application
	// manifest specifies use of ComCtl32.dll version 6 or later to enable
	// visual styles.  Otherwise, any window creation will fail.
	InitCommonControls();

	CWinApp::InitInstance();

	// Initialize OLE libraries
	if (!AfxOleInit())
	{
		AfxMessageBox(IDP_OLE_INIT_FAILED);
		return FALSE;
	}
	AfxEnableControlContainer();
	// Standard initialization
	// If you are not using these features and wish to reduce the size
	// of your final executable, you should remove from the following
	// the specific initialization routines you do not need
	// Change the registry key under which our settings are stored
	// TODO: You should modify this string to be something appropriate
	// such as the name of your company or organization
	SetRegistryKey(_T("Local AppWizard-Generated Applications"));
	LoadStdProfileSettings(4);  // Load standard INI file options (including MRU)
	// Register the application's document templates.  Document templates
	//  serve as the connection between documents, frame windows and views
/*
	CEGMenu::SetMenuDrawMode( CEGMenu::STYLE_XP_2003 );
	CEGMenu::SetXpBlending(0);
	CEGMenu::SetAcceleratorsDraw(1);
*/
	CSingleDocTemplate* pDocTemplate;
	pDocTemplate = new CSingleDocTemplate(
		IDR_MAINFRAME,
		RUNTIME_CLASS(CMaterialsEditorDoc),
		RUNTIME_CLASS(CMainFrame),       // main SDI frame window
		RUNTIME_CLASS(CMaterialsEditorView));
	if (!pDocTemplate)
		return FALSE;
	AddDocTemplate(pDocTemplate);

	/*HBITMAP hBmp = CombineResources( 16, IDB_TOOLBAR, NULL );
	BITMAPINFO bi;
	memset( &bi, 0, sizeof(BITMAPINFO) );
	GetBitmapInfo( hBmp, &bi );
	pDocTemplate->m_NewMenuShared.LoadToolBar( hBmp, CSize( bi.bmiHeader.biWidth, 16 ), menuIDs, CLR_NONE ); 
	DeleteObject(hBmp);*/

	// Enable DDE Execute open
	EnableShellOpen();
	RegisterShellFileTypes(TRUE);
	// Parse command line for standard shell commands, DDE, file open
	CCommandLineInfo cmdInfo;
	ParseCommandLine(cmdInfo);
	// Dispatch commands specified on the command line.  Will return FALSE if
	// app was launched with /RegServer, /Register, /Unregserver or /Unregister.
	if (!ProcessShellCommand(cmdInfo))
		return FALSE;
	// The one and only window has been initialized, so show and update it
	m_pMainWnd->ShowWindow(SW_SHOW);
	m_pMainWnd->UpdateWindow();
	// call DragAcceptFiles only if there's a suffix
	//  In an SDI app, this should occur after ProcessShellCommand
	// Enable drag/drop open
	m_pMainWnd->DragAcceptFiles();
	return TRUE;
}



// CAboutDlg dialog used for App About

class CAboutDlg : public CDialog
{
public:
	CAboutDlg();

// Dialog Data
	enum { IDD = IDD_ABOUTBOX };

protected:
	virtual void DoDataExchange(CDataExchange* pDX);    // DDX/DDV support

// Implementation
protected:
	DECLARE_MESSAGE_MAP()
};

CAboutDlg::CAboutDlg() : CDialog(CAboutDlg::IDD)
{
}

void CAboutDlg::DoDataExchange(CDataExchange* pDX)
{
	CDialog::DoDataExchange(pDX);
}

BEGIN_MESSAGE_MAP(CAboutDlg, CDialog)
END_MESSAGE_MAP()

// App command to run the dialog
void CMaterialsEditorApp::OnAppAbout()
{
	CAboutDlg aboutDlg;
	aboutDlg.DoModal();
}


// CMaterialsEditorApp message handlers

