#pragma once
#include "IImage.h"
#include "Mutex.h"

namespace VPP
{
	class BitmapImage : public IImage
	{
	public:
		BitmapImage(int width, int height, int stride);
		BitmapImage(int width, int height, int stride, unsigned char * pixels, bool copy);
		BitmapImage(const BitmapImage &) = delete;
		BitmapImage & operator=(const BitmapImage &) = delete;

		virtual const unsigned char * GetGrayPixels() const;
		virtual unsigned char * GetRgbPixels() const;
		virtual int GetWidth() const;
		virtual int GetHeight() const;
		virtual int GetStride() const;
		virtual shared_ptr<const IImage> Clone(bool copy) const;

	private:
		int mWidth;
		int mHeight;
		int mStride;

		unsigned char * mReferencedRgbPixels;
		shared_ptr<unsigned char> mRgbPixels;
		mutable shared_ptr<unsigned char> mGrayPixels;
		mutable Mutex mMutex;
	};
}