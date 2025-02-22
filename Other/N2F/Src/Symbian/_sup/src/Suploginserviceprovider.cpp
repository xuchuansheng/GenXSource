/*
============================================================================
Name        : Suploginserviceprovider.cpp
Author      : Vitaly Vinogradov
Version     : 1.0.0
Copyright   : (c) Next2Friends, 2008
Description : Login stuff controller class implementation
============================================================================
*/

// INCLUDE FILES
#include <s32file.h>
#include <eikappui.h>
#include <eikapp.h>
#include <eikenv.h>
#include <senserviceconnection.h>
#include <senxmlservicedescription.h>
#include <senservicepattern.h>
#include <sensoapmessage.h>
#include <xml\XmlFrameworkConstants.h>
#include "Suploginserviceprovider.h"
#include "sup.pan"
#include "common.h"


CSupLoginServiceProvider::CSupLoginServiceProvider()
: CCoeStatic( KUidSupLoginController )
{
	iConnection = NULL;
	iMemberID = NULL;
	iUsername = NULL;
	iPassword = NULL;
	iLogged = EFalse;

	iSettingsFile = TFileName(KNullDesC);
}

CSupLoginServiceProvider::~CSupLoginServiceProvider()
{
	// Free dynamically allocated memory

	if (iConnection)
	{
		delete iConnection;
		iConnection = NULL;
	}

	if (iMemberID)
	{
		delete iMemberID;
		iMemberID = NULL;
	}

	if (iUsername)
	{
		delete iUsername;
		iUsername = NULL;
	}

	if (iPassword)
	{
		delete iPassword;
		iPassword = NULL;
	}
}

void CSupLoginServiceProvider::ConstructL()
{
	__LOGSTR_TOFILE("CSupLoginServiceProvider::ConstructL() begins");

	// Form full path to credentials settings file
	RFs &fs = CCoeEnv::Static()->FsSession();

	TFileName privatePath;
	fs.PrivatePath(privatePath);
	TParse parser;
	TFileName processFileName(RProcess().FileName());
	User::LeaveIfError(parser.Set(KLoginProviderSettingsFilename, &privatePath, &processFileName));
	iSettingsFile = parser.FullName();

	// Read credentials from settings file
	ReadDataFromFileL();

	if (iMemberID && iUsername && iPassword)
		iLogged = ETrue;

	// Discover and retrieve description of the web service
	CSenXmlServiceDescription* serviceDesc = CSenXmlServiceDescription::NewLC( KServiceEndpointMemberId, KNullDesC8 );

	serviceDesc->SetFrameworkIdL(KDefaultBasicWebServicesFrameworkID);

	// Create connection to web service
	iConnection = CSenServiceConnection::NewL(*this, *serviceDesc);

	CleanupStack::PopAndDestroy(); // serviceDesc

	__LOGSTR_TOFILE("CSupLoginServiceProvider::ConstructL() ends");
}

TBool CSupLoginServiceProvider::WriteDataToFileL()
{
	__LOGSTR_TOFILE("CSupLoginServiceProvider::WriteDataToFileL() begins");

	// If current operation should be cancelled
	if (iCancelStatus)
		return EFalse;

	// Return value
	TBool retValue = EFalse;

	// If credentials data is not empty
	if (iMemberID && iUsername && iPassword)
	{
		retValue = ETrue;

		RFs fsSession;
		RFileWriteStream writeStream; // Write file stream

		// Install write file session
		User::LeaveIfError(fsSession.Connect());
		CleanupClosePushL(fsSession);

		// Open file stream
		// if already exists - replace with newer version
		TInt err = writeStream.Replace(fsSession, iSettingsFile, EFileStream | EFileWrite | EFileShareExclusive);
		CleanupClosePushL(writeStream);

		// Return EFalse if failed to open stream
		if (err != KErrNone)
		{
			retValue = EFalse;

			__LOGSTR_TOFILE("CSupLoginServiceProvider::WriteDataToFileL() failed to open file");
		}

		if (retValue)
		{
			__LOGSTR_TOFILE("CSupLoginServiceProvider::WriteDataToFileL() succeed to open the file");

			// Write data
			// iMemberID
			writeStream.WriteInt32L(iMemberID->Des().MaxLength());
			writeStream << *iMemberID;
			// iUsername
			writeStream.WriteInt32L(iUsername->Des().MaxLength());
			writeStream << *iUsername;
			// iPassword
			writeStream.WriteInt32L(iPassword->Des().MaxLength());
			writeStream << *iPassword;

			// Just to ensure that any buffered data is written to the stream
			writeStream.CommitL();
		}	

		// Free resource handlers
		CleanupStack::PopAndDestroy(&writeStream);
		CleanupStack::PopAndDestroy(&fsSession);		
	}	

	__LOGSTR_TOFILE("CSupLoginServiceProvider::WriteDataToFileL() ends");

	return retValue;
}


