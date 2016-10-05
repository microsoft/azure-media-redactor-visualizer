#include "stdafx.h"
#include "BlurFilterWrapper.h"
#include "BitmapImage.h"

using namespace System;
using namespace System::Collections::Generic;
using namespace AzureMediaRedactor::Models;
using namespace AzureMediaRedactor::Extensions::Filter;

BlurFilterWrapper::BlurFilterWrapper()
{
	_filter = new BoxBlurFilter();
}

BlurFilterWrapper::~BlurFilterWrapper()
{
	this->!BlurFilterWrapper();
}

BlurFilterWrapper::!BlurFilterWrapper()
{
	delete _filter;
}

void BlurFilterWrapper::Filter(IVideoFrame ^ frame)
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

void BlurFilterWrapper::SetProvider(IVideoFilterProvider ^ provider, int userData)
{
	_provider = provider;
	_userData = userData;
}
