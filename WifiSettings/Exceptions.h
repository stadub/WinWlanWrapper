#pragma once

using namespace System;

namespace WifiSettings {

	public ref class WlanNativeException :Exception{
	public:
		WlanNativeException(String ^message) :Exception(message){}
		WlanNativeException(String ^message, Exception ^innerException) : Exception(message, innerException){}
	};

	public ref class OpenHandleException : WlanNativeException{
	public:
		static OpenHandleException ^AllreadyOpened();
		OpenHandleException(String ^message) :WlanNativeException(message){}
		OpenHandleException(String ^message, Exception ^innerException) : WlanNativeException(message, innerException){}
	};



	OpenHandleException ^OpenHandleException::AllreadyOpened()
	{
		return gcnew OpenHandleException("Client Handle allready opened");
	}
	
}