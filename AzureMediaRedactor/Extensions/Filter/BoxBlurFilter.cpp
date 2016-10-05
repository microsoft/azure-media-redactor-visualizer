#include "Stdafx.h"
#include "BoxBlurFilter.h"
#include "GradientMask.h"

using namespace VPP;

static GradientMask Mask;

static void GenerateGradientMask(cv::Mat& mask, int leftMargin, int topMargin, int width, int height)
{
	int xOffset = (leftMargin << GradientMask::Level) / width;
	int yOffset = (topMargin << GradientMask::Level) / height;

	unsigned char * h = mask.data;

	for (int row = 0; row < mask.rows; row++)
	{
		unsigned char * c = h;

		for (int col = 0; col < mask.cols; col++)
		{
			int x = (col << GradientMask::Level) / width + xOffset;
			int y = (row << GradientMask::Level) / height + yOffset;

			*c++ = Mask.Get(y, x);
		}

		h += mask.step;
	}
}

static void AlphaCopy(const cv::Mat& src, cv::Mat dst, cv::Mat& alphamask)
{
	for (int i = 0; i < src.rows; i++)
	{
		for (int j = 0; j < src.cols; j++)
		{
			unsigned char alpha = alphamask.at<unsigned char>(i, j);
			unsigned char beta = 255 - alpha;

			const cv::Vec3b& srcPixel = src.at<cv::Vec3b>(i, j);
			cv::Vec3b& dstPixel = dst.at<cv::Vec3b>(i, j);

			dstPixel[0] = static_cast<unsigned char>((srcPixel[0] * alpha + dstPixel[0] * beta) >> 8);
			dstPixel[1] = static_cast<unsigned char>((srcPixel[1] * alpha + dstPixel[1] * beta) >> 8);
			dstPixel[2] = static_cast<unsigned char>((srcPixel[2] * alpha + dstPixel[2] * beta) >> 8);
		}
	}
}

void BoxBlurFilter::Filter(const shared_ptr<IImage> & image, const shared_ptr<const Object> & object, int) const
{
	int left = static_cast<int>(object->Left);
	int top = static_cast<int>(object->Top);
	int right = static_cast<int>(object->Left + object->Width);
	int bottom = static_cast<int>(object->Top + object->Height);

	int leftMargin = 0, rightMargin = 0, topMargin = 0, bottomMargin = 0;

	if (left < 0)
	{
		leftMargin = -left;
		left = 0;
	}
	if (top < 0)
	{
		topMargin = -top;
		top = 0;
	}
	if (right > image->GetWidth())
	{
		rightMargin = right - image->GetWidth();
		right = image->GetWidth();
	}
	if (bottom > image->GetHeight())
	{
		bottomMargin = bottom - image->GetHeight();
		bottom = image->GetHeight();
	}

	if (left >= right || top >= bottom)
	{
		return;
	}

	cv::Mat frame(image->GetHeight(), image->GetWidth(), CV_8UC3, image->GetRgbPixels(), image->GetStride());
	cv::Mat area(frame, cv::Rect(left, top, right - left, bottom - top));
	cv::Mat canvas(area.rows, area.cols, area.type());
	cv::Mat mask(area.rows, area.cols, CV_8UC1);

	GenerateGradientMask(mask, leftMargin, topMargin, static_cast<int>(object->Width), static_cast<int>(object->Height));

	int hRadius = ((area.cols - 1) >> 3) + 1;
	int vRadius = ((area.rows - 1) >> 3) + 1;

	cv::boxFilter(area, canvas, area.depth(), cv::Size(hRadius, vRadius));

	AlphaCopy(canvas, area, mask);
}