/*
============================================================================
Name        : Supapploginview.h
Author      : Vitaly Vinogradov
Version     : 1.0.0
Copyright   : (c) Next2Friends, 2008
Description : Login view
============================================================================
*/

#ifndef __SUPAPPLOGINVIEW_H__
#define __SUPAPPLOGINVIEW_H__

// INCLUDES
#include <coecntrl.h>
#include <coeview.h>
#include <eiklabel.h>
#include <eikedwin.h>
#include <eikseced.h>
#include <aknprogressdialog.h> // for MProgressDialogCallback
#include "common.h"

// FORWARD DECLARATIONS
class CAknWaitDialog;
#if USE_SKIN_SUPPORT
class CAknsBasicBackgroundControlContext;
#endif

// CLASS DECLARATION
class CSupAppLoginView : public CCoeControl, public MCoeView, public MProgressDialogCallback,
	public MRequestObserver
{
public: // New methods

	/**
	* NewL.
	* Two-phased constructor.
	* Create a CSupAppLoginView object, which will draw itself to aRect.
	* @param aRect The rectangle this view will be drawn to.
	* @return a pointer to the created instance of CSupAppLoginView.
	*/
	static CSupAppLoginView* NewL( const TRect& aRect );

	/**
	* NewLC.
	* Two-phased constructor.
	* Create a CSupAppLoginView object, which will draw itself
	* to aRect.
	* @param aRect Rectangle this view will be drawn to.
	* @return A pointer to the created instance of CSupAppLoginView.
	*/
	static CSupAppLoginView* NewLC( const TRect& aRect );

	/**
	* ~CSupAppLoginView
	* Virtual Destructor.
	*/
	virtual ~CSupAppLoginView();

public:  // Functions from base classes

	/**
	* From CCoeControl, Draw
	* Draw this CSupAppLoginView to the screen.
	* @param aRect the rectangle of this view that needs updating
	*/
	void Draw( const TRect& aRect ) const;

	/**
	* From CoeControl, SizeChanged.
	* Called by framework when the view size is changed.
	*/
	virtual void SizeChanged();

protected: // from MRequestObserver
	void HandleRequestCompletedL(const TInt& aError);

public: // From MCoeView
	TVwsViewId ViewId() const;

	// Initiates login process
	void DoLogin();

private: // Constructors

	/**
	* ConstructL
	* 2nd phase constructor.
	* Perform the second phase construction of a
	* CSupAppLoginView object.
	* @param aRect The rectangle this view will be drawn to.
	*/
	void ConstructL(const TRect& aRect);

	/**
	* CSupAppLoginView.
	* C++ default constructor.
	*/
	CSupAppLoginView();

private:
	/**
	* From CCoeControl, CountComponentControls
	*/
	TInt CountComponentControls() const;

	/**
	* From CCoeControl, ComponentControl
	*/
	CCoeControl* ComponentControl(TInt aIndex) const;

	/**
	* From CCoeControl, HandleResourceChange
	*/
	void HandleResourceChange(TInt aType);

	/**
	* From CCoeControl, OfferKeyEventL
	*/
	TKeyResponse OfferKeyEventL(const TKeyEvent& aKeyEvent, TEventCode aType);

#if USE_SKIN_SUPPORT
	/**
	* MOP relationship support
	*/
	TTypeUid::Ptr MopSupplyObject(TTypeUid aId);
#endif

private: // From MCoeView
	void ViewActivatedL(const TVwsViewId& aPrevViewId, TUid aCustomMessageId, const TDesC8& aCustomMessage);

	void ViewDeactivated();

private:
	// Get's called when a dialog is dismissed
	// From MProgressDialogCallback
	void DialogDismissedL(TInt aButtonId);

	// New functions
	// Starts wait dialog
	void StartWaitDialog();

	// Stops the wait dialog
	void StopWaitDialog();

private:
	void SetControlFocus(); // Auxillary method for setting control focus

private: // Member variables
	CEikLabel* iLoginLabel;
	CEikEdwin* iLoginEdwin;
	CEikLabel* iPassLabel;
	CEikSecretEditor* iPassEdwin;

	CAknWaitDialog* iWaitDialog; // Wait dialog

	//HBufC* iViewParam; // Param's used to interchange with other views

	TInt iFocus; // Current control to be focused

	TInt iFocusControls; // Total count of controls which could be focused

	// Enable skin support if necessary
#if USE_SKIN_SUPPORT
	CAknsBasicBackgroundControlContext* iBgContext;
#endif
};

#endif // __SUPAPPLOGINVIEW_H__

// End of File
