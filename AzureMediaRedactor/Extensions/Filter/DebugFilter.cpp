#include "Stdafx.h"
#include "DebugFilter.h"

using namespace VPP;

VPP::DebugFilter::DebugFilter(int argb)
{
	_r = (argb >> 16) & 0xff;
	_g = (argb >> 8) & 0xff;
	_b = argb & 0xff;
}

void DebugFilter::Filter(const shared_ptr<IImage> & image, const shared_ptr<const Object> & object, int tag) const
{
	int left = static_cast<int>(object->Left);
	int top = static_cast<int>(object->Top);
	int right = static_cast<int>(object->Left + object->Width);
	int bottom = static_cast<int>(object->Top + object->Height);

	left = max(0, left);
	top = max(0, top);
	right = min(image->GetWidth(), right);
	bottom = min(image->GetHeight(), bottom);

	if (left >= right || top >= bottom)
	{
		return;
	}

	cv::Mat frame(image->GetHeight(), image->GetWidth(), CV_8UC3, reinterpret_cast<unsigned char*>(image->GetRgbPixels()), image->GetStride());
	cv::Mat area(frame, cv::Rect(left, top, right - left, bottom - top));
	cv::rectangle(frame, cv::Point(left, top), cv::Point(right, bottom), CV_RGB(_r, _g, _b));

	char buffer[128] = { 0 };
	sprintf_s(buffer, "%04d", tag);
	cv::putText(frame, buffer, cv::Point(left + 1, top + 15), cv::FONT_HERSHEY_SIMPLEX, 0.5, CV_RGB(255, 0, 255));
}