TBool CSupLoginServiceProvider::ReadDataFromFileL()
{
	__LOGSTR_TOFILE("CSupLoginServiceProvider::ReadDataFromFileL() begins");

	// If current operation should be cancelled
	if (iCancelStatus)
		return EFalse;

	TBool retValue = ETrue;

	RFs fsSession;
	RFileReadStream readStream; // Read stream from file

	// Install read file session
	User::LeaveIfError(fsSession.Connect());
	CleanupClosePushL(fsSession);

	TInt err = readStream.Open(fsSession, iSettingsFile, EFileStream | EFileRead | EFileShareExclusive);
	CleanupClosePushL(readStream);

	// If file does not exist - return EFalse
	if (err != KErrNone)
	{
		retValue = EFalse;

		__LOGSTR_TOFILE("CSupLoginServiceProvider::ReadDataFromFileL() failed to open");
	}

	if (retValue)
	{
		TInt valueMaxLen = 0;
		HBufC8* tempValue = NULL;	

		// iMemberID
		valueMaxLen = readStream.ReadInt32L();

		__LOGSTR_TOFILE("CSupLoginServiceProvider::ReadDataFromFileL() reading member id");

		tempValue = HBufC8::NewL(readStream, valueMaxLen);
		if (tempValue)
		{
			if (iMemberID)
			{
				delete iMemberID;
				iMemberID = NULL;
			}

			iMemberID = tempValue->Des().AllocL();

			delete tempValue;
			tempValue = NULL;
		}

		// iUsername
		valueMaxLen = readStream.ReadInt32L();

		__LOGSTR_TOFILE("CSupLoginServiceProvider::ReadDataFromFileL() reading username");

		tempValue = HBufC8::NewL(readStream, valueMaxLen);
		if (tempValue)
		{
			if (iUsername)
			{
				delete iUsername;
				iUsername = NULL;
			}

			iUsername = tempValue->Des().AllocL();

			delete tempValue;
			tempValue = NULL;
		}

		// iPassword
		valueMaxLen = readStream.ReadInt32L();

		__LOGSTR_TOFILE("CSupLoginServiceProvider::ReadDataFromFileL() reading password");

		tempValue = HBufC8::NewL(readStream, valueMaxLen);
		if (tempValue)
		{
			if (iPassword)
			{
				delete iPassword;
				iPassword = NULL;
			}

			iPassword = tempValue->Des().AllocL();

			delete tempValue;
			tempValue = NULL;
		}
	}

	// Free resource handlers
	CleanupStack::PopAndDestroy(&readStream);
	CleanupStack::PopAndDestroy(&fsSession);	

	__LOGSTR_TOFILE("CSupLoginServiceProvider::ReadDataFromFileL() ends");

	return retValue;
}

TInt CSupLoginServiceProvider::RequestMemberID(const TDesC8 &aRequestUsername, const TDesC8 &aRequestPassword)
{
	__LOGSTR_TOFILE("CSupLoginServiceProvider::RequestMemberID() begins");

	// If current operation should be cancelled
	if (iCancelStatus)
		return KErrCancel;

	// Create empty SOAP message to hold the controller application
	CSenSoapMessage* soapRequest = CSenSoapMessage::NewL();
	CleanupStack::PushL(soapRequest);

	// Set SOAP action HTTP header
	soapRequest->SetSoapActionL(KSoapActionMemberId);

	// Get handle to SOAP body element
	CSenElement& messageBody = soapRequest->BodyL();

	// Add new GetMemberID element into SOAP body
	CSenElement& memberidRequest =  messageBody.AddElementL(KServiceXmlns,
		_L8("GetMemberID"));

	// Add username child element into the SOAP body
	CSenElement& usernameString = memberidRequest.AddElementL(_L8("NickName"));
	usernameString.SetContentL( aRequestUsername );

	// Add password element to the request
	CSenElement& passwordString = memberidRequest.AddElementL(_L8("WebPassword"));
	passwordString.SetContentL( aRequestPassword );

	// Submit SOAP async request
	TInt retValue = iConnection->SendL(*soapRequest);

	CleanupStack::PopAndDestroy(); // soapRequest

	__LOGSTR_TOFILE("CSupLoginServiceProvider::RequestMemberID() ends");

	return retValue;
}

