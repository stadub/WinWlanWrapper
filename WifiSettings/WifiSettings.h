// WifiSettings.h

#pragma once

#include <Windows.h>
#include "Helpers.h"
#include "Exceptions.h"
#include "WifiSettingsEnums.h"

using namespace System;
using namespace System::Collections::Generic;

namespace WifiSettings {


	public value struct WLanIntreface
	{

	private:
		initonly  WlanInterfaceState state;
		initonly  Guid wlanGuid;
		initonly  String ^ name;

	public:
		WLanIntreface(WlanInterfaceState state, Guid wlanGuid,String ^ name){
			this->state = state;
			this->wlanGuid = wlanGuid;
			this->name = name;

		}
		property WlanInterfaceState State
		{
			WlanInterfaceState get() { return state; }
		}
		property Guid WlanGuid
		{
			Guid get() { return wlanGuid; }
		}
		property String ^ Name
		{
			String^ get() { return name; }
		}
		//
		/*RO_PROP(WlanInterfaceState^, State);
		RO_PROP(Guid Guid);
		RO_PROP(String Name);*/
	};

	public ref class AvailableNetwork {
		publicProp(String ^,		ProfileName);
		publicProp(String ^,		Dot11Ssid);
		publicProp(BssType,			Dot11BssType);
		publicProp(Int32,			NumberOfBssids);
		publicProp(Boolean,			NetworkConnectable);		
		publicProp(Int32,			NotConnectableReason);
		publicProp(Int32,			NumberOfPhyTypes);
		//DOT11_PHY_TYPE         dot11PhyTypes[WLAN_MAX_PHY_TYPE_NUMBER];
		//Boolean                MorePhyTypes;
		publicProp(Int32,			SignalQuality);
		publicProp(Boolean,			SecurityEnabled);
		publicProp(AuthAlgorithm,	DefaultAuthAlgorithm);
		publicProp(CipherAlgorithm, DefaultCipherAlgorithm);
		publicProp(WlanNetworkFlags, Flags);
	public:
		static String ^ GetNotConnectableReasonDescription(Int32 reason);
		virtual String ^ ToString() override;
	};

	public ref class WlanNative:IDisposable
	{
		DWORD dwNegotiatedVersion = 0;
		HANDLE hClientHandle = NULL;
	public:
		void OpenHandle();
		void CloseHandle();
		List<WLanIntreface> ^ GetInterfaces();
		List<AvailableNetwork^> ^ WlanNative::GetNetworks(WLanIntreface ^ wlan);
		// this will do our disposition of the native memory
		~WlanNative(){ this->!WlanNative(); }
	protected:
		void ThrowIfHandleClosed();
		// an explicit Finalize() method—as a failsafe
		!WlanNative() { if (!hClientHandle)CloseHandle(); }
	};

	public ref class Utilis {
	public:
		static const GUID GetNativeGuidFromManaged(System::Guid guild);
	};


}
