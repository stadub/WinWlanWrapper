//#define RO_PROP  (Type,Name) private: Type^ m_(Name);public:property Type^ Name	{Type^ get(){return m_(Name);}void set(Type^ value){m_(Name) = value;}}
//
#define publicProp(_Type_,_Name_) \
private:\
	_Type_ m_##_Name_; \
public:\
	property _Type_ _Name_	{\
	_Type_ get(){ return m_##_Name_; }\
	void set(_Type_ value){ m_##_Name_ = value; }\
}