void CSupLoginServiceProvider::HandleMessageL(const TDesC8 &aResponce)
{
	__LOGSTR_TOFILE("CSupLoginServiceProvider::HandleMessageL() begins");

	if (aResponce.Length() > 0)
	{
		// Construct the parser object
		CParser* parser = CParser::NewL(KXmlMimeType, *this);
		CleanupStack::PushL(parser);

		parser->ParseBeginL();
		// Start parsing XML from a descriptor
		// Current class will receive events
		// generated by the parser
		parser->ParseL(aResponce);

		parser->ParseEndL();

		// Destroy the parser when done.
		CleanupStack::PopAndDestroy(parser);
	}
	else
	{
		// Current logging status
		iLogged = EFalse;
		iRequestObserver->HandleRequestCompletedL(KErrUnknown);
		return;
	}

	__LOGSTR_TOFILE("CSupLoginServiceProvider::HandleMessageL() ends");	
}

void CSupLoginServiceProvider::HandleErrorL(const TInt aErrorCode, const TDesC8 &aMessage)
{
	__LOGSTR_TOFILE2("CSupLoginServiceProvider::HandleErrorL() begins with aErrorCode == %d and aMessage == %S", aErrorCode, &aMessage);

	// Re-read credentials data from file
	ReadDataFromFileL();

	// Callback to the request observer
	iRequestObserver->HandleRequestCompletedL(aErrorCode);

	__LOGSTR_TOFILE("CSupLoginServiceProvider::HandleErrorL() ends");
}

void CSupLoginServiceProvider::SetStatus(const TInt aStatus)
{
	iConectionStatus = aStatus;
}

CSupLoginServiceProvider* CSupLoginServiceProvider::InstanceL()
{
	__LOGSTR_TOFILE("CSupLoginServiceProvider::InstanceL() begins");

	CSupLoginServiceProvider* controllerInstance = static_cast<CSupLoginServiceProvider*>
		( CCoeEnv::Static( KUidSupLoginController ) );

	if (!controllerInstance)
	{
		// Create instance
		controllerInstance = new (ELeave)CSupLoginServiceProvider;
		CleanupStack::PushL( controllerInstance );
		controllerInstance->ConstructL();
		CleanupStack::Pop();
	}

	__LOGSTR_TOFILE("CSupLoginServiceProvider::InstanceL() ends");

	return controllerInstance;
}

TInt CSupLoginServiceProvider::StartLoginL(MRequestObserver* aObserver, const TDesC8 &aUsername, const TDesC8 &aPassword)
{
	__LOGSTR_TOFILE("CSupLoginServiceProvider::StartLoginL() begins");

	// If current operation should be cancelled
	if (iCancelStatus)
		return KErrCancel;

	__ASSERT_ALWAYS(aObserver, Panic(ENoObserver));

	// Set observer of the login request
	iRequestObserver = aObserver;

	// Code should be added later
	if (iUsername)
	{
		delete iUsername;
		iUsername = NULL;
	}
	iUsername = aUsername.AllocL();

	if (iPassword)
	{
		delete iPassword;
		iPassword = NULL;
	}
	iPassword = aPassword.AllocL();

	__LOGSTR_TOFILE2("CSupLoginServiceProvider::StartLoginL() iUsername == %S and iPassword == %S", iUsername, iPassword);

	// Request for the member id
	TInt retValue = RequestMemberID(*iUsername, *iPassword);

	__LOGSTR_TOFILE("CSupLoginServiceProvider::StartLoginL() ends");

	return retValue;
}

void CSupLoginServiceProvider::CancelLogin()
{
	__LOGSTR_TOFILE("CSupLoginServiceProvider::CancelLogin() begins");

	// Set cancel status
	iCancelStatus = ETrue;

	if (iConnection)
	{
		iConnection->Cancel();
	}

	__LOGSTR_TOFILE("CSupLoginServiceProvider::CancelLogin() ends");
}

TInt CSupLoginServiceProvider::GetStatus()
{
	// Reset cancel status
	iCancelStatus = EFalse;

	return iConectionStatus;
}

