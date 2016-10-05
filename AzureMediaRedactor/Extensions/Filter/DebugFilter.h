#pragma once
#include "IFilter.h"
#include "IImage.h"
#include "Object.h"

namespace VPP
{
	class DebugFilter : public IFilter
	{
		int _r, _g, _b;

	public:
		DebugFilter(int argb);

		virtual void Filter(const shared_ptr<IImage> & image, const shared_ptr<const Object> & object, int tag) const;
	};
}