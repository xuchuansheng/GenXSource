#pragma once

#include <webservicebase.h>

namespace WS_SnapUpService {
	class SnapUpServiceSoap;
}

//! Describes params for DeviceUploadPhoto method
struct WS_SUS_DeviceUploadPhoto_InitParams;

//! WebServiceN2FSnapUpService class 

/*!
	WebServiceN2FSnapUpService class provides wrapper for N2F SnapUpService web-service
	@ http://next2friends.com:90/SnapUpService.asmx
*/

class WebServiceN2FSnapUpService
	:public WebServiceBase
{
public:

	N2FCORE_API	WebServiceN2FSnapUpService();
	N2FCORE_API	virtual ~WebServiceN2FSnapUpService();

	//! Web-service initialization method

	/*!
		Wrapped web-service initialization method
		\param customEndPoint a custom end-point to use for web-service, if empty string is passed - default end-point is used
		\return bool as initialization result
	*/
	N2FCORE_API virtual bool Initialize(CString& customEndPoint);

	//! Upload photo to personal photo galery
	/*! 
		N2F PhotoOrganise service method: DeviceUploadPhoto
		\param nickname user nickname
		\param webPassword web password
		\param base64Photo base64 encoded image
		\param dateTime date-time stamp
		\return bool true - if succeeded, otherwise - false
	*/
	N2FCORE_API bool DeviceUploadPhoto(CString& nickname, CString& password, CString& photoFileName, SYSTEMTIME &dateTime);

protected:

	//! Initialize parameters for DeviceUploadPhoto method

	/*! 
		Initialize parameters for DeviceUploadPhoto method
		\param webMemberID web member ID
		\param webPassword web password
		\param base64Photo base64 encoded image
		\param dateTime date-time stamp
		\param pOut [out] pointer to WS_PO_DeviceUploadPhoto_InitParams struct, which will hold all needed params,
		this structure is allocated by client code
		\see WS_PO_DeviceUploadPhoto_InitParams
		\return bool true - if succeeded, otherwise - false	
	*/

	N2FCORE_API bool PrepareParams_DeviceUploadPhoto(CString& nickname, CString& password, 
		CString& photoFileName, SYSTEMTIME &dateTime, WS_SUS_DeviceUploadPhoto_InitParams* pOut);

private:

	WS_SnapUpService::SnapUpServiceSoap		*iSnapUpService;
};
