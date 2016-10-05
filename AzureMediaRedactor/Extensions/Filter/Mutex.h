#pragma once

#ifdef __cplusplus_cli
#include <vcclr.h>
struct Mutex
{
	gcroot<System::Object ^> Object;

	Mutex() : Object(gcnew System::Object())
	{}
};

class LockGuard
{
	gcroot<System::Object ^> mObject;
public:
	LockGuard(Mutex mutex) : mObject(mutex.Object)
	{
		System::Threading::Monitor::Enter(mObject);
	}

	~LockGuard()
	{
		System::Threading::Monitor::Exit(mObject);
	}
};
#else
#define Mutex mutex
#define LockGuard lock_guard<mutex>
#endif