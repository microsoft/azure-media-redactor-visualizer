#include "Stdafx.h"
#include "BitmapImage.h"

using namespace VPP;

BitmapImage::BitmapImage(int width, int height, int stride) :
	mReferencedRgbPixels(nullptr),
	mStride(stride),
	mWidth(width),
	mHeight(height)
{
}

BitmapImage::BitmapImage(int width, int height, int stride, unsigned char * pixels, bool copy) :
	BitmapImage(width, height, stride)
{
	if (pixels != nullptr)
	{
		if (copy)
		{
			shared_ptr<unsigned char> buffer(new unsigned char[mStride * mHeight], default_delete<unsigned char[]>());
			memcpy_s(buffer.get(), mStride * mHeight, pixels, mStride * mHeight);
			mRgbPixels = buffer;
		}
		else
		{
			mReferencedRgbPixels = pixels;
		}
	}
}

const unsigned char * BitmapImage::GetGrayPixels() const
{
	if (!mGrayPixels)
	{
		LockGuard lock(mMutex);
		if (!mGrayPixels)
		{
			shared_ptr<unsigned char> buffer(new unsigned char[mWidth * mHeight], default_delete<unsigned char[]>());
			auto rgbPtr = GetRgbPixels();
			auto grayPtr = buffer.get();

			for (int i = 0; i < mHeight; i++)
			{
				auto sourcePtr = &rgbPtr[i * mStride];
				auto destPtr = &grayPtr[i * mWidth];
				for (int j = 0; j < mWidth; j++)
				{
					unsigned char b = sourcePtr[j * 3 + 0];
					unsigned char g = sourcePtr[j * 3 + 1];
					unsigned char r = sourcePtr[j * 3 + 2];
					destPtr[j] = static_cast<unsigned char>(round(0.2989 * r + 0.5870 * g + 0.1140 * b));
				}
			}

			mGrayPixels = buffer;
		}
	}

	return mGrayPixels.get();
}

unsigned char * BitmapImage::GetRgbPixels() const
{
	if (mReferencedRgbPixels != nullptr)
	{
		return mReferencedRgbPixels;
	}

	return mRgbPixels.get();
}

int BitmapImage::GetWidth() const
{
	return mWidth;
}

int BitmapImage::GetHeight() const
{
	return mHeight;
}

int BitmapImage::GetStride() const
{
	return mStride;
}

shared_ptr<const IImage> BitmapImage::Clone(bool copy) const
{
	shared_ptr<BitmapImage> image = make_shared<BitmapImage>(mWidth, mHeight, mStride);

	if (copy)
	{
		{
			shared_ptr<unsigned char> buffer(new unsigned char[mStride * mHeight], default_delete<unsigned char[]>());
			memcpy_s(buffer.get(), mStride * mHeight, mRgbPixels.get(), mStride * mHeight);
			image->mRgbPixels = buffer;
		}

		if (mGrayPixels)
		{
			shared_ptr<unsigned char> buffer(new unsigned char[mWidth * mHeight], default_delete<unsigned char[]>());
			memcpy_s(buffer.get(), mWidth * mHeight, mGrayPixels.get(), mWidth * mHeight);
			image->mGrayPixels = buffer;
		}
	}
	else
	{
		image->mRgbPixels = mRgbPixels;
		image->mGrayPixels = mGrayPixels;
	}

	return image;
}