// This is the main DLL file.

#ifndef UNICODE
#define UNICODE
#endif

#include "stdafx.h"

#include "WifiSettings.h"
#include "Helpers.h"


#include <wlanapi.h>



#include <objbase.h>
#include <wtypes.h>

#pragma comment(lib, "wlanapi.lib")
#pragma comment(lib, "ole32.lib")

using namespace System::Runtime::InteropServices;
using namespace System::Diagnostics;
using namespace System::Text;


namespace WifiSettings {


	void WlanNative::OpenHandle()
	{
		if (hClientHandle != 0){
			throw OpenHandleException::AllreadyOpened();
		}
		pin_ptr<DWORD> pdwNegotiatedVersion = &dwNegotiatedVersion;
		pin_ptr<HANDLE> phClientHandle = &hClientHandle;
		auto hResult = WlanOpenHandle(2, NULL, pdwNegotiatedVersion, phClientHandle);

		if (hResult != ERROR_SUCCESS){
			Marshal::ThrowExceptionForHR(hResult);

		}
	}

	void WlanNative::CloseHandle(){
		if (hClientHandle == 0){
			Trace::TraceWarning("Client Handle doesn't opened");
		}
		pin_ptr<HANDLE> phClientHandle = &hClientHandle;
		auto hResult = WlanCloseHandle(phClientHandle, NULL);

		dwNegotiatedVersion = 0;
		hClientHandle = NULL;
		if (hResult != ERROR_SUCCESS){
			Marshal::ThrowExceptionForHR(hResult);
		}
	}

	List<WLanIntreface> ^ WlanNative::GetInterfaces()
	{
		ThrowIfHandleClosed();
		auto list = gcnew List <WLanIntreface>();
		PWLAN_INTERFACE_INFO_LIST ppInterfaceList;
		auto hResult=WlanEnumInterfaces(hClientHandle, NULL, &ppInterfaceList);
		if (hResult != ERROR_SUCCESS){
			Marshal::ThrowExceptionForHR(hResult);
		}
		int numberOfItems = ppInterfaceList->dwNumberOfItems;
		for (int i = 0; i < numberOfItems; i++)
		{
			auto current = ppInterfaceList->InterfaceInfo[i];
			auto guid = current.InterfaceGuid;

			auto data4 = gcnew array<Byte>(8);
			for (int i = 0; i < 8; i++) 
				data4[i] = guid.Data4[i];

			auto wlanGuid = Guid((Int32)guid.Data1, (Int16)guid.Data2, (Int16)guid.Data3, data4);
			auto description = gcnew String(current.strInterfaceDescription);

			list->Add(WLanIntreface(static_cast<WlanInterfaceState>(current.isState), wlanGuid, description));
		}
		WlanFreeMemory(ppInterfaceList);
		return list;
	}

	void WlanNative::ThrowIfHandleClosed()
	{
		if (hClientHandle == 0){
			throw gcnew WlanNativeException("Client Handle doesn't opened");
		}
	}

	String ^ AvailableNetwork::GetNotConnectableReasonDescription(Int32 reason){

		WCHAR buffer(10240);
		auto result = WlanReasonCodeToString(reason, 10240, &buffer, NULL);
		return gcnew String(&buffer);
	}

	String ^ AvailableNetwork::ToString()
	{
		auto stringBuilder = gcnew StringBuilder();
		stringBuilder->AppendFormat("[Name:'{0}' ProfileName:'{1}', ", Dot11Ssid, ProfileName);
		stringBuilder->AppendFormat("Connectable:{0}, ",NetworkConnectable.ToString());
		stringBuilder->AppendFormat("BssType:{0}, ", Dot11BssType);
		stringBuilder->AppendFormat("BssidsCount:{0}, ", NumberOfBssids);
		stringBuilder->AppendFormat("NumberOfPhyTypes:{0}, ",  NumberOfPhyTypes);
		stringBuilder->AppendFormat("SignalQuality:{0}% ]", SignalQuality);
		return stringBuilder->ToString();
	}

