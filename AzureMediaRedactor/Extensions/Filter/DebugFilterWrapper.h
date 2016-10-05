#pragma once
#include "DebugFilter.h"

namespace AzureMediaRedactor
{
	namespace Extensions
	{
		namespace Filter
		{
			using namespace System;
			using namespace System::Collections::Generic;
			using namespace System::ComponentModel::Composition;
			using namespace System::Drawing;
			using namespace VPP;
			using namespace AzureMediaRedactor::Models;

			[Export("DEBUG", IVideoFilter::typeid)]
			public ref class DebugFilterWrapper : public IVideoFilter
			{
				DebugFilter * _filter;
				IVideoFilterProvider ^ _provider;
				int _userData;

			public:
				[ImportingConstructor]
				DebugFilterWrapper([Import("Color")] Color color);
				~DebugFilterWrapper();
				!DebugFilterWrapper();

				virtual void Filter(IVideoFrame ^ frame);
				virtual void SetProvider(IVideoFilterProvider ^ provider, int userData);
			};
		}
	}
}