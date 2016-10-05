#include "stdafx.h"
#include "DebugFilterWrapper.h"
#include "BitmapImage.h"

using namespace System;
using namespace System::Collections::Generic;
using namespace System::Drawing;
using namespace AzureMediaRedactor::Models;
using namespace AzureMediaRedactor::Extensions::Filter;

DebugFilterWrapper::DebugFilterWrapper(Color color)
{
	_filter = new DebugFilter(color.ToArgb());
}

DebugFilterWrapper::~DebugFilterWrapper()
{
	this->!DebugFilterWrapper();
}

DebugFilterWrapper::!DebugFilterWrapper()
{
	delete _filter;
}

void DebugFilterWrapper::Filter(IVideoFrame ^ frame)
{
	if (_provider == nullptr)
	{
		return;
	}

	shared_ptr<VPP::IImage> image = make_shared<VPP::BitmapImage>(
		frame->Image->Width,
		frame->Image->Height,
		frame->Image->Stride,
		reinterpret_cast<unsigned char *>(frame->Image->Data.ToPointer()),
		false);

	for each (Annotation ^ annotation in _provider->OnFiltering(frame->Timestamp, _userData))
	{
		_filter->Filter(image, make_shared<VPP::Object>(
			annotation->X,
			annotation->Y,
			annotation->Width,
			annotation->Height),
			annotation->Id);
	}
}

void DebugFilterWrapper::SetProvider(IVideoFilterProvider ^ provider, int userData)
{
	_provider = provider;
	_userData = userData;
}
