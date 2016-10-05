#pragma once
#include "IImage.h"
#include "Object.h"

namespace VPP 
{
	class IFilter
	{
	public:
		virtual void Filter(const shared_ptr<IImage> & image, const shared_ptr<const Object> & objects, int tag) const = 0;
		virtual ~IFilter() {}
	};
}
