#pragma once
#include "IFilter.h"
#include "IImage.h"
#include "Object.h"

namespace VPP 
{
	class BoxBlurFilter : public IFilter
	{
	public:
		virtual void Filter(const shared_ptr<IImage> & image, const shared_ptr<const Object> & object, int tag) const;
	};
}