	List<AvailableNetwork^> ^ WlanNative::GetNetworks(WLanIntreface ^ wlan)
	{
		ThrowIfHandleClosed();
		auto interfaceGuid = Utilis::GetNativeGuidFromManaged(wlan->WlanGuid);
		PWLAN_AVAILABLE_NETWORK_LIST pNetworksList;
		auto hResult = WlanGetAvailableNetworkList(hClientHandle, &interfaceGuid, 2, NULL, &pNetworksList);
		if (hResult != ERROR_SUCCESS){
			Marshal::ThrowExceptionForHR(hResult);
		}
		auto list = gcnew List <AvailableNetwork^>();
		int numberOfItems = pNetworksList->dwNumberOfItems;
		for (int i = 0; i < numberOfItems; i++)
		{
			auto current = pNetworksList->Network[i];

			auto currentManaged = gcnew AvailableNetwork();

			currentManaged->ProfileName = gcnew String(current.strProfileName);

			auto ssid = current.dot11Ssid;
			auto Dot11Ssid = gcnew String(reinterpret_cast<const char*>(ssid.ucSSID), 0,(int)ssid.uSSIDLength);
			currentManaged->Dot11Ssid = Dot11Ssid;

			currentManaged->Dot11BssType = static_cast<BssType>(current.dot11BssType);
			currentManaged->NumberOfBssids = current.uNumberOfBssids;
			currentManaged->NetworkConnectable = current.bNetworkConnectable;
			currentManaged->NotConnectableReason = current.wlanNotConnectableReason;

			currentManaged->NumberOfPhyTypes = current.uNumberOfPhyTypes;
			currentManaged->SignalQuality = current.wlanSignalQuality;
			currentManaged->SecurityEnabled = current.bSecurityEnabled;
			currentManaged->DefaultAuthAlgorithm = static_cast<AuthAlgorithm>(current.dot11DefaultAuthAlgorithm);
			currentManaged->DefaultCipherAlgorithm = static_cast<CipherAlgorithm>(current.dot11DefaultCipherAlgorithm);

			currentManaged->Flags = static_cast<WlanNetworkFlags>(current.dwFlags);

			list->Add(currentManaged);
		}
		WlanFreeMemory(pNetworksList);
		return list;
	}


	const GUID Utilis::GetNativeGuidFromManaged(System::Guid guild)
	{
		GUID interfaceGuid;
		auto source_array = guild.ToByteArray();
		Marshal::Copy(source_array, 0, IntPtr(&interfaceGuid), 16);
		return interfaceGuid;
	}

	/*List<AvailableNetwork^> WlanNative::GetNetworks1(WLanIntreface ^ wlan);
	{
		ThrowIfHandleClosed();
		PWLAN_BSS_LIST pWlanBssList;
		auto interfaceGuid=GetNativeGuidFromManaged(wlan->WlanGuid);
		auto hResult=WlanGetNetworkBssList(hClientHandle, &interfaceGuid, NULL, dot11_BSS_type_any, NULL, NULL, &pWlanBssList);
		if (hResult != ERROR_SUCCESS){
			Marshal::ThrowExceptionForHR(hResult);
		}

		int numberOfItems = pWlanBssList->dwNumberOfItems;
		for (int i = 0; i < numberOfItems; i++)
		{
			auto entry = pWlanBssList->wlanBssEntries + i * sizeof(WLAN_BSS_ENTRY)+8;
			wlanBssEntries[i] = (WLAN_BSS_ENTRY)Marshal.
				PtrToStructure(pWlanBssEntry, typeof(WLAN_BSS_ENTRY));
		}
		for (int i = 0; i < numberOfItems; i++)
		{


			auto current = ppInterfaceList->InterfaceInfo[i];
			auto guid = current.InterfaceGuid;

			auto data4 = gcnew array<Byte>(8);
			for (int i = 0; i < 8; i++)
				data4[i] = guid.Data4[i];

			auto wlanGuid = Guid((Int32)guid.Data1, (Int16)guid.Data2, (Int16)guid.Data3, data4);
			auto description = gcnew String(current.strInterfaceDescription);

			list->Add(WLanIntreface((WlanInterfaceState)current.isState, wlanGuid, description));
		}
		WlanFreeMemory(ppInterfaceList);
	}*/
}