TBool CSupLoginServiceProvider::IsLoggedIn()
{
	return iLogged;
}

// Getters realizations
HBufC8* CSupLoginServiceProvider::GetMemberID() const
{
	return iMemberID;
}

HBufC8* CSupLoginServiceProvider::GetUsername() const
{
	return iUsername;
}

HBufC8* CSupLoginServiceProvider::GetPassword() const
{
	return iPassword;
}

void CSupLoginServiceProvider::SetIAPL(TUint32 aIap)
{
	CSenServicePattern* pattern =  CSenServicePattern::NewLC(KServiceEndpointMemberId, KNullDesC8);
	pattern->SetConsumerIapIdL(aIap);
	pattern->SetFrameworkIdL(KDefaultBasicWebServicesFrameworkID);

	// Recreate connection instance with new IAP value
	if (iConnection)
	{
		delete iConnection;
		iConnection = NULL;
	}

	// Create connection to web service
	iConnection = CSenServiceConnection::NewL(*this, *pattern);

	CleanupStack::PopAndDestroy(pattern);
}

// Xml parsing stuff
void CSupLoginServiceProvider::OnStartDocumentL( const RDocumentParameters& /*aDocParam*/,
												TInt aErrorCode )
{
	if ( KErrNone == aErrorCode )
	{
		// Do something here when the parser at the start of the document.
	}
	else
	{
		// Do something if error happens.
		iLogged = EFalse;

		// Callback to the request observer
		iRequestObserver->HandleRequestCompletedL(aErrorCode);
	}
}

void CSupLoginServiceProvider::OnEndDocumentL( TInt aErrorCode )
{
	if ( KErrNone == aErrorCode )
	{
		// Do something here when the parser reaches the end of the document.
	}

	// Callback to the request observer
	iRequestObserver->HandleRequestCompletedL(aErrorCode);
}

void CSupLoginServiceProvider::OnStartElementL( const RTagInfo& aElement,
											   const RAttributeArray& /*aAttributes*/, TInt aErrorCode )
{
	if ( KErrNone == aErrorCode )
	{
		// Found start of an element, for example: "<tag>"
		// The name of the element is stored in aElement.LocalName().DesC().

		// Do something with the start of an element.
	}
	else
	{
		// Do something if error happens.
		// Display error messages here.
		iLogged = EFalse;

		// Callback to the request observer
		iRequestObserver->HandleRequestCompletedL(aErrorCode);
	}
}

void CSupLoginServiceProvider::OnEndElementL( const RTagInfo &aElement, TInt aErrorCode )
{
	if ( KErrNone == aErrorCode )
	{
		// Found the end of an element, for example: "</tag>"
		// The name of the element is stored in aElement.LocalName().DesC().

		// Do something with the end of an element.
	}
	else
	{
		// Do something if error happens.
		// Display error messages here.
		iLogged = EFalse;

		// Callback to the request observer
		iRequestObserver->HandleRequestCompletedL(aErrorCode);
	}
}

void CSupLoginServiceProvider::OnContentL( const TDesC8 &aBytes, TInt aErrorCode )
{
	if ( KErrNone == aErrorCode )
	{
		// aBytes stored the value of the parsed contents.
		iMemberID = aBytes.AllocL();

		// Save data to file
		WriteDataToFileL();

		// Current logging status
		iLogged = ETrue;
	}
	else
	{
		// Display error messages here.
		iLogged = EFalse;
	}
}

void CSupLoginServiceProvider::OnStartPrefixMappingL( const RString& /*aPrefix*/,
													 const RString& /*aUri*/, TInt aErrorCode )
{
}

void CSupLoginServiceProvider::OnEndPrefixMappingL( const RString& /*aPrefix*/,
												   TInt aErrorCode )
{
}

void CSupLoginServiceProvider::OnIgnorableWhiteSpaceL( const TDesC8& /*aBytes*/,
													  TInt aErrorCode )
{
}

void CSupLoginServiceProvider::OnSkippedEntityL( const RString& /*aName*/,
												TInt aErrorCode )
{
}

void CSupLoginServiceProvider::OnProcessingInstructionL( const TDesC8& /*aTarget*/,
														const TDesC8& /*aData*/, TInt aErrorCode )
{
}

void CSupLoginServiceProvider::OnError( TInt aErrorCode )
{
	// Do something if error happens.
}

TAny* CSupLoginServiceProvider::GetExtendedInterface( const TInt32 /*aUid*/ )
{
	return 0;
}