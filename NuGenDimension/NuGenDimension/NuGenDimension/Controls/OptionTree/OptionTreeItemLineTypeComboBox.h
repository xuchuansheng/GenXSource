#ifndef OT_ITEMLINETYPECOMBOBOX
#define OT_ITEMLINETYPECOMBOBOX

#if _MSC_VER > 1000
#pragma once
#endif // _MSC_VER > 1000
// OptionTreeItemComboBox.h : header file
//

// Added Headers
//#include "CommonRes.h"
#include "OptionTreeDef.h"
#include "OptionTreeItem.h"


#include "..//LineStyleCombo.h"

// Classes
class COptionTree;

/////////////////////////////////////////////////////////////////////////////
// COptionTreeItemLineTypeComboBox window

class COptionTreeItemLineTypeComboBox : public CLineStyleCombo, public COptionTreeItem
{
// Construction
public:
	COptionTreeItemLineTypeComboBox();
	virtual void OnMove();
	virtual void OnRefresh();
	virtual void OnCommit();
	virtual void OnActivate();
	virtual void CleanDestroyWindow();
	virtual void OnDeSelect();
	virtual void OnSelect();
	virtual void DrawAttribute(CDC *pDC, const RECT &rcRect);
// Attributes
private:
	unsigned int   m_line_type;
public:
	unsigned int   GetLineType() const {return m_line_type;};
	void           SetLineType(unsigned int ci){m_line_type = ci;};

	void           LostFocus() {m_bFocus=FALSE;};
// Operations
public:

// Overrides
	// ClassWizard generated virtual function overrides
	//{{AFX_VIRTUAL(COptionTreeItemLineTypeComboBox)
	//}}AFX_VIRTUAL

// Implementation
public:
	long GetDropDownHeight();
	void SetDropDownHeight(long lHeight);
	BOOL CreateComboItem(DWORD dwAddStyle = 0);
	virtual ~COptionTreeItemLineTypeComboBox();

	// Generated message map functions
protected:
	BOOL m_bFocus;
	long m_lDropDownHeight;
	//{{AFX_MSG(COptionTreeItemLineTypeComboBox)
	afx_msg void OnSetFocus(CWnd* pOldWnd);
	afx_msg void OnKillFocus(CWnd* pNewWnd);
	//}}AFX_MSG

	DECLARE_MESSAGE_MAP()

};

/////////////////////////////////////////////////////////////////////////////

//{{AFX_INSERT_LOCATION}}
// Microsoft Visual C++ will insert additional declarations immediately before the previous line.

#endif // !OT_ITEMCOMBOBOX
