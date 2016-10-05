#pragma once

namespace VPP
{
	class IImage
	{
	public:
		virtual const unsigned char * GetGrayPixels() const = 0;
		virtual unsigned char * GetRgbPixels() const = 0;
		virtual int GetWidth() const = 0;
		virtual int GetHeight() const = 0;
		virtual int GetStride() const = 0;
		virtual shared_ptr<const IImage> Clone(bool copy) const = 0;
		virtual ~IImage() {};
	};